// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class ArtifactsSdkPredictorTests
    {
        private readonly string _rootDir;

        public ArtifactsSdkPredictorTests()
        {
            // Isolate each test into its own folder
            _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(ArtifactsSdkPredictorTests), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootDir);
        }

        [Fact]
        public void SkipWhenNotUsingSdk()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"src\project.csproj"));
            projectRootElement.AddProperty("ArtifactsPath", Path.Combine(_rootDir, "out"));

            var artifactItem = projectRootElement.AddItem(ArtifactsSdkPredictor.ArtifactsItemName, "Artifact.txt");
            artifactItem.AddMetadata(ArtifactsSdkPredictor.DestinationFolderMetadata, @"$(ArtifactsPath)\Project");

            Directory.CreateDirectory(Path.Combine(_rootDir, "src"));
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifact.txt"), "SomeContent");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);
            new ArtifactsSdkPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        [Fact]
        public void FindArtifactsForExistingFile()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"src\project.csproj"));
            projectRootElement.AddProperty(ArtifactsSdkPredictor.UsingMicrosoftArtifactsSdkPropertyName, "true");
            projectRootElement.AddProperty("ArtifactsPath", Path.Combine(_rootDir, "out"));

            var artifactItem = projectRootElement.AddItem(ArtifactsSdkPredictor.ArtifactsItemName, "Artifact.txt");
            artifactItem.AddMetadata(ArtifactsSdkPredictor.DestinationFolderMetadata, @"$(ArtifactsPath)\Project");

            Directory.CreateDirectory(Path.Combine(_rootDir, "src"));
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifact.txt"), "SomeContent");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"src\Artifact.txt", nameof(ArtifactsSdkPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"out\Project\Artifact.txt", nameof(ArtifactsSdkPredictor)),
            };
            new ArtifactsSdkPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindArtifactsForExistingFileMultipleDestinationDirs()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"src\project.csproj"));
            projectRootElement.AddProperty(ArtifactsSdkPredictor.UsingMicrosoftArtifactsSdkPropertyName, "true");
            projectRootElement.AddProperty("ArtifactsPath", Path.Combine(_rootDir, "out"));

            var artifactItem = projectRootElement.AddItem(ArtifactsSdkPredictor.ArtifactsItemName, "Artifact.txt");
            artifactItem.AddMetadata(ArtifactsSdkPredictor.DestinationFolderMetadata, @"$(ArtifactsPath)\Project; $(ArtifactsPath)\Project2; \n\t$(ArtifactsPath)\Project3 ;");

            Directory.CreateDirectory(Path.Combine(_rootDir, "src"));
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifact.txt"), "SomeContent");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"src\Artifact.txt", nameof(ArtifactsSdkPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"out\Project\Artifact.txt", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"out\Project2\Artifact.txt", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"out\Project3\Artifact.txt", nameof(ArtifactsSdkPredictor)),
            };
            new ArtifactsSdkPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindRobocopyForExistingFile()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"src\project.csproj"));
            projectRootElement.AddProperty(ArtifactsSdkPredictor.UsingMicrosoftArtifactsSdkPropertyName, "true");
            projectRootElement.AddProperty("ArtifactsPath", Path.Combine(_rootDir, "out"));

            var artifactItem = projectRootElement.AddItem(ArtifactsSdkPredictor.RobocopyItemName, "Robocopy.txt");
            artifactItem.AddMetadata(ArtifactsSdkPredictor.DestinationFolderMetadata, @"$(ArtifactsPath)\Project");

            Directory.CreateDirectory(Path.Combine(_rootDir, "src"));
            File.WriteAllText(Path.Combine(_rootDir, @"src\Robocopy.txt"), "SomeContent");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"src\Robocopy.txt", nameof(ArtifactsSdkPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"out\Project\Robocopy.txt", nameof(ArtifactsSdkPredictor)),
            };
            new ArtifactsSdkPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindArtifactsForExistingDirectoryRecursive()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"src\project.csproj"));
            projectRootElement.AddProperty(ArtifactsSdkPredictor.UsingMicrosoftArtifactsSdkPropertyName, "true");
            projectRootElement.AddProperty("ArtifactsPath", Path.Combine(_rootDir, "out"));

            var artifactItem = projectRootElement.AddItem(ArtifactsSdkPredictor.ArtifactsItemName, "Artifacts");
            artifactItem.AddMetadata(ArtifactsSdkPredictor.DestinationFolderMetadata, @"$(ArtifactsPath)\Project");

            // Recursive is the default. Also testing the file matching logic here.
            artifactItem.AddMetadata(ArtifactsSdkPredictor.FileMatchMetadata, "*.txt");
            artifactItem.AddMetadata(ArtifactsSdkPredictor.FileExcludeMetadata, "exclude.*");
            artifactItem.AddMetadata(ArtifactsSdkPredictor.DirExcludeMetadata, "excludeDir");

            Directory.CreateDirectory(Path.Combine(_rootDir, @"src\Artifacts"));
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\1.txt"), "SomeContent");
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\2.txt"), "SomeContent");
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\exclude.txt"), "SomeContent"); // excluded explicitly
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\something.jpg"), "SomeContent"); // excluded by not matching

            Directory.CreateDirectory(Path.Combine(_rootDir, @"src\Artifacts\a"));
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\a\3.txt"), "SomeContent");
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\a\4.txt"), "SomeContent");
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\a\exclude.txt"), "SomeContent"); // excluded explicitly
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\a\something.jpg"), "SomeContent"); // excluded by not matching

            Directory.CreateDirectory(Path.Combine(_rootDir, @"src\Artifacts\b"));
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\b\5.txt"), "SomeContent");
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\b\6.txt"), "SomeContent");
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\b\exclude.txt"), "SomeContent"); // excluded explicitly
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\b\something.jpg"), "SomeContent"); // excluded by not matching

            // Whole dir is excluded
            Directory.CreateDirectory(Path.Combine(_rootDir, @"src\Artifacts\excludeDir"));
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\excludeDir\7.txt"), "SomeContent");
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\excludeDir\8.txt"), "SomeContent");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"src\Artifacts\1.txt", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"src\Artifacts\2.txt", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"src\Artifacts\a\3.txt", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"src\Artifacts\a\4.txt", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"src\Artifacts\b\5.txt", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"src\Artifacts\b\6.txt", nameof(ArtifactsSdkPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"out\Project\1.txt", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"out\Project\2.txt", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"out\Project\a\3.txt", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"out\Project\a\4.txt", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"out\Project\b\5.txt", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"out\Project\b\6.txt", nameof(ArtifactsSdkPredictor)),
            };
            new ArtifactsSdkPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindArtifactsForExistingDirectoryNotRecursive()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"src\project.csproj"));
            projectRootElement.AddProperty(ArtifactsSdkPredictor.UsingMicrosoftArtifactsSdkPropertyName, "true");
            projectRootElement.AddProperty("ArtifactsPath", Path.Combine(_rootDir, "out"));

            // Recursive is the default
            var artifactItem = projectRootElement.AddItem(ArtifactsSdkPredictor.ArtifactsItemName, "Artifacts");
            artifactItem.AddMetadata(ArtifactsSdkPredictor.DestinationFolderMetadata, @"$(ArtifactsPath)\Project");

            // Also testing the unspecified match logic here (matches all).
            artifactItem.AddMetadata(ArtifactsSdkPredictor.IsRecursiveMetadata, "false");

            Directory.CreateDirectory(Path.Combine(_rootDir, @"src\Artifacts"));
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\1.txt"), "SomeContent");
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\2.txt"), "SomeContent");

            Directory.CreateDirectory(Path.Combine(_rootDir, @"src\Artifacts\a"));
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\a\3.txt"), "SomeContent");
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\a\4.txt"), "SomeContent");

            Directory.CreateDirectory(Path.Combine(_rootDir, @"src\Artifacts\b"));
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\b\5.txt"), "SomeContent");
            File.WriteAllText(Path.Combine(_rootDir, @"src\Artifacts\b\6.txt"), "SomeContent");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"src\Artifacts\1.txt", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"src\Artifacts\2.txt", nameof(ArtifactsSdkPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"out\Project\1.txt", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"out\Project\2.txt", nameof(ArtifactsSdkPredictor)),
            };
            new ArtifactsSdkPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindArtifactsForGeneratedFile()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"foo\foo.csproj"));
            projectRootElement.AddProperty(ArtifactsSdkPredictor.UsingMicrosoftArtifactsSdkPropertyName, "true");
            projectRootElement.AddProperty("EnlistmentRoot", _rootDir);
            projectRootElement.AddProperty("OutputPath", @"bin\x64");

            // Copying another project's output to this project's output dir
            var artifactItem = projectRootElement.AddItem(ArtifactsSdkPredictor.ArtifactsItemName, @"$(EnlistmentRoot)\bar\$(OutputPath)\Bar.dll;$(EnlistmentRoot)\bar\$(OutputPath)\Bar.pdb");
            artifactItem.AddMetadata(ArtifactsSdkPredictor.DestinationFolderMetadata, @"$(OutputPath)");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"bar\bin\x64\Bar.dll", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"bar\bin\x64\Bar.pdb", nameof(ArtifactsSdkPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"foo\bin\x64\Bar.dll", nameof(ArtifactsSdkPredictor)),
                new PredictedItem(@"foo\bin\x64\Bar.pdb", nameof(ArtifactsSdkPredictor)),
            };
            new ArtifactsSdkPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindArtifactsForGeneratedDirectory()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"foo\foo.csproj"));
            projectRootElement.AddProperty(ArtifactsSdkPredictor.UsingMicrosoftArtifactsSdkPropertyName, "true");
            projectRootElement.AddProperty("EnlistmentRoot", _rootDir);
            projectRootElement.AddProperty("OutputPath", @"bin\x64");

            // Copying another project's output dir to this project's output dir
            var artifactItem = projectRootElement.AddItem(ArtifactsSdkPredictor.ArtifactsItemName, @"$(EnlistmentRoot)\bar\$(OutputPath)");
            artifactItem.AddMetadata(ArtifactsSdkPredictor.DestinationFolderMetadata, @"$(OutputPath)");
            artifactItem.AddMetadata(ArtifactsSdkPredictor.FileMatchMetadata, "*.dll *.pdb");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputDirectories = new[]
            {
                new PredictedItem(@"bar\bin\x64", nameof(ArtifactsSdkPredictor)),
            };
            var expectedOutputDirectories = new[]
            {
                new PredictedItem(@"foo\bin\x64", nameof(ArtifactsSdkPredictor)),
            };
            new ArtifactsSdkPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    null,
                    expectedInputDirectories.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputDirectories.MakeAbsolute(_rootDir));
        }
    }
}