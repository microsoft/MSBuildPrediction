// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using System;
    using System.IO;
    using Microsoft.Build.Prediction;
    using Microsoft.Build.Prediction.Predictors;
    using Microsoft.Build.Prediction.Tests;
    using Xunit;

    public class ProjectFileAndImportedFilesTests : TestBase
    {
        private const string ImportTestsDirectoryPath = @"TestsData\Import";
        private const string NestedImportsProjectFileName = "NestedImports.csproj";

        protected override string TestsDirectoryPath => ImportTestsDirectoryPath;

        [Fact]
        public void ProjectFileAndNestedImportedFilesInCsProj()
        {
            BuildInput[] expectedInputs =
            {
                new BuildInput(Path.Combine(Environment.CurrentDirectory, ImportTestsDirectoryPath, NestedImportsProjectFileName), false),
                new BuildInput(Path.Combine(Environment.CurrentDirectory, ImportTestsDirectoryPath, @"Import\NestedTargets.targets"), false),
                new BuildInput(Path.Combine(Environment.CurrentDirectory, ImportTestsDirectoryPath, @"Import\NestedTargets2.targets"), false),
            };

            BuildOutputDirectory[] expectedOutputs = null;

            var predictor = new ProjectFileAndImportedFiles();
            ParseAndVerifyProject(NestedImportsProjectFileName, predictor, expectedInputs, expectedOutputs);
        }
    }
}
