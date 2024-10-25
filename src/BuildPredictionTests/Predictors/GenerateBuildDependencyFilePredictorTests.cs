// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors;

public class GenerateBuildDependencyFilePredictorTests
{
    [Fact]
    public void DoesNotGenerateDependencyFile()
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create("project.csproj");

        string projectAssetsFile = Path.Combine(projectRootElement.DirectoryPath, @"obj\project.assets.json");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.ProjectAssetsFilePropertyName, projectAssetsFile);

        string projectDepsFilePath = Path.Combine(projectRootElement.DirectoryPath, @"bin\x64\Debug\net8.0\project.deps.json");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.ProjectDepsFilePathPropertyName, projectDepsFilePath);

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        new GenerateBuildDependencyFilePredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                null,
                null,
                null,
                null);
    }

    [Fact]
    public void GeneratesDependencyFile()
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create("project.csproj");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.GenerateDependencyFilePropertyName, "true");

        string projectAssetsFile = Path.Combine(projectRootElement.DirectoryPath, @"obj\project.assets.json");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.ProjectAssetsFilePropertyName, projectAssetsFile);

        string projectDepsFilePath = Path.Combine(projectRootElement.DirectoryPath, @"bin\x64\Debug\net8.0\project.deps.json");
        projectRootElement.AddProperty(GenerateBuildDependencyFilePredictor.ProjectDepsFilePathPropertyName, projectDepsFilePath);

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        var expectedInputFiles = new[]
        {
            new PredictedItem(projectAssetsFile, nameof(GenerateBuildDependencyFilePredictor)),
        };
        var expectedOutputFiles = new[]
        {
            new PredictedItem(projectDepsFilePath, nameof(GenerateBuildDependencyFilePredictor)),
        };

        new GenerateBuildDependencyFilePredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                expectedInputFiles,
                null,
                expectedOutputFiles,
                null);
    }
}