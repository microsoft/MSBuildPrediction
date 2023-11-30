// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class AzureCloudServiceWorkerFilesPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance("project.ccproj");
            var expectedInputFiles = new[]
            {
                new PredictedItem($@"Worker1\{AzureCloudServiceWorkerFilesPredictor.AppConfigFileName}", nameof(AzureCloudServiceWorkerFilesPredictor)),
                new PredictedItem($@"Worker2\{AzureCloudServiceWorkerFilesPredictor.AppConfigFileName}", nameof(AzureCloudServiceWorkerFilesPredictor)),
            };
            new AzureCloudServiceWorkerFilesPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles.MakeAbsolute(Directory.GetCurrentDirectory()),
                    null,
                    null,
                    null);
        }

        [Fact]
        public void SkipOtherProjectTypes()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance("project.csproj");
            new AzureCloudServiceWorkerFilesPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        private static ProjectInstance CreateTestProjectInstance(string fileName)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create($@"AzureCloudService\{fileName}");

            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            itemGroup.AddItem(AzureCloudServiceWorkerFilesPredictor.ProjectReferenceItemName, @"..\Worker1\Worker1.csproj");
            itemGroup.AddItem(AzureCloudServiceWorkerFilesPredictor.ProjectReferenceItemName, @"..\Worker2\Worker2.csproj");
            itemGroup.AddItem(AzureCloudServiceWorkerFilesPredictor.ProjectReferenceItemName, @"..\WorkerNoAppConfig\WorkerNoAppConfig.csproj");

            // Add app.config files since existence is checked, but not for WorkerNoAppConfig
            Directory.CreateDirectory(@"Worker1");
            File.WriteAllText($@"Worker1\{AzureCloudServiceWorkerFilesPredictor.AppConfigFileName}", "SomeContent");
            Directory.CreateDirectory(@"Worker2");
            File.WriteAllText($@"Worker2\{AzureCloudServiceWorkerFilesPredictor.AppConfigFileName}", "SomeContent");

            return TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);
        }
    }
}