// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests
{
    using System;
    using System.Diagnostics;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;
    using Xunit;

    public class ProjectPredictionExecutorTests
    {
        [Fact]
        public void EmptyPredictionsResultInEmptyAggregateResult()
        {
            var predictors = new IProjectPredictor[]
            {
                new MockPredictor(new ProjectPredictions(null, null)),
                new MockPredictor(new ProjectPredictions(null, null)),
            };

            var executor = new ProjectPredictionExecutor(predictors);

            var project = TestHelpers.CreateProjectFromRootElement(ProjectRootElement.Create());
            ProjectPredictions predictions = executor.PredictInputsAndOutputs(project);
            Assert.NotNull(predictions);
            Assert.Equal(0, predictions.BuildInputs.Count);
            Assert.Equal(0, predictions.BuildOutputDirectories.Count);
        }

        [Fact]
        public void DistinctInputsAndOutputsAreAggregated()
        {
            var predictors = new IProjectPredictor[]
            {
                new MockPredictor(new ProjectPredictions(
                    new[] { new BuildInput(@"foo\bar1", false) },
                    new[] { new BuildOutputDirectory(@"blah\boo1") })),
                new MockPredictor2(new ProjectPredictions(
                    new[] { new BuildInput(@"foo\bar2", false) },
                    new[] { new BuildOutputDirectory(@"blah\boo2") })),
            };

            var executor = new ProjectPredictionExecutor(predictors);

            var project = TestHelpers.CreateProjectFromRootElement(ProjectRootElement.Create());

            ProjectPredictions predictions = executor.PredictInputsAndOutputs(project);

            BuildInput[] expectedInputs =
            {
                new BuildInput(@"foo\bar1", false, "MockPredictor"),
                new BuildInput(@"foo\bar2", false, "MockPredictor2"),
            };

            BuildOutputDirectory[] expectedBuildOutputDirectories =
            {
                new BuildOutputDirectory(@"blah\boo1", "MockPredictor"),
                new BuildOutputDirectory(@"blah\boo2", "MockPredictor2"),
            };

            predictions.AssertPredictions(expectedInputs, expectedBuildOutputDirectories);
        }

        [Fact]
        public void DuplicateInputsAndOutputsMergePredictedBys()
        {
            var predictors = new IProjectPredictor[]
            {
                new MockPredictor(new ProjectPredictions(
                    new[] { new BuildInput(@"foo\bar", false) },
                    new[] { new BuildOutputDirectory(@"blah\boo") })),
                new MockPredictor2(new ProjectPredictions(
                    new[] { new BuildInput(@"foo\bar", false) },
                    new[] { new BuildOutputDirectory(@"blah\boo") })),
            };

            var executor = new ProjectPredictionExecutor(predictors);

            var project = TestHelpers.CreateProjectFromRootElement(ProjectRootElement.Create());

            ProjectPredictions predictions = executor.PredictInputsAndOutputs(project);

            predictions.AssertPredictions(
                new[] { new BuildInput(@"foo\bar", false, "MockPredictor", "MockPredictor2") },
                new[] { new BuildOutputDirectory(@"blah\boo", "MockPredictor", "MockPredictor2") });
        }

        [Fact]
        public void PredictorSparsenessStressTest()
        {
            int[] numPredictorCases = { 40 };  // Set to 1000 or more for reduced average noise when using tickResults for measurements.
            int[] sparsenessPercentages = { 0, 25, 50, 75, 100 };
            var tickResults = new long[numPredictorCases.Length][];

            var proj = TestHelpers.CreateProjectFromRootElement(ProjectRootElement.Create());

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
                                predictors[i] = new MockPredictor(null);
                            }
                            else
                            {
                                predictors[i] = new MockPredictor(new ProjectPredictions(null, null));
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
            private readonly ProjectPredictions _predictionsToReturn;

            public MockPredictor(ProjectPredictions predictionsToReturn)
            {
                _predictionsToReturn = predictionsToReturn;
            }

            public bool TryPredictInputsAndOutputs(
                Project project,
                ProjectInstance projectInstance,
                out ProjectPredictions predictions)
            {
                if (_predictionsToReturn == null)
                {
                    predictions = null;
                    return false;
                }

                predictions = _predictionsToReturn;
                return true;
            }
        }

        // Second class name to get different results from PredictedBy values.
        private class MockPredictor2 : MockPredictor
        {
            public MockPredictor2(ProjectPredictions predictionsToReturn)
                : base(predictionsToReturn)
            {
            }
        }
    }
}
