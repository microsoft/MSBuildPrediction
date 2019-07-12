// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class AvailableItemNameItemsPredictorTests
    {
        [Fact]
        public void AvailableItemNamesFindItems()
        {
            Project project = CreateTestProject(
                new[] { "Available1", "Available2" },
                Tuple.Create("Available1", "available1Value"),
                Tuple.Create("Available1", "available1Value2"),
                Tuple.Create("Available2", "available2Value"),
                Tuple.Create("NotAvailable", "shouldNotGetThisAsAnInput"));
            new AvailableItemNameItemsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    new[] { new PredictedItem("available1Value", nameof(AvailableItemNameItemsPredictor)), new PredictedItem("available1Value2", nameof(AvailableItemNameItemsPredictor)), new PredictedItem("available2Value", nameof(AvailableItemNameItemsPredictor)) },
                    null,
                    null,
                    null);
        }

        private static Project CreateTestProject(IEnumerable<string> availableItemNames, params Tuple<string, string>[] itemNamesAndValues)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();

            // Add Items.
            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            foreach (Tuple<string, string> itemNameAndValue in itemNamesAndValues)
            {
                itemGroup.AddItem(itemNameAndValue.Item1, itemNameAndValue.Item2);
            }

            // Add AvailableItemName items referring to the item names we'll add soon.
            ProjectItemGroupElement namesItemGroup = projectRootElement.AddItemGroup();
            foreach (string availableItemName in availableItemNames)
            {
                namesItemGroup.AddItem(AvailableItemNameItemsPredictor.AvailableItemName, availableItemName);
            }

            return TestHelpers.CreateProjectFromRootElement(projectRootElement);
        }
    }
}
