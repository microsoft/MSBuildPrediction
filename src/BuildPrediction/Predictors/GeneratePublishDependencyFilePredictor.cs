// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors;

/// <summary>
/// Makes predictions based on the GeneratePublishDependencyFile target.
/// </summary>
public sealed class GeneratePublishDependencyFilePredictor : IProjectPredictor
{
    internal const string PublishAotPropertyName = "PublishAot";
    internal const string PublishDirPropertyName = "PublishDir";
    internal const string PublishSingleFilePropertyName = "PublishSingleFile";
    internal const string SelfContainedPropertyName = "SelfContained";
    internal const string PreserveStoreLayoutPropertyName = "PreserveStoreLayout";
    internal const string PublishTrimmedPropertyName = "PublishTrimmed";
    internal const string RuntimeStorePackagesItemName = "RuntimeStorePackages";
    internal const string PackageReferenceItemName = "PackageReference";
    internal const string PrivateAssetsMetadataName = "PrivateAssets";
    internal const string PublishMetadataName = "Publish";
    internal const string PublishDepsFilePathPropertyName = "PublishDepsFilePath";
    internal const string ProjectDepsFileNamePropertyName = "ProjectDepsFileName";
    internal const string IntermediateOutputPathPropertyName = "IntermediateOutputPath";

    /// <inheritdoc/>
    public void PredictInputsAndOutputs(
        ProjectInstance projectInstance,
        ProjectPredictionReporter predictionReporter)
    {
        if (!projectInstance.GetPropertyValue(GenerateBuildDependencyFilePredictor.GenerateDependencyFilePropertyName).Equals("true", StringComparison.OrdinalIgnoreCase)
            || ShouldUseBuildDependencyFile(projectInstance)
            || projectInstance.GetPropertyValue(PublishAotPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        string projectAssetsFilePropertyName = projectInstance.GetPropertyValue(GenerateBuildDependencyFilePredictor.ProjectAssetsFilePropertyName);
        if (!string.IsNullOrEmpty(projectAssetsFilePropertyName))
        {
            predictionReporter.ReportInputFile(projectAssetsFilePropertyName);
        }

        string publishDepsFilePath = GetEffectivePublishDepsFilePath(projectInstance);
        string intermediateDepsFilePath = !string.IsNullOrEmpty(publishDepsFilePath)
            ? publishDepsFilePath
            : projectInstance.GetPropertyValue(IntermediateOutputPathPropertyName) + projectInstance.GetPropertyValue(ProjectDepsFileNamePropertyName);
        if (!string.IsNullOrEmpty(intermediateDepsFilePath))
        {
            predictionReporter.ReportOutputFile(intermediateDepsFilePath);
        }

        // Note: GetCopyToPublishDirectoryItemsGraphPredictor will predict the final (published) location for the publish deps file since that's the target which does that copy.
    }

    /// <summary>
    /// Determines the value of _UseBuildDependencyFile by emulating the behavior from the _ComputeUseBuildDependencyFile target (and the _ComputePackageReferencePublish target).
    /// </remarks>
    internal static bool ShouldUseBuildDependencyFile(ProjectInstance projectInstance)
    {
        bool hasExcludeFromPublishPackageReference = false;
        foreach (ProjectItemInstance packageReference in projectInstance.GetItems(PackageReferenceItemName))
        {
            string packageReferencePublishMetadata = packageReference.GetMetadataValue(PublishMetadataName);
            if (packageReferencePublishMetadata.Equals("false", StringComparison.OrdinalIgnoreCase))
            {
                hasExcludeFromPublishPackageReference = true;
                break;
            }

            if (string.IsNullOrEmpty(packageReferencePublishMetadata)
                && packageReference.GetMetadataValue(PrivateAssetsMetadataName).Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                hasExcludeFromPublishPackageReference = true;
                break;
            }
        }

        bool trimRuntimeAssets = projectInstance.GetPropertyValue(PublishSingleFilePropertyName).Equals("true", StringComparison.OrdinalIgnoreCase)
            && projectInstance.GetPropertyValue(SelfContainedPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase);
        return !hasExcludeFromPublishPackageReference
            && projectInstance.GetItems(RuntimeStorePackagesItemName).Count == 0
            && !projectInstance.GetPropertyValue(PreserveStoreLayoutPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase)
            && !projectInstance.GetPropertyValue(PublishTrimmedPropertyName).Equals("true", StringComparison.OrdinalIgnoreCase)
            && !trimRuntimeAssets;
    }

    /// <summary>
    /// Calculates the effective value of $(PublishDepsFilePath). In unspecified, the default value is calculated inside the GeneratePublishDependencyFile target.
    /// </summary>
    /// <remarks>
    /// This can return null in the case of PublishSingleFile since the deps.json file is embedded within the single-file bundle.
    /// </remarks>
    internal static string GetEffectivePublishDepsFilePath(ProjectInstance projectInstance)
    {
        string publishDepsFilePath = projectInstance.GetPropertyValue(PublishDepsFilePathPropertyName);
        if (!string.IsNullOrEmpty(publishDepsFilePath))
        {
            return publishDepsFilePath;
        }

        if (!projectInstance.GetPropertyValue(PublishSingleFilePropertyName).Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            return projectInstance.GetPropertyValue(PublishDirPropertyName) + projectInstance.GetPropertyValue(ProjectDepsFileNamePropertyName);
        }

        return null;
    }
}