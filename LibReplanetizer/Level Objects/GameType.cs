// Copyright (C) 2018-2023, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

namespace LibReplanetizer
{
    public class GameType
    {
        private static readonly int[] MOBY_SIZES = { 0x78, 0x88, 0x88, 0x70 };

        public readonly static GameType RaC1 = new GameType(1);
        public readonly static GameType RaC2 = new GameType(2);
        public readonly static GameType RaC3 = new GameType(3);
        public readonly static GameType DL = new GameType(4);

        public int num { get; private set; }
        public int mobyElemSize { get; private set; }

        private GameType(int gameNum)
        {
            num = gameNum;

            mobyElemSize = MOBY_SIZES[gameNum - 1];
        }
    }
}
