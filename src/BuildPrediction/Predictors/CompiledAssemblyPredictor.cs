// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System.IO;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Predicts the compiled assembly output.
    /// </summary>
    public sealed class CompiledAssemblyPredictor : IProjectPredictor
    {
        internal const string IntermediateAssemblyItemName = "IntermediateAssembly";

        internal const string OutDirPropertyName = "OutDir";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            string outDir = projectInstance.GetPropertyValue(OutDirPropertyName);

            // The compiled assembly as output directly from the compiler.
            foreach (ProjectItemInstance item in projectInstance.GetItems(IntermediateAssemblyItemName))
            {
                var intermediateAssembly = item.EvaluatedInclude;
                predictionReporter.ReportOutputFile(intermediateAssembly);

                // CopyFilesToOutputDirectory copies @(IntermediateAssembly) items to the output directory.
                if (!string.IsNullOrWhiteSpace(outDir))
                {
                    predictionReporter.ReportOutputFile(Path.Combine(outDir, Path.GetFileName(intermediateAssembly)));
                }
            }
        }
    }
}
