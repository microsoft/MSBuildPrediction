// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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
        private readonly Dictionary<string, PredictedItem> _dependenciesByPath = new Dictionary<string, PredictedItem>(PathComparer.Instance);

        public DefaultProjectPredictionCollector()
        {
            Predictions = new ProjectPredictions(
                _inputsFilesByPath.Values,
                _inputsDirectoriesByPath.Values,
                _outputFilesByPath.Values,
                _outputDirectoriesByPath.Values,
                _dependenciesByPath.Values);
        }

        /// <summary>
        /// Gets an aggregation of all predictions.
        /// </summary>
        internal ProjectPredictions Predictions { get; }

        public void AddInputFile(string path, ProjectInstance projectInstance, string predictorName) => AddPredictedItem(_inputsFilesByPath, path, projectInstance, predictorName);

        public void AddInputDirectory(string path, ProjectInstance projectInstance, string predictorName) => AddPredictedItem(_inputsDirectoriesByPath, path, projectInstance, predictorName);

        public void AddOutputFile(string path, ProjectInstance projectInstance, string predictorName) => AddPredictedItem(_outputFilesByPath, path, projectInstance, predictorName);

        public void AddOutputDirectory(string path, ProjectInstance projectInstance, string predictorName) => AddPredictedItem(_outputDirectoriesByPath, path, projectInstance, predictorName);

        public void AddDependency(string path, ProjectInstance projectInstance, string predictorName) => AddPredictedDependency(_dependenciesByPath, path, projectInstance, predictorName);

        private static void AddPredictedItem(Dictionary<string, PredictedItem> items, string path, ProjectInstance projectInstance, string predictorName)
        {
            // Make the path absolute if needed.
            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(Path.Combine(projectInstance.Directory, path));
            }

            AddPredictedDependency(items, path, projectInstance, predictorName);
        }

        private static void AddPredictedDependency(Dictionary<string, PredictedItem> items, string path, ProjectInstance projectInstance, string predictorName)
        {
            // Get the existing item, or add a new one if needed.
            lock (items)
            {
                if (!items.TryGetValue(path, out PredictedItem item))
                {
                    item = new PredictedItem(path, predictorName);
                    items.Add(path, item);
                }
                else
                {
                    item.AddPredictedBy(predictorName);
                }
            }
        }
    }
}