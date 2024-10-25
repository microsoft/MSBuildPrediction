// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors;

public class GeneratePublishDependencyFilePredictorTests
{
    [Fact]
    public void DoesNotGenerateDependencyFile()
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create("project.csproj");

        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.IntermediateOutputPathPropertyName, @"obj\");
        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.PublishDirPropertyName, @"bin\x64\Debug\net8.0\publish\");
        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.ProjectDepsFileNamePropertyName, "project.deps.json");

        string projectAssetsFile = Path.Combine(projectRootElement.DirectoryPath, @"obj\project.assets.json");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.ProjectAssetsFilePropertyName, projectAssetsFile);

        string projectDepsFilePath = Path.Combine(projectRootElement.DirectoryPath, @"bin\x64\Debug\net8.0\project.deps.json");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.ProjectDepsFilePathPropertyName, projectDepsFilePath);

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        new GeneratePublishDependencyFilePredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                null,
                null,
                null,
                null);
    }

    [Fact]
    public void UseBuildDependencyFile()
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create("project.csproj");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.GenerateDependencyFilePropertyName, "true");

        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.IntermediateOutputPathPropertyName, @"obj\");
        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.PublishDirPropertyName, @"bin\x64\Debug\net8.0\publish\");
        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.ProjectDepsFileNamePropertyName, "project.deps.json");

        string projectAssetsFile = Path.Combine(projectRootElement.DirectoryPath, @"obj\project.assets.json");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.ProjectAssetsFilePropertyName, projectAssetsFile);

        string projectDepsFilePath = Path.Combine(projectRootElement.DirectoryPath, @"bin\x64\Debug\net8.0\project.deps.json");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.ProjectDepsFilePathPropertyName, projectDepsFilePath);

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        new GeneratePublishDependencyFilePredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                null,
                null,
                null,
                null);
    }

    [Fact]
    public void PublishTrimmed()
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create("project.csproj");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.GenerateDependencyFilePropertyName, "true");
        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.SelfContainedPropertyName, "true");
        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.PublishTrimmedPropertyName, "true");

        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.IntermediateOutputPathPropertyName, @"obj\");
        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.PublishDirPropertyName, @"bin\x64\Debug\net8.0\publish\");
        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.ProjectDepsFileNamePropertyName, "project.deps.json");

        string projectAssetsFile = Path.Combine(projectRootElement.DirectoryPath, @"obj\project.assets.json");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.ProjectAssetsFilePropertyName, projectAssetsFile);

        string projectDepsFilePath = Path.Combine(projectRootElement.DirectoryPath, @"bin\x64\Debug\net8.0\project.deps.json");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.ProjectDepsFilePathPropertyName, projectDepsFilePath);

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        var expectedInputFiles = new[]
        {
            new PredictedItem(projectAssetsFile, nameof(GeneratePublishDependencyFilePredictor)),
        };
        var expectedOutputFiles = new[]
        {
            new PredictedItem(@"bin\x64\Debug\net8.0\publish\project.deps.json", nameof(GeneratePublishDependencyFilePredictor)),
        };

        new GeneratePublishDependencyFilePredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                expectedInputFiles,
                null,
                expectedOutputFiles.MakeAbsolute(projectRootElement.DirectoryPath),
                null);
    }

    [Fact]
    public void PublishSingleFile()
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create("project.csproj");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.GenerateDependencyFilePropertyName, "true");
        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.SelfContainedPropertyName, "true");
        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.PublishSingleFilePropertyName, "true");

        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.IntermediateOutputPathPropertyName, @"obj\");
        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.PublishDirPropertyName, @"bin\x64\Debug\net8.0\publish\");
        projectRootElement.AddProperty(GeneratePublishDependencyFilePredictor.ProjectDepsFileNamePropertyName, "project.deps.json");

        string projectAssetsFile = Path.Combine(projectRootElement.DirectoryPath, @"obj\project.assets.json");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.ProjectAssetsFilePropertyName, projectAssetsFile);

        string projectDepsFilePath = Path.Combine(projectRootElement.DirectoryPath, @"bin\x64\Debug\net8.0\project.deps.json");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.ProjectDepsFilePathPropertyName, projectDepsFilePath);

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        var expectedInputFiles = new[]
        {
            new PredictedItem(projectAssetsFile, nameof(GeneratePublishDependencyFilePredictor)),
        };
        var expectedOutputFiles = new[]
        {
            new PredictedItem(@"obj\project.deps.json", nameof(GeneratePublishDependencyFilePredictor)),
        };

        new GeneratePublishDependencyFilePredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                expectedInputFiles,
                null,
                expectedOutputFiles.MakeAbsolute(projectRootElement.DirectoryPath),
                null);
    }
}