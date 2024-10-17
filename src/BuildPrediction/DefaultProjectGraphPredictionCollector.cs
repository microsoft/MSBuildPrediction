// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Build.Execution;
using Microsoft.Build.Graph;

namespace Microsoft.Build.Prediction
{
    /// <summary>
    /// The default implementation which just aggregates all predictions into a <see cref="ProjectPredictions"/> object.
    /// </summary>
    internal sealed class DefaultProjectGraphPredictionCollector : IProjectPredictionCollector
    {
        private readonly Dictionary<ProjectInstance, DefaultProjectPredictionCollector> _collectorByProjectInstance;

        public DefaultProjectGraphPredictionCollector(ProjectGraph projectGraph)
        {
            IReadOnlyCollection<ProjectGraphNode> projectGraphNodes = projectGraph.ProjectNodes;

            var predictionsPerNode = new Dictionary<ProjectGraphNode, ProjectPredictions>(projectGraphNodes.Count);
            GraphPredictions = new ProjectGraphPredictions(predictionsPerNode);

            _collectorByProjectInstance = new Dictionary<ProjectInstance, DefaultProjectPredictionCollector>(projectGraphNodes.Count);
            foreach (ProjectGraphNode projectGraphNode in projectGraphNodes)
            {
                var collector = new DefaultProjectPredictionCollector();
                predictionsPerNode.Add(projectGraphNode, collector.Predictions);
                _collectorByProjectInstance.Add(projectGraphNode.ProjectInstance, collector);
            }
        }

        /// <summary>
        /// Gets an aggregation of all predictions.
        /// </summary>
        internal ProjectGraphPredictions GraphPredictions { get; }

        public void AddInputFile(string path, ProjectInstance projectInstance, string predictorName) => GetProjectCollector(projectInstance).AddInputFile(path, projectInstance, predictorName);

        public void AddInputDirectory(string path, ProjectInstance projectInstance, string predictorName) => GetProjectCollector(projectInstance).AddInputDirectory(path, projectInstance, predictorName);

        public void AddOutputFile(string path, ProjectInstance projectInstance, string predictorName) => GetProjectCollector(projectInstance).AddOutputFile(path, projectInstance, predictorName);

        public void AddOutputDirectory(string path, ProjectInstance projectInstance, string predictorName) => GetProjectCollector(projectInstance).AddOutputDirectory(path, projectInstance, predictorName);

        public void AddDependency(string path, ProjectInstance projectInstance, string predictorName) => GetProjectCollector(projectInstance).AddDependency(path, projectInstance, predictorName);

        private DefaultProjectPredictionCollector GetProjectCollector(ProjectInstance projectInstance)
        {
            if (!_collectorByProjectInstance.TryGetValue(projectInstance, out DefaultProjectPredictionCollector collector))
            {
                throw new InvalidOperationException("Prediction collected for ProjectInstance not in the ProjectGraph");
            }

            return collector;
        }
    }
}