// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Predicts inputs based on AdditionalIncludeDirectories for items related to C++ source files.
    /// </summary>
    public class AdditionalIncludeDirectoriesPredictor : IProjectPredictor
    {
        // See Microsoft.CppCommon.targets for items which use AdditionalIncludeDirectories.
        internal const string ClCompileItemName = "ClCompile";
        internal const string FxCompileItemName = "FxCompile";
        internal const string MidlItemName = "Midl";
        internal const string ResourceCompileItemName = "ResourceCompile";

        internal const string ExcludedFromBuildMetadata = "ExcludedFromBuild";

        internal const string AdditionalIncludeDirectoriesMetadata = "AdditionalIncludeDirectories";

        private static readonly char[] IncludePathsSeparator = { ';' };

        /// <inheritdoc />
        public void PredictInputsAndOutputs(ProjectInstance projectInstance, ProjectPredictionReporter reporter)
        {
            // This predictor only applies to vcxproj files
            if (!projectInstance.FullPath.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // AdditionalIncludeDirectories are commonly added via an ItemDefinitionGroup, so use a HashSet to dedupe the repeats.
            var reportedIncludes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            ReportInputsForItemType(reporter, projectInstance, ClCompileItemName, reportedIncludes);
            ReportInputsForItemType(reporter, projectInstance, FxCompileItemName, reportedIncludes);
            ReportInputsForItemType(reporter, projectInstance, MidlItemName, reportedIncludes);
            ReportInputsForItemType(reporter, projectInstance, ResourceCompileItemName, reportedIncludes);
        }

        private void ReportInputsForItemType(ProjectPredictionReporter reporter, ProjectInstance projectInstance, string itemType, HashSet<string> reportedIncludes)
        {
            ICollection<ProjectItemInstance> items = projectInstance.GetItems(itemType);
            if (items.Count == 0)
            {
                return;
            }

            foreach (ProjectItemInstance item in items)
            {
                if (item.GetMetadataValue(ExcludedFromBuildMetadata).Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string[] additionalIncludeDirectories = item.GetMetadataValue(AdditionalIncludeDirectoriesMetadata)
                    .Split(IncludePathsSeparator, StringSplitOptions.RemoveEmptyEntries);
                foreach (string directory in additionalIncludeDirectories)
                {
                    string trimmedDirectory = directory.Trim();
                    if (!string.IsNullOrEmpty(trimmedDirectory) && reportedIncludes.Add(trimmedDirectory))
                    {
                        reporter.ReportInputDirectory(trimmedDirectory);
                    }
                }
            }
        }
    }
}