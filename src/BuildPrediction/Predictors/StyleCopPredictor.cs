// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using System.Xml;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Predicts inputs and outputs when using the StyleCop.MSBuild nuget package (https://www.nuget.org/packages/StyleCop.MSBuild/).
    /// </summary>
    public sealed class StyleCopPredictor : IProjectPredictor
    {
        internal const string StyleCopTargetName = "StyleCop";

        internal const string StyleCopEnabledPropertyName = "StyleCopEnabled";

        internal const string StyleCopOverrideSettingsFilePropertyName = "StyleCopOverrideSettingsFile";

        internal const string StyleCopSettingsDefaultFileName = "Settings.StyleCop";

        internal const string StyleCopSettingsAlternateFileName = "Settings.SourceAnalysis";

        internal const string StyleCopSettingsLegacyFileName = "StyleCop.Settings";

        internal const string StyleCopAdditionalAddinPathsItemName = "StyleCopAdditionalAddinPaths";

        internal const string StyleCopOutputFilePropertyName = "StyleCopOutputFile";

        private readonly XmlReaderSettings _xmlReaderSettings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null, // Avoid external schema checks.
        };

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(ProjectInstance projectInstance, ProjectPredictionReporter predictionReporter)
        {
            // This predictor only applies when StyleCop exists and is enabled.
            if (!projectInstance.Targets.ContainsKey(StyleCopTargetName)
                || projectInstance.GetPropertyValue(StyleCopEnabledPropertyName).Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Find the StyleCop settings file as an input. If the override settings file is specified and valid,
            // it's used. Else fall back to finding the project settings file. Note that the validation or lack thereof
            // mimics what StyleCop actually does.
            string styleCopSettingsFile = TryGetOverrideSettingsFile(projectInstance, out string styleCopOverrideSettingsFile)
                ? styleCopOverrideSettingsFile
                : GetProjectSettingsFile(projectInstance.Directory);
            if (!string.IsNullOrEmpty(styleCopSettingsFile))
            {
                predictionReporter.ReportInputFile(styleCopSettingsFile);
            }

            // For Completeness we should consider Compile items as well since they're passed to StyleCop, but in practice another predictor will take care of that.

            // StyleCop addins as input directories
            foreach (ProjectItemInstance item in projectInstance.GetItems(StyleCopAdditionalAddinPathsItemName))
            {
                string addinPath = item.GetMetadataValue("FullPath");
                string expandedAddinPath = Environment.ExpandEnvironmentVariables(addinPath);
                if (Directory.Exists(expandedAddinPath))
                {
                    predictionReporter.ReportInputDirectory(expandedAddinPath);
                }
            }

            // StyleCop violations file as an output
            string styleCopOutputFile = projectInstance.GetPropertyValue(StyleCopOutputFilePropertyName);
            if (!string.IsNullOrEmpty(styleCopOutputFile))
            {
                predictionReporter.ReportOutputFile(styleCopOutputFile);
            }

            // When StyleCopCacheResults is true, a StyleCop.Cache file is written adjacent to the project.
            // Currently we want to avoid predicting this as predicting outputs to non-output directories generally leads to problems in the consumers of this library.
            // If the need for absolute completeness arises, it should be added and those consumers will just need to deal.
        }

        private static string GetProjectSettingsFile(string projectDir)
        {
            string possibleSettingsPath = Path.Combine(projectDir, StyleCopSettingsDefaultFileName);
            if (File.Exists(possibleSettingsPath))
            {
                return possibleSettingsPath;
            }

            possibleSettingsPath = Path.Combine(projectDir, StyleCopSettingsAlternateFileName);
            if (File.Exists(possibleSettingsPath))
            {
                return possibleSettingsPath;
            }

            possibleSettingsPath = Path.Combine(projectDir, StyleCopSettingsLegacyFileName);
            if (File.Exists(possibleSettingsPath))
            {
                return possibleSettingsPath;
            }

            return null;
        }

        private bool TryGetOverrideSettingsFile(ProjectInstance projectInstance, out string settingsFile)
        {
            settingsFile = projectInstance.GetPropertyValue(StyleCopOverrideSettingsFilePropertyName);

            if (string.IsNullOrEmpty(settingsFile))
            {
                return false;
            }

            try
            {
                settingsFile = Path.Combine(projectInstance.Directory, settingsFile);

                // Ignore the override settings file when it's missing
                if (!File.Exists(settingsFile))
                {
                    return false;
                }

                using (var reader = new StreamReader(settingsFile))
                using (XmlReader xmlReader = XmlReader.Create(reader, _xmlReaderSettings))
                {
                    var xmlDocument = new XmlDocument() { XmlResolver = null };
                    xmlDocument.Load(xmlReader);

                    // If the file is a valid XML file, that's good enough for StyleCop.
                    return true;
                }
            }
            catch (XmlException)
            {
                // Any exceptions while parsing result in ignoring the override settings file.
                return false;
            }
        }
    }
}