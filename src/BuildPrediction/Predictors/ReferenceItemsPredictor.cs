// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System;
    using System.IO;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Finds EmbeddedResource items as inputs.
    /// </summary>
    public class ReferenceItemsPredictor : IProjectPredictor
    {
        internal const string ReferenceItemName = "Reference";

        internal const string HintPathMetadata = "HintPath";

        // Note that this isn't static to avoid holding onto memory after prediction is over.
        private readonly char[] _invalidPathCharacters = Path.GetInvalidPathChars();

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            Project project,
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            foreach (ProjectItemInstance item in projectInstance.GetItems(ReferenceItemName))
            {
                // <HintPath> metadata is treated as the truth if it exists.
                // Example: <Reference Include="SomeAssembly">
                //            <HintPath>..\packages\SomePackage.1.0.0.0\lib\net45\SomeAssembly.dll</HintPath>
                //          </Reference>
                string hintPath = item.GetMetadataValue(HintPathMetadata);
                if (!string.IsNullOrEmpty(hintPath))
                {
                    predictionReporter.ReportInputFile(hintPath);
                    continue;
                }
                else
                {
                    // If there is no hint path then if the reference is valid then the EvaluatedInclude is either a path to a file
                    // or the name of a dll from the GAC or platform.
                    string identity = item.EvaluatedInclude;

                    // Since we don't know whether it's even a file path, check that it's at least a valid path before trying to use it like one.
                    if (identity.IndexOfAny(_invalidPathCharacters) != -1)
                    {
                        continue;
                    }

                    // If it's from the GAC or platform, it won't have directory separators.
                    // Example: <Reference Include="System.Data" />
#if NETCOREAPP // netcoreapp has an overload which takes character and StringComparison while Net472 doesn't, and analyzers enforce that we provide a StringComparison when possible.
                    if (identity.IndexOf(Path.DirectorySeparatorChar, StringComparison.Ordinal) == -1)
#else
                    if (identity.IndexOf(Path.DirectorySeparatorChar) == -1)
#endif
                    {
                        // Edge-case if the reference is adjacent to the project so might not have directory separators. Check file existence in that case.
                        // Example: <Reference Include="CheckedInReference.dll" />
                        if (!File.Exists(Path.Combine(projectInstance.Directory, identity)))
                        {
                            continue;
                        }
                    }

                    // The value seems like it could be a file path since it's a valid path and has directory separators. Note that we can't
                    // actually check for file existence here since it might be a reference to an assembly produced by another project in the repository.
                    predictionReporter.ReportInputFile(identity);
                }
            }
        }
    }
}
