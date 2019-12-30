// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class RefAssemblyPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddItem(RefAssemblyPredictor.IntermediateRefAssemblyItemName, @"obj\ref\Foo.dll");
            projectRootElement.AddProperty(RefAssemblyPredictor.TargetRefPathPropertyName, @"bin\ref\Foo.dll");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\ref\Foo.dll", nameof(RefAssemblyPredictor)),
                new PredictedItem(@"bin\ref\Foo.dll", nameof(RefAssemblyPredictor)),
            };

            new RefAssemblyPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    null,
                    null,
                    expectedOutputFiles,
                    null);
        }
    }
}
