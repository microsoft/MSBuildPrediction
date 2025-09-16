// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class FakesPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            const string FakesOutputPath = @"bin\FakesAssemblies";
            ProjectInstance projectInstance = CreateTestProjectInstance(FakesOutputPath, ["A.fakes", "B.fakes", "C.fakes"]);

            var expectedInputFiles = new[]
            {
                new PredictedItem("A.fakes", nameof(FakesPredictor)),
                new PredictedItem("B.fakes", nameof(FakesPredictor)),
                new PredictedItem("C.fakes", nameof(FakesPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(Path.Combine(FakesOutputPath, "A.Fakes.dll"), nameof(FakesPredictor)),
                new PredictedItem(Path.Combine(FakesOutputPath, "B.Fakes.dll"), nameof(FakesPredictor)),
                new PredictedItem(Path.Combine(FakesOutputPath, "C.Fakes.dll"), nameof(FakesPredictor)),
            };
            new FakesPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles,
                    null,
                    expectedOutputFiles,
                    null);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void NoOutputsReportedIfInvalidFakesOutputPath(string fakesOutputPath)
        {
            ProjectInstance projectInstance = CreateTestProjectInstance(fakesOutputPath, ["A.fakes", "B.fakes", "C.fakes"]);
            var expectedInputFiles = new[]
            {
                new PredictedItem("A.fakes", nameof(FakesPredictor)),
                new PredictedItem("B.fakes", nameof(FakesPredictor)),
                new PredictedItem("C.fakes", nameof(FakesPredictor)),
            };

            new FakesPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles,
                    null,
                    null,
                    null);
        }

        [Fact]
        public void NoPredictionsReportedNoFakesItems()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance(@"bin\FakesAssemblies", []);
            var expectedInputFiles = new[]
            {
                new PredictedItem("A.fakes", nameof(FakesPredictor)),
                new PredictedItem("B.fakes", nameof(FakesPredictor)),
                new PredictedItem("C.fakes", nameof(FakesPredictor)),
            };

            new FakesPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        private static ProjectInstance CreateTestProjectInstance(
            string fakesOutputPath,
            ReadOnlySpan<string> fakesItems)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(FakesPredictor.FakesImportedPropertyName, "true");

            if (fakesOutputPath != null)
            {
                projectRootElement.AddProperty(FakesPredictor.FakesOutputPathPropertyName, fakesOutputPath);
            }

            foreach (string fakesItem in fakesItems)
            {
                projectRootElement.AddItem(FakesPredictor.FakesItemName, fakesItem);
            }

            return TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);
        }
    }
}