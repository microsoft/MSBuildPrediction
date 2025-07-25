// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class ProjectFileAndImportsPredictorTests : TestBase
    {
        private const string ImportTestsDirectoryPath = @"TestsData\Import";
        private const string NestedImportsProjectFileName = "NestedImports.csproj";

        protected override string TestsDirectoryPath => ImportTestsDirectoryPath;

        [Fact]
        public void ProjectFileAndNestedImportedFilesInCsProj()
        {
            PredictedItem[] expectedFileInputs =
            {
                new PredictedItem(Path.Combine(NestedImportsProjectFileName), nameof(ProjectFileAndImportsPredictor)),
                new PredictedItem(Path.Combine(@"Import\NestedTargets.targets"), nameof(ProjectFileAndImportsPredictor)),
                new PredictedItem(Path.Combine(@"Import\NestedTargets2.targets"), nameof(ProjectFileAndImportsPredictor)),
            };

            var predictor = new ProjectFileAndImportsPredictor();
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