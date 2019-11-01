// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using Microsoft.Build.Execution;

    /// <summary>
    /// Finds TypeScriptCompile items as inputs.
    /// </summary>
    public sealed class TypeScriptCompileItemsPredictor : IProjectPredictor
    {
        internal const string TypeScriptCompileItemName = "TypeScriptCompile";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            foreach (ProjectItemInstance item in projectInstance.GetItems(TypeScriptCompileItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);
            }
        }
    }
}
