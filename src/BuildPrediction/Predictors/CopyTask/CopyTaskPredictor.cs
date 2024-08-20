// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Execution;

namespace Microsoft.Build.Prediction.Predictors.CopyTask
{
    /// <summary>
    /// Parses Copy tasks from Targets in the provided Project to predict inputs
    /// and outputs.
    /// </summary>
    /// <remarks>
    /// This predictor assumes that the Build target is the primary for MSBuild evaluation,
    /// and follows the Targets activated by that target, along with all custom Targets
    /// present in the current project file.
    /// </remarks>
    public sealed class CopyTaskPredictor : IProjectPredictor
    {
        private const string CopyTaskName = "Copy";
        private const string CopyTaskSourceFiles = "SourceFiles";
        private const string CopyTaskSourceFolders = "SourceFolders";
        private const string CopyTaskDestinationFiles = "DestinationFiles";
        private const string CopyTaskDestinationFolder = "DestinationFolder";

        /// <inheritdoc />
        public void PredictInputsAndOutputs(
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            // Determine the active Targets in this Project.
            var activeTargets = new Dictionary<string, ProjectTargetInstance>(StringComparer.OrdinalIgnoreCase);

            // Start with the default targets, initial targets and all of their parent targets, the closure of its dependencies.
            foreach (string target in projectInstance.DefaultTargets)
            {
                projectInstance.AddToActiveTargets(target, activeTargets);
            }

            foreach (string target in projectInstance.InitialTargets)
            {
                projectInstance.AddToActiveTargets(target, activeTargets);
            }

            // Aside from InitialTargets and DefaultTargets, for completeness of inputs/outputs detection,
            // include custom targets defined directly in this Project.
            // Note that this misses targets defined in any custom targets files.
            foreach (ProjectTargetInstance target in projectInstance.Targets.Values
                .Where(t => string.Equals(t.Location.File, projectInstance.ProjectFileLocation.File, PathComparer.Comparison)))
            {
                projectInstance.AddToActiveTargets(target.Name, activeTargets);
            }

            projectInstance.AddBeforeAndAfterTargets(activeTargets);

            // Then parse copy tasks for these targets.
            foreach (KeyValuePair<string, ProjectTargetInstance> target in activeTargets)
            {
                ParseCopyTask(target.Value, projectInstance, predictionReporter);
            }
        }

