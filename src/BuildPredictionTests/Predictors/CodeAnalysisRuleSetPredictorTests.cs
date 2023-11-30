// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class CodeAnalysisRuleSetPredictorTests
    {
        private readonly string _rootDir;

        // Use the same predictor instance per test to test caching properly. Note that this means each test case should be considered a single prediction session.
        private readonly CodeAnalysisRuleSetPredictor _predictor = new CodeAnalysisRuleSetPredictor();

        public CodeAnalysisRuleSetPredictorTests()
        {
            // Isolate each test into its own folder
            _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(CodeAnalysisRuleSetPredictorTests), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootDir);
        }

        [Fact]
        public void NoRulsetProperty()
        {
            AssertInputs(null, null);
        }

        [Fact]
        public void EmptyRulesetProperty()
        {
            AssertInputs(string.Empty, null);
        }

        [Fact]
        public void MissingFile()
        {
            AssertInputs("doesNotExist.ruleset", null);
        }

        [Fact]
        public void SimpleRuleSet()
        {
            var rulesetA = CreateRuleSetFile("a.ruleset")
                .WriteToDisk();

            var expectedInputs = new[]
            {
                rulesetA.FullPath,
            };
            AssertInputs("a.ruleset", expectedInputs);
        }

        [Fact]
        public void BadRuleSetContent()
        {
            File.WriteAllText(Path.Combine(_rootDir, "a.ruleset"), "badContent");

            AssertInputs("a.ruleset", null);
        }

        [Fact]
        public void RuleSetIsCached()
        {
            var rulesetA = CreateRuleSetFile("a.ruleset")
                .WriteToDisk();

            var expectedInputs = new[]
            {
                rulesetA.FullPath,
            };
            AssertInputs("a.ruleset", expectedInputs);

            // To be sure caching works, clear out the file, making it invalid. Parsing would fail if there wasn't a cache hit.
            File.WriteAllText(rulesetA.FullPath, "badContent");

            AssertInputs("a.ruleset", expectedInputs);
        }

        [Fact]
        public void RuleSetWithRuleHintPaths()
        {
            var rulesetA = CreateRuleSetFile("a.ruleset")
                .WithRuleHintPaths("rules1.dll")
                .WithRuleHintPaths("rules2.dll")
                .WriteToDisk();

            var expectedInputs = new[]
            {
                rulesetA.FullPath,
                Path.Combine(_rootDir, "rules1.dll"),
                Path.Combine(_rootDir, "rules2.dll"),
            };
            AssertInputs("a.ruleset", expectedInputs);
        }

        [Fact]
        public void RuleSetWithIncludes()
        {
            var rulesetA = CreateRuleSetFile("a.ruleset")
                .WithInclude("b.ruleset")
                .WriteToDisk();
            var rulesetB = CreateRuleSetFile("b.ruleset")
                .WriteToDisk();

            var expectedInputs = new[]
            {
                rulesetA.FullPath,
                rulesetB.FullPath,
            };
            AssertInputs("a.ruleset", expectedInputs);
        }

        [Fact]
        public void RuleSetWithMissingInclude()
        {
            var rulesetA = CreateRuleSetFile("a.ruleset")
                .WithInclude("doesNotExist.ruleset")
                .WriteToDisk();

            // We still predict a.ruleset and just ignore anything invalid.
            var expectedInputs = new[]
            {
                rulesetA.FullPath,
            };
            AssertInputs("a.ruleset", expectedInputs);
        }

        [Fact]
        public void RuleSetWithRelativeSubsirIncludes()
        {
            // Graph is: a -> b -> c -> d -> e
            // a, d, and e are in the foo subdir
            // b and c are in the bar subir
            var rulesetA = CreateRuleSetFile(@"foo\a.ruleset")
                .WithInclude(@"..\bar\b.ruleset")
                .WriteToDisk();
            var rulesetB = CreateRuleSetFile(@"bar\b.ruleset")
                .WithInclude(@"c.ruleset")
                .WriteToDisk();
            var rulesetC = CreateRuleSetFile(@"bar\c.ruleset")
                .WithInclude(@"..\foo\d.ruleset")
                .WriteToDisk();
            var rulesetD = CreateRuleSetFile(@"foo\d.ruleset")
                .WithInclude(@"e.ruleset")
                .WriteToDisk();
            var rulesetE = CreateRuleSetFile(@"foo\e.ruleset")
                .WriteToDisk();

            var expectedInputs = new[]
            {
                rulesetA.FullPath,
                rulesetB.FullPath,
                rulesetC.FullPath,
                rulesetD.FullPath,
                rulesetE.FullPath,
            };
            AssertInputs(@"foo\a.ruleset", expectedInputs);
        }

        [Fact]
        public void RuleSetWithAbsoluteIncludes()
        {
            var rulesetB = CreateRuleSetFile(@"bar\b.ruleset")
                .WriteToDisk();
            var rulesetA = CreateRuleSetFile(@"foo\a.ruleset")
                .WithInclude(rulesetB.FullPath)
                .WriteToDisk();

            Assert.True(Path.IsPathRooted(rulesetB.FullPath));
            var expectedInputs = new[]
            {
                rulesetA.FullPath,
                rulesetB.FullPath,
            };
            AssertInputs(@"foo\a.ruleset", expectedInputs);
        }

        [Fact]
        public void RuleSetWithIncludesAndRuleHintPaths()
        {
            var rulesetA = CreateRuleSetFile("a.ruleset")
                .WithInclude("b.ruleset")
                .WithRuleHintPaths("a.dll")
                .WithRuleHintPaths("missing.dll", createFile: false) // not an expected input
                .WriteToDisk();
            var rulesetB = CreateRuleSetFile("b.ruleset")
                .WithRuleHintPaths("b.dll")
                .WriteToDisk();

            var expectedInputs = new[]
            {
                rulesetA.FullPath,
                Path.Combine(_rootDir, "a.dll"),
                rulesetB.FullPath,
                Path.Combine(_rootDir, "b.dll"),
            };
            AssertInputs("a.ruleset", expectedInputs);
        }

        [Fact]
        public void RuleSetWithSelfCycle()
        {
            var rulesetA = CreateRuleSetFile("a.ruleset")
                .WithInclude("a.ruleset")
                .WriteToDisk();

            var expectedInputs = new[]
            {
                rulesetA.FullPath,
            };
            AssertInputs("a.ruleset", expectedInputs);
        }

        [Fact]
        public void RuleSetWithSimpleCycle()
        {
            var rulesetA = CreateRuleSetFile("a.ruleset")
                .WithInclude("b.ruleset")
                .WriteToDisk();
            var rulesetB = CreateRuleSetFile("b.ruleset")
                .WithInclude("a.ruleset")
                .WriteToDisk();

            var expectedInputs = new[]
            {
                rulesetA.FullPath,
                rulesetB.FullPath,
            };
            AssertInputs("a.ruleset", expectedInputs);
        }

        [Fact]
        public void RuleSetWithComplexCycle()
        {
            // Two entrypoints (a1 and a2) which point to difference parts of the (b, c, d) cycle.
            // b, c, and d all point to rulesets outside the cycle as well.
            // d points to z1 which is part of another z1, z2 cycle.
            var rulesetA1 = CreateRuleSetFile("a1.ruleset")
                .WithInclude("b.ruleset")
                .WriteToDisk();
            var rulesetA2 = CreateRuleSetFile("a2.ruleset")
                .WithInclude("c.ruleset")
                .WriteToDisk();
            var rulesetB = CreateRuleSetFile("b.ruleset")
                .WithInclude("c.ruleset")
                .WithInclude("x.ruleset")
                .WriteToDisk();
            var rulesetC = CreateRuleSetFile("c.ruleset")
                .WithInclude("d.ruleset")
                .WithInclude("y.ruleset")
                .WriteToDisk();
            var rulesetD = CreateRuleSetFile("d.ruleset")
                .WithInclude("b.ruleset")
                .WithInclude("z1.ruleset")
                .WriteToDisk();
            var rulesetX = CreateRuleSetFile("x.ruleset")
                .WriteToDisk();
            var rulesetY = CreateRuleSetFile("y.ruleset")
                .WriteToDisk();
            var rulesetZ1 = CreateRuleSetFile("z1.ruleset")
                .WithInclude("z2.ruleset")
                .WriteToDisk();
            var rulesetZ2 = CreateRuleSetFile("z2.ruleset")
                .WithInclude("z1.ruleset")
                .WriteToDisk();

            var expectedResultsInCycleZ = new[]
            {
                rulesetZ1.FullPath,
                rulesetZ2.FullPath,
            };

            var expectedResultsInCycleBCD = expectedResultsInCycleZ.Union(
                new[]
                {
                    rulesetB.FullPath,
                    rulesetC.FullPath,
                    rulesetD.FullPath,
                    rulesetX.FullPath,
                    rulesetY.FullPath,
                }).ToList();

            AssertInputs("b.ruleset", expectedResultsInCycleBCD);
            AssertInputs("c.ruleset", expectedResultsInCycleBCD);
            AssertInputs("d.ruleset", expectedResultsInCycleBCD);
            AssertInputs("z2.ruleset", expectedResultsInCycleZ);
            AssertInputs("a1.ruleset", expectedResultsInCycleBCD.Union(new[] { rulesetA1.FullPath }).ToList());
            AssertInputs("a2.ruleset", expectedResultsInCycleBCD.Union(new[] { rulesetA2.FullPath }).ToList());
        }

        private void AssertInputs(string ruleSetPath, IList<string> expectedInputs)
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, "project.proj"));
            if (ruleSetPath != null)
            {
                ProjectPropertyGroupElement propertyGroup = projectRootElement.AddPropertyGroup();
                projectRootElement.AddProperty(CodeAnalysisRuleSetPredictor.CodeAnalysisRuleSetPropertyName, ruleSetPath);
            }

            var projectInstance = TestHelpers.CreateProjectInstanceFromRootElement(projectRootElement);

            var expectedInputFiles = expectedInputs?
                .Select(input => new PredictedItem(input, nameof(CodeAnalysisRuleSetPredictor)))
                .ToArray();

            _predictor
                .GetProjectPredictions(projectInstance)
                .AssertPredictions(
                    projectInstance,
                    expectedInputFiles,
                    null,
                    null,
                    null);
        }

        private RulesetFile CreateRuleSetFile(string path) => new RulesetFile(Path.Combine(_rootDir, path));

        private sealed class RulesetFile
        {
            private readonly XDocument _xmlDoc;

            private readonly XElement _xmlRoot;

            private readonly string _directory;

            public RulesetFile(string path)
            {
                FullPath = path;
                _directory = Path.GetDirectoryName(path);
                Directory.CreateDirectory(_directory);

                _xmlDoc = new XDocument();
                _xmlRoot = new XElement("RuleSet");
                _xmlDoc.Add(_xmlRoot);
            }

            public string FullPath { get; }

            public RulesetFile WithInclude(string includePath)
            {
                _xmlRoot.Add(new XElement("Include", new XAttribute("Path", includePath)));
                return this;
            }

            public RulesetFile WithRuleHintPaths(string ruleHintPath, bool createFile = true)
            {
                var ruleHintPaths = _xmlRoot.Elements("RuleHintPaths").FirstOrDefault();
                if (ruleHintPaths == null)
                {
                    ruleHintPaths = new XElement("RuleHintPaths");
                    _xmlRoot.Add(ruleHintPaths);
                }

                ruleHintPaths.Add(new XElement("Path", ruleHintPath));

                if (createFile)
                {
                    var asoluteRuleHintPath = Path.Combine(_directory, ruleHintPath);
                    Directory.CreateDirectory(Path.GetDirectoryName(asoluteRuleHintPath));
                    File.WriteAllText(asoluteRuleHintPath, "SomeContent");
                }

                return this;
            }

            public RulesetFile WriteToDisk()
            {
                using (XmlWriter writer = XmlWriter.Create(FullPath))
                {
                    _xmlDoc.WriteTo(writer);
                }

                return this;
            }
        }
    }
}