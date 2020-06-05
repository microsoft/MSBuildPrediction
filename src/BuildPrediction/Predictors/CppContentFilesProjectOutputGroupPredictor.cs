// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Predicts inputs based on the C++ ContentFilesProjectOutputGroup target.
    /// </summary>
    public sealed class CppContentFilesProjectOutputGroupPredictor : IProjectPredictor
    {
        // See Microsoft.CppBuild.targets for items used in ContentFilesProjectOutputGroup.
        // We're avoiding items which already are included by other predictors (eg ClCompile), and items which aren't available at evaluation time (eg FxcOutputs).
        internal const string XmlItemName = "Xml";
        internal const string TextItemName = "Text";
        internal const string FontItemName = "Font";
        internal const string ObjectItemName = "Object";
        internal const string LibraryItemName = "Library";
        internal const string ManifestItemName = "Manifest";
        internal const string ImageItemName = "Image";
        internal const string MediaItemName = "Media";

        /// <inheritdoc />
        public void PredictInputsAndOutputs(ProjectInstance projectInstance, ProjectPredictionReporter reporter)
        {
            // This predictor only applies to vcxproj files
            if (!projectInstance.FullPath.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            ReportInputsForItemType(reporter, projectInstance, XmlItemName);
            ReportInputsForItemType(reporter, projectInstance, TextItemName);
            ReportInputsForItemType(reporter, projectInstance, FontItemName);
            ReportInputsForItemType(reporter, projectInstance, ObjectItemName);
            ReportInputsForItemType(reporter, projectInstance, LibraryItemName);
            ReportInputsForItemType(reporter, projectInstance, ManifestItemName);
            ReportInputsForItemType(reporter, projectInstance, ImageItemName);
            ReportInputsForItemType(reporter, projectInstance, MediaItemName);
        }

        private void ReportInputsForItemType(ProjectPredictionReporter reporter, ProjectInstance projectInstance, string itemType)
        {
            ICollection<ProjectItemInstance> items = projectInstance.GetItems(itemType);
            if (items.Count == 0)
            {
                return;
            }

            foreach (ProjectItemInstance item in items)
            {
                reporter.ReportInputFile(item.EvaluatedInclude);
            }
        }
    }
}
