// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors
{
    /// <summary>
    /// Finds the code analysis ruleset and any ruleset or rule assemblies it includes as inputs.
    /// </summary>
    /// <remarks>
    /// This predictor parses the ruleset XML. In the event of invalid XML or an unexpected schema, the predictor will simply not report those inputs,
    /// rather than throwing exceptions and failing prediction altogether.
    /// </remarks>
    public sealed class CodeAnalysisRuleSetPredictor : IProjectPredictor
    {
        internal const string CodeAnalysisRuleSetPropertyName = "CodeAnalysisRuleSet";

        internal const string CodeAnalysisRuleSetDirectoriesPropertyName = "CodeAnalysisRuleSetDirectories";

        private readonly HashSet<string> _emptySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly List<string> _emptyList = new List<string>(0);

        // Often rulesets are reused across projects, so keep a cache to avoid parsing the same ruleset over and over.
        private readonly ConcurrentDictionary<string, HashSet<string>> _cachedInputs = new ConcurrentDictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            string ruleSetPath = projectInstance.GetPropertyValue(CodeAnalysisRuleSetPropertyName);
            if (string.IsNullOrWhiteSpace(ruleSetPath))
            {
                // Bail out fast if no rulset is configured.
                return;
            }

            string ruleSetDirectoriesStr = projectInstance.GetPropertyValue(CodeAnalysisRuleSetDirectoriesPropertyName);
            List<string> ruleSetDirectories = string.IsNullOrWhiteSpace(ruleSetDirectoriesStr)
                ? _emptyList
                : ruleSetDirectoriesStr.SplitStringList();

            HashSet<string> inputs = ParseRuleset(
                ruleSetPath,
                ruleSetDirectories,
                projectInstance.Directory,
                visitedRuleSets: null,
                isInCycle: false);
            foreach (string input in inputs)
            {
                predictionReporter.ReportInputFile(input);
            }
        }

        // Based on resolution logic from: https://github.com/Microsoft/msbuild/blob/master/src/Tasks/ResolveCodeAnalysisRuleSet.cs
        private static string GetResolvedRuleSetPath(
            string ruleSet,
            List<string> ruleSetDirectories,
            string projectDirectory)
        {
            if (string.IsNullOrEmpty(ruleSet))
            {
                return null;
            }

            if (ruleSet.Equals(Path.GetFileName(ruleSet), StringComparison.OrdinalIgnoreCase))
            {
                // This is a simple file name.
                // Check if the file exists in the MSBuild project directory.
                if (!string.IsNullOrEmpty(projectDirectory))
                {
                    string fullName = Path.GetFullPath(Path.Combine(projectDirectory, ruleSet));
                    if (File.Exists(fullName))
                    {
                        return fullName;
                    }
                }

                // Try the rule set directories.
                foreach (string directory in ruleSetDirectories)
                {
                    string fullPath = Path.GetFullPath(Path.Combine(directory, ruleSet));
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }
            else if (!Path.IsPathRooted(ruleSet))
            {
                // This is a path relative to the project.
                if (!string.IsNullOrEmpty(projectDirectory))
                {
                    string fullPath = Path.GetFullPath(Path.Combine(projectDirectory, ruleSet));
                    if (File.Exists(fullPath))
                    {
                        return fullPath;
                    }
                }
            }
            else
            {
                // This is a full path.
                string fullPath = Path.GetFullPath(ruleSet);
                if (File.Exists(fullPath))
                {
                    return fullPath;
                }
            }

            // We can't resolve the rule set to any existing file.
            return null;
        }

        /// <summary>
        /// Rulesets can be recursive and can have assembly inputs.
        /// This will return full paths for all rulesets and hint assemblies.
        /// </summary>
        /// <remarks>
        /// Below is an example of CodeAnalysisRuleSet.
        /// <![CDATA[
        /// <RuleSet Name="OurCustomRules" Description=" " ToolsVersion="15.0">
        ///  <Include Path="rulesets\globalizationrules.ruleset" Action="Default" />
        ///  <RuleHintPaths>
        ///    <Path>..\..\FxCop\FxCop.OurCustomRules.dll</Path>
        ///  </RuleHintPaths>
        /// </RuleSet>
        /// ]]>
        /// </remarks>
        private HashSet<string> ParseRuleset(
            string ruleSetPath,
            List<string> ruleSetDirectories,
            string currentRoot,
            HashSet<string> visitedRuleSets,
            bool isInCycle)
        {
            ruleSetPath = GetResolvedRuleSetPath(ruleSetPath, ruleSetDirectories, currentRoot);
            if (string.IsNullOrWhiteSpace(ruleSetPath))
            {
                return _emptySet;
            }

            if (_cachedInputs.TryGetValue(ruleSetPath, out HashSet<string> cachedResults))
            {
                return cachedResults;
            }

            // Cycle Detection
            if (visitedRuleSets != null && visitedRuleSets.Contains(ruleSetPath))
            {
                // We found a cycle. CodeAnalysis seems to be tolerant of this just uses the union of all the rulesets anyway, regardless of the cycle.
                // We're caching results for many rulesets for many projects, so we can't just stop here, as the result for this ruleset will then be incomplete.
                // However, the visited rule sets to get to the cycle might contain more than the cycle itself, so we can't use that list.
                // To handle this, we parse this rule set again starting at this file, which we know is part of the cycle. We also make sure *not* to cache
                // intermediate results while doing this inner parse (controlled by the isInCycle flag).The result will be the entire cycle, which we can cache.
                if (isInCycle)
                {
                    // We're done with the inner parse
                    return _emptySet;
                }
                else
                {
                    // Return the results of the inner parse.
                    // Passing an empty currentRoot since ruleSetPath is absolute at this point so currentRoot won't be used anyway.
                    // Passing an empty visitedRuleSets as we want to re-visit the entire cycle to gather all results from it. Passing the existing visitedRuleSets would immediate break and collect nothing.
                    return ParseRuleset(
                        ruleSetPath,
                        ruleSetDirectories,
                        string.Empty,
                        new HashSet<string>(StringComparer.OrdinalIgnoreCase),
                        true);
                }
            }

            try
            {
                XElement ruleset = XDocument.Load(ruleSetPath).Element("RuleSet");
                if (ruleset == null)
                {
                    // Error case. Bail out gracefully.
                    return _emptySet;
                }

                var results = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ruleSetPath,
                };

                string ruleSetPathDirectory = Path.GetDirectoryName(ruleSetPath);

                // Handle all included rulesets
                HashSet<string> newVisitedRuleSets = null;
                foreach (XElement includeElement in ruleset.Elements("Include"))
                {
                    var unexpandedIncludePath = includeElement.Attribute("Path")?.Value;
                    if (!string.IsNullOrEmpty(unexpandedIncludePath))
                    {
                        var expandedIncludePath = Environment.ExpandEnvironmentVariables(unexpandedIncludePath);

                        if (newVisitedRuleSets == null)
                        {
                            // Creating a new set each time we recurse could get expensive, but in practice the full graph
                            // of rulesets should be fairly small with lots of reuse across the repo.
                            newVisitedRuleSets = visitedRuleSets == null
                                ? new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                                : new HashSet<string>(visitedRuleSets, StringComparer.OrdinalIgnoreCase);
                            newVisitedRuleSets.Add(ruleSetPath);
                        }

                        var innerParseResults = ParseRuleset(expandedIncludePath, ruleSetDirectories, ruleSetPathDirectory, newVisitedRuleSets, isInCycle);
                        results.UnionWith(innerParseResults);
                    }
                }

                // Handle all hinted rule assemblies
                foreach (XElement ruleHintPathsElement in ruleset.Elements("RuleHintPaths"))
                {
                    foreach (XElement pathElement in ruleHintPathsElement.Elements("Path"))
                    {
                        var unexpandedRuleHintPath = pathElement.Value;
                        if (!string.IsNullOrEmpty(unexpandedRuleHintPath))
                        {
                            var expandedRuleHintPath = Environment.ExpandEnvironmentVariables(unexpandedRuleHintPath);
                            var absoluteRuleHintPath = Path.Combine(ruleSetPathDirectory, expandedRuleHintPath);
                            if (File.Exists(absoluteRuleHintPath))
                            {
                                results.Add(absoluteRuleHintPath);
                            }
                        }
                    }
                }

                // As described above, do not cache intermediate results inside a cycle.
                if (!isInCycle)
                {
                    _cachedInputs.TryAdd(ruleSetPath, results);
                }

                return results;
            }
            catch (XmlException)
            {
                // Error case. Bail out gracefully.
                return _emptySet;
            }
        }
    }
}