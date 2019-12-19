// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class EmbeddedResourceItemsPredictorTests
    {
        [Fact]
        public void NoCopy()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(NoneItemsPredictor.OutDirPropertyName, @"bin\");
            projectRootElement.AddItem(EmbeddedResourceItemsPredictor.EmbeddedResourceItemName, "Resource1.resx");
            projectRootElement.AddItem(EmbeddedResourceItemsPredictor.EmbeddedResourceItemName, "Resource2.resx");
            projectRootElement.AddItem(EmbeddedResourceItemsPredictor.EmbeddedResourceItemName, "Resource3.resx");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("Resource1.resx", nameof(EmbeddedResourceItemsPredictor)),
                new PredictedItem("Resource2.resx", nameof(EmbeddedResourceItemsPredictor)),
                new PredictedItem("Resource3.resx", nameof(EmbeddedResourceItemsPredictor)),
            };
            new EmbeddedResourceItemsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles,
                    null,
                    null,
                    null);
        }

        [Fact]
        public void WithCopy()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(EmbeddedResourceItemsPredictor.OutDirPropertyName, @"bin\");

            ProjectItemElement item1 = projectRootElement.AddItem(EmbeddedResourceItemsPredictor.EmbeddedResourceItemName, "Resource1.resx");
            item1.AddMetadata("CopyToOutputDirectory", "PreserveNewest");

            ProjectItemElement item2 = projectRootElement.AddItem(EmbeddedResourceItemsPredictor.EmbeddedResourceItemName, "Resource2.resx");
            item2.AddMetadata("CopyToOutputDirectory", "PreserveNewest");

            ProjectItemElement item3 = projectRootElement.AddItem(EmbeddedResourceItemsPredictor.EmbeddedResourceItemName, "Resource3.resx");
            item3.AddMetadata("CopyToOutputDirectory", "PreserveNewest");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("Resource1.resx", nameof(EmbeddedResourceItemsPredictor)),
                new PredictedItem("Resource2.resx", nameof(EmbeddedResourceItemsPredictor)),
                new PredictedItem("Resource3.resx", nameof(EmbeddedResourceItemsPredictor)),
            };
            var expectedoutputFiles = new[]
            {
                new PredictedItem(@"bin\Resource1.resx", nameof(EmbeddedResourceItemsPredictor)),
                new PredictedItem(@"bin\Resource2.resx", nameof(EmbeddedResourceItemsPredictor)),
                new PredictedItem(@"bin\Resource3.resx", nameof(EmbeddedResourceItemsPredictor)),
            };
            new EmbeddedResourceItemsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles,
                    null,
                    expectedoutputFiles,
                    null);
        }
    }
}
