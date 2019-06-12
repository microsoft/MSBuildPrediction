// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Executes a set of <see cref="IProjectStaticPredictor"/> instances against
    /// a <see cref="Microsoft.Build.Evaluation.Project"/> instance, aggregating
    /// the result.
    /// </summary>
    public sealed class ProjectStaticPredictionExecutor
    {
        private readonly PredictorAndName[] _predictors;
        private readonly PredictionOptions _options;

        /// <summary>Initializes a new instance of the <see cref="ProjectStaticPredictionExecutor"/> class.</summary>
        /// <param name="predictors">The set of <see cref="IProjectStaticPredictor"/> instances to use for prediction.</param>
        public ProjectStaticPredictionExecutor(
            IEnumerable<IProjectStaticPredictor> predictors)
            : this(predictors, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ProjectStaticPredictionExecutor"/> class.</summary>
        /// <param name="predictors">The set of <see cref="IProjectStaticPredictor"/> instances to use for prediction.</param>
        /// <param name="options">The options to use for prediction.</param>
        public ProjectStaticPredictionExecutor(
            IEnumerable<IProjectStaticPredictor> predictors,
            PredictionOptions options)
        {
            _predictors = predictors
                .ThrowIfNull(nameof(predictors))
                .Select(p => new PredictorAndName(p))
                .ToArray();  // Array = faster parallel performance.
            _options = options ?? new PredictionOptions();
        }

        /// <summary>
        /// Executes all predictors in parallel against the provided Project and aggregates
        /// the results into one set of predictions. All paths in the final predictions are
        /// fully qualified paths, not relative to the directory containing the Project or
        /// to the repository root directory, since inputs and outputs could lie outside of
        /// that directory.
        /// </summary>
        /// <param name="project">The project to execute predictors against.</param>
        /// <returns>An object describing all predicted inputs and outputs.</returns>
        public StaticPredictions PredictInputsAndOutputs(Project project)
        {
            project.ThrowIfNull(nameof(project));

            // Squash the Project with its full XML contents and tracking down to
            // a more memory-efficient format that can be used to evaluate conditions.
            // TODO: Static Graph needs to provide both, not just ProjectInstance, when we integrate.
            ProjectInstance projectInstance = project.CreateProjectInstance(ProjectInstanceSettings.ImmutableWithFastItemLookup);

            // Perf: Compared ConcurrentQueue vs. static array of results,
            // queue is 25% slower when all predictors return empty results,
            // ~25% faster as predictors return more and more false/null results,
            // with the breakeven point in the 10-15% null range.
            // ConcurrentBag 10X worse than either of the above, ConcurrentStack about the same.
            // Keeping queue implementation since many predictors return false.
            var results = new ConcurrentQueue<StaticPredictions>();

            // Special-case single-threaded prediction to avoid the overhead of Parallel.For in favor of a simple loop.
            if (_options.MaxDegreeOfParallelism == 1)
            {
                for (var i = 0; i < _predictors.Length; i++)
                {
                    ExecuteSinglePredictor(project, projectInstance, _predictors[i], results);
                }
            }
            else
            {
                Parallel.For(
                    0,
                    _predictors.Length,
                    new ParallelOptions { MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism },
                    i => ExecuteSinglePredictor(project, projectInstance, _predictors[i], results));
            }

            var inputsByPath = new Dictionary<string, BuildInput>(PathComparer.Instance);
            var outputDirectoriesByPath = new Dictionary<string, BuildOutputDirectory>(PathComparer.Instance);

            foreach (StaticPredictions predictions in results)
            {
                // TODO: Determine policy when dup inputs vary by IsDirectory.
                foreach (BuildInput input in predictions.BuildInputs)
                {
                    if (inputsByPath.TryGetValue(input.Path, out BuildInput existingInput))
                    {
                        existingInput.AddPredictedBy(input.PredictedBy);
                    }
                    else
                    {
                        inputsByPath[input.Path] = input;
                    }
                }

                foreach (BuildOutputDirectory outputDir in predictions.BuildOutputDirectories)
                {
                    if (outputDirectoriesByPath.TryGetValue(outputDir.Path, out BuildOutputDirectory existingOutputDir))
                    {
                        existingOutputDir.AddPredictedBy(outputDir.PredictedBy);
                    }
                    else
                    {
                        outputDirectoriesByPath[outputDir.Path] = outputDir;
                    }
                }
            }

            return new StaticPredictions(inputsByPath.Values, outputDirectoriesByPath.Values);
        }

        private static void ExecuteSinglePredictor(
            Project project,
            ProjectInstance projectInstance,
            PredictorAndName predictorAndName,
            ConcurrentQueue<StaticPredictions> results)
        {
            bool success = predictorAndName.Predictor.TryPredictInputsAndOutputs(
                project,
                projectInstance,
                out StaticPredictions result);

            // Tag each prediction with its source.
            // Check for null even on success as a bad predictor could do that.
            if (success && result != null)
            {
                foreach (BuildInput item in result.BuildInputs)
                {
                    item.AddPredictedBy(predictorAndName.TypeName);
                }

                foreach (BuildOutputDirectory item in result.BuildOutputDirectories)
                {
                    item.AddPredictedBy(predictorAndName.TypeName);
                }

                results.Enqueue(result);
            }
        }

        private readonly struct PredictorAndName
        {
            public readonly IProjectStaticPredictor Predictor;

            /// <summary>
            /// Cached type name - we expect predictor instances to be reused many times in
            /// an overall parsing session, avoid doing the reflection over and over in
            /// the prediction methods.
            /// </summary>
            public readonly string TypeName;

            public PredictorAndName(IProjectStaticPredictor predictor)
            {
                Predictor = predictor;
                TypeName = predictor.GetType().Name;
            }
        }
    }
}
