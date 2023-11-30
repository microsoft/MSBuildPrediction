// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class GetCopyToPublishDirectoryItemsGraphPredictorTests
    {
        private readonly string _rootDir;

        public GetCopyToPublishDirectoryItemsGraphPredictorTests()
        {
            // Isolate each test into its own folder
            _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(GetCopyToPublishDirectoryItemsGraphPredictor), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootDir);
        }

        [Fact]
        public void PublishNoCopy()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.DefaultTargets = "Publish";
            projectRootElement.AddProperty(GetCopyToPublishDirectoryItemsGraphPredictor.PublishDirPropertyName, @"bin\Publish");

            const bool shouldCopy = false;
            ProjectRootElement dep1 = CreateDependencyProject("dep1", shouldCopy);
            ProjectRootElement dep2 = CreateDependencyProject("dep2", shouldCopy);
            ProjectRootElement dep3 = CreateDependencyProject("dep3", shouldCopy);

            // The main project depends on 1 and 2; 2 depends on 3; 3 depends on 1. Note that this predictor should *not* be transitive
            projectRootElement.AddItem("ProjectReference", @"..\dep1\dep1.proj");
            projectRootElement.AddItem("ProjectReference", @"..\dep2\dep2.proj");
            dep2.AddItem("ProjectReference", @"..\dep3\dep3.proj");
            dep3.AddItem("ProjectReference", @"..\dep1\dep1.proj");

            projectRootElement.Save();
            dep1.Save();
            dep2.Save();
            dep3.Save();

            new GetCopyToPublishDirectoryItemsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertNoPredictions();
        }

        [Fact]
        public void Publish()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.DefaultTargets = "Publish";
            projectRootElement.AddProperty(GetCopyToPublishDirectoryItemsGraphPredictor.PublishDirPropertyName, @"bin\Publish");

            const bool shouldCopy = true;
            ProjectRootElement dep1 = CreateDependencyProject("dep1", shouldCopy);
            ProjectRootElement dep2 = CreateDependencyProject("dep2", shouldCopy);
            ProjectRootElement dep3 = CreateDependencyProject("dep3", shouldCopy);

            // The main project depends on 1 and 2; 2 depends on 3; 3 depends on 1. Note that this should *not* be transitive
            projectRootElement.AddItem("ProjectReference", @"..\dep1\dep1.proj");
            projectRootElement.AddItem("ProjectReference", @"..\dep2\dep2.proj");
            dep2.AddItem("ProjectReference", @"..\dep3\dep3.proj");
            dep3.AddItem("ProjectReference", @"..\dep1\dep1.proj");

            projectRootElement.Save();
            dep1.Save();
            dep2.Save();
            dep3.Save();

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"dep1\dep1.xml", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep1\dep1.resx", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep1\dep1.cs", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep1\dep1.txt", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.xml", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.resx", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.cs", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.txt", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
            };

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"src\bin\Publish\dep1.xml", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\Publish\dep1.resx", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\Publish\dep1.cs", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\Publish\dep1.txt", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\Publish\dep2.xml", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\Publish\dep2.resx", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\Publish\dep2.cs", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\Publish\dep2.txt", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
            };

            new GetCopyToPublishDirectoryItemsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertPredictions(
                    _rootDir,
                    expectedInputFiles,
                    null,
                    expectedOutputFiles,
                    null);
        }

        [Fact]
        public void DeployOnBuild()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(GetCopyToPublishDirectoryItemsGraphPredictor.PublishDirPropertyName, @"bin\Publish");
            projectRootElement.AddProperty(GetCopyToPublishDirectoryItemsGraphPredictor.PublishDirPropertyName, @"bin\Publish");
            projectRootElement.AddProperty(GetCopyToPublishDirectoryItemsGraphPredictor.SupportsDeployOnBuildPropertyName, "true");
            projectRootElement.AddProperty(GetCopyToPublishDirectoryItemsGraphPredictor.DeployOnBuildPropertyName, "true");

            const bool shouldCopy = true;
            ProjectRootElement dep1 = CreateDependencyProject("dep1", shouldCopy);
            ProjectRootElement dep2 = CreateDependencyProject("dep2", shouldCopy);
            ProjectRootElement dep3 = CreateDependencyProject("dep3", shouldCopy);

            // The main project depends on 1 and 2; 2 depends on 3; 3 depends on 1. Note that this should *not* be transitive
            projectRootElement.AddItem("ProjectReference", @"..\dep1\dep1.proj");
            projectRootElement.AddItem("ProjectReference", @"..\dep2\dep2.proj");
            dep2.AddItem("ProjectReference", @"..\dep3\dep3.proj");
            dep3.AddItem("ProjectReference", @"..\dep1\dep1.proj");

            projectRootElement.Save();
            dep1.Save();
            dep2.Save();
            dep3.Save();

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"dep1\dep1.xml", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep1\dep1.resx", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep1\dep1.cs", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep1\dep1.txt", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.xml", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.resx", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.cs", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.txt", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
            };

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"src\bin\Publish\dep1.xml", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\Publish\dep1.resx", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\Publish\dep1.cs", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\Publish\dep1.txt", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\Publish\dep2.xml", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\Publish\dep2.resx", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\Publish\dep2.cs", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\Publish\dep2.txt", nameof(GetCopyToPublishDirectoryItemsGraphPredictor)),
            };

            new GetCopyToPublishDirectoryItemsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertPredictions(
                    _rootDir,
                    expectedInputFiles,
                    null,
                    expectedOutputFiles,
                    null);
        }

        private ProjectRootElement CreateDependencyProject(string projectName, bool shouldCopy)
        {
            string projectDir = Path.Combine(_rootDir, projectName);
            Directory.CreateDirectory(projectDir);

            string projectFileName = projectName + ".proj";
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(projectDir, projectFileName));

            ProjectItemElement contentItem = projectRootElement.AddItem(ContentItemsPredictor.ContentItemName, projectName + ".xml");
            ProjectItemElement embeddedResourceItem = projectRootElement.AddItem(EmbeddedResourceItemsPredictor.EmbeddedResourceItemName, projectName + ".resx");
            ProjectItemElement compileItem = projectRootElement.AddItem(CompileItemsPredictor.CompileItemName, projectName + ".cs");
            ProjectItemElement noneItem = projectRootElement.AddItem(NoneItemsPredictor.NoneItemName, projectName + ".txt");

            if (shouldCopy)
            {
                contentItem.AddMetadata(GetCopyToPublishDirectoryItemsGraphPredictor.CopyToPublishDirectoryMetadataName, "PreserveNewest");
                embeddedResourceItem.AddMetadata(GetCopyToPublishDirectoryItemsGraphPredictor.CopyToPublishDirectoryMetadataName, "PreserveNewest");
                compileItem.AddMetadata(GetCopyToPublishDirectoryItemsGraphPredictor.CopyToPublishDirectoryMetadataName, "PreserveNewest");
                noneItem.AddMetadata(GetCopyToPublishDirectoryItemsGraphPredictor.CopyToPublishDirectoryMetadataName, "PreserveNewest");
            }

            // The caller may modify the returned project, so don't save it yet.
            return projectRootElement;
        }
    }
}