// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction
{
    using System.Collections.Generic;
    using Microsoft.Build.Prediction.Predictors;
    using Microsoft.Build.Prediction.Predictors.CopyTask;

    /// <summary>
    /// Creates instances of <see cref="IProjectPredictor"/> for use with
    /// <see cref="ProjectPredictionExecutor"/>.
    /// </summary>
    public static class ProjectPredictors
    {
        /// <summary>
        /// Gets a collection of all <see cref="IProjectPredictor"/>s. This is for convencience to avoid needing to specify all predictors explicitly.
        /// </summary>
        /// <remarks>
        /// This includes the following predictors:
        /// <list type="bullet">
        /// <item><see cref="AvailableItemNameItemsPredictor"/></item>
        /// <item><see cref="ContentItemsPredictor"/></item>
        /// <item><see cref="NoneItemsPredictor"/></item>
        /// <item><see cref="CopyTaskPredictor"/></item>
        /// <item><see cref="CompileItemsPredictor"/></item>
        /// <item><see cref="IntermediateOutputPathPredictor"/></item>
        /// <item><see cref="OutDirOrOutputPathPredictor"/></item>
        /// <item><see cref="ProjectFileAndImportsPredictor"/></item>
        /// <item><see cref="AzureCloudServicePredictor"/></item>
        /// <item><see cref="ServiceFabricServiceManifestPredictor"/></item>
        /// <item><see cref="AzureCloudServiceWorkerFilesPredictor"/></item>
        /// <item><see cref="CodeAnalysisRuleSetPredictor"/></item>
        /// <item><see cref="AssemblyOriginatorKeyFilePredictor"/></item>
        /// <item><see cref="EmbeddedResourceItemsPredictor"/></item>
        /// <item><see cref="ReferenceItemsPredictor"/></item>
        /// <item><see cref="ArtifactsSdkPredictor"/></item>
        /// <item><see cref="StyleCopPredictor"/></item>
        /// <item><see cref="ManifestsPredictor"/></item>
        /// <item><see cref="VSCTCompileItemsPredictor"/></item>
        /// <item><see cref="EditorConfigFilesItemsPredictor"/></item>
        /// <item><see cref="ApplicationIconPredictor"/></item>
        /// <item><see cref="GeneratePackageOnBuildPredictor"/></item>
        /// <item><see cref="CompiledAssemblyPredictor"/></item>
        /// <item><see cref="DocumentationFilePredictor"/></item>
        /// <item><see cref="RefAssemblyPredictor"/></item>
        /// <item><see cref="SymbolsFilePredictor"/></item>
        /// <item><see cref="XamlAppDefPredictor"/></item>
        /// <item><see cref="TypeScriptCompileItemsPredictor"/></item>
        /// <item><see cref="ApplicationDefinitionItemsPredictor"/></item>
        /// <item><see cref="PageItemsPredictor"/></item>
        /// <item><see cref="ResourceItemsPredictor"/></item>
        /// <item><see cref="SplashScreenItemsPredictor"/></item>
        /// <item><see cref="TsConfigPredictor"/></item>
        /// </list>
        /// </remarks>
        /// <returns>A collection of <see cref="IProjectPredictor"/>.</returns>
        public static IReadOnlyCollection<IProjectPredictor> AllProjectPredictors => new IProjectPredictor[]
        {
            new AvailableItemNameItemsPredictor(),
            new ContentItemsPredictor(),
            new NoneItemsPredictor(),
            new CopyTaskPredictor(),
            new CompileItemsPredictor(),
            new IntermediateOutputPathPredictor(),
            new OutDirOrOutputPathPredictor(),
            new ProjectFileAndImportsPredictor(),
            new AzureCloudServicePredictor(),
            new ServiceFabricServiceManifestPredictor(),
            new AzureCloudServiceWorkerFilesPredictor(),
            new CodeAnalysisRuleSetPredictor(),
            new AssemblyOriginatorKeyFilePredictor(),
            new EmbeddedResourceItemsPredictor(),
            new ReferenceItemsPredictor(),
            new ArtifactsSdkPredictor(),
            new StyleCopPredictor(),
            new ManifestsPredictor(),
            new VSCTCompileItemsPredictor(),
            new EditorConfigFilesItemsPredictor(),
            new ApplicationIconPredictor(),
            new GeneratePackageOnBuildPredictor(),
            new CompiledAssemblyPredictor(),
            new DocumentationFilePredictor(),
            new RefAssemblyPredictor(),
            new SymbolsFilePredictor(),
            new XamlAppDefPredictor(),
            new TypeScriptCompileItemsPredictor(),
            new ApplicationDefinitionItemsPredictor(),
            new PageItemsPredictor(),
            new ResourceItemsPredictor(),
            new SplashScreenItemsPredictor(),
            new TsConfigPredictor(),
            //// NOTE! When adding a new predictor here, be sure to update the doc comment above.
        };

        /// <summary>
        /// Gets a collection of all <see cref="IProjectGraphPredictor"/>s. This is for convencience to avoid needing to specify all graph predictors explicitly.
        /// </summary>
        /// <remarks>
        /// This includes the following graph predictors:
        /// <list type="bullet">
        /// <item><see cref="ProjectFileAndImportsGraphPredictor"/></item>
        /// <item><see cref="GetCopyToOutputDirectoryItemsGraphPredictor"/></item>
        /// </list>
        /// </remarks>
        /// <returns>A collection of <see cref="IProjectGraphPredictor"/>.</returns>
        public static IReadOnlyCollection<IProjectGraphPredictor> AllProjectGraphPredictors => new IProjectGraphPredictor[]
        {
            new ProjectFileAndImportsGraphPredictor(),
            new GetCopyToOutputDirectoryItemsGraphPredictor(),
            //// NOTE! When adding a new predictor here, be sure to update the doc comment above.
        };
    }
}
