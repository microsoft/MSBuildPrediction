// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Build.Prediction.StandardPredictors;
    using Microsoft.Build.Prediction.StandardPredictors.CopyTask;
    using Xunit;

    public class ProjectStaticPredictorsTests
    {
        [Fact]
        public void BasicPredictors()
        {
            IReadOnlyCollection<IProjectStaticPredictor> predictors = ProjectStaticPredictors.BasicPredictors;

            Assert.Equal(6, predictors.Count);
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is AvailableItemNameItems));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is CopyTaskPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is CSharpCompileItems));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is IntermediateOutputPathIsOutputDir));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is OutDirOrOutputPathIsOutputDir));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is ProjectFileAndImportedFiles));
        }
    }
}
