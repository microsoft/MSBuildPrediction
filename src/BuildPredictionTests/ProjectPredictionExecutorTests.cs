// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Xunit;

namespace Microsoft.Build.Prediction.Tests
{
    public class ProjectPredictionExecutorTests
    {
        [Fact]
        public void EmptyPredictionsResultInEmptyAggregateResult()
        {
            var predictors = new IProjectPredictor[]
            {
                new MockPredictor(null, null, null, null),
                new MockPredictor(null, null, null, null),
            };

            var executor = new ProjectPredictionExecutor(predictors);

            var project = TestHelpers.CreateProjectInstanceFromRootElement(ProjectRootElement.Create());
            ProjectPredictions predictions = executor.PredictInputsAndOutputs(project);
            Assert.NotNull(predictions);
            Assert.Empty(predictions.InputFiles);
            Assert.Empty(predictions.InputDirectories);
            Assert.Empty(predictions.OutputFiles);
            Assert.Empty(predictions.OutputDirectories);
        }

        [Fact]
        public void DistinctInputsAndOutputsAreAggregated()
        {
            /*
            Missed value in the input files:
            PredictedItem: 1\inputFile; PredictedBy=MockPredictor
            from among actual list
            PredictedItem: E:\MSBuildPrediction\src\BuildPredictionTests\bin\Debug\net472\2\inputFile; PredictedBy=MockPredictor2:: PredictedItem: E:\MSBuildPrediction\src\BuildPredictionTests\bin\Debug\net472\1\inputFile; PredictedBy=MockPredictor
            */
            var predictors = new IProjectPredictor[]
            {
                new MockPredictor(
                    new[] { @"1\inputFile" },
                    new[] { @"1\inputDirectory" },
                    new[] { @"1\outputFile" },
                    new[] { @"1\outputDirectory" }),
                new MockPredictor2(
                    new[] { @"2\inputFile" },
                    new[] { @"2\inputDirectory" },
                    new[] { @"2\outputFile" },
                    new[] { @"2\outputDirectory" }),
            };

            var executor = new ProjectPredictionExecutor(predictors);

            var project = TestHelpers.CreateProjectInstanceFromRootElement(ProjectRootElement.Create());

            ProjectPredictions predictions = executor.PredictInputsAndOutputs(project);

            predictions.AssertPredictions(
                project,
                new[] { new PredictedItem(@"1\inputFile", "MockPredictor"), new PredictedItem(@"2\inputFile", "MockPredictor2"), },
                new[] { new PredictedItem(@"1\inputDirectory", "MockPredictor"), new PredictedItem(@"2\inputDirectory", "MockPredictor2"), },
                new[] { new PredictedItem(@"1\outputFile", "MockPredictor"), new PredictedItem(@"2\outputFile", "MockPredictor2"), },
                new[] { new PredictedItem(@"1\outputDirectory", "MockPredictor"), new PredictedItem(@"2\outputDirectory", "MockPredictor2"), });
        }

        [Fact]
        public void DuplicateInputsAndOutputsMergePredictedBys()
        {
            var predictors = new IProjectPredictor[]
            {
                new MockPredictor(
                    new[] { @"common\inputFile" },
                    new[] { @"common\inputDirectory" },
                    new[] { @"common\outputFile" },
                    new[] { @"common\outputDirectory" }),
                new MockPredictor2(
                    new[] { @"common\inputFile" },
                    new[] { @"common\inputDirectory" },
                    new[] { @"common\outputFile" },
                    new[] { @"common\outputDirectory" }),
            };

            var executor = new ProjectPredictionExecutor(predictors);

            var project = TestHelpers.CreateProjectInstanceFromRootElement(ProjectRootElement.Create());

            ProjectPredictions predictions = executor.PredictInputsAndOutputs(project);

            predictions.AssertPredictions(
                project,
                new[] { new PredictedItem(@"common\inputFile", "MockPredictor", "MockPredictor2"), },
                new[] { new PredictedItem(@"common\inputDirectory", "MockPredictor", "MockPredictor2"), },
                new[] { new PredictedItem(@"common\outputFile", "MockPredictor", "MockPredictor2"), },
                new[] { new PredictedItem(@"common\outputDirectory", "MockPredictor", "MockPredictor2"), });
        }

