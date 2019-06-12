// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class IntermediateOutputPathIsOutputDirTests
    {
        [Fact]
        public void IntermediateOutputPathFoundAsOutputDir()
        {
            const string IntermediateOutputPath = @"C:\repo\bin\x64";
            Project project = CreateTestProject(IntermediateOutputPath);
            new IntermediateOutputPathIsOutputDir()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    null,
                    null,
                    null,
                    new[] { new PredictedItem(IntermediateOutputPath, nameof(IntermediateOutputPathIsOutputDir)) });
        }

        [Fact]
        public void RelativeIntermediateOutputPathFoundAsOutputDir()
        {
            const string IntermediateOutputPath = @"bin\x64";
            Project project = CreateTestProject(IntermediateOutputPath);
            var predictor = new IntermediateOutputPathIsOutputDir();
            new IntermediateOutputPathIsOutputDir()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    null,
                    null,
                    null,
                    new[] { new PredictedItem(IntermediateOutputPath, nameof(IntermediateOutputPathIsOutputDir)) });
        }

        [Fact]
        public void NoOutputsReportedIfNoIntermediateOutputPath()
        {
            Project project = CreateTestProject(null);
            new IntermediateOutputPathIsOutputDir()
                .GetProjectPredictions(project)
                .AssertNoPredictions();
        }

        private static Project CreateTestProject(string intermediateOutDir)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            if (intermediateOutDir != null)
            {
                projectRootElement.AddProperty(IntermediateOutputPathIsOutputDir.IntermediateOutputPathMacro, intermediateOutDir);
            }

            return TestHelpers.CreateProjectFromRootElement(projectRootElement);
        }
    }
}
