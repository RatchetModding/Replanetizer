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
    public class SplineFixtureTests
    {
        private static readonly string? GameplayFile = FixtureConfig.GetFixtureFile("gameplay_ntsc");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing gameplay_ntsc.";

        [SkippableFact]
        public void GetSplines_ReturnsNonEmpty()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            Assert.NotEmpty(parser.GetSplines());
        }

        [SkippableFact]
        public void Spline_VertexBufferIsNotNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var spline in parser.GetSplines())
                Assert.NotNull(spline.vertexBuffer);
        }

        [SkippableFact]
        public void Spline_VertexBufferLengthIsMultipleOf3()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var spline in parser.GetSplines())
                Assert.Equal(0, spline.vertexBuffer.Length % 3);
        }

        [SkippableFact]
        public void Spline_ToByteArray_VertexCountIsCorrect()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var spline in parser.GetSplines())
            {
                int expectedCount = spline.vertexBuffer.Length / 3;
                byte[] bytes = spline.ToByteArray();
                Assert.Equal(expectedCount, (int) DataFunctions.ReadUint(bytes, 0x00));
            }
        }

        [SkippableFact]
        public void Spline_ToByteArray_LengthMatchesVertexCount()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var spline in parser.GetSplines())
            {
                int count = spline.vertexBuffer.Length / 3;
                byte[] bytes = spline.ToByteArray();
                Assert.Equal(count * 0x10 + 0x10, bytes.Length);
            }
        }

        [SkippableFact]
        public void Spline_AllVertexCoordinatesAreFinite()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var spline in parser.GetSplines())
            {
                foreach (float v in spline.vertexBuffer)
                    Assert.True(float.IsFinite(v), $"Spline id={spline.id} has non-finite vertex coordinate {v}.");
            }
        }
    }
}
