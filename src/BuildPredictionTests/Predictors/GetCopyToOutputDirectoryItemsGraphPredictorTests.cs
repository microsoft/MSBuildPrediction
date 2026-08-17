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

        [Theory]
        [InlineData("Always")]
        [InlineData("PreserveNewest")]
        [InlineData("IfDifferent")]
        public void TransitiveProjectReferenceOutputCopiedAsContent(string copyToOutputDirectory)
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.OutDirPropertyName, @"bin\");
            projectRootElement.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.MSBuildCopyContentTransitivelyPropertyName, "true");

            string directDependencyFile = Path.Combine(_rootDir, @"direct\direct.csproj");
            ProjectRootElement directDependency = ProjectRootElement.Create(directDependencyFile);
            directDependency.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.MSBuildCopyContentTransitivelyPropertyName, "true");

            string contentDependencyFile = Path.Combine(_rootDir, @"content\content.csproj");
            ProjectRootElement contentDependency = ProjectRootElement.Create(contentDependencyFile);
            contentDependency.AddProperty("TargetPath", @"bin\content.dll");

            projectRootElement.AddItem("ProjectReference", @"..\direct\direct.csproj");
            ProjectItemElement contentReference = directDependency.AddItem("ProjectReference", @"..\content\content.csproj");
            contentReference.AddMetadata("OutputItemType", "Content");
            contentReference.AddMetadata("CopyToOutputDirectory", copyToOutputDirectory);

            projectRootElement.Save();
            directDependency.Save();
            contentDependency.Save();

            new GetCopyToOutputDirectoryItemsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertPredictions(
                    _rootDir,
                    [new PredictedItem(@"content\bin\content.dll", nameof(GetCopyToOutputDirectoryItemsGraphPredictor))],
                    null,
                    [new PredictedItem(@"src\bin\content.dll", nameof(GetCopyToOutputDirectoryItemsGraphPredictor))],
                    null);
        }

        [Fact]
        public void SharedTransitiveProjectReferenceOutputCopiedAsContent()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.OutDirPropertyName, @"bin\");
            projectRootElement.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.MSBuildCopyContentTransitivelyPropertyName, "true");

            ProjectRootElement ordinaryDependency = ProjectRootElement.Create(Path.Combine(_rootDir, @"aaa\aaa.csproj"));
            ProjectRootElement contentDependency = ProjectRootElement.Create(Path.Combine(_rootDir, @"bbb\bbb.csproj"));
            ProjectRootElement sharedDependency = ProjectRootElement.Create(Path.Combine(_rootDir, @"zzz\zzz.csproj"));
            ordinaryDependency.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.MSBuildCopyContentTransitivelyPropertyName, "true");
            contentDependency.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.MSBuildCopyContentTransitivelyPropertyName, "true");
            sharedDependency.AddProperty("TargetPath", @"bin\content.dll");

            projectRootElement.AddItem("ProjectReference", @"..\aaa\aaa.csproj");
            projectRootElement.AddItem("ProjectReference", @"..\bbb\bbb.csproj");
            ordinaryDependency.AddItem("ProjectReference", @"..\zzz\zzz.csproj");
            ProjectItemElement contentReference = contentDependency.AddItem("ProjectReference", @"..\zzz\zzz.csproj");
            contentReference.AddMetadata("OutputItemType", "Content");
            contentReference.AddMetadata("CopyToOutputDirectory", "PreserveNewest");

            projectRootElement.Save();
            ordinaryDependency.Save();
            contentDependency.Save();
            sharedDependency.Save();

            new GetCopyToOutputDirectoryItemsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertPredictions(
                    _rootDir,
                    [new PredictedItem(@"zzz\bin\content.dll", nameof(GetCopyToOutputDirectoryItemsGraphPredictor))],
                    null,
                    [new PredictedItem(@"src\bin\content.dll", nameof(GetCopyToOutputDirectoryItemsGraphPredictor))],
                    null);
        }

        [Fact]
        public void UnsetMSBuildCopyContentTransitivelyUsesLegacyOneLevelBehavior()
        {
            string projectFile = Path.Combine(_rootDir, "src", "project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.OutDirPropertyName, "bin");

            string directDependencyFile = Path.Combine(_rootDir, "direct", "direct.csproj");
            ProjectRootElement directDependency = ProjectRootElement.Create(directDependencyFile);

            string contentDependencyFile = Path.Combine(_rootDir, "content", "content.csproj");
            ProjectRootElement contentDependency = ProjectRootElement.Create(contentDependencyFile);
            contentDependency.AddProperty("TargetPath", Path.Combine("bin", "content.dll"));

            projectRootElement.AddItem("ProjectReference", Path.Combine("..", "direct", "direct.csproj"));
            ProjectItemElement contentReference = directDependency.AddItem("ProjectReference", Path.Combine("..", "content", "content.csproj"));
            contentReference.AddMetadata("OutputItemType", "Content");
            contentReference.AddMetadata("CopyToOutputDirectory", "PreserveNewest");

            projectRootElement.Save();
            directDependency.Save();
            contentDependency.Save();

            new GetCopyToOutputDirectoryItemsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertNoPredictions();
        }

        [Theory]
        [InlineData("TargetPath", "nested", "renamed.dll")]
        [InlineData("Link", "linked", "linked.dll")]
        public void ProjectReferenceOutputUsesDestinationMetadata(string metadataName, string destinationDirectory, string destinationFileName)
        {
            string destinationPath = Path.Combine(destinationDirectory, destinationFileName);
            string projectFile = Path.Combine(_rootDir, "src", "project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.OutDirPropertyName, "bin");

            string dependencyFile = Path.Combine(_rootDir, "dep", "dep.csproj");
            ProjectRootElement dependency = ProjectRootElement.Create(dependencyFile);
            dependency.AddProperty("TargetPath", Path.Combine("bin", "dep.dll"));

            ProjectItemElement projectReference = projectRootElement.AddItem("ProjectReference", Path.Combine("..", "dep", "dep.csproj"));
            projectReference.AddMetadata("OutputItemType", "Content");
            projectReference.AddMetadata("CopyToOutputDirectory", "PreserveNewest");
            projectReference.AddMetadata(metadataName, destinationPath);

            projectRootElement.Save();
            dependency.Save();

            new GetCopyToOutputDirectoryItemsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertPredictions(
                    _rootDir,
                    [new PredictedItem(Path.Combine("dep", "bin", "dep.dll"), nameof(GetCopyToOutputDirectoryItemsGraphPredictor))],
                    null,
                    [new PredictedItem(Path.Combine("src", "bin", destinationPath), nameof(GetCopyToOutputDirectoryItemsGraphPredictor))],
                    null);
        }

        [Fact]
        public void AmbiguousProjectReferenceOutputDestinationsAreNotPredicted()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.OutDirPropertyName, @"bin\");

            string dependencyFile = Path.Combine(_rootDir, @"dep\dep.csproj");
            ProjectRootElement dependency = ProjectRootElement.Create(dependencyFile);
            dependency.AddProperty("TargetPath", @"bin\dep.dll");

            ProjectItemElement targetPathReference = projectRootElement.AddItem("ProjectReference", @"..\dep\dep.csproj");
            targetPathReference.AddMetadata("OutputItemType", "Content");
            targetPathReference.AddMetadata("CopyToOutputDirectory", "PreserveNewest");
            targetPathReference.AddMetadata("TargetPath", "nested/renamed.dll");

            ProjectItemElement linkReference = projectRootElement.AddItem("ProjectReference", @"..\dep\dep.csproj");
            linkReference.AddMetadata("OutputItemType", "Content");
            linkReference.AddMetadata("CopyToOutputDirectory", "PreserveNewest");
            linkReference.AddMetadata("Link", "linked/linked.dll");

            projectRootElement.Save();
            dependency.Save();

            new GetCopyToOutputDirectoryItemsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertPredictions(
                    _rootDir,
                    [new PredictedItem(@"dep\bin\dep.dll", nameof(GetCopyToOutputDirectoryItemsGraphPredictor))],
                    null,
                    null,
                    null);
        }

        [Theory]
        [InlineData("Content", "Never")]
        [InlineData(null, "PreserveNewest")]
        public void ProjectReferenceOutputNotCopiedAsContent(string outputItemType, string copyToOutputDirectory)
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.OutDirPropertyName, @"bin\");

            string dependencyFile = Path.Combine(_rootDir, @"dep\dep.csproj");
            ProjectRootElement dependency = ProjectRootElement.Create(dependencyFile);
            dependency.AddProperty("TargetPath", @"bin\dep.dll");

            ProjectItemElement projectReference = projectRootElement.AddItem("ProjectReference", @"..\dep\dep.csproj");
            if (outputItemType != null)
            {
                projectReference.AddMetadata("OutputItemType", outputItemType);
            }

            projectReference.AddMetadata("CopyToOutputDirectory", copyToOutputDirectory);

            projectRootElement.Save();
            dependency.Save();

            new GetCopyToOutputDirectoryItemsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertNoPredictions();
        }

        [Fact]
        public void AmbiguousProjectReferenceOutputIsNotPredicted()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.OutDirPropertyName, @"bin\");

            string dependencyFile = Path.Combine(_rootDir, @"dep\dep.csproj");
            ProjectRootElement dependency = ProjectRootElement.Create(dependencyFile);
            dependency.AddProperty("TargetPath", @"bin\dep.dll");

            ProjectItemElement copiedReference = projectRootElement.AddItem("ProjectReference", @"..\dep\dep.csproj");
            copiedReference.AddMetadata("OutputItemType", "Content");
            copiedReference.AddMetadata("CopyToOutputDirectory", "PreserveNewest");
            projectRootElement.AddItem("ProjectReference", @"..\dep\dep.csproj");

            projectRootElement.Save();
            dependency.Save();

            new GetCopyToOutputDirectoryItemsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertNoPredictions();
        }

        [Fact]
        public void CaseSensitiveProjectPathsAreDistinct()
        {
            if (Environment.OSVersion.Platform != PlatformID.MacOSX
                && Environment.OSVersion.Platform != PlatformID.Unix)
            {
                return;
            }

            string projectFile = Path.Combine(_rootDir, "src", "project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(GetCopyToOutputDirectoryItemsGraphPredictor.OutDirPropertyName, "bin");

            ProjectRootElement copiedDependency = ProjectRootElement.Create(Path.Combine(_rootDir, "Dep", "project.csproj"));
            copiedDependency.AddProperty("TargetPath", Path.Combine("bin", "copied.dll"));
            ProjectRootElement ordinaryDependency = ProjectRootElement.Create(Path.Combine(_rootDir, "dep", "project.csproj"));
            ordinaryDependency.AddProperty("TargetPath", Path.Combine("bin", "ordinary.dll"));

            ProjectItemElement copiedReference = projectRootElement.AddItem("ProjectReference", Path.Combine("..", "Dep", "project.csproj"));
            copiedReference.AddMetadata("OutputItemType", "Content");
            copiedReference.AddMetadata("CopyToOutputDirectory", "PreserveNewest");
            projectRootElement.AddItem("ProjectReference", Path.Combine("..", "dep", "project.csproj"));

            projectRootElement.Save();
            copiedDependency.Save();
            ordinaryDependency.Save();

            new GetCopyToOutputDirectoryItemsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertPredictions(
                    _rootDir,
                    [new PredictedItem(Path.Combine("Dep", "bin", "copied.dll"), nameof(GetCopyToOutputDirectoryItemsGraphPredictor))],
                    null,
                    [new PredictedItem(Path.Combine("src", "bin", "copied.dll"), nameof(GetCopyToOutputDirectoryItemsGraphPredictor))],
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