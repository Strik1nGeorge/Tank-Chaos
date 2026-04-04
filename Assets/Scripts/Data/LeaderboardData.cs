using System;
using System.Collections.Generic;

/// <summary>Single leaderboard entry with level index</summary>
[Serializable]
public class LeaderboardEntry
{
    public string PlayerName;
    public int    Score;
    public float  ClearTime;
    public string DateString;
    public int    LevelIndex; // 1~5

    public LeaderboardEntry(string name, int score, float clearTime, int levelIndex = 1)
    {
        PlayerName = name;
        Score      = score;
        ClearTime  = clearTime;
        LevelIndex = levelIndex;
        DateString = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
    }
}

/// <summary>Leaderboard container for a single level (wrapper class for JsonUtility)</summary>
[Serializable]
public class LeaderboardList
{
    public List<LeaderboardEntry> Entries = new List<LeaderboardEntry>();
}

/// <summary>Top-level container for all 5 level leaderboards</summary>
[Serializable]
public class AllLeaderboards
{
    public LeaderboardList Level1 = new LeaderboardList();
    public LeaderboardList Level2 = new LeaderboardList();
    public LeaderboardList Level3 = new LeaderboardList();
    public LeaderboardList Level4 = new LeaderboardList();
    public LeaderboardList Level5 = new LeaderboardList();

    public LeaderboardList GetLevel(int levelIndex)
    {
        switch (levelIndex)
        {
            case 1: return Level1;
            case 2: return Level2;
            case 3: return Level3;
            case 4: return Level4;
            case 5: return Level5;
            default: return Level1;
        }
    }
}
