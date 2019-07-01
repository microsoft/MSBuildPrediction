// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class ProjectPredictorsTests
    {
        [Fact]
        public void BasicPredictors()
        {
            IReadOnlyCollection<IProjectPredictor> predictors = ProjectPredictors.BasicPredictors;

            Assert.Equal(14, predictors.Count);
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is AvailableItemNameItems));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is ContentItems));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is NoneItems));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is CSharpCompileItems));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is IntermediateOutputPathIsOutputDir));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is OutDirOrOutputPathIsOutputDir));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is ProjectFileAndImportedFiles));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is AzureCloudServicePredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is ServiceFabricServiceManifestPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is AzureCloudServiceWorkerFilesPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is CodeAnalysisRuleSetPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is AssemblyOriginatorKeyFilePredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is EmbeddedResourceItemsPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is ReferenceItemsPredictor));
        }

        [Fact]
        public void AllPredictors()
        {
            // All predictors means all predictors. Use reflection to ensure we really did get all creatable IProjectPredictors.
            var expectedPredictorTypes = typeof(IProjectPredictor).Assembly.GetTypes()
                .Where(type => !type.IsInterface && !type.IsAbstract && typeof(IProjectPredictor).IsAssignableFrom(type))
                .ToList();

            IReadOnlyCollection<IProjectPredictor> actualPredictors = ProjectPredictors.AllPredictors;

            Assert.Equal(expectedPredictorTypes.Count, actualPredictors.Count);
            foreach (Type predictorType in expectedPredictorTypes)
            {
                Assert.NotNull(actualPredictors.FirstOrDefault(predictor => predictor.GetType() == predictorType));
            }
        }
    }
}
