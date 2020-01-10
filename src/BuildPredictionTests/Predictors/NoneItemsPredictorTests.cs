// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class NoneItemsPredictorTests
    {
        [Fact]
        public void NoCopy()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(NoneItemsPredictor.OutDirPropertyName, @"bin\");
            projectRootElement.AddItem(NoneItemsPredictor.NoneItemName, "Foo.xml");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            new NoneItemsPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    new[] { new PredictedItem("Foo.xml", nameof(NoneItemsPredictor)) },
                    null,
                    null,
                    null);
        }

        [Fact]
        public void WithCopy()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(NoneItemsPredictor.OutDirPropertyName, @"bin\");

            ProjectItemElement item = projectRootElement.AddItem(NoneItemsPredictor.NoneItemName, "Foo.xml");
            item.AddMetadata("CopyToOutputDirectory", "PreserveNewest");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            new NoneItemsPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    new[] { new PredictedItem("Foo.xml", nameof(NoneItemsPredictor)) },
                    null,
                    new[] { new PredictedItem(@"bin\Foo.xml", nameof(NoneItemsPredictor)) },
                    null);
        }
    }
}
