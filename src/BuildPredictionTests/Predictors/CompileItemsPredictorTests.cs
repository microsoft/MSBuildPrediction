// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class CompileItemsPredictorTests
    {
        [Fact]
        public void CSharpFilesFoundFromDirectListingInCsproj()
        {
            Project project = CreateTestProject("Test.cs");
            new CompileItemsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    new[] { new PredictedItem("Test.cs", nameof(CompileItemsPredictor)) },
                    null,
                    null,
                    null);
        }

        private static Project CreateTestProject(params string[] compileItemIncludes)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            foreach (string compileItemInclude in compileItemIncludes)
            {
                itemGroup.AddItem(CompileItemsPredictor.CompileItemName, compileItemInclude);
            }

            return TestHelpers.CreateProjectFromRootElement(projectRootElement);
        }
    }
}
