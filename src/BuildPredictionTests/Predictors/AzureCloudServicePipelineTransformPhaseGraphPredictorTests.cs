// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using System;
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class AzureCloudServicePipelineTransformPhaseGraphPredictorTests
    {
        private readonly string _rootDir;

        public AzureCloudServicePipelineTransformPhaseGraphPredictorTests()
        {
            // Isolate each test into its own folder
            _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(AzureCloudServicePipelineTransformPhaseGraphPredictorTests), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootDir);
        }

        [Fact]
        public void FindItems()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.ccproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement
                .AddItem("ProjectReference", @"..\WebRole1\WebRole1.csproj")
                .AddMetadata(AzureCloudServicePipelineTransformPhaseGraphPredictor.RoleTypeMetadataName, "Web");
            projectRootElement
                .AddItem("ProjectReference", @"..\WebRole2\WebRole2.csproj")
                .AddMetadata(AzureCloudServicePipelineTransformPhaseGraphPredictor.RoleTypeMetadataName, "Web");
            projectRootElement
                .AddItem("ProjectReference", @"..\WebRole3\WebRole3.csproj")
                .AddMetadata(AzureCloudServicePipelineTransformPhaseGraphPredictor.RoleTypeMetadataName, "Web");
            projectRootElement
                .AddItem("ProjectReference", @"..\WorkerRole\WorkerRole.csproj")
                .AddMetadata(AzureCloudServicePipelineTransformPhaseGraphPredictor.RoleTypeMetadataName, "Worker");
            projectRootElement
                .AddItem("ProjectReference", @"..\NoRole\NoRole.csproj");

            // WebRole with content items
            string webRole1File = Path.Combine(_rootDir, @"WebRole1\WebRole1.csproj");
            ProjectRootElement webRole1RootElement = ProjectRootElement.Create(webRole1File);
            webRole1RootElement.AddItem(ContentItemsPredictor.ContentItemName, "content1.txt");
            webRole1RootElement.AddItem(ContentItemsPredictor.ContentItemName, "content2.txt");
            webRole1RootElement.AddItem(ContentItemsPredictor.ContentItemName, "content3.txt");

            // WebRole with reference items
            string webRole2File = Path.Combine(_rootDir, @"WebRole2\WebRole2.csproj");
            ProjectRootElement webRole2RootElement = ProjectRootElement.Create(webRole2File);
            webRole2RootElement
                .AddItem(ReferenceItemsPredictor.ReferenceItemName, "Reference1")
                .AddMetadata(ReferenceItemsPredictor.HintPathMetadata, @"..\packages\Package1\lib\net45\Reference1.dll");
            webRole2RootElement
                .AddItem(ReferenceItemsPredictor.ReferenceItemName, @"..\packages\Package2\lib\net45\Reference2.dll")
                .AddMetadata("Name", "Reference2");

            // WebRole with TypeScriptCompile items
            string webRole3File = Path.Combine(_rootDir, @"WebRole3\WebRole3.csproj");
            ProjectRootElement webRole3RootElement = ProjectRootElement.Create(webRole3File);
            webRole3RootElement.AddItem(TypeScriptCompileItemsPredictor.TypeScriptCompileItemName, "script1.ts");
            webRole3RootElement.AddItem(TypeScriptCompileItemsPredictor.TypeScriptCompileItemName, "script2.ts");
            webRole3RootElement.AddItem(TypeScriptCompileItemsPredictor.TypeScriptCompileItemName, "script3.ts");

            // WorkerRole. Should have no predictions
            string workerRoleFile = Path.Combine(_rootDir, @"WorkerRole\WorkerRole.csproj");
            ProjectRootElement workerRoleRootElement = ProjectRootElement.Create(workerRoleFile);
            workerRoleRootElement.AddItem(ContentItemsPredictor.ContentItemName, "content.txt");
            workerRoleRootElement
                .AddItem(ReferenceItemsPredictor.ReferenceItemName, @"..\packages\Package\lib\net45\Reference.dll")
                .AddMetadata("Name", "Reference");

            // No role type. Should have no predictions
            string noRoleFile = Path.Combine(_rootDir, @"NoRole\NoRole.csproj");
            ProjectRootElement noRoleRootElement = ProjectRootElement.Create(noRoleFile);
            noRoleRootElement.AddItem(ContentItemsPredictor.ContentItemName, "content.txt");
            noRoleRootElement
                .AddItem(ReferenceItemsPredictor.ReferenceItemName, @"..\packages\Package\lib\net45\Reference.dll")
                .AddMetadata("Name", "Reference");

            projectRootElement.Save();
            webRole1RootElement.Save();
            webRole2RootElement.Save();
            webRole3RootElement.Save();
            workerRoleRootElement.Save();
            noRoleRootElement.Save();

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"WebRole1\content1.txt", nameof(AzureCloudServicePipelineTransformPhaseGraphPredictor)),
                new PredictedItem(@"WebRole1\content2.txt", nameof(AzureCloudServicePipelineTransformPhaseGraphPredictor)),
                new PredictedItem(@"WebRole1\content3.txt", nameof(AzureCloudServicePipelineTransformPhaseGraphPredictor)),
                new PredictedItem(@"packages\Package1\lib\net45\Reference1.dll", nameof(AzureCloudServicePipelineTransformPhaseGraphPredictor)),
                new PredictedItem(@"packages\Package2\lib\net45\Reference2.dll", nameof(AzureCloudServicePipelineTransformPhaseGraphPredictor)),
                new PredictedItem(@"WebRole3\script1.ts", nameof(AzureCloudServicePipelineTransformPhaseGraphPredictor)),
                new PredictedItem(@"WebRole3\script2.ts", nameof(AzureCloudServicePipelineTransformPhaseGraphPredictor)),
                new PredictedItem(@"WebRole3\script3.ts", nameof(AzureCloudServicePipelineTransformPhaseGraphPredictor)),
            };
            new AzureCloudServicePipelineTransformPhaseGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertPredictions(
                    _rootDir,
                    expectedInputFiles,
                    null,
                    null,
                    null);
        }

        [Fact]
        public void SkipOtherProjectTypes()
        {
            string projectFile = Path.Combine(_rootDir, @"src\project.csproj");
            ProjectRootElement projectRootElement = ProjectRootElement.Create(projectFile);
            projectRootElement
                .AddItem("ProjectReference", @"..\WebRole1\WebRole1.csproj")
                .AddMetadata(AzureCloudServicePipelineTransformPhaseGraphPredictor.RoleTypeMetadataName, "Web");
            projectRootElement
                .AddItem("ProjectReference", @"..\WebRole2\WebRole2.csproj")
                .AddMetadata(AzureCloudServicePipelineTransformPhaseGraphPredictor.RoleTypeMetadataName, "Web");
            projectRootElement
                .AddItem("ProjectReference", @"..\WebRole3\WebRole3.csproj")
                .AddMetadata(AzureCloudServicePipelineTransformPhaseGraphPredictor.RoleTypeMetadataName, "Web");
            projectRootElement
                .AddItem("ProjectReference", @"..\WorkerRole\WorkerRole.csproj")
                .AddMetadata(AzureCloudServicePipelineTransformPhaseGraphPredictor.RoleTypeMetadataName, "Worker");
            projectRootElement
                .AddItem("ProjectReference", @"..\NoRole\NoRole.csproj");

            // WebRole with content items
            string webRole1File = Path.Combine(_rootDir, @"WebRole1\WebRole1.csproj");
            ProjectRootElement webRole1RootElement = ProjectRootElement.Create(webRole1File);
            webRole1RootElement.AddItem(ContentItemsPredictor.ContentItemName, "content1.txt");
            webRole1RootElement.AddItem(ContentItemsPredictor.ContentItemName, "content2.txt");
            webRole1RootElement.AddItem(ContentItemsPredictor.ContentItemName, "content3.txt");

            // WebRole with reference items
            string webRole2File = Path.Combine(_rootDir, @"WebRole2\WebRole2.csproj");
            ProjectRootElement webRole2RootElement = ProjectRootElement.Create(webRole2File);
            webRole2RootElement
                .AddItem(ReferenceItemsPredictor.ReferenceItemName, "Reference1")
                .AddMetadata(ReferenceItemsPredictor.HintPathMetadata, @"..\packages\Package1\lib\net45\Reference1.dll");
            webRole2RootElement
                .AddItem(ReferenceItemsPredictor.ReferenceItemName, @"..\packages\Package2\lib\net45\Reference2.dll")
                .AddMetadata("Name", "Reference2");

            // WebRole with TypeScriptCompile items
            string webRole3File = Path.Combine(_rootDir, @"WebRole3\WebRole3.csproj");
            ProjectRootElement webRole3RootElement = ProjectRootElement.Create(webRole3File);
            webRole3RootElement.AddItem(TypeScriptCompileItemsPredictor.TypeScriptCompileItemName, "script1.ts");
            webRole3RootElement.AddItem(TypeScriptCompileItemsPredictor.TypeScriptCompileItemName, "script2.ts");
            webRole3RootElement.AddItem(TypeScriptCompileItemsPredictor.TypeScriptCompileItemName, "script3.ts");

            // WorkerRole. Should have no predictions
            string workerRoleFile = Path.Combine(_rootDir, @"WorkerRole\WorkerRole.csproj");
            ProjectRootElement workerRoleRootElement = ProjectRootElement.Create(workerRoleFile);
            workerRoleRootElement.AddItem(ContentItemsPredictor.ContentItemName, "content.txt");
            workerRoleRootElement
                .AddItem(ReferenceItemsPredictor.ReferenceItemName, @"..\packages\Package\lib\net45\Reference.dll")
                .AddMetadata("Name", "Reference");

            // No role type. Should have no predictions
            string noRoleFile = Path.Combine(_rootDir, @"NoRole\NoRole.csproj");
            ProjectRootElement noRoleRootElement = ProjectRootElement.Create(noRoleFile);
            noRoleRootElement.AddItem(ContentItemsPredictor.ContentItemName, "content.txt");
            noRoleRootElement
                .AddItem(ReferenceItemsPredictor.ReferenceItemName, @"..\packages\Package\lib\net45\Reference.dll")
                .AddMetadata("Name", "Reference");

            projectRootElement.Save();
            webRole1RootElement.Save();
            webRole2RootElement.Save();
            webRole3RootElement.Save();
            workerRoleRootElement.Save();
            noRoleRootElement.Save();

            new AzureCloudServicePipelineTransformPhaseGraphPredictor()
                .GetProjectPredictions(projectFile)
                .AssertNoPredictions();
        }
    }
}
