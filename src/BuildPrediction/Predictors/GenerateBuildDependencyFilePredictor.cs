// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors;

/// <summary>
/// Makes predictions based on the GenerateBuildDependencyFile target.
/// </summary>
public sealed class GenerateBuildDependencyFilePredictor : IProjectPredictor
{
    internal const string GenerateDependencyFilePropertyName = "GenerateDependencyFile";
    internal const string ProjectAssetsFilePropertyName = "ProjectAssetsFile";
    internal const string ProjectDepsFilePathPropertyName = "ProjectDepsFilePath";

    /// <inheritdoc/>
    public void PredictInputsAndOutputs(
        ProjectInstance projectInstance,
        ProjectPredictionReporter predictionReporter)
    {
        if (!projectInstance.GetPropertyValue(GenerateDependencyFilePropertyName).Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        predictionReporter.ReportInputFile(projectInstance.GetPropertyValue(ProjectAssetsFilePropertyName));
        predictionReporter.ReportOutputFile(projectInstance.GetPropertyValue(ProjectDepsFilePathPropertyName));
    }
}