// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using System;
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class LinkItemsPredictorTests
    {
        private readonly string _rootDir;

        public LinkItemsPredictorTests()
        {
            // Isolate each test into its own folder
            _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(LinkItemsPredictorTests), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootDir);
        }

        [Fact]
        public void FindsItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.vcxproj"));
            projectRootElement.AddItem(LinkItemsPredictor.LinkItemName, "foo.lib");
            projectRootElement.AddItem(LinkItemsPredictor.LinkItemName, "bar.lib");
            projectRootElement.AddItem(LinkItemsPredictor.LinkItemName, "baz.lib");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("foo.lib", nameof(LinkItemsPredictor)),
                new PredictedItem("bar.lib", nameof(LinkItemsPredictor)),
                new PredictedItem("baz.lib", nameof(LinkItemsPredictor)),
            };
            new LinkItemsPredictor()
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
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddItem(LinkItemsPredictor.LinkItemName, "foo.lib");
            projectRootElement.AddItem(LinkItemsPredictor.LinkItemName, "bar.lib");
            projectRootElement.AddItem(LinkItemsPredictor.LinkItemName, "baz.lib");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            new AdditionalIncludeDirectoriesPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }
    }
}
