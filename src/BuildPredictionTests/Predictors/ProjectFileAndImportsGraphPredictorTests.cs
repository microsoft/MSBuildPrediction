// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using System;
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class ProjectFileAndImportsGraphPredictorTests
    {
        private readonly string _rootDir;

        public ProjectFileAndImportsGraphPredictorTests()
        {
            // Isolate each test into its own folder
            _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(ArtifactsSdkPredictorTests), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootDir);
        }

        [Fact]
        public void FindItems()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);

            ProjectRootElement dep1 = CreateDependencyProject("dep1");
            ProjectRootElement dep2 = CreateDependencyProject("dep2");
            ProjectRootElement dep3 = CreateDependencyProject("dep3");
            ProjectRootElement dep4 = CreateDependencyProject("dep4");

            // The main project depends on 1 and 2; 2 depends on 3; 3 depends on 1 and 4.
            // This tests both transitivity and deduping.
            projectRootElement.AddItem("ProjectReference", @"..\dep1\dep1.proj");
            projectRootElement.AddItem("ProjectReference", @"..\dep2\dep2.proj");
            dep2.AddItem("ProjectReference", @"..\dep3\dep3.proj");
            dep3.AddItem("ProjectReference", @"..\dep1\dep1.proj");
            dep3.AddItem("ProjectReference", @"..\dep4\dep4.proj");

            projectRootElement.Save();
            dep1.Save();
            dep2.Save();
            dep3.Save();
            dep4.Save();

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"dep1\dep1.proj", nameof(ProjectFileAndImportsGraphPredictor)),
                new PredictedItem(@"dep1\dep1.targets", nameof(ProjectFileAndImportsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.proj", nameof(ProjectFileAndImportsGraphPredictor)),
                new PredictedItem(@"dep2\dep2.targets", nameof(ProjectFileAndImportsGraphPredictor)),
                new PredictedItem(@"dep3\dep3.proj", nameof(ProjectFileAndImportsGraphPredictor)),
                new PredictedItem(@"dep3\dep3.targets", nameof(ProjectFileAndImportsGraphPredictor)),
                new PredictedItem(@"dep4\dep4.proj", nameof(ProjectFileAndImportsGraphPredictor)),
                new PredictedItem(@"dep4\dep4.targets", nameof(ProjectFileAndImportsGraphPredictor)),
            };

            new ProjectFileAndImportsGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertPredictions(
                    _rootDir,
                    expectedInputFiles,
                    null,
                    null,
                    null);
        }

        private ProjectRootElement CreateDependencyProject(string projectName)
        {
            string projectDir = Path.Combine(_rootDir, projectName);
            Directory.CreateDirectory(projectDir);

            string projectFileName = projectName + ".proj";
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(projectDir, projectFileName));

            string importFileName = projectName + ".targets";
            ProjectRootElement importRootElement = ProjectRootElement.Create(Path.Combine(projectDir, importFileName));
            importRootElement.Save();

            projectRootElement.AddImport(importFileName);

            // The caller may modify the returned project, so don't save it yet.
            return projectRootElement;
        }
    }
}
