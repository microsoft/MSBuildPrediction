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

            Assert.Equal(20, predictors.Count);
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is AvailableItemNameItemsPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is ContentItemsPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is NoneItemsPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is CompileItemsPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is IntermediateOutputPathPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is OutDirOrOutputPathPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is ProjectFileAndImportsPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is AzureCloudServicePredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is ServiceFabricServiceManifestPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is AzureCloudServiceWorkerFilesPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is CodeAnalysisRuleSetPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is AssemblyOriginatorKeyFilePredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is EmbeddedResourceItemsPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is ReferenceItemsPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is StyleCopPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is ManifestsPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is VSCTCompileItemsPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is EditorConfigFilesItemsPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is ApplicationIconPredictor));
            Assert.NotNull(predictors.FirstOrDefault(predictor => predictor is GeneratePackageOnBuildPredictor));
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
