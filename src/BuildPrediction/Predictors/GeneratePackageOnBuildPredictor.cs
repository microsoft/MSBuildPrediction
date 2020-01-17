// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System;
    using System.IO;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Predicts inputs and outputs when using the built-in feature of the Sdk to create a NuGet package during the build.
    /// </summary>
    public sealed class GeneratePackageOnBuildPredictor : IProjectPredictor
    {
        internal const string GeneratePackageOnBuildPropertyName = "GeneratePackageOnBuild";

        internal const string PackageIdPropertyName = "PackageId";

        internal const string PackageVersionPropertyName = "PackageVersion";

        internal const string PackageOutputPathPropertyName = "PackageOutputPath";

        internal const string OutputPathPropertyName = "OutputPath";

        internal const string OutputFileNamesWithoutVersionPropertyName = "OutputFileNamesWithoutVersion";

        internal const string NuspecOutputPathPropertyName = "NuspecOutputPath";

        internal const string IncludeSourcePropertyName = "IncludeSource";

        internal const string IncludeSymbolsPropertyName = "IncludeSymbols";

        internal const string SymbolPackageFormatPropertyName = "SymbolPackageFormat";

        internal const string NuspecFilePropertyName = "NuspecFile";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            // This is based on NuGet.Build.Tasks.Pack.targets and GetPackOutputItemsTask
            // See: https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Core/NuGet.Build.Tasks.Pack/NuGet.Build.Tasks.Pack.targets
            // See: https://github.com/NuGet/NuGet.Client/blob/dev/src/NuGet.Core/NuGet.Build.Tasks.Pack/GetPackOutputItemsTask.cs
            var generatePackageOnBuild = projectInstance.GetPropertyValue(GeneratePackageOnBuildPropertyName);
            if (!generatePackageOnBuild.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var packageId = projectInstance.GetPropertyValue(PackageIdPropertyName);
            var packageVersion = projectInstance.GetPropertyValue(PackageVersionPropertyName);
            var packageOutputPath = projectInstance.GetPropertyValue(PackageOutputPathPropertyName);
            var nuspecOutputPath = projectInstance.GetPropertyValue(NuspecOutputPathPropertyName);
            var includeSource = projectInstance.GetPropertyValue(IncludeSourcePropertyName).Equals("true", StringComparison.OrdinalIgnoreCase);
            var includeSymbols = projectInstance.GetPropertyValue(IncludeSymbolsPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase);
            var outputFileNamesWithoutVersion = projectInstance.GetPropertyValue(OutputFileNamesWithoutVersionPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase);

            var symbolPackageFormat = projectInstance.GetPropertyValue(SymbolPackageFormatPropertyName);

            // PackageOutputPath defaults to OutputPath in the _CalculateInputsOutputsForPack target, not statically.
            if (string.IsNullOrEmpty(packageOutputPath))
            {
                packageOutputPath = projectInstance.GetPropertyValue(OutputPathPropertyName);
            }

            // All params are effectively required
            if (!string.IsNullOrEmpty(packageId)
                && !string.IsNullOrEmpty(packageVersion)
                && !string.IsNullOrEmpty(packageOutputPath)
                && !string.IsNullOrEmpty(nuspecOutputPath)
                && !string.IsNullOrEmpty(symbolPackageFormat))
            {
                var fileBaseName = outputFileNamesWithoutVersion ? packageId : $"{packageId}.{packageVersion}";

                // Nuspec files can also be provided instead of generated, in which case we should treat it like an input, not an output.
                var nuspecFile = projectInstance.GetPropertyValue(NuspecFilePropertyName);

                predictionReporter.ReportOutputFile(Path.Combine(packageOutputPath, fileBaseName + ".nupkg"));
                if (string.IsNullOrEmpty(nuspecFile))
                {
                    predictionReporter.ReportOutputFile(Path.Combine(nuspecOutputPath, fileBaseName + ".nuspec"));
                }
                else
                {
                    predictionReporter.ReportInputFile(nuspecFile);
                }

                if (includeSource || includeSymbols)
                {
                    predictionReporter.ReportOutputFile(Path.Combine(packageOutputPath, fileBaseName + (symbolPackageFormat.Equals("snupkg", StringComparison.OrdinalIgnoreCase) ? ".snupkg" : ".symbols.nupkg")));
                    if (string.IsNullOrEmpty(nuspecFile))
                    {
                        predictionReporter.ReportOutputFile(Path.Combine(nuspecOutputPath, fileBaseName + ".symbols.nuspec"));
                    }
                }
            }
        }
    }
}
