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
    /// Integration tests that parse an <c>engine</c> game file.
    ///
    /// These tests are skipped automatically when the environment variable
    /// <c>REPLANETIZER_TEST_FIXTURES</c> is not set or does not point to a
    /// directory containing the required game files.
    ///
    /// Example:
    ///   set REPLANETIZER_TEST_FIXTURES=C:\RaC1\level01
    ///   dotnet test
    /// </summary>
    public class EngineRoundTripTests
    {
        private const string EngineFileName = "engine.ps3";

        private static string? GetEngineFile()
            => FixtureConfig.GetFixtureFile(EngineFileName);

        [SkippableFact]
        public void EngineParser_ParsesWithoutException()
        {
            string? path = GetEngineFile();
            Skip.If(path == null, $"Set env var REPLANETIZER_TEST_FIXTURES to a directory containing '{EngineFileName}'.");

            using var parser = new EngineParser(path!);
            // If we get here, the header was read without throwing.
        }

        [SkippableFact]
        public void EngineParser_GetGameType_ReturnsValidGameType()
        {
            string? path = GetEngineFile();
            Skip.If(path == null, "Fixture not configured.");

            using var parser = new EngineParser(path!);
            GameType gameType = parser.GetGameType();

            Assert.NotNull(gameType);
            Assert.True(gameType.num >= 1 && gameType.num <= 4,
                $"Expected game number in [1,4], got {gameType.num}.");
        }

        [SkippableFact]
        public void EngineParser_GetTextures_ReturnsNonNull()
        {
            string? path = GetEngineFile();
            Skip.If(path == null, "Fixture not configured.");

            using var parser = new EngineParser(path!);
            var textures = parser.GetTextures();

            Assert.NotNull(textures);
        }

        [SkippableFact]
        public void EngineParser_GetLights_ReturnsNonNull()
        {
            string? path = GetEngineFile();
            Skip.If(path == null, "Fixture not configured.");

            using var parser = new EngineParser(path!);
            var lights = parser.GetLights();

            Assert.NotNull(lights);
        }

        [SkippableFact]
        public void EngineParser_GetTieModels_ReturnsNonNull()
        {
            string? path = GetEngineFile();
            Skip.If(path == null, "Fixture not configured.");

            using var parser = new EngineParser(path!);
            var tieModels = parser.GetTieModels();

            Assert.NotNull(tieModels);
        }

        [SkippableFact]
        public void EngineParser_GetShrubModels_ReturnsNonNull()
        {
            string? path = GetEngineFile();
            Skip.If(path == null, "Fixture not configured.");

            using var parser = new EngineParser(path!);
            var shrubModels = parser.GetShrubModels();

            Assert.NotNull(shrubModels);
        }
        [SkippableFact]
        public void EngineSerializer_RoundTrip_BytesMatchOriginal()
        {
            string? path = GetEngineFile();
            Skip.If(path == null, "Fixture not configured.");

            byte[] original = File.ReadAllBytes(path!);

            // Level constructor parses both engine and gameplay from the same directory.
            var level = new Level(path!);

            string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            try
            {
                new EngineSerializer().Save(level, tempDir);

                string outputPath = Path.Combine(tempDir, EngineFileName);
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