        /// <summary>
        /// Parses the input and output files for copy tasks of given target.
        /// </summary>
        private static void ParseCopyTask(
            ProjectTargetInstance target,
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            // Get all Copy tasks from targets.
            List<ProjectTaskInstance> tasks = target.Tasks
                .Where(task => string.Equals(task.Name, CopyTaskName, StringComparison.Ordinal))
                .ToList();

            if (tasks.Any() && projectInstance.EvaluateConditionCarefully(target.Condition))
            {
                foreach (ProjectTaskInstance task in tasks)
                {
                    if (projectInstance.EvaluateConditionCarefully(task.Condition))
                    {
                        bool hasSourceFiles = task.Parameters.TryGetValue(CopyTaskSourceFiles, out string sourceFiles) && !string.IsNullOrEmpty(sourceFiles);
                        bool hasSourceFolders = task.Parameters.TryGetValue(CopyTaskSourceFolders, out string sourceFolders) && !string.IsNullOrEmpty(sourceFolders);
                        bool hasDestinationFiles = task.Parameters.TryGetValue(CopyTaskDestinationFiles, out string destinationFiles) && !string.IsNullOrEmpty(destinationFiles);
                        bool hasDestinationFolder = task.Parameters.TryGetValue(CopyTaskDestinationFolder, out string destinationFolder) && !string.IsNullOrEmpty(destinationFolder);

                        // The task will nop if there are no sources.
                        if (!hasSourceFiles && !hasSourceFolders)
                        {
                            continue;
                        }

                        // The task will error if there is no destination
                        if (!hasDestinationFiles && !hasDestinationFolder)
                        {
                            continue;
                        }

                        // The task will error if both destination types are used.
                        if (hasDestinationFolder && hasDestinationFiles)
                        {
                            continue;
                        }

                        // SourceFolders and DestinationFiles can't be used together.
                        if (hasSourceFolders && hasDestinationFiles)
                        {
                            continue;
                        }

                        var inputs = EvaluateExpression(hasSourceFolders ? sourceFolders : sourceFiles, projectInstance, task);
                        if (inputs.NumExpressions == 0)
                        {
                            continue;
                        }

                        foreach (string file in inputs.Paths)
                        {
                            if (hasSourceFolders)
                            {
                                predictionReporter.ReportInputDirectory(file);
                            }
                            else
                            {
                                predictionReporter.ReportInputFile(file);
                            }
                        }

                        var outputs = EvaluateExpression(hasDestinationFolder ? destinationFolder : destinationFiles, projectInstance, task);
                        if (outputs.NumExpressions == 0)
                        {
                            continue;
                        }

                        // When using batch tokens, the user should specify exactly one total token, and it must appear in both the input and output.
                        // If not using batch tokens, then any number of other tokens is fine.
                        if ((outputs.NumBatchExpressions == 1 && outputs.NumExpressions == 1 &&
                             inputs.NumBatchExpressions == 1 && inputs.NumExpressions == 1) ||
                            (outputs.NumBatchExpressions == 0 && inputs.NumBatchExpressions == 0))
                        {
                            ProcessOutputs(inputs.Paths, outputs.Paths, hasDestinationFolder, predictionReporter);
                        }
                        else
                        {
                            // Ignore case we cannot handle.
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Validates that a task's outputs are sane. If so, predicts output directories.
        /// </summary>
        /// <param name="inputs">The inputs specified in SourceFiles on a copy task.</param>
        /// <param name="outputs">
        /// The outputs specified in the DestinationFolder or DestinationFiles attribute on a copy task.
        /// </param>
        /// <param name="copyTaskSpecifiesDestinationFolder">True if the user has specified DestinationFolder.</param>
        /// <param name="predictionReporter">A reporter to report predictions to.</param>
        private static void ProcessOutputs(
            List<string> inputs,
            List<string> outputs,
            bool copyTaskSpecifiesDestinationFolder,
            ProjectPredictionReporter predictionReporter)
        {
            for (int i = 0; i < inputs.Count; i++)
            {
                string predictedOutputDirectory;

                // If the user specified a destination folder, they could have specified an expression that evaluates to
                // either exactly one or N folders. We need to handle each case.
                if (copyTaskSpecifiesDestinationFolder)
                {
                    if (outputs.Count == 0)
                    {
                        // Output files couldn't be parsed, bail out.
                        break;
                    }

                    // If output directories isn't 1 or N, bail out.
                    if (inputs.Count != outputs.Count && outputs.Count > 1)
                    {
                        break;
                    }

                    predictedOutputDirectory = outputs.Count == 1 ? outputs[0] : outputs[i];
                }
                else
                {
                    if (i >= outputs.Count)
                    {
                        break;
                    }

                    // The output list is a set of files. Predict their directories.
                    predictedOutputDirectory = Path.GetDirectoryName(outputs[i]);
                }

                predictionReporter.ReportOutputDirectory(predictedOutputDirectory);
            }
        }

        private static (List<string> Paths, int NumExpressions, int NumBatchExpressions) EvaluateExpression(string rawFileListString, ProjectInstance project, ProjectTaskInstance task)
        {
            List<string> expressions = rawFileListString.SplitStringList();
            int numBatchExpressions = 0;

            List<string> paths = new();
            HashSet<string> seenPaths = new(PathComparer.Instance);
            foreach (string expression in expressions)
            {
                List<string> evaluatedFiles = FileExpression.EvaluateExpression(expression, project, task, out bool isBatched);
                if (isBatched)
                {
                    numBatchExpressions++;
                }

                foreach (string file in evaluatedFiles)
                {
                    if (string.IsNullOrWhiteSpace(file))
                    {
                        continue;
                    }

                    if (seenPaths.Add(file))
                    {
                        paths.Add(file);
                    }
                }
            }

            return (paths, expressions.Count, numBatchExpressions);
        }
    }
}