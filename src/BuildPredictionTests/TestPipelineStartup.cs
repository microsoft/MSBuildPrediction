// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.Build.Locator;
using Xunit.Sdk;
using Xunit.v3;

[assembly: TestPipelineStartup(typeof(Microsoft.Build.Prediction.Tests.TestPipelineStartup))]

namespace Microsoft.Build.Prediction.Tests
{
    internal sealed class TestPipelineStartup : ITestPipelineStartup
    {
        public ValueTask StartAsync(IMessageSink diagnosticMessageSink)
        {
            MSBuildLocator.RegisterDefaults();
            return default;
        }

        public ValueTask StopAsync() => default;
    }
}