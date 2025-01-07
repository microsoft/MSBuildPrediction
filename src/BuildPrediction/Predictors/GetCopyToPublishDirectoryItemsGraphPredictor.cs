// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Build.Execution;
using Microsoft.Build.Graph;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Predicts files copied by the GetCopyToPublishDirectoryItems target.
    /// </summary>
    public sealed class GetCopyToPublishDirectoryItemsGraphPredictor : IProjectGraphPredictor
    {
        internal const string PublishDirPropertyName = "PublishDir";

        internal const string SupportsDeployOnBuildPropertyName = "SupportsDeployOnBuild";
        internal const string DeployOnBuildPropertyName = "DeployOnBuild";

        internal const string CopyToPublishDirectoryMetadataName = "CopyToPublishDirectory";

        internal const string PublishTargetName = "Publish";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(ProjectGraphNode projectGraphNode, ProjectPredictionReporter predictionReporter)
        {
            if (IsPublishing(projectGraphNode.ProjectInstance))
            {
                string publishDir = projectGraphNode.ProjectInstance.GetPropertyValue(PublishDirPropertyName);
                PredictInputsAndOutputs(projectGraphNode, publishDir, predictionReporter);
            }
        }

        internal static void PredictInputsAndOutputs(
            ProjectGraphNode projectGraphNode,
            string publishDir,
            ProjectPredictionReporter predictionReporter)
        {
            ReportCopyToPublishDirectoryItems(projectGraphNode.ProjectInstance, publishDir, predictionReporter);

            // Note that GetCopyToPublishDirectoryItems effectively only is able to go one project reference deep despite appearing recursive for the same reasons as GetCopyToOutputDirectoryItems.
            // See: https://github.com/dotnet/sdk/blob/master/src/Tasks/Microsoft.NET.Build.Tasks/targets/Microsoft.NET.Publish.targets
            foreach (ProjectGraphNode dependency in projectGraphNode.ProjectReferences)
            {
                ReportCopyToPublishDirectoryItems(dependency.ProjectInstance, publishDir, predictionReporter);
            }
        }

        private static bool IsPublishing(ProjectInstance projectInstance)
        {
            // Check whether the project will publish as part of the build
            if (projectInstance.GetPropertyValue(SupportsDeployOnBuildPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase)
                && projectInstance.GetPropertyValue(DeployOnBuildPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Check whether the project specified the Publish target as a default target
            foreach (string defaultTarget in projectInstance.DefaultTargets)
            {
                if (defaultTarget.Equals(PublishTargetName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            // Publish does not happen by default.
            return false;
        }

        private static void ReportCopyToPublishDirectoryItems(
            ProjectInstance projectInstance,
            string publishDir,
            ProjectPredictionReporter predictionReporter)
        {
            // Process each item type considered in GetCopyToPublishDirectoryItems. Yes, Compile is considered.
            ReportCopyToPublishDirectoryItems(projectInstance, ContentItemsPredictor.ContentItemName, publishDir, predictionReporter);
            ReportCopyToPublishDirectoryItems(projectInstance, ContentItemsPredictor.ContentWithTargetPathItemName, publishDir, predictionReporter);
            ReportCopyToPublishDirectoryItems(projectInstance, EmbeddedResourceItemsPredictor.EmbeddedResourceItemName, publishDir, predictionReporter);
            ReportCopyToPublishDirectoryItems(projectInstance, CompileItemsPredictor.CompileItemName, publishDir, predictionReporter);
            ReportCopyToPublishDirectoryItems(projectInstance, NoneItemsPredictor.NoneItemName, publishDir, predictionReporter);

            // Process items added by AddDepsJsonAndRuntimeConfigToPublishItemsForReferencingProjects
            bool hasRuntimeOutput = projectInstance.GetPropertyValue(GetCopyToOutputDirectoryItemsGraphPredictor.HasRuntimeOutputPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase);
            if (hasRuntimeOutput)
            {
                if (projectInstance.GetPropertyValue(GenerateBuildDependencyFilePredictor.GenerateDependencyFilePropertyName).Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    if (GeneratePublishDependencyFilePredictor.ShouldUseBuildDependencyFile(projectInstance))
                    {
                        string projectDepsFilePath = projectInstance.GetPropertyValue(GenerateBuildDependencyFilePredictor.ProjectDepsFilePathPropertyName);
                        if (!string.IsNullOrEmpty(projectDepsFilePath))
                        {
                            predictionReporter.ReportInputFile(projectDepsFilePath);

                            if (!string.IsNullOrEmpty(publishDir))
                            {
                                predictionReporter.ReportOutputFile(Path.Combine(publishDir, Path.GetFileName(projectDepsFilePath)));
                            }
                        }
                    }
                    else
                    {
                        string publishDepsFilePath = GeneratePublishDependencyFilePredictor.GetEffectivePublishDepsFilePath(projectInstance);
                        if (!string.IsNullOrEmpty(publishDepsFilePath))
                        {
                            predictionReporter.ReportInputFile(publishDepsFilePath);

                            if (!string.IsNullOrEmpty(publishDir))
                            {
                                predictionReporter.ReportOutputFile(Path.Combine(publishDir, Path.GetFileName(publishDepsFilePath)));
                            }
                        }
                    }
                }

                if (projectInstance.GetPropertyValue(GenerateRuntimeConfigurationFilesPredictor.GenerateRuntimeConfigurationFilesPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    string projectRuntimeConfigFilePath = projectInstance.GetPropertyValue(GenerateRuntimeConfigurationFilesPredictor.ProjectRuntimeConfigFilePathPropertyName);
                    if (!string.IsNullOrEmpty(projectRuntimeConfigFilePath))
                    {
                        predictionReporter.ReportInputFile(projectRuntimeConfigFilePath);

                        if (!string.IsNullOrEmpty(publishDir))
                        {
                            predictionReporter.ReportOutputFile(Path.Combine(publishDir, Path.GetFileName(projectRuntimeConfigFilePath)));
                        }
                    }
                }
            }
        }

        private static void ReportCopyToPublishDirectoryItems(
            ProjectInstance projectInstance,
            string itemName,
            string publishDir,
            ProjectPredictionReporter predictionReporter)
        {
            foreach (ProjectItemInstance item in projectInstance.GetItems(itemName))
            {
                var copyToPublishDirectoryValue = item.GetMetadataValue(CopyToPublishDirectoryMetadataName);
                if (copyToPublishDirectoryValue.Equals("Always", StringComparison.OrdinalIgnoreCase)
                    || copyToPublishDirectoryValue.Equals("PreserveNewest", StringComparison.OrdinalIgnoreCase))
                {
                    // The item will be relative to the project instance passed in, not the current project instance, so make the path absolute.
                    predictionReporter.ReportInputFile(Path.Combine(projectInstance.Directory, item.EvaluatedInclude));

                    if (!string.IsNullOrEmpty(publishDir))
                    {
                        string targetPath = item.GetTargetPath();
                        if (!string.IsNullOrEmpty(targetPath))
                        {
                            predictionReporter.ReportOutputFile(Path.Combine(publishDir, targetPath));
                        }
                    }
                }
            }
        }
    }
}