// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using System;
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class StyleCopPredictorTests
    {
        private readonly string _rootDir;

        public StyleCopPredictorTests()
        {
            // Isolate each test into its own folder
            _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(StyleCopPredictorTests), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootDir);
        }

        [Fact]
        public void SkipWhenStyleCopMissing()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);
            new StyleCopPredictor()
                .GetProjectPredictions(project)
                .AssertNoPredictions();
        }

        [Fact]
        public void SkipWhenStyleCopDisabled()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopEnabledPropertyName, "false");
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopOutputFilePropertyName, @"bin\x64\StyleCopViolations.xml");
            projectRootElement.AddTarget(StyleCopPredictor.StyleCopTargetName);

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);
            new StyleCopPredictor()
                .GetProjectPredictions(project)
                .AssertNoPredictions();
        }

        [Fact]
        public void FindItemsWithNoSettingsFile()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopEnabledPropertyName, "true");
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopOutputFilePropertyName, @"bin\x64\StyleCopViolations.xml");
            projectRootElement.AddTarget(StyleCopPredictor.StyleCopTargetName);

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"bin\x64\StyleCopViolations.xml", nameof(StyleCopPredictor)),
            };
            new StyleCopPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    null,
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindItemsWithAddinPaths()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopEnabledPropertyName, "true");
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopOutputFilePropertyName, @"bin\x64\StyleCopViolations.xml");
            projectRootElement.AddTarget(StyleCopPredictor.StyleCopTargetName);

            projectRootElement.AddItem(StyleCopPredictor.StyleCopAdditionalAddinPathsItemName, @"addinPaths\1");
            projectRootElement.AddItem(StyleCopPredictor.StyleCopAdditionalAddinPathsItemName, @"addinPaths\2");
            projectRootElement.AddItem(StyleCopPredictor.StyleCopAdditionalAddinPathsItemName, @"addinPaths\3");
            projectRootElement.AddItem(StyleCopPredictor.StyleCopAdditionalAddinPathsItemName, @"addinPaths\doesNotExist"); // Will be excluded

            Directory.CreateDirectory(Path.Combine(_rootDir, @"addinPaths\1"));
            Directory.CreateDirectory(Path.Combine(_rootDir, @"addinPaths\2"));
            Directory.CreateDirectory(Path.Combine(_rootDir, @"addinPaths\3"));

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputDirectories = new[]
            {
                new PredictedItem(@"addinPaths\1", nameof(StyleCopPredictor)),
                new PredictedItem(@"addinPaths\2", nameof(StyleCopPredictor)),
                new PredictedItem(@"addinPaths\3", nameof(StyleCopPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"bin\x64\StyleCopViolations.xml", nameof(StyleCopPredictor)),
            };
            new StyleCopPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    null,
                    expectedInputDirectories.MakeAbsolute(_rootDir),
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindItemsWithOverrideSettingsFile()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopEnabledPropertyName, "true");
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopOutputFilePropertyName, @"bin\x64\StyleCopViolations.xml");
            projectRootElement.AddTarget(StyleCopPredictor.StyleCopTargetName);

            File.WriteAllText(Path.Combine(_rootDir, "CustomSettings.StyleCop"), "<StyleCopSettings></StyleCopSettings>");
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopOverrideSettingsFilePropertyName, "CustomSettings.StyleCop");

            // Ensure the default project settings don't get picked up
            File.WriteAllText(Path.Combine(_rootDir, StyleCopPredictor.StyleCopSettingsDefaultFileName), "SomeContent");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("CustomSettings.StyleCop", nameof(StyleCopPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"bin\x64\StyleCopViolations.xml", nameof(StyleCopPredictor)),
            };
            new StyleCopPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindItemsWithMissingOverrideSettingsFile()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopEnabledPropertyName, "true");
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopOutputFilePropertyName, @"bin\x64\StyleCopViolations.xml");
            projectRootElement.AddTarget(StyleCopPredictor.StyleCopTargetName);

            projectRootElement.AddProperty(StyleCopPredictor.StyleCopOverrideSettingsFilePropertyName, "CustomSettings.StyleCop");

            // Ensure the default project settings do get picked up
            File.WriteAllText(Path.Combine(_rootDir, StyleCopPredictor.StyleCopSettingsDefaultFileName), "SomeContent");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(StyleCopPredictor.StyleCopSettingsDefaultFileName, nameof(StyleCopPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"bin\x64\StyleCopViolations.xml", nameof(StyleCopPredictor)),
            };
            new StyleCopPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindItemsWithMissingInvalidSettingsFile()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopEnabledPropertyName, "true");
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopOutputFilePropertyName, @"bin\x64\StyleCopViolations.xml");
            projectRootElement.AddTarget(StyleCopPredictor.StyleCopTargetName);

            File.WriteAllText(Path.Combine(_rootDir, "CustomSettings.StyleCop"), "This is not valid Xml");
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopOverrideSettingsFilePropertyName, "CustomSettings.StyleCop");

            // Ensure the default project settings do get picked up
            File.WriteAllText(Path.Combine(_rootDir, StyleCopPredictor.StyleCopSettingsDefaultFileName), "SomeContent");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(StyleCopPredictor.StyleCopSettingsDefaultFileName, nameof(StyleCopPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"bin\x64\StyleCopViolations.xml", nameof(StyleCopPredictor)),
            };
            new StyleCopPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindItemsWithDefaultSettingsFile()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopEnabledPropertyName, "true");
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopOutputFilePropertyName, @"bin\x64\StyleCopViolations.xml");
            projectRootElement.AddTarget(StyleCopPredictor.StyleCopTargetName);

            File.WriteAllText(Path.Combine(_rootDir, StyleCopPredictor.StyleCopSettingsDefaultFileName), "SomeContent");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(StyleCopPredictor.StyleCopSettingsDefaultFileName, nameof(StyleCopPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"bin\x64\StyleCopViolations.xml", nameof(StyleCopPredictor)),
            };
            new StyleCopPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindItemsWithAlternateSettingsFile()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopEnabledPropertyName, "true");
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopOutputFilePropertyName, @"bin\x64\StyleCopViolations.xml");
            projectRootElement.AddTarget(StyleCopPredictor.StyleCopTargetName);

            File.WriteAllText(Path.Combine(_rootDir, StyleCopPredictor.StyleCopSettingsAlternateFileName), "SomeContent");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(StyleCopPredictor.StyleCopSettingsAlternateFileName, nameof(StyleCopPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"bin\x64\StyleCopViolations.xml", nameof(StyleCopPredictor)),
            };
            new StyleCopPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindItemsWithLegacySettingsFile()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopEnabledPropertyName, "true");
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopOutputFilePropertyName, @"bin\x64\StyleCopViolations.xml");
            projectRootElement.AddTarget(StyleCopPredictor.StyleCopTargetName);

            File.WriteAllText(Path.Combine(_rootDir, StyleCopPredictor.StyleCopSettingsLegacyFileName), "SomeContent");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(StyleCopPredictor.StyleCopSettingsLegacyFileName, nameof(StyleCopPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"bin\x64\StyleCopViolations.xml", nameof(StyleCopPredictor)),
            };
            new StyleCopPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindItemsWithMultipleSettingsFiles()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopEnabledPropertyName, "true");
            projectRootElement.AddProperty(StyleCopPredictor.StyleCopOutputFilePropertyName, @"bin\x64\StyleCopViolations.xml");
            projectRootElement.AddTarget(StyleCopPredictor.StyleCopTargetName);

            // Only the default one will be chosen as it's the first one which exists.
            File.WriteAllText(Path.Combine(_rootDir, StyleCopPredictor.StyleCopSettingsDefaultFileName), "SomeContent");
            File.WriteAllText(Path.Combine(_rootDir, StyleCopPredictor.StyleCopSettingsAlternateFileName), "SomeContent");
            File.WriteAllText(Path.Combine(_rootDir, StyleCopPredictor.StyleCopSettingsLegacyFileName), "SomeContent");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(StyleCopPredictor.StyleCopSettingsDefaultFileName, nameof(StyleCopPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"bin\x64\StyleCopViolations.xml", nameof(StyleCopPredictor)),
            };
            new StyleCopPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }
    }
}