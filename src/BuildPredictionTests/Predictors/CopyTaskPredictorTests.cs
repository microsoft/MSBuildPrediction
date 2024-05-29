// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Prediction.Predictors.CopyTask;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class CopyTaskPredictorTests : TestBase
    {
        private const string CopyTestsDirectoryPath = @"TestsData\Copy\";

        private readonly PredictedItem _copy1Dll = new PredictedItem("copy1.dll", nameof(CopyTaskPredictor));
        private readonly PredictedItem _copy2Dll = new PredictedItem("copy2.dll", nameof(CopyTaskPredictor));
        private readonly PredictedItem _copy3Dll = new PredictedItem(@"Copy\copy3.dll", nameof(CopyTaskPredictor));

        protected override string TestsDirectoryPath => CopyTestsDirectoryPath;

        [Fact]
        public void TestDefaultTargetDestinationFilesCopyProject()
        {
            PredictedItem[] expectedInputFiles =
            {
                _copy1Dll,
                _copy2Dll,
            };

            PredictedItem[] expectedOutputDirectories =
            {
                new PredictedItem(@"target\Debug\x64\folder1", nameof(CopyTaskPredictor)),
                new PredictedItem(@"target\Debug\x64\folder2", nameof(CopyTaskPredictor)),
            };

            var predictor = new CopyTaskPredictor();
            ParseAndVerifyProject("destinationFilesCopy.csproj", predictor, expectedInputFiles, null, null, expectedOutputDirectories);
        }

        [Fact]
        public void TestCustomTargetFilesCopy()
        {
            PredictedItem[] expectedInputFiles =
            {
                _copy1Dll,
                _copy2Dll,
            };

            PredictedItem[] expectedOutputDirectories =
            {
                new PredictedItem(@"target\Debug\x64\folder1", nameof(CopyTaskPredictor)),
                new PredictedItem(@"target\Debug\x64\folder2", nameof(CopyTaskPredictor)),
            };

            var predictor = new CopyTaskPredictor();
            ParseAndVerifyProject("customTargetWithCopy.csproj", predictor, expectedInputFiles, null, null, expectedOutputDirectories);
        }

        /// <summary>
        /// Tests copy parsing for non-standard default targets.
        /// Makes sure that when DefaultTargets/InitialTargets specified differ from the default Build target,
        /// we ignore the default and only consider the Build target.
        /// </summary>
        [Fact]
        public void TestCopyParseNonStandardDefaultTargets()
        {
            PredictedItem[] expectedInputFiles =
            {
                _copy2Dll,
            };

            PredictedItem[] expectedOutputDirectories =
            {
                new PredictedItem(@"target\Debug\x64\folder1", nameof(CopyTaskPredictor)),
            };

            var predictor = new CopyTaskPredictor();
            ParseAndVerifyProject("CopyDefaultCustomTargets.csproj", predictor, expectedInputFiles, null, null, expectedOutputDirectories);
        }

        /// <summary>
        /// Tests the copy inputs before after targets.
        /// Scenario: With MSBuild v4.0, Target Synchronization can happen with DependsOnTargets
        /// and Before/After targets on the downstream target. This ensures that inputs from those copy tasks are captured in the predictions.
        /// We need to use a custom targets file since all targets in the project are automatically added to be parsed and do not test the logic
        /// of target synchronizations.
        /// </summary>
        [Fact]
        public void TestCopyInputsBeforeAfterTargets()
        {
            PredictedItem[] expectedInputFiles =
            {
                _copy1Dll,
                _copy2Dll,
            };

            PredictedItem[] expectedOutputDirectories =
            {
                new PredictedItem(@"target\Debug\x64\folder1", nameof(CopyTaskPredictor)),
                new PredictedItem(@"target\Debug\x64\folder3", nameof(CopyTaskPredictor)),
                new PredictedItem(@"target\Debug\x64\folder4", nameof(CopyTaskPredictor)),
            };

            var predictor = new CopyTaskPredictor();
            ParseAndVerifyProject("CopyCustomImportedTargets.csproj", predictor, expectedInputFiles, null, null, expectedOutputDirectories);
        }

        /// <summary>
        /// Tests the copy batched items with this file macros.
        /// Scenario: Copy tasks are allowed to declare inputs with MSBuild batching. To capture more dependency
        /// closure to get a more complete DGG, parsing these batched inputs is recommended. In the absence of such
        /// parsing, users would have to declare QCustomInput/Outputs for each of the batched copies.
        /// Additionally, copy tasks in targets that exist in other folders can use $(MSBuildThisFile) macros that evaluate
        /// to something outside the Project's context. These need to be evaluated correctly as well.
        /// </summary>
        [Fact]
        public void TestCopyBatchedItemsWithThisFileMacros()
        {
            PredictedItem[] expectedInputFiles =
            {
                _copy1Dll,
                _copy2Dll,

                // TODO: Note double backslash in test - add path normalization and canonicalization to input and output paths.
                new PredictedItem(@"Copy\\copy3.dll", nameof(CopyTaskPredictor)),
            };

            PredictedItem[] expectedOutputDirectories =
            {
                new PredictedItem(@"target\Debug\x64\folder1", nameof(CopyTaskPredictor)),
                new PredictedItem(@"target\Debug\x64\folder2", nameof(CopyTaskPredictor)),
                new PredictedItem(@"target\Debug\x64\folder3", nameof(CopyTaskPredictor)),
            };

            var predictor = new CopyTaskPredictor();
            ParseAndVerifyProject("CopyTestBatchingInputs.csproj", predictor, expectedInputFiles, null, null, expectedOutputDirectories);
        }

        /// <summary>
        /// Test that copy tasks with batch inputs work.
        /// </summary>
        [Fact]
        public void TestCopyBatchingDestinationFolder()
        {
            PredictedItem[] expectedInputFiles =
            {
                _copy1Dll,
                _copy2Dll,
                new PredictedItem("SomeFile.cs", nameof(CopyTaskPredictor)),
            };

            PredictedItem[] expectedOutputDirectories =
            {
                // TODO: Note trailing backslash in test - add path normalization and canonicalization to input and output paths.
                new PredictedItem(@"Debug\x64\", nameof(CopyTaskPredictor)),
            };

            var predictor = new CopyTaskPredictor();
            ParseAndVerifyProject("CopyTestBatchingDestinationFolder.csproj", predictor, expectedInputFiles, null, null, expectedOutputDirectories);
        }

        [Fact]
        public void TestCopyParseTimeNotExistFilesCopyProject()
        {
            PredictedItem[] expectedInputFiles =
            {
                new PredictedItem("NotExist1.dll", nameof(CopyTaskPredictor)),
                new PredictedItem("NotExist2.dll", nameof(CopyTaskPredictor)),
            };

            PredictedItem[] expectedOutputDirectories =
            {
                new PredictedItem(@"target\Debug\x64\folder1", nameof(CopyTaskPredictor)),
                new PredictedItem(@"target\Debug\x64\folder2", nameof(CopyTaskPredictor)),
            };

            var predictor = new CopyTaskPredictor();
            ParseAndVerifyProject("copyparsetimenotexistfile.csproj", predictor, expectedInputFiles, null, null, expectedOutputDirectories);
        }

        [Fact]
        public void TestWildcardsInIncludeCopyProject()
        {
            PredictedItem[] expectedInputFiles =
            {
                _copy1Dll,
                _copy2Dll,
                _copy3Dll,
            };

            PredictedItem[] expectedOutputDirectories =
            {
                new PredictedItem(@"target\debug\amd64\folder1", nameof(CopyTaskPredictor)),
            };

            var predictor = new CopyTaskPredictor();
            ParseAndVerifyProject("wildcardsInIncludeCopy.csproj", predictor, expectedInputFiles, null, null, expectedOutputDirectories);
        }

        [Fact]
        public void TestIncludeViaItemGroupCopyProject()
        {
            PredictedItem[] expectedInputFiles =
            {
                _copy1Dll,
                _copy2Dll,
            };

            PredictedItem[] expectedOutputDirectories =
            {
                new PredictedItem(@"target\debug\amd64\folder1", nameof(CopyTaskPredictor)),
            };

            var predictor = new CopyTaskPredictor();
            ParseAndVerifyProject("IncludeViaItemGroupCopy.csproj", predictor, expectedInputFiles, null, null, expectedOutputDirectories);
        }

        [Fact]
        public void TestDestinationFilesItemTransformationCopyProject()
        {
            PredictedItem[] expectedInputFiles =
            {
                _copy1Dll,
                _copy2Dll,
            };

            PredictedItem[] expectedOutputDirectories =
            {
                new PredictedItem(@"target\debug\amd64\folder1", nameof(CopyTaskPredictor)),
            };

            var predictor = new CopyTaskPredictor();
            ParseAndVerifyProject("DestinationFilesItemTransformation.csproj", predictor, expectedInputFiles, null, null, expectedOutputDirectories);
        }

        [Fact]
        public void TestMultipleTargetsDestinationFolderCopyProject()
        {
            PredictedItem[] expectedInputFiles =
            {
                _copy1Dll,
                _copy2Dll,
            };

            PredictedItem[] expectedOutputDirectories =
            {
                new PredictedItem(@"target\Debug\x64\folder1", nameof(CopyTaskPredictor)),
                new PredictedItem(@"target\Debug\x64\folder2", nameof(CopyTaskPredictor)),
            };

            var predictor = new CopyTaskPredictor();
            ParseAndVerifyProject("destinationFolderMultipleTargetsCopy.csproj", predictor, expectedInputFiles, null, null, expectedOutputDirectories);
        }

        [Fact]
        public void TestTargetDependsOnCopyProject()
        {
            PredictedItem[] expectedInputFiles =
            {
                _copy1Dll,
                _copy2Dll,
            };

            PredictedItem[] expectedOutputDirectories =
            {
                new PredictedItem(@"target\debug\amd64\folder1", nameof(CopyTaskPredictor)),
                new PredictedItem(@"target\debug\amd64\folder2", nameof(CopyTaskPredictor)),
            };

            var predictor = new CopyTaskPredictor();
            ParseAndVerifyProject("TargetDependsOnCopy.csproj", predictor, expectedInputFiles, null, null, expectedOutputDirectories);
        }

        [Fact]
        public void TestTargetConditionInCopyProject()
        {
            PredictedItem[] expectedInputFiles =
            {
                _copy1Dll,
                new PredictedItem("copy1dependency.dll", nameof(CopyTaskPredictor)),
            };

            PredictedItem[] expectedOutputDirectories =
            {
                new PredictedItem(@"target\debug\amd64\folder1", nameof(CopyTaskPredictor)),
            };

            var predictor = new CopyTaskPredictor();
            ParseAndVerifyProject("TargetConditionInCopy.csproj", predictor, expectedInputFiles, null, null, expectedOutputDirectories);
        }

        [Fact]
        public void TestTaskConditionInCopyProject()
        {
            PredictedItem[] expectedInputFiles =
            {
                _copy1Dll,
            };

            PredictedItem[] expectedOutputDirectories =
            {
                new PredictedItem(@"target\debug\amd64\folder1", nameof(CopyTaskPredictor)),
            };

            var predictor = new CopyTaskPredictor();
            ParseAndVerifyProject("TaskConditionInCopy.csproj", predictor, expectedInputFiles, null, null, expectedOutputDirectories);
        }
    }
}