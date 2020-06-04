// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class CppContentFilesProjectOutputGroupPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance(@"project.vcxproj");
            var expectedInputFiles = new[]
            {
                new PredictedItem("Xml.xml", nameof(CppContentFilesProjectOutputGroupPredictor)),
                new PredictedItem("Text.txt", nameof(CppContentFilesProjectOutputGroupPredictor)),
                new PredictedItem("Font.ttf", nameof(CppContentFilesProjectOutputGroupPredictor)),
                new PredictedItem("Object.obj", nameof(CppContentFilesProjectOutputGroupPredictor)),
                new PredictedItem("Library.lib", nameof(CppContentFilesProjectOutputGroupPredictor)),
                new PredictedItem("Manifest.manifest", nameof(CppContentFilesProjectOutputGroupPredictor)),
                new PredictedItem("Image.bmp", nameof(CppContentFilesProjectOutputGroupPredictor)),
                new PredictedItem("Media.wav", nameof(CppContentFilesProjectOutputGroupPredictor)),
            };
            new CppContentFilesProjectOutputGroupPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles,
                    null,
                    null,
                    null);
        }

        [Fact]
        public void SkipOtherProjectTypes()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance(@"project.csproj");
            new CppContentFilesProjectOutputGroupPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        private static ProjectInstance CreateTestProjectInstance(string fileName)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(fileName);

            projectRootElement.AddItem(CppContentFilesProjectOutputGroupPredictor.XmlItemName, "Xml.xml");
            projectRootElement.AddItem(CppContentFilesProjectOutputGroupPredictor.TextItemName, "Text.txt");
            projectRootElement.AddItem(CppContentFilesProjectOutputGroupPredictor.FontItemName, "Font.ttf");
            projectRootElement.AddItem(CppContentFilesProjectOutputGroupPredictor.ObjectItemName, "Object.obj");
            projectRootElement.AddItem(CppContentFilesProjectOutputGroupPredictor.LibraryItemName, "Library.lib");
            projectRootElement.AddItem(CppContentFilesProjectOutputGroupPredictor.ManifestItemName, "Manifest.manifest");
            projectRootElement.AddItem(CppContentFilesProjectOutputGroupPredictor.ImageItemName, "Image.bmp");
            projectRootElement.AddItem(CppContentFilesProjectOutputGroupPredictor.MediaItemName, "Media.wav");

            return TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);
        }
    }
}
