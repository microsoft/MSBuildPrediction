// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class CompileItemsPredictorTests
    {
        [Fact]
        public void NoCopy()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(CompileItemsPredictor.OutDirPropertyName, @"bin\");
            projectRootElement.AddItem(CompileItemsPredictor.CompileItemName, "Test.cs");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            new CompileItemsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    new[] { new PredictedItem("Test.cs", nameof(CompileItemsPredictor)) },
                    null,
                    null,
                    null);
        }

        [Fact]
        public void WithCopy()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(CompileItemsPredictor.OutDirPropertyName, @"bin\");

            ProjectItemElement item = projectRootElement.AddItem(CompileItemsPredictor.CompileItemName, "Test.cs");
            item.AddMetadata("CopyToOutputDirectory", "PreserveNewest");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            new CompileItemsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    new[] { new PredictedItem("Test.cs", nameof(CompileItemsPredictor)) },
                    null,
                    new[] { new PredictedItem(@"bin\Test.cs", nameof(CompileItemsPredictor)) },
                    null);
        }
    }
}
