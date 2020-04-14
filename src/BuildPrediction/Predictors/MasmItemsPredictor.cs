// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Predicts inputs for Assembler files built as MASM items.
    /// </summary>
    /// <remarks>
    /// MASM/ml64.exe, assembles and links one or more assembly-language source files.
    ///
    /// While not as widely used, the masm.props/masm.targets are distributed with Visual Studio C/C++ SDK:
    /// - $(VCTargetsPath)\BuildCustomizations\masm.props
    /// - $(VCTargetsPath)\BuildCustomizations\masm.targets
    /// </remarks>
    /// <seealso cref="Microsoft.Build.Prediction.IProjectPredictor" />
    public class MasmItemsPredictor : IProjectPredictor
    {
        /// <summary>
        /// Item name for a file to be assembled.
        /// </summary>
        internal const string MasmItemName = "MASM";

        /// <summary>
        /// Property name declared in masm.targets
        /// </summary>
        internal const string MasmBeforeTargetsPropertyName = "MASMBeforeTargets";

        /// <summary>
        /// Optional item meta data name that represents the paths for the assembler to generate an assembled code listing file.
        /// </summary>
        internal const string AssembledCodeListingFileMetadata = "AssembledCodeListingFile";

        /// <summary>
        /// Optional item meta data name that specifies whether to generate browse information file and its
        /// optional name or location of the browse information file.
        /// </summary>
        internal const string BrowseFileMetadata = "BrowseFile";

        /// <summary>
        /// Item meta data name that determines if the item group should skipped, and not assembled.
        /// </summary>
        internal const string ExcludedFromBuildMetadata = "ExcludedFromBuild";

        /// <summary>
        /// Item meta data name that represents the fully qualified path to the assembler output.
        /// </summary>
        internal const string ObjectFileNameMetadata = "ObjectFileName";

        /// <summary>
        /// Item meta data name that represents the paths for the assembler to search for include file(s).
        /// </summary>
        internal const string IncludePathsMetadata = "IncludePaths";

        /// <summary>
        /// Character separator for values specified in %(MASM.IncludePaths).
        /// </summary>
        private static readonly char[] IncludePathsSeparator = { ';' };

        /// <inheritdoc />
        public void PredictInputsAndOutputs(ProjectInstance project, ProjectPredictionReporter reporter)
        {
            // This is based on $(VCTargetsPath)\BuildCustomizations\masm.targets, if the before targets
            // property isn't set then the masm.targets haven't been imported, and no @(MASM) items will
            // be processed by the MASM targets.
            if (string.IsNullOrWhiteSpace(project.GetPropertyValue(MasmBeforeTargetsPropertyName)))
            {
                return;
            }

            ICollection<ProjectItemInstance> masmItems = project.GetItems(MasmItemName);
            if (masmItems.Count == 0)
            {
                return;
            }

            var reportedIncludes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (ProjectItemInstance masmItem in masmItems)
            {
                if (IsExcludedFromBuild(masmItem))
                {
                    continue;
                }

                ReportInputs(reporter, masmItem, reportedIncludes);
                ReportOutputs(reporter, masmItem);
            }
        }

        private bool IsExcludedFromBuild(ProjectItemInstance masmItem) =>
            masmItem.GetMetadataValue(ExcludedFromBuildMetadata).Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase);

        private void ReportInputs(ProjectPredictionReporter reporter, ProjectItemInstance masmItem, HashSet<string> reportedIncludes)
        {
            reporter.ReportInputFile(masmItem.EvaluatedInclude);

            string[] includePaths = masmItem.GetMetadataValue(IncludePathsMetadata)
                .Split(IncludePathsSeparator, StringSplitOptions.RemoveEmptyEntries);

            // Avoid reporting paths that we've already reported for this project.
            foreach (string includePath in includePaths)
            {
                string trimmedPath = includePath.Trim();
                if (!string.IsNullOrEmpty(trimmedPath) && reportedIncludes.Add(trimmedPath))
                {
                    reporter.ReportInputDirectory(trimmedPath);
                }
            }
        }

        private void ReportOutputs(ProjectPredictionReporter reporter, ProjectItemInstance masmItem)
        {
            reporter.ReportOutputFile(masmItem.GetMetadataValue(ObjectFileNameMetadata));

            string assembledCodeListingFile = masmItem.GetMetadataValue(AssembledCodeListingFileMetadata);
            if (!string.IsNullOrWhiteSpace(assembledCodeListingFile))
            {
                reporter.ReportOutputFile(assembledCodeListingFile);
            }

            string browseFile = masmItem.GetMetadataValue(BrowseFileMetadata);
            if (!string.IsNullOrWhiteSpace(browseFile))
            {
                reporter.ReportOutputFile(browseFile);
            }
        }
    }
}
