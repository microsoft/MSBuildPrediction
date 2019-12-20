// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System;
    using System.IO;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

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

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(Project project, ProjectInstance projectInstance, ProjectPredictionReporter predictionReporter)
        {
            // This predictor only applies when StyleCop exists and is enabled.
            if (!projectInstance.Targets.ContainsKey(StyleCopTargetName)
                || projectInstance.GetPropertyValue(StyleCopEnabledPropertyName).Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Find the StyleCop settings file as an input.
            string styleCopOverrideSettingsFile = projectInstance.GetPropertyValue(StyleCopOverrideSettingsFilePropertyName);
            string styleCopSettingsFile = !string.IsNullOrEmpty(styleCopOverrideSettingsFile)
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
    }
}
