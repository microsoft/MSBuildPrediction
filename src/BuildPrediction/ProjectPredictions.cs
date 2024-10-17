// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Build.Prediction
{
    /// <summary>
    /// Predictions of build inputs and outputs provided by implementations of
    /// <see cref="IProjectPredictor"/>.
    /// </summary>
    public sealed class ProjectPredictions
    {
        /// <summary>Initializes a new instance of the <see cref="ProjectPredictions"/> class.</summary>
        /// <param name="inputFiles">A collection of predicted input files.</param>
        /// <param name="inputDirectories">A collection of predicted input directories.</param>
        /// <param name="outputFiles">A collection of predicted output files.</param>
        /// <param name="outputDirectories">A collection of predicted output directories.</param>
        /// <param name="dependencies">A collection of predicted dependencies.</param>
        public ProjectPredictions(
            IReadOnlyCollection<PredictedItem> inputFiles,
            IReadOnlyCollection<PredictedItem> inputDirectories,
            IReadOnlyCollection<PredictedItem> outputFiles,
            IReadOnlyCollection<PredictedItem> outputDirectories,
            IReadOnlyCollection<PredictedItem> dependencies)
        {
            InputFiles = inputFiles ?? Array.Empty<PredictedItem>();
            InputDirectories = inputDirectories ?? Array.Empty<PredictedItem>();
            OutputFiles = outputFiles ?? Array.Empty<PredictedItem>();
            OutputDirectories = outputDirectories ?? Array.Empty<PredictedItem>();
            Dependencies = dependencies ?? Array.Empty<PredictedItem>();
        }

        /// <summary>Gets a collection of predicted input files.</summary>
        public IReadOnlyCollection<PredictedItem> InputFiles { get; }

        /// <summary>Gets a collection of predicted input directories.</summary>
        public IReadOnlyCollection<PredictedItem> InputDirectories { get; }

        /// <summary>Gets a collection of predicted output files.</summary>
        public IReadOnlyCollection<PredictedItem> OutputFiles { get; }

        /// <summary>Gets a collection of predicted output directories.</summary>
        public IReadOnlyCollection<PredictedItem> OutputDirectories { get; }

        /// <summary>Gets a collection of predicted dependencies.</summary>
        public IReadOnlyCollection<PredictedItem> Dependencies { get; }
    }
}