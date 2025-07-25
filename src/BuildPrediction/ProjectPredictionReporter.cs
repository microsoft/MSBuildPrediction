// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction
{
    /// <summary>
    /// A reporter that <see cref="IProjectPredictor"/> instances use to report predictions.
    /// </summary>
    /// <remarks>
    /// This struct is used internally by the prediction library and is not intended to be created externally.
    /// <see cref="IProjectPredictor"/> instances recieve this in their <see cref="IProjectPredictor.PredictInputsAndOutputs(ProjectInstance, ProjectPredictionReporter)"/>
    /// methods as a means to report predictions to the <see cref="IProjectPredictionCollector"/>. Generally the methods on this struct should mirror those on <see cref="IProjectPredictionCollector"/>.
    /// </remarks>
#pragma warning disable CA1815 // Override equals and operator equals on value types. Justification: This struct is never compared or equated.
    public ref struct ProjectPredictionReporter
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        private readonly IProjectPredictionCollector _predictionCollector;

        private readonly ProjectInstance _projectInstance;

        private readonly string _predictorName;

        /// <summary>Initializes a new instance of the <see cref="ProjectPredictionReporter"/> struct.</summary>
        /// <remarks>
        /// Internal to avoid public creation.
        /// </remarks>
        internal ProjectPredictionReporter(
            IProjectPredictionCollector predictionCollector,
            ProjectInstance projectInstance,
            string predictorName)
        {
            _predictionCollector = predictionCollector;
            _projectInstance = projectInstance;
            _predictorName = predictorName;
        }

        /// <summary>
        /// Report a prediction for an input file.
        /// </summary>
        /// <param name="path">The path of the file input.</param>
        public void ReportInputFile(string path) => _predictionCollector.AddInputFile(path, _projectInstance, _predictorName);

        /// <summary>
        /// Report a prediction for an input directory. Implicitly this means the directory's contents, but not its subdirectories, are used as inputs. This is equivalent to a "*" glob.
        /// </summary>
        /// <param name="path">The path of the file input.</param>
        public void ReportInputDirectory(string path) => _predictionCollector.AddInputDirectory(path, _projectInstance, _predictorName);

        /// <summary>
        /// Report a prediction for an output file.
        /// </summary>
        /// <param name="path">The path of the directory output.</param>
        public void ReportOutputFile(string path) => _predictionCollector.AddOutputFile(path, _projectInstance, _predictorName);

        /// <summary>
        /// Report a prediction for an output directory.
        /// </summary>
        /// <param name="path">The path of the directory output.</param>
        public void ReportOutputDirectory(string path) => _predictionCollector.AddOutputDirectory(path, _projectInstance, _predictorName);
    }
}