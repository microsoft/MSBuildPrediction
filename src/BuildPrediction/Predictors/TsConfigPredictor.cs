// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Build.Execution;
using Microsoft.Build.Globbing;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Predicts inputs and outputs for tsconfig files.
    /// </summary>
    public sealed class TsConfigPredictor : IProjectPredictor
    {
        internal const string ContentItemName = "Content";

        internal const string TypeScriptCompileItemName = "TypeScriptCompile";

        internal const string TsConfigFileName = "tsconfig.json";

        internal const string JsConfigFileName = "jsconfig.json";

        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions()
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            List<string> configFiles = FindConfigFiles(projectInstance);

            foreach (string configFile in configFiles)
            {
                string configFileFullPath = Path.Combine(projectInstance.Directory, configFile);
                string configFileContent = File.ReadAllText(configFileFullPath);
                bool isTsConfig = Path.GetFileName(configFile).Equals(TsConfigFileName, PathComparer.Comparison);

                TsConfig tsConfig;
                try
                {
                    tsConfig = JsonSerializer.Deserialize<TsConfig>(configFileContent, _jsonSerializerOptions);
                }
                catch (JsonException)
                {
                    // Ignore invalid config files
                    continue;
                }

                string configFileDir = Path.GetDirectoryName(configFileFullPath);

                tsConfig.ApplyDefaults();

                if (tsConfig.Files != null)
                {
                    foreach (string file in tsConfig.Files)
                    {
                        string fileFullPath = PathUtilities.NormalizePath(Path.Combine(configFileDir, file));
                        predictionReporter.ReportInputFile(fileFullPath);
                    }
                }

                // Reuse MSBuild's globbing logic as it should match what tsc does.
                if (tsConfig.Include != null)
                {
                    var directoriesToEnumerate = new Queue<string>();

                    var includeGlobs = new MSBuildGlob[tsConfig.Include.Count];
                    for (var i = 0; i < tsConfig.Include.Count; i++)
                    {
                        string include = tsConfig.Include[i];
                        MSBuildGlob includeGlob = MSBuildGlob.Parse(configFileDir, include);
                        directoriesToEnumerate.Enqueue(includeGlob.FixedDirectoryPart);
                        includeGlobs[i] = includeGlob;
                    }

                    var excludeGlobs = new MSBuildGlob[tsConfig.Exclude.Count];
                    for (var i = 0; i < tsConfig.Exclude.Count; i++)
                    {
                        string exclude = tsConfig.Exclude[i];

                        // Handle the case where just a folder name is used as an exclude value
                        if (exclude.IndexOf('*', StringComparison.Ordinal) == -1)
                        {
                            exclude += "/**";
                        }

                        excludeGlobs[i] = MSBuildGlob.Parse(configFileDir, exclude);
                    }

                    var finalGlob = new MSBuildGlobWithGaps(new CompositeGlob(includeGlobs), excludeGlobs);
                    var visitedDirectories = new HashSet<string>(PathComparer.Instance);
                    while (directoriesToEnumerate.Count > 0)
                    {
                        string directoryToEnumerate = directoriesToEnumerate.Dequeue();

                        // In case the initial globs has parent/child relationships
                        if (!visitedDirectories.Add(directoryToEnumerate))
                        {
                            continue;
                        }

                        // Some globs might point to non-existent paths.
                        if (!Directory.Exists(directoryToEnumerate))
                        {
                            continue;
                        }

                        foreach (string file in Directory.EnumerateFiles(directoryToEnumerate, "*", SearchOption.TopDirectoryOnly))
                        {
                            if (finalGlob.IsMatch(file))
                            {
                                predictionReporter.ReportInputFile(file);
                            }
                        }

                        foreach (string directory in Directory.EnumerateDirectories(directoryToEnumerate, "*", SearchOption.TopDirectoryOnly))
                        {
                            directoriesToEnumerate.Enqueue(directory);
                        }
                    }
                }

                if (isTsConfig && tsConfig.CompilerOptions != null)
                {
                    if (!string.IsNullOrEmpty(tsConfig.CompilerOptions.OutFile))
                    {
                        string outFileFullPath = PathUtilities.NormalizePath(Path.Combine(configFileDir, tsConfig.CompilerOptions.OutFile));
                        predictionReporter.ReportOutputFile(outFileFullPath);
                    }

                    if (!string.IsNullOrEmpty(tsConfig.CompilerOptions.OutDir))
                    {
                        string outDirFullPath = PathUtilities.NormalizePath(Path.Combine(configFileDir, tsConfig.CompilerOptions.OutDir));
                        predictionReporter.ReportOutputDirectory(outDirFullPath);
                    }
                }
            }
        }

        /// <summary>
        /// This mostly follows the logic of the FindConfigFiles target and task.
        /// There is some probing behavior when there are no Content or TypeScriptCompile items, but we're not currently supporting that scenario.
        /// </summary>
        private List<string> FindConfigFiles(ProjectInstance projectInstance)
        {
            ICollection<ProjectItemInstance> contentItems = projectInstance.GetItems(ContentItemName);
            ICollection<ProjectItemInstance> typeScriptCompileItems = projectInstance.GetItems(TypeScriptCompileItemName);

            static bool IsConfigFile(string file)
            {
                if (file == null)
                {
                    return false;
                }

                string fileName = Path.GetFileName(file);
                return fileName.Equals(TsConfigFileName, PathComparer.Comparison)
                    || fileName.Equals(JsConfigFileName, PathComparer.Comparison);
            }

            static void AddConfigFiles(List<string> configFiles, ICollection<ProjectItemInstance> items)
            {
                foreach (ProjectItemInstance item in items)
                {
                    string itemInclude = item.EvaluatedInclude;
                    if (IsConfigFile(itemInclude))
                    {
                        configFiles.Add(itemInclude);
                    }
                }
            }

            var configFiles = new List<string>();
            AddConfigFiles(configFiles, contentItems);
            AddConfigFiles(configFiles, typeScriptCompileItems);
            return configFiles;
        }

        /// <summary>
        /// The TsConfig file schema.
        /// See: https://www.typescriptlang.org/docs/handbook/tsconfig-json.html.
        /// </summary>
        private sealed class TsConfig
        {
            public TsConfigCompilerOptions CompilerOptions { get; set; }

            public List<string> Files { get; set; }

            public List<string> Include { get; set; }

            public List<string> Exclude { get; set; }

            public void ApplyDefaults()
            {
                /*
                    From the docs:
                        If the "files" and "include" are both left unspecified, the compiler defaults to including all
                        TypeScript (.ts, .d.ts and .tsx) files in the containing directory and subdirectories except those
                        excluded using the "exclude" property. JS files (.js and .jsx) are also included if allowJs is set to true.
                */
                if (Files == null && Include == null)
                {
                    Include = new List<string>
                    {
                        "**/*.ts",
                        "**/*.d.ts",
                        "**/*.tsx",
                    };

                    if (CompilerOptions != null && CompilerOptions.AllowJs)
                    {
                        Include.Add("**/*.js");
                        Include.Add("**/*.jsx");
                    }
                }

                /*
                    From the docs:
                        The "exclude" property defaults to excluding the node_modules, bower_components, jspm_packages and <outDir> directories when not specified.
                */
                if (Exclude == null)
                {
                    Exclude = new List<string>
                    {
                        "node_modules",
                        "bower_components",
                        "jspm_packages",
                    };

                    if (CompilerOptions != null && !string.IsNullOrEmpty(CompilerOptions.OutDir))
                    {
                        Exclude.Add(CompilerOptions.OutDir);
                    }
                }
            }
        }

        private sealed class TsConfigCompilerOptions
        {
            public bool AllowJs { get; set; }

            public string OutFile { get; set; }

            public string OutDir { get; set; }
        }
    }
}