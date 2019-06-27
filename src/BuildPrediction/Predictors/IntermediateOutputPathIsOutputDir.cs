// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Scrapes the $(IntermediateOutputPath) if found.
    /// </summary>
    internal class IntermediateOutputPathIsOutputDir : IProjectPredictor
    {
        internal const string IntermediateOutputPathMacro = "IntermediateOutputPath";

        public void PredictInputsAndOutputs(
            Project project,
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            string intermediateOutputPath = projectInstance.GetPropertyValue(IntermediateOutputPathMacro);
            if (!string.IsNullOrWhiteSpace(intermediateOutputPath))
            {
                predictionReporter.ReportOutputDirectory(intermediateOutputPath);
            }
        }
    }
}
