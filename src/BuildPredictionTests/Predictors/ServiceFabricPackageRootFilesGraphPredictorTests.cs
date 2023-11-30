// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class ServiceFabricPackageRootFilesGraphPredictorTests
    {
        private readonly string _rootDir;

        public ServiceFabricPackageRootFilesGraphPredictorTests()
        {
            // Isolate each test into its own folder
            _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(ServiceFabricPackageRootFilesGraphPredictorTests), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootDir);
        }

        [Fact]
        public void FindItems()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.sfproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(ServiceFabricPackageRootFilesGraphPredictor.ServicePackageRootFolderPropertyName, "PackageRoot");
            projectRootElement.AddItem("ProjectReference", @"..\dep1\dep1.csproj");
            projectRootElement.AddItem("ProjectReference", @"..\dep2\dep2.csproj");
            projectRootElement.AddItem("ProjectReference", @"..\dep3\dep3.csproj");
            projectRootElement.AddItem("ProjectReference", @"..\dep4\dep4.csproj");
            projectRootElement.AddItem("ProjectReference", @"..\dep5\dep5.csproj");

            // Content package file
            string dependency1File = Path.Combine(_rootDir, @"dep1\dep1.csproj");
            ProjectRootElement dependency1RootElement = ProjectRootElement.Create(dependency1File);
            dependency1RootElement.AddItem(ContentItemsPredictor.ContentItemName, @"PackageRoot\Config\Settings.xml");

            // None package file
            string dependency2File = Path.Combine(_rootDir, @"dep2\dep2.csproj");
            ProjectRootElement dependency2RootElement = ProjectRootElement.Create(dependency2File);
            dependency2RootElement.AddItem(NoneItemsPredictor.NoneItemName, @"PackageRoot\Config\Settings.xml");

            // Linked package file
            string dependency3File = Path.Combine(_rootDir, @"dep3\dep3.csproj");
            ProjectRootElement dependency3RootElement = ProjectRootElement.Create(dependency3File);
            dependency3RootElement.AddItem(ContentItemsPredictor.ContentItemName, @"..\dep3_linked\PackageRoot\Config\Settings.xml")
                .AddMetadata("Link", @"PackageRoot\Config\Settings.xml");

            // Package file on disk
            string dependency4File = Path.Combine(_rootDir, @"dep4\dep4.csproj");
            ProjectRootElement dependency4RootElement = ProjectRootElement.Create(dependency4File);
            Directory.CreateDirectory(Path.Combine(_rootDir, @"dep4\PackageRoot\Config"));
            File.WriteAllText(Path.Combine(_rootDir, @"dep4\PackageRoot\Config\Settings.xml"), "dummy");

            // No package file
            string dependency5File = Path.Combine(_rootDir, @"dep5\dep5.csproj");
            ProjectRootElement dependency5RootElement = ProjectRootElement.Create(dependency5File);

            projectRootElement.Save();
            dependency1RootElement.Save();
            dependency2RootElement.Save();
            dependency3RootElement.Save();
            dependency4RootElement.Save();
            dependency5RootElement.Save();

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"dep1\PackageRoot\Config\Settings.xml", nameof(ServiceFabricPackageRootFilesGraphPredictor)),
                new PredictedItem(@"dep2\PackageRoot\Config\Settings.xml", nameof(ServiceFabricPackageRootFilesGraphPredictor)),
                new PredictedItem(@"dep3_linked\PackageRoot\Config\Settings.xml", nameof(ServiceFabricPackageRootFilesGraphPredictor)),
                new PredictedItem(@"dep4\PackageRoot\Config\Settings.xml", nameof(ServiceFabricPackageRootFilesGraphPredictor)),
            };
            new ServiceFabricPackageRootFilesGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertPredictions(
                    _rootDir,
                    expectedInputFiles,
                    null,
                    null,
                    null);
        }

        [Fact]
        public void SkipOtherProjectTypes()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement.AddProperty(ServiceFabricPackageRootFilesGraphPredictor.ServicePackageRootFolderPropertyName, "PackageRoot");
            projectRootElement.AddItem("ProjectReference", @"..\dep1\dep1.csproj");
            projectRootElement.AddItem("ProjectReference", @"..\dep2\dep2.csproj");
            projectRootElement.AddItem("ProjectReference", @"..\dep3\dep3.csproj");
            projectRootElement.AddItem("ProjectReference", @"..\dep4\dep4.csproj");

            // Content package file
            string dependency1File = Path.Combine(_rootDir, @"dep1\dep1.csproj");
            ProjectRootElement dependency1RootElement = ProjectRootElement.Create(dependency1File);
            dependency1RootElement.AddItem(ContentItemsPredictor.ContentItemName, @"PackageRoot\Config\Settings.xml");
            Directory.CreateDirectory(Path.Combine(_rootDir, @"dep1\PackageRoot"));

            // None package file
            string dependency2File = Path.Combine(_rootDir, @"dep2\dep2.csproj");
            ProjectRootElement dependency2RootElement = ProjectRootElement.Create(dependency2File);
            dependency2RootElement.AddItem(NoneItemsPredictor.NoneItemName, @"PackageRoot\Config\Settings.xml");
            Directory.CreateDirectory(Path.Combine(_rootDir, @"dep2\PackageRoot"));

            // Linked package file
            string dependency3File = Path.Combine(_rootDir, @"dep3\dep3.csproj");
            ProjectRootElement dependency3RootElement = ProjectRootElement.Create(dependency3File);
            dependency3RootElement.AddItem(ContentItemsPredictor.ContentItemName, @"..\dep3_linked\PackageRoot\Config\Settings.xml")
                .AddMetadata("Link", @"PackageRoot\Config\Settings.xml");
            Directory.CreateDirectory(Path.Combine(_rootDir, @"dep3\PackageRoot"));

            // Package file on disk
            string dependency4File = Path.Combine(_rootDir, @"dep4\dep4.csproj");
            ProjectRootElement dependency4RootElement = ProjectRootElement.Create(dependency4File);
            Directory.CreateDirectory(Path.Combine(_rootDir, @"dep4\PackageRoot\Config"));
            File.WriteAllText(Path.Combine(_rootDir, @"dep4\PackageRoot\Config\Settings.xml"), "dummy");

            projectRootElement.Save();
            dependency1RootElement.Save();
            dependency2RootElement.Save();
            dependency3RootElement.Save();
            dependency4RootElement.Save();

            new ServiceFabricPackageRootFilesGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertNoPredictions();
        }
    }
}