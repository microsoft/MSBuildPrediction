// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction
{
    /// <summary>
    /// Implementations of this interface are used during project prediction to
    /// collect all predictions from predictors.
    /// </summary>
    public interface IProjectPredictionCollector
    {
        /// <summary>
        /// Add a prediction for an input file.
        /// </summary>
        /// <param name="path">The path of the input file.</param>
        /// <param name="projectDirectory">The path to the directory of the project prediction originated from, used for determining where the path might be relative to.</param>
        /// <param name="predictorName">The name of the predictor which made the prediction, used for debugging purposes.</param>
        void AddInputFile(string path, string projectDirectory, string predictorName);

        /// <summary>
        /// Add a prediction for an input directory. Implicitly this means the directory's contents, but not its subdirectories, are used as inputs. This is equivalent to a "*" glob.
        /// </summary>
        /// <param name="path">The path of the input directory.</param>
        /// <param name="projectDirectory">The path to the directory of the project prediction originated from, used for determining where the path might be relative to.</param>
        /// <param name="predictorName">The name of the predictor which made the prediction, used for debugging purposes.</param>
        void AddInputDirectory(string path, string projectDirectory, string predictorName);

        /// <summary>
        /// Add a prediction for an output file.
        /// </summary>
        /// <param name="path">The path of the output file.</param>
        /// <param name="projectDirectory">The path to the directory of the project prediction originated from, used for determining where the path might be relative to.</param>
        /// <param name="predictorName">The name of the predictor which made the prediction, used for debugging purposes.</param>
        void AddOutputFile(string path, string projectDirectory, string predictorName);

        /// <summary>
        /// Add a prediction for an output directory.
        /// </summary>
        /// <param name="path">The path of the output directory.</param>
        /// <param name="projectDirectory">The path to the directory of the project prediction originated from, used for determining where the path might be relative to.</param>
        /// <param name="predictorName">The name of the predictor which made the prediction, used for debugging purposes.</param>
        void AddOutputDirectory(string path, string projectDirectory, string predictorName);
    }
}
