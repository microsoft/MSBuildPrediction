// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Predicts inputs for the key file used for strong naming.
    /// </summary>
    public class ApplicationIconPredictor : IProjectPredictor
    {
        internal const string ApplicationIconPropertyName = "ApplicationIcon";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            Project project,
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
