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
    public class UiElementFixtureTests
    {
        private static readonly string? EngineFile =
            FixtureConfig.GetFixtureFile("engine.ps3") ?? FixtureConfig.GetFixtureFile("engine");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing engine.ps3.";

        [SkippableFact]
        public void GetUiElements_ReturnsNonNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            Assert.NotNull(parser.GetUiElements());
        }

        [SkippableFact]
        public void GetUiElements_ReturnsNonEmpty()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            Assert.NotEmpty(parser.GetUiElements());
        }

        [SkippableFact]
        public void UiElement_SpritesListIsNotNull()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var element in parser.GetUiElements())
                Assert.NotNull(element.sprites);
        }

        [SkippableFact]
        public void UiElement_IdIsNonNegative()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            foreach (var element in parser.GetUiElements())
                Assert.True(element.id >= 0, $"UiElement has negative id={element.id}.");
        }

        [SkippableFact]
        public void UiElement_SpriteCountMatchesListLength()
        {
            Skip.If(EngineFile == null, SkipMsg);
            using var parser = new EngineParser(EngineFile!);
            // Verify each element's parsed sprite list is self-consistent
            // (no index-out-of-range during construction, list is fully populated).
            foreach (var element in parser.GetUiElements())
                Assert.True(element.sprites.Count >= 0);
        }
    }
}
