// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Predicts the compiled ref assembly output.
    /// </summary>
    public sealed class RefAssemblyPredictor : IProjectPredictor
    {
        internal const string IntermediateRefAssemblyItemName = "IntermediateRefAssembly";

        internal const string TargetRefPathPropertyName = "TargetRefPath";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            Project project,
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            // The compiled ref assembly as output directly from the compiler.
            foreach (ProjectItemInstance item in projectInstance.GetItems(IntermediateRefAssemblyItemName))
            {
                predictionReporter.ReportOutputFile(item.EvaluatedInclude);
            }

            // CopyFilesToOutputDirectory copies @(IntermediateRefAssembly) items to the output directory.
            // It uses $(TargetRefPath) directly rather than an item.
            string targetRefPath = projectInstance.GetPropertyValue(TargetRefPathPropertyName);
            if (!string.IsNullOrWhiteSpace(targetRefPath))
            {
                predictionReporter.ReportOutputFile(targetRefPath);
            }
        }
    }
}
