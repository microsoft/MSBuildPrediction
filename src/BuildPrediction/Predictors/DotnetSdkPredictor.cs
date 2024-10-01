// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors;

/// <summary>
/// Makes predictions based on using Microsoft.NET.Sdk.
/// </summary>
public sealed class DotnetSdkPredictor : IProjectPredictor
{
    internal const string UsingMicrosoftNETSdkPropertyName = "UsingMicrosoftNETSdk";

    private readonly ConcurrentDictionary<string, bool> _globalJsonExistenceCache = new(PathComparer.Instance);

    /// <inheritdoc/>
    public void PredictInputsAndOutputs(
        ProjectInstance projectInstance,
        ProjectPredictionReporter predictionReporter)
    {
        var usingMicrosoftNETSdk = projectInstance.GetPropertyValue(UsingMicrosoftNETSdkPropertyName);
        if (!usingMicrosoftNETSdk.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // Microsoft.NET.Sdk reads global.json.
        string currentProbeDirectory = projectInstance.Directory;
        while (currentProbeDirectory != null)
        {
            string globalJsonPath = Path.Combine(currentProbeDirectory, "global.json");
            bool globalJsonPathExists = _globalJsonExistenceCache.GetOrAdd(globalJsonPath, File.Exists);
            if (globalJsonPathExists)
            {
                predictionReporter.ReportInputFile(globalJsonPath);
                break;
            }

            currentProbeDirectory = Path.GetDirectoryName(currentProbeDirectory);
        }
    }
}