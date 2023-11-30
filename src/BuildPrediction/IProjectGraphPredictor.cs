// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Graph;

namespace Microsoft.Build.Prediction
{
    /// <summary>
    /// Implementations of this interface may be run in parallel against a single evaluated MSBuild Project
    /// file to predict, prior to execution of a build, file, directory/folder, and glob patterns for
    /// build inputs, and output directories written by the project.
    ///
    /// The resulting inputs, if any, are intended to feed into build caching algorithms to provide an
    /// initial hash for cache lookups. Inputs need not be 100% complete on a per-project basis, but
    /// more accuracy and completeness leads to better cache performance.The output directories provide
    /// guidance to build execution sandboxing to allow better static analysis of the effects of
    /// executing the Project.
    /// </summary>
    public interface IProjectGraphPredictor
    {
        /// <summary>
        /// Performs static prediction of build inputs and outputs for use by caching and sandboxing.
        /// This method may be executing on multiple threads simultaneously and should act as a
        /// pure method transforming its inputs into zero or more predictions in a thread-safe
        /// and idempotent fashion.
        /// </summary>
        /// <param name="projectGraphNode">A <see cref="ProjectGraphNode"/> to use for predictions.</param>
        /// <param name="predictionReporter">A reporter to report predictions to.</param>
        /// <remarks>
        /// Non-async since this should not require I/O, just CPU when examining the Project.
        /// </remarks>
        void PredictInputsAndOutputs(
            ProjectGraphNode projectGraphNode,
            ProjectPredictionReporter predictionReporter);
    }
}