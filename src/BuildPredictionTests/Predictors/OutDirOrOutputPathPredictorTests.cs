// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class OutDirOrOutputPathPredictorTests
    {
        [Fact]
        public void OutDirFoundAsOutputDir()
        {
            const string outDir = @"C:\repo\bin\x64";
            ProjectInstance projectInstance = CreateTestProject(outDir, null);
            new OutDirOrOutputPathPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    null,
                    null,
                    null,
                    new[] { new PredictedItem(outDir, nameof(OutDirOrOutputPathPredictor)) });
        }

        [Fact]
        public void OutputPathUsedAsFallback()
        {
            const string outputPath = @"C:\repo\OutputPath";
            ProjectInstance projectInstance = CreateTestProject(null, outputPath);
            new OutDirOrOutputPathPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    null,
                    null,
                    null,
                    new[] { new PredictedItem(outputPath, nameof(OutDirOrOutputPathPredictor)) });
        }

        [Fact]
        public void NoOutputsReportedIfNoOutDirOrOutputPath()
        {
            ProjectInstance projectInstance = CreateTestProject(null, null);
            new OutDirOrOutputPathPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        private static ProjectInstance CreateTestProject(string outDir, string outputPath)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            if (outDir != null)
            {
                projectRootElement.AddProperty(OutDirOrOutputPathPredictor.OutDirMacro, outDir);
            }

            if (outputPath != null)
            {
                projectRootElement.AddProperty(OutDirOrOutputPathPredictor.OutputPathMacro, outputPath);
            }

            return TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);
        }
    }
}
