// Copyright (C) 2018-2021, The Replanetizer Contributors.
// Replanetizer is free software: you can redistribute it
// and/or modify it under the terms of the GNU General Public
// License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// Please see the LICENSE.md file for more details.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Replanetizer.ModelLists
{
    public static class ModelLists
    {
        private static readonly NLog.Logger LOGGER = NLog.LogManager.GetCurrentClassLogger();

        public static Lazy<Dictionary<int, string>> MOB_NAMES = new(() =>
        {
            return GetModelNames("ModelListRC1.txt");
        });

        public static Lazy<Dictionary<int, string>> TIE_NAMES = new(() =>
        {
            return GetModelNames("TieModelsRC1.txt");
        });

        private static Dictionary<int, string> GetModelNames(string fileName)
        {
            var modelNames = new Dictionary<int, string>();

            try
            {
                string applicationFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string listFolder = Path.Join(applicationFolder, "ModelLists");
                var fullPath = Path.Join(listFolder, fileName);

                using (StreamReader stream = new StreamReader(fullPath))
                {
                    string line;
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
    }
}