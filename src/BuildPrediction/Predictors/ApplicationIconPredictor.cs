// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Predicts inputs for the key file used for strong naming.
    /// </summary>
    public sealed class ApplicationIconPredictor : IProjectPredictor
    {
        internal const string ApplicationIconPropertyName = "ApplicationIcon";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            var assemblyOriginatorKeyFile = projectInstance.GetPropertyValue(ApplicationIconPropertyName);
            if (!string.IsNullOrEmpty(assemblyOriginatorKeyFile))
            {
                predictionReporter.ReportInputFile(assemblyOriginatorKeyFile);
            }
        }
    }
}