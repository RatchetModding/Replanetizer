// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using Xunit;

namespace LibReplanetizer.Tests.Models
{
    public class TextureConfigTests
    {
        // Bit 0 of mode = "S from low bits" flag; bit 1 = ClampEdge.
        // Bit 2 = "T from low bits" flag; bit 3 = ClampEdge for T.

        private static TextureConfig ConfigWith(int id, int mode)
            => new TextureConfig { id = id, start = 0, size = 4, mode = mode };

        [Fact]
        public void WrapModeS_Repeat_WhenBit0SetAndBit1Clear()
        {
            var cfg = ConfigWith(0, 0b01); // bit0=1, bit1=0 → Repeat
            Assert.Equal(TextureConfig.WrapMode.Repeat, cfg.wrapModeS);
        }

        [Fact]
        public void WrapModeS_ClampEdge_WhenBit0AndBit1Set()
        {
            var cfg = ConfigWith(0, 0b11); // bit0=1, bit1=1 → ClampEdge
            Assert.Equal(TextureConfig.WrapMode.ClampEdge, cfg.wrapModeS);
        }

        [Fact]
        public void WrapModeS_DefaultRepeat_WhenNoBitSet()
        {
            var cfg = ConfigWith(0, 0);
            Assert.Equal(TextureConfig.WrapMode.Repeat, cfg.wrapModeS);
        }

        [Fact]
        public void SetWrapModeS_ClampEdge_SetsBit1()
        {
            var cfg = ConfigWith(0, 0b01); // bit0 set → use low-bit path
            cfg.wrapModeS = TextureConfig.WrapMode.ClampEdge;
            Assert.Equal(TextureConfig.WrapMode.ClampEdge, cfg.wrapModeS);
        }

        [Fact]
        public void SetWrapModeS_Repeat_ClearsBit1()
        {
            var cfg = ConfigWith(0, 0b11); // currently ClampEdge
            cfg.wrapModeS = TextureConfig.WrapMode.Repeat;
            Assert.Equal(TextureConfig.WrapMode.Repeat, cfg.wrapModeS);
        }

        [Fact]
        public void WrapModeT_Repeat_WhenBit2SetAndBit3Clear()
        {
            var cfg = ConfigWith(0, 0b0100); // bit2=1, bit3=0 → Repeat
            Assert.Equal(TextureConfig.WrapMode.Repeat, cfg.wrapModeT);
        }

        [Fact]
        public void WrapModeT_ClampEdge_WhenBit2AndBit3Set()
        {
            var cfg = ConfigWith(0, 0b1100); // bit2=1, bit3=1 → ClampEdge
            Assert.Equal(TextureConfig.WrapMode.ClampEdge, cfg.wrapModeT);
        }

        [Fact]
        public void SetWrapModeT_DoesNotAffectWrapModeS()
        {
            // bit0 = S flag, bit1 = S ClampEdge, bit2 = T flag, bit3 = T ClampEdge
            var cfg = ConfigWith(0, 0b0111); // S=ClampEdge, T=Repeat
            cfg.wrapModeT = TextureConfig.WrapMode.ClampEdge;
            // S must still be ClampEdge
            Assert.Equal(TextureConfig.WrapMode.ClampEdge, cfg.wrapModeS);
            Assert.Equal(TextureConfig.WrapMode.ClampEdge, cfg.wrapModeT);
        }
    }
}
