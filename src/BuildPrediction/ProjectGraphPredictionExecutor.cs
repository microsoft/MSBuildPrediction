// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Evaluation.Context;
using Microsoft.Build.Execution;
using Microsoft.Build.Graph;

namespace Microsoft.Build.Prediction
{
    /// <summary>
    /// Executes a set of <see cref="IProjectPredictor"/> instances and <see cref="IProjectGraphPredictor"/>
    /// instances against a project graph aggregating the results.
    /// </summary>
    public sealed class ProjectGraphPredictionExecutor
    {
        private readonly ValueAndTypeName<IProjectGraphPredictor>[] _projectGraphPredictors;
        private readonly ValueAndTypeName<IProjectPredictor>[] _projectPredictors;
        private readonly ProjectPredictionOptions _options;

        /// <summary>Initializes a new instance of the <see cref="ProjectGraphPredictionExecutor"/> class.</summary>
        /// <param name="projectGraphPredictors">The set of <see cref="IProjectGraphPredictor"/> instances to use for prediction.</param>
        /// <param name="projectPredictors">The set of <see cref="IProjectPredictor"/> instances to use for prediction.</param>
        public ProjectGraphPredictionExecutor(
            IEnumerable<IProjectGraphPredictor> projectGraphPredictors,
            IEnumerable<IProjectPredictor> projectPredictors)
            : this(projectGraphPredictors, projectPredictors, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ProjectGraphPredictionExecutor"/> class.</summary>
        /// <param name="projectGraphPredictors">The set of <see cref="IProjectGraphPredictor"/> instances to use for prediction.</param>
        /// <param name="projectPredictors">The set of <see cref="IProjectPredictor"/> instances to use for prediction.</param>
        /// <param name="options">The options to use for prediction.</param>
        public ProjectGraphPredictionExecutor(
            IEnumerable<IProjectGraphPredictor> projectGraphPredictors,
            IEnumerable<IProjectPredictor> projectPredictors,
            ProjectPredictionOptions options)
        {
            _projectGraphPredictors = projectGraphPredictors
                .ThrowIfNull(nameof(projectGraphPredictors))
                .Select(p => new ValueAndTypeName<IProjectGraphPredictor>(p))
                .ToArray(); // Array = faster parallel performance.
            _projectPredictors = projectPredictors
                .ThrowIfNull(nameof(projectPredictors))
                .Select(p => new ValueAndTypeName<IProjectPredictor>(p))
                .ToArray(); // Array = faster parallel performance.
            _options = options ?? new ProjectPredictionOptions();
        }

        /// <summary>
        /// Constructs a graph starting from the given graph entry points and execute all project and
        /// graph predictors against the resulting projects.
        /// </summary>
        /// <param name="projectGraph">Project graph to run predictions on.</param>
        /// <returns>An object describing all predicted inputs and outputs.</returns>
        public ProjectGraphPredictions PredictInputsAndOutputs(ProjectGraph projectGraph)
        {
            var projectGraphPredictionCollector = new DefaultProjectGraphPredictionCollector(projectGraph);
            PredictInputsAndOutputs(projectGraph, projectGraphPredictionCollector);
            return projectGraphPredictionCollector.GraphPredictions;
        }

        /// <summary>
        /// Constructs a graph starting from the given graph entry points and execute all project and
        /// graph predictors against the resulting projects.
        /// </summary>
        /// <param name="projectGraph">Project graph to run predictions on.</param>
        /// <param name="projectPredictionCollector">The prediction collector to use.</param>
        public void PredictInputsAndOutputs(
            ProjectGraph projectGraph,
            IProjectPredictionCollector projectPredictionCollector)
        {
            projectGraph.ThrowIfNull(nameof(projectGraph));
            projectPredictionCollector.ThrowIfNull(nameof(projectPredictionCollector));

            // Special-case single-threaded prediction to avoid the overhead of Parallel.ForEach in favor of a simple loop.
            if (_options.MaxDegreeOfParallelism == 1)
            {
                foreach (var projectNode in projectGraph.ProjectNodes)
                {
                    ExecuteAllPredictors(projectNode, _projectPredictors, _projectGraphPredictors, projectPredictionCollector);
                }
            }
            else
            {
                Parallel.ForEach(
                    projectGraph.ProjectNodes.ToArray(),
                    new ParallelOptions() { MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism },
                    projectNode => ExecuteAllPredictors(projectNode, _projectPredictors, _projectGraphPredictors, projectPredictionCollector));
            }
        }

        private static void ExecuteAllPredictors(
            ProjectGraphNode projectGraphNode,
            ValueAndTypeName<IProjectPredictor>[] projectPredictors,
            ValueAndTypeName<IProjectGraphPredictor>[] projectGraphPredictors,
            IProjectPredictionCollector projectPredictionCollector)
        {
            ProjectInstance projectInstance = projectGraphNode.ProjectInstance;

            // Run the project predictors. Use single-threaded prediction since we're already parallelizing on projects.
            ProjectPredictionExecutor.ExecuteProjectPredictors(projectInstance, projectPredictors, projectPredictionCollector, maxDegreeOfParallelism: 1);

            // Run the graph predictors
            for (var i = 0; i < projectGraphPredictors.Length; i++)
            {
                var predictionReporter = new ProjectPredictionReporter(
                    projectPredictionCollector,
                    projectInstance,
                    projectGraphPredictors[i].TypeName);

                projectGraphPredictors[i].Value.PredictInputsAndOutputs(
                    projectGraphNode,
                    predictionReporter);
            }
        }
    }
}