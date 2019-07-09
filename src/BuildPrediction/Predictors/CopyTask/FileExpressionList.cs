// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors.CopyTask
{
    using System.Collections.Generic;
    using Microsoft.Build.Execution;

    /// <summary>
    /// Contains a parsed list of file expressions as well as the list of files derived from evaluating said
    /// expressions.
    /// </summary>
    internal class FileExpressionList
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileExpressionList"/> class.
        /// </summary>
        /// <param name="rawFileListString">The unprocessed list of file expressions.</param>
        /// <param name="project">The project where the expression list exists.</param>
        /// <param name="task">The task where the expression list exists.</param>
        public FileExpressionList(string rawFileListString, ProjectInstance project, ProjectTaskInstance task)
        {
            List<string> expressions = rawFileListString.SplitStringList();
            NumExpressions = expressions.Count;

            var seenFiles = new HashSet<string>(PathComparer.Instance);
            foreach (string expression in expressions)
            {
                List<string> evaluatedFiles = FileExpression.EvaluateExpression(expression, project, task, out bool isBatched);
                if (isBatched)
                {
                    NumBatchExpressions++;
                }

                foreach (string file in evaluatedFiles)
                {
                    if (string.IsNullOrWhiteSpace(file))
                    {
                        continue;
                    }

                    if (seenFiles.Add(file))
                    {
                        DedupedFiles.Add(file);
                    }

                    AllFiles.Add(file);
                }
            }
        }

        /// <summary>
        /// Gets the set of all files in all of the expanded expressions. May include duplicates.
        /// </summary>
        public List<string> AllFiles { get; } = new List<string>();

        /// <summary>
        /// Gets the set of all files in the expanded expressions. Duplicates are removed.
        /// </summary>
        public List<string> DedupedFiles { get; } = new List<string>();

        /// <summary>
        /// Gets the total number of expressions in the file list.
        /// </summary>
        public int NumExpressions { get; }

        /// <summary>
        /// Gets the number of batch expressions in the file list.
        /// </summary>
        public int NumBatchExpressions { get; }
    }
}
