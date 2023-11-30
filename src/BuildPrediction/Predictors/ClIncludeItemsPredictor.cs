// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Finds ClInclude items, typically header files used during compilation.
    /// </summary>
    public sealed class ClIncludeItemsPredictor : IProjectPredictor
    {
        internal const string ClIncludeItemName = "ClInclude";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            foreach (ProjectItemInstance item in projectInstance.GetItems(ClIncludeItemName))
            {
                // ClInclude items aren't directly used by the build process, but they're usually read during compilation of a cpp file via #include
                // Because of this, they're merely a hint and might not actually be read, so check for file existence.
                string clIncludeFullPath = Path.Combine(projectInstance.Directory, item.EvaluatedInclude);
                if (File.Exists(clIncludeFullPath))
                {
                    predictionReporter.ReportInputFile(clIncludeFullPath);
                }
            }
        }
    }
}