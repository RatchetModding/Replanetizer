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
    public class LanguageDataFixtureTests
    {
        private static readonly string? GameplayFile = FixtureConfig.GetFixtureFile("gameplay_ntsc");
        private const string SkipMsg = "Set env var REPLANETIZER_TEST_FIXTURES to a directory containing gameplay_ntsc.";

        [SkippableFact]
        public void GetEnglish_ReturnsNonNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            Assert.NotNull(parser.GetEnglish());
        }

        [SkippableFact]
        public void GetEnglish_ReturnsNonEmpty()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            Assert.NotEmpty(parser.GetEnglish());
        }

        [SkippableFact]
        public void LanguageData_TextIsNotNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            foreach (var entry in parser.GetEnglish())
                Assert.NotNull(entry.text);
        }

        [SkippableFact]
        public void LanguageData_AllLanguagesReturnNonNull()
        {
            Skip.If(GameplayFile == null, SkipMsg);
            using var parser = new GameplayParser(GameType.RaC1, GameplayFile!);
            Assert.NotNull(parser.GetUkEnglish());
            Assert.NotNull(parser.GetFrench());
            Assert.NotNull(parser.GetGerman());
            Assert.NotNull(parser.GetSpanish());
            Assert.NotNull(parser.GetItalian());
            Assert.NotNull(parser.GetJapanese());
            Assert.NotNull(parser.GetKorean());
        }
    }
}
