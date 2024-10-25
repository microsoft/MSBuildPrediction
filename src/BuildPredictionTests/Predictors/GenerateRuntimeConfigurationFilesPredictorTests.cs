// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors;

public class GenerateRuntimeConfigurationFilesPredictorTests
{
    private readonly string _rootDir;

    public GenerateRuntimeConfigurationFilesPredictorTests()
    {
        // Isolate each test into its own folder
        _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(GenerateRuntimeConfigurationFilesPredictorTests), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_rootDir);
    }

    [Fact]
    public void DoesNotGenerateRuntimeConfigurationFiles()
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"src\project.csproj"));

        string userRuntimeConfig = Path.Combine(projectRootElement.DirectoryPath, @"runtimeconfig.template.json");
        projectRootElement.AddProperty(GenerateRuntimeConfigurationFilesPredictor.UserRuntimeConfigPropertyName, userRuntimeConfig);

        string projectRuntimeConfigFilePath = Path.Combine(projectRootElement.DirectoryPath, @"bin\x64\Debug\net8.0\project.runtimeconfig.json");
        projectRootElement.AddProperty(GenerateRuntimeConfigurationFilesPredictor.ProjectRuntimeConfigFilePathPropertyName, projectRuntimeConfigFilePath);

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        new GenerateRuntimeConfigurationFilesPredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                null,
                null,
                null,
                null);
    }

    [Fact]
    public void GeneratesRuntimeConfigurationFiles()
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"src\project.csproj"));
        projectRootElement.AddProperty(GenerateRuntimeConfigurationFilesPredictor.GenerateRuntimeConfigurationFilesPropertyName, "true");

        string userRuntimeConfig = Path.Combine(projectRootElement.DirectoryPath, @"runtimeconfig.template.json");
        projectRootElement.AddProperty(GenerateRuntimeConfigurationFilesPredictor.UserRuntimeConfigPropertyName, userRuntimeConfig);

        string projectRuntimeConfigFilePath = Path.Combine(projectRootElement.DirectoryPath, @"bin\x64\Debug\net8.0\project.runtimeconfig.json");
        projectRootElement.AddProperty(GenerateRuntimeConfigurationFilesPredictor.ProjectRuntimeConfigFilePathPropertyName, projectRuntimeConfigFilePath);

        string projectRuntimeConfigDevFilePath = Path.Combine(projectRootElement.DirectoryPath, @"bin\x64\Debug\net8.0\project.runtimeconfig.dev.json");
        projectRootElement.AddProperty(GenerateRuntimeConfigurationFilesPredictor.ProjectRuntimeConfigDevFilePathPropertyName, projectRuntimeConfigDevFilePath);

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        var expectedOutputFiles = new[]
        {
            new PredictedItem(projectRuntimeConfigFilePath, nameof(GenerateRuntimeConfigurationFilesPredictor)),
            new PredictedItem(projectRuntimeConfigDevFilePath, nameof(GenerateRuntimeConfigurationFilesPredictor)),
        };

        new GenerateRuntimeConfigurationFilesPredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                null,
                null,
                expectedOutputFiles,
                null);
    }

    [Fact]
    public void UserRuntimeConfigExists()
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"src\project.csproj"));
        projectRootElement.AddProperty(GenerateRuntimeConfigurationFilesPredictor.GenerateRuntimeConfigurationFilesPropertyName, "true");

        string userRuntimeConfig = Path.Combine(projectRootElement.DirectoryPath, @"runtimeconfig.template.json");
        projectRootElement.AddProperty(GenerateRuntimeConfigurationFilesPredictor.UserRuntimeConfigPropertyName, userRuntimeConfig);
        Directory.CreateDirectory(projectRootElement.DirectoryPath);
        File.WriteAllText(userRuntimeConfig, "dummy");

        string projectRuntimeConfigFilePath = Path.Combine(projectRootElement.DirectoryPath, @"bin\x64\Debug\net8.0\project.runtimeconfig.json");
        projectRootElement.AddProperty(GenerateRuntimeConfigurationFilesPredictor.ProjectRuntimeConfigFilePathPropertyName, projectRuntimeConfigFilePath);

        string projectRuntimeConfigDevFilePath = Path.Combine(projectRootElement.DirectoryPath, @"bin\x64\Debug\net8.0\project.runtimeconfig.dev.json");
        projectRootElement.AddProperty(GenerateRuntimeConfigurationFilesPredictor.ProjectRuntimeConfigDevFilePathPropertyName, projectRuntimeConfigDevFilePath);

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        var expectedInputFiles = new[]
        {
            new PredictedItem(userRuntimeConfig, nameof(GenerateRuntimeConfigurationFilesPredictor)),
        };
        var expectedOutputFiles = new[]
        {
            new PredictedItem(projectRuntimeConfigFilePath, nameof(GenerateRuntimeConfigurationFilesPredictor)),
            new PredictedItem(projectRuntimeConfigDevFilePath, nameof(GenerateRuntimeConfigurationFilesPredictor)),
        };

        new GenerateRuntimeConfigurationFilesPredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                expectedInputFiles,
                null,
                expectedOutputFiles,
                null);
    }
}