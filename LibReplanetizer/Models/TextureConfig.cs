// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System.ComponentModel;

namespace LibReplanetizer
{

    public class TextureConfig
    {
        public static readonly string[] WRAP_MODE_STRINGS = { "Repeat", "ClampEdge" };

        /*
         * Only those two modes are used by the game.
         */
        public enum WrapMode
        {
            Repeat = 0,
            ClampEdge = 1
        }

        [Category("Attributes"), DisplayName("Texture ID")]
        public int id { get; set; }
        [Category("Attributes"), DisplayName("Vertex Start Index")]
        public int start { get; set; }
        [Category("Attributes"), DisplayName("Number of Vertices")]
        public int size { get; set; }
        [Category("Attributes"), DisplayName("Mode")]
        public int mode { get; set; }
        [Category("Attributes"), DisplayName("Texture Wrap S")]
        public WrapMode wrapModeS
        {
            get
            {
                if ((mode & 0b01) > 0)
                {
                    return (((mode & 0b10) > 0)) ? WrapMode.ClampEdge : WrapMode.Repeat;
                }

                if (((mode >> 24) & 0b01) > 0)
                {
                    return ((((mode >> 24) & 0b10) > 0)) ? WrapMode.ClampEdge : WrapMode.Repeat;
                }

                return WrapMode.Repeat;
            }
            set
            {
                if ((mode & 0b01) > 0)
                {
                    switch (value)
                    {
                        case WrapMode.Repeat:
                            mode = mode & (~0b10);
                            break;
                        case WrapMode.ClampEdge:
                            mode = mode | 0b10;
                            break;
                        default:
                            break;
                    }
                    return;
                }

                if (((mode >> 24) & 0b01) > 0)
                {
                    switch (value)
                    {
                        case WrapMode.Repeat:
                            mode = mode & ~(0b10 << 24);
                            break;
                        case WrapMode.ClampEdge:
                            mode = mode | (0b10 << 24);
                            break;
                        default:
                            break;
                    }
                    return;
                }
            }
        }

        [Category("Attributes"), DisplayName("Texture Wrap T")]
        public WrapMode wrapModeT
        {
            get
            {
                if (((mode >> 2) & 0b01) > 0)
                {
                    return ((((mode >> 2) & 0b10) > 0)) ? WrapMode.ClampEdge : WrapMode.Repeat;
                }

                if (((mode >> 26) & 0b01) > 0)
                {
                    return ((((mode >> 26) & 0b10) > 0)) ? WrapMode.ClampEdge : WrapMode.Repeat;
                }

                return WrapMode.Repeat;
            }
            set
            {
                if (((mode >> 2) & 0b01) > 0)
                {
                    switch (value)
                    {
                        case WrapMode.Repeat:
                            mode = mode & ~(0b10 << 2);
                            break;
                        case WrapMode.ClampEdge:
                            mode = mode | (0b10 << 2);
                            break;
                        default:
                            break;
                    }
                    return;
                }

                if (((mode >> 26) & 0b01) > 0)
                {
                    switch (value)
                    {
                        case WrapMode.Repeat:
                            mode = mode & ~(0b10 << 26);
                            break;
                        case WrapMode.ClampEdge:
                            mode = mode | (0b10 << 26);
                            break;
                        default:
                            break;
                    }
                    return;
                }
            }
        }

        public bool IgnoresTransparency()
        {
            switch (mode)
            {
                case 136311: /* 100001010001110111 (RaC 3) */
                case 136279: /* 100001010001010111 (RaC 3) */
                case 136277: /* 100001010001010101 (RaC 3) */
                case 136413: /* 100001010011011101 (RaC 3) */
                case 136447: /* 100001010011111111 (RaC 3) */
                case 136533: /* 100001010101010101 (RaC 3) */
                    return true;
                default:
                    return false;
            }
        }

        // In the following are some observations of different modes and their corresponding wrap mode as reported by Renderdoc.
        // The ? just says that this has not yet been confirmed through Renderdoc.
        // Mobies seem to be broken as they all use the repeat mode even when they clearly shouldn't based on the art.

        // A RaC 1 database of TextureWrapMode mappings
        // Terrain:
        // 0000000000000000000000000101 (000000005): ?
        // 0000000000000000000000000111 (000000007): ?
        // 0000000000000000000000001101 (000000013): ?
        // 0000000000000000000000001111 (000000015): ?
        // Tie:
        // 0000000000000000000000000101 (000000005): ?
        // 0000000000000000000000000111 (000000007): ?
        // 0000000000000000000000001101 (000000013): ?
        // 0000000000000000000000001111 (000000015): ?
        // Shrub:
        // 0101000000000000000000000000 (083886080): ?
        // 0111000000000000000000000000 (117440512): ?
        // Moby:
        // 0101000000000000000000000000 (083886080): ?

        // A RaC 2 database of TextureWrapMode mappings
        // Terrain:
        // 0111000000000000000000000000 (117440512): S = ClampEdge, T = Repeat
        // 1111000000000000000000000000 (251658240): S = ClampEdge, T = ClampEdge
        // 1101000000000000000000000000 (218103808): S = Repeat,    T = ClampEdge
        // Tie:
        // 0000000000000000000001010101 (000000085): S = Repeat,    T = Repeat
        // 0000000000000000000001011101 (000000093): S = Repeat,    T = ClampEdge
        // 0000000000000000000001011111 (000000095): S = ClampEdge, T = ClampEdge
        // Shrub:
        // 1111000000000000000000000000 (251658240): S = ClampEdge, T = ClampEdge
        // Moby:
        // 0101000000000000000000000000 (083886080): S = Repeat,    T = Repeat

        //
        // It becomes apparent that the texture config mode works the following way:
        // Each feature consists of two bits.
        // The lower bit signifies whether a feature is present / set.
        // The higher bit is the value of the feature.
        // It remains to be identified what all these features are and why they are shifted by 24 bits sometimes.
        //
    }
}
