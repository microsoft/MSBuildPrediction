// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction
{
    using System.Collections.Generic;
    using Microsoft.Build.Prediction.StandardPredictors;
    using Microsoft.Build.Prediction.StandardPredictors.CopyTask;

    /// <summary>
    /// Creates instances of <see cref="IProjectStaticPredictor"/> for use with
    /// <see cref="ProjectStaticPredictionExecutor"/>.
    /// </summary>
    public static class ProjectStaticPredictors
    {
        /// <summary>
        /// Gets a collection of all basic <see cref="IProjectStaticPredictor"/>s. This is for convencience to avoid needing to specify all basic predictors explicitly.
        /// </summary>
        /// <remarks>
        /// This includes the following predictors:
        /// <list type="bullet">
        /// <item><see cref="AvailableItemNameItems"/></item>
        /// <item><see cref="CSharpCompileItems"/></item>
        /// <item><see cref="IntermediateOutputPathIsOutputDir"/></item>
        /// <item><see cref="OutDirOrOutputPathIsOutputDir"/></item>
        /// <item><see cref="ProjectFileAndImportedFiles"/></item>
        /// </list>
        /// </remarks>
        /// <returns>A collection of <see cref="IProjectStaticPredictor"/>.</returns>
        public static IReadOnlyCollection<IProjectStaticPredictor> BasicPredictors => new IProjectStaticPredictor[]
        {
            new AvailableItemNameItems(),
            new CSharpCompileItems(),
            new IntermediateOutputPathIsOutputDir(),
            new OutDirOrOutputPathIsOutputDir(),
            new ProjectFileAndImportedFiles(),
            //// NOTE! When adding a new predictor here, be sure to update the doc comment above.
        };

        /// <summary>
        /// Gets a collection of all <see cref="IProjectStaticPredictor"/>s. This is for convencience to avoid needing to specify all predictors explicitly.
        /// </summary>
        /// <remarks>
        /// This includes the following predictors:
        /// <list type="bullet">
        /// <item><see cref="AvailableItemNameItems"/></item>
        /// <item><see cref="CopyTaskPredictor"/></item>
        /// <item><see cref="CSharpCompileItems"/></item>
        /// <item><see cref="IntermediateOutputPathIsOutputDir"/></item>
        /// <item><see cref="OutDirOrOutputPathIsOutputDir"/></item>
        /// <item><see cref="ProjectFileAndImportedFiles"/></item>
        /// </list>
        /// </remarks>
        /// <returns>A collection of <see cref="IProjectStaticPredictor"/>.</returns>
        public static IReadOnlyCollection<IProjectStaticPredictor> AllPredictors => new IProjectStaticPredictor[]
        {
            new AvailableItemNameItems(),
            new CopyTaskPredictor(),
            new CSharpCompileItems(),
            new IntermediateOutputPathIsOutputDir(),
            new OutDirOrOutputPathIsOutputDir(),
            new ProjectFileAndImportedFiles(),
            //// NOTE! When adding a new predictor here, be sure to update the doc comment above.
        };
    }
}
