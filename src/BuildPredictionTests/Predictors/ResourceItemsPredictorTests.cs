// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class ResourceItemsPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            itemGroup.AddItem(ResourceItemsPredictor.ResourceItemName, "Foo.png");
            itemGroup.AddItem(ResourceItemsPredictor.ResourceItemName, "Bar.png");
            itemGroup.AddItem(ResourceItemsPredictor.ResourceItemName, "Baz.png");
            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("Foo.png", nameof(ResourceItemsPredictor)),
                new PredictedItem("Bar.png", nameof(ResourceItemsPredictor)),
                new PredictedItem("Baz.png", nameof(ResourceItemsPredictor)),
            };

            new ResourceItemsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles,
                    null,
                    null,
                    null);
        }
    }
}
