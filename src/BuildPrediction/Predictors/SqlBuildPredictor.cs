// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Makes predictions for the SqlBuild target for sqlproj projects.
    /// </summary>
    public sealed class SqlBuildPredictor : IProjectPredictor
    {
        internal const string BuildItemName = "Build";
        internal const string PostDeployItemName = "PostDeploy";
        internal const string PreDeployItemName = "PreDeploy";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            // This predictor only applies to sqlproj files
            if (!projectInstance.FullPath.EndsWith(".sqlproj", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            foreach (ProjectItemInstance item in projectInstance.GetItems(BuildItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);
            }

            foreach (ProjectItemInstance item in projectInstance.GetItems(PostDeployItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);
            }

            foreach (ProjectItemInstance item in projectInstance.GetItems(PreDeployItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);
            }
        }
    }
}
