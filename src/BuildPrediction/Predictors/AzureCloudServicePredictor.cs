﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Predicts inputs for Azure Cloud Service projects based on the service definition files (*.csdef) and service configuration files (*.cscfg).
    /// </summary>
    public sealed class AzureCloudServicePredictor : IProjectPredictor
    {
        internal const string ServiceConfigurationItemName = "ServiceConfiguration";

        internal const string ServiceDefinitionItemName = "ServiceDefinition";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            // This predictor only applies to ccproj files
            if (!projectInstance.FullPath.EndsWith(".ccproj", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            foreach (ProjectItemInstance item in projectInstance.GetItems(ServiceConfigurationItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);
            }

            foreach (ProjectItemInstance item in projectInstance.GetItems(ServiceDefinitionItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);
            }
        }
    }
}