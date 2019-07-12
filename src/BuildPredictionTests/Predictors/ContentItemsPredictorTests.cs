// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class ContentItemsPredictorTests
    {
        [Fact]
        public void ContentItemsFindItems()
        {
            Project project = CreateTestProject("Foo.xml");
            new ContentItemsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    new[] { new PredictedItem("Foo.xml", nameof(ContentItemsPredictor)) },
                    null,
                    null,
                    null);
        }

        private static Project CreateTestProject(params string[] contentItemIncludes)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            foreach (string contentItemInclude in contentItemIncludes)
            {
                itemGroup.AddItem(ContentItemsPredictor.ContentItemName, contentItemInclude);
            }

            return TestHelpers.CreateProjectFromRootElement(projectRootElement);
        }
    }
}
