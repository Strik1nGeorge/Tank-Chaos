using System.Collections.Generic;
using System.IO;
using UnityEngine;


/// <summary>
/// Persistent save system.
/// - Leaderboards: stored as JSON via AllLeaderboards.
/// - Level unlock progress: stored in PlayerPrefs (MaxUnlockedLevel).
/// </summary>
public static class SaveSystem
{
    private static readonly string _saveFilePath =
        Path.Combine(Application.persistentDataPath, "leaderboard_v2.json");

    private const int MAX_ENTRIES     = 10;
    private const string UNLOCK_KEY   = "MaxUnlockedLevel";

    // ── Level unlock ──

    /// <summary>Returns the highest unlocked level (defaults to 1).</summary>
    public static int GetMaxUnlockedLevel()
    {
        return PlayerPrefs.GetInt(UNLOCK_KEY, 1);
    }

    /// <summary>Unlocks a level if it exceeds the current max.</summary>
    public static void UnlockLevel(int levelIndex)
    {
        int current = GetMaxUnlockedLevel();
        if (levelIndex > current)
        {
            PlayerPrefs.SetInt(UNLOCK_KEY, levelIndex);
            PlayerPrefs.Save();
            Debug.Log($"[SaveSystem] Unlocked Level {levelIndex}!");
        }
    }

    // ── Leaderboard loading ──

    /// <summary>Loads all leaderboards from disk.</summary>
    public static AllLeaderboards LoadAllLeaderboards()
    {
        if (!File.Exists(_saveFilePath))
            return new AllLeaderboards();

        try
        {
            string json = File.ReadAllText(_saveFilePath);
            AllLeaderboards data = JsonUtility.FromJson<AllLeaderboards>(json);
            return data ?? new AllLeaderboards();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] Load error: {e.Message}");
            return new AllLeaderboards();
        }
    }

    /// <summary>Loads entries for a single level.</summary>
    public static List<LeaderboardEntry> LoadLeaderboard(int levelIndex = 1)
    {
        return LoadAllLeaderboards().GetLevel(levelIndex).Entries;
    }

    // ── Leaderboard saving ──

    /// <summary>Adds a new entry and saves to disk.</summary>
    public static void AddAndSaveEntry(string playerName, int score, float clearTime, int levelIndex = 1)
    {
        if (string.IsNullOrWhiteSpace(playerName))
            playerName = "Unknown Tanker";

        AllLeaderboards all = LoadAllLeaderboards();
        LeaderboardList levelList = all.GetLevel(levelIndex);

        LeaderboardEntry entry = new LeaderboardEntry(playerName, score, clearTime, levelIndex);
        levelList.Entries.Add(entry);

        levelList.Entries.Sort((a, b) =>
        {
            int cmp = b.Score.CompareTo(a.Score);
            return cmp != 0 ? cmp : a.ClearTime.CompareTo(b.ClearTime);
        });

        // Trim to max entries
        if (levelList.Entries.Count > MAX_ENTRIES)
            levelList.Entries = levelList.Entries.GetRange(0, MAX_ENTRIES);

        try
        {
            string json = JsonUtility.ToJson(all, true);
            File.WriteAllText(_saveFilePath, json);
            Debug.Log($"[SaveSystem] L{levelIndex} saved: {playerName} {score}pts {clearTime:F1}s");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SaveSystem] Save error: {e.Message}");
        }
    }
}
