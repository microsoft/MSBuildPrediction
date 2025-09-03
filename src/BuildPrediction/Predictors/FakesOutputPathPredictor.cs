// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Predicts the output directory for Microsoft Fakes assemblies based on the FakesOutputPath property.
    /// </summary>
    public sealed class FakesOutputPathPredictor : IProjectPredictor
    {
        internal const string FakesOutputPathPropertyName = "FakesOutputPath";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            string fakesOutputPath = projectInstance.GetPropertyValue(FakesOutputPathPropertyName);
            if (!string.IsNullOrWhiteSpace(fakesOutputPath))
            {
                predictionReporter.ReportOutputDirectory(fakesOutputPath);
            }
        }
    }
}