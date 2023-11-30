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
    /// Predicts inputs for Azure Cloud Service projects for the PipelineTransformPhase target's behavior.
    /// </summary>
    public sealed class AzureCloudServicePipelineTransformPhaseGraphPredictor : IProjectGraphPredictor
    {
        internal const string ProjectReferenceItemName = "ProjectReference";

        internal const string RoleTypeMetadataName = "RoleType";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(ProjectGraphNode projectGraphNode, ProjectPredictionReporter predictionReporter)
        {
            ProjectInstance projectInstance = projectGraphNode.ProjectInstance;

            // This predictor only applies to ccproj files
            if (!projectInstance.FullPath.EndsWith(".ccproj", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Emulates the behavior of the PrepareRoleItems target, specifically for web roles.
            ICollection<ProjectItemInstance> projectReferences = projectInstance.GetItems(ProjectReferenceItemName);
            var webRoleProjects = new HashSet<string>(projectReferences.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var projectReferenceItem in projectReferences)
            {
                if (projectReferenceItem.GetMetadataValue(RoleTypeMetadataName).Equals("Web", StringComparison.OrdinalIgnoreCase))
                {
                    string webRoleProjectFullPath = Path.GetFullPath(Path.Combine(projectInstance.Directory, projectReferenceItem.EvaluatedInclude));
                    webRoleProjects.Add(webRoleProjectFullPath);
                }
            }

            // The PipelineTransformPhase target will be called on all web role projects
            foreach (ProjectGraphNode projectReference in projectGraphNode.ProjectReferences)
            {
                ProjectInstance referencedProjectInstance = projectReference.ProjectInstance;
                if (!webRoleProjects.Contains(referencedProjectInstance.FullPath))
                {
                    continue;
                }

                // The CompileTypeScript target gets triggered as a dependent target, so add predictions for TypeScriptCompile items
                foreach (ProjectItemInstance item in referencedProjectInstance.GetItems(TypeScriptCompileItemsPredictor.TypeScriptCompileItemName))
                {
                    // The item will be relative to the referenced project, not the current ccproj, so make the path absolute.
                    string fullPath = Path.Combine(referencedProjectInstance.Directory, item.EvaluatedInclude);
                    predictionReporter.ReportInputFile(fullPath);
                }

                // The PipelineTransformPhase target builds up FilesForPackagingFromProject items in a bunch of different CollectFilesFrom* dependent targets.
                // Only the ones deemed important and not covered by other predictors will be handled here.

                // Emulates the CollectFilesFromContent target. Note that this target grabs all Content items regardless of whether they're marked as CopyToOutputDirectory
                foreach (ProjectItemInstance item in referencedProjectInstance.GetItems(ContentItemsPredictor.ContentItemName))
                {
                    // The item will be relative to the referenced project, not the current ccproj, so make the path absolute.
                    string fullPath = Path.Combine(referencedProjectInstance.Directory, item.EvaluatedInclude);
                    predictionReporter.ReportInputFile(fullPath);
                }

                // Emulates the CollectFilesFromReference target.
                foreach (ProjectItemInstance item in referencedProjectInstance.GetItems(ReferenceItemsPredictor.ReferenceItemName))
                {
                    if (ReferenceItemsPredictor.TryGetReferenceItemFilePath(referencedProjectInstance, item, out string filePath))
                    {
                        // The item will be relative to the referenced project, not the current ccproj, so make the path absolute.
                        string fullPath = Path.GetFullPath(Path.Combine(referencedProjectInstance.Directory, filePath));
                        predictionReporter.ReportInputFile(fullPath);
                    }
                }
            }
        }
    }
}