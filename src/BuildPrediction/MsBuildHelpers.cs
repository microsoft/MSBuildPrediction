// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Build.Exceptions;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction
{
    /// <summary>
    /// Helper methods for working with the MSBuild object model.
    /// </summary>
    internal static class MsBuildHelpers
    {
        /// <summary>
        /// MSBuild include list-string delimiters.
        /// </summary>
        /// <remarks>
        /// These are common split characters for dealing with MSBuild string-lists of the form
        /// 'item1;item2;item3', '   item1    ; \n\titem2 ; \r\n       item3', and so forth.
        /// </remarks>
        private static readonly char[] IncludeDelimiters = { ';', '\n', '\r', '\t' };

        private static readonly string RelativeSpecifier = "." + Path.DirectorySeparatorChar;

        /// <summary>
        /// Splits a given file list based on delimiters into a size-optimized list.
        /// If you only need an Enumerable, use <see cref="SplitStringListEnumerable" />.
        /// </summary>
        /// <param name="stringList">
        /// An MSBuild string-list, where whitespace is ignored and the semicolon ';' is used as a separator.
        /// </param>
        /// <returns>A size-optimized list of strings resulting from parsing the string-list.</returns>
        public static List<string> SplitStringList(this string stringList)
        {
            string[] split = stringList.Trim().Split(IncludeDelimiters, StringSplitOptions.RemoveEmptyEntries);
            var splitList = new List<string>(split.Length);
            foreach (string s in split)
            {
                string trimmed = s.Trim();
                if (trimmed.Length > 0)
                {
                    splitList.Add(trimmed);
                }
            }

            return splitList;
        }

        /// <summary>
        /// Splits a given file list based on delimiters into an enumerable.
        /// If you need a size-optimized list, use <see cref="SplitStringList"/>.
        /// </summary>
        /// <param name="stringList">
        /// An MSBuild string-list, where whitespace is ignored and the semicolon ';' is used as a separator.
        /// </param>
        /// <returns>A size-optimized list of strings resulting from parsing the string-list.</returns>
        public static IEnumerable<string> SplitStringListEnumerable(this string stringList)
        {
            return stringList.Trim().Split(IncludeDelimiters, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => s.Length > 0);
        }

        /// <summary>
        /// Evaluates a given value in a project's context.
        /// </summary>
        /// <param name="projectInstance">The MSBuild Project instance to use for the evaluation context.</param>
        /// <param name="unevaluatedValue">Unevaluated value.</param>
        /// <returns>List of evaluated values.</returns>
        public static IEnumerable<string> EvaluateValue(this ProjectInstance projectInstance, string unevaluatedValue)
        {
            if (string.IsNullOrWhiteSpace(unevaluatedValue))
            {
                return Enumerable.Empty<string>();
            }

            string evaluated = projectInstance.ExpandString(unevaluatedValue);
            return SplitStringListEnumerable(evaluated);
        }

        /// <summary>
        /// Given a target name, gets set of targets that are to be executed,
        /// for the provided target name and all targets that those depend on.
        /// </summary>
        /// <param name="projectInstance">An MSBuild Project instance to use for context.</param>
        /// <param name="evaluatedTargetName">Evaluated target name that we should analyze.</param>
        /// <param name="activeTargets">Collection into which targets should be added.</param>
        public static bool AddToActiveTargets(
            this ProjectInstance projectInstance,
            string evaluatedTargetName,
            Dictionary<string, ProjectTargetInstance> activeTargets)
        {
            // Avoid circular dependencies
            if (activeTargets.ContainsKey(evaluatedTargetName))
            {
                return false;
            }

            // The Project or its includes might not actually include the target name.
            if (projectInstance.Targets.TryGetValue(evaluatedTargetName, out ProjectTargetInstance target)
                && projectInstance.EvaluateConditionCarefully(target.Condition))
            {
                activeTargets.Add(evaluatedTargetName, target);

                // Parse all parent targets that current target depends on.
                var dependsOnTargets = projectInstance.EvaluateValue(target.DependsOnTargets);
                foreach (string dependsOnTarget in dependsOnTargets)
                {
                    AddToActiveTargets(projectInstance, dependsOnTarget, activeTargets);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Expand (recursively) set of active targets to include targets which reference any
        /// of the active targets with BeforeTarget or AfterTarget.
        /// </summary>
        /// <param name="projectInstance">An MSBuild Project instance to use for context.</param>
        /// <param name="activeTargets">
        /// Set of active targets. Will be modified in place to add targets that reference this
        /// graph with BeforeTarget or AfterTarget.
        /// </param>
        public static void AddBeforeAndAfterTargets(this ProjectInstance projectInstance, Dictionary<string, ProjectTargetInstance> activeTargets)
        {
            var newTargetsToConsider = true;
            while (newTargetsToConsider)
            {
                newTargetsToConsider = false;

                foreach (KeyValuePair<string, ProjectTargetInstance> pair in projectInstance.Targets)
                {
                    string targetName = pair.Key;
                    ProjectTargetInstance targetInstance = pair.Value;

                    // If the target is not already in our list of active targets ...
                    if (!activeTargets.ContainsKey(targetName))
                    {
                        IEnumerable<string> hookedTargets = projectInstance.EvaluateValue(targetInstance.AfterTargets)
                            .Concat(projectInstance.EvaluateValue(targetInstance.BeforeTargets));
                        foreach (string hookedTarget in hookedTargets)
                        {
                            // ... and it hooks a running target with BeforeTargets/AfterTargets ...
                            if (activeTargets.ContainsKey(hookedTarget))
                            {
                                // ... then add it to the list of running targets ...
                                newTargetsToConsider |= projectInstance.AddToActiveTargets(targetName, activeTargets);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Evaluates a condition in the context of a flattened ProjectInstance,
        /// avoiding processing where the condition contains constructs that are
        /// difficult to evaluate statically.
        /// </summary>
        /// <returns>
        /// If condition true or false. If any MSBuild project exception is thrown during evaluation,
        /// result will be false.
        /// </returns>
        /// <exception cref="InvalidProjectFileException">
        /// Thrown by MSBuild when evaluation fails. This exception cannot be easily
        /// handled locally, e.g. by catching and returning false, as it will usually
        /// leave the Project in a bad state, e.g. an empty Targets collection,
        /// which will affect downstream code that tries to evaluate targets.
        /// </exception>
        public static bool EvaluateConditionCarefully(this ProjectInstance projectInstance, string condition)
        {
            // To avoid extra work, return true (default) if condition is empty.
            if (string.IsNullOrWhiteSpace(condition))
            {
                return true;
            }

            // We cannot handle %(...) metadata accesses in conditions. For example, see these conditions
            // in Microsoft.WebApplication.targets:
            //
            //   <Copy SourceFiles="@(Content)" Condition="'%(Content.Link)' == ''"
            //   <Copy SourceFiles="@(Content)" Condition="!$(DisableLinkInCopyWebApplication) And '%(Content.Link)' != ''"
            //
            // Attempting to evaluate these conditions throws an MSB4191 exception from
            // Project.ReevaluateIfNecessary(), trashing Project (Project.Targets collection
            // becomes empty, for example). Extra info at:
            // http://stackoverflow.com/questions/4721879/ms-build-access-compiler-settings-in-a-subsequent-task
            // ProjectInstance.EvaluateCondition() also does not support bare metadata based condition parsing,
            // it uses the internal Expander class with option ExpandPropertiesAndItems but not the
            // more extensive ExpandAll or ExpandMetadata.
            // https://github.com/Microsoft/msbuild/blob/master/src/Build/Instance/ProjectInstance.cs#L1763
            if (condition.IndexOf("%(", StringComparison.Ordinal) != -1)
            {
                return false;
            }

            try
            {
                return projectInstance.EvaluateCondition(condition);
            }
            catch (InvalidProjectFileException)
            {
                // In some cases, the condition may not evaluate properly until targets have actually been executed.
                // For example, the condition `Condition="$(Foo)"` where the property Foo is not defined except inside a target which runs before the target with the condition.
                // To be conservative, assume these conditions evaluate to true.
                return true;
            }
        }

        /// <summary>
        /// Determines whether the CopyToOutputDirectory metadata is a value which would typically cause it to be copied.
        /// </summary>
        /// <remarks>
        /// See the GetCopyToOutputDirectoryItems target: https://github.com/microsoft/msbuild/blob/master/src/Tasks/Microsoft.Common.CurrentVersion.targets.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ShouldCopyToOutputDirectory(this ProjectItemInstance item)
        {
            var copyToOutputDirectoryValue = item.GetMetadataValue("CopyToOutputDirectory");
            if (copyToOutputDirectoryValue.Equals("Always", StringComparison.OrdinalIgnoreCase)
                || copyToOutputDirectoryValue.Equals("PreserveNewest", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determins what the TargetPath metadata would be set to after calling the AssignTargetPath task.
        /// </summary>
        /// <remarks>
        /// See: https://github.com/microsoft/msbuild/blob/master/src/Tasks/AssignTargetPath.cs.
        /// </remarks>
        public static string GetTargetPath(this ProjectItemInstance item)
        {
            string link = item.GetMetadataValue("Link");

            // If file has a link, use that.
            if (!string.IsNullOrEmpty(link))
            {
                return link;
            }

            var evaluatedInclude = item.EvaluatedInclude;

            // If the file path is relative and doesn't contain any relative specifiers then just use the file path as-is
            if (!Path.IsPathRooted(evaluatedInclude) && !evaluatedInclude.Contains(RelativeSpecifier, StringComparison.Ordinal))
            {
                return evaluatedInclude;
            }

            // Normalize the path
            string evaluatedIncludeFullPath = Path.GetFullPath(evaluatedInclude);

            string projectDir = item.Project.Directory;
            bool projectDirHasTrailingSlash = projectDir[projectDir.Length - 1] == Path.DirectorySeparatorChar;

            // If the item is under the project dir, return the relative path from the project dir.
            if (evaluatedIncludeFullPath.StartsWith(projectDir, StringComparison.OrdinalIgnoreCase)
                && (projectDirHasTrailingSlash
                    || (evaluatedIncludeFullPath.Length > projectDir.Length && evaluatedIncludeFullPath[projectDir.Length] == Path.DirectorySeparatorChar)))
            {
                return evaluatedIncludeFullPath.Substring(projectDir.Length + (projectDirHasTrailingSlash ? 0 : 1));
            }

            // The item is not under the project dir. Return the filename only.
            return Path.GetFileName(evaluatedInclude);
        }
    }
}