// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.Models;
using LibReplanetizer.Parsers;
using Xunit;

namespace LibReplanetizer.Tests.Integration
{
    public class CollisionFixtureTests
    {
        private static readonly string? EngineFile =
            FixtureConfig.GetFixtureFile("engine.ps3") ?? FixtureConfig.GetFixtureFile("engine");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing engine.ps3.";

        [SkippableFact]
        public void GetCollisionModel_ReturnsNonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            Assert.NotNull(parser.GetCollisionModel());
        }

        [SkippableFact]
        public void Collision_VertexBuffer_NonEmpty()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var collision = (Collision) parser.GetCollisionModel();
            Assert.NotEmpty(collision.vertexBuffer);
        }

        [SkippableFact]
        public void Collision_IndBuff_NonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var collision = (Collision) parser.GetCollisionModel();
            Assert.NotNull(collision.indBuff);
        }

        [SkippableFact]
        public void Collision_ColorBuff_NonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var collision = (Collision) parser.GetCollisionModel();
            Assert.NotNull(collision.colorBuff);
        }

        [SkippableFact]
        public void Collision_VertexCount_ConsistentWithBuffer()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            var collision = (Collision) parser.GetCollisionModel();
            Assert.Equal(collision.vertexBuffer.Length / collision.vertexStride, collision.vertexCount);
        }
    }
}
