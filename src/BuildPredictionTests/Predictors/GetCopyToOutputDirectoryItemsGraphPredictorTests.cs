// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
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

            // The main project depends on 1 and 2; 2 depends on 3; 3 depends on 1.
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

            // The main project depends on 1 and 2; 2 depends on 3; 3 depends on 1.
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

        [Theory]
        [InlineData(false, false)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(true, true)]
        public void WithCopy(bool copyContentTransitively, bool hasRuntimeOutput)
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.OutDirPropertyName, @"bin\");

            const bool shouldCopy = true;
            ProjectRootElement dep1 = CreateDependencyProject("dep1", shouldCopy);
            ProjectRootElement dep2 = CreateDependencyProject("dep2", shouldCopy);
            ProjectRootElement dep3 = CreateDependencyProject("dep3", shouldCopy);

            AddPropertyToAllProjects(GetCopyToOutputDirectoryItemsGraphPredictor.MSBuildCopyContentTransitivelyPropertyName, copyContentTransitively.ToString());

            AddPropertyToAllProjects(GenerateBuildDependencyFilePredictor.ProjectDepsFilePathPropertyName, @"$(MSBuildProjectDirectory)\bin\$(MSBuildProjectName).deps.json");
            AddPropertyToAllProjects(GenerateRuntimeConfigurationFilesPredictor.ProjectRuntimeConfigFilePathPropertyName, @"$(MSBuildProjectDirectory)\bin\$(MSBuildProjectName).runtimeconfig.json");
            AddPropertyToAllProjects(GenerateRuntimeConfigurationFilesPredictor.ProjectRuntimeConfigDevFilePathPropertyName, @"$(MSBuildProjectDirectory)\bin\$(MSBuildProjectName).runtimeconfig.dev.json");

            if (hasRuntimeOutput)
            {
                AddPropertyToAllProjects(GetCopyToOutputDirectoryItemsGraphPredictor.HasRuntimeOutputPropertyName, "true");
                AddPropertyToAllProjects(GenerateBuildDependencyFilePredictor.GenerateDependencyFilePropertyName, "true");
                AddPropertyToAllProjects(GenerateRuntimeConfigurationFilesPredictor.GenerateRuntimeConfigurationFilesPropertyName, "true");
            }

            // The main project depends on 1 and 2; 2 depends on 3; 3 depends on 1.
            projectRootElement.AddItem("ProjectReference", @"..\dep1\dep1.proj");
            projectRootElement.AddItem("ProjectReference", @"..\dep2\dep2.proj");
            dep2.AddItem("ProjectReference", @"..\dep3\dep3.proj");
            dep3.AddItem("ProjectReference", @"..\dep1\dep1.proj");

            projectRootElement.Save();
            dep1.Save();
            dep2.Save();
            dep3.Save();

            List<PredictedItem> expectedInputFiles =
            [
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
            ];

            List<PredictedItem> expectedOutputFiles =
            [
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
            ];

            if (hasRuntimeOutput)
            {
                expectedInputFiles.AddRange(
                    [
                        new PredictedItem(@"dep1\bin\dep1.deps.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"dep1\bin\dep1.runtimeconfig.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"dep1\bin\dep1.runtimeconfig.dev.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"dep2\bin\dep2.deps.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"dep2\bin\dep2.runtimeconfig.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"dep2\bin\dep2.runtimeconfig.dev.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                    ]);

                expectedOutputFiles.AddRange(
                    [
                        new PredictedItem(@"src\bin\dep1.deps.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"src\bin\dep1.runtimeconfig.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"src\bin\dep1.runtimeconfig.dev.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"src\bin\dep2.deps.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"src\bin\dep2.runtimeconfig.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"src\bin\dep2.runtimeconfig.dev.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                    ]);
            }

            if (copyContentTransitively)
            {
                expectedInputFiles.AddRange(
                    [
                        new PredictedItem(@"dep3\dep3.xml", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"dep3\dep3.resx", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"dep3\dep3.cs", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"dep3\dep3.txt", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"dep3\dep3.xaml", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                    ]);

                expectedOutputFiles.AddRange(
                    [
                        new PredictedItem(@"src\bin\dep3.xml", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"src\bin\dep3.resx", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"src\bin\dep3.cs", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"src\bin\dep3.txt", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        new PredictedItem(@"src\bin\dep3.xaml", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                    ]);

                if (hasRuntimeOutput)
                {
                    expectedInputFiles.AddRange(
                        [
                            new PredictedItem(@"dep3\bin\dep3.deps.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                            new PredictedItem(@"dep3\bin\dep3.runtimeconfig.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                            new PredictedItem(@"dep3\bin\dep3.runtimeconfig.dev.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        ]);

                    expectedOutputFiles.AddRange(
                        [
                            new PredictedItem(@"src\bin\dep3.deps.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                            new PredictedItem(@"src\bin\dep3.runtimeconfig.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                            new PredictedItem(@"src\bin\dep3.runtimeconfig.dev.json", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                        ]);
                    }
            }

            new GetCopyToOutputDirectoryItemsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertPredictions(
                    _rootDir,
                    expectedInputFiles,
                    null,
                    expectedOutputFiles,
                    null);

            void AddPropertyToAllProjects(string propertyName, string propertyValue)
            {
                projectRootElement.AddProperty(propertyName, propertyValue);
                dep1.AddProperty(propertyName, propertyValue);
                dep2.AddProperty(propertyName, propertyValue);
                dep3.AddProperty(propertyName, propertyValue);
            }
        }

        [Fact]
        public void DependencyWithFakesAssemblies()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.OutDirPropertyName, @"bin\");

            string dependencyProjectFile = Path.Combine(_rootDir, @"dep\dep.csproj");
            ProjectRootElement dependencyProjectRootElement = ProjectRootElement.Create(dependencyProjectFile);
            dependencyProjectRootElement.AddProperty(FakesPredictor.FakesImportedPropertyName, "true");
            dependencyProjectRootElement.AddProperty(FakesPredictor.FakesUseV2GenerationPropertyName, "true");
            dependencyProjectRootElement.AddProperty(FakesPredictor.FakesOutputPathPropertyName, @"bin\FakesAssemblies");
            dependencyProjectRootElement.AddItem(FakesPredictor.FakesItemName, "A.fakes");
            dependencyProjectRootElement.AddItem(FakesPredictor.FakesItemName, "B.fakes");
            dependencyProjectRootElement.AddItem(FakesPredictor.FakesItemName, "C.fakes");

            projectRootElement.AddItem("ProjectReference", @"..\dep\dep.csproj");

            projectRootElement.Save();
            dependencyProjectRootElement.Save();

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"dep\bin\FakesAssemblies\A.Fakes.dll", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep\bin\FakesAssemblies\B.Fakes.dll", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"dep\bin\FakesAssemblies\C.Fakes.dll", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
            };

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"src\bin\A.Fakes.dll", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\B.Fakes.dll", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
                new PredictedItem(@"src\bin\C.Fakes.dll", nameof(GetCopyToOutputDirectoryItemsGraphPredictor)),
            };

            new GetCopyToOutputDirectoryItemsGraphPredictor()
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