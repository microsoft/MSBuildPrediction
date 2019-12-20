// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using Microsoft.Build.Evaluation;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Predicts inputs and outputs for projects using the Microsoft.Build.Artifacts Sdk.
    /// See: https://github.com/microsoft/MSBuildSdks/tree/master/src/Artifacts.
    /// </summary>
    public sealed class ArtifactsSdkPredictor : IProjectPredictor
    {
        internal const string UsingMicrosoftArtifactsSdkPropertyName = "UsingMicrosoftArtifactsSdk";

        internal const string ArtifactsItemName = "Artifacts";

        internal const string RobocopyItemName = "Robocopy";

        internal const string DestinationFolderMetadata = "DestinationFolder";

        internal const string IsRecursiveMetadata = "IsRecursive";

        internal const string FileMatchMetadata = "FileMatch";

        internal const string FileExcludeMetadata = "FileExclude";

        internal const string DirExcludeMetadata = "DirExclude";

        private static readonly char[] MultiSplits = { '\t', ' ', '\n', '\r' };

        private static readonly char[] Wildcards = { '?', '*' };

        private static readonly char[] DestinationFolderSeparator = { ';' };

        private static readonly char[] DirectorySeparatorChars = { Path.DirectorySeparatorChar };

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(Project project, ProjectInstance projectInstance, ProjectPredictionReporter predictionReporter)
        {
            // This predictor only applies to projects using the Microsoft.Build.Artifacts Sdk.
            var usingMicrosoftArtifactsSdk = projectInstance.GetPropertyValue(UsingMicrosoftArtifactsSdkPropertyName);
            if (!usingMicrosoftArtifactsSdk.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // The Sdk allows both Artifacts and Robocopy items
            ProcessItems(projectInstance.GetItems(ArtifactsItemName), predictionReporter);
            ProcessItems(projectInstance.GetItems(RobocopyItemName), predictionReporter);
        }

        private static void ProcessItems(ICollection<ProjectItemInstance> items, ProjectPredictionReporter predictionReporter)
        {
            foreach (ProjectItemInstance item in items)
            {
                string source = item.EvaluatedInclude;
                string[] destinationFolders = item.GetMetadataValue(DestinationFolderMetadata).Split(DestinationFolderSeparator, StringSplitOptions.RemoveEmptyEntries);
                if (string.IsNullOrEmpty(source)
                    || destinationFolders.Length == 0)
                {
                    // Ignore invalid items.
                    continue;
                }

                // Make absolute
                source = Path.GetFullPath(Path.Combine(item.Project.Directory, source)).TrimEnd(DirectorySeparatorChars);

                if (File.Exists(source))
                {
                    // If the source is a known file, we can easily predict the inputs and outputs
                    ProcessFile(source, Path.GetFileName(source), destinationFolders, null, predictionReporter);
                }
                else if (Directory.Exists(source))
                {
                    // If the source is a known directory, we can use the rest of the metadata to do a precise prediction.
                    var matcher = new Matcher(item, source);

                    // This value defaults to true, so check that it's not explicitly false
                    bool isRecursive = !string.Equals(item.GetMetadataValue(IsRecursiveMetadata), "false", StringComparison.OrdinalIgnoreCase);

                    ProcessDirectory(
                        matcher,
                        isRecursive,
                        matcher.SearchPattern,
                        source,
                        destinationFolders,
                        null,
                        predictionReporter);
                }
                else
                {
                    // The source doesn't exist, so is probably a file or folder generated at build time. Based on presence
                    // of certain metadata, make a best guess as to whether this is probably a file or folder artifact, and
                    // do vague predictions.
                    bool isProbablyDirectory = !string.IsNullOrEmpty(item.GetMetadataValue(FileMatchMetadata))
                        || !string.IsNullOrEmpty(item.GetMetadataValue(FileExcludeMetadata))
                        || !string.IsNullOrEmpty(item.GetMetadataValue(DirExcludeMetadata))
                        || string.Equals(item.GetMetadataValue(IsRecursiveMetadata), "true", StringComparison.OrdinalIgnoreCase);
                    if (isProbablyDirectory)
                    {
                        // We don't know anything about the file which might be copied, so just predict the source and destination directories
                        predictionReporter.ReportInputDirectory(source);

                        foreach (string destination in destinationFolders)
                        {
                            predictionReporter.ReportOutputDirectory(destination);
                        }
                    }
                    else
                    {
                        ProcessFile(source, Path.GetFileName(source), destinationFolders, null, predictionReporter);
                    }
                }
            }
        }

        private static void ProcessFile(
            string sourceFile,
            string fileName,
            string[] destinationFolders,
            string destinationSubDirectory,
            ProjectPredictionReporter predictionReporter)
        {
            // Inputs
            predictionReporter.ReportInputFile(sourceFile);

            // Outputs
            foreach (string destination in destinationFolders)
            {
                string destinationFile = !string.IsNullOrEmpty(destinationSubDirectory)
                    ? Path.Combine(destination, destinationSubDirectory, fileName)
                    : Path.Combine(destination, fileName);
                predictionReporter.ReportOutputFile(destinationFile);
            }
        }

        private static void ProcessDirectory(
            Matcher matcher,
            bool isRecursive,
            string searchPattern,
            string sourceDirectory,
            string[] destinationFolders,
            string destinationSubDirectory,
            ProjectPredictionReporter predictionReporter)
        {
            foreach (string sourceFile in Directory.EnumerateFiles(sourceDirectory, searchPattern))
            {
                string fileName = Path.GetFileName(sourceFile);
                if (matcher.IsMatch(fileName, destinationSubDirectory, isFile: true))
                {
                    ProcessFile(sourceFile, fileName, destinationFolders, destinationSubDirectory, predictionReporter);
                }
            }

            // Doing recursion manually so we can consider DirExcludes
            if (isRecursive)
            {
                foreach (string childSourceDirectory in Directory.EnumerateDirectories(sourceDirectory))
                {
                    string childSourceName = Path.GetFileName(childSourceDirectory);
                    string childDestinationSubDirectory = !string.IsNullOrEmpty(destinationSubDirectory)
                        ? Path.Combine(destinationSubDirectory, childSourceName)
                        : childSourceName;
                    if (matcher.IsMatch(childDestinationSubDirectory, destinationSubDirectory, isFile: false))
                    {
                        ProcessDirectory(
                            matcher,
                            isRecursive: true,
                            searchPattern,
                            childSourceDirectory,
                            destinationFolders,
                            childDestinationSubDirectory,
                            predictionReporter);
                    }
                }
            }
        }

        /// <summary>
        /// This is based on the logic in RobocopyMetadata.
        /// See: https://github.com/microsoft/MSBuildSdks/blob/master/src/Artifacts/Tasks/RobocopyMetadata.cs.
        /// </summary>
        private ref struct Matcher
        {
            private static readonly List<string> _emptyStringList = new List<string>(0);

            private static readonly List<Regex> _emptyRegexList = new List<Regex>(0);

            private string _sourceFolder;

            private bool _doMatchAll;

            private List<string> _fileMatches;

            private List<Regex> _fileRegexMatches;

            private List<string> _fileExcludes;

            private List<Regex> _fileRegexExcludes;

            private List<string> _dirExcludes;

            private List<Regex> _dirRegexExcludes;

            public Matcher(ProjectItemInstance item, string source)
            {
                _sourceFolder = source;

                ParsePatterns(
                    item.GetMetadataValue(FileMatchMetadata),
                    out _doMatchAll,
                    out _fileMatches,
                    out _fileRegexMatches,
                    out string fileWildcardMatch);
                ParsePatterns(
                    item.GetMetadataValue(FileExcludeMetadata),
                    out _,
                    out _fileExcludes,
                    out _fileRegexExcludes,
                    out _);
                ParsePatterns(
                    item.GetMetadataValue(DirExcludeMetadata),
                    out _,
                    out _dirExcludes,
                    out _dirRegexExcludes,
                    out _);

                // Optimization to only enumerate files which match the pattern.
                // This can only be done if there's exactly one pattern though.
                SearchPattern = _fileMatches.Count + _fileRegexMatches.Count == 1
                    ? _fileMatches.Count == 1
                        ? _fileMatches[0]
                        : fileWildcardMatch
                    : "*";
            }

            public string SearchPattern { get; }

            public bool IsMatch(string item, string subDirectory, bool isFile)
            {
                bool isMatch = false;
                bool isDeep = !string.IsNullOrEmpty(subDirectory);
                string deepDir = isDeep ? Path.Combine(_sourceFolder, subDirectory) : _sourceFolder;
                string deepItem = isDeep ? Path.Combine(subDirectory, item) : item;
                string rootedItem = Path.Combine(deepDir, item);

                if (isFile)
                {
                    if (_doMatchAll || (_fileMatches.Count == 0 && _fileRegexMatches.Count == 0))
                    {
                        isMatch = true;
                    }
                    else
                    {
                        foreach (string match in _fileMatches)
                        {
                            bool isRooted = Path.IsPathRooted(match);
                            if ((isRooted && string.Equals(match, rootedItem, StringComparison.OrdinalIgnoreCase)) ||
                               (!isRooted && string.Equals(match, item, StringComparison.OrdinalIgnoreCase)))
                            {
                                isMatch = true;
                                break;
                            }
                        }

                        if (!isMatch)
                        {
                            foreach (Regex match in _fileRegexMatches)
                            {
                                // Allow for wildcard directories but not rooted ones
                                if (match.IsMatch(item) || (isDeep && match.IsMatch(deepItem)))
                                {
                                    isMatch = true;
                                    break;
                                }
                            }
                        }
                    }

                    foreach (string exclude in _fileExcludes)
                    {
                        bool isRooted = Path.IsPathRooted(exclude);
                        if ((isRooted && rootedItem.Equals(exclude, StringComparison.OrdinalIgnoreCase)) ||
                           (!isRooted && item.Equals(exclude, StringComparison.OrdinalIgnoreCase)))
                        {
                            return false;
                        }
                    }

                    foreach (Regex exclude in _fileRegexExcludes)
                    {
                        // Allow for wildcard directories but not rooted ones
                        if (exclude.IsMatch(item) || (isDeep && exclude.IsMatch(deepItem)))
                        {
                            return false;
                        }
                    }
                }
                else
                {
                    isMatch = true;
                    foreach (string exclude in _dirExcludes)
                    {
                        // Exclude directories with matching sub-directory
                        if (rootedItem.EndsWith(exclude, StringComparison.OrdinalIgnoreCase))
                        {
                            return false;
                        }
                    }

                    foreach (Regex exclude in _dirRegexExcludes)
                    {
                        // Allow for wildcard directories but not rooted ones
                        if (exclude.IsMatch(item) || (isDeep && exclude.IsMatch(deepItem)))
                        {
                            return false;
                        }
                    }
                }

                return isMatch;
            }

            private static void ParsePatterns(
                string items,
                out bool doMatchAll,
                out List<string> literals,
                out List<Regex> regularExpressions,
                out string wildcardPattern)
            {
                doMatchAll = false;
                wildcardPattern = null;

                if (!string.IsNullOrEmpty(items))
                {
                    literals = new List<string>();
                    regularExpressions = new List<Regex>();

                    foreach (string item in items.Split(MultiSplits, StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (item == "*")
                        {
                            doMatchAll = true;
                        }
                        else if (item.IndexOfAny(Wildcards) >= 0)
                        {
                            if (wildcardPattern == null)
                            {
                                wildcardPattern = item;
                            }

                            string regexString = item
                                .Replace(@"\", @"\\", StringComparison.Ordinal)
                                .Replace(".", "[.]", StringComparison.Ordinal)
                                .Replace("?", ".", StringComparison.Ordinal)
                                .Replace("*", ".*", StringComparison.Ordinal);
                            regularExpressions.Add(new Regex($"^{regexString}$", RegexOptions.IgnoreCase));
                        }
                        else
                        {
                            literals.Add(item);
                        }
                    }
                }
                else
                {
                    literals = _emptyStringList;
                    regularExpressions = _emptyRegexList;
                }
            }
        }
    }
}
