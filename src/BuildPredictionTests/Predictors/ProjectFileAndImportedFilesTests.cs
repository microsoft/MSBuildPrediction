// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
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
            PredictedItem[] expectedFileInputs =
            {
                new PredictedItem(Path.Combine(NestedImportsProjectFileName), nameof(ProjectFileAndImportedFiles)),
                new PredictedItem(Path.Combine(@"Import\NestedTargets.targets"), nameof(ProjectFileAndImportedFiles)),
                new PredictedItem(Path.Combine(@"Import\NestedTargets2.targets"), nameof(ProjectFileAndImportedFiles)),
            };

            var predictor = new ProjectFileAndImportedFiles();
            ParseAndVerifyProject(
                NestedImportsProjectFileName,
                predictor,
                expectedFileInputs,
                null,
                null,
                null);
        }
    }
}
