// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using Microsoft.Build.Execution;

    /// <summary>
    /// Finds project filename and imports, as inputs.
    /// </summary>
    public sealed class ProjectFileAndImportsPredictor : IProjectPredictor
    {
        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            predictionReporter.ReportInputFile(projectInstance.FullPath);
            foreach (string importPath in projectInstance.ImportPaths)
            {
                predictionReporter.ReportInputFile(importPath);
            }
        }
    }
}
