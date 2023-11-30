// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class ClIncludeItemsPredictorTests
    {
        private readonly string _rootDir;

        public ClIncludeItemsPredictorTests()
        {
            // Isolate each test into its own folder
            _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(ClIncludeItemsPredictorTests), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootDir);
        }

        [Fact]
        public void FindsItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.vcxproj"));
            projectRootElement.AddItem(ClIncludeItemsPredictor.ClIncludeItemName, "foo.h");
            projectRootElement.AddItem(ClIncludeItemsPredictor.ClIncludeItemName, "bar.h");
            projectRootElement.AddItem(ClIncludeItemsPredictor.ClIncludeItemName, "doesNotExist.h");
            projectRootElement.AddItem(ClIncludeItemsPredictor.ClIncludeItemName, "baz.h");

            // The files have to exist
            File.WriteAllText(Path.Combine(_rootDir, "foo.h"), "SomeContent");
            File.WriteAllText(Path.Combine(_rootDir, "bar.h"), "SomeContent");
            File.WriteAllText(Path.Combine(_rootDir, "baz.h"), "SomeContent");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("foo.h", nameof(ClIncludeItemsPredictor)),
                new PredictedItem("bar.h", nameof(ClIncludeItemsPredictor)),
                new PredictedItem("baz.h", nameof(ClIncludeItemsPredictor)),
            };
            new ClIncludeItemsPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles,
                    null,
                    null,
                    null);
        }
    }
}