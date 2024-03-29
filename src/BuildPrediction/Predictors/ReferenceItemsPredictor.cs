﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Finds Reference items and any related files as inputs.
    /// </summary>
    public sealed class ReferenceItemsPredictor : IProjectPredictor
    {
        internal const string ReferenceItemName = "Reference";

        internal const string HintPathMetadata = "HintPath";

        internal const string AllowedReferenceRelatedFileExtensionsPropertyName = "AllowedReferenceRelatedFileExtensions";

        private static readonly char[] InvalidPathCharacters = Path.GetInvalidPathChars();

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            List<string> relatedFileExtensions = projectInstance.GetPropertyValue(AllowedReferenceRelatedFileExtensionsPropertyName).SplitStringList();

            foreach (ProjectItemInstance item in projectInstance.GetItems(ReferenceItemName))
            {
                if (TryGetReferenceItemFilePath(projectInstance, item, out string filePath))
                {
                    ReportInputAndRelatedFiles(filePath, projectInstance.Directory, relatedFileExtensions, predictionReporter);
                }
            }
        }

        internal static bool TryGetReferenceItemFilePath(ProjectInstance projectInstance, ProjectItemInstance referenceItem, out string filePath)
        {
            // <HintPath> metadata is treated as the truth if it exists.
            // Example: <Reference Include="SomeAssembly">
            //            <HintPath>..\packages\SomePackage.1.0.0.0\lib\net45\SomeAssembly.dll</HintPath>
            //          </Reference>
            string hintPath = referenceItem.GetMetadataValue(HintPathMetadata);
            if (!string.IsNullOrEmpty(hintPath))
            {
                filePath = hintPath;
                return true;
            }
            else
            {
                // If there is no hint path then if the reference is valid then the EvaluatedInclude is either a path to a file
                // or the name of a dll from the GAC or platform.
                string identity = referenceItem.EvaluatedInclude;

                // Since we don't know whether it's even a file path, check that it's at least a valid path before trying to use it like one.
                if (identity.IndexOfAny(InvalidPathCharacters) != -1)
                {
                    filePath = null;
                    return false;
                }

                // If it's from the GAC or platform, it won't have directory separators.
                // Example: <Reference Include="System.Data" />
                if (identity.IndexOf(Path.DirectorySeparatorChar, StringComparison.Ordinal) == -1)
                {
                    // Edge-case if the reference is adjacent to the project so might not have directory separators. Check file existence in that case.
                    // Example: <Reference Include="CheckedInReference.dll" />
                    if (!File.Exists(Path.Combine(projectInstance.Directory, identity)))
                    {
                        filePath = null;
                        return false;
                    }
                }

                // The value seems like it could be a file path since it's a valid path and has directory separators. Note that we can't
                // actually check for file existence here since it might be a reference to an assembly produced by another project in the repository.
                filePath = identity;
                return true;
            }
        }

        private void ReportInputAndRelatedFiles(
            string referencePath,
            string projectDirectory,
            List<string> relatedFileExtensions,
            ProjectPredictionReporter predictionReporter)
        {
            var referenceFullPath = Path.GetFullPath(Path.Combine(projectDirectory, referencePath));

            predictionReporter.ReportInputFile(referenceFullPath);

            // If the reference doesn't exist, it might be generated by the build.
            // In that case, we can't know for sure whether there will be related
            // files or not, so don't bother trying in that case.
            if (!File.Exists(referenceFullPath))
            {
                return;
            }

            foreach (var ext in relatedFileExtensions)
            {
                var relatedFile = Path.ChangeExtension(referenceFullPath, ext);
                if (File.Exists(relatedFile))
                {
                    predictionReporter.ReportInputFile(relatedFile);
                }
            }
        }
    }
}