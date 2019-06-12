// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Build.Evaluation;

    /// <summary>
    /// Base class that provides helper methods for test code, including
    /// interfacing with sample files in the TestsData folder.
    /// </summary>
    public abstract class TestBase
    {
        /// <summary>
        /// Gets the relative path for resource files used by the test suite.
        /// The path is relative to the BuildPredictionTests output folder.
        /// This is typically something like @"TestsData\Xxx" where Xxx is the
        /// test suite type.
        /// </summary>
        protected abstract string TestsDirectoryPath { get; }

        protected void ParseAndVerifyProject(
            string projFileName,
            IProjectPredictor predictor,
            IReadOnlyCollection<PredictedItem> expectedInputFiles,
            IReadOnlyCollection<PredictedItem> expectedInputDirectories,
            IReadOnlyCollection<PredictedItem> expectedOutputFiles,
            IReadOnlyCollection<PredictedItem> expectedOutputDirectories)
        {
            var projectCollection = new ProjectCollection();
            var project = new Project(
                Path.Combine(TestsDirectoryPath, projFileName),  // TestsData files are marked to CopyToOutput and are available next to the executing assembly
                new Dictionary<string, string>
                {
                    { "Platform", "amd64" },
                    { "Configuration", "debug" },
                },
                null,
                projectCollection);

            predictor
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles,
                    expectedInputDirectories,
                    expectedOutputFiles,
                    expectedOutputDirectories);
        }
    }
}
