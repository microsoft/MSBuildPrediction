// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors;

public class DotnetSdkPredictorTests
{
    private readonly string _rootDir;

    public DotnetSdkPredictorTests()
    {
        // Isolate each test into its own folder
        _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(DotnetSdkPredictorTests), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_rootDir);
    }

    [Fact]
    public void GlobalJsonExistsAdjacent()
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"src\project.csproj"));
        projectRootElement.AddProperty(DotnetSdkPredictor.UsingMicrosoftNETSdkPropertyName, "true");

        Directory.CreateDirectory(Path.Combine(_rootDir, "src"));
        File.WriteAllText(Path.Combine(_rootDir, @"src\global.json"), "{}");

        // Extraneous one above, which is not predicted as an input.
        File.WriteAllText(Path.Combine(_rootDir, "global.json"), "{}");

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        var expectedInputFiles = new[]
        {
            new PredictedItem(@"src\global.json", nameof(DotnetSdkPredictor)),
        };

        new DotnetSdkPredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                expectedInputFiles.MakeAbsolute(_rootDir),
                null,
                null,
                null);
    }

    [Fact]
    public void GlobalJsonExistsAbove()
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"src\project.csproj"));
        projectRootElement.AddProperty(DotnetSdkPredictor.UsingMicrosoftNETSdkPropertyName, "true");

        File.WriteAllText(Path.Combine(_rootDir, "global.json"), "{}");

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        var expectedInputFiles = new[]
        {
            new PredictedItem(@"global.json", nameof(DotnetSdkPredictor)),
        };

        new DotnetSdkPredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                expectedInputFiles.MakeAbsolute(_rootDir),
                null,
                null,
                null);
    }

    [Fact]
    public void NoGlobalJsonExists()
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"src\project.csproj"));
        projectRootElement.AddProperty(DotnetSdkPredictor.UsingMicrosoftNETSdkPropertyName, "true");

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        new DotnetSdkPredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                null,
                null,
                null,
                null);
    }

    [Fact]
    public void NotUsingDotnetSdk()
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"src\project.csproj"));

        Directory.CreateDirectory(Path.Combine(_rootDir, "src"));
        File.WriteAllText(Path.Combine(_rootDir, "global.json"), "{}");

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        new DotnetSdkPredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                null,
                null,
                null,
                null);
    }
}