        [Fact]
        public void PredictorSparsenessStressTest()
        {
            int[] numPredictorCases = { 40 };  // Set to 1000 or more for reduced average noise when using tickResults for measurements.
            int[] sparsenessPercentages = { 0, 25, 50, 75, 100 };
            var tickResults = new long[numPredictorCases.Length][];

            var proj = TestHelpers.CreateProjectInstanceFromRootElement(ProjectRootElement.Create());

            // Run through twice and keep the second round only - first round affected by JIT overhead.
            for (int iter = 0; iter < 2; iter++)
            {
                for (int p = 0; p < numPredictorCases.Length; p++)
                {
                    int numPredictors = numPredictorCases[p];
                    tickResults[p] = new long[sparsenessPercentages.Length];
                    var predictors = new IProjectPredictor[numPredictors];
                    int sparseIndex = 0;

                    for (int s = 0; s < sparsenessPercentages.Length; s++)
                    {
                        int sparsenessPercentage = sparsenessPercentages[s];
                        for (int i = 0; i < numPredictors; i++)
                        {
                            if (sparseIndex < sparsenessPercentage)
                            {
                                predictors[i] = new MockPredictor(null, null, null, null);
                            }
                            else
                            {
                                predictors[i] = new MockPredictor(
                                    new[] { $@"{i}\inputFile" },
                                    new[] { $@"{i}\inputDirectory" },
                                    new[] { $@"{i}\outputFile" },
                                    new[] { $@"{i}\outputDirectory" });
                            }

                            sparseIndex++;
                            if (sparseIndex > sparsenessPercentage)
                            {
                                sparseIndex = 0;
                            }
                        }

                        var executor = new ProjectPredictionExecutor(predictors);
                        Stopwatch sw = Stopwatch.StartNew();
                        executor.PredictInputsAndOutputs(proj);
                        sw.Stop();
                        tickResults[p][s] = sw.ElapsedTicks;
                        Console.WriteLine($"{numPredictors} @ {sparsenessPercentage}%: {sw.ElapsedTicks} ticks");
                    }
                }
            }
        }

        private class MockPredictor : IProjectPredictor
        {
            private readonly IEnumerable<string> _inputFiles;
            private readonly IEnumerable<string> _inputDirectories;
            private readonly IEnumerable<string> _outputFiles;
            private readonly IEnumerable<string> _outputDirectories;

            public MockPredictor(
                IEnumerable<string> inputFiles,
                IEnumerable<string> inputDirectories,
                IEnumerable<string> outputFiles,
                IEnumerable<string> outputDirectories)
            {
                _inputFiles = inputFiles ?? Array.Empty<string>();
                _inputDirectories = inputDirectories ?? Array.Empty<string>();
                _outputFiles = outputFiles ?? Array.Empty<string>();
                _outputDirectories = outputDirectories ?? Array.Empty<string>();
            }

            public void PredictInputsAndOutputs(
                ProjectInstance projectInstance,
                ProjectPredictionReporter predictionReporter)
            {
                foreach (var item in _inputFiles)
                {
                    predictionReporter.ReportInputFile(item);
                }

                foreach (var item in _inputDirectories)
                {
                    predictionReporter.ReportInputDirectory(item);
                }

                foreach (var item in _outputFiles)
                {
                    predictionReporter.ReportOutputFile(item);
                }

                foreach (var item in _outputDirectories)
                {
                    predictionReporter.ReportOutputDirectory(item);
                }
            }
        }

        // Second class name to get different results from PredictedBy values.
        private class MockPredictor2 : MockPredictor
        {
            public MockPredictor2(
                IEnumerable<string> inputFiles,
                IEnumerable<string> inputDirectories,
                IEnumerable<string> outputFiles,
                IEnumerable<string> outputDirectories)
                : base(inputFiles, inputDirectories, outputFiles, outputDirectories)
            {
            }
        }
    }
}