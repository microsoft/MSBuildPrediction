// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Predicts inputs for Win32 manifest, ClickOnce application and deployment manifests, or a native manifest.
    /// </summary>
    /// <remarks>
    /// This logic is based on the GenerateManifests Target in Microsoft.Common.CurrentVersion.targets.
    /// See: https://github.com/microsoft/msbuild/blob/master/src/Tasks/Microsoft.Common.CurrentVersion.targets.
    /// </remarks>
    public sealed class ManifestsPredictor : IProjectPredictor
    {
        internal const string ApplicationManifestPropertyName = "ApplicationManifest";

        internal const string BaseApplicationManifestItemName = "BaseApplicationManifest";

        internal const string NoneItemName = "None";

        internal const string ManifestExtension = ".manifest";

        internal const string GenerateClickOnceManifestsPropertyName = "GenerateClickOnceManifests";

        internal const string OutputTypePropertyName = "OutputType";

        internal const string AssemblyNamePropertyName = "AssemblyName";

        internal const string TargetFileNamePropertyName = "TargetFileName";

        internal const string IntermediateOutputPathPropertyName = "IntermediateOutputPath";

        internal const string OutDirPropertyName = "OutDir";

        internal const string HostInBrowserPropertyName = "HostInBrowser";

        internal const string TargetZonePropertyName = "TargetZone";

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(ProjectInstance projectInstance, ProjectPredictionReporter predictionReporter)
        {
            // Non-ClickOnce applications use $(Win32Manifest) as an input to CSC, and the _SetEmbeddedWin32ManifestProperties target sets $(Win32Manifest) = $(ApplicationManifest).
            // ClickOnce applications don't use $(Win32Manifest) but do use $(ApplicationManifest) as an input anyway, so just always consider it an input.
            string applicationManifest = projectInstance.GetPropertyValue(ApplicationManifestPropertyName);
            if (!string.IsNullOrEmpty(applicationManifest))
            {
                predictionReporter.ReportInputFile(applicationManifest);
            }

            // ClickOnce applications
            if (projectInstance.GetPropertyValue(GenerateClickOnceManifestsPropertyName).Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase))
            {
                // $(_DeploymentBaseManifest) is an input to the GenerateApplicationManifest target/task, which in the _SetExternalWin32ManifestProperties
                // target is defined as $(ApplicationManifest) if it's set, which we've already predicted as an input above.
                if (string.IsNullOrEmpty(applicationManifest))
                {
                    // If $(ApplicationManifest) isn't set, $(_DeploymentBaseManifest) is set to @(_DeploymentBaseManifestWithTargetPath), which in the AssignTargetPaths target
                    // is set to @(BaseApplicationManifest) if any or @(None) items with the '.manifest' extension otherwise.
                    var baseApplicationManifests = projectInstance.GetItems(BaseApplicationManifestItemName);
                    if (baseApplicationManifests.Count > 0)
                    {
                        foreach (ProjectItemInstance item in baseApplicationManifests)
                        {
                            predictionReporter.ReportInputFile(item.EvaluatedInclude);
                        }
                    }
                    else
                    {
                        var none = projectInstance.GetItems(NoneItemName);
                        foreach (ProjectItemInstance item in none)
                        {
                            if (item.EvaluatedInclude.EndsWith(ManifestExtension, StringComparison.OrdinalIgnoreCase))
                            {
                                predictionReporter.ReportInputFile(item.EvaluatedInclude);
                            }
                        }
                    }
                }

                // Application manifest
                var applicationManifestName = projectInstance.GetPropertyValue(OutputTypePropertyName).Equals("library", StringComparison.OrdinalIgnoreCase)
                    ? "Native." + projectInstance.GetPropertyValue(AssemblyNamePropertyName) + ManifestExtension
                    : projectInstance.GetPropertyValue(TargetFileNamePropertyName) + ManifestExtension;
                predictionReporter.ReportOutputFile(projectInstance.GetPropertyValue(IntermediateOutputPathPropertyName) + applicationManifestName);
                predictionReporter.ReportOutputFile(projectInstance.GetPropertyValue(OutDirPropertyName) + applicationManifestName);

                // Deployment manifest
                var deploymentManifestName = projectInstance.GetPropertyValue(HostInBrowserPropertyName).Equals(bool.TrueString, StringComparison.OrdinalIgnoreCase)
                    ? projectInstance.GetPropertyValue(AssemblyNamePropertyName) + ".xbap"
                    : projectInstance.GetPropertyValue(AssemblyNamePropertyName) + ".application";
                predictionReporter.ReportOutputFile(projectInstance.GetPropertyValue(IntermediateOutputPathPropertyName) + deploymentManifestName);
                predictionReporter.ReportOutputFile(projectInstance.GetPropertyValue(OutDirPropertyName) + deploymentManifestName);

                // Intermediate Trust info file
                if (!string.IsNullOrEmpty(projectInstance.GetPropertyValue(TargetZonePropertyName)))
                {
                    predictionReporter.ReportOutputFile(projectInstance.GetPropertyValue(IntermediateOutputPathPropertyName) + projectInstance.GetPropertyValue(TargetFileNamePropertyName) + ".TrustInfo.xml");
                }
            }
        }
    }
}
