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
        /// Gets a collection of all basic <see cref="IProjectPredictor"/>s. This is for convencience to avoid needing to specify all basic predictors explicitly.
        /// </summary>
        /// <remarks>
        /// This includes the following predictors:
        /// <list type="bullet">
        /// <item><see cref="AvailableItemNameItems"/></item>
        /// <item><see cref="ContentItems"/></item>
        /// <item><see cref="NoneItems"/></item>
        /// <item><see cref="CSharpCompileItems"/></item>
        /// <item><see cref="IntermediateOutputPathIsOutputDir"/></item>
        /// <item><see cref="OutDirOrOutputPathIsOutputDir"/></item>
        /// <item><see cref="ProjectFileAndImportedFiles"/></item>
        /// <item><see cref="AzureCloudServicePredictor"/></item>
        /// <item><see cref="ServiceFabricServiceManifestPredictor"/></item>
        /// <item><see cref="AzureCloudServiceWorkerFilesPredictor"/></item>
        /// <item><see cref="CodeAnalysisRuleSetPredictor"/></item>
        /// <item><see cref="AssemblyOriginatorKeyFilePredictor"/></item>
        /// <item><see cref="EmbeddedResourceItemsPredictor"/></item>
        /// <item><see cref="ReferenceItemsPredictor"/></item>
        /// <item><see cref="StyleCopPredictor"/></item>
        /// </list>
        /// </remarks>
        /// <returns>A collection of <see cref="IProjectPredictor"/>.</returns>
        public static IReadOnlyCollection<IProjectPredictor> BasicPredictors => new IProjectPredictor[]
        {
            new AvailableItemNameItems(),
            new ContentItems(),
            new NoneItems(),
            new CSharpCompileItems(),
            new IntermediateOutputPathIsOutputDir(),
            new OutDirOrOutputPathIsOutputDir(),
            new ProjectFileAndImportedFiles(),
            new AzureCloudServicePredictor(),
            new ServiceFabricServiceManifestPredictor(),
            new AzureCloudServiceWorkerFilesPredictor(),
            new CodeAnalysisRuleSetPredictor(),
            new AssemblyOriginatorKeyFilePredictor(),
            new EmbeddedResourceItemsPredictor(),
            new ReferenceItemsPredictor(),
            new StyleCopPredictor(),
            //// NOTE! When adding a new predictor here, be sure to update the doc comment above.
        };

        /// <summary>
        /// Gets a collection of all <see cref="IProjectPredictor"/>s. This is for convencience to avoid needing to specify all predictors explicitly.
        /// </summary>
        /// <remarks>
        /// This includes the following predictors:
        /// <list type="bullet">
        /// <item><see cref="AvailableItemNameItems"/></item>
        /// <item><see cref="ContentItems"/></item>
        /// <item><see cref="NoneItems"/></item>
        /// <item><see cref="CopyTaskPredictor"/></item>
        /// <item><see cref="CSharpCompileItems"/></item>
        /// <item><see cref="IntermediateOutputPathIsOutputDir"/></item>
        /// <item><see cref="OutDirOrOutputPathIsOutputDir"/></item>
        /// <item><see cref="ProjectFileAndImportedFiles"/></item>
        /// <item><see cref="AzureCloudServicePredictor"/></item>
        /// <item><see cref="ServiceFabricServiceManifestPredictor"/></item>
        /// <item><see cref="AzureCloudServiceWorkerFilesPredictor"/></item>
        /// <item><see cref="CodeAnalysisRuleSetPredictor"/></item>
        /// <item><see cref="AssemblyOriginatorKeyFilePredictor"/></item>
        /// <item><see cref="EmbeddedResourceItemsPredictor"/></item>
        /// <item><see cref="ReferenceItemsPredictor"/></item>
        /// <item><see cref="ArtifactsSdkPredictor"/></item>
        /// <item><see cref="StyleCopPredictor"/></item>
        /// </list>
        /// </remarks>
        /// <returns>A collection of <see cref="IProjectPredictor"/>.</returns>
        public static IReadOnlyCollection<IProjectPredictor> AllPredictors => new IProjectPredictor[]
        {
            new AvailableItemNameItems(),
            new ContentItems(),
            new NoneItems(),
            new CopyTaskPredictor(),
            new CSharpCompileItems(),
            new IntermediateOutputPathIsOutputDir(),
            new OutDirOrOutputPathIsOutputDir(),
            new ProjectFileAndImportedFiles(),
            new AzureCloudServicePredictor(),
            new ServiceFabricServiceManifestPredictor(),
            new AzureCloudServiceWorkerFilesPredictor(),
            new CodeAnalysisRuleSetPredictor(),
            new AssemblyOriginatorKeyFilePredictor(),
            new EmbeddedResourceItemsPredictor(),
            new ReferenceItemsPredictor(),
            new ArtifactsSdkPredictor(),
            new StyleCopPredictor(),
            //// NOTE! When adding a new predictor here, be sure to update the doc comment above.
        };
    }
}
