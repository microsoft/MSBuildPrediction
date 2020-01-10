// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class ProjectPredictorsTests
    {
        [Fact]
        public void AllProjectPredictors() => AssertAllPredictors(ProjectPredictors.AllProjectPredictors);

        [Fact]
        public void AllProjectGraphPredictors() => AssertAllPredictors(ProjectPredictors.AllProjectGraphPredictors);

        private static void AssertAllPredictors<T>(IReadOnlyCollection<T> actualPredictors)
        {
            // All predictors means all predictors. Use reflection to ensure we really did get all creatable IProjectPredictors.
            Type[] expectedPredictorTypes = typeof(T).Assembly.GetTypes()
                .Where(type => !type.IsInterface && !type.IsAbstract && typeof(T).IsAssignableFrom(type))
                .ToArray();

            Assert.Equal(expectedPredictorTypes.Length, actualPredictors.Count);
            foreach (Type predictorType in expectedPredictorTypes)
            {
                Assert.NotNull(actualPredictors.FirstOrDefault(predictor => predictor.GetType() == predictorType));
            }
        }
    }
}
