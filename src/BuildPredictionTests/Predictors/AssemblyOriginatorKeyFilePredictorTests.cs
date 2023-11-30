// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class AssemblyOriginatorKeyFilePredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create("project.csproj");
            projectRootElement.AddProperty(AssemblyOriginatorKeyFilePredictor.SignAssemblyPropertyName, "true");
            projectRootElement.AddProperty(AssemblyOriginatorKeyFilePredictor.AssemblyOriginatorKeyFilePropertyName, "StrongNaming.snk");
            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("StrongNaming.snk", nameof(AssemblyOriginatorKeyFilePredictor)),
            };
            new AssemblyOriginatorKeyFilePredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles,
                    null,
                    null,
                    null);
        }

        [Fact]
        public void SigningDisabled()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create("project.csproj");
            projectRootElement.AddProperty(AssemblyOriginatorKeyFilePredictor.AssemblyOriginatorKeyFilePropertyName, "StrongNaming.snk");
            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            new AssemblyOriginatorKeyFilePredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        [Fact]
        public void NoKeyFile()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create("project.csproj");
            projectRootElement.AddProperty(AssemblyOriginatorKeyFilePredictor.SignAssemblyPropertyName, "true");
            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            new AssemblyOriginatorKeyFilePredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }
    }
}