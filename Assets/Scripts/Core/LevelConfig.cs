
/// <summary>
/// Data class holding per-level configuration.
/// </summary>
public class LevelConfig
{
    public int    LevelIndex;
    public string LevelName;
    public string LevelSubtitle;
    public float  TimeLimit;          // -1 = no time limit
    public int    BasicEnemyCount;
    public int    FastEnemyCount;
    public int    InitialSpawnCount;
    public bool   RespawnOnDeath;
    public float  RespawnDelay;
    public bool   HasArmoredEnemy;
    public bool   HasBoss;
    public int    BossReinforceCount;
    public float  ObstacleDensity;    // 1.0 = default
    public bool   UseFixedMap;        // true = use a pre-designed fixed map
    public int    BgmTrackIndex;      // 0-based, used by AudioManager
}

/// <summary>
/// Static database of all level configurations (3 levels).
/// </summary>
public static class LevelDatabase
{
    private static LevelConfig[] _levels;

    public static LevelConfig Get(int levelIndex)
    {
        if (_levels == null) Build();
        // levelIndex is 1-based
        int idx = UnityEngine.Mathf.Clamp(levelIndex - 1, 0, _levels.Length - 1);
        return _levels[idx];
    }

    public static int TotalLevels => 3;

    private static void Build()
    {
        _levels = new LevelConfig[]
        {
            // Level 1: The Proving Grounds
            // Basic enemies only, no respawns
            new LevelConfig
            {
                LevelIndex        = 1,
                LevelName         = "The Proving Grounds",
                LevelSubtitle     = "Eliminate all enemies",
                TimeLimit         = 150f,
                BasicEnemyCount   = 10,
                FastEnemyCount    = 0,
                InitialSpawnCount = 10,
                RespawnOnDeath    = false,
                HasArmoredEnemy   = false,
                HasBoss           = false,
                ObstacleDensity   = 1.0f,
                BgmTrackIndex     = 1,
            },

            // Level 2: The Escalation
            // Mixed enemy types with respawns
            new LevelConfig
            {
                LevelIndex        = 2,
                LevelName         = "The Escalation",
                LevelSubtitle     = "10 basic + 10 fast — reinforcements incoming",
                TimeLimit         = 150f,
                BasicEnemyCount   = 10,
                FastEnemyCount    = 10,
                InitialSpawnCount = 10,
                RespawnOnDeath    = true,
                RespawnDelay      = 1.5f,
                HasArmoredEnemy   = false,
                HasBoss           = false,
                ObstacleDensity   = 1.0f,
                BgmTrackIndex     = 2,
            },

            // Level 3: Titan's Wrath
            // Boss fight with reinforcements
            new LevelConfig
            {
                LevelIndex        = 3,
                LevelName         = "Titan's Wrath",
                LevelSubtitle     = "Face the Titan — and survive",
                TimeLimit         = -1f,
                BasicEnemyCount   = 5,
                FastEnemyCount    = 0,
                InitialSpawnCount = 5,
                RespawnOnDeath    = false,
                HasArmoredEnemy   = false,
                HasBoss           = true,
                BossReinforceCount= 5,
                ObstacleDensity   = 0.6f,   // Reduced so Boss has more room to navigate
                UseFixedMap       = false,
                BgmTrackIndex     = 3,
            },
        };
    }
}
