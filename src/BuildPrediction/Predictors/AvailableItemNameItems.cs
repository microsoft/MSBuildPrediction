﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Build.Evaluation;
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
    public class AvailableItemNameItems : IProjectPredictor
    {
        internal const string AvailableItemName = "AvailableItemName";

        /// <inheritdoc/>
        public bool TryPredictInputsAndOutputs(
            Project project,
            ProjectInstance projectInstance,
            out ProjectPredictions predictions)
        {
            // TODO: Need to determine how to normalize evaluated include selected below and determine if it is relative to project.
            var availableItemNames = new HashSet<string>(
                project.GetItems(AvailableItemName).Select(item => item.EvaluatedInclude),
                StringComparer.OrdinalIgnoreCase);

            List<BuildInput> itemInputs = availableItemNames.SelectMany(
                availableItemName => project.GetItems(availableItemName).Select(
                    item => new BuildInput(
                        Path.Combine(project.DirectoryPath, item.EvaluatedInclude),
                        isDirectory: false)))
                .ToList();
            if (itemInputs.Count > 0)
            {
                predictions = new ProjectPredictions(itemInputs, null);
                return true;
            }

            predictions = null;
            return false;
        }
    }
}
