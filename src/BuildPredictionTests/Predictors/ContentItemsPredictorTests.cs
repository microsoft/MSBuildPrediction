// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class ContentItemsPredictorTests
    {
        [Fact]
        public void NoCopy()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(ContentItemsPredictor.OutDirPropertyName, @"bin\");

            ProjectItemElement item1 = projectRootElement.AddItem(ContentItemsPredictor.ContentItemName, "Foo.xml");

            ProjectItemElement item2 = projectRootElement.AddItem(ContentItemsPredictor.ContentWithTargetPathItemName, "Bar.xml");
            item2.AddMetadata("TargetPath", @"bin\Bar.xml");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            PredictedItem[] expectedInputFiles =
            [
                new PredictedItem("Foo.xml", nameof(ContentItemsPredictor)),
                new PredictedItem("Bar.xml", nameof(ContentItemsPredictor)),
            ];

            new ContentItemsPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles,
                    null,
                    null,
                    null);
        }

        [Fact]
        public void WithCopy()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(ContentItemsPredictor.OutDirPropertyName, @"bin\");

            ProjectItemElement item1 = projectRootElement.AddItem(ContentItemsPredictor.ContentItemName, "Foo.xml");
            item1.AddMetadata("CopyToOutputDirectory", "PreserveNewest");

            ProjectItemElement item2 = projectRootElement.AddItem(ContentItemsPredictor.ContentWithTargetPathItemName, "Bar.xml");
            item2.AddMetadata("CopyToOutputDirectory", "PreserveNewest");
            item2.AddMetadata("TargetPath", @"Bar\Bar.xml");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            PredictedItem[] expectedInputFiles =
            [
                new PredictedItem("Foo.xml", nameof(ContentItemsPredictor)),
                new PredictedItem("Bar.xml", nameof(ContentItemsPredictor)),
            ];

            PredictedItem[] expectedOutputFiles =
            [
                new PredictedItem(@"bin\Foo.xml", nameof(ContentItemsPredictor)),
                new PredictedItem(@"bin\Bar\Bar.xml", nameof(ContentItemsPredictor)),
            ];

            new ContentItemsPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles,
                    null,
                    expectedOutputFiles,
                    null);
        }
    }
}