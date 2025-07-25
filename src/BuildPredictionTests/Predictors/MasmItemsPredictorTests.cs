// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#if !NETCOREAPP
using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Prediction.Predictors;
using Xunit;

// These tests rely on VCTargetsPath, which isn't available in the netcoreapp flavor of MSBuild.
// MASM files and C++ builds in general aren't available on netcoreapp, so skipping these tests is OK.
namespace Microsoft.Build.Prediction.Tests.Predictors
{
    public class MasmItemsPredictorTests
    {
        private const string AsmFile = "Foo.asm";

        private const string ObjFile = "Foo.obj";

        private const string ListingFile = "listing.txt";

        private const string BrowseFile = "browse.txt";

        private static readonly KeyValuePair<string, string> MasmItemMetadata =
            CreateMetaData(MasmItemsPredictor.IncludePathsMetadata, "asm-include;asm-include2");

        private static readonly PredictedItem[] ExpectedInputFiles = new[]
        {
            new PredictedItem(AsmFile, nameof(MasmItemsPredictor)),
        };

        private static readonly PredictedItem[] ExpectedOutputFiles = new[]
        {
            new PredictedItem(ObjFile, nameof(MasmItemsPredictor)),
        };

        private static readonly PredictedItem[] ExpectedInputDirectories = new[]
        {
            new PredictedItem("asm-include", nameof(MasmItemsPredictor)),
            new PredictedItem("asm-include2", nameof(MasmItemsPredictor)),
        };

        [Fact]
        public void MasmItemsWithMetadataInline()
        {
            ProjectRootElement project = CreateMasmTestProject();
            project.AddItem(MasmItemsPredictor.MasmItemName, AsmFile, new[] { MasmItemMetadata });

            project.AssertPredictions<MasmItemsPredictor>(
                ExpectedInputFiles,
                ExpectedInputDirectories,
                ExpectedOutputFiles,
                null);
        }

        [Fact]
        public void MasmItemsWithItemGroupDefinitionMetadata()
        {
            ProjectRootElement project = CreateMasmTestProject();

            project.AddItemDefinition(MasmItemsPredictor.MasmItemName)
                .AddMetadata(MasmItemMetadata.Key, MasmItemMetadata.Value);

            project.AddItem(MasmItemsPredictor.MasmItemName, AsmFile);

            project.AssertPredictions<MasmItemsPredictor>(
                ExpectedInputFiles,
                ExpectedInputDirectories,
                ExpectedOutputFiles,
                null);
        }

        [Fact]
        public void MasmItemsWithBrowseFile()
        {
            ProjectRootElement project = CreateMasmTestProject();

            var browseFileMetadata = CreateMetaData(MasmItemsPredictor.BrowseFileMetadata, BrowseFile);
            project.AddItem(MasmItemsPredictor.MasmItemName, AsmFile, new[] { browseFileMetadata, MasmItemMetadata });

            project.AssertPredictions<MasmItemsPredictor>(
                ExpectedInputFiles,
                ExpectedInputDirectories,
                ExpectedOutputFiles.Union(PredictItems(BrowseFile)).ToArray(),
                null);
        }

        [Fact]
        public void MasmItemsWithAssembledCodeListingFile()
        {
            ProjectRootElement project = CreateMasmTestProject();

            var listingFileMetadata = CreateMetaData(MasmItemsPredictor.AssembledCodeListingFileMetadata, ListingFile);
            project.AddItem(MasmItemsPredictor.MasmItemName, AsmFile, new[] { listingFileMetadata, MasmItemMetadata });

            project.AssertPredictions<MasmItemsPredictor>(
                ExpectedInputFiles,
                ExpectedInputDirectories,
                ExpectedOutputFiles.Union(PredictItems(ListingFile)).ToArray(),
                null);
        }

        [Fact]
        public void MasmItemsExcludedFromBuild()
        {
            ProjectRootElement project = CreateMasmTestProject();

            var excludeMetadata = CreateMetaData(MasmItemsPredictor.ExcludedFromBuildMetadata, bool.TrueString);
            project.AddItem(MasmItemsPredictor.MasmItemName, AsmFile, new[] { excludeMetadata });

            // Validate that if the MASM item is defined, but marked ExcludedFromBuild that we don't emit it.
            project.AssertNoPredictions<MasmItemsPredictor>();
        }

        private static KeyValuePair<string, string> CreateMetaData(string name, string value) =>
            new KeyValuePair<string, string>(name, value);

        private ProjectRootElement CreateMasmTestProject()
        {
            ProjectRootElement projectRootElement = ProjectRootElement.Create();
            projectRootElement.AddImport(@"$(VCTargetsPath)\BuildCustomizations\masm.props");
            projectRootElement.AddImport(@"$(VCTargetsPath)\BuildCustomizations\masm.targets");

            return projectRootElement;
        }

        private PredictedItem[] PredictItems(params string[] predictions)
        {
            PredictedItem[] items = new PredictedItem[predictions.Length];

            for (int i = 0; i < predictions.Length; i++)
            {
                items[i] = new PredictedItem(predictions[i], nameof(MasmItemsPredictor));
            }

            return items;
        }
    }
}
#endif