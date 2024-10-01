// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors;

public sealed class LinkItemsPredictorTests
{
    [Theory]
    [InlineData(LinkItemsPredictor.LinkItemName)]
    [InlineData(LinkItemsPredictor.LibItemName)]
    [InlineData(LinkItemsPredictor.ImpLibItemName)]
    public void ItemDefinitionGroup(string itemType)
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create(@"src\project.vcxproj");

        ProjectItemDefinitionElement itemDefinition = projectRootElement.AddItemDefinitionGroup().AddItemDefinition(itemType);
        itemDefinition.AddMetadata(LinkItemsPredictor.AdditionalDependenciesMetadata, @"..\AdditionalDependency.lib;%(AdditionalDependencies)");
        itemDefinition.AddMetadata(LinkItemsPredictor.AdditionalLibraryDirectoriesMetadata, @"..\AdditionalLibraryDirectory;%(AdditionalLibraryDirectories)");

        projectRootElement.AddItem(itemType, @"..\someLib.lib");

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        var expectedInputFiles = new[]
        {
            new PredictedItem("someLib.lib", nameof(LinkItemsPredictor)),
            new PredictedItem("AdditionalDependency.lib", nameof(LinkItemsPredictor)),
        };
        var expectedInputDirectories = new[]
        {
            new PredictedItem("AdditionalLibraryDirectory", nameof(LinkItemsPredictor)),
        };
        new LinkItemsPredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                expectedInputFiles.MakeAbsolute(Directory.GetCurrentDirectory()),
                expectedInputDirectories.MakeAbsolute(Directory.GetCurrentDirectory()),
                null,
                null);
    }

    [Theory]
    [InlineData(LinkItemsPredictor.LinkItemName)]
    [InlineData(LinkItemsPredictor.LibItemName)]
    [InlineData(LinkItemsPredictor.ImpLibItemName)]
    public void OverrideMetadata(string itemType)
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create(@"src\project.vcxproj");

        ProjectItemDefinitionElement itemDefinition = projectRootElement.AddItemDefinitionGroup().AddItemDefinition(itemType);
        itemDefinition.AddMetadata(LinkItemsPredictor.AdditionalDependenciesMetadata, @"..\AdditionalDependency.lib;%(AdditionalDependencies)");
        itemDefinition.AddMetadata(LinkItemsPredictor.AdditionalLibraryDirectoriesMetadata, @"..\AdditionalLibraryDirectory;%(AdditionalLibraryDirectories)");

        ProjectItemElement item = projectRootElement.AddItem(itemType, @"..\someLib.lib");
        item.AddMetadata(LinkItemsPredictor.AdditionalDependenciesMetadata, @"..\ReplacedAdditionalDependency.lib");
        item.AddMetadata(LinkItemsPredictor.AdditionalLibraryDirectoriesMetadata, @"..\ReplacedAdditionalLibraryDirectory");

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        var expectedInputFiles = new[]
        {
            new PredictedItem("someLib.lib", nameof(LinkItemsPredictor)),
            new PredictedItem("ReplacedAdditionalDependency.lib", nameof(LinkItemsPredictor)),
        };
        var expectedInputDirectories = new[]
        {
            new PredictedItem("ReplacedAdditionalLibraryDirectory", nameof(LinkItemsPredictor)),
        };
        new LinkItemsPredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                expectedInputFiles.MakeAbsolute(Directory.GetCurrentDirectory()),
                expectedInputDirectories.MakeAbsolute(Directory.GetCurrentDirectory()),
                null,
                null);
    }

    [Theory]
    [InlineData(LinkItemsPredictor.LinkItemName)]
    [InlineData(LinkItemsPredictor.LibItemName)]
    [InlineData(LinkItemsPredictor.ImpLibItemName)]
    public void AppendMetadata(string itemType)
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create(@"src\project.vcxproj");

        ProjectItemDefinitionElement itemDefinition = projectRootElement.AddItemDefinitionGroup().AddItemDefinition(itemType);
        itemDefinition.AddMetadata(LinkItemsPredictor.AdditionalDependenciesMetadata, @"..\AdditionalDependency.lib;%(AdditionalDependencies)");
        itemDefinition.AddMetadata(LinkItemsPredictor.AdditionalLibraryDirectoriesMetadata, @"..\AdditionalLibraryDirectory;%(AdditionalLibraryDirectories)");

        ProjectItemElement item = projectRootElement.AddItem(itemType, @"..\someLib.lib");
        item.AddMetadata(LinkItemsPredictor.AdditionalDependenciesMetadata, @"..\AnotherAdditionalDependency.lib;%(AdditionalDependencies)");
        item.AddMetadata(LinkItemsPredictor.AdditionalLibraryDirectoriesMetadata, @"..\AnotherAdditionalLibraryDirectory;%(AdditionalLibraryDirectories)");

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        var expectedInputFiles = new[]
        {
            new PredictedItem("someLib.lib", nameof(LinkItemsPredictor)),
            new PredictedItem("AdditionalDependency.lib", nameof(LinkItemsPredictor)),
            new PredictedItem("AnotherAdditionalDependency.lib", nameof(LinkItemsPredictor)),
        };
        var expectedInputDirectories = new[]
        {
            new PredictedItem("AdditionalLibraryDirectory", nameof(LinkItemsPredictor)),
            new PredictedItem("AnotherAdditionalLibraryDirectory", nameof(LinkItemsPredictor)),
        };
        new LinkItemsPredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                expectedInputFiles.MakeAbsolute(Directory.GetCurrentDirectory()),
                expectedInputDirectories.MakeAbsolute(Directory.GetCurrentDirectory()),
                null,
                null);
    }

    [Theory]
    [InlineData(LinkItemsPredictor.LinkItemName)]
    [InlineData(LinkItemsPredictor.LibItemName)]
    [InlineData(LinkItemsPredictor.ImpLibItemName)]
    public void DuplicatesAndSpaces(string itemType)
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create(@"src\project.vcxproj");

        ProjectItemDefinitionElement itemDefinition = projectRootElement.AddItemDefinitionGroup().AddItemDefinition(itemType);
        itemDefinition.AddMetadata(LinkItemsPredictor.AdditionalDependenciesMetadata, @"..\AdditionalDependency.lib;%(AdditionalDependencies)");
        itemDefinition.AddMetadata(LinkItemsPredictor.AdditionalLibraryDirectoriesMetadata, @"..\AdditionalLibraryDirectory;%(AdditionalLibraryDirectories)");

        ProjectItemElement item = projectRootElement.AddItem(itemType, @"..\someLib.lib");
        item.AddMetadata(
            LinkItemsPredictor.AdditionalDependenciesMetadata,
            $@"%(AdditionalDependencies); ;;%(AdditionalDependencies);..\AnotherAdditionalDependency.lib;{Environment.NewLine}..\AnotherAdditionalDependency.lib;%(AdditionalDependencies)");
        item.AddMetadata(
            LinkItemsPredictor.AdditionalLibraryDirectoriesMetadata,
            $@"%(AdditionalLibraryDirectories); ;;%(AdditionalLibraryDirectories);..\AnotherAdditionalLibraryDirectories;{Environment.NewLine}..\AnotherAdditionalLibraryDirectories;%(AdditionalLibraryDirectories)");

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

        var expectedInputFiles = new[]
        {
            new PredictedItem("someLib.lib", nameof(LinkItemsPredictor)),
            new PredictedItem("AdditionalDependency.lib", nameof(LinkItemsPredictor)),
            new PredictedItem("AnotherAdditionalDependency.lib", nameof(LinkItemsPredictor)),
        };
        var expectedInputDirectories = new[]
        {
            new PredictedItem("AdditionalLibraryDirectory", nameof(LinkItemsPredictor)),
            new PredictedItem("AnotherAdditionalLibraryDirectories", nameof(LinkItemsPredictor)),
        };
        new LinkItemsPredictor()
            .GetProjectPredictions(projectInstance)
            .AssertPredictions(
                projectInstance,
                expectedInputFiles.MakeAbsolute(Directory.GetCurrentDirectory()),
                expectedInputDirectories.MakeAbsolute(Directory.GetCurrentDirectory()),
                null,
                null);
    }

    [Fact]
    public void SkipOtherProjectTypes()
    {
        ProjectRootElement projectRootElement = ProjectRootElement.Create(@"src\project.csproj");

        ProjectItemDefinitionElement itemDefinition = projectRootElement.AddItemDefinitionGroup().AddItemDefinition(LinkItemsPredictor.LinkItemName);
        itemDefinition.AddMetadata(LinkItemsPredictor.AdditionalDependenciesMetadata, @"..\AdditionalDependency.lib;%(AdditionalDependencies)");
        itemDefinition.AddMetadata(LinkItemsPredictor.AdditionalLibraryDirectoriesMetadata, @"..\AdditionalLibraryDirectory;%(AdditionalLibraryDirectories)");

        projectRootElement.AddItem(LinkItemsPredictor.LinkItemName, @"..\someLib.lib");

        ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);
        new LinkItemsPredictor()
            .GetProjectPredictions(projectInstance)
            .AssertNoPredictions();
    }
}