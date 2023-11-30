// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class AvailableItemNameItemsPredictorTests
    {
        [Fact]
        public void AvailableItemNamesFindItems()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance(
                new[] { "Available1", "Available2" },
                Tuple.Create("Available1", "available1Value"),
                Tuple.Create("Available1", "available1Value2"),
                Tuple.Create("Available2", "available2Value"),
                Tuple.Create("NotAvailable", "shouldNotGetThisAsAnInput"));
            new AvailableItemNameItemsPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    new[] { new PredictedItem("available1Value", nameof(AvailableItemNameItemsPredictor)), new PredictedItem("available1Value2", nameof(AvailableItemNameItemsPredictor)), new PredictedItem("available2Value", nameof(AvailableItemNameItemsPredictor)) },
                    null,
                    null,
                    null);
        }

        private static ProjectInstance CreateTestProjectInstance(IEnumerable<string> availableItemNames, params Tuple<string, string>[] itemNamesAndValues)
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

            return TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);
        }
    }
}