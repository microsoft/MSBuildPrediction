// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class FakesOutputPathIntegrationTests
    {
        [Fact]
        public void FakesOutputPathPredictorIncludedInAllPredictors()
        {
            // Verify that our new predictor is included in the AllProjectPredictors collection
            var allPredictors = ProjectPredictors.AllProjectPredictors;
            Assert.Contains(allPredictors, p => p is FakesOutputPathPredictor);
        }
    }
}