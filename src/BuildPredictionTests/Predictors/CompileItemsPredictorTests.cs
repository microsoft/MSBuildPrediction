// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class CompileItemsPredictorTests
    {
        [Fact]
        public void NoCopy()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(CompileItemsPredictor.OutDirPropertyName, @"bin\");
            projectRootElement.AddItem(CompileItemsPredictor.CompileItemName, "Test.cs");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            new CompileItemsPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
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

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            new CompileItemsPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    new[] { new PredictedItem("Test.cs", nameof(CompileItemsPredictor)) },
                    null,
                    new[] { new PredictedItem(@"bin\Test.cs", nameof(CompileItemsPredictor)) },
                    null);
        }
    }
}