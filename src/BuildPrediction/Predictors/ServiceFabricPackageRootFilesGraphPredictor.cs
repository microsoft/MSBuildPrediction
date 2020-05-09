// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Graph;

    /// <summary>
    /// Predicts inputs for Service Fabric projects based on the behavior of the various packaging targets which get and copy package files.
    /// </summary>
    public sealed class ServiceFabricPackageRootFilesGraphPredictor : IProjectGraphPredictor
    {
        internal const string ServicePackageRootFolderPropertyName = "ServicePackageRootFolder";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(ProjectGraphNode projectGraphNode, ProjectPredictionReporter predictionReporter)
        {
            ProjectInstance projectInstance = projectGraphNode.ProjectInstance;

            // This predictor only applies to sfproj files
            if (!projectInstance.FullPath.EndsWith(".sfproj", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (projectGraphNode.ProjectReferences.Count == 0)
            {
                return;
            }

            string servicePackageRootFolder = projectInstance.GetPropertyValue(ServicePackageRootFolderPropertyName);
            if (string.IsNullOrEmpty(servicePackageRootFolder))
            {
                return;
            }

            foreach (ProjectGraphNode projectReference in projectGraphNode.ProjectReferences)
            {
                // This emulates the behavior of the GetPackageRootFiles task.
                // We do not emulate the behavior of the FilterPackageFiles task as it requires parsing the service manifest
                // and the extra cost and complexity does not outweigh the slight overprediction.
                // For the same reason we are not doing output prediction.
                string projectFolder = Path.GetFullPath(Path.GetDirectoryName(projectReference.ProjectInstance.FullPath));
                string fullPackageRootPath = Path.Combine(projectFolder, servicePackageRootFolder);
                if (fullPackageRootPath[fullPackageRootPath.Length - 1] != Path.DirectorySeparatorChar)
                {
                    fullPackageRootPath += Path.DirectorySeparatorChar;
                }

                // Add files from none and content items under the package root path.
                // This is basically the same as the directory enumeration below, but there are some slight differences, so emulating
                // the odd behavior of GetPackageRootFiles is preferable to potentially being wrong.
                AddPackageRootFilesFromItems(projectReference.ProjectInstance.GetItems(NoneItemsPredictor.NoneItemName), projectFolder, fullPackageRootPath, predictionReporter);
                AddPackageRootFilesFromItems(projectReference.ProjectInstance.GetItems(ContentItemsPredictor.ContentItemName), projectFolder, fullPackageRootPath, predictionReporter);

                // Add files under the package root path.
                string[] packageRootFilesInFileSystem = Directory.GetFiles(fullPackageRootPath, "*", SearchOption.AllDirectories);
                foreach (string packageRootFileInFileSystem in packageRootFilesInFileSystem)
                {
                    predictionReporter.ReportInputFile(packageRootFileInFileSystem);
                }
            }
        }

        private void AddPackageRootFilesFromItems(ICollection<ProjectItemInstance> items, string projectFolder, string packageRootPath, ProjectPredictionReporter predictionReporter)
        {
            foreach (ProjectItemInstance item in items)
            {
                string relativePath = item.EvaluatedInclude;

                // If it's not a valid path, don't process it.
                if (relativePath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    continue;
                }

                // Check whether the file is directly included from within the package root folder.
                string packageFileFullPath = Path.GetFullPath(Path.Combine(projectFolder, relativePath));
                if (packageFileFullPath.StartsWith(packageRootPath, StringComparison.OrdinalIgnoreCase))
                {
                    predictionReporter.ReportInputFile(packageFileFullPath);
                    continue;
                }

                // Check whether the file is referenced elsewhere but linked to be under the package root folder.
                string linkMetadata = item.GetMetadataValue("Link");
                if (!string.IsNullOrEmpty(linkMetadata))
                {
                    string linkFullPath = Path.GetFullPath(Path.Combine(projectFolder, linkMetadata));
                    if (linkFullPath.StartsWith(packageRootPath, StringComparison.OrdinalIgnoreCase))
                    {
                        predictionReporter.ReportInputFile(packageFileFullPath);
                        continue;
                    }
                }
            }
        }
    }
}
