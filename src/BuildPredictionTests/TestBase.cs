// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Build.Definition;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

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
            var globalProperties = new Dictionary<string, string>
            {
                { "Platform", "x64" },
                { "Configuration", "Debug" },
            };
            var projectCollection = new ProjectCollection();

            var projectOptions = new ProjectOptions
            {
                ProjectCollection = projectCollection,
                GlobalProperties = globalProperties,
            };

            // TestsData files are marked to CopyToOutput and are available next to the executing assembly
            var projectInstance = ProjectInstance.FromFile(Path.Combine(TestsDirectoryPath, projFileName), projectOptions);

            predictor
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles,
                    expectedInputDirectories,
                    expectedOutputFiles,
                    expectedOutputDirectories);
        }
    }
}
