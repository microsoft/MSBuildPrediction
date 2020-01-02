// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class XamlAppDefPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddItem(XamlAppDefPredictor.XamlAppDefItemName, "Foo.xaml");
            projectRootElement.AddItem(XamlAppDefPredictor.XamlAppDefItemName, "Bar.xaml");
            projectRootElement.AddItem(XamlAppDefPredictor.XamlAppDefItemName, "Baz.xaml");
            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("Foo.xaml", nameof(XamlAppDefPredictor)),
                new PredictedItem("Bar.xaml", nameof(XamlAppDefPredictor)),
                new PredictedItem("Baz.xaml", nameof(XamlAppDefPredictor)),
            };

            new XamlAppDefPredictor()
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
