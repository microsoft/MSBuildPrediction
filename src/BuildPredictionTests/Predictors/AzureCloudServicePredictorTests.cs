// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class AzureCloudServicePredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance("project.ccproj");
            var expectedInputFiles = new[]
            {
                new PredictedItem("ServiceDefinition.csdef", nameof(AzureCloudServicePredictor)),
                new PredictedItem("ServiceConfiguration.Local.cscfg", nameof(AzureCloudServicePredictor)),
                new PredictedItem("ServiceConfiguration.Prod.cscfg", nameof(AzureCloudServicePredictor)),
            };
            new AzureCloudServicePredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles,
                    null,
                    null,
                    null);
        }

        [Fact]
        public void SkipOtherProjectTypes()
        {
            ProjectInstance projectInstance = CreateTestProjectInstance("project.csproj");
            new AzureCloudServicePredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }

        private static ProjectInstance CreateTestProjectInstance(string fileName)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(fileName);
            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            itemGroup.AddItem(AzureCloudServicePredictor.ServiceDefinitionItemName, "ServiceDefinition.csdef");
            itemGroup.AddItem(AzureCloudServicePredictor.ServiceConfigurationItemName, "ServiceConfiguration.Local.cscfg");
            itemGroup.AddItem(AzureCloudServicePredictor.ServiceConfigurationItemName, "ServiceConfiguration.Prod.cscfg");

            return TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);
        }
    }
}