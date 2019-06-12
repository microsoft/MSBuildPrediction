// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction
{
    /// <summary>
    /// A reporter that <see cref="IProjectPredictor"/> instances use to report predictions.
    /// </summary>
    /// <remarks>
    /// This struct is used internally by the prediction library and is not intended to be created externally.
    /// <see cref="IProjectPredictor"/> instances recieve this in their <see cref="IProjectPredictor.TryPredictInputsAndOutputs(Evaluation.Project, Execution.ProjectInstance, out ProjectPredictions)"/>
    /// methods as a means to report predictions to the <see cref="IProjectPredictionCollector"/>. Generally the methods on this struct should mirror those on <see cref="IProjectPredictionCollector"/>.
    /// </remarks>
#pragma warning disable CA1815 // Override equals and operator equals on value types. Justification: This struct is never compared or equated.
    public ref struct ProjectPredictionReporter
#pragma warning restore CA1815 // Override equals and operator equals on value types
    {
        private readonly IProjectPredictionCollector _predictionCollector;

        private readonly string _projectDirectory;

        private readonly string _predictorName;

        /// <summary>Initializes a new instance of the <see cref="ProjectPredictionReporter"/> struct.</summary>
        /// <remarks>
        /// Internal to avoid public creation.
        /// </remarks>
        internal ProjectPredictionReporter(
            IProjectPredictionCollector predictionCollector,
            string projectDirectory,
            string predictorName)
        {
            _predictionCollector = predictionCollector;
            _projectDirectory = projectDirectory;
            _predictorName = predictorName;
        }

        /// <summary>
        /// Report a prediction for an input file.
        /// </summary>
        /// <param name="path">The path of the file input.</param>
        public void ReportInputFile(string path) => _predictionCollector.AddInputFile(path, _projectDirectory, _predictorName);

        /// <summary>
        /// Report a prediction for an input directory. Implicitly this means the directory's contents, but not its subdirectories, are used as inputs. This is equivalent to a "*" glob.
        /// </summary>
        /// <param name="path">The path of the file input.</param>
        public void ReportInputDirectory(string path) => _predictionCollector.AddInputDirectory(path, _projectDirectory, _predictorName);

        /// <summary>
        /// Report a prediction for an output file.
        /// </summary>
        /// <param name="path">The path of the directory output.</param>
        public void ReportOutputFile(string path) => _predictionCollector.AddOutputFile(path, _projectDirectory, _predictorName);

        /// <summary>
        /// Report a prediction for an output directory.
        /// </summary>
        /// <param name="path">The path of the directory output.</param>
        public void ReportOutputDirectory(string path) => _predictionCollector.AddOutputDirectory(path, _projectDirectory, _predictorName);
    }
}
