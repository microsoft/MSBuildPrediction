// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class CompiledAssemblyPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddItem(CompiledAssemblyPredictor.IntermediateAssemblyItemName, @"obj\Foo.dll");
            projectRootElement.AddProperty(CompiledAssemblyPredictor.OutDirPropertyName, "bin");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\Foo.dll", nameof(CompiledAssemblyPredictor)),
                new PredictedItem(@"bin\Foo.dll", nameof(CompiledAssemblyPredictor)),
            };

            new CompiledAssemblyPredictor()
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
