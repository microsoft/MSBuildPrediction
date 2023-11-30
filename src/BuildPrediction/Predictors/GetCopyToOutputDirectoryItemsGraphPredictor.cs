// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Build.Execution;
using Microsoft.Build.Graph;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Predicts files copied from dependencies in the GetCopyToOutputDirectoryItems.
    /// </summary>
    public sealed class GetCopyToOutputDirectoryItemsGraphPredictor : IProjectGraphPredictor
    {
        internal const string UseCommonOutputDirectoryPropertyName = "UseCommonOutputDirectory";
        internal const string OutDirPropertyName = "OutDir";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(ProjectGraphNode projectGraphNode, ProjectPredictionReporter predictionReporter)
        {
            ProjectInstance projectInstance = projectGraphNode.ProjectInstance;

            // The GetCopyToOutputDirectoryItems target gets called on all dependencies, unless UseCommonOutputDirectory is set to true.
            var useCommonOutputDirectory = projectInstance.GetPropertyValue(UseCommonOutputDirectoryPropertyName);
            if (!useCommonOutputDirectory.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                string outDir = projectInstance.GetPropertyValue(OutDirPropertyName);

                // Note that GetCopyToOutputDirectoryItems effectively only is able to go one project reference deep despite being recursive as
                // it uses @(_MSBuildProjectReferenceExistent) to recurse, which is not set in the recursive calls.
                // See: https://github.com/microsoft/msbuild/blob/master/src/Tasks/Microsoft.Common.CurrentVersion.targets
                foreach (ProjectGraphNode dependency in projectGraphNode.ProjectReferences)
                {
                    // Process each item type considered in GetCopyToOutputDirectoryItems. Yes, Compile is considered.
                    ReportCopyToOutputDirectoryItemsAsInputs(dependency.ProjectInstance, ContentItemsPredictor.ContentItemName, outDir, predictionReporter);
                    ReportCopyToOutputDirectoryItemsAsInputs(dependency.ProjectInstance, EmbeddedResourceItemsPredictor.EmbeddedResourceItemName, outDir, predictionReporter);
                    ReportCopyToOutputDirectoryItemsAsInputs(dependency.ProjectInstance, CompileItemsPredictor.CompileItemName, outDir, predictionReporter);
                    ReportCopyToOutputDirectoryItemsAsInputs(dependency.ProjectInstance, NoneItemsPredictor.NoneItemName, outDir, predictionReporter);

                    // Process each item type considered in GetCopyToOutputDirectoryXamlAppDefs
                    ReportCopyToOutputDirectoryItemsAsInputs(dependency.ProjectInstance, XamlAppDefPredictor.XamlAppDefItemName, outDir, predictionReporter);
                }
            }
        }

        private static void ReportCopyToOutputDirectoryItemsAsInputs(
            ProjectInstance projectInstance,
            string itemName,
            string outDir,
            ProjectPredictionReporter predictionReporter)
        {
            foreach (ProjectItemInstance item in projectInstance.GetItems(itemName))
            {
                if (item.ShouldCopyToOutputDirectory())
                {
                    // The item will be relative to the project instance passed in, not the current project instance, so make the path absolute.
                    predictionReporter.ReportInputFile(Path.Combine(projectInstance.Directory, item.EvaluatedInclude));

                    if (!string.IsNullOrEmpty(outDir))
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
}