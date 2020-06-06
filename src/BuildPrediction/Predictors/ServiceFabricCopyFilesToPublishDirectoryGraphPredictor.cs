// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Graph;

    /// <summary>
    /// Predicts inputs for Service Fabric projects based on the behavior of the publish targets.
    /// </summary>
    public sealed class ServiceFabricCopyFilesToPublishDirectoryGraphPredictor : IProjectGraphPredictor
    {
        /// <inheritdoc/>
        public void PredictInputsAndOutputs(ProjectGraphNode projectGraphNode, ProjectPredictionReporter predictionReporter)
        {
            ProjectInstance projectInstance = projectGraphNode.ProjectInstance;

            // This predictor only applies to sfproj files
            if (!projectInstance.FullPath.EndsWith(".sfproj", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // A service fabric project publishes its dependencies with its own publish dir. However, we're not providing the publish dir
            // since that requires cracking open the service manifest to find the correct subdir. Instead we'll just predict the directory.
            foreach (ProjectGraphNode projectReference in projectGraphNode.ProjectReferences)
            {
                GetCopyToPublishDirectoryItemsGraphPredictor.PredictInputsAndOutputs(projectReference, publishDir: null, predictionReporter);
            }

            string publishDir = projectInstance.GetPropertyValue(GetCopyToPublishDirectoryItemsGraphPredictor.PublishDirPropertyName);
            if (!string.IsNullOrEmpty(publishDir))
            {
                predictionReporter.ReportOutputDirectory(publishDir);
            }
        }
    }
}
