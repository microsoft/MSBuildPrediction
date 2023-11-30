// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class IntermediateOutputPathPredictorTests
    {
        [Fact]
        public void IntermediateOutputPathFoundAsOutputDir()
        {
            const string IntermediateOutputPath = @"C:\repo\bin\x64";
            ProjectInstance projectInstance = CreateTestProjectInstance(IntermediateOutputPath);
            new IntermediateOutputPathPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    null,
                    null,
                    null,
                    new[] { new PredictedItem(IntermediateOutputPath, nameof(IntermediateOutputPathPredictor)) });
        }

        [Fact]
        public void RelativeIntermediateOutputPathFoundAsOutputDir()
        {
            const string IntermediateOutputPath = @"bin\x64";
            ProjectInstance projectInstance = CreateTestProjectInstance(IntermediateOutputPath);
            var predictor = new IntermediateOutputPathPredictor();
            new IntermediateOutputPathPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    null,
                    null,
                    null,
                    new[] { new PredictedItem(IntermediateOutputPath, nameof(IntermediateOutputPathPredictor)) });
        }

        [Fact]
        public void NoOutputsReportedIfNoIntermediateOutputPath()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance(null);
            new IntermediateOutputPathPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        private static ProjectInstance CreateTestProjectInstance(string intermediateOutDir)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            if (intermediateOutDir != null)
            {
                projectRootElement.AddProperty(IntermediateOutputPathPredictor.IntermediateOutputPathMacro, intermediateOutDir);
            }

            return TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);
        }
    }
}