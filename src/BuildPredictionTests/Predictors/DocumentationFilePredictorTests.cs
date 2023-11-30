// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class DocumentationFilePredictorTests
    {
        [Fact]
        public void FindItems()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddItem(DocumentationFilePredictor.DocFileItemItemName, @"obj\Foo.xml");
            projectRootElement.AddItem(DocumentationFilePredictor.FinalDocFileItemName, @"bin\Foo.xml");

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"obj\Foo.xml", nameof(DocumentationFilePredictor)),
                new PredictedItem(@"bin\Foo.xml", nameof(DocumentationFilePredictor)),
            };

            new DocumentationFilePredictor()
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    null,
                    null,
                    expectedOutputFiles,
                    null);
        }
    }
}