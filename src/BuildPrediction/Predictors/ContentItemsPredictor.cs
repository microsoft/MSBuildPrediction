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
        }
    }
}