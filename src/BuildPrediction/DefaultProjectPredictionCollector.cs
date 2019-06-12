// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// The default implementation which just aggregates all predictions into a <see cref="ProjectPredictions"/> object.
    /// </summary>
    internal sealed class DefaultProjectPredictionCollector : IProjectPredictionCollector
    {
        private readonly Dictionary<string, PredictedItem> _inputsFilesByPath = new Dictionary<string, PredictedItem>(PathComparer.Instance);
        private readonly Dictionary<string, PredictedItem> _inputsDirectoriesByPath = new Dictionary<string, PredictedItem>(PathComparer.Instance);
        private readonly Dictionary<string, PredictedItem> _outputFilesByPath = new Dictionary<string, PredictedItem>(PathComparer.Instance);
        private readonly Dictionary<string, PredictedItem> _outputDirectoriesByPath = new Dictionary<string, PredictedItem>(PathComparer.Instance);

        /// <summary>
        /// Gets an aggregation of all predictions.
        /// </summary>
        internal ProjectPredictions Predictions => new ProjectPredictions(
            _inputsFilesByPath.Values,
            _inputsDirectoriesByPath.Values,
            _outputFilesByPath.Values,
            _outputDirectoriesByPath.Values);

        public void AddInputFile(string path, string projectDirectory, string predictorName) => AddBuildItem(_inputsFilesByPath, path, projectDirectory, predictorName);

        public void AddInputDirectory(string path, string projectDirectory, string predictorName) => AddBuildItem(_inputsDirectoriesByPath, path, projectDirectory, predictorName);

        public void AddOutputFile(string path, string projectDirectory, string predictorName) => AddBuildItem(_outputFilesByPath, path, projectDirectory, predictorName);

        public void AddOutputDirectory(string path, string projectDirectory, string predictorName) => AddBuildItem(_outputDirectoriesByPath, path, projectDirectory, predictorName);

        private static void AddBuildItem(Dictionary<string, PredictedItem> items, string path, string projectDirectory, string predictorName)
        {
            // Make the path absolute if needed.
            if (!Path.IsPathRooted(path))
            {
                path = Path.GetFullPath(Path.Combine(projectDirectory, path));
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

            // Add the predictor
            lock (items)
            {
                item.AddPredictedBy(predictorName);
            }
        }
    }
}
