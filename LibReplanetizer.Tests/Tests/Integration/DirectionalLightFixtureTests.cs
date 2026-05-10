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
    public class DirectionalLightFixtureTests
    {
        private static readonly string? GameplayFile = FixtureConfig.GetFixtureFile("gameplay_ntsc");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing gameplay_ntsc.";

        [SkippableFact]
        public void GetDirectionalLights_ReturnsNonNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            Assert.NotNull(parser.GetDirectionalLights());
        }

        [SkippableFact]
        public void DirectionalLight_ToByteArray_HasCorrectLength()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var light in parser.GetDirectionalLights())
                Assert.Equal(DirectionalLight.ELEMENTSIZE, light.ToByteArray().Length);
        }

        [SkippableFact]
        public void DirectionalLight_SerializeRoundTrip_ColorAPreserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var light in parser.GetDirectionalLights())
            {
                byte[] bytes = light.ToByteArray();
                Assert.Equal(light.colorA.X, DataFunctions.ReadFloat(bytes, 0x00), 5);
                Assert.Equal(light.colorA.Y, DataFunctions.ReadFloat(bytes, 0x04), 5);
                Assert.Equal(light.colorA.Z, DataFunctions.ReadFloat(bytes, 0x08), 5);
                Assert.Equal(light.colorA.W, DataFunctions.ReadFloat(bytes, 0x0C), 5);
            }
        }

        [SkippableFact]
        public void DirectionalLight_SerializeRoundTrip_DirectionBPreserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var light in parser.GetDirectionalLights())
            {
                byte[] bytes = light.ToByteArray();
                Assert.Equal(light.directionB.X, DataFunctions.ReadFloat(bytes, 0x30), 5);
                Assert.Equal(light.directionB.Y, DataFunctions.ReadFloat(bytes, 0x34), 5);
                Assert.Equal(light.directionB.Z, DataFunctions.ReadFloat(bytes, 0x38), 5);
                Assert.Equal(light.directionB.W, DataFunctions.ReadFloat(bytes, 0x3C), 5);
            }
        }
    }
}
