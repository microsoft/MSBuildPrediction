// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction
{
    /// <summary>
    /// The default implementation which just aggregates all predictions into a <see cref="ProjectPredictions"/> object.
    /// </summary>
    internal sealed class DefaultProjectPredictionCollector : IProjectPredictionCollector
    {
        private readonly Dictionary<string, PredictedItem> _inputsFilesByPath = new Dictionary<string, PredictedItem>(PathComparer.Instance);
        private readonly Dictionary<string, PredictedItem> _inputsDirectoriesByPath = new Dictionary<string, PredictedItem>(PathComparer.Instance);
        private readonly Dictionary<string, PredictedItem> _outputFilesByPath = new Dictionary<string, PredictedItem>(PathComparer.Instance);
        private readonly Dictionary<string, PredictedItem> _outputDirectoriesByPath = new Dictionary<string, PredictedItem>(PathComparer.Instance);

        // Cache for Path.GetFullPath results to avoid repeated string allocations for the same relative paths.
        private readonly ConcurrentDictionary<(string Directory, string Path), string> _absolutePathCache = new ConcurrentDictionary<(string, string), string>();

        public DefaultProjectPredictionCollector()
        {
            Predictions = new ProjectPredictions(
                _inputsFilesByPath.Values,
                _inputsDirectoriesByPath.Values,
                _outputFilesByPath.Values,
                _outputDirectoriesByPath.Values);
        }

        /// <summary>
        /// Gets an aggregation of all predictions.
        /// </summary>
        internal ProjectPredictions Predictions { get; }

        public void AddInputFile(string path, ProjectInstance projectInstance, string predictorName) => AddPredictedItem(_inputsFilesByPath, path, projectInstance, predictorName);

        public void AddInputDirectory(string path, ProjectInstance projectInstance, string predictorName) => AddPredictedItem(_inputsDirectoriesByPath, path, projectInstance, predictorName);

        public void AddOutputFile(string path, ProjectInstance projectInstance, string predictorName) => AddPredictedItem(_outputFilesByPath, path, projectInstance, predictorName);

        public void AddOutputDirectory(string path, ProjectInstance projectInstance, string predictorName) => AddPredictedItem(_outputDirectoriesByPath, path, projectInstance, predictorName);

        private void AddPredictedItem(Dictionary<string, PredictedItem> items, string path, ProjectInstance projectInstance, string predictorName)
        {
            // Make the path absolute if needed, using a cache to avoid repeated string allocations.
            if (!Path.IsPathRooted(path))
            {
                var cacheKey = (projectInstance.Directory, path);
                path = _absolutePathCache.GetOrAdd(cacheKey, static k => Path.GetFullPath(Path.Combine(k.Directory, k.Path)));
            }

            // Get or add the item and record the predictor in a single lock acquisition.
            lock (items)
            {
                if (!items.TryGetValue(path, out PredictedItem item))
                {
                    item = new PredictedItem(path);
                    items.Add(path, item);
                }

                item.AddPredictedBy(predictorName);
            }
        }
    }
}