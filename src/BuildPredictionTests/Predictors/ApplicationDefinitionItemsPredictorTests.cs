﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class ApplicationDefinitionItemsPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            itemGroup.AddItem(ApplicationDefinitionItemsPredictor.ApplicationDefinitionItemName, "App.xaml");
            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            new ApplicationDefinitionItemsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    new[] { new PredictedItem("App.xaml", nameof(ApplicationDefinitionItemsPredictor)) },
                    null,
                    null,
                    null);
        }
    }
}
