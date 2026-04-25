// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.LevelObjects;
using LibReplanetizer.Parsers;
using Xunit;

namespace LibReplanetizer.Tests.Integration
{
    public class LightConfigFixtureTests
    {
        private static readonly string? EngineFile =
            FixtureConfig.GetFixtureFile("engine.ps3") ?? FixtureConfig.GetFixtureFile("engine");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing engine.ps3.";

        [SkippableFact]
        public void GetLightConfig_ReturnsNonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            Assert.NotNull(parser.GetLightConfig());
        }

        [SkippableFact]
        public void LightConfig_Serialize_HasPositiveLength()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var config = parser.GetLightConfig();
            Assert.True(config.Serialize().Length >= 0x30);
        }

        [SkippableFact]
        public void LightConfig_SerializeRoundTrip_FirstBlockPreserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var config = parser.GetLightConfig();
            byte[] bytes = config.Serialize();
            Assert.Equal(config.off00, DataFunctions.ReadFloat(bytes, 0x00), 5);
            Assert.Equal(config.off04, DataFunctions.ReadFloat(bytes, 0x04), 5);
            Assert.Equal(config.off08, DataFunctions.ReadFloat(bytes, 0x08), 5);
            Assert.Equal(config.off0C, DataFunctions.ReadFloat(bytes, 0x0C), 5);
        }

        [SkippableFact]
        public void LightConfig_SerializeRoundTrip_SecondBlockPreserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var config = parser.GetLightConfig();
            byte[] bytes = config.Serialize();
            Assert.Equal(config.off10, DataFunctions.ReadFloat(bytes, 0x10), 5);
            Assert.Equal(config.off14, DataFunctions.ReadFloat(bytes, 0x14), 5);
            Assert.Equal(config.off18, DataFunctions.ReadFloat(bytes, 0x18), 5);
            Assert.Equal(config.off1C, DataFunctions.ReadFloat(bytes, 0x1C), 5);
        }

        [SkippableFact]
        public void LightConfig_SerializeRoundTrip_ThirdBlockPreserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var config = parser.GetLightConfig();
            byte[] bytes = config.Serialize();
            Assert.Equal(config.off20, DataFunctions.ReadFloat(bytes, 0x20), 5);
            Assert.Equal(config.off24, DataFunctions.ReadFloat(bytes, 0x24), 5);
            Assert.Equal(config.off28, DataFunctions.ReadFloat(bytes, 0x28), 5);
            Assert.Equal(config.off2C, DataFunctions.ReadFloat(bytes, 0x2C), 5);
        }

        [SkippableFact]
        public void LightConfig_AllFieldsAreFinite()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var config = parser.GetLightConfig();
            Assert.True(float.IsFinite(config.off00));
            Assert.True(float.IsFinite(config.off04));
            Assert.True(float.IsFinite(config.off08));
            Assert.True(float.IsFinite(config.off0C));
            Assert.True(float.IsFinite(config.off10));
            Assert.True(float.IsFinite(config.off14));
            Assert.True(float.IsFinite(config.off18));
            Assert.True(float.IsFinite(config.off1C));
            Assert.True(float.IsFinite(config.off20));
            Assert.True(float.IsFinite(config.off24));
            Assert.True(float.IsFinite(config.off28));
            Assert.True(float.IsFinite(config.off2C));
        }
    }
}
