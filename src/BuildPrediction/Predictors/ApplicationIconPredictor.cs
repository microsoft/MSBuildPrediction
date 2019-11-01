// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using Microsoft.Build.Execution;

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
