// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class VSCTCompileItemsPredictorTests
    {
        [Fact]
        public void VSCTCompileItemsFindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            itemGroup.AddItem(VSCTCompileItemsPredictor.VSCTCompileItemName, "SomeExtensionPackage.vsct");
            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            new VSCTCompileItemsPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    new[] { new PredictedItem("SomeExtensionPackage.vsct", nameof(VSCTCompileItemsPredictor)) },
                    null,
                    null,
                    null);
        }
    }
}
