// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class ServiceFabricServiceManifestPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance("project.sfproj");
            var expectedInputFiles = new[]
            {
                new PredictedItem($@"Service1\PackageRoot\{ServiceFabricServiceManifestPredictor.ServiceManifestFileName}", nameof(ServiceFabricServiceManifestPredictor)),
                new PredictedItem($@"Service2\PackageRoot\{ServiceFabricServiceManifestPredictor.ServiceManifestFileName}", nameof(ServiceFabricServiceManifestPredictor)),
                new PredictedItem($@"ServiceFabricApp\ApplicationPackageRoot\Foo\{ServiceFabricServiceManifestPredictor.ServiceManifestFileName}", nameof(ServiceFabricServiceManifestPredictor)),
                new PredictedItem($@"ServiceFabricApp\ApplicationPackageRoot\Bar\{ServiceFabricServiceManifestPredictor.ServiceManifestFileName}", nameof(ServiceFabricServiceManifestPredictor)),
            };
            new ServiceFabricServiceManifestPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles.MakeAbsolute(Directory.GetCurrentDirectory()),
                    null,
                    null,
                    null);
        }

        [Fact]
        public void UpdateServiceFabricApplicationManifestDisabled()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance("project.sfproj", isEnabled: false);
            new ServiceFabricServiceManifestPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        [Fact]
        public void NoProjectReferences()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance("project.sfproj", hasProjectReferences: false);
            new ServiceFabricServiceManifestPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        [Fact]
        public void SkipOtherProjectTypes()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance("project.csproj");
            new ServiceFabricServiceManifestPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        private static ProjectInstance CreateTestProjectInstance(string fileName, bool hasProjectReferences = true, bool isEnabled = true)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create($@"ServiceFabricApp\{fileName}");

            // These are generally set in Microsoft.VisualStudio.Azure.Fabric.Application.targets
            ProjectPropertyGroupElement propertyGroup = projectRootElement.AddPropertyGroup();
            propertyGroup.AddProperty(ServiceFabricServiceManifestPredictor.ApplicationPackageRootFolderPropertyName, "ApplicationPackageRoot");
            propertyGroup.AddProperty(ServiceFabricServiceManifestPredictor.ServicePackageRootFolderPropertyName, "PackageRoot");
            if (isEnabled)
            {
                propertyGroup.AddProperty(ServiceFabricServiceManifestPredictor.UpdateServiceFabricApplicationManifestEnabledPropertyName, "true");
            }

            if (hasProjectReferences)
            {
                ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
                itemGroup.AddItem(ServiceFabricServiceManifestPredictor.ProjectReferenceItemName, @"..\Service1\Service1.csproj");
                itemGroup.AddItem(ServiceFabricServiceManifestPredictor.ProjectReferenceItemName, @"..\Service2\Service2.csproj");
            }

            // Extra service manifests, and some extraneous files too
            Directory.CreateDirectory(@"ServiceFabricApp\ApplicationPackageRoot");
            File.WriteAllText($@"ServiceFabricApp\ApplicationPackageRoot\{ServiceFabricServiceManifestPredictor.ServiceManifestFileName}", "SomeContent"); // Not in a subdir, should not get picked up
            File.WriteAllText($@"ServiceFabricApp\ApplicationPackageRoot\extraneous.txt", "SomeContent");
            Directory.CreateDirectory(@"ServiceFabricApp\ApplicationPackageRoot\Foo");
            File.WriteAllText($@"ServiceFabricApp\ApplicationPackageRoot\Foo\{ServiceFabricServiceManifestPredictor.ServiceManifestFileName}", "SomeContent");
            File.WriteAllText($@"ServiceFabricApp\ApplicationPackageRoot\Foo\extraneous.txt", "SomeContent");
            Directory.CreateDirectory(@"ServiceFabricApp\ApplicationPackageRoot\Bar");
            File.WriteAllText($@"ServiceFabricApp\ApplicationPackageRoot\Bar\{ServiceFabricServiceManifestPredictor.ServiceManifestFileName}", "SomeContent");
            File.WriteAllText($@"ServiceFabricApp\ApplicationPackageRoot\Bar\extraneous.txt", "SomeContent");

            return TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);
        }
    }
}
