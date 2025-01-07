// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Finds Content items as inputs.
    /// </summary>
    public sealed class ContentItemsPredictor : IProjectPredictor
    {
        internal const string OutDirPropertyName = "OutDir";
        internal const string ContentItemName = "Content";
        internal const string ContentWithTargetPathItemName = "ContentWithTargetPath";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            string outDir = projectInstance.GetPropertyValue(OutDirPropertyName);

            foreach (ProjectItemInstance item in projectInstance.GetItems(ContentItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);

                if (!string.IsNullOrEmpty(outDir) && item.ShouldCopyToOutputDirectory())
                {
                    string targetPath = item.GetTargetPath();
                    if (!string.IsNullOrEmpty(targetPath))
                    {
                        predictionReporter.ReportOutputFile(Path.Combine(outDir, targetPath));
                    }
                }
            }

            foreach (ProjectItemInstance item in projectInstance.GetItems(ContentWithTargetPathItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);

                if (!string.IsNullOrEmpty(outDir) && item.ShouldCopyToOutputDirectory())
                {
                    // Note: Using TargetPath directly instead of GetTargetPath since ContentWithTargetPath items don't have AssignTargetPath called on them.
                    string targetPath = item.GetMetadataValue("TargetPath");
                    if (!string.IsNullOrEmpty(targetPath))
                    {
                        predictionReporter.ReportOutputFile(Path.Combine(outDir, targetPath));
                    }
                }
            }
        }
    }
}