// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using Microsoft.Build.Execution;

    /// <summary>
    /// Finds EditorConfigFiles items as inputs.
    /// </summary>
    public sealed class EditorConfigFilesItemsPredictor : IProjectPredictor
    {
        internal const string EditorConfigFilesItemName = "EditorConfigFiles";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            foreach (ProjectItemInstance item in projectInstance.GetItems(EditorConfigFilesItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);
            }
        }
    }
}
