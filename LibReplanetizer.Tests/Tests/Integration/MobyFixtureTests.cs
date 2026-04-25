// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using LibReplanetizer.Parsers;
using Xunit;

namespace LibReplanetizer.Tests.Integration
{
    /// <summary>
    /// Integration tests that load Moby data from real game fixture files.
    /// Tests are automatically skipped when <c>REPLANETIZER_TEST_FIXTURES</c>
    /// is not set or the files are absent.
    /// </summary>
    public class MobyFixtureTests
    {
        private static readonly string? GameplayFile = FixtureConfig.GetFixtureFile("gameplay_ntsc");
        private static readonly string? EngineFile =
            FixtureConfig.GetFixtureFile("engine.ps3") ?? FixtureConfig.GetFixtureFile("engine");

        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing gameplay_ntsc and engine.ps3.";

        private static List<Moby> LoadMobies()
        {
            using var engineParser = new EngineParser(EngineFile!);
            var mobyModels = engineParser.GetMobyModels();

            using var gameplayParser = new GameplayParser(GameType.RaC1, GameplayFile!);
            var pVars = gameplayParser.GetPvars();
            return gameplayParser.GetMobies(mobyModels, pVars);
        }

        [SkippableFact]
        public void GetMobies_ReturnsNonEmptyList()
        {
            Skip.If(GameplayFile == null || EngineFile == null, SkipMsg);
            var mobies = LoadMobies();
            Assert.NotEmpty(mobies);
        }

        [SkippableFact]
        public void GetMobies_AllHaveValidScale()
        {
            Skip.If(GameplayFile == null || EngineFile == null, SkipMsg);
            var mobies = LoadMobies();
            foreach (var moby in mobies)
            {
                Assert.True(moby.scale.X > 0f, $"Moby id={moby.mobyID} has non-positive scale {moby.scale.X}.");
            }
        }

        [SkippableFact]
        public void GetMobies_PvarIndexConsistentWithPvars()
        {
            Skip.If(GameplayFile == null || EngineFile == null, SkipMsg);
            var mobies = LoadMobies();
            // Mobies with pvarIndex == -1 must have empty pVars; others must have non-null pVars.
            foreach (var moby in mobies)
            {
                if (moby.pvarIndex == -1)
                    Assert.Empty(moby.pVars);
                else
                    Assert.NotNull(moby.pVars);
            }
        }

        [SkippableFact]
        public void GetMobies_SpawnTypeIsNonNegative()
        {
            Skip.If(GameplayFile == null || EngineFile == null, SkipMsg);
            var mobies = LoadMobies();
            foreach (var moby in mobies)
                Assert.True((int) moby.spawnType >= 0);
        }

        [SkippableFact]
        public void GetMobies_DrawDistanceIsNonNegative()
        {
            Skip.If(GameplayFile == null || EngineFile == null, SkipMsg);
            var mobies = LoadMobies();
            foreach (var moby in mobies)
                Assert.True(moby.drawDistance >= 0, $"Moby id={moby.mobyID} has negative drawDistance.");
        }

        [SkippableFact]
        public void Moby_ToByteArray_HasCorrectLength()
        {
            Skip.If(GameplayFile == null || EngineFile == null, SkipMsg);
            var mobies = LoadMobies();
            foreach (var moby in mobies)
            {
                byte[] bytes = moby.ToByteArray();
                Assert.Equal(GameType.RaC1.mobyElemSize, bytes.Length);
            }
        }

        [SkippableFact]
        public void Moby_SerializeRoundTrip_ModelIdPreserved()
        {
            Skip.If(GameplayFile == null || EngineFile == null, SkipMsg);
            var mobies = LoadMobies();
            foreach (var moby in mobies)
            {
                int originalModelId = moby.modelID;
                byte[] bytes = moby.ToByteArray();
                int restoredModelId = DataFunctions.ReadInt(bytes, 0x18);
                Assert.Equal(originalModelId, restoredModelId);
            }
        }

        [SkippableFact]
        public void Moby_SerializeRoundTrip_ColorPreserved()
        {
            Skip.If(GameplayFile == null || EngineFile == null, SkipMsg);
            var mobies = LoadMobies();
            foreach (var moby in mobies)
            {
                byte[] bytes = moby.ToByteArray();
                Assert.Equal((byte) moby.color.R, (byte) DataFunctions.ReadInt(bytes, 0x64));
                Assert.Equal((byte) moby.color.G, (byte) DataFunctions.ReadInt(bytes, 0x68));
                Assert.Equal((byte) moby.color.B, (byte) DataFunctions.ReadInt(bytes, 0x6C));
            }
        }

        [SkippableFact]
        public void Moby_SerializeRoundTrip_OcclusionPreserved()
        {
            Skip.If(GameplayFile == null || EngineFile == null, SkipMsg);
            var mobies = LoadMobies();
            foreach (var moby in mobies)
            {
                byte[] bytes = moby.ToByteArray();
                bool restoredOcclusion = DataFunctions.ReadInt(bytes, 0x5C) > 0;
                Assert.Equal(moby.occlusion, restoredOcclusion);
            }
        }

        [SkippableFact]
        public void Moby_SerializeRoundTrip_ScalePreserved()
        {
            Skip.If(GameplayFile == null || EngineFile == null, SkipMsg);
            var mobies = LoadMobies();
            foreach (var moby in mobies)
            {
                byte[] bytes = moby.ToByteArray();
                float scale = DataFunctions.ReadFloat(bytes, 0x1C);
                Assert.Equal(moby.scale.X, scale, 5);
            }
        }
    }
}
