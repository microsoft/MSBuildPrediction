// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Microsoft.Build.Prediction
{
    /// <summary>
    /// Represents various options used during project prediction to change behavior.
    /// </summary>
    public sealed class ProjectPredictionOptions
    {
        /// <summary>
        /// Gets or sets the max degree of parallelism to use for prediction execution. Defaults to <see cref="Environment.ProcessorCount"/>.
        /// </summary>
        /// <remarks>
        /// If the caller of <see cref="ProjectPredictionExecutor"/> is parallelizing across projects, it's recommended to set this to 1 to avoid over-scheduling.
        /// </remarks>
        public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;

        /// <summary>
        /// Gets or sets the max degree of parallelism to use for running predictors within a single project during graph prediction.
        /// Defaults to 1, meaning predictors for each project run sequentially.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When using <see cref="ProjectGraphPredictionExecutor"/>, projects are processed in parallel (controlled by <see cref="MaxDegreeOfParallelism"/>),
        /// and predictors within each project are processed with this level of parallelism. The total thread usage may be up to
        /// <see cref="MaxDegreeOfParallelism"/> × <see cref="MaxDegreeOfParallelismPerProject"/>, so care should be taken to avoid over-subscription.
        /// </para>
        /// <para>
        /// Increasing this value can help when individual projects are bottlenecks (e.g. projects with many items that take a long time to predict),
        /// but it assumes all predictors are thread-safe. Built-in predictors are thread-safe, but custom predictors should be verified.
        /// </para>
        /// </remarks>
        public int MaxDegreeOfParallelismPerProject { get; set; } = 1;
    }
}