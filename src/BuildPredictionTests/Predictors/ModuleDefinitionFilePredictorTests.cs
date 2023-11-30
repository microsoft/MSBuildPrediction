// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class ModuleDefinitionFilePredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance(@"project.vcxproj");
            var expectedInputDirectories = new[]
            {
                new PredictedItem("Link.def", nameof(ModuleDefinitionFilePredictor)),
                new PredictedItem("Lib.def", nameof(ModuleDefinitionFilePredictor)),
                new PredictedItem("ImpLib.def", nameof(ModuleDefinitionFilePredictor)),
            };
            new ModuleDefinitionFilePredictor()
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
            ProjectInstance projectInstance = CreateTestProjectInstance(@"project.csproj");
            new ModuleDefinitionFilePredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        private static ProjectInstance CreateTestProjectInstance(string fileName)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(fileName);

            projectRootElement.AddItemDefinitionGroup()
                .AddItemDefinition(ModuleDefinitionFilePredictor.LinkItemName)
                .AddMetadata(ModuleDefinitionFilePredictor.ModuleDefinitionFileMetadata, @".\Link.def");
            projectRootElement.AddItem(ModuleDefinitionFilePredictor.LinkItemName, "Link.lib");

            projectRootElement.AddItem(ModuleDefinitionFilePredictor.LibItemName, "Lib.lib")
                .AddMetadata(ModuleDefinitionFilePredictor.ModuleDefinitionFileMetadata, @".\Lib.def");

            // Has spaces
            projectRootElement.AddItem(ModuleDefinitionFilePredictor.ImpLibItemName, "ImpLib.lib")
                .AddMetadata(ModuleDefinitionFilePredictor.ModuleDefinitionFileMetadata, @" .\ImpLib.def ");

            return TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);
        }
    }
}