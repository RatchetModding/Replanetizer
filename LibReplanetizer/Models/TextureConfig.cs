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
        [Category("Attributes"), DisplayName("Texture ID")]
        public int id { get; set; }
        [Category("Attributes"), DisplayName("Vertex Start Index")]
        public int start { get; set; }
        [Category("Attributes"), DisplayName("Number of Vertices")]
        public int size { get; set; }
        [Category("Attributes"), DisplayName("Mode")]
        public int mode { get; set; }

        /*
         * The textureConfig modes in the following are probably bitmask but more testing is required
         * to confirm that, until then simply add all modes that are observed.
         * Ideally write the binary + the game in which this was observed as it could be that the
         * bitmasks differ between the games.
         */

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
    }
}
