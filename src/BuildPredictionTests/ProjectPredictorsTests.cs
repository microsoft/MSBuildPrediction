// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Build.Prediction.Predictors;
    using Microsoft.Build.Prediction.Predictors.CopyTask;
    using Xunit;

    public class ProjectPredictorsTests
    {
        [Fact]
        public void BasicPredictors()
        {
            IReadOnlyCollection<IProjectPredictor> predictors = ProjectPredictors.BasicPredictors;

            Assert.Equal(7, predictors.Count);
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is AvailableItemNameItems));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is ContentItems));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is NoneItems));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is CSharpCompileItems));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is IntermediateOutputPathIsOutputDir));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is OutDirOrOutputPathIsOutputDir));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is ProjectFileAndImportedFiles));
        }

        [Fact]
        public void AllPredictors()
        {
            IReadOnlyCollection<IProjectPredictor> predictors = ProjectPredictors.AllPredictors;

            Assert.Equal(8, predictors.Count);
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is AvailableItemNameItems));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is ContentItems));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is NoneItems));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is CopyTaskPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is CSharpCompileItems));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is IntermediateOutputPathIsOutputDir));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is OutDirOrOutputPathIsOutputDir));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is ProjectFileAndImportedFiles));
        }
    }
}
