// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class GeneratePackageOnBuildPredictorTests
    {
        [Fact]
        public void Disabled()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageIdPropertyName, "SomePackage");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageVersionPropertyName, "1.2.3");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageOutputPathPropertyName, "bin");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.NuspecOutputPathPropertyName, "obj");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.SymbolPackageFormatPropertyName, "symbols.nupkg");
            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            new GeneratePackageOnBuildPredictor()
                .GetProjectPredictions(project)
                .AssertNoPredictions();
        }

        [Fact]
        public void GeneratedNuspecDefaultNuspecOutputPath()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.GeneratePackageOnBuildPropertyName, "true");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageIdPropertyName, "SomePackage");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageVersionPropertyName, "1.2.3");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.OutputPathPropertyName, "bin");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.NuspecOutputPathPropertyName, "obj");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.SymbolPackageFormatPropertyName, "symbols.nupkg");
            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\SomePackage.1.2.3.nuspec", nameof(GeneratePackageOnBuildPredictor)),
                new PredictedItem(@"bin\SomePackage.1.2.3.nupkg", nameof(GeneratePackageOnBuildPredictor)),
            };
            new GeneratePackageOnBuildPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    null,
                    null,
                    expectedOutputFiles,
                    null);
        }

        [Fact]
        public void GeneratedNuspecCustomNuspecOutputPath()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.GeneratePackageOnBuildPropertyName, "true");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageIdPropertyName, "SomePackage");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageVersionPropertyName, "1.2.3");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageOutputPathPropertyName, "bin");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.NuspecOutputPathPropertyName, "obj");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.SymbolPackageFormatPropertyName, "symbols.nupkg");
            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\SomePackage.1.2.3.nuspec", nameof(GeneratePackageOnBuildPredictor)),
                new PredictedItem(@"bin\SomePackage.1.2.3.nupkg", nameof(GeneratePackageOnBuildPredictor)),
            };
            new GeneratePackageOnBuildPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    null,
                    null,
                    expectedOutputFiles,
                    null);
        }

        [Fact]
        public void ProvidedNuspec()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.GeneratePackageOnBuildPropertyName, "true");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageIdPropertyName, "SomePackage");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageVersionPropertyName, "1.2.3");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.NuspecFilePropertyName, "SomePackage.nuspec");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageOutputPathPropertyName, "bin");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.NuspecOutputPathPropertyName, "obj");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.SymbolPackageFormatPropertyName, "symbols.nupkg");
            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"SomePackage.nuspec", nameof(GeneratePackageOnBuildPredictor)),
            };
            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"bin\SomePackage.1.2.3.nupkg", nameof(GeneratePackageOnBuildPredictor)),
            };
            new GeneratePackageOnBuildPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles,
                    null,
                    expectedOutputFiles,
                    null);
        }

        [Fact]
        public void IncludeSource()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.GeneratePackageOnBuildPropertyName, "true");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.IncludeSourcePropertyName, "true");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageIdPropertyName, "SomePackage");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageVersionPropertyName, "1.2.3");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageOutputPathPropertyName, "bin");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.NuspecOutputPathPropertyName, "obj");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.SymbolPackageFormatPropertyName, "symbols.nupkg");
            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\SomePackage.1.2.3.nuspec", nameof(GeneratePackageOnBuildPredictor)),
                new PredictedItem(@"obj\SomePackage.1.2.3.symbols.nuspec", nameof(GeneratePackageOnBuildPredictor)),
                new PredictedItem(@"bin\SomePackage.1.2.3.nupkg", nameof(GeneratePackageOnBuildPredictor)),
                new PredictedItem(@"bin\SomePackage.1.2.3.symbols.nupkg", nameof(GeneratePackageOnBuildPredictor)),
            };
            new GeneratePackageOnBuildPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    null,
                    null,
                    expectedOutputFiles,
                    null);
        }

        [Fact]
        public void IncludeSymbols()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.GeneratePackageOnBuildPropertyName, "true");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.IncludeSymbolsPropertyName, "true");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageIdPropertyName, "SomePackage");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageVersionPropertyName, "1.2.3");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageOutputPathPropertyName, "bin");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.NuspecOutputPathPropertyName, "obj");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.SymbolPackageFormatPropertyName, "symbols.nupkg");
            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\SomePackage.1.2.3.nuspec", nameof(GeneratePackageOnBuildPredictor)),
                new PredictedItem(@"obj\SomePackage.1.2.3.symbols.nuspec", nameof(GeneratePackageOnBuildPredictor)),
                new PredictedItem(@"bin\SomePackage.1.2.3.nupkg", nameof(GeneratePackageOnBuildPredictor)),
                new PredictedItem(@"bin\SomePackage.1.2.3.symbols.nupkg", nameof(GeneratePackageOnBuildPredictor)),
            };
            new GeneratePackageOnBuildPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    null,
                    null,
                    expectedOutputFiles,
                    null);
        }

        [Fact]
        public void Snupkg()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.GeneratePackageOnBuildPropertyName, "true");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.IncludeSymbolsPropertyName, "true");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageIdPropertyName, "SomePackage");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageVersionPropertyName, "1.2.3");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.PackageOutputPathPropertyName, "bin");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.NuspecOutputPathPropertyName, "obj");
            projectRootElement.AddProperty(GeneratePackageOnBuildPredictor.SymbolPackageFormatPropertyName, "snupkg");
            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\SomePackage.1.2.3.nuspec", nameof(GeneratePackageOnBuildPredictor)),
                new PredictedItem(@"obj\SomePackage.1.2.3.symbols.nuspec", nameof(GeneratePackageOnBuildPredictor)),
                new PredictedItem(@"bin\SomePackage.1.2.3.nupkg", nameof(GeneratePackageOnBuildPredictor)),
                new PredictedItem(@"bin\SomePackage.1.2.3.snupkg", nameof(GeneratePackageOnBuildPredictor)),
            };
            new GeneratePackageOnBuildPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    null,
                    null,
                    expectedOutputFiles,
                    null);
        }
    }
}
