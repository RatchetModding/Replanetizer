// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Parsers;
using Xunit;

namespace LibReplanetizer.Tests.Integration
{
    /// <summary>
    /// Integration tests that load Terrain data from real game fixture files.
    /// Tests are automatically skipped when <c>REPLANETIZER_TEST_FIXTURES</c>
    /// is not set or the files are absent.
    /// </summary>
    public class TerrainFixtureTests
    {
        private static readonly string? EngineFile =
            FixtureConfig.GetFixtureFile("engine.ps3") ?? FixtureConfig.GetFixtureFile("engine");

        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing engine.ps3.";

        [SkippableFact]
        public void GetTerrainModel_ReturnsNonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var terrain = parser.GetTerrainModel();
            Assert.NotNull(terrain);
        }

        [SkippableFact]
        public void GetTerrainModel_FragmentsIsNonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var terrain = parser.GetTerrainModel();
            Assert.NotNull(terrain.fragments);
        }

        [SkippableFact]
        public void GetTerrainModel_FragmentsIsNonEmpty()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var terrain = parser.GetTerrainModel();
            Assert.NotEmpty(terrain.fragments);
        }

        [SkippableFact]
        public void TerrainFragment_AllHaveNonNullModel()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var terrain = parser.GetTerrainModel();
            foreach (var frag in terrain.fragments)
                Assert.NotNull(frag.model);
        }

        [SkippableFact]
        public void TerrainFragment_CullingSizeIsPositive()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var terrain = parser.GetTerrainModel();
            foreach (var frag in terrain.fragments)
                Assert.True(frag.cullingSize > 0f, $"TerrainFragment has non-positive cullingSize={frag.cullingSize}.");
        }

        [SkippableFact]
        public void TerrainFragment_ModelIdMatchesFragmentModelId()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var terrain = parser.GetTerrainModel();
            foreach (var frag in terrain.fragments)
                Assert.Equal(frag.modelID, frag.model!.id);
        }

        [SkippableFact]
        public void TerrainFragment_VertexBufferIsNonEmpty()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var terrain = parser.GetTerrainModel();
            foreach (var frag in terrain.fragments)
                Assert.NotEmpty(frag.model!.vertexBuffer);
        }
    }
}
