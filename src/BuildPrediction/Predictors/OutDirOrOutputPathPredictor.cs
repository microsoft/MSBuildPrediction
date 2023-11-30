// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Scrapes the $(OutDir) or, if not found, $(OutputPath) as an output directory.
    /// </summary>
    public sealed class OutDirOrOutputPathPredictor : IProjectPredictor
    {
        internal const string OutDirMacro = "OutDir";
        internal const string OutputPathMacro = "OutputPath";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            // For an MSBuild project, the output goes to $(OutDir) by default. Usually $(OutDir)
            // equals $(OutputPath). Many targets expect OutputPath/OutDir to be defined and
            // MsBuild.exe reports an error if these macros are undefined.
            string outDir = projectInstance.GetPropertyValue(OutDirMacro);
            if (!string.IsNullOrWhiteSpace(outDir))
            {
                predictionReporter.ReportOutputDirectory(outDir);
            }
            else
            {
                // Some projects use custom code with $(OutputPath) set instead of following the common .targets pattern.
                // Fall back to $(OutputPath) first when $(OutDir) is not set.
                string outputPath = projectInstance.GetPropertyValue(OutputPathMacro);
                if (!string.IsNullOrWhiteSpace(outputPath))
                {
                    predictionReporter.ReportOutputDirectory(outputPath);
                }
            }
        }
    }
}