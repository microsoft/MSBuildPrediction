// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Tests.Predictors
{
    using System;
    using System.IO;
    using Microsoft.Build.Construction;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Prediction.Predictors;
    using Xunit;

    public class TsConfigPredictorTests
    {
        private readonly string _rootDir;

        public TsConfigPredictorTests()
        {
            // Isolate each test into its own folder
            _rootDir = Path.Combine(Directory.GetCurrentDirectory(), nameof(TsConfigPredictorTests), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_rootDir);
        }

        [Fact]
        public void Files()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddItem(TsConfigPredictor.ContentItemName, TsConfigPredictor.TsConfigFileName);

            const string TsConfigContent = @"
{
    ""compilerOptions"": {
        ""outFile"": ""dist/out.js"",
    },
    ""files"": [
        ""foo.ts"",
        ""bar.ts"",
        ""baz.ts"",
    ]
}";
            File.WriteAllText(Path.Combine(_rootDir, TsConfigPredictor.TsConfigFileName), TsConfigContent);

            WriteDummyFiles(
                "foo.ts",
                "foo.spec.ts",
                "bar.ts",
                "bar.spec.ts",
                "baz.ts",
                "baz.spec.ts",
                "unrelatedFile.txt",
                @"node_modules\a\index.ts",
                @"node_modules\b\index.ts",
                @"node_modules\c\index.ts");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"foo.ts", nameof(TsConfigPredictor)),
                new PredictedItem(@"bar.ts", nameof(TsConfigPredictor)),
                new PredictedItem(@"baz.ts", nameof(TsConfigPredictor)),
            };

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"dist\out.js", nameof(TsConfigPredictor)),
            };

            new TsConfigPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void Globs()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddItem(TsConfigPredictor.ContentItemName, TsConfigPredictor.TsConfigFileName);

            const string TsConfigContent = @"
{
    ""compilerOptions"": {
        ""outFile"": ""dist/out.js"",
    },
    ""include"": [
        ""**/*.ts""
    ],
    ""exclude"": [
        ""node_modules"",
        ""**/*.spec.ts""
    ]
}";
            File.WriteAllText(Path.Combine(_rootDir, TsConfigPredictor.TsConfigFileName), TsConfigContent);

            WriteDummyFiles(
                "foo.ts",
                "foo.spec.ts",
                "bar.ts",
                "bar.spec.ts",
                "baz.ts",
                "baz.spec.ts",
                "unrelatedFile.txt",
                @"node_modules\a\index.ts",
                @"node_modules\b\index.ts",
                @"node_modules\c\index.ts");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"foo.ts", nameof(TsConfigPredictor)),
                new PredictedItem(@"bar.ts", nameof(TsConfigPredictor)),
                new PredictedItem(@"baz.ts", nameof(TsConfigPredictor)),
            };

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"dist\out.js", nameof(TsConfigPredictor)),
            };

            new TsConfigPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void GlobsWithAncestorPaths()
        {
            string projectDir = Path.Combine(_rootDir, "src");
            string declarationsDir = Path.Combine(_rootDir, "declarations");

            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(projectDir, @"project.csproj"));
            projectRootElement.AddItem(TsConfigPredictor.ContentItemName, TsConfigPredictor.TsConfigFileName);

            const string TsConfigContent = @"
{
    ""compilerOptions"": {
        ""outFile"": ""dist/out.js"",
    },
    ""include"": [
        ""**/*.ts"",
        ""../declarations/**/*.ts"",
        ""../doesNotExist/**/*.ts""
    ],
    ""exclude"": [
        ""node_modules"",
        ""**/*.spec.ts""
    ]
}";
            Directory.CreateDirectory(projectDir);
            File.WriteAllText(Path.Combine(projectDir, TsConfigPredictor.TsConfigFileName), TsConfigContent);

            WriteDummyFiles(
                @"src\foo.ts",
                @"src\bar.ts",
                @"src\baz.ts",
                @"declarations\x.d.ts",
                @"declarations\y.d.ts",
                @"declarations\z.d.ts",
                @"node_modules\a\index.ts",
                @"node_modules\b\index.ts",
                @"node_modules\c\index.ts");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"src\foo.ts", nameof(TsConfigPredictor)),
                new PredictedItem(@"src\bar.ts", nameof(TsConfigPredictor)),
                new PredictedItem(@"src\baz.ts", nameof(TsConfigPredictor)),
                new PredictedItem(@"declarations\x.d.ts", nameof(TsConfigPredictor)),
                new PredictedItem(@"declarations\y.d.ts", nameof(TsConfigPredictor)),
                new PredictedItem(@"declarations\z.d.ts", nameof(TsConfigPredictor)),
            };

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"src\dist\out.js", nameof(TsConfigPredictor)),
            };

            new TsConfigPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void DefaultGlobs()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddItem(TsConfigPredictor.ContentItemName, TsConfigPredictor.TsConfigFileName);

            const string TsConfigContent = @"
{
    ""compilerOptions"": {
        ""outFile"": ""dist/out.js"",
    }
}";
            File.WriteAllText(Path.Combine(_rootDir, TsConfigPredictor.TsConfigFileName), TsConfigContent);

            WriteDummyFiles(
                "foo.ts",
                "bar.ts",
                "baz.ts",
                "unrelatedFile.txt",
                @"node_modules\a\index.ts",
                @"node_modules\b\index.ts",
                @"node_modules\c\index.ts");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"foo.ts", nameof(TsConfigPredictor)),
                new PredictedItem(@"bar.ts", nameof(TsConfigPredictor)),
                new PredictedItem(@"baz.ts", nameof(TsConfigPredictor)),
            };

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"dist\out.js", nameof(TsConfigPredictor)),
            };

            new TsConfigPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void OutDir()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddItem(TsConfigPredictor.ContentItemName, TsConfigPredictor.TsConfigFileName);

            const string TsConfigContent = @"
{
    ""compilerOptions"": {
        ""outDir"": ""dist"",
    },
    ""files"": [
        ""foo.ts"",
        ""bar.ts"",
        ""baz.ts"",
    ]
}";
            File.WriteAllText(Path.Combine(_rootDir, TsConfigPredictor.TsConfigFileName), TsConfigContent);

            WriteDummyFiles(
                "foo.ts",
                "bar.ts",
                "baz.ts");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"foo.ts", nameof(TsConfigPredictor)),
                new PredictedItem(@"bar.ts", nameof(TsConfigPredictor)),
                new PredictedItem(@"baz.ts", nameof(TsConfigPredictor)),
            };

            var expectedOutputDirectories = new[]
            {
                new PredictedItem(@"dist", nameof(TsConfigPredictor)),
            };

            new TsConfigPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    null,
                    expectedOutputDirectories.MakeAbsolute(_rootDir));
        }

        [Fact]
        public void MultipleConfigs()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddItem(TsConfigPredictor.ContentItemName, Path.Combine("foo", TsConfigPredictor.TsConfigFileName));
            projectRootElement.AddItem(TsConfigPredictor.ContentItemName, Path.Combine("bar", TsConfigPredictor.TsConfigFileName));
            projectRootElement.AddItem(TsConfigPredictor.ContentItemName, Path.Combine("baz", TsConfigPredictor.TsConfigFileName));

            const string TsConfigContent1 = @"
{
    ""compilerOptions"": {
        ""outFile"": ""../dist/foo.js"",
    },
    ""files"": [
        ""index.ts"",
    ]
}";
            const string TsConfigContent2 = @"
{
    ""compilerOptions"": {
        ""outFile"": ""../dist/bar.js"",
    },
    ""files"": [
        ""index.ts"",
    ]
}";
            const string TsConfigContent3 = @"
{
    ""compilerOptions"": {
        ""outFile"": ""../dist/baz.js"",
    },
    ""files"": [
        ""index.ts"",
    ]
}";
            Directory.CreateDirectory(Path.Combine(_rootDir, "foo"));
            Directory.CreateDirectory(Path.Combine(_rootDir, "bar"));
            Directory.CreateDirectory(Path.Combine(_rootDir, "baz"));
            File.WriteAllText(Path.Combine(_rootDir, "foo", TsConfigPredictor.TsConfigFileName), TsConfigContent1);
            File.WriteAllText(Path.Combine(_rootDir, "bar", TsConfigPredictor.TsConfigFileName), TsConfigContent2);
            File.WriteAllText(Path.Combine(_rootDir, "baz", TsConfigPredictor.TsConfigFileName), TsConfigContent3);

            WriteDummyFiles(
                @"foo\index.ts",
                @"bar\index.ts",
                @"baz\index.ts");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"foo\index.ts", nameof(TsConfigPredictor)),
                new PredictedItem(@"bar\index.ts", nameof(TsConfigPredictor)),
                new PredictedItem(@"baz\index.ts", nameof(TsConfigPredictor)),
            };

            var expectedOutputFiles = new[]
            {
                new PredictedItem(@"dist\foo.js", nameof(TsConfigPredictor)),
                new PredictedItem(@"dist\bar.js", nameof(TsConfigPredictor)),
                new PredictedItem(@"dist\baz.js", nameof(TsConfigPredictor)),
            };

            new TsConfigPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    expectedOutputFiles.MakeAbsolute(_rootDir),
                    null);
        }

        [Fact]
        public void JsConfig()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create(Path.Combine(_rootDir, @"project.csproj"));
            projectRootElement.AddItem(TsConfigPredictor.ContentItemName, TsConfigPredictor.JsConfigFileName);

            const string JsConfigContent = @"
{
    ""files"": [
        ""foo.js"",
        ""bar.js"",
        ""baz.js"",
    ]
}";
            File.WriteAllText(Path.Combine(_rootDir, TsConfigPredictor.JsConfigFileName), JsConfigContent);

            WriteDummyFiles(
                "foo.js",
                "bar.js",
                "baz.js",
                "unrelatedFile.txt",
                @"node_modules\a\index.js",
                @"node_modules\b\index.js",
                @"node_modules\c\index.js");

            Project project = TestHelpers.CreateProjectFromRootElement(projectRootElement);

            var expectedInputFiles = new[]
            {
                new PredictedItem(@"foo.js", nameof(TsConfigPredictor)),
                new PredictedItem(@"bar.js", nameof(TsConfigPredictor)),
                new PredictedItem(@"baz.js", nameof(TsConfigPredictor)),
            };

            new TsConfigPredictor()
                .GetProjectPredictions(project)
                .AssertPredictions(
                    project,
                    expectedInputFiles.MakeAbsolute(_rootDir),
                    null,
                    null,
                    null);
        }

        private void WriteDummyFiles(params string[] files)
        {
            foreach (string file in files)
            {
                string fileFullPath = Path.Combine(_rootDir, file);
                Directory.CreateDirectory(Path.GetDirectoryName(fileFullPath));
                File.WriteAllText(fileFullPath, "dummy");
            }
        }
    }
}
