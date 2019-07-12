// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Finds project filename and imports, as inputs.
    /// </summary>
    public class ProjectFileAndImportsPredictor : IProjectPredictor
    {
        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            Project project,
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            predictionReporter.ReportInputFile(projectInstance.FullPath);
            foreach (ResolvedImport import in project.Imports)
            {
                predictionReporter.ReportInputFile(import.ImportedProject.FullPath);
            }
        }
    }
}
