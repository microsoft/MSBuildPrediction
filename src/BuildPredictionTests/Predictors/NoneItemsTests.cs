// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    // TODO: Need to add .NET Core and .NET Framework based examples including use of SDK includes.
    public class NoneItemsTests
    {
        [Fact]
        public void NoneItemsFindItems()
        {
            Project project = CreateTestProject("Foo.xml");
            new NoneItems()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    new[] { new PredictedItem("Foo.xml", nameof(NoneItems)) },
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
                itemGroup.AddItem(NoneItems.NoneItemName, noneItemInclude);
            }

            return TestHelpers.CreateProjectFromRootElement(projectRootElement);
        }
    }
}
