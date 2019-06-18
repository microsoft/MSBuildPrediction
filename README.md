# Microsoft.Build.Prediction
[![Build Status](https://dev.azure.com/ms/BuildXL/_apis/build/status/microsoft.MSBuildPrediction-CI?branchName=master)](https://dev.azure.com/ms/BuildXL/_build/latest?definitionId=154&branchName=master)

This library runs predictors against evaluated MSBuild [Project]([https://docs.microsoft.com/en-us/dotnet/api/microsoft.build.evaluation.project]) instances to predict file and directory inputs that will be read, and output directories that will be written, by the project.

Predictors are implementations of the `IProjectPredictor` interface. Execution logic in this library applies the predictors in parallel to a given Project. The library aggregates results from all predictors into a final set of predicted inputs and outputs for a Project.

Input and output predictions produced here can be used, for instance, for Project build caching and sandboxing. Predicted inputs are added to the project file itself and known global files and folders from SDKs and tools to produce a set of files and folders that can be hashed and summarized to produce an inputs hash that can be used to look up the results of previous build executions. The more accurate and complete the predicted set of inputs, the narrower the set of cached output results, and the better the cache performance. Predicted build output directories are used to guide static construction and analysis of build sandboxes.

## Usage
Basic usage:
```cs
List<IProjectPredictor> predictors = new List<IProjectPredictor>();
predictors.AddRange(ProjectPredictors.AllPredictors);
// Add any custom IProjectPredictor implementations

var predictionExecutor = new ProjectPredictionExecutor(predictors);

Project project = /* Create an MSBuild Project */;
ProjectPredictions predictions = predictionExecutor.PredictInputsAndOutputs(project);
```

Providing a custom `IProjectPredictionCollector`:
```cs
IReadOnlyCollection<IProjectPredictor> predictors = ProjectPredictors.AllPredictors;

var predictionExecutor = new ProjectPredictionExecutor(predictors);
var predictionCollector = new CustomProjectPredictionCollector();

Project project = /* Create an MSBuild Project */;
predictionExecutor.PredictInputsAndOutputs(project, predictionCollector);
```

Using alongside MSBuild's `ProjectGraph`:
```cs
string projectFile = /* Your entry project file */;
ProjectCollection projectCollection = ProjectCollection.GlobalProjectCollection;

// Need to keep all Projects since ProjectGraph returns a ProjectInstance but ProjectPredictionExecutor requires a Project.
var projects = new List<Project>();
var projectGraph = new ProjectGraph(
    projectFile,
    projectCollection
    (string projectPath, Dictionary<string, string> globalProperties, ProjectCollection projCollection) =>
    {
        var project = new Project(projectPath, globalProperties, null, projCollection);
        lock (projects)
        {
            projects.Add(project);
        }

        return project.CreateProjectInstance(ProjectInstanceSettings.ImmutableWithFastItemLookup);
    });

IReadOnlyCollection<IProjectPredictor> predictors = ProjectPredictors.AllPredictors;

// Using single-threaded prediction since we're parallelizing on projects instead.
var predictionExecutor = new ProjectPredictionExecutor(predictors, new ProjectPredictionOptions { MaxDegreeOfParallelism = 1 });

var predictionCollector = new CustomProjectPredictionCollector();

Parallel.ForEach(
    projects.ToArray(),
    new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount },
    project => predictionExecutor.PredictInputsAndOutputs(project, predictionCollector));
```

## Design
See [Design](documentation/design.md).

## Contributing
This project welcomes contributions and suggestions.  Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.microsoft.com.

When you submit a pull request, a CLA-bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., label, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## License
Microsoft.Build.Prediction is licensed under the [MIT License](LICENSE).
