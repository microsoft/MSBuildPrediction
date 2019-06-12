﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Predictions of build inputs and outputs provided by implementations of
    /// <see cref="IProjectPredictor"/>.
    /// </summary>
    public sealed class ProjectPredictions
    {
        /// <summary>Initializes a new instance of the <see cref="ProjectPredictions"/> class.</summary>
        /// <param name="buildInputs">A collection of predicted file or directory inputs.</param>
        /// <param name="buildOutputDirectories">A collection of predicted directory outputs.</param>
        public ProjectPredictions(
            IReadOnlyCollection<BuildInput> buildInputs,
            IReadOnlyCollection<BuildOutputDirectory> buildOutputDirectories)
        {
            BuildInputs = buildInputs ?? Array.Empty<BuildInput>();
            BuildOutputDirectories = buildOutputDirectories ?? Array.Empty<BuildOutputDirectory>();
        }

        /// <summary>Gets a collection of predicted file or directory inputs.</summary>
        public IReadOnlyCollection<BuildInput> BuildInputs { get; }

        /// <summary>Gets a collection of predicted directory outputs.</summary>
        public IReadOnlyCollection<BuildOutputDirectory> BuildOutputDirectories { get; }
    }
}
