// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
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
            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("Foo.xaml", nameof(PageItemsPredictor)),
                new PredictedItem("Bar.xaml", nameof(PageItemsPredictor)),
                new PredictedItem("Baz.xaml", nameof(PageItemsPredictor)),
            };

            new PageItemsPredictor()
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