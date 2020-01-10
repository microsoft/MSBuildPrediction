// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Executes a set of <see cref="IProjectPredictor"/> instances against
    /// a <see cref="ProjectInstance"/> instance, aggregating
    /// the results.
    /// </summary>
    public sealed class ProjectPredictionExecutor
    {
        private readonly PredictorAndName[] _predictors;
        private readonly ProjectPredictionOptions _options;

        /// <summary>Initializes a new instance of the <see cref="ProjectPredictionExecutor"/> class.</summary>
        /// <param name="predictors">The set of <see cref="IProjectPredictor"/> instances to use for prediction.</param>
        public ProjectPredictionExecutor(
            IEnumerable<IProjectPredictor> predictors)
            : this(predictors, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ProjectPredictionExecutor"/> class.</summary>
        /// <param name="predictors">The set of <see cref="IProjectPredictor"/> instances to use for prediction.</param>
        /// <param name="options">The options to use for prediction.</param>
        public ProjectPredictionExecutor(
            IEnumerable<IProjectPredictor> predictors,
            ProjectPredictionOptions options)
        {
            _predictors = predictors
                .ThrowIfNull(nameof(predictors))
                .Select(p => new PredictorAndName(p))
                .ToArray();  // Array = faster parallel performance.
            _options = options ?? new ProjectPredictionOptions();
        }

        /// <summary>
        /// Executes all predictors against the provided Project and aggregates the results
        /// into one set of predictions. All paths in the final predictions are fully qualified
        /// paths, not relative to the directory containing the Project, since inputs and
        /// outputs could lie outside of that directory.
        /// </summary>
        /// <param name="projectInstance">The project instance to execute predictors against.</param>
        /// <returns>An object describing all predicted inputs and outputs.</returns>
        public ProjectPredictions PredictInputsAndOutputs(ProjectInstance projectInstance)
        {
            var eventSink = new DefaultProjectPredictionCollector();
            PredictInputsAndOutputs(projectInstance, eventSink);
            return eventSink.Predictions;
        }

        /// <summary>
        /// Executes all predictors against the provided Project and reports all results
        /// to the provided event sink. Custom event sinks can be used to avoid translating
        /// predictions from <see cref="ProjectPredictions"/> to the caller's own object model,
        /// or for custom path normalization logic.
        /// </summary>
        /// <param name="projectInstance">The project instance to execute predictors against.</param>
        /// <param name="projectPredictionCollector">The prediction collector to use.</param>
        public void PredictInputsAndOutputs(ProjectInstance projectInstance, IProjectPredictionCollector projectPredictionCollector)
        {
            projectInstance.ThrowIfNull(nameof(projectInstance));
            projectPredictionCollector.ThrowIfNull(nameof(projectPredictionCollector));

            // Special-case single-threaded prediction to avoid the overhead of Parallel.For in favor of a simple loop.
            if (_options.MaxDegreeOfParallelism == 1)
            {
                for (var i = 0; i < _predictors.Length; i++)
                {
                    ExecuteSinglePredictor(projectInstance, _predictors[i], projectPredictionCollector);
                }
            }
            else
            {
                Parallel.For(
                    0,
                    _predictors.Length,
                    new ParallelOptions { MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism },
                    i => ExecuteSinglePredictor(projectInstance, _predictors[i], projectPredictionCollector));
            }
        }

        private static void ExecuteSinglePredictor(
            ProjectInstance projectInstance,
            PredictorAndName predictorAndName,
            IProjectPredictionCollector projectPredictionCollector)
        {
            var predictionReporter = new ProjectPredictionReporter(
                projectPredictionCollector,
                projectInstance.Directory,
                predictorAndName.TypeName);

            predictorAndName.Predictor.PredictInputsAndOutputs(
                projectInstance,
                predictionReporter);
        }

        private readonly struct PredictorAndName
        {
            public readonly IProjectPredictor Predictor;

            /// <summary>
            /// Cached type name - we expect predictor instances to be reused many times in
            /// an overall parsing session, avoid doing the reflection over and over in
            /// the prediction methods.
            /// </summary>
            public readonly string TypeName;

            public PredictorAndName(IProjectPredictor predictor)
            {
                Predictor = predictor;
                TypeName = predictor.GetType().Name;
            }
        }
    }
}
