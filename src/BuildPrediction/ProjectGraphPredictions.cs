// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction
{
    using System.Collections.Generic;
    using Microsoft.Build.Graph;

    /// <summary>
    /// Predictions of build inputs and outputs per graph node provided by implementations of
    /// <see cref="IProjectGraphPredictor"/> and <see cref="IProjectPredictor"/>.
    /// </summary>
    public sealed class ProjectGraphPredictions
    {
        /// <summary>Initializes a new instance of the <see cref="ProjectGraphPredictions"/> class.</summary>
        /// <param name="predictionsPerNode">The predictions for each graph node.</param>
        public ProjectGraphPredictions(
            IReadOnlyDictionary<ProjectGraphNode, ProjectPredictions> predictionsPerNode)
        {
            PredictionsPerNode = predictionsPerNode ?? new Dictionary<ProjectGraphNode, ProjectPredictions>(0);
        }

        /// <summary>Gets the predictions for each graph node.</summary>
        public IReadOnlyDictionary<ProjectGraphNode, ProjectPredictions> PredictionsPerNode { get; }
    }
}
