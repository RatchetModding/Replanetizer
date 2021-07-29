using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace Replanetizer.ModelLists
{
    public static class ModelLists
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        public static Lazy<Dictionary<int, string>> mobNames = new(() =>
        {
            return GetModelNames("ModelLists/ModelListRC1.txt");
        });
        
        public static Lazy<Dictionary<int, string>> tieNames = new(() =>
        {
            return GetModelNames("ModelLists/TieModelsRC1.txt");
        });

        private static Dictionary<int, string> GetModelNames(string fileName)
        {
            var modelNames = new Dictionary<int, string>();

            try
            {
                var fullPath = Path.Join(Assembly.GetExecutingAssembly().Location, fileName);
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
                Logger.Warn("Model list file not found! No names for you!");
            }

            return modelNames;
        }
    }
}