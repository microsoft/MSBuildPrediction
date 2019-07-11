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

    public class ManifestsPredictorTests
    {
        private readonly string _rootDir;

        public ManifestsPredictorTests()
        {
            // Isolate each test into its own folder
            _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(ManifestsPredictorTests), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootDir);
        }

        [Fact]
        public void SkipWhenNoManifest()
        {
            ProjectRootElement projectRootElement = CreateProjectRootElement();

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);
            new ManifestsPredictor()
                .GetProjectPredictions(project)
                .AssertNoPredictions();
        }

        [Fact]
        public void FindItemsForWin32Manifest()
        {
            ProjectRootElement projectRootElement = CreateProjectRootElement();
            projectRootElement.AddProperty(ManifestsPredictor.ApplicationManifestPropertyName, "app.manifest");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"app.manifest", nameof(ManifestsPredictor)),
            };
            new ManifestsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    null,
                    null);
        }

        [Fact]
        public void FindItemsForClickOnceWithApplicationManifest()
        {
            ProjectRootElement projectRootElement = CreateProjectRootElement();

            // This is generally set by the toolset for certain output types
            projectRootElement.AddProperty(ManifestsPredictor.GenerateClickOnceManifestsPropertyName, "true");

            projectRootElement.AddProperty(ManifestsPredictor.ApplicationManifestPropertyName, "app.manifest");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"app.manifest", nameof(ManifestsPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\Release\MyApplication.exe.manifest", nameof(ManifestsPredictor)),
                new PredictedItem(@"bin\Release\MyApplication.exe.manifest", nameof(ManifestsPredictor)),
                new PredictedItem(@"obj\Release\MyApplication.application", nameof(ManifestsPredictor)),
                new PredictedItem(@"bin\Release\MyApplication.application", nameof(ManifestsPredictor)),
            };
            new ManifestsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindItemsForClickOnceWithBaseApplicationManifests()
        {
            ProjectRootElement projectRootElement = CreateProjectRootElement();

            // This is generally set by the toolset for certain output types
            projectRootElement.AddProperty(ManifestsPredictor.GenerateClickOnceManifestsPropertyName, "true");

            projectRootElement.AddItem(ManifestsPredictor.BaseApplicationManifestItemName, @"Properties\app.manifest");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"Properties\app.manifest", nameof(ManifestsPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\Release\MyApplication.exe.manifest", nameof(ManifestsPredictor)),
                new PredictedItem(@"bin\Release\MyApplication.exe.manifest", nameof(ManifestsPredictor)),
                new PredictedItem(@"obj\Release\MyApplication.application", nameof(ManifestsPredictor)),
                new PredictedItem(@"bin\Release\MyApplication.application", nameof(ManifestsPredictor)),
            };
            new ManifestsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindItemsForClickOnceWithNoneItemManifest()
        {
            ProjectRootElement projectRootElement = CreateProjectRootElement();

            // This is generally set by the toolset for certain output types
            projectRootElement.AddProperty(ManifestsPredictor.GenerateClickOnceManifestsPropertyName, "true");

            projectRootElement.AddItem(ManifestsPredictor.NoneItemName, @"Properties\app.manifest");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"Properties\app.manifest", nameof(ManifestsPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\Release\MyApplication.exe.manifest", nameof(ManifestsPredictor)),
                new PredictedItem(@"bin\Release\MyApplication.exe.manifest", nameof(ManifestsPredictor)),
                new PredictedItem(@"obj\Release\MyApplication.application", nameof(ManifestsPredictor)),
                new PredictedItem(@"bin\Release\MyApplication.application", nameof(ManifestsPredictor)),
            };
            new ManifestsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindItemsForClickOnceWithNativeManifest()
        {
            ProjectRootElement projectRootElement = CreateProjectRootElement();

            // This is generally set by the toolset for certain output types
            projectRootElement.AddProperty(ManifestsPredictor.GenerateClickOnceManifestsPropertyName, "true");

            projectRootElement.AddProperty(ManifestsPredictor.OutputTypePropertyName, @"Library");
            projectRootElement.AddItem(ManifestsPredictor.NoneItemName, @"Properties\app.manifest");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"Properties\app.manifest", nameof(ManifestsPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\Release\Native.MyApplication.manifest", nameof(ManifestsPredictor)),
                new PredictedItem(@"bin\Release\Native.MyApplication.manifest", nameof(ManifestsPredictor)),
                new PredictedItem(@"obj\Release\MyApplication.application", nameof(ManifestsPredictor)),
                new PredictedItem(@"bin\Release\MyApplication.application", nameof(ManifestsPredictor)),
            };
            new ManifestsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindItemsForClickOnceWithHostInBrowser()
        {
            ProjectRootElement projectRootElement = CreateProjectRootElement();

            // This is generally set by the toolset for certain output types
            projectRootElement.AddProperty(ManifestsPredictor.GenerateClickOnceManifestsPropertyName, "true");

            projectRootElement.AddProperty(ManifestsPredictor.HostInBrowserPropertyName, "true");
            projectRootElement.AddItem(ManifestsPredictor.NoneItemName, @"Properties\app.manifest");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"Properties\app.manifest", nameof(ManifestsPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\Release\MyApplication.exe.manifest", nameof(ManifestsPredictor)),
                new PredictedItem(@"bin\Release\MyApplication.exe.manifest", nameof(ManifestsPredictor)),
                new PredictedItem(@"obj\Release\MyApplication.xbap", nameof(ManifestsPredictor)),
                new PredictedItem(@"bin\Release\MyApplication.xbap", nameof(ManifestsPredictor)),
            };
            new ManifestsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void FindItemsForClickOnceWithTargetZone()
        {
            ProjectRootElement projectRootElement = CreateProjectRootElement();

            // This is generally set by the toolset for certain output types
            projectRootElement.AddProperty(ManifestsPredictor.GenerateClickOnceManifestsPropertyName, "true");

            projectRootElement.AddProperty(ManifestsPredictor.HostInBrowserPropertyName, "true");
            projectRootElement.AddProperty(ManifestsPredictor.TargetZonePropertyName, "Internet");
            projectRootElement.AddItem(ManifestsPredictor.NoneItemName, @"Properties\app.manifest");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"Properties\app.manifest", nameof(ManifestsPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\Release\MyApplication.exe.manifest", nameof(ManifestsPredictor)),
                new PredictedItem(@"bin\Release\MyApplication.exe.manifest", nameof(ManifestsPredictor)),
                new PredictedItem(@"obj\Release\MyApplication.xbap", nameof(ManifestsPredictor)),
                new PredictedItem(@"bin\Release\MyApplication.xbap", nameof(ManifestsPredictor)),
                new PredictedItem(@"obj\Release\MyApplication.exe.TrustInfo.xml", nameof(ManifestsPredictor)),
            };
            new ManifestsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        private ProjectRootElement CreateProjectRootElement()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddProperty(ManifestsPredictor.AssemblyNamePropertyName, @"MyApplication");
            projectRootElement.AddProperty(ManifestsPredictor.TargetFileNamePropertyName, @"MyApplication.exe");
            projectRootElement.AddProperty(ManifestsPredictor.IntermediateOutputPathPropertyName, @"obj\Release\");
            projectRootElement.AddProperty(ManifestsPredictor.OutDirPropertyName, @"bin\Release\");
            return projectRootElement;
        }
    }
}