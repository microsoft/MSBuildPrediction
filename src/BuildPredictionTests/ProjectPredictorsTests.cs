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
            var expectedPredictorTypes = new[]
            {
                typeof(AvailableItemNameItemsPredictor),
                typeof(ContentItemsPredictor),
                typeof(NoneItemsPredictor),
                typeof(CompileItemsPredictor),
                typeof(IntermediateOutputPathPredictor),
                typeof(OutDirOrOutputPathPredictor),
                typeof(ProjectFileAndImportsPredictor),
                typeof(AzureCloudServicePredictor),
                typeof(ServiceFabricServiceManifestPredictor),
                typeof(AzureCloudServiceWorkerFilesPredictor),
                typeof(CodeAnalysisRuleSetPredictor),
                typeof(AssemblyOriginatorKeyFilePredictor),
                typeof(EmbeddedResourceItemsPredictor),
                typeof(ReferenceItemsPredictor),
                typeof(StyleCopPredictor),
                typeof(ManifestsPredictor),
                typeof(VSCTCompileItemsPredictor),
                typeof(EditorConfigFilesItemsPredictor),
                typeof(ApplicationIconPredictor),
                typeof(GeneratePackageOnBuildPredictor),
                typeof(CompiledAssemblyPredictor),
                typeof(DocumentationFilePredictor),
                typeof(RefAssemblyPredictor),
                typeof(SymbolsFilePredictor),
                typeof(XamlAppDefPredictor),
                typeof(TypeScriptCompileItemsPredictor),
                typeof(ApplicationDefinitionItemsPredictor),
                typeof(PageItemsPredictor),
                typeof(ResourceItemsPredictor),
                typeof(SplashScreenItemsPredictor),
            };

            AssertPredictorsList(expectedPredictorTypes, ProjectPredictors.BasicPredictors);
        }

        [Fact]
        public void AllPredictors()
        {
            // All predictors means all predictors. Use reflection to ensure we really did get all creatable IProjectPredictors.
            Type[] expectedPredictorTypes = typeof(IProjectPredictor).Assembly.GetTypes()
                .Where(type => !type.IsInterface && !type.IsAbstract && typeof(IProjectPredictor).IsAssignableFrom(type))
                .ToArray();

            AssertPredictorsList(expectedPredictorTypes, ProjectPredictors.AllPredictors);
        }

        private static void AssertPredictorsList(Type[] expectedPredictorTypes, IReadOnlyCollection<IProjectPredictor> actualPredictors)
        {
            Assert.Equal(expectedPredictorTypes.Length, actualPredictors.Count);
            foreach (Type predictorType in expectedPredictorTypes)
            {
                Assert.NotNull(actualPredictors.FirstOrDefault(predictor => predictor.GetType() == predictorType));
            }
        }
    }
}
