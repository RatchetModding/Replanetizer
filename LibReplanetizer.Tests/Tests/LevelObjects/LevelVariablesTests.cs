// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.IO;
using Xunit;
using LibReplanetizer.LevelObjects;

namespace LibReplanetizer.Tests.LevelObjects
{
    /// <summary>
    /// Unit tests for the LevelVariables computed properties that have no file dependency.
    /// A LevelVariables constructed with pointer=0 skips file reading so all
    /// properties stay at their C# defaults, giving us a clean object to test.
    /// </summary>
    public class LevelVariablesPropertyTests
    {
        /// <summary>Returns a default-initialised LevelVariables (no file I/O).</summary>
        private static LevelVariables MakeDefault()
        {
            // Passing levelVarPointer=0 causes the constructor to return early.
            using var ms = new MemoryStream();
            using var fs = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 1, FileOptions.DeleteOnClose);
            return new LevelVariables(GameType.RaC1, fs, 0, 0);
        }

        [Fact]
        public void IsSphericalWorld_DefaultIsFalse()
        {
            var lv = MakeDefault();
            Assert.False(lv.isSphericalWorld);
        }

        [Fact]
        public void SetIsSphericalWorld_True_GetterReturnsTrue()
        {
            var lv = MakeDefault();
            lv.isSphericalWorld = true;
            Assert.True(lv.isSphericalWorld);
        }

        [Fact]
        public void SetIsSphericalWorld_FalseAfterTrue_GetterReturnsFalse()
        {
            var lv = MakeDefault();
            lv.isSphericalWorld = true;
            lv.isSphericalWorld = false;
            Assert.False(lv.isSphericalWorld);
        }

        [Fact]
        public void SetIsSphericalWorld_DoesNotAffectOtherProperties()
        {
            var lv = MakeDefault();
            lv.deathPlaneZ = -50f;
            lv.isSphericalWorld = true;
            Assert.Equal(-50f, lv.deathPlaneZ);
        }

        [Fact]
        public void FogColor_CanBeAssigned()
        {
            var lv = MakeDefault();
            var color = new SixLabors.ImageSharp.PixelFormats.Rgb24(100, 150, 200);
            lv.fogColor = color;
            Assert.Equal(color, lv.fogColor);
        }

        [Fact]
        public void ShipPosition_DefaultIsZero()
        {
            var lv = MakeDefault();
            Assert.Equal(OpenTK.Mathematics.Vector3.Zero, lv.shipPosition);
        }
    }
}
