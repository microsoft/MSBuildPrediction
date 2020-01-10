// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Generates inputs from all globally scoped MSBuild Items whose types
    /// are listed in AvailableItemName metadata.
    /// </summary>
    /// <remarks>
    /// AvailableItemNames are usually used for integration with Visual Studio,
    /// see https://docs.microsoft.com/en-us/visualstudio/msbuild/visual-studio-integration-msbuild?view=vs-2017 ,
    /// but they are a useful shorthand for finding file items.
    ///
    /// As an example, for vcxproj projects the ClCompile item name is listed
    /// as an AvailableItemName by Microsoft.CppCommon.targets.
    ///
    /// Interestingly, C# Compile items have no AvailableItemName in their associated .targets file.
    /// </remarks>
    public sealed class AvailableItemNameItemsPredictor : IProjectPredictor
    {
        internal const string AvailableItemName = "AvailableItemName";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            var availableItemNames = new HashSet<string>(
                projectInstance.GetItems(AvailableItemName).Select(item => item.EvaluatedInclude),
                StringComparer.OrdinalIgnoreCase);

            foreach (string availableItemName in availableItemNames)
            {
                foreach (ProjectItemInstance item in projectInstance.GetItems(availableItemName))
                {
                    predictionReporter.ReportInputFile(item.EvaluatedInclude);
                }
            }
        }
    }
}
