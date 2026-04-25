// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.IO;

namespace LibReplanetizer.Tests.Integration
{
    /// <summary>
    /// Resolves the path to game fixture files for integration tests.
    ///
    /// Set the environment variable <c>REPLANETIZER_TEST_FIXTURES</c> to a
    /// directory that contains unpacked game level files (e.g. <c>gameplay_ntsc</c>,
    /// <c>engine</c>).  When the variable is not set, integration tests are
    /// automatically skipped.
    /// </summary>
    public static class FixtureConfig
    {
        private const string EnvVar = "REPLANETIZER_TEST_FIXTURES";

        /// <summary>
        /// Returns the base fixture directory, or <c>null</c> if the environment
        /// variable is not set or the directory does not exist.
        /// </summary>
        public static string? GetFixturesPath()
        {
            string? path = Environment.GetEnvironmentVariable(EnvVar);
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return null;
            return path;
        }

        /// <summary>
        /// Returns the full path to <paramref name="fileName"/> inside the fixture
        /// directory, or <c>null</c> if the directory is not configured or the file
        /// does not exist.
        /// </summary>
        public static string? GetFixtureFile(string fileName)
        {
            string? dir = GetFixturesPath();
            if (dir == null) return null;
            string full = Path.Combine(dir, fileName);
            return File.Exists(full) ? full : null;
        }
    }
}
