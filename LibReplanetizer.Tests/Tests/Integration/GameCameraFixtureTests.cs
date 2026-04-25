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
    public class GameCameraFixtureTests
    {
        private static readonly string? GameplayFile = FixtureConfig.GetFixtureFile("gameplay_ntsc");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing gameplay_ntsc.";

        [SkippableFact]
        public void GetGameCameras_ReturnsNonNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            Assert.NotNull(parser.GetGameCameras());
        }

        [SkippableFact]
        public void GameCamera_ToByteArray_HasCorrectLength()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var cam in parser.GetGameCameras())
                Assert.Equal(GameCamera.ELEMENTSIZE, cam.ToByteArray().Length);
        }

        [SkippableFact]
        public void GameCamera_SerializeRoundTrip_IdPreserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var cam in parser.GetGameCameras())
            {
                byte[] bytes = cam.ToByteArray();
                Assert.Equal(cam.id, DataFunctions.ReadInt(bytes, 0x00));
            }
        }

        [SkippableFact]
        public void GameCamera_SerializeRoundTrip_PvarIndexPreserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var cam in parser.GetGameCameras())
            {
                byte[] bytes = cam.ToByteArray();
                Assert.Equal(cam.pvarIndex, DataFunctions.ReadInt(bytes, 0x1C));
            }
        }

        [SkippableFact]
        public void GameCamera_SerializeRoundTrip_PositionPreserved()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var cam in parser.GetGameCameras())
            {
                byte[] bytes = cam.ToByteArray();
                Assert.Equal(cam.position.X, DataFunctions.ReadFloat(bytes, 0x04), 5);
                Assert.Equal(cam.position.Y, DataFunctions.ReadFloat(bytes, 0x08), 5);
                Assert.Equal(cam.position.Z, DataFunctions.ReadFloat(bytes, 0x0C), 5);
            }
        }
    }
}
