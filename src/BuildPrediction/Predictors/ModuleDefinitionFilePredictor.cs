// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Predicts inputs based on ModuleDefinitionFile metadata for items related to C++ linking.
    /// </summary>
    public class ModuleDefinitionFilePredictor : IProjectPredictor
    {
        // See Microsoft.CppCommon.targets for items which use ModuleDefinitionFile.
        internal const string LinkItemName = "Link";
        internal const string LibItemName = "Lib";
        internal const string ImpLibItemName = "ImpLib";

        internal const string ModuleDefinitionFileMetadata = "ModuleDefinitionFile";

        /// <inheritdoc />
        public void PredictInputsAndOutputs(ProjectInstance projectInstance, ProjectPredictionReporter reporter)
        {
            // This predictor only applies to vcxproj files
            if (!projectInstance.FullPath.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // ModuleDefinitionFile are commonly added via an ItemDefinitionGroup, so use a HashSet to dedupe the repeats.
            var reportedInputs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            ReportInputsForItemType(reporter, projectInstance, LinkItemName, reportedInputs);
            ReportInputsForItemType(reporter, projectInstance, LibItemName, reportedInputs);
            ReportInputsForItemType(reporter, projectInstance, ImpLibItemName, reportedInputs);
        }

        private void ReportInputsForItemType(ProjectPredictionReporter reporter, ProjectInstance projectInstance, string itemType, HashSet<string> reportedInputs)
        {
            ICollection<ProjectItemInstance> items = projectInstance.GetItems(itemType);
            if (items.Count == 0)
            {
                return;
            }

            foreach (ProjectItemInstance item in items)
            {
                string moduleDefinitionFile = item.GetMetadataValue(ModuleDefinitionFileMetadata).Trim();
                if (!string.IsNullOrEmpty(moduleDefinitionFile) && reportedInputs.Add(moduleDefinitionFile))
                {
                    reporter.ReportInputDirectory(moduleDefinitionFile);
                }
            }
        }
    }
}
