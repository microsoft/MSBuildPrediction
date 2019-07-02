// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class ReferenceItemsPredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(@"src\project.csproj");

            // AllowedReferenceRelatedFileExtensions is usually set in Microsoft.Common.CurrentVersion.targets
            const string allowedReferenceRelatedFileExtensions = @"
      .pdb;
      .xml;
      .pri;
      .dll.config;
      .exe.config
";
            projectRootElement.AddProperty(ReferenceItemsPredictor.AllowedReferenceRelatedFileExtensionsPropertyName, allowedReferenceRelatedFileExtensions);

            // Reference using HintPath - Is an input
            projectRootElement
                .AddItem(ReferenceItemsPredictor.ReferenceItemName, "Reference1")
                .AddMetadata(ReferenceItemsPredictor.HintPathMetadata, @"..\packages\Package1\lib\net45\Reference1.dll");

            // Reference not using HintPath (usually uses <Name> though) - Is an input
            projectRootElement
                .AddItem(ReferenceItemsPredictor.ReferenceItemName, @"..\packages\Package2\lib\net45\Reference2.dll")
                .AddMetadata("Name", "Reference2");

            // Reference adjacent to the project (must exist) - Is an input
            projectRootElement.AddItem(ReferenceItemsPredictor.ReferenceItemName, @"CheckedInAssembly.dll");
            Directory.CreateDirectory("src");
            File.WriteAllText(@"src\CheckedInAssembly.dll", "SomeContent");

            // Reference with related files (which must exist) - Is an input as well as related files
            projectRootElement
                .AddItem(ReferenceItemsPredictor.ReferenceItemName, @"..\packages\Package3\lib\net45\Reference3.dll")
                .AddMetadata("Name", "Reference3");
            Directory.CreateDirectory(@"packages\Package3\lib\net45");
            File.WriteAllText(@"packages\Package3\lib\net45\Reference3.dll", "SomeContent");
            File.WriteAllText(@"packages\Package3\lib\net45\Reference3.pdb", "SomeContent");
            File.WriteAllText(@"packages\Package3\lib\net45\Reference3.xml", "SomeContent");

            // References from the platform or GAC - NOT inputs
            projectRootElement.AddItem(ReferenceItemsPredictor.ReferenceItemName, "System.Data");
            projectRootElement.AddItem(ReferenceItemsPredictor.ReferenceItemName, "System.Management.Automation, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"packages\Package1\lib\net45\Reference1.dll", nameof(ReferenceItemsPredictor)),
                new PredictedItem(@"packages\Package2\lib\net45\Reference2.dll", nameof(ReferenceItemsPredictor)),
                new PredictedItem(@"packages\Package3\lib\net45\Reference3.dll", nameof(ReferenceItemsPredictor)),
                new PredictedItem(@"packages\Package3\lib\net45\Reference3.pdb", nameof(ReferenceItemsPredictor)),
                new PredictedItem(@"packages\Package3\lib\net45\Reference3.xml", nameof(ReferenceItemsPredictor)),
                new PredictedItem(@"src\CheckedInAssembly.dll", nameof(ReferenceItemsPredictor)),
            };
            new ReferenceItemsPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(Directory.GetCurrentDirectory()),
                    null,
                    null,
                    null);
        }
    }
}
