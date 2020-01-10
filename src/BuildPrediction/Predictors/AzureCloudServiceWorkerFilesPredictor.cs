// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System;
    using System.IO;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Predicts inputs for Azure Cloud Service projects for the worker project files to be copied to the CS package.
    /// </summary>
    public sealed class AzureCloudServiceWorkerFilesPredictor : IProjectPredictor
    {
        internal const string ProjectReferenceItemName = "ProjectReference";

        internal const string AppConfigFileName = "app.config";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            // This predictor only applies to ccproj files
            if (!projectInstance.FullPath.EndsWith(".ccproj", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            /*
                From Microsoft.WindowsAzure.targets in the CollectWorkerRoleFiles target:
                    <MSBuild
                      Projects="$(_WorkerRoleProject)"
                      Targets="SourceFilesProjectOutputGroup"
                      Properties="$(_WorkerRoleSetConfiguration);$(_WorkerRoleSetPlatform)"
                      ContinueOnError="false">
                      <Output TaskParameter="TargetOutputs" ItemName="SourceFilesOutputGroup" />
                    </MSBuild>

                    <!-- Add the app config file from SourceFilesOutputGroup -->
                    <ItemGroup>
                      <WorkerFiles Include="@(SourceFilesOutputGroup)" Condition=" '%(SourceFilesOutputGroup.TargetPath)' == '$(WorkerEntryPoint).config' " >
                        <TargetPath>%(TargetPath)</TargetPath>
                        <RoleOwner>$(_WorkerRoleProject)</RoleOwner>
                        <RoleOwnerName>$(_WorkerRoleProjectName)</RoleOwnerName>
                      </WorkerFiles>
                    </ItemGroup>

                Effectively, it just finds project references' app.config files to copy. Note that in theory the app.config can be already named <assembly-name>.config,
                but this should be uncommon so for now this is a gap we're willing to live with.

                TODO: There is a lot more logic adding WorkerFiles. However, they're generally all in the project reference's output directory or from nuget packages,
                neither of which require as precise prediction. Perhaps when we have a better way to make predictions based on dependencies we can be more accurate here.
            */
            foreach (var projectReferenceItem in projectInstance.GetItems(ProjectReferenceItemName))
            {
                var projectReferenceRootDir = projectReferenceItem.GetMetadataValue("RootDir");
                var projectReferenceDirectory = projectReferenceItem.GetMetadataValue("Directory");
                var appConfigFile = projectReferenceRootDir + projectReferenceDirectory + AppConfigFileName;
                if (File.Exists(appConfigFile))
                {
                    predictionReporter.ReportInputFile(appConfigFile);
                }
            }
        }
    }
}
