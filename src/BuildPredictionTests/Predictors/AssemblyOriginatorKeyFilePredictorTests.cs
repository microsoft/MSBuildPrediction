// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class AssemblyOriginatorKeyFilePredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create("project.csproj");
            projectRootElement.AddProperty(AssemblyOriginatorKeyFilePredictor.SignAssemblyPropertyName, "true");
            projectRootElement.AddProperty(AssemblyOriginatorKeyFilePredictor.AssemblyOriginatorKeyFilePropertyName, "StrongNaming.snk");
            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("StrongNaming.snk", nameof(AssemblyOriginatorKeyFilePredictor)),
            };
            new AssemblyOriginatorKeyFilePredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(Directory.GetCurrentDirectory()),
                    null,
                    null,
                    null);
        }

        [Fact]
        public void SigningDisabled()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create("project.csproj");
            projectRootElement.AddProperty(AssemblyOriginatorKeyFilePredictor.AssemblyOriginatorKeyFilePropertyName, "StrongNaming.snk");
            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            new AssemblyOriginatorKeyFilePredictor()
                .GetProjectPredictions(project)
                .AssertNoPredictions();
        }

        [Fact]
        public void NoKeyFile()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create("project.csproj");
            projectRootElement.AddProperty(AssemblyOriginatorKeyFilePredictor.SignAssemblyPropertyName, "true");
            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            new AssemblyOriginatorKeyFilePredictor()
                .GetProjectPredictions(project)
                .AssertNoPredictions();
        }
    }
}
