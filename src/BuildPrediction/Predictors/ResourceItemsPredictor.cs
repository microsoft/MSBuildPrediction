// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Finds Resource items as inputs, used by WPF applications.
    /// </summary>
    public sealed class ResourceItemsPredictor : IProjectPredictor
    {
        internal const string ResourceItemName = "Resource";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            Project project,
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            foreach (ProjectItemInstance item in projectInstance.GetItems(ResourceItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);
            }
        }
    }
}
