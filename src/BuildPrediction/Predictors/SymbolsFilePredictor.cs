// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Predicts the symbols file output.
    /// </summary>
    public sealed class SymbolsFilePredictor : IProjectPredictor
    {
        internal const string DebugSymbolsIntermediatePathItemName = "_DebugSymbolsIntermediatePath";

        internal const string DebugSymbolsOutputPathItemName = "_DebugSymbolsOutputPath";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            // The symbols file as output directly from the compiler.
            foreach (ProjectItemInstance item in projectInstance.GetItems(DebugSymbolsIntermediatePathItemName))
            {
                predictionReporter.ReportOutputFile(item.EvaluatedInclude);
            }

            // CopyFilesToOutputDirectory copies @(_DebugSymbolsIntermediatePath) items to the output directory using a different item group.
            foreach (ProjectItemInstance item in projectInstance.GetItems(DebugSymbolsOutputPathItemName))
            {
                predictionReporter.ReportOutputFile(item.EvaluatedInclude);
            }
        }
    }
}