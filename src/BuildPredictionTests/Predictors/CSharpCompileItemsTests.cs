// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    // TODO: Need to add .NET Core and .NET Framework based examples including use of SDK includes.
    public class CSharpCompileItemsTests
    {
        [Fact]
        public void CSharpFilesFoundFromDirectListingInCsproj()
        {
            Project project = CreateTestProject("Test.cs");
            ProjectInstance projectInstance = project.CreateProjectInstance(ProjectInstanceSettings.ImmutableWithFastItemLookup);
            var predictor = new CSharpCompileItems();
            bool hasPredictions = predictor.TryPredictInputsAndOutputs(project, projectInstance, out ProjectPredictions predictions);
            Assert.True(hasPredictions);
            predictions.AssertPredictions(new[] { new BuildInput(Path.Combine(project.DirectoryPath, "Test.cs"), isDirectory: false) }, null);
        }

        private static Project CreateTestProject(params string[] compileItemIncludes)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            foreach (string compileItemInclude in compileItemIncludes)
            {
                itemGroup.AddItem("Compile", compileItemInclude);
            }

            return TestHelpers.CreateProjectFromRootElement(projectRootElement);
        }
    }
}
