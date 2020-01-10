// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Microsoft.Build.Prediction.Predictors.CopyTask
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Build.Execution;

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
                        var inputs = new FileExpressionList(
                            task.Parameters[CopyTaskSourceFiles],
                            projectInstance,
                            task);
                        if (inputs.NumExpressions == 0)
                        {
                            continue;
                        }

                        foreach (var file in inputs.DedupedFiles)
                        {
                            predictionReporter.ReportInputFile(file);
                        }

                        bool hasDestinationFolder = task.Parameters.TryGetValue(
                            CopyTaskDestinationFolder,
                            out string destinationFolder);
                        bool hasDestinationFiles = task.Parameters.TryGetValue(
                            CopyTaskDestinationFiles,
                            out string destinationFiles);

                        if (hasDestinationFiles || hasDestinationFolder)
                        {
                            // Having both is an MSBuild violation, which it will complain about.
                            if (hasDestinationFolder && hasDestinationFiles)
                            {
                                continue;
                            }

                            string destination = destinationFolder ?? destinationFiles;

                            var outputs = new FileExpressionList(destination, projectInstance, task);

                            // When using batch tokens, the user should specify exactly one total token, and it must appear in both the input and output.
                            // Doing otherwise should be a BuildCop error. If not using batch tokens, then any number of other tokens is fine.
                            if ((outputs.NumBatchExpressions == 1 && outputs.NumExpressions == 1 &&
                                 inputs.NumBatchExpressions == 1 && inputs.NumExpressions == 1) ||
                                (outputs.NumBatchExpressions == 0 && inputs.NumBatchExpressions == 0))
                            {
                                ProcessOutputs(inputs, outputs, hasDestinationFolder, predictionReporter);
                            }
                            else
                            {
                                // Ignore case we cannot handle.
                            }
                        }
                        else
                        {
                            // Ignore malformed case.
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
            FileExpressionList inputs,
            FileExpressionList outputs,
            bool copyTaskSpecifiesDestinationFolder,
            ProjectPredictionReporter predictionReporter)
        {
            for (int i = 0; i < inputs.DedupedFiles.Count; i++)
            {
                string predictedOutputDirectory;

                // If the user specified a destination folder, they could have specified an expression that evaluates to
                // either exactly one or N folders. We need to handle each case.
                if (copyTaskSpecifiesDestinationFolder)
                {
                    if (outputs.DedupedFiles.Count == 0)
                    {
                        // Output files couldn't be parsed, bail out.
                        break;
                    }

                    // If output directories isn't 1 or N, bail out.
                    if (inputs.DedupedFiles.Count != outputs.DedupedFiles.Count && outputs.DedupedFiles.Count > 1)
                    {
                        break;
                    }

                    predictedOutputDirectory = outputs.DedupedFiles.Count == 1 ? outputs.DedupedFiles[0] : outputs.DedupedFiles[i];
                }
                else
                {
                    if (i >= outputs.DedupedFiles.Count)
                    {
                        break;
                    }

                    // The output list is a set of files. Predict their directories.
                    predictedOutputDirectory = Path.GetDirectoryName(outputs.DedupedFiles[i]);
                }

                predictionReporter.ReportOutputDirectory(predictedOutputDirectory);
            }
        }
    }
}
