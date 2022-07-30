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
    }
}
