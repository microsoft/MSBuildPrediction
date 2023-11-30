// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Finds ApplicationDefinition items as inputs, used by WPF applications.
    /// </summary>
    public sealed class ApplicationDefinitionItemsPredictor : IProjectPredictor
    {
        internal const string ApplicationDefinitionItemName = "ApplicationDefinition";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            foreach (ProjectItemInstance item in projectInstance.GetItems(ApplicationDefinitionItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);
            }
        }
    }
}