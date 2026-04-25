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
    public class PointLightFixtureTests
    {
        private static readonly string? GameplayFile = FixtureConfig.GetFixtureFile("gameplay_ntsc");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing gameplay_ntsc.";

        [SkippableFact]
        public void GetPointLights_ReturnsNonNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            Assert.NotNull(parser.GetPointLights());
        }

        [SkippableFact]
        public void PointLight_ToByteArray_HasCorrectLength()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            int expectedSize = PointLight.GetElementSize(GameType.RaC1);
            foreach (var light in parser.GetPointLights())
                Assert.Equal(expectedSize, light.ToByteArray().Length);
        }

        [SkippableFact]
        public void PointLight_RadiusIsNonNegative()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var light in parser.GetPointLights())
                Assert.True(light.radius >= 0f, $"PointLight id={light.id} has negative radius.");
        }

        [SkippableFact]
        public void PointLight_SerializeRoundTrip_RadiusPreserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var light in parser.GetPointLights())
            {
                byte[] bytes = light.ToByteArray();
                Assert.Equal(light.radius, DataFunctions.ReadFloat(bytes, 0x0C), 5);
            }
        }

        [SkippableFact]
        public void PointLight_SerializeRoundTrip_PositionPreserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var light in parser.GetPointLights())
            {
                byte[] bytes = light.ToByteArray();
                Assert.Equal(light.position.X, DataFunctions.ReadFloat(bytes, 0x00), 5);
                Assert.Equal(light.position.Y, DataFunctions.ReadFloat(bytes, 0x04), 5);
                Assert.Equal(light.position.Z, DataFunctions.ReadFloat(bytes, 0x08), 5);
            }
        }
    }
}
