// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Predicts inputs for the key file used for strong naming.
    /// </summary>
    public sealed class AssemblyOriginatorKeyFilePredictor : IProjectPredictor
    {
        internal const string SignAssemblyPropertyName = "SignAssembly";

        internal const string AssemblyOriginatorKeyFilePropertyName = "AssemblyOriginatorKeyFile";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            // The key file is only an input when signing is enabled.
            var signAssembly = projectInstance.GetPropertyValue(SignAssemblyPropertyName);
            if (!signAssembly.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var assemblyOriginatorKeyFile = projectInstance.GetPropertyValue(AssemblyOriginatorKeyFilePropertyName);
            if (!string.IsNullOrEmpty(assemblyOriginatorKeyFile))
            {
                predictionReporter.ReportInputFile(assemblyOriginatorKeyFile);
            }
        }
    }
}