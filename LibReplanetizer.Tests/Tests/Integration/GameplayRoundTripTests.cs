// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.IO;
using LibReplanetizer.Parsers;
using LibReplanetizer.Serializers;
using Xunit;

namespace LibReplanetizer.Tests.Integration
{
    /// <summary>
    /// Integration tests that parse a <c>gameplay_ntsc</c> game file.
    ///
    /// These tests are skipped automatically when the environment variable
    /// <c>REPLANETIZER_TEST_FIXTURES</c> is not set or does not point to a
    /// directory containing the required game files.
    ///
    /// Example:
    ///   set REPLANETIZER_TEST_FIXTURES=C:\RaC1\level01
    ///   dotnet test
    /// </summary>
    public class GameplayRoundTripTests
    {
        private const string GameplayFileName = "gameplay_ntsc";

        private static string? GetGameplayFile()
            => FixtureConfig.GetFixtureFile(GameplayFileName);

        [SkippableFact]
        public void GameplayParser_RC1_ParsesWithoutException()
        {
            string? path = GetGameplayFile();
            Skip.If(path == null, $"Set env var REPLANETIZER_TEST_FIXTURES to a directory containing '{GameplayFileName}'.");

            using var parser = new GameplayParser(GameType.RaC1, path!);
            // If we get here, the header was read without throwing.
        }

        [SkippableFact]
        public void GameplayParser_RC1_GetSplines_ReturnsNonNull()
        {
            string? path = GetGameplayFile();
            Skip.If(path == null, "Fixture not configured.");

            using var parser = new GameplayParser(GameType.RaC1, path!);
            var splines = parser.GetSplines();

            Assert.NotNull(splines);
        }

        [SkippableFact]
        public void GameplayParser_RC1_GetLevelVariables_ReturnsNonNull()
        {
            string? path = GetGameplayFile();
            Skip.If(path == null, "Fixture not configured.");

            using var parser = new GameplayParser(GameType.RaC1, path!);
            var vars = parser.GetLevelVariables();

            Assert.NotNull(vars);
        }

        [SkippableFact]
        public void GameplayParser_RC1_GetDirectionalLights_ReturnsNonNull()
        {
            string? path = GetGameplayFile();
            Skip.If(path == null, "Fixture not configured.");

            using var parser = new GameplayParser(GameType.RaC1, path!);
            var lights = parser.GetDirectionalLights();

            Assert.NotNull(lights);
        }

        [SkippableFact]
        public void GameplayParser_RC1_GetSoundInstances_ReturnsNonNull()
        {
            string? path = GetGameplayFile();
            Skip.If(path == null, "Fixture not configured.");

            using var parser = new GameplayParser(GameType.RaC1, path!);
            var sounds = parser.GetSoundInstances();

            Assert.NotNull(sounds);
        }
        [SkippableFact]
        public void GameplaySerializer_RoundTrip_BytesMatchOriginal()
        {
            string? gameplayPath = GetGameplayFile();
            Skip.If(gameplayPath == null, "Fixture not configured.");

            // Level requires the engine file path; derive it from the fixture directory.
            string fixtureDir = FixtureConfig.GetFixturesPath()!;
            string engineFile = FixtureConfig.GetFixtureFile("engine.ps3")
                             ?? FixtureConfig.GetFixtureFile("engine")!;
            Skip.If(engineFile == null, "Engine fixture file not found; cannot construct Level.");

            byte[] original = File.ReadAllBytes(gameplayPath!);

            var level = new Level(engineFile!);

            string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            try
            {
                new GameplaySerializer().Save(level, tempDir);

                string outputPath = Path.Combine(tempDir, GameplayFileName);
                byte[] serialized = File.ReadAllBytes(outputPath);

                Assert.Equal(original.Length, serialized.Length);
                Assert.Equal(original, serialized);
            }
            finally
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
