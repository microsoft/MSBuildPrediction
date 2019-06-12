// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System.Collections.Generic;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Finds Compile items, typically but not necessarily always from csproj files, as inputs.
    /// </summary>
    public class CSharpCompileItems : IProjectPredictor
    {
        internal const string CompileItemName = "Compile";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            Project project,
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            foreach (ProjectItem item in project.GetItems(CompileItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);
            }
        }
    }
}
