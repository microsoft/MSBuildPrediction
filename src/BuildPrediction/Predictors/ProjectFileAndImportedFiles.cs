// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System.Collections.Generic;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Finds project filename and imports, as inputs.
    /// </summary>
    public class ProjectFileAndImportedFiles : IProjectPredictor
    {
        /// <inheritdoc/>
        public bool TryPredictInputsAndOutputs(
            Project project,
            ProjectInstance projectInstance,
            out ProjectPredictions predictions)
        {
            var inputs = new List<BuildInput>()
            {
                new BuildInput(project.FullPath, false),
            };

            foreach (ResolvedImport import in project.Imports)
            {
                inputs.Add(new BuildInput(import.ImportedProject.FullPath, isDirectory: false));
            }

            predictions = new ProjectPredictions(inputs, null);

            return true;
        }
    }
}
