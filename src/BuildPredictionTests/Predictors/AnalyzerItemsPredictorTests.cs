// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class AnalyzerItemsPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            itemGroup.AddItem(AnalyzerItemsPredictor.AnalyzerItemName, "Foo.dll");
            itemGroup.AddItem(AnalyzerItemsPredictor.AnalyzerItemName, "Bar.dll");
            itemGroup.AddItem(AnalyzerItemsPredictor.AnalyzerItemName, "Baz.dll");
            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("Foo.dll", nameof(AnalyzerItemsPredictor)),
                new PredictedItem("Bar.dll", nameof(AnalyzerItemsPredictor)),
                new PredictedItem("Baz.dll", nameof(AnalyzerItemsPredictor)),
            };

            new AnalyzerItemsPredictor()
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
