// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class SymbolsFilePredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddItem(SymbolsFilePredictor.DebugSymbolsIntermediatePathItemName, @"obj\Foo.pdb");
            projectRootElement.AddItem(SymbolsFilePredictor.DebugSymbolsOutputPathItemName, @"bin\Foo.pdb");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\Foo.pdb", nameof(SymbolsFilePredictor)),
                new PredictedItem(@"bin\Foo.pdb", nameof(SymbolsFilePredictor)),
            };

            new SymbolsFilePredictor()
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