// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class XamlAppDefPredictorTests
    {
        [Fact]
        public void NoCopy()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddItem(XamlAppDefPredictor.XamlAppDefItemName, "Foo.xaml");
            projectRootElement.AddItem(XamlAppDefPredictor.XamlAppDefItemName, "Bar.xaml");
            projectRootElement.AddItem(XamlAppDefPredictor.XamlAppDefItemName, "Baz.xaml");
            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("Foo.xaml", nameof(XamlAppDefPredictor)),
                new PredictedItem("Bar.xaml", nameof(XamlAppDefPredictor)),
                new PredictedItem("Baz.xaml", nameof(XamlAppDefPredictor)),
            };

            new XamlAppDefPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles,
                    null,
                    null,
                    null);
        }

        [Fact]
        public void WithCopy()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(XamlAppDefPredictor.OutDirPropertyName, @"bin\");
            projectRootElement.AddItem(XamlAppDefPredictor.XamlAppDefItemName, "Foo.xaml")
                .AddMetadata("CopyToOutputDirectory", "PreserveNewest");
            projectRootElement.AddItem(XamlAppDefPredictor.XamlAppDefItemName, "Bar.xaml")
                .AddMetadata("CopyToOutputDirectory", "Always");
            projectRootElement.AddItem(XamlAppDefPredictor.XamlAppDefItemName, "Baz.xaml")
                .AddMetadata("CopyToOutputDirectory", "Never");
            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("Foo.xaml", nameof(XamlAppDefPredictor)),
                new PredictedItem("Bar.xaml", nameof(XamlAppDefPredictor)),
                new PredictedItem("Baz.xaml", nameof(XamlAppDefPredictor)),
            };

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"bin\Foo.xaml", nameof(XamlAppDefPredictor)),
                new PredictedItem(@"bin\Bar.xaml", nameof(XamlAppDefPredictor)),
            };

            new XamlAppDefPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles,
                    null,
                    expectedOutputFiles,
                    null);
        }
    }
}
