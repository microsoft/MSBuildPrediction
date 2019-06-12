// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction
{
    using System;

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
    }
}
