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
