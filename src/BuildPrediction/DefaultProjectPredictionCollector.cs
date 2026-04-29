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
        // Path.GetFullPath is pure string manipulation on .NET Core and calls the GetFullPathName API on
        // .NET Framework (which also does not perform I/O). The cache saves the allocation cost of
        // Path.Combine + Path.GetFullPath when the same relative path is reported by multiple predictors.
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
            // Make the path absolute if needed.
            if (!Path.IsPathRooted(path))
            {
                var cacheKey = (projectInstance.Directory, path);
                path = _absolutePathCache.GetOrAdd(cacheKey, static k => Path.GetFullPath(Path.Combine(k.Directory, k.Path)));
            }

            // Get the existing item, or add a new one if needed.
            PredictedItem item;
            lock (items)
            {
                if (!items.TryGetValue(path, out item))
                {
                    item = new PredictedItem(path);
                    items.Add(path, item);
                }
            }

            // Record the predictor, locking on the item to protect its HashSet.
            lock (item)
            {
                item.AddPredictedBy(predictorName);
            }
        }
    }
}