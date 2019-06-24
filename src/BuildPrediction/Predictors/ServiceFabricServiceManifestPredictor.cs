// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System;
    using System.IO;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Predicts inputs for Service Fabric projects based on the service manifest files (ServiceManifest.xml).
    /// </summary>
    public class ServiceFabricServiceManifestPredictor : IProjectPredictor
    {
        internal const string ProjectReferenceItemName = "ProjectReference";

        internal const string ServicePackageRootFolderPropertyName = "ServicePackageRootFolder";

        internal const string ApplicationPackageRootFolderPropertyName = "ApplicationPackageRootFolder";

        internal const string ServiceManifestFileName = "ServiceManifest.xml";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            Project project,
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            // This predictor only applies to sfproj files
            if (!projectInstance.FullPath.EndsWith(".sfproj", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            /*
                From Microsoft.VisualStudio.Azure.Fabric.ApplicationProject.targets in the FixUpServiceFabricApplicationManifest target:
                  <ItemGroup>
                    <_ServiceManifestFullPath Include="@(ProjectReference -> '%(RootDir)%(Directory)$(ServicePackageRootFolder)\ServiceManifest.xml')" />
                  </ItemGroup>

                  <FindServiceManifests ApplicationPackageRootFolder="$(ApplicationPackageRootFolder)">
                    <Output TaskParameter="ServiceManifestFiles" ItemName="_ServiceManifestFullPath" />
                  </FindServiceManifests>

                Then it runs a cleanup util on @(_ServiceManifestFullPath), making them all inputs.
            */
            var servicePackageRootFolder = projectInstance.GetPropertyValue(ServicePackageRootFolderPropertyName);
            foreach (var projectReferenceItem in projectInstance.GetItems(ProjectReferenceItemName))
            {
                // Equivalent of '%(RootDir)%(Directory)$(ServicePackageRootFolder)\ServiceManifest.xml'
                var projectReferenceRootDir = projectReferenceItem.GetMetadataValue("RootDir");
                var projectReferenceDirectory = projectReferenceItem.GetMetadataValue("Directory");
                var serviceManifestFile = projectReferenceRootDir + projectReferenceDirectory + servicePackageRootFolder + Path.DirectorySeparatorChar + ServiceManifestFileName;
                predictionReporter.ReportInputFile(serviceManifestFile);
            }

            // The FindServiceManifests task simply enumerates the directories in $(ApplicationPackageRootFolder) looking for ServiceManifest.xml files.
            var applicationPackageRootFolder = projectInstance.GetPropertyValue(ApplicationPackageRootFolderPropertyName);
            if (!Path.IsPathRooted(applicationPackageRootFolder))
            {
                applicationPackageRootFolder = Path.Combine(projectInstance.Directory, applicationPackageRootFolder);
            }

            foreach (string directory in Directory.EnumerateDirectories(applicationPackageRootFolder))
            {
                string serviceManifestFile = Path.Combine(directory, ServiceManifestFileName);
                if (File.Exists(serviceManifestFile))
                {
                    predictionReporter.ReportInputFile(serviceManifestFile);
                }
            }
        }
    }
}
