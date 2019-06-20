// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class AzureCloudServicePredictorTests
    {
        [Fact]
        public void FindItems()
        {
            Project project = CreateTestProject("project.ccproj");
            var expectedInputFiles = new[]
            {
                new PredictedItem("ServiceDefinition.csdef", nameof(AzureCloudServicePredictor)),
                new PredictedItem("ServiceConfiguration.Local.cscfg", nameof(AzureCloudServicePredictor)),
                new PredictedItem("ServiceConfiguration.Prod.cscfg", nameof(AzureCloudServicePredictor)),
            };
            new AzureCloudServicePredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles,
                    null,
                    null,
                    null);
        }

        [Fact]
        public void SkipOtherProjectTypes()
        {
            Project project = CreateTestProject("project.csproj");
            new AzureCloudServicePredictor()
                .GetProjectPredictions(project)
                .AssertNoPredictions();
        }

        private static Project CreateTestProject(string fileName)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(fileName);
            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            itemGroup.AddItem(AzureCloudServicePredictor.ServiceDefinitionItemName, "ServiceDefinition.csdef");
            itemGroup.AddItem(AzureCloudServicePredictor.ServiceConfigurationItemName, "ServiceConfiguration.Local.cscfg");
            itemGroup.AddItem(AzureCloudServicePredictor.ServiceConfigurationItemName, "ServiceConfiguration.Prod.cscfg");

            return TestHelpers.CreateProjectFromRootElement(projectRootElement);
        }
    }
}
