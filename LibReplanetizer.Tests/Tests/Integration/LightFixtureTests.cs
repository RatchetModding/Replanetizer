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
    public class LightFixtureTests
    {
        private static readonly string? EngineFile =
            FixtureConfig.GetFixtureFile("engine.ps3") ?? FixtureConfig.GetFixtureFile("engine");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing engine.ps3.";

        [SkippableFact]
        public void GetLights_ReturnsNonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            Assert.NotNull(parser.GetLights());
        }

        [SkippableFact]
        public void GetLights_ReturnsNonEmpty()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            Assert.NotEmpty(parser.GetLights());
        }

        [SkippableFact]
        public void Light_Serialize_HasCorrectLength()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var light in parser.GetLights())
                Assert.Equal(0x40, light.Serialize().Length);
        }

        [SkippableFact]
        public void Light_SerializeRoundTrip_Color1Preserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var light in parser.GetLights())
            {
                byte[] bytes = light.Serialize();
                Assert.Equal(light.color1.X, DataFunctions.ReadFloat(bytes, 0x00), 5);
                Assert.Equal(light.color1.Y, DataFunctions.ReadFloat(bytes, 0x04), 5);
                Assert.Equal(light.color1.Z, DataFunctions.ReadFloat(bytes, 0x08), 5);
                Assert.Equal(light.color1.W, DataFunctions.ReadFloat(bytes, 0x0C), 5);
            }
        }

        [SkippableFact]
        public void Light_SerializeRoundTrip_Direction1Preserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var light in parser.GetLights())
            {
                byte[] bytes = light.Serialize();
                Assert.Equal(light.direction1.X, DataFunctions.ReadFloat(bytes, 0x10), 5);
                Assert.Equal(light.direction1.Y, DataFunctions.ReadFloat(bytes, 0x14), 5);
                Assert.Equal(light.direction1.Z, DataFunctions.ReadFloat(bytes, 0x18), 5);
                Assert.Equal(light.direction1.W, DataFunctions.ReadFloat(bytes, 0x1C), 5);
            }
        }

        [SkippableFact]
        public void Light_SerializeRoundTrip_Color2Preserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var light in parser.GetLights())
            {
                byte[] bytes = light.Serialize();
                Assert.Equal(light.color2.X, DataFunctions.ReadFloat(bytes, 0x20), 5);
                Assert.Equal(light.color2.Y, DataFunctions.ReadFloat(bytes, 0x24), 5);
                Assert.Equal(light.color2.Z, DataFunctions.ReadFloat(bytes, 0x28), 5);
                Assert.Equal(light.color2.W, DataFunctions.ReadFloat(bytes, 0x2C), 5);
            }
        }

        [SkippableFact]
        public void Light_SerializeRoundTrip_Direction2Preserved()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var light in parser.GetLights())
            {
                byte[] bytes = light.Serialize();
                Assert.Equal(light.direction2.X, DataFunctions.ReadFloat(bytes, 0x30), 5);
                Assert.Equal(light.direction2.Y, DataFunctions.ReadFloat(bytes, 0x34), 5);
                Assert.Equal(light.direction2.Z, DataFunctions.ReadFloat(bytes, 0x38), 5);
                Assert.Equal(light.direction2.W, DataFunctions.ReadFloat(bytes, 0x3C), 5);
            }
        }

        [SkippableFact]
        public void Light_AllChannelsAreFinite()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var light in parser.GetLights())
            {
                Assert.True(float.IsFinite(light.color1.X) && float.IsFinite(light.color1.Y)
                    && float.IsFinite(light.color1.Z) && float.IsFinite(light.color1.W));
                Assert.True(float.IsFinite(light.color2.X) && float.IsFinite(light.color2.Y)
                    && float.IsFinite(light.color2.Z) && float.IsFinite(light.color2.W));
                Assert.True(float.IsFinite(light.direction1.X) && float.IsFinite(light.direction1.Y)
                    && float.IsFinite(light.direction1.Z) && float.IsFinite(light.direction1.W));
                Assert.True(float.IsFinite(light.direction2.X) && float.IsFinite(light.direction2.Y)
                    && float.IsFinite(light.direction2.Z) && float.IsFinite(light.direction2.W));
            }
        }
    }
}
