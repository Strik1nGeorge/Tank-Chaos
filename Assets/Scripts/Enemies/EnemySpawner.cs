using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages enemy spawning based on LevelConfig for Levels 1-5.
/// - L1, L3: Basic enemies
/// - L2, L4: Basic + Fast enemies with respawn-on-death
/// - L4: Armored enemies
/// - L5: Boss + reinforcements
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    // Spawner state
    private LevelConfig _config;
    private int  _basicRemaining;
    private int  _fastRemaining;
    private int  _activeEnemies;
    private int  _bossActive; // 0 or 1

    private int   _pendingRespawnCount;
    private float _respawnTimer;

    // Boss reinforcement count
    private int  _reinforceRemaining;

    public System.Action OnAllEnemiesCleared;

    // Singleton setup
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (_pendingRespawnCount > 0)
        {
            _respawnTimer -= Time.deltaTime;
            if (_respawnTimer <= 0f)
            {
                // Spawn the next enemy
                SpawnNextEnemy();
                _pendingRespawnCount--;

                // Reset timer if more pending
                if (_pendingRespawnCount > 0)
                    _respawnTimer = _config?.RespawnDelay ?? 1.0f;
                else
                    CheckClear(); // No more pending — check if level is clear
                    }
        }
    }

    // Public API

    /// <summary>Start spawning enemies for a level.</summary>
    public void StartLevel(LevelConfig cfg)
    {
        _config = cfg;
        _basicRemaining     = cfg.BasicEnemyCount;
        _fastRemaining      = cfg.FastEnemyCount;
        _activeEnemies      = 0;
        _bossActive         = 0;
        _reinforceRemaining = cfg.BossReinforceCount;
        _pendingRespawnCount = 0;
        _respawnTimer       = 0f;

        Debug.Log($"[EnemySpawner] StartLevel {cfg.LevelIndex} ?Basic={_basicRemaining} Fast={_fastRemaining} Boss={cfg.HasBoss}");

        // L5: spawn Boss
        if (cfg.HasBoss)
        {
            SpawnBoss();
            for (int i = 0; i < Mathf.Min(cfg.InitialSpawnCount, _reinforceRemaining); i++)
                SpawnReinforce();
            return;
        }

        // Spawn InitialSpawnCount enemies at start
        int toSpawn = cfg.InitialSpawnCount;
        for (int i = 0; i < toSpawn; i++)
            SpawnNextEnemy();
    }

    // Convenience shortcuts
    public void StartLevel1() { StartLevel(LevelDatabase.Get(1)); }
    public void StartLevel2() { StartLevel(LevelDatabase.Get(2)); }

    /// <summary>Called when an enemy dies.</summary>
    public void OnEnemyDied(EnemyBase enemy)
    {
        bool isBoss = enemy is BossEnemy;

        if (isBoss)
        {
            _bossActive = 0;
            // Boss defeated — check clear
            CheckClear();
            return;
        }

        _activeEnemies--;

        bool hasMore = (_basicRemaining + _fastRemaining) > 0;

        if (hasMore && _config != null && _config.RespawnOnDeath)
        {
            // Respawn-on-death mode (L2, L4)
            _pendingRespawnCount++;
            // Start timer if this is the first pending
            if (_pendingRespawnCount == 1)
                _respawnTimer = _config.RespawnDelay;
        }
        else
        {
            // No more to spawn — check clear
            CheckClear();
        }
    }

    /// <summary>Called when the boss changes phase. Spawns reinforcements.</summary>
    public void OnBossPhaseChanged(int phase)
    {
        // Spawn reinforcements on phase change
        if (_reinforceRemaining > 0 && phase > 1)
        {
            int count = Mathf.Min(2, _reinforceRemaining);
            for (int i = 0; i < count; i++) SpawnReinforce();
        }
    }

    /// <summary>Destroy all living enemies and reset counters.</summary>
    public void ClearAllEnemies()
    {
        _basicRemaining     = 0;
        _fastRemaining      = 0;
        _activeEnemies      = 0;
        _bossActive         = 0;
        _reinforceRemaining = 0;
        _pendingRespawnCount = 0;

        EnemyBase[] living = FindObjectsByType<EnemyBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        foreach (var e in living) Destroy(e.gameObject);

        Debug.Log("[EnemySpawner] ClearAllEnemies done.");
    }

    // Internal spawn helpers

    private void SpawnNextEnemy()
    {
        if (_config == null) return;
        if (_basicRemaining + _fastRemaining <= 0) return;

        Vector2 pos = FindEmptySpawnPosition();

        // Weighted random: choose Basic or Fast based on remaining ratio
        bool spawnFast = false;
        int total = _basicRemaining + _fastRemaining;
        if (_fastRemaining > 0 && _basicRemaining > 0)
            spawnFast = (Random.Range(0, total) < _fastRemaining);
        else if (_fastRemaining > 0)
            spawnFast = true;

        string prefabName = spawnFast ? "Prefabs/FastEnemy" : "Prefabs/BasicEnemy";

        if (spawnFast) _fastRemaining--;
        else           _basicRemaining--;

        GameObject prefab = Resources.Load<GameObject>(prefabName);
        if (prefab == null)
        {
            Debug.LogError($"[EnemySpawner] ?{prefabName} Prefab?Build Prefabs");
            return;
        }

        GameObject go = Object.Instantiate(prefab, pos, Quaternion.identity);

        // L4: armored enemies with boosted HP
        if (_config.HasArmoredEnemy)
        {
            go.transform.localScale = Vector3.one * 1.2f;
            EnemyBase eb = go.GetComponent<EnemyBase>();
            if (eb != null) eb.SetArmorMultiplier(3);
        }

        _activeEnemies++;
        Debug.Log($"[EnemySpawner] Spawned {go.name} @ {pos}. Remaining: Basic={_basicRemaining} Fast={_fastRemaining}");
    }

    private void SpawnBoss()
    {
        Vector2 pos = FindEmptySpawnPosition(minDist: 10f);
        GameObject prefab = Resources.Load<GameObject>("Prefabs/BossEnemy");
        if (prefab == null)
        {
            Debug.LogError("[EnemySpawner] ?Prefabs/BossEnemy?Build Prefabs");
            return;
        }
        Object.Instantiate(prefab, pos, Quaternion.identity);
        _bossActive = 1;
        Debug.Log($"[EnemySpawner] Boss spawned @ {pos}");
    }

    private void SpawnReinforce()
    {
        if (_reinforceRemaining <= 0) return;
        Vector2 pos = FindEmptySpawnPosition();
        GameObject prefab = Resources.Load<GameObject>("Prefabs/BasicEnemy");
        if (prefab != null)
        {
            Object.Instantiate(prefab, pos, Quaternion.identity);
            _reinforceRemaining--;
            _activeEnemies++;
        }
    }

    private void CheckClear()
    {
        bool bossStillAlive = _bossActive > 0;
        bool normalStillAlive = (_basicRemaining + _fastRemaining + _activeEnemies + _pendingRespawnCount) > 0;

        if (!bossStillAlive && !normalStillAlive)
        {
            Debug.Log("[EnemySpawner] All enemies cleared!");
            OnAllEnemiesCleared?.Invoke();
        }
    }

    private Vector2 FindEmptySpawnPosition(float minDist = 5f)
    {
        int attempts = 50;
        int layerMask = LayerMask.GetMask("Default", "Player");

        for (int i = 0; i < attempts; i++)
        {
            float x = Random.Range(1.5f, 21.5f);
            float y = Random.Range(1.5f, 21.5f);
            Vector2 pos = new Vector2(x, y);

            if (Vector2.Distance(pos, PlayerSpawnPoint.Position) < minDist) continue;
            if (Physics2D.OverlapCircle(pos, 0.4f, layerMask) != null) continue;
            return pos;
        }
        return new Vector2(Random.Range(2f, 20f), Random.Range(2f, 20f));
    }
}
