// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System.Collections.Generic;
    using Microsoft.Build.Graph;

    /// <summary>
    /// Finds project filename and imports from transitive dependencies as inputs.
    /// </summary>
    public sealed class ProjectFileAndImportsGraphPredictor : IProjectGraphPredictor
    {
        /// <inheritdoc/>
        public void PredictInputsAndOutputs(ProjectGraphNode projectGraphNode, ProjectPredictionReporter predictionReporter)
        {
            var seenGraphNodes = new HashSet<ProjectGraphNode>(projectGraphNode.ProjectReferences);
            var graphNodesToProcess = new Queue<ProjectGraphNode>(projectGraphNode.ProjectReferences);
            while (graphNodesToProcess.Count > 0)
            {
                ProjectGraphNode currentNode = graphNodesToProcess.Dequeue();

                // Predict the project file itself and all its imports.
                predictionReporter.ReportInputFile(currentNode.ProjectInstance.FullPath);
                foreach (string import in currentNode.ProjectInstance.ImportPaths)
                {
                    predictionReporter.ReportInputFile(import);
                }

                // Recurse transitively
                foreach (ProjectGraphNode projectReference in currentNode.ProjectReferences)
                {
                    if (seenGraphNodes.Add(projectReference))
                    {
                        graphNodesToProcess.Enqueue(projectReference);
                    }
                }
            }
        }
    }
}
