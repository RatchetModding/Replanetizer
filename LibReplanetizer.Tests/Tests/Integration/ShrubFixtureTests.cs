// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.Collections.Generic;
using System.Linq;
using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using LibReplanetizer.Parsers;
using Xunit;

namespace LibReplanetizer.Tests.Integration
{
    /// <summary>
    /// Integration tests that load Shrub data from real game fixture files.
    /// Tests are automatically skipped when <c>REPLANETIZER_TEST_FIXTURES</c>
    /// is not set or the files are absent.
    /// </summary>
    public class ShrubFixtureTests
    {
        private static readonly string? EngineFile =
            FixtureConfig.GetFixtureFile("engine.ps3") ?? FixtureConfig.GetFixtureFile("engine");

        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing engine.ps3.";

        private static (List<Shrub> shrubs, List<Model> models) LoadShrubs()
        {
            using var parser = new EngineParser(EngineFile!);
            var models = parser.GetShrubModels();
            var shrubs = parser.GetShrubs(models);
            return (shrubs, models);
        }

        [SkippableFact]
        public void GetShrubs_ReturnsNonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (shrubs, _) = LoadShrubs();
            Assert.NotNull(shrubs);
        }

        [SkippableFact]
        public void GetShrubs_AllHaveNonNegativeDrawDistance()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (shrubs, _) = LoadShrubs();
            foreach (var shrub in shrubs)
                Assert.True(shrub.drawDistance >= 0f, $"Shrub modelID={shrub.modelID} has negative drawDistance.");
        }

        [SkippableFact]
        public void Shrub_ToByteArray_HasCorrectLength()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (shrubs, _) = LoadShrubs();
            foreach (var shrub in shrubs)
                Assert.Equal(Shrub.ELEMENTSIZE, shrub.ToByteArray().Length);
        }

        [SkippableFact]
        public void Shrub_SerializeRoundTrip_ModelIdPreserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (shrubs, _) = LoadShrubs();
            foreach (var shrub in shrubs)
            {
                byte[] bytes = shrub.ToByteArray();
                Assert.Equal(shrub.modelID, DataFunctions.ReadInt(bytes, 0x50));
            }
        }

        [SkippableFact]
        public void Shrub_SerializeRoundTrip_DrawDistancePreserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (shrubs, _) = LoadShrubs();
            foreach (var shrub in shrubs)
            {
                byte[] bytes = shrub.ToByteArray();
                Assert.Equal(shrub.drawDistance, DataFunctions.ReadFloat(bytes, 0x54), 5);
            }
        }

        [SkippableFact]
        public void Shrub_SerializeRoundTrip_ColorPreserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (shrubs, _) = LoadShrubs();
            foreach (var shrub in shrubs)
            {
                byte[] bytes = shrub.ToByteArray();
                Assert.Equal(shrub.color.R, bytes[0x60]);
                Assert.Equal(shrub.color.G, bytes[0x61]);
                Assert.Equal(shrub.color.B, bytes[0x62]);
            }
        }

        [SkippableFact]
        public void Shrub_SerializeRoundTrip_LightPreserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (shrubs, _) = LoadShrubs();
            foreach (var shrub in shrubs)
            {
                byte[] bytes = shrub.ToByteArray();
                Assert.Equal(shrub.light, DataFunctions.ReadUshort(bytes, 0x68));
            }
        }

        [SkippableFact]
        public void Shrub_ToByteArray_AlwaysWritesFfffAt6A()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (shrubs, _) = LoadShrubs();
            foreach (var shrub in shrubs)
            {
                byte[] bytes = shrub.ToByteArray();
                Assert.Equal(0xffff, DataFunctions.ReadUshort(bytes, 0x6A));
            }
        }

        [SkippableFact]
        public void Shrub_ModelReferencesMatchShrubModels()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (shrubs, models) = LoadShrubs();
            var modelIds = new HashSet<short>(models.Select(m => m.id));
            foreach (var shrub in shrubs)
            {
                if (shrub.model != null)
                    Assert.Contains(shrub.model.id, modelIds);
            }
        }
    }
}
