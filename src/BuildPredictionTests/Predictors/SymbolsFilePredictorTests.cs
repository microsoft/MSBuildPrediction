// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class SymbolsFilePredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddItem(SymbolsFilePredictor.DebugSymbolsIntermediatePathItemName, @"obj\Foo.pdb");
            projectRootElement.AddItem(SymbolsFilePredictor.DebugSymbolsOutputPathItemName, @"bin\Foo.pdb");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\Foo.pdb", nameof(SymbolsFilePredictor)),
                new PredictedItem(@"bin\Foo.pdb", nameof(SymbolsFilePredictor)),
            };

            new SymbolsFilePredictor()
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
