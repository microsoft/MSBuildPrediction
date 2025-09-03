// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class FakesOutputPathPredictorTests
    {
        [Fact]
        public void FakesOutputPathFoundAsOutputDir()
        {
            const string FakesOutputPath = @"C:\repo\FakesAssemblies";
            ProjectInstance projectInstance = CreateTestProjectInstance(FakesOutputPath);
            new FakesOutputPathPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    null,
                    null,
                    null,
                    new[] { new PredictedItem(FakesOutputPath, nameof(FakesOutputPathPredictor)) });
        }

        [Fact]
        public void RelativeFakesOutputPathFoundAsOutputDir()
        {
            const string FakesOutputPath = @"bin\FakesAssemblies";
            ProjectInstance projectInstance = CreateTestProjectInstance(FakesOutputPath);
            new FakesOutputPathPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    null,
                    null,
                    null,
                    new[] { new PredictedItem(FakesOutputPath, nameof(FakesOutputPathPredictor)) });
        }

        [Fact]
        public void DefaultFakesAssembliesDirectoryFoundAsOutputDir()
        {
            const string FakesOutputPath = @"FakesAssemblies";
            ProjectInstance projectInstance = CreateTestProjectInstance(FakesOutputPath);
            new FakesOutputPathPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    null,
                    null,
                    null,
                    new[] { new PredictedItem(FakesOutputPath, nameof(FakesOutputPathPredictor)) });
        }

        [Fact]
        public void NoOutputsReportedIfNoFakesOutputPath()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance(null);
            new FakesOutputPathPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        [Fact]
        public void NoOutputsReportedIfEmptyFakesOutputPath()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance(string.Empty);
            new FakesOutputPathPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        [Fact]
        public void NoOutputsReportedIfWhitespaceFakesOutputPath()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance("   ");
            new FakesOutputPathPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        private static ProjectInstance CreateTestProjectInstance(string fakesOutputPath)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            if (fakesOutputPath != null)
            {
                projectRootElement.AddProperty(FakesOutputPathPredictor.FakesOutputPathPropertyName, fakesOutputPath);
            }

            return TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);
        }
    }
}