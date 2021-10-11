// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer.LevelObjects;
using LibReplanetizer.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LibReplanetizer
{
    public class Mission
    {
        /*
         * I suspect that the gameplay_mission_data files contain the mobies (I may be wrong though)
         */

        public int missionID;
        public List<Moby> mobies;
        public List<Model> models;
        public List<Texture> textures;

        public Mission(int id)
        {
            missionID = id;
            mobies = new List<Moby>();
            models = new List<Model>();
            textures = new List<Texture>();
        }
    }
}
