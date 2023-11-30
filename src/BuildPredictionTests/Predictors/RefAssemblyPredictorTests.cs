// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class RefAssemblyPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddItem(RefAssemblyPredictor.IntermediateRefAssemblyItemName, @"obj\ref\Foo.dll");
            projectRootElement.AddProperty(RefAssemblyPredictor.TargetRefPathPropertyName, @"bin\ref\Foo.dll");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\ref\Foo.dll", nameof(RefAssemblyPredictor)),
                new PredictedItem(@"bin\ref\Foo.dll", nameof(RefAssemblyPredictor)),
            };

            new RefAssemblyPredictor()
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