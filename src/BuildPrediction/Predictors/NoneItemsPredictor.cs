// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System.IO;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Finds None items as inputs.
    /// </summary>
    public sealed class NoneItemsPredictor : IProjectPredictor
    {
        internal const string OutDirPropertyName = "OutDir";
        internal const string NoneItemName = "None";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            Project project,
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            string outDir = projectInstance.GetPropertyValue(OutDirPropertyName);

            foreach (ProjectItemInstance item in projectInstance.GetItems(NoneItemName))
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
