// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Execution;
using Microsoft.Build.Prediction;
using Xunit;

namespace Microsoft.Build.Prediction.Tests
{
    public class MsBuildHelpersTests
    {
        [Theory]
        [InlineData(null, false)]
        [InlineData("Never", false)]
        [InlineData("Always", true)]
        [InlineData("PreserveNewest", true)]
        public void ShouldCopyToOutputDirectory(string copyToOutputDirectoryValue, bool expectedResult)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            ProjectItemElement item = projectRootElement.AddItem("Foo", "Foo.xml");
            if (!string.IsNullOrEmpty(copyToOutputDirectoryValue))
            {
                item.AddMetadata("CopyToOutputDirectory", copyToOutputDirectoryValue);
            }

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);
            ProjectItemInstance itemInstance = projectInstance.GetItems("Foo").Single();

            Assert.Equal(expectedResult, itemInstance.ShouldCopyToOutputDirectory());
        }

        [Theory]
        [InlineData("Foo.xml", null, "Foo.xml")]
        [InlineData("Foo.xml", @"Link=Bar\Baz.xml", @"Bar\Baz.xml")]
        [InlineData("Foo.xml", @"TargetPath=Bar\Baz.xml;Link=ShouldNotBeUsed.xml", @"Bar\Baz.xml")]
        [InlineData(@".\.\.\X\.\.\.\.\Foo.xml", null, @"X\Foo.xml")]
        [InlineData(@"{ProjectDir}\X\Foo.xml", null, @"X\Foo.xml")]
        [InlineData(@"{ProjectDir}\.\.\.\.\X\.\.\.\Foo.xml", null, @"X\Foo.xml")]
        [InlineData(@"{ProjectDir}\..\..\..\X\Y\Z\Foo.xml", null, @"Foo.xml")]
        public void GetTargetPath(string itemIdentity, string properties, string expectedResult)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();

            itemIdentity = itemIdentity.Replace("{ProjectDir}", projectRootElement.DirectoryPath, StringComparison.Ordinal);
            ProjectItemElement item = projectRootElement.AddItem("Foo", itemIdentity);

            if (properties is not null)
            {
                foreach (string property in properties.Split(';'))
                {
                    string[] propertyParts = property.Split(['='], 2);
                    item.AddMetadata(propertyParts[0], propertyParts[1]);
                }
            }

            ProjectInstance projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);
            ProjectItemInstance itemInstance = projectInstance.GetItems("Foo").Single();

            Assert.Equal(expectedResult, itemInstance.GetTargetPath());
        }
    }
}