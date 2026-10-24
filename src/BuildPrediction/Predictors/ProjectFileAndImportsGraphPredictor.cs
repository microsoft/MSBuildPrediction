// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Build.Graph;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Finds unique project filenames and imports from transitive dependencies as inputs.
    /// </summary>
    public sealed class ProjectFileAndImportsGraphPredictor : IProjectGraphPredictor
    {
        /// <inheritdoc/>
        public void PredictInputsAndOutputs(ProjectGraphNode projectGraphNode, ProjectPredictionReporter predictionReporter)
        {
            if (projectGraphNode.ProjectReferences.Count == 0)
            {
                return;
            }

            var seenGraphNodes = new HashSet<ProjectGraphNode>(projectGraphNode.ProjectReferences);
            var graphNodesToProcess = new Queue<ProjectGraphNode>(projectGraphNode.ProjectReferences);
            var seenInputPaths = new HashSet<string>(StringComparer.Ordinal);
            while (graphNodesToProcess.Count > 0)
            {
                ProjectGraphNode currentNode = graphNodesToProcess.Dequeue();

                // Predict the project file itself and all its imports.
                if (seenInputPaths.Add(currentNode.ProjectInstance.FullPath))
                {
                    predictionReporter.ReportInputFile(currentNode.ProjectInstance.FullPath);
                }

                foreach (string import in currentNode.ProjectInstance.ImportPaths)
                {
                    if (seenInputPaths.Add(import))
                    {
                        predictionReporter.ReportInputFile(import);
                    }
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