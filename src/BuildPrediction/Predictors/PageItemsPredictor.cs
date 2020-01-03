// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Finds Page items as inputs, used by WPF applications.
    /// </summary>
    public sealed class PageItemsPredictor : IProjectPredictor
    {
        internal const string PageItemName = "Page";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            Project project,
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            foreach (ProjectItemInstance item in projectInstance.GetItems(PageItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);
            }
        }
    }
}
