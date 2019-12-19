// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Definition;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;
    using Xunit;

    internal static class TestHelpers
    {
        public static void AssertNoPredictions(this ProjectPredictions predictions) => predictions.AssertPredictions(null, null, null, null);

        public static void AssertPredictions(
            this ProjectPredictions predictions,
            Project project,
            IReadOnlyCollection<PredictedItem> expectedInputFiles,
            IReadOnlyCollection<PredictedItem> expectedInputDirectories,
            IReadOnlyCollection<PredictedItem> expectedOutputFiles,
            IReadOnlyCollection<PredictedItem> expectedOutputDirectories)
            => AssertPredictions(
                predictions,
                expectedInputFiles.MakeAbsolute(project.DirectoryPath),
                expectedInputDirectories.MakeAbsolute(project.DirectoryPath),
                expectedOutputFiles.MakeAbsolute(project.DirectoryPath),
                expectedOutputDirectories.MakeAbsolute(project.DirectoryPath));

        public static Project ProjectFromXml(string xml)
        {
            var settings = new XmlReaderSettings
            {
                XmlResolver = null,  // Prevent external calls for namespaces.
            };

            using (var stringReader = new StringReader(xml))
            using (var xmlReader = XmlReader.Create(stringReader, settings))
            {
                ProjectRootElement projectRootElement = ProjectRootElement.Create(xmlReader);
                return Project.FromProjectRootElement(projectRootElement, new ProjectOptions());
            }
        }

        public static Project CreateProjectFromRootElement(ProjectRootElement projectRootElement)
        {
            var globalProperties = new Dictionary<string, string>
            {
                { "Platform", "amd64" },
                { "Configuration", "debug" },
            };

            // Use a new project collection to avoid collisions in the global one.
            // TODO! Refactor tests to be more isolated, both ProjectCollection-wide and disk-wise
            var projectCollection = new ProjectCollection();

            return new Project(projectRootElement, globalProperties, toolsVersion: ProjectCollection.GlobalProjectCollection.DefaultToolsVersion, projectCollection);
        }

        public static ProjectInstance CreateProjectInstanceFromRootElement(ProjectRootElement projectRootElement)
        {
            var globalProperties = new Dictionary<string, string>
            {
                { "Platform", "amd64" },
                { "Configuration", "debug" },
            };

            // Use a new project collection to avoid collisions in the global one.
            // TODO! Refactor tests to be more isolated, both ProjectCollection-wide and disk-wise
            var projectCollection = new ProjectCollection();

            return new ProjectInstance(projectRootElement, globalProperties, toolsVersion: ProjectCollection.GlobalProjectCollection.DefaultToolsVersion, projectCollection);
        }

        public static ProjectPredictions GetProjectPredictions(this IProjectPredictor predictor, Project project)
        {
            ProjectInstance projectInstance = project.CreateProjectInstance(ProjectInstanceSettings.ImmutableWithFastItemLookup);
            var projectPredictionCollector = new DefaultProjectPredictionCollector();
            var predictionReporter = new ProjectPredictionReporter(
                projectPredictionCollector,
                projectInstance.Directory,
                predictor.GetType().Name);
            predictor.PredictInputsAndOutputs(project, projectInstance, predictionReporter);
            return projectPredictionCollector.Predictions;
        }

        public static IReadOnlyCollection<PredictedItem> MakeAbsolute(this IReadOnlyCollection<PredictedItem> items, string baseDir)
            => items?.Select(item => new PredictedItem(Path.Combine(baseDir, item.Path), item.PredictedBy.ToArray())).ToList();

        private static void AssertPredictions(
            this ProjectPredictions predictions,
            IReadOnlyCollection<PredictedItem> expectedInputFiles,
            IReadOnlyCollection<PredictedItem> expectedInputDirectories,
            IReadOnlyCollection<PredictedItem> expectedOutputFiles,
            IReadOnlyCollection<PredictedItem> expectedOutputDirectories)
        {
            Assert.NotNull(predictions);

            if (expectedInputFiles == null)
            {
                Assert.Equal(0, predictions.InputFiles.Count);
            }
            else
            {
                CheckCollection(expectedInputFiles, predictions.InputFiles, PredictedItemComparer.Instance, "input files");
            }

            if (expectedInputDirectories == null)
            {
                Assert.Equal(0, predictions.InputDirectories.Count);
            }
            else
            {
                CheckCollection(expectedInputDirectories, predictions.InputDirectories, PredictedItemComparer.Instance, "input directories");
            }

            if (expectedOutputFiles == null)
            {
                Assert.Equal(0, predictions.OutputFiles.Count);
            }
            else
            {
                CheckCollection(expectedOutputFiles, predictions.OutputFiles, PredictedItemComparer.Instance, "output files");
            }

            if (expectedOutputDirectories == null)
            {
                Assert.Equal(0, predictions.OutputDirectories.Count);
            }
            else
            {
                CheckCollection(expectedOutputDirectories, predictions.OutputDirectories, PredictedItemComparer.Instance, "output directories");
            }
        }

        private static void CheckCollection<T>(IReadOnlyCollection<T> expected, IReadOnlyCollection<T> actual, IEqualityComparer<T> comparer, string type)
        {
            var actualSet = new HashSet<T>(actual, comparer);
            var expectedSet = new HashSet<T>(expected, comparer);

            List<T> expectedNotInActual = expected.Where(i => !actualSet.Contains(i)).ToList();
            List<T> actualNotExpected = actual.Where(i => !expectedSet.Contains(i)).ToList();
            if (expectedSet.Count != actualSet.Count)
            {
                throw new ArgumentException(
                    $"Mismatched count - expected {expectedSet.Count} but got {actualSet.Count}.{Environment.NewLine}" +
                    $"Expected {type}{PrettifyCollection(expected)}{Environment.NewLine}" +
                    $"Actual {PrettifyCollection(actual)}{Environment.NewLine}" +
                    $"Extra expected {PrettifyCollection(expectedSet.Except(actualSet, comparer).ToList())}{Environment.NewLine}" +
                    $"Extra actual {PrettifyCollection(actualSet.Except(expectedSet, comparer).ToList())}");
            }

            foreach (T expectedItem in expectedSet)
            {
                Assert.True(
                    actualSet.Contains(expectedItem),
                    $"Missed value in the {type}: {expectedItem} from among actual list {PrettifyCollection(actual)}");
            }
        }

        private static string PrettifyCollection<T>(IReadOnlyCollection<T> items)
        {
            if (items.Count == 0)
            {
                return "[]";
            }

            var builder = new StringBuilder();
            builder.Append("[");
            builder.Append(Environment.NewLine);
            foreach (var item in items)
            {
                builder.Append("\t");
                builder.Append(item);
                builder.Append(",");
                builder.Append(Environment.NewLine);
            }

            builder.Append("]");
            builder.Append(Environment.NewLine);

            return builder.ToString();
        }
    }
}
