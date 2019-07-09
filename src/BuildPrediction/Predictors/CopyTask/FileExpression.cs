// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors.CopyTask
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Helper class for evaluating MSBuild file item expression (e.g. @(Compile), %(Content.Filename), etc.)
    /// </summary>
    internal static class FileExpression
    {
        private static readonly Regex BatchedItemRegex = new Regex(@"%\((?<ItemType>[^\.\)]+)\.?(?<Metadata>[^\).]*?)\)", RegexOptions.Compiled);

        /// <summary>
        /// Evaluates file expressions, which are MSBuild expressions that
        /// evaluate to a list of files (e.g. @(Compile -> '%(filename)') or %(None.filename)).
        /// <param name="expression">An unprocessed string for a single expression.</param>
        /// <param name="project">The project where the expression exists.</param>
        /// <param name="task">The task where the expression exists.</param>
        /// <returns>the set of all files in the evaluated expression.</returns>
        public static List<string> EvaluateExpression(
            string expression,
            ProjectInstance project,
            ProjectTaskInstance task,
            out bool isBatched)
        {
            expression = expression.Trim();

            isBatched = !expression.StartsWith("@(", StringComparison.Ordinal) // This would make it a transform expression instead of a batched expression
                && expression.IndexOf("%(", StringComparison.Ordinal) >= 0;
            return isBatched
                ? EvaluateBatchedExpression(expression, project, task)
                : EvaluateLiteralExpression(expression, project, task);
        }

        /// <summary>
        /// Evaluates a literal expression, e.g. '$(Outdir)\foo.dll'.
        /// </summary>
        /// <param name="expression">An unprocessed string for a single expression.</param>
        /// <param name="project">The project where the expression exists.</param>
        /// <param name="task">The task where the expression exists.</param>
        /// <returns>The set of all files in the evaluated expression.</returns>
        private static List<string> EvaluateLiteralExpression(string expression, ProjectInstance project, ProjectTaskInstance task)
        {
            expression = ProcessExpression(expression, project, task);
            expression = project.ExpandString(expression);
            return expression.SplitStringList();
        }

        /// <summary>
        /// Evaluates a batch expression, e.g. '%(Compile.fileName).%(Compile.extension))'.
        /// </summary>
        /// <param name="expression">An unprocessed string for a single expression.</param>
        /// <param name="project">The project where the expression exists.</param>
        /// <param name="task">The task where the expression exists.</param>
        private static List<string> EvaluateBatchedExpression(string expression, ProjectInstance project, ProjectTaskInstance task)
        {
            expression = ProcessExpression(expression, project, task);

            // Copy task has batching in it. Get the batched items if possible, then parse inputs.
            Match regexMatch = BatchedItemRegex.Match(expression);
            if (regexMatch.Success)
            {
                // If the user didn't specify a metadata item, then we default to Identity
                string transformItem = string.IsNullOrEmpty(regexMatch.Groups[2].Value)
                    ? BatchedItemRegex.Replace(expression, @"%(Identity)")
                    : BatchedItemRegex.Replace(expression, @"%($2)");

                // Convert the batch into a transform. If this is an item -> metadata based transition then it will do the replacements for you.
                string expandedString = project.ExpandString($"@({regexMatch.Groups["ItemType"].Value}-> '{transformItem}')");
                return expandedString.SplitStringList();
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Trim and expand an expression.
        /// </summary>
        /// <param name="expression">An unprocessed string for a single expression.</param>
        /// <param name="project">The project where the expression exists.</param>
        /// <param name="task">The task where the expression exists.</param>
        private static string ProcessExpression(string expression, ProjectInstance project, ProjectTaskInstance task)
        {
            if (task != null)
            {
                string copyTaskFilePath = task.Location.File;

                // Process MsBuildThis* macros
                // ignore copy tasks within the proj - evaluation will just work.
                if (!copyTaskFilePath.Equals(project.FullPath, StringComparison.OrdinalIgnoreCase))
                {
                    // We leave off the trailing ')' to allow for macro operations. This could allow us to misdetect macros
                    // (e.g. $(MsBuildThisFileButNotReally), but should be rare and should still function correctly even if we
                    // do.
                    if (expression.IndexOf("$(MSBuildThisFile", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
#pragma warning disable CA1308 // Normalize strings to uppercase
                        expression = expression.ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
#pragma warning disable CA1307 // Specify StringComparison. We already normalized the string above
                        expression = expression.Replace("$(msbuildthisfiledirectory)", Path.GetDirectoryName(copyTaskFilePath) + "\\");
                        expression = expression.Replace("$(msbuildthisfile)", Path.GetFileName(copyTaskFilePath));
                        expression = expression.Replace("$(msbuildthisfileextension)", Path.GetExtension(copyTaskFilePath));
                        expression = expression.Replace("$(msbuildthisfilefullpath)", copyTaskFilePath);
                        expression = expression.Replace("$(msbuildthisfilename)", Path.GetFileNameWithoutExtension(copyTaskFilePath));
#pragma warning restore CA1307 // Specify StringComparison
                    }
                }
            }

            return expression;
        }
    }
}
