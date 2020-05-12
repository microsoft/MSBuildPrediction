// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class AdditionalIncludeDirectoriesPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance(@"src\project.vcxproj");
            var expectedInputDirectories = new[]
            {
                new PredictedItem("ClCompileIncludes", nameof(AdditionalIncludeDirectoriesPredictor)),
                new PredictedItem("ReplacedFxCompileIncludes", nameof(AdditionalIncludeDirectoriesPredictor)),
                new PredictedItem("MidlIncludes", nameof(AdditionalIncludeDirectoriesPredictor)),
                new PredictedItem("AnotherMidlIncludes", nameof(AdditionalIncludeDirectoriesPredictor)),
                new PredictedItem("ResourceCompileIncludes", nameof(AdditionalIncludeDirectoriesPredictor)),
                new PredictedItem("AnotherResourceCompileIncludes", nameof(AdditionalIncludeDirectoriesPredictor)),
            };
            new AdditionalIncludeDirectoriesPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    null,
                    expectedInputDirectories.MakeAbsolute(Directory.GetCurrentDirectory()),
                    null,
                    null);
        }

        [Fact]
        public void SkipOtherProjectTypes()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance(@"src\project.csproj");
            new AdditionalIncludeDirectoriesPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        private static ProjectInstance CreateTestProjectInstance(string fileName)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(fileName);

#pragma warning disable SA1118 // Parameter should not span multiple lines. Justification: Used to help match formatting in real projects
            projectRootElement.AddItemDefinitionGroup()
                .AddItemDefinition(AdditionalIncludeDirectoriesPredictor.ClCompileItemName)
                .AddMetadata(
                    AdditionalIncludeDirectoriesPredictor.AdditionalIncludeDirectoriesMetadata,
                    @"..\ClCompileIncludes
                    ;%(AdditionalIncludeDirectories)");
            projectRootElement.AddItemDefinitionGroup()
                .AddItemDefinition(AdditionalIncludeDirectoriesPredictor.FxCompileItemName)
                .AddMetadata(
                    AdditionalIncludeDirectoriesPredictor.AdditionalIncludeDirectoriesMetadata,
                    @"..\FxCompileIncludes
                    ;%(AdditionalIncludeDirectories)");
            projectRootElement.AddItemDefinitionGroup()
                .AddItemDefinition(AdditionalIncludeDirectoriesPredictor.MidlItemName)
                .AddMetadata(
                    AdditionalIncludeDirectoriesPredictor.AdditionalIncludeDirectoriesMetadata,
                    @"..\MidlIncludes
                    ;%(AdditionalIncludeDirectories)");
            projectRootElement.AddItemDefinitionGroup()
                .AddItemDefinition(AdditionalIncludeDirectoriesPredictor.ResourceCompileItemName)
                .AddMetadata(
                    AdditionalIncludeDirectoriesPredictor.AdditionalIncludeDirectoriesMetadata,
                    @"..\ResourceCompileIncludes
                    ;%(AdditionalIncludeDirectories)");
#pragma warning restore SA1118 // Parameter should not span multiple lines

            // No change to AdditionalIncludeDirectories
            projectRootElement.AddItem(AdditionalIncludeDirectoriesPredictor.ClCompileItemName, "foo.cpp");

            // Overrieds AdditionalIncludeDirectories
            projectRootElement.AddItem(AdditionalIncludeDirectoriesPredictor.FxCompileItemName, "foo.fx")
                .AddMetadata(
                    AdditionalIncludeDirectoriesPredictor.AdditionalIncludeDirectoriesMetadata,
                    @"..\ReplacedFxCompileIncludes");

            // Appends to AdditionalIncludeDirectories
            projectRootElement.AddItem(AdditionalIncludeDirectoriesPredictor.MidlItemName, "foo.idl")
                .AddMetadata(
                    AdditionalIncludeDirectoriesPredictor.AdditionalIncludeDirectoriesMetadata,
                    @"..\AnotherMidlIncludes;%(AdditionalIncludeDirectories)");

            // Has duplicates and spaces
            projectRootElement.AddItem(AdditionalIncludeDirectoriesPredictor.ResourceCompileItemName, "foo.rc")
                .AddMetadata(
                    AdditionalIncludeDirectoriesPredictor.AdditionalIncludeDirectoriesMetadata,
                    @"%(AdditionalIncludeDirectories); ;;%(AdditionalIncludeDirectories);..\AnotherResourceCompileIncludes;%(AdditionalIncludeDirectories)");

            return TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);
        }
    }
}
