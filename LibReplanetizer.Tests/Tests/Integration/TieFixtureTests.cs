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
    /// Integration tests that load Tie data from real game fixture files.
    /// Tests are automatically skipped when <c>REPLANETIZER_TEST_FIXTURES</c>
    /// is not set or the files are absent.
    /// </summary>
    public class TieFixtureTests
    {
        private static readonly string? EngineFile =
            FixtureConfig.GetFixtureFile("engine.ps3") ?? FixtureConfig.GetFixtureFile("engine");

        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing engine.ps3.";

        private static (List<Tie> ties, List<Model> models) LoadTies()
        {
            using var parser = new EngineParser(EngineFile!);
            var models = parser.GetTieModels();
            var ties = parser.GetTies(models);
            return (ties, models);
        }

        [SkippableFact]
        public void GetTies_ReturnsNonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (ties, _) = LoadTies();
            Assert.NotNull(ties);
        }

        [SkippableFact]
        public void Tie_ToByteArray_HasCorrectLength()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (ties, _) = LoadTies();
            const int TIE_ELEMENTSIZE = 0x70;
            foreach (var tie in ties)
                Assert.Equal(TIE_ELEMENTSIZE, tie.ToByteArray().Length);
        }

        [SkippableFact]
        public void Tie_SerializeRoundTrip_ModelIdPreserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (ties, _) = LoadTies();
            foreach (var tie in ties)
            {
                byte[] bytes = tie.ToByteArray();
                Assert.Equal(tie.modelID, DataFunctions.ReadInt(bytes, 0x50));
            }
        }

        [SkippableFact]
        public void Tie_SerializeRoundTrip_LightPreserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (ties, _) = LoadTies();
            foreach (var tie in ties)
            {
                byte[] bytes = tie.ToByteArray();
                Assert.Equal(tie.light, DataFunctions.ReadUshort(bytes, 0x68));
            }
        }

        [SkippableFact]
        public void Tie_SerializeRoundTrip_Off54Preserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (ties, _) = LoadTies();
            foreach (var tie in ties)
            {
                byte[] bytes = tie.ToByteArray();
                Assert.Equal(tie.off54, DataFunctions.ReadUint(bytes, 0x54));
            }
        }

        [SkippableFact]
        public void Tie_ToByteArray_AlwaysWritesFfffAt6A()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (ties, _) = LoadTies();
            foreach (var tie in ties)
            {
                byte[] bytes = tie.ToByteArray();
                Assert.Equal(0xffff, DataFunctions.ReadUshort(bytes, 0x6A));
            }
        }

        [SkippableFact]
        public void Tie_ModelReferencesMatchTieModels()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (ties, models) = LoadTies();
            var modelIds = new HashSet<short>(models.Select(m => m.id));
            foreach (var tie in ties)
            {
                if (tie.model != null)
                    Assert.Contains(tie.model.id, modelIds);
            }
        }

        [SkippableFact]
        public void Tie_SerializeRoundTrip_MatrixTranslationPreserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            var (ties, _) = LoadTies();
            foreach (var tie in ties)
            {
                byte[] bytes = tie.ToByteArray();
                var recovered = DataFunctions.ReadMatrix4(bytes, 0x00);
                Assert.Equal(tie.position.X, recovered.Row3.X, 3);
                Assert.Equal(tie.position.Y, recovered.Row3.Y, 3);
                Assert.Equal(tie.position.Z, recovered.Row3.Z, 3);
            }
        }
    }
}
