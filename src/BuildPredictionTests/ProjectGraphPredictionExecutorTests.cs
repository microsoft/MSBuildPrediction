// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Graph;
    using Xunit;

    public class ProjectGraphPredictionExecutorTests
    {
        private readonly string _rootDir;

        public ProjectGraphPredictionExecutorTests()
        {
            // Isolate each test into its own folder
            _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(ProjectGraphPredictionExecutorTests), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootDir);
        }

        [Fact]
        public void EmptyPredictionsResultInEmptyAggregateResult()
        {
            var graphPredictors = new IProjectGraphPredictor[]
            {
                new MockGraphPredictor(null, null, null, null),
                new MockGraphPredictor(null, null, null, null),
            };

            var predictors = new IProjectPredictor[]
            {
                new MockPredictor(null, null, null, null),
                new MockPredictor(null, null, null, null),
            };

            var executor = new ProjectGraphPredictionExecutor(graphPredictors, predictors);

            ProjectRootElement projectA = CreateProject("a");
            ProjectRootElement projectB = CreateProject("b");
            ProjectRootElement projectC = CreateProject("c");
            ProjectRootElement projectD = CreateProject("d");

            // A depends on B, D; B depends on C, D; C depends on D
            projectA.AddItem("ProjectReference", @"..\b\b.proj");
            projectA.AddItem("ProjectReference", @"..\d\d.proj");
            projectB.AddItem("ProjectReference", @"..\c\c.proj");
            projectB.AddItem("ProjectReference", @"..\d\d.proj");
            projectC.AddItem("ProjectReference", @"..\d\d.proj");

            projectA.Save();
            projectB.Save();
            projectC.Save();
            projectD.Save();

            var projectGraph = new ProjectGraph(projectA.FullPath, new ProjectCollection());
            ProjectGraphPredictions graphPredictions = executor.PredictInputsAndOutputs(projectGraph);

            AssertPredictionsMadeForEveryNode(projectGraph, graphPredictions);
            foreach (ProjectPredictions projectPredictions in graphPredictions.PredictionsPerNode.Values)
            {
                projectPredictions.AssertNoPredictions();
            }
        }

        [Fact]
        public void DistinctInputsAndOutputsAreAggregated()
        {
            var graphPredictors = new IProjectGraphPredictor[]
            {
                new MockGraphPredictor(
                    new[] { @"inputFile1" },
                    new[] { @"inputDirectory1" },
                    new[] { @"outputFile1" },
                    new[] { @"outputDirectory1" }),
                new MockGraphPredictor2(
                    new[] { @"inputFile2" },
                    new[] { @"inputDirectory2" },
                    new[] { @"outputFile2" },
                    new[] { @"outputDirectory2" }),
            };

            var predictors = new IProjectPredictor[]
            {
                new MockPredictor(
                    new[] { @"inputFile3" },
                    new[] { @"inputDirectory3" },
                    new[] { @"outputFile3" },
                    new[] { @"outputDirectory3" }),
                new MockPredictor2(
                    new[] { @"inputFile4" },
                    new[] { @"inputDirectory4" },
                    new[] { @"outputFile4" },
                    new[] { @"outputDirectory4" }),
            };

            var executor = new ProjectGraphPredictionExecutor(graphPredictors, predictors);

            ProjectRootElement projectA = CreateProject("a");
            ProjectRootElement projectB = CreateProject("b");
            ProjectRootElement projectC = CreateProject("c");
            ProjectRootElement projectD = CreateProject("d");

            // A depends on B, D; B depends on C, D; C depends on D
            projectA.AddItem("ProjectReference", @"..\b\b.proj");
            projectA.AddItem("ProjectReference", @"..\d\d.proj");
            projectB.AddItem("ProjectReference", @"..\c\c.proj");
            projectB.AddItem("ProjectReference", @"..\d\d.proj");
            projectC.AddItem("ProjectReference", @"..\d\d.proj");

            projectA.Save();
            projectB.Save();
            projectC.Save();
            projectD.Save();

            var projectGraph = new ProjectGraph(projectA.FullPath, new ProjectCollection());
            ProjectGraphPredictions graphPredictions = executor.PredictInputsAndOutputs(projectGraph);

            AssertPredictionsMadeForEveryNode(projectGraph, graphPredictions);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"a\inputFile3", "MockPredictor"),
                new PredictedItem(@"a\inputFile4", "MockPredictor2"),
                new PredictedItem(@"b\inputFile1", "MockGraphPredictor"),
                new PredictedItem(@"b\inputFile2", "MockGraphPredictor2"),
                new PredictedItem(@"d\inputFile1", "MockGraphPredictor"),
                new PredictedItem(@"d\inputFile2", "MockGraphPredictor2"),
            };
            var expectedInputDirectories = new[]
            {
                new PredictedItem(@"a\inputDirectory3", "MockPredictor"),
                new PredictedItem(@"a\inputDirectory4", "MockPredictor2"),
                new PredictedItem(@"b\inputDirectory1", "MockGraphPredictor"),
                new PredictedItem(@"b\inputDirectory2", "MockGraphPredictor2"),
                new PredictedItem(@"d\inputDirectory1", "MockGraphPredictor"),
                new PredictedItem(@"d\inputDirectory2", "MockGraphPredictor2"),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"a\outputFile3", "MockPredictor"),
                new PredictedItem(@"a\outputFile4", "MockPredictor2"),
                new PredictedItem(@"b\outputFile1", "MockGraphPredictor"),
                new PredictedItem(@"b\outputFile2", "MockGraphPredictor2"),
                new PredictedItem(@"d\outputFile1", "MockGraphPredictor"),
                new PredictedItem(@"d\outputFile2", "MockGraphPredictor2"),
            };
            var expectedOutputDirectories = new[]
            {
                new PredictedItem(@"a\outputDirectory3", "MockPredictor"),
                new PredictedItem(@"a\outputDirectory4", "MockPredictor2"),
                new PredictedItem(@"b\outputDirectory1", "MockGraphPredictor"),
                new PredictedItem(@"b\outputDirectory2", "MockGraphPredictor2"),
                new PredictedItem(@"d\outputDirectory1", "MockGraphPredictor"),
                new PredictedItem(@"d\outputDirectory2", "MockGraphPredictor2"),
            };
            GetPredictionsForProject(graphPredictions, "a").AssertPredictions(
                _rootDir,
                expectedInputFiles,
                expectedInputDirectories,
                expectedOutputFiles,
                expectedOutputDirectories);

            expectedInputFiles = new[]
            {
                new PredictedItem(@"b\inputFile3", "MockPredictor"),
                new PredictedItem(@"b\inputFile4", "MockPredictor2"),
                new PredictedItem(@"c\inputFile1", "MockGraphPredictor"),
                new PredictedItem(@"c\inputFile2", "MockGraphPredictor2"),
                new PredictedItem(@"d\inputFile1", "MockGraphPredictor"),
                new PredictedItem(@"d\inputFile2", "MockGraphPredictor2"),
            };
            expectedInputDirectories = new[]
            {
                new PredictedItem(@"b\inputDirectory3", "MockPredictor"),
                new PredictedItem(@"b\inputDirectory4", "MockPredictor2"),
                new PredictedItem(@"c\inputDirectory1", "MockGraphPredictor"),
                new PredictedItem(@"c\inputDirectory2", "MockGraphPredictor2"),
                new PredictedItem(@"d\inputDirectory1", "MockGraphPredictor"),
                new PredictedItem(@"d\inputDirectory2", "MockGraphPredictor2"),
            };
            expectedOutputFiles = new[]
            {
                new PredictedItem(@"b\outputFile3", "MockPredictor"),
                new PredictedItem(@"b\outputFile4", "MockPredictor2"),
                new PredictedItem(@"c\outputFile1", "MockGraphPredictor"),
                new PredictedItem(@"c\outputFile2", "MockGraphPredictor2"),
                new PredictedItem(@"d\outputFile1", "MockGraphPredictor"),
                new PredictedItem(@"d\outputFile2", "MockGraphPredictor2"),
            };
            expectedOutputDirectories = new[]
            {
                new PredictedItem(@"b\outputDirectory3", "MockPredictor"),
                new PredictedItem(@"b\outputDirectory4", "MockPredictor2"),
                new PredictedItem(@"c\outputDirectory1", "MockGraphPredictor"),
                new PredictedItem(@"c\outputDirectory2", "MockGraphPredictor2"),
                new PredictedItem(@"d\outputDirectory1", "MockGraphPredictor"),
                new PredictedItem(@"d\outputDirectory2", "MockGraphPredictor2"),
            };
            GetPredictionsForProject(graphPredictions, "b").AssertPredictions(
                _rootDir,
                expectedInputFiles,
                expectedInputDirectories,
                expectedOutputFiles,
                expectedOutputDirectories);

            expectedInputFiles = new[]
            {
                new PredictedItem(@"c\inputFile3", "MockPredictor"),
                new PredictedItem(@"c\inputFile4", "MockPredictor2"),
                new PredictedItem(@"d\inputFile1", "MockGraphPredictor"),
                new PredictedItem(@"d\inputFile2", "MockGraphPredictor2"),
            };
            expectedInputDirectories = new[]
            {
                new PredictedItem(@"c\inputDirectory3", "MockPredictor"),
                new PredictedItem(@"c\inputDirectory4", "MockPredictor2"),
                new PredictedItem(@"d\inputDirectory1", "MockGraphPredictor"),
                new PredictedItem(@"d\inputDirectory2", "MockGraphPredictor2"),
            };
            expectedOutputFiles = new[]
            {
                new PredictedItem(@"c\outputFile3", "MockPredictor"),
                new PredictedItem(@"c\outputFile4", "MockPredictor2"),
                new PredictedItem(@"d\outputFile1", "MockGraphPredictor"),
                new PredictedItem(@"d\outputFile2", "MockGraphPredictor2"),
            };
            expectedOutputDirectories = new[]
            {
                new PredictedItem(@"c\outputDirectory3", "MockPredictor"),
                new PredictedItem(@"c\outputDirectory4", "MockPredictor2"),
                new PredictedItem(@"d\outputDirectory1", "MockGraphPredictor"),
                new PredictedItem(@"d\outputDirectory2", "MockGraphPredictor2"),
            };
            GetPredictionsForProject(graphPredictions, "c").AssertPredictions(
                _rootDir,
                expectedInputFiles,
                expectedInputDirectories,
                expectedOutputFiles,
                expectedOutputDirectories);

            expectedInputFiles = new[]
            {
                new PredictedItem(@"d\inputFile3", "MockPredictor"),
                new PredictedItem(@"d\inputFile4", "MockPredictor2"),
            };
            expectedInputDirectories = new[]
            {
                new PredictedItem(@"d\inputDirectory3", "MockPredictor"),
                new PredictedItem(@"d\inputDirectory4", "MockPredictor2"),
            };
            expectedOutputFiles = new[]
            {
                new PredictedItem(@"d\outputFile3", "MockPredictor"),
                new PredictedItem(@"d\outputFile4", "MockPredictor2"),
            };
            expectedOutputDirectories = new[]
            {
                new PredictedItem(@"d\outputDirectory3", "MockPredictor"),
                new PredictedItem(@"d\outputDirectory4", "MockPredictor2"),
            };
            GetPredictionsForProject(graphPredictions, "d").AssertPredictions(
                _rootDir,
                expectedInputFiles,
                expectedInputDirectories,
                expectedOutputFiles,
                expectedOutputDirectories);
        }

        [Fact]
        public void DuplicateInputsAndOutputsMergePredictedBys()
        {
            var graphPredictors = new IProjectGraphPredictor[]
            {
                new MockGraphPredictor(
                    new[] { @"..\common\inputFile" },
                    new[] { @"..\common\inputDirectory" },
                    new[] { @"..\common\outputFile" },
                    new[] { @"..\common\outputDirectory" }),
                new MockGraphPredictor2(
                    new[] { @"..\common\inputFile" },
                    new[] { @"..\common\inputDirectory" },
                    new[] { @"..\common\outputFile" },
                    new[] { @"..\common\outputDirectory" }),
            };

            var predictors = new IProjectPredictor[]
            {
                new MockPredictor(
                    new[] { @"..\common\inputFile" },
                    new[] { @"..\common\inputDirectory" },
                    new[] { @"..\common\outputFile" },
                    new[] { @"..\common\outputDirectory" }),
                new MockPredictor2(
                    new[] { @"..\common\inputFile" },
                    new[] { @"..\common\inputDirectory" },
                    new[] { @"..\common\outputFile" },
                    new[] { @"..\common\outputDirectory" }),
            };

            var executor = new ProjectGraphPredictionExecutor(graphPredictors, predictors);

            ProjectRootElement projectA = CreateProject("a");
            ProjectRootElement projectB = CreateProject("b");

            // A depends on B
            projectA.AddItem("ProjectReference", @"..\b\b.proj");

            projectA.Save();
            projectB.Save();

            var projectGraph = new ProjectGraph(projectA.FullPath, new ProjectCollection());
            ProjectGraphPredictions graphPredictions = executor.PredictInputsAndOutputs(projectGraph);

            AssertPredictionsMadeForEveryNode(projectGraph, graphPredictions);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"common\inputFile", "MockGraphPredictor", "MockGraphPredictor2", "MockPredictor", "MockPredictor2"),
            };
            var expectedInputDirectories = new[]
            {
                new PredictedItem(@"common\inputDirectory", "MockGraphPredictor", "MockGraphPredictor2", "MockPredictor", "MockPredictor2"),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"common\outputFile", "MockGraphPredictor", "MockGraphPredictor2", "MockPredictor", "MockPredictor2"),
            };
            var expectedOutputDirectories = new[]
            {
                new PredictedItem(@"common\outputDirectory", "MockGraphPredictor", "MockGraphPredictor2", "MockPredictor", "MockPredictor2"),
            };

            GetPredictionsForProject(graphPredictions, "a").AssertPredictions(
                _rootDir,
                expectedInputFiles,
                expectedInputDirectories,
                expectedOutputFiles,
                expectedOutputDirectories);
        }

        private ProjectRootElement CreateProject(string projectName)
        {
            string projectPath = Path.Combine(_rootDir, projectName, projectName + ".proj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectPath);

            // The caller may modify the returned project, so don't save it yet.
            return projectRootElement;
        }

        private ProjectPredictions GetPredictionsForProject(ProjectGraphPredictions graphPredictions, string projectName)
        {
            string expectedFullPath = Path.Combine(_rootDir, projectName, projectName + ".proj");
            foreach (KeyValuePair<ProjectGraphNode, ProjectPredictions> pair in graphPredictions.PredictionsPerNode)
            {
                if (pair.Key.ProjectInstance.FullPath.Equals(expectedFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    return pair.Value;
                }
            }

            throw new InvalidOperationException($"Could not find predictions for project {projectName}");
        }

        private void AssertPredictionsMadeForEveryNode(ProjectGraph projectGraph, ProjectGraphPredictions graphPredictions)
        {
            Assert.Equal(projectGraph.ProjectNodes.Count, graphPredictions.PredictionsPerNode.Count);
            foreach (ProjectGraphNode node in projectGraph.ProjectNodes)
            {
                graphPredictions.PredictionsPerNode.ContainsKey(node);
            }
        }

        private class MockGraphPredictor : IProjectGraphPredictor
        {
            private readonly IEnumerable<string> _inputFiles;
            private readonly IEnumerable<string> _inputDirectories;
            private readonly IEnumerable<string> _outputFiles;
            private readonly IEnumerable<string> _outputDirectories;

            public MockGraphPredictor(
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

            public void PredictInputsAndOutputs(ProjectGraphNode projectGraphNode, ProjectPredictionReporter predictionReporter)
            {
                foreach (ProjectGraphNode dependency in projectGraphNode.ProjectReferences)
                {
                    var dependencyDir = dependency.ProjectInstance.Directory;

                    foreach (string item in _inputFiles)
                    {
                        predictionReporter.ReportInputFile(Path.GetFullPath(Path.Combine(dependencyDir, item)));
                    }

                    foreach (string item in _inputDirectories)
                    {
                        predictionReporter.ReportInputDirectory(Path.GetFullPath(Path.Combine(dependencyDir, item)));
                    }

                    foreach (string item in _outputFiles)
                    {
                        predictionReporter.ReportOutputFile(Path.GetFullPath(Path.Combine(dependencyDir, item)));
                    }

                    foreach (string item in _outputDirectories)
                    {
                        predictionReporter.ReportOutputDirectory(Path.GetFullPath(Path.Combine(dependencyDir, item)));
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
        private class MockGraphPredictor2 : MockGraphPredictor
        {
            public MockGraphPredictor2(
                IEnumerable<string> inputFiles,
                IEnumerable<string> inputDirectories,
                IEnumerable<string> outputFiles,
                IEnumerable<string> outputDirectories)
                : base(inputFiles, inputDirectories, outputFiles, outputDirectories)
            {
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
