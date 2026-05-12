using System.Collections.Generic;
using System.IO;

public static class LevelLists
{
    public static Dictionary<int, string>? GetLevelNames(string gameFile, string levelListsFolder)
    {
        string path = Path.Join(levelListsFolder, $"{gameFile}.txt");

        if (!File.Exists(path))
            return null;

        var names = new Dictionary<int, string>();

        foreach (var line in File.ReadAllLines(path))
        {
            var parts = line.Split('=', 2);
            if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out int id))
                names[id] = parts[1].Trim();
        }

        return names;
    }

    private static readonly Dictionary<string, string> GAME_IDS = new() {
        { "NPEA00385", "RC1" },
        { "NPUA80643", "RC1" },
        { "NPEA00386", "RC2" },
        { "NPUA80644", "RC2" },
        { "NPEA00387", "RC3" },
        { "NPUA80645", "RC3" },
        { "NPEA00423", "RC4" },
        { "NPUA80646", "RC4" },
    };
    public static string? DetectGameFile(string path)
    {
        foreach (var entry in GAME_IDS)
            if (path.Contains(entry.Key))
                return entry.Value;
        return null;
    }
}
