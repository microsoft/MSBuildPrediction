// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class GetCopyToOutputDirectoryItemsGraphPredictorTests
    {
        private readonly string _rootDir;

        public GetCopyToOutputDirectoryItemsGraphPredictorTests()
        {
            // Isolate each test into its own folder
            _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(GetCopyToOutputDirectoryItemsGraphPredictor), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootDir);
        }

        [Fact]
        public void NoCopy()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(ContentItemsPredictor.OutDirPropertyName, @"bin\");

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

            new GetCopyToOutputDirectoryItemsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertNoPredictions();
        }

        [Fact]
        public void UseCommonOutputDirectory()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.OutDirPropertyName, @"bin\");
            projectRootElement.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.UseCommonOutputDirectoryPropertyName, "true");

            const bool shouldCopy = true;
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

            new GetCopyToOutputDirectoryItemsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertNoPredictions();
        }

        [Fact]
        public void WithCopy()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.OutDirPropertyName, @"bin\");

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
                new PredictedItem(@"dep1\dep1.xml", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep1\dep1.resx", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep1\dep1.cs", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep1\dep1.txt", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep1\dep1.xaml", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.xml", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.resx", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.cs", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.txt", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.xaml", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
            };

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"src\bin\dep1.xml", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\dep1.resx", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\dep1.cs", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\dep1.txt", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\dep1.xaml", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\dep2.xml", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\dep2.resx", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\dep2.cs", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\dep2.txt", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\dep2.xaml", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
            };

            var expectedDependencies = new[]
            {
                new PredictedItem(@"dep1\dep1.proj", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.proj", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
            };

            new GetCopyToOutputDirectoryItemsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertPredictions(
                    _rootDir,
                    expectedInputFiles,
                    null,
                    expectedOutputFiles,
                    null,
                    expectedDependencies);
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
            ProjectItemElement xamlAppDefItem = projectRootElement.AddItem(XamlAppDefPredictor.XamlAppDefItemName, projectName + ".xaml");

            if (shouldCopy)
            {
                contentItem.AddMetadata("CopyToOutputDirectory", "PreserveNewest");
                embeddedResourceItem.AddMetadata("CopyToOutputDirectory", "PreserveNewest");
                compileItem.AddMetadata("CopyToOutputDirectory", "PreserveNewest");
                noneItem.AddMetadata("CopyToOutputDirectory", "PreserveNewest");
                xamlAppDefItem.AddMetadata("CopyToOutputDirectory", "PreserveNewest");
            }

            // The caller may modify the returned project, so don't save it yet.
            return projectRootElement;
        }
    }
}