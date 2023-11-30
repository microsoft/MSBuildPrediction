// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class CompiledAssemblyPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddItem(CompiledAssemblyPredictor.IntermediateAssemblyItemName, @"obj\Foo.dll");
            projectRootElement.AddProperty(CompiledAssemblyPredictor.OutDirPropertyName, "bin");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\Foo.dll", nameof(CompiledAssemblyPredictor)),
                new PredictedItem(@"bin\Foo.dll", nameof(CompiledAssemblyPredictor)),
            };

            new CompiledAssemblyPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    null,
                    null,
                    expectedOutputFiles,
                    null);
        }
    }
}