// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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
        internal const string MSBuildCopyContentTransitivelyPropertyName = "MSBuildCopyContentTransitively";
        internal const string HasRuntimeOutputPropertyName = "HasRuntimeOutput";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(ProjectGraphNode projectGraphNode, ProjectPredictionReporter predictionReporter)
        {
            string outDir = projectGraphNode.ProjectInstance.GetPropertyValue(OutDirPropertyName);
            HashSet<ProjectGraphNode> visitedNodes = new();
            PredictInputsAndOutputs(projectGraphNode, outDir, predictionReporter, visitedNodes);
        }

        private static void PredictInputsAndOutputs(
            ProjectGraphNode projectGraphNode,
            string outDir,
            ProjectPredictionReporter predictionReporter,
            HashSet<ProjectGraphNode> visitedNodes)
        {
            ProjectInstance projectInstance = projectGraphNode.ProjectInstance;

            // The GetCopyToOutputDirectoryItems target gets called on all dependencies, unless UseCommonOutputDirectory is set to true.
            var useCommonOutputDirectory = projectInstance.GetPropertyValue(UseCommonOutputDirectoryPropertyName);
            if (!useCommonOutputDirectory.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                bool copyContentTransitively = projectInstance.GetPropertyValue(MSBuildCopyContentTransitivelyPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase);
                Dictionary<string, ProjectReferenceContentInfo> projectReferenceContentByPath = GetProjectReferenceContentByPath(projectInstance);

                foreach (ProjectGraphNode dependency in projectGraphNode.ProjectReferences)
                {
                    ReportProjectReferenceOutput(
                        dependency.ProjectInstance,
                        outDir,
                        predictionReporter,
                        projectReferenceContentByPath);

                    if (!visitedNodes.Add(dependency))
                    {
                        // Avoid duplicate predictions
                        continue;
                    }

                    // If transitive, recurse
                    if (copyContentTransitively)
                    {
                        PredictInputsAndOutputs(dependency, outDir, predictionReporter, visitedNodes);
                    }

                    // Process each item type considered in GetCopyToOutputDirectoryItems. Yes, Compile is considered.
                    ReportCopyToOutputDirectoryItemsAsInputs(dependency.ProjectInstance, ContentItemsPredictor.ContentItemName, outDir, predictionReporter);
                    ReportCopyToOutputDirectoryItemsAsInputs(dependency.ProjectInstance, ContentItemsPredictor.ContentWithTargetPathItemName, outDir, predictionReporter);
                    ReportCopyToOutputDirectoryItemsAsInputs(dependency.ProjectInstance, EmbeddedResourceItemsPredictor.EmbeddedResourceItemName, outDir, predictionReporter);
                    ReportCopyToOutputDirectoryItemsAsInputs(dependency.ProjectInstance, CompileItemsPredictor.CompileItemName, outDir, predictionReporter);
                    ReportCopyToOutputDirectoryItemsAsInputs(dependency.ProjectInstance, NoneItemsPredictor.NoneItemName, outDir, predictionReporter);

                    // Process each item type considered in GetCopyToOutputDirectoryXamlAppDefs
                    ReportCopyToOutputDirectoryItemsAsInputs(dependency.ProjectInstance, XamlAppDefPredictor.XamlAppDefItemName, outDir, predictionReporter);

                    // Process items added by AddDepsJsonAndRuntimeConfigToCopyItemsForReferencingProjects
                    bool hasRuntimeOutput = dependency.ProjectInstance.GetPropertyValue(HasRuntimeOutputPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase);
                    if (hasRuntimeOutput)
                    {
                        if (dependency.ProjectInstance.GetPropertyValue(GenerateBuildDependencyFilePredictor.GenerateDependencyFilePropertyName).Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            string projectDepsFilePath = dependency.ProjectInstance.GetPropertyValue(GenerateBuildDependencyFilePredictor.ProjectDepsFilePathPropertyName);
                            if (!string.IsNullOrEmpty(projectDepsFilePath))
                            {
                                predictionReporter.ReportInputFile(projectDepsFilePath);
                                predictionReporter.ReportOutputFile(Path.Combine(outDir, Path.GetFileName(projectDepsFilePath)));
                            }
                        }

                        if (dependency.ProjectInstance.GetPropertyValue(GenerateRuntimeConfigurationFilesPredictor.GenerateRuntimeConfigurationFilesPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase))
                        {
                            string projectRuntimeConfigFilePath = dependency.ProjectInstance.GetPropertyValue(GenerateRuntimeConfigurationFilesPredictor.ProjectRuntimeConfigFilePathPropertyName);
                            if (!string.IsNullOrEmpty(projectRuntimeConfigFilePath))
                            {
                                predictionReporter.ReportInputFile(projectRuntimeConfigFilePath);
                                predictionReporter.ReportOutputFile(Path.Combine(outDir, Path.GetFileName(projectRuntimeConfigFilePath)));
                            }

                            string projectRuntimeConfigDevFilePath = dependency.ProjectInstance.GetPropertyValue(GenerateRuntimeConfigurationFilesPredictor.ProjectRuntimeConfigDevFilePathPropertyName);
                            if (!string.IsNullOrEmpty(projectRuntimeConfigDevFilePath))
                            {
                                predictionReporter.ReportInputFile(projectRuntimeConfigDevFilePath);
                                predictionReporter.ReportOutputFile(Path.Combine(outDir, Path.GetFileName(projectRuntimeConfigDevFilePath)));
                            }
                        }
                    }

                    // FakesV2 projects add Fakes assemblies as content which are transitively copied to referencing projects. See CopyFakesAssembliesToOutputDir target.
                    if (dependency.ProjectInstance.GetPropertyValue(FakesPredictor.FakesImportedPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase)
                        && dependency.ProjectInstance.GetPropertyValue(FakesPredictor.FakesUseV2GenerationPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase))
                    {
                        string fakesOutputPath = dependency.ProjectInstance.GetPropertyValue(FakesPredictor.FakesOutputPathPropertyName);
                        if (!string.IsNullOrWhiteSpace(fakesOutputPath))
                        {
                            // Make it absolute since it may be relative to the dependency project
                            fakesOutputPath = Path.Combine(dependency.ProjectInstance.Directory, fakesOutputPath);

                            foreach (ProjectItemInstance item in dependency.ProjectInstance.GetItems(FakesPredictor.FakesItemName))
                            {
                                string fakesAssemblyFileName = $"{Path.GetFileNameWithoutExtension(item.EvaluatedInclude)}.Fakes.dll";
                                predictionReporter.ReportInputFile(Path.Combine(fakesOutputPath, fakesAssemblyFileName));

                                if (!string.IsNullOrEmpty(outDir))
                                {
                                    predictionReporter.ReportOutputFile(Path.Combine(outDir, fakesAssemblyFileName));
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void ReportProjectReferenceOutput(
            ProjectInstance dependency,
            string outDir,
            ProjectPredictionReporter predictionReporter,
            Dictionary<string, ProjectReferenceContentInfo> projectReferenceContentByPath)
        {
            if (!projectReferenceContentByPath.TryGetValue(dependency.FullPath, out ProjectReferenceContentInfo contentInfo)
                || !contentInfo.HasCopiedContent)
            {
                return;
            }

            string targetPath = dependency.GetPropertyValue("TargetPath");
            if (string.IsNullOrEmpty(targetPath))
            {
                return;
            }

            string absoluteTargetPath = Path.IsPathRooted(targetPath)
                ? targetPath
                : Path.Combine(dependency.Directory, targetPath);
            predictionReporter.ReportInputFile(absoluteTargetPath);

            if (string.IsNullOrEmpty(outDir))
            {
                return;
            }

            foreach (string explicitTargetPath in contentInfo.ExplicitTargetPaths)
            {
                predictionReporter.ReportOutputFile(Path.Combine(outDir, explicitTargetPath));
            }

            if (contentInfo.UsesDefaultTargetPath)
            {
                string defaultTargetPath = Path.GetFileName(absoluteTargetPath);
                if (!contentInfo.ExplicitTargetPaths.Contains(defaultTargetPath))
                {
                    predictionReporter.ReportOutputFile(Path.Combine(outDir, defaultTargetPath));
                }
            }
        }

        private static Dictionary<string, ProjectReferenceContentInfo> GetProjectReferenceContentByPath(ProjectInstance projectInstance)
        {
            var projectReferenceContentByPath = new Dictionary<string, ProjectReferenceContentInfo>(PathComparer.Instance);
            foreach (ProjectItemInstance projectReferenceItem in projectInstance.GetItems("ProjectReference"))
            {
                string projectReferencePath = projectReferenceItem.EvaluatedInclude;
                if (!Path.IsPathRooted(projectReferencePath))
                {
                    projectReferencePath = Path.Combine(projectInstance.Directory, projectReferencePath);
                }

                projectReferencePath = Path.GetFullPath(projectReferencePath);
                if (!projectReferenceContentByPath.TryGetValue(projectReferencePath, out ProjectReferenceContentInfo contentInfo))
                {
                    contentInfo = new ProjectReferenceContentInfo();
                    projectReferenceContentByPath.Add(projectReferencePath, contentInfo);
                }

                contentInfo.Add(projectReferenceItem);
            }

            return projectReferenceContentByPath;
        }

        private static bool ShouldCopyProjectReferenceOutput(ProjectItemInstance item)
        {
            string copyToOutputDirectory = item.GetMetadataValue("CopyToOutputDirectory");
            return copyToOutputDirectory.Equals("Always", StringComparison.OrdinalIgnoreCase)
                || copyToOutputDirectory.Equals("PreserveNewest", StringComparison.OrdinalIgnoreCase)
                || copyToOutputDirectory.Equals("IfDifferent", StringComparison.OrdinalIgnoreCase);
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

        private sealed class ProjectReferenceContentInfo
        {
            public bool HasCopiedContent { get; private set; }

            public bool UsesDefaultTargetPath { get; private set; }

            public HashSet<string> ExplicitTargetPaths { get; } = new(PathComparer.Instance);

            public void Add(ProjectItemInstance item)
            {
                bool copiesContent =
                    !item.GetMetadataValue("BuildReference").Equals("false", StringComparison.OrdinalIgnoreCase)
                    && item.GetMetadataValue("OutputItemType").Equals("Content", StringComparison.OrdinalIgnoreCase)
                    && ShouldCopyProjectReferenceOutput(item);
                if (!copiesContent)
                {
                    return;
                }

                HasCopiedContent = true;
                string targetPath = item.GetMetadataValue("TargetPath");
                if (!string.IsNullOrEmpty(targetPath))
                {
                    ExplicitTargetPaths.Add(targetPath);
                    return;
                }

                string link = item.GetMetadataValue("Link");
                if (!string.IsNullOrEmpty(link))
                {
                    ExplicitTargetPaths.Add(link);
                    return;
                }

                UsesDefaultTargetPath = true;
            }
        }
    }
}