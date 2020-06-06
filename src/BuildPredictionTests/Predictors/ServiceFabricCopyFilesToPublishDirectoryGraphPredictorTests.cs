// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using System;
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class ServiceFabricCopyFilesToPublishDirectoryGraphPredictorTests
    {
        private readonly string _rootDir;

        public ServiceFabricCopyFilesToPublishDirectoryGraphPredictorTests()
        {
            // Isolate each test into its own folder
            _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(ServiceFabricCopyFilesToPublishDirectoryGraphPredictor), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootDir);
        }

        [Fact]
        public void FindItems()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.sfproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(GetCopyToPublishDirectoryItemsGraphPredictor.PublishDirPropertyName, @"bin\Publish");

            ProjectRootElement service1 = CreateServiceProject("service1");
            ProjectRootElement service2 = CreateServiceProject("service2");
            ProjectRootElement service3 = CreateServiceProject("service3");

            projectRootElement.AddItem("ProjectReference", @"..\service1\service1.csproj");
            projectRootElement.AddItem("ProjectReference", @"..\service2\service2.csproj");
            projectRootElement.AddItem("ProjectReference", @"..\service3\service3.csproj");

            projectRootElement.Save();
            service1.Save();
            service2.Save();
            service3.Save();

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"service1_dep1\service1_dep1.xml", nameof(ServiceFabricCopyFilesToPublishDirectoryGraphPredictor)),
                new PredictedItem(@"service1_dep2\service1_dep2.xml", nameof(ServiceFabricCopyFilesToPublishDirectoryGraphPredictor)),
                new PredictedItem(@"service2_dep1\service2_dep1.xml", nameof(ServiceFabricCopyFilesToPublishDirectoryGraphPredictor)),
                new PredictedItem(@"service2_dep2\service2_dep2.xml", nameof(ServiceFabricCopyFilesToPublishDirectoryGraphPredictor)),
                new PredictedItem(@"service3_dep1\service3_dep1.xml", nameof(ServiceFabricCopyFilesToPublishDirectoryGraphPredictor)),
                new PredictedItem(@"service3_dep2\service3_dep2.xml", nameof(ServiceFabricCopyFilesToPublishDirectoryGraphPredictor)),
            };

            var expectedOutputDirectories = new[]
            {
                new PredictedItem(@"src\bin\Publish", nameof(ServiceFabricCopyFilesToPublishDirectoryGraphPredictor)),
            };

            new ServiceFabricCopyFilesToPublishDirectoryGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertPredictions(
                    _rootDir,
                    expectedInputFiles,
                    null,
                    null,
                    expectedOutputDirectories);
        }

        [Fact]
        public void SkipOtherProjectTypes()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(GetCopyToPublishDirectoryItemsGraphPredictor.PublishDirPropertyName, @"bin\Publish");

            ProjectRootElement service1 = CreateServiceProject("service1");
            ProjectRootElement service2 = CreateServiceProject("service2");
            ProjectRootElement service3 = CreateServiceProject("service3");

            projectRootElement.AddItem("ProjectReference", @"..\service1\service1.csproj");
            projectRootElement.AddItem("ProjectReference", @"..\service2\service2.csproj");
            projectRootElement.AddItem("ProjectReference", @"..\service3\service3.csproj");

            projectRootElement.Save();
            service1.Save();
            service2.Save();
            service3.Save();

            new ServiceFabricPackageRootFilesGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertNoPredictions();
        }

        private ProjectRootElement CreateServiceProject(string projectName)
        {
            string projectDir = Path.Combine(_rootDir, projectName);
            Directory.CreateDirectory(projectDir);

            string projectFileName = projectName + ".csproj";
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(projectDir, projectFileName));

            ProjectRootElement dep1 = CreateDependencyProject(projectName + "_dep1");
            ProjectRootElement dep2 = CreateDependencyProject(projectName + "_dep2");
            ProjectRootElement dep3 = CreateDependencyProject(projectName + "_dep3");

            // The main project depends on 1 and 2; 2 depends on 3; 3 depends on 1. Note that this should *not* be transitive
            projectRootElement.AddItem("ProjectReference", @$"..\{projectName}_dep1\{projectName}_dep1.csproj");
            projectRootElement.AddItem("ProjectReference", @$"..\{projectName}_dep2\{projectName}_dep2.csproj");
            dep2.AddItem("ProjectReference", @$"..\{projectName}_dep3\{projectName}_dep3.csproj");
            dep3.AddItem("ProjectReference", @$"..\{projectName}_dep1\{projectName}_dep1.csproj");

            dep1.Save();
            dep2.Save();
            dep3.Save();

            return projectRootElement;
        }

        private ProjectRootElement CreateDependencyProject(string projectName)
        {
            string projectDir = Path.Combine(_rootDir, projectName);
            Directory.CreateDirectory(projectDir);

            string projectFileName = projectName + ".csproj";
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(projectDir, projectFileName));

            projectRootElement.AddItem(ContentItemsPredictor.ContentItemName, projectName + ".xml")
                .AddMetadata(GetCopyToPublishDirectoryItemsGraphPredictor.CopyToPublishDirectoryMetadataName, "PreserveNewest");

            // The caller may modify the returned project, so don't save it yet.
            return projectRootElement;
        }
    }
}
