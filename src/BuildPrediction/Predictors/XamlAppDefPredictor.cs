﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Finds XamlAppDef items as inputs, used for xaml compilation.
    /// </summary>
    public sealed class XamlAppDefPredictor : IProjectPredictor
    {
        internal const string OutDirPropertyName = "OutDir";
        internal const string XamlAppDefItemName = "XamlAppDef";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            string outDir = projectInstance.GetPropertyValue(OutDirPropertyName);

            foreach (ProjectItemInstance item in projectInstance.GetItems(XamlAppDefItemName))
            {
                predictionReporter.ReportInputFile(item.EvaluatedInclude);

                // The GetCopyToOutputDirectoryXamlAppDefs target mimics GetCopyToOutputDirectoryItems for XamlAppDef items.
                if (!string.IsNullOrEmpty(outDir) && item.ShouldCopyToOutputDirectory())
                {
                    string targetPath = item.GetTargetPath();
                    if (!string.IsNullOrEmpty(targetPath))
                    {
                        predictionReporter.ReportOutputFile(Path.Combine(outDir, targetPath));
                    }
                }
            }
        }
    }
}