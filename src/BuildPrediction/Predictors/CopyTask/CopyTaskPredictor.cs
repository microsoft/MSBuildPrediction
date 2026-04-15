// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
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

        // Matches simple MSBuild property references like $(PropertyName), excluding complex
        // expressions with dots (e.g., $(Foo.Bar)), nested references, or function calls.
        private static readonly Regex SimplePropertyReferenceRegex = new Regex(@"\$\(([A-Za-z_][A-Za-z0-9_]*)\)", RegexOptions.Compiled);

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
        /// Iterates through target children in order so that PropertyGroup and ItemGroup
        /// definitions within the target are evaluated before Copy tasks that reference them.
        /// </summary>
        private static void ParseCopyTask(
            ProjectTargetInstance target,
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            if (!projectInstance.EvaluateConditionCarefully(target.Condition))
            {
                return;
            }

            // Check if this target has any Copy tasks at all before doing work.
            bool hasCopyTasks = false;
            foreach (ProjectTargetInstanceChild child in target.Children)
            {
                if (child is ProjectTaskInstance taskChild
                    && string.Equals(taskChild.Name, CopyTaskName, StringComparison.Ordinal))
                {
                    hasCopyTasks = true;
                    break;
                }
            }

            if (!hasCopyTasks)
            {
                return;
            }

            // Track properties and items modified inside this target so they can be restored
            // after processing. Properties set inside targets are not evaluated during static
            // analysis, so we replay them here to make them available to Copy task expressions.
            var originalProperties = new Dictionary<string, (bool Existed, string Value)>(StringComparer.OrdinalIgnoreCase);
            var addedItems = new List<(string ItemType, ProjectItemInstance Item)>();

            try
            {
                foreach (ProjectTargetInstanceChild child in target.Children)
                {
                    if (child is ProjectPropertyGroupTaskInstance propertyGroup)
                    {
                        EvaluatePropertyGroup(propertyGroup, projectInstance, originalProperties);
                    }
                    else if (child is ProjectItemGroupTaskInstance itemGroup)
                    {
                        EvaluateItemGroup(itemGroup, projectInstance, addedItems);
                    }
                    else if (child is ProjectTaskInstance task
                        && string.Equals(task.Name, CopyTaskName, StringComparison.Ordinal))
                    {
                        ProcessCopyTask(task, projectInstance, predictionReporter);
                    }
                }
            }
            finally
            {
                // Restore modified properties to avoid affecting evaluation of other targets,
                // since targets are not necessarily traversed in execution order.
                foreach (KeyValuePair<string, (bool Existed, string Value)> kvp in originalProperties)
                {
                    if (kvp.Value.Existed)
                    {
                        projectInstance.SetProperty(kvp.Key, kvp.Value.Value);
                    }
                    else
                    {
                        projectInstance.RemoveProperty(kvp.Key);
                    }
                }

                // Restore modified items
                foreach ((string itemType, ProjectItemInstance item) in addedItems)
                {
                    projectInstance.RemoveItem(item);
                }
            }
        }

        /// <summary>
        /// Evaluates a PropertyGroup defined inside a target, setting properties on the
        /// project instance so they are available for subsequent expression expansion.
        /// </summary>
        private static void EvaluatePropertyGroup(
            ProjectPropertyGroupTaskInstance propertyGroup,
            ProjectInstance projectInstance,
            Dictionary<string, (bool Existed, string Value)> originalProperties)
        {
            if (!projectInstance.EvaluateConditionCarefully(propertyGroup.Condition))
            {
                return;
            }

            foreach (ProjectPropertyGroupTaskPropertyInstance property in propertyGroup.Properties)
            {
                if (!projectInstance.EvaluateConditionCarefully(property.Condition))
                {
                    continue;
                }

                // Snapshot the original value before modification (only on first encounter)
                if (!originalProperties.ContainsKey(property.Name))
                {
                    ProjectPropertyInstance existing = projectInstance.GetProperty(property.Name);
                    originalProperties[property.Name] = existing != null
                        ? (true, existing.EvaluatedValue)
                        : (false, null);
                }

                string evaluatedValue = projectInstance.ExpandString(property.Value);
                projectInstance.SetProperty(property.Name, evaluatedValue);
            }
        }

        /// <summary>
        /// Evaluates an ItemGroup defined inside a target, adding items to the
        /// project instance so they are available for subsequent expression expansion.
        /// </summary>
        private static void EvaluateItemGroup(
            ProjectItemGroupTaskInstance itemGroup,
            ProjectInstance projectInstance,
            List<(string ItemType, ProjectItemInstance Item)> addedItems)
        {
            if (!projectInstance.EvaluateConditionCarefully(itemGroup.Condition))
            {
                return;
            }

            foreach (ProjectItemGroupTaskItemInstance item in itemGroup.Items)
            {
                if (!projectInstance.EvaluateConditionCarefully(item.Condition))
                {
                    continue;
                }

                string evaluatedInclude = projectInstance.ExpandString(item.Include);
                if (string.IsNullOrEmpty(evaluatedInclude))
                {
                    continue;
                }

                foreach (string includePart in evaluatedInclude.SplitStringList())
                {
                    ProjectItemInstance addedItem = projectInstance.AddItem(item.ItemType, includePart);
                    addedItems.Add((item.ItemType, addedItem));
                }
            }
        }

        /// <summary>
        /// Processes a single Copy task instance, evaluating its parameters and reporting predictions.
        /// </summary>
        private static void ProcessCopyTask(
            ProjectTaskInstance task,
            ProjectInstance projectInstance,
            ProjectPredictionReporter predictionReporter)
        {
            if (!projectInstance.EvaluateConditionCarefully(task.Condition))
            {
                return;
            }

            bool hasSourceFiles = task.Parameters.TryGetValue(CopyTaskSourceFiles, out string sourceFiles) && !string.IsNullOrEmpty(sourceFiles);
            bool hasSourceFolders = task.Parameters.TryGetValue(CopyTaskSourceFolders, out string sourceFolders) && !string.IsNullOrEmpty(sourceFolders);
            bool hasDestinationFiles = task.Parameters.TryGetValue(CopyTaskDestinationFiles, out string destinationFiles) && !string.IsNullOrEmpty(destinationFiles);
            bool hasDestinationFolder = task.Parameters.TryGetValue(CopyTaskDestinationFolder, out string destinationFolder) && !string.IsNullOrEmpty(destinationFolder);

            // The task will nop if there are no sources.
            if (!hasSourceFiles && !hasSourceFolders)
            {
                return;
            }

            // The task will error if there is no destination
            if (!hasDestinationFiles && !hasDestinationFolder)
            {
                return;
            }

            // The task will error if both destination types are used.
            if (hasDestinationFolder && hasDestinationFiles)
            {
                return;
            }

            // SourceFolders and DestinationFiles can't be used together.
            if (hasSourceFolders && hasDestinationFiles)
            {
                return;
            }

            var inputs = EvaluateExpression(hasSourceFolders ? sourceFolders : sourceFiles, projectInstance, task);
            if (inputs.NumExpressions == 0)
            {
                return;
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

            // Skip output prediction if the destination expression references properties that
            // cannot be resolved (e.g., properties set only at build time by task outputs).
            // Predicting with unresolved properties would produce incorrect paths.
            string rawDestinationExpression = hasDestinationFolder ? destinationFolder : destinationFiles;
            if (HasUnresolvableProperties(rawDestinationExpression, projectInstance))
            {
                return;
            }

            var outputs = EvaluateExpression(rawDestinationExpression, projectInstance, task);
            if (outputs.NumExpressions == 0)
            {
                return;
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

        /// <summary>
        /// Checks whether an expression contains simple MSBuild property references $(PropertyName)
        /// that cannot be resolved to a non-empty value. Properties that remain unresolved would
        /// produce incorrect path predictions (e.g., $(Undefined)\folder expands to \folder).
        /// </summary>
        private static bool HasUnresolvableProperties(string expression, ProjectInstance projectInstance)
        {
            foreach (Match match in SimplePropertyReferenceRegex.Matches(expression))
            {
                string propertyName = match.Groups[1].Value;
                if (string.IsNullOrEmpty(projectInstance.GetPropertyValue(propertyName)))
                {
                    return true;
                }
            }

            return false;
        }
    }
}