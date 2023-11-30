// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Finds ClInclude items, typically header files used during compilation.
    /// </summary>
    public sealed class LinkItemsPredictor : IProjectPredictor
    {
        internal const string LinkItemName = "Link";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            // This predictor only applies to vcxproj files
            if (!projectInstance.FullPath.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            foreach (ProjectItemInstance item in projectInstance.GetItems(LinkItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);
            }
        }
    }
}