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
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
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
    }
}
