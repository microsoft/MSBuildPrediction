// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Build.Prediction
{
    /// <summary>
    /// Represents a prediction.
    /// </summary>
    public sealed class PredictedItem
    {
        private readonly HashSet<string> _predictedBy = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>Initializes a new instance of the <see cref="PredictedItem"/> class.</summary>
        /// <param name="path">
        /// Provides a rooted path to a predicted build input.
        /// </param>
        public PredictedItem(string path)
        {
            Path = path.ThrowIfNullOrEmpty(nameof(path));
        }

        // For unit testing
        internal PredictedItem(string path, params string[] predictedBys)
            : this(path)
        {
            foreach (string p in predictedBys)
            {
                AddPredictedBy(p);
            }
        }

        /// <summary>
        /// Gets a relative or rooted path to a predicted build item.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the class name of each contributor to this prediction, for debugging purposes.
        /// These values are set in internally by <see cref="ProjectPredictionExecutor"/>.
        /// </summary>
        public IReadOnlyCollection<string> PredictedBy => _predictedBy;

        /// <inheritdoc/>
        public override string ToString() => $"PredictedItem: {Path}; PredictedBy={string.Join(",", PredictedBy)}";

        internal void AddPredictedBy(string predictedBy) => _predictedBy.Add(predictedBy);
    }
}