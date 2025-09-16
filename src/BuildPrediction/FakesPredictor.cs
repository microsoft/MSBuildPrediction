// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Predicts inputs and outputs related to Fakes.
    /// </summary>
    public sealed class FakesPredictor : IProjectPredictor
    {
        internal const string FakesImportedPropertyName = "FakesImported";

        internal const string FakesUseV2GenerationPropertyName = "FakesUseV2Generation";

        internal const string FakesOutputPathPropertyName = "FakesOutputPath";

        internal const string FakesItemName = "Fakes";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(ProjectInstance projectInstance, ProjectPredictionReporter predictionReporter)
        {
            if (!projectInstance.GetPropertyValue(FakesImportedPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            string fakesOutputPath = projectInstance.GetPropertyValue(FakesOutputPathPropertyName);
            foreach (ProjectItemInstance item in projectInstance.GetItems(FakesItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);

                if (!string.IsNullOrWhiteSpace(fakesOutputPath))
                {
                    string fakesAssembly = Path.Combine(fakesOutputPath, $"{Path.GetFileNameWithoutExtension(item.EvaluatedInclude)}.Fakes.dll");
                    predictionReporter.ReportOutputFile(fakesAssembly);
                }
            }
        }
    }
}