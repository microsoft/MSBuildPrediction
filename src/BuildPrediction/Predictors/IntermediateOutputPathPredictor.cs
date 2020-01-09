﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using Microsoft.Build.Execution;

    /// <summary>
    /// Scrapes the $(IntermediateOutputPath) if found.
    /// </summary>
    public sealed class IntermediateOutputPathPredictor : IProjectPredictor
    {
        internal const string IntermediateOutputPathMacro = "IntermediateOutputPath";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
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
