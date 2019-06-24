// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class ServiceFabricServiceManifestPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            Project project = CreateTestProject("project.sfproj");
            var expectedInputFiles = new[]
            {
                new PredictedItem($@"Service1\PackageRoot\{ServiceFabricServiceManifestPredictor.ServiceManifestFileName}", nameof(ServiceFabricServiceManifestPredictor)),
                new PredictedItem($@"Service2\PackageRoot\{ServiceFabricServiceManifestPredictor.ServiceManifestFileName}", nameof(ServiceFabricServiceManifestPredictor)),
                new PredictedItem($@"ServiceFabricApp\ApplicationPackageRoot\Foo\{ServiceFabricServiceManifestPredictor.ServiceManifestFileName}", nameof(ServiceFabricServiceManifestPredictor)),
                new PredictedItem($@"ServiceFabricApp\ApplicationPackageRoot\Bar\{ServiceFabricServiceManifestPredictor.ServiceManifestFileName}", nameof(ServiceFabricServiceManifestPredictor)),
            };
            new ServiceFabricServiceManifestPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(Directory.GetCurrentDirectory()),
                    null,
                    null,
                    null);
        }

        [Fact]
        public void SkipOtherProjectTypes()
        {
            Project project = CreateTestProject("project.csproj");
            new ServiceFabricServiceManifestPredictor()
                .GetProjectPredictions(project)
                .AssertNoPredictions();
        }

        private static Project CreateTestProject(string fileName)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create($@"ServiceFabricApp\{fileName}");

            // These are generally set in Microsoft.VisualStudio.Azure.Fabric.Application.targets
            ProjectPropertyGroupElement propertyGroup = projectRootElement.AddPropertyGroup();
            propertyGroup.AddProperty(ServiceFabricServiceManifestPredictor.ApplicationPackageRootFolderPropertyName, "ApplicationPackageRoot");
            propertyGroup.AddProperty(ServiceFabricServiceManifestPredictor.ServicePackageRootFolderPropertyName, "PackageRoot");

            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            itemGroup.AddItem(ServiceFabricServiceManifestPredictor.ProjectReferenceItemName, @"..\Service1\Service1.csproj");
            itemGroup.AddItem(ServiceFabricServiceManifestPredictor.ProjectReferenceItemName, @"..\Service2\Service2.csproj");

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

            return TestHelpers.CreateProjectFromRootElement(projectRootElement);
        }
    }
}
