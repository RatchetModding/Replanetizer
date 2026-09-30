using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using LibReplanetizer;

using LevelFilesList = System.Collections.Generic.List<LevelLists.LevelFileInfo>;

public static class LevelLists
{
    public class LevelFileInfo
    {
        public LevelFileInfo(int id, string name, string? location)
        {
            
            Id = id;
            Name = name;
            Location = location;

            if (location != null && !File.Exists(location))
                throw new FileNotFoundException($"Level file not found at {location}");
        }

        /* Level ID ("%d" in "level%d" folder) */
        public int Id { get; }

        /* Level name */
        public string Name { get; }

        /* Location is null if level file was not found */
        public string? Location { get; }
    }

    private static string LEVEL_LISTS_FOLDER = Path.Join(AppContext.BaseDirectory, "LevelLists");

    private static Dictionary<GameType, string> PER_GAME_LEVELS_FILE_NAME = new() {
            { GameType.RaC1, "RC1.txt" },
            { GameType.RaC2, "RC2.txt" },
            { GameType.RaC3, "RC3.txt" },
            { GameType.DL, "RC4.txt" },
    };

    private static Dictionary<int, string>? GetGameLevelNames(GameType game)
    {
        string path = Path.Join(LEVEL_LISTS_FOLDER, PER_GAME_LEVELS_FILE_NAME[game]);

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

    private static readonly Dictionary<string, GameType> GAME_IDS = new() {
        { "NPEA00385", GameType.RaC1 },
        { "NPUA80643", GameType.RaC1 },
        { "NPEA00386", GameType.RaC2 },
        { "NPUA80644", GameType.RaC2 },
        { "NPEA00387", GameType.RaC3 },
        { "NPUA80645", GameType.RaC3 },
        { "NPEA00423", GameType.DL },
        { "NPUA80646", GameType.DL },
    };

    private static readonly Dictionary<string, GameType> GENERIC_MAPPINGS = new() {
        { "rc1", GameType.RaC1 },
        { "rc2", GameType.RaC2 },
        { "rc3", GameType.RaC3 },
        { "rc4", GameType.DL },
    };

    private static LevelFilesList? ProbeGameDirectory(string ps3dataFolderPath, GameType gameType)
    {
        int nonEmptyLevelCount = 0;
        var result = new LevelFilesList();
        var gameLevels = GetGameLevelNames(gameType);

        if (gameLevels == null)
        {
            Debug.WriteLine($"No level list found for game {gameType.num}?!");
            return null;
        }

        foreach(var (id, name) in gameLevels)
        {
            string levelFilePath = Path.Join(ps3dataFolderPath, $"level{id}", "engine.ps3");
            string? location = null;

            if (File.Exists(levelFilePath))
            {
                nonEmptyLevelCount++;
                location = levelFilePath;
            }

            result.Add(new LevelFileInfo(id, name, location));
        }

        return nonEmptyLevelCount > 0 ? result : null;
    }

    private static GameType? DetectGameFromPath(string ps3dataPath)
    {
        foreach (var entry in GAME_IDS)
            if (ps3dataPath.Contains(entry.Key))
                return entry.Value;
        foreach (var entry in GENERIC_MAPPINGS)
            if (ps3dataPath.Contains(entry.Key))
                return entry.Value;
        return null;
    }

    public static Dictionary<GameType, LevelFilesList>? ProbeDirectory(string folderPath)
    {
        LevelFilesList? gameLevels;

        if (!Directory.Exists(folderPath))
            return null;

        var result = new Dictionary<GameType, LevelFilesList>();

        /*
         * Check if folderPath is the root folder of the Collection:
         * <root>/
         *     rc1/ps3data/...
         *     rc2/ps3data/...
         *     rc3/ps3data/...
         *
         * For convenience, we don't require ALL games' subfolders to
         * be present, and we also take "rc4" into account, even though
         * it would never be present inside the actual Collection's files
         * (since RC4 was a standalone game).
         */
        foreach (var entry in GENERIC_MAPPINGS)
        {
            string gameFolder = Path.Join(folderPath, entry.Key.ToLower());
            if (!Directory.Exists(gameFolder))
                continue;

            string gamePs3DataFolder = Path.Join(gameFolder, "ps3data");
            if (!Directory.Exists(gamePs3DataFolder))
                continue;

            gameLevels = ProbeGameDirectory(gamePs3DataFolder, entry.Value);
            if (gameLevels == null)
                continue;

            result[entry.Value] = gameLevels;
        }

        if (result.Count > 0)
        {
            return result;
        }

        /*
         * We didn't find anything using the Collection root search,
         * so folderPath must be a game's ps3data folder instead.
         * Use heuristics based on folderPath to determine which game
         * this is, then return the level list for that game if found.
         *
         * TODO: could we load one level's engine.ps3 to figure out
         *       which game this is instead of path heuristics?
         */
        var game = DetectGameFromPath(folderPath);
        if (game == null)
            return null;

        gameLevels = ProbeGameDirectory(folderPath, game);
        if (gameLevels == null)
            return null;

        result[game] = gameLevels;

        return result;
    }
}
