// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using Microsoft.Build.Execution;

    /// <summary>
    /// Predicts the documentation file output.
    /// </summary>
    public sealed class DocumentationFilePredictor : IProjectPredictor
    {
        internal const string DocFileItemItemName = "DocFileItem";

        internal const string FinalDocFileItemName = "FinalDocFile";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            // The documentation file as output directly from the compiler.
            // Note that this item generally has exactly one item which is the value of $(DocumentationFile),
            // but this item is the one that's actually used so we'll use ti here too since it's included statically.
            foreach (ProjectItemInstance item in projectInstance.GetItems(DocFileItemItemName))
            {
                predictionReporter.ReportOutputFile(item.EvaluatedInclude);
            }

            // CopyFilesToOutputDirectory copies @(DocFileItem) items to the output directory using a different item group.
            foreach (ProjectItemInstance item in projectInstance.GetItems(FinalDocFileItemName))
            {
                predictionReporter.ReportOutputFile(item.EvaluatedInclude);
            }
        }
    }
}
