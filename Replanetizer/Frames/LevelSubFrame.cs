﻿// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

namespace Replanetizer.Frames
{
    public abstract class LevelSubFrame : Frame
    {
        protected LevelFrame levelFrame;

        public LevelSubFrame(Window wnd, LevelFrame levelFrame) : base(wnd)
        {
            this.levelFrame = levelFrame;
        }
    }
}
