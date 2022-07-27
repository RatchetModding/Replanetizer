// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using LibReplanetizer;
using LibReplanetizer.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Replanetizer.ModelLists
{
    public static class ModelLists
    {
        private static Dictionary<int, string>? __RC1_MOB_NAMES = null;
        private static Dictionary<int, string>? __RC1_TIE_NAMES = null;
        private static Dictionary<int, string>? __RC2_MOB_NAMES = null;


        public static Dictionary<int, string> RC1_MOB_NAMES
        {
            get
            {
                if (__RC1_MOB_NAMES == null)
                {
                    __RC1_MOB_NAMES = GetModelNames("MobyModelsRC1.txt");
                }
                return __RC1_MOB_NAMES;
            }
        }

        public static Dictionary<int, string> RC1_TIE_NAMES
        {
            get
            {
                if (__RC1_TIE_NAMES == null)
                {
                    __RC1_TIE_NAMES = GetModelNames("TieModelsRC1.txt");
                }
                return __RC1_TIE_NAMES;
            }
        }

        public static Dictionary<int, string> RC2_MOB_NAMES
        {
            get
            {
                if (__RC2_MOB_NAMES == null)
                {
                    __RC2_MOB_NAMES = GetModelNames("MobyModelsRC2.txt");
                }
                return __RC2_MOB_NAMES;
            }
        }


        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        private static Dictionary<int, string> GetModelNames(string fileName)
        {
            var modelNames = new Dictionary<int, string>();

            try
            {
                string? applicationFolder = System.AppContext.BaseDirectory;
                string listFolder = Path.Join(applicationFolder, "ModelLists");
                var fullPath = Path.Join(listFolder, fileName);

                using (StreamReader stream = new StreamReader(fullPath))
                {
                    string? line;
                    while ((line = stream.ReadLine()) != null)
                    {
                        string[] stringPart = line.Split('=');
                        int modelId = int.Parse(stringPart[0], NumberStyles.HexNumber);
                        modelNames.Add(modelId, stringPart[1]);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                LOGGER.Warn("Model list file not found! No names for you!");
            }

            return modelNames;
        }

        public static string? GetModelName(Model obj, GameType game)
        {
            string? result = "Unknown";

            switch (game.num)
            {
                case 1:
                    if (obj is MobyModel)
                    {
                        if (!RC1_MOB_NAMES.TryGetValue(obj.id, out result)) return null;
                        break;
                    }
                    else if (obj is TieModel)
                    {
                        if (!RC1_TIE_NAMES.TryGetValue(obj.id, out result)) return null;
                        break;
                    }
                    else
                    {
                        return null;
                    }
                case 2:
                    if (obj is MobyModel)
                    {
                        if (!RC2_MOB_NAMES.TryGetValue(obj.id, out result)) return null;
                        break;
                    }
                    else
                    {
                        return null;
                    }
                default:
                    return null;
            }

            return result;
        }
    }
}
