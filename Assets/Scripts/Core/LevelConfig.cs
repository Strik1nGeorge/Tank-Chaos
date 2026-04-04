
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
    public int    BgmTrackIndex;      // 0-based, used by AudioManager
}

/// <summary>
/// Static database of all level configurations.
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

    public static int TotalLevels => 5;

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
                TimeLimit         = -1f,
                BasicEnemyCount   = 10,
                FastEnemyCount    = 0,
                InitialSpawnCount = 10,
                RespawnOnDeath    = false,
                HasArmoredEnemy   = false,
                HasBoss           = false,
                ObstacleDensity   = 1.0f,
                BgmTrackIndex     = 0,
            },

            // Level 2: The Escalation
            // Mixed enemy types with respawns
            new LevelConfig
            {
                LevelIndex        = 2,
                LevelName         = "The Escalation",
                LevelSubtitle     = "10 basic + 10 fast ?reinforcements incoming",
                TimeLimit         = -1f,
                BasicEnemyCount   = 10,
                FastEnemyCount    = 10,
                InitialSpawnCount = 10,
                RespawnOnDeath    = true,
                RespawnDelay      = 1.5f,
                HasArmoredEnemy   = false,
                HasBoss           = false,
                ObstacleDensity   = 1.0f,
                BgmTrackIndex     = 1,
            },

            // Level 3: Blitz Storm
            // 90-second timed survival, fast enemies only
            new LevelConfig
            {
                LevelIndex        = 3,
                LevelName         = "Blitz Storm",
                LevelSubtitle     = "Outrun the clock ?90 seconds!",
                TimeLimit         = 90f,
                BasicEnemyCount   = 0,
                FastEnemyCount    = 15,
                InitialSpawnCount = 15,
                RespawnOnDeath    = false,
                HasArmoredEnemy   = false,
                HasBoss           = false,
                ObstacleDensity   = 1.4f,
                BgmTrackIndex     = 2,
            },

            // Level 4: Siege Protocol
            // Armored enemies with high HP, slow respawn
            new LevelConfig
            {
                LevelIndex        = 4,
                LevelName         = "Siege Protocol",
                LevelSubtitle     = "Armored enemies ?aim for weak spots",
                TimeLimit         = -1f,
                BasicEnemyCount   = 10,
                FastEnemyCount    = 10,
                InitialSpawnCount = 5,
                RespawnOnDeath    = true,
                RespawnDelay      = 4.0f,
                HasArmoredEnemy   = true,
                HasBoss           = false,
                ObstacleDensity   = 1.2f,
                BgmTrackIndex     = 3 % 4,
            },

            // Level 5: Titan's Wrath
            // Boss fight with reinforcements
            new LevelConfig
            {
                LevelIndex        = 5,
                LevelName         = "Titan's Wrath",
                LevelSubtitle     = "Face the Titan ?and survive",
                TimeLimit         = -1f,
                BasicEnemyCount   = 5,
                FastEnemyCount    = 0,
                InitialSpawnCount = 5,
                RespawnOnDeath    = false,
                HasArmoredEnemy   = false,
                HasBoss           = true,
                BossReinforceCount= 5,
                ObstacleDensity   = 0.8f, // fewer obstacles so the boss can move
                BgmTrackIndex     = 0,
            },
        };
    }
}
