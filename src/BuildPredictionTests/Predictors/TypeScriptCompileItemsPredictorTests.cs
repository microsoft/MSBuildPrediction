// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class TypeScriptCompileItemsPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddItem(TypeScriptCompileItemsPredictor.TypeScriptCompileItemName, "Foo.ts");
            projectRootElement.AddItem(TypeScriptCompileItemsPredictor.TypeScriptCompileItemName, "Bar.ts");
            projectRootElement.AddItem(TypeScriptCompileItemsPredictor.TypeScriptCompileItemName, "Baz.ts");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("Foo.ts", nameof(TypeScriptCompileItemsPredictor)),
                new PredictedItem("Bar.ts", nameof(TypeScriptCompileItemsPredictor)),
                new PredictedItem("Baz.ts", nameof(TypeScriptCompileItemsPredictor)),
            };
            new TypeScriptCompileItemsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles,
                    null,
                    null,
                    null);
        }
    }
}
