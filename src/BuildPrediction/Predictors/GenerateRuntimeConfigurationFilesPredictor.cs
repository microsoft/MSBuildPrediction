// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors;

/// <summary>
/// Makes predictions based on the GenerateRuntimeConfigurationFiles target.
/// </summary>
public sealed class GenerateRuntimeConfigurationFilesPredictor : IProjectPredictor
{
    internal const string GenerateRuntimeConfigurationFilesPropertyName = "GenerateRuntimeConfigurationFiles";
    internal const string UserRuntimeConfigPropertyName = "UserRuntimeConfig";
    internal const string ProjectRuntimeConfigFilePathPropertyName = "ProjectRuntimeConfigFilePath";
    internal const string ProjectRuntimeConfigDevFilePathPropertyName = "ProjectRuntimeConfigDevFilePath";

    /// <inheritdoc/>
    public void PredictInputsAndOutputs(
        ProjectInstance projectInstance,
        ProjectPredictionReporter predictionReporter)
    {
        if (!projectInstance.GetPropertyValue(GenerateRuntimeConfigurationFilesPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string userRuntimeConfig = projectInstance.GetPropertyValue(UserRuntimeConfigPropertyName);
        if (!string.IsNullOrEmpty(userRuntimeConfig))
        {
            string userRuntimeConfigFullPath = Path.Combine(projectInstance.Directory, userRuntimeConfig);
            if (File.Exists(userRuntimeConfigFullPath))
            {
                predictionReporter.ReportInputFile(userRuntimeConfigFullPath);
            }
        }

        string projectRuntimeConfigFilePath = projectInstance.GetPropertyValue(ProjectRuntimeConfigFilePathPropertyName);
        if (!string.IsNullOrEmpty(projectRuntimeConfigFilePath))
        {
            predictionReporter.ReportOutputFile(projectRuntimeConfigFilePath);
        }

        string projectRuntimeConfigDevFilePath = projectInstance.GetPropertyValue(ProjectRuntimeConfigDevFilePathPropertyName);
        if (!string.IsNullOrEmpty(projectRuntimeConfigDevFilePath))
        {
            predictionReporter.ReportOutputFile(projectRuntimeConfigDevFilePath);
        }
    }
}