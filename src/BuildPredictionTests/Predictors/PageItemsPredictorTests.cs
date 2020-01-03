// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class PageItemsPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            itemGroup.AddItem(PageItemsPredictor.PageItemName, "Foo.xaml");
            itemGroup.AddItem(PageItemsPredictor.PageItemName, "Bar.xaml");
            itemGroup.AddItem(PageItemsPredictor.PageItemName, "Baz.xaml");
            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("Foo.xaml", nameof(PageItemsPredictor)),
                new PredictedItem("Bar.xaml", nameof(PageItemsPredictor)),
                new PredictedItem("Baz.xaml", nameof(PageItemsPredictor)),
            };

            new PageItemsPredictor()
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
