// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class NoneItemsPredictorTests
    {
        [Fact]
        public void NoneItemsFindItems()
        {
            Project project = CreateTestProject("Foo.xml");
            new NoneItemsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    new[] { new PredictedItem("Foo.xml", nameof(NoneItemsPredictor)) },
                    null,
                    null,
                    null);
        }

        private static Project CreateTestProject(params string[] noneItemIncludes)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            foreach (string noneItemInclude in noneItemIncludes)
            {
                itemGroup.AddItem(NoneItemsPredictor.NoneItemName, noneItemInclude);
            }

            return TestHelpers.CreateProjectFromRootElement(projectRootElement);
        }
    }
}
