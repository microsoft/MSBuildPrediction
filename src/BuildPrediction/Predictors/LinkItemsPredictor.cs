// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors;

/// <summary>
/// Predicts inputs based on Link, Lib, and ImpLib items.
/// </summary>
public sealed class LinkItemsPredictor : IProjectPredictor
{
    // See Microsoft.CppCommon.targets for items which use AdditionalDependencies.
    internal const string LinkItemName = "Link";
    internal const string LibItemName = "Lib";
    internal const string ImpLibItemName = "ImpLib";

    internal const string AdditionalDependenciesMetadata = "AdditionalDependencies";
    internal const string AdditionalLibraryDirectoriesMetadata = "AdditionalLibraryDirectories";

    private static readonly char[] IncludePathsSeparator = [';'];

    /// <inheritdoc />
    public void PredictInputsAndOutputs(ProjectInstance projectInstance, ProjectPredictionReporter reporter)
    {
        // This predictor only applies to vcxproj files
        if (!projectInstance.FullPath.EndsWith(".vcxproj", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // These are commonly added via an ItemDefinitionGroup, so use a HashSet to dedupe the repeats.
        HashSet<string> reportedFiles = new(StringComparer.OrdinalIgnoreCase);
        HashSet<string> reportedDirectories = new(StringComparer.OrdinalIgnoreCase);

        ReportInputsForItemType(reporter, projectInstance, LinkItemName, reportedFiles, reportedDirectories);
        ReportInputsForItemType(reporter, projectInstance, LibItemName, reportedFiles, reportedDirectories);
        ReportInputsForItemType(reporter, projectInstance, ImpLibItemName, reportedFiles, reportedDirectories);
    }

    private void ReportInputsForItemType(
        ProjectPredictionReporter reporter,
        ProjectInstance projectInstance,
        string itemType,
        HashSet<string> reportedFiles,
        HashSet<string> reportedDirectories)
    {
        ICollection<ProjectItemInstance> items = projectInstance.GetItems(itemType);
        if (items.Count == 0)
        {
            return;
        }

        foreach (ProjectItemInstance item in items)
        {
            reporter.ReportInputFile(item.EvaluatedInclude);

            string[] additionalDependencies = item.GetMetadataValue(AdditionalDependenciesMetadata)
                .Split(IncludePathsSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (string dependency in additionalDependencies)
            {
                string trimmedDependency = dependency.Trim();
                if (!string.IsNullOrEmpty(trimmedDependency) && reportedFiles.Add(trimmedDependency))
                {
                    reporter.ReportInputFile(trimmedDependency);
                }
            }

            string[] additionalLibraryDirectories = item.GetMetadataValue(AdditionalLibraryDirectoriesMetadata)
                .Split(IncludePathsSeparator, StringSplitOptions.RemoveEmptyEntries);
            foreach (string directory in additionalLibraryDirectories)
            {
                string trimmedDirectory = directory.Trim();
                if (!string.IsNullOrEmpty(trimmedDirectory) && reportedDirectories.Add(trimmedDirectory))
                {
                    reporter.ReportInputDirectory(trimmedDirectory);
                }
            }
        }
    }
}