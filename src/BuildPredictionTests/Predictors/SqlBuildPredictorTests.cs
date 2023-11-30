// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class SqlBuildPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create("project.sqlproj");
            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            itemGroup.AddItem(SqlBuildPredictor.BuildItemName, "Build1.sql");
            itemGroup.AddItem(SqlBuildPredictor.BuildItemName, "Build2.sql");
            itemGroup.AddItem(SqlBuildPredictor.BuildItemName, "Build3.sql");
            itemGroup.AddItem(SqlBuildPredictor.PreDeployItemName, "PreDeploy1.sql");
            itemGroup.AddItem(SqlBuildPredictor.PreDeployItemName, "PreDeploy2.sql");
            itemGroup.AddItem(SqlBuildPredictor.PreDeployItemName, "PreDeploy3.sql");
            itemGroup.AddItem(SqlBuildPredictor.PostDeployItemName, "PostDeploy1.sql");
            itemGroup.AddItem(SqlBuildPredictor.PostDeployItemName, "PostDeploy2.sql");
            itemGroup.AddItem(SqlBuildPredictor.PostDeployItemName, "PostDeploy3.sql");
            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem("Build1.sql", nameof(SqlBuildPredictor)),
                new PredictedItem("Build2.sql", nameof(SqlBuildPredictor)),
                new PredictedItem("Build3.sql", nameof(SqlBuildPredictor)),
                new PredictedItem("PreDeploy1.sql", nameof(SqlBuildPredictor)),
                new PredictedItem("PreDeploy2.sql", nameof(SqlBuildPredictor)),
                new PredictedItem("PreDeploy3.sql", nameof(SqlBuildPredictor)),
                new PredictedItem("PostDeploy1.sql", nameof(SqlBuildPredictor)),
                new PredictedItem("PostDeploy2.sql", nameof(SqlBuildPredictor)),
                new PredictedItem("PostDeploy3.sql", nameof(SqlBuildPredictor)),
            };

            new SqlBuildPredictor()
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
            ProjectRootElement projectRootElement = ProjectRootElement.Create("project.csproj");
            ProjectItemGroupElement itemGroup = projectRootElement.AddItemGroup();
            itemGroup.AddItem(SqlBuildPredictor.BuildItemName, "Build1.sql");
            itemGroup.AddItem(SqlBuildPredictor.BuildItemName, "Build2.sql");
            itemGroup.AddItem(SqlBuildPredictor.BuildItemName, "Build3.sql");
            itemGroup.AddItem(SqlBuildPredictor.PreDeployItemName, "PreDeploy1.sql");
            itemGroup.AddItem(SqlBuildPredictor.PreDeployItemName, "PreDeploy2.sql");
            itemGroup.AddItem(SqlBuildPredictor.PreDeployItemName, "PreDeploy3.sql");
            itemGroup.AddItem(SqlBuildPredictor.PostDeployItemName, "PostDeploy1.sql");
            itemGroup.AddItem(SqlBuildPredictor.PostDeployItemName, "PostDeploy2.sql");
            itemGroup.AddItem(SqlBuildPredictor.PostDeployItemName, "PostDeploy3.sql");
            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            new SqlBuildPredictor()
                .GetProjectPredictions(projectInstance)
                .AssertNoPredictions();
        }
    }
}