using UnityEngine;
using System;
using UnityEngine.SceneManagement;

/// <summary>Global game state enum</summary>
public enum GameState
{
    MainMenu,
    LevelSelect,  // Added: Level selection screen
    Playing,
    Paused,
    GameOver,
    Victory       // Unified victory state (level info distinguished by CurrentLevel)
}

/// <summary>
/// GameManager — Core logic for level flow and global states.
/// Now supports 5-level progressive unlocking system.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ──────────────────────────────────────────
    // State
    // ──────────────────────────────────────────
    public GameState    CurrentState { get; private set; } = GameState.MainMenu;
    public int          CurrentLevel { get; private set; } = 1;
    public LevelConfig  CurrentConfig { get; private set; }

    public float TotalClearTime { get; private set; }
    public int   CurrentScore   { get; private set; } = 0;

    private float _levelTimer;
    public  float TimeRemaining => _levelTimer;

    // ──────────────────────────────────────────
    // Component References
    // ──────────────────────────────────────────
    private EnemySpawner _enemySpawner;
    private PlayerHealth _playerHealth;
    private MapGenerator _mapGenerator;

    // ──────────────────────────────────────────
    // Events
    // ──────────────────────────────────────────
    public event Action<GameState> OnGameStateChanged;

    // ══════════════════════════════════════════
    // Lifecycle
    // ══════════════════════════════════════════
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _mapGenerator = FindFirstObjectByType<MapGenerator>();
        _enemySpawner = EnemySpawner.Instance;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerHealth = player.GetComponent<PlayerHealth>();

        SubscribeToEvents();
        SetupPhysicsLayers();

        // Enter Main Menu on startup
        ChangeState(GameState.MainMenu);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            TogglePause();

        if (CurrentState == GameState.Playing)
        {
            TotalClearTime += Time.deltaTime;

            // Countdown
            if (CurrentConfig != null && CurrentConfig.TimeLimit > 0)
            {
                _levelTimer -= Time.deltaTime;
                if (_levelTimer <= 0f)
                {
                    _levelTimer = 0f;
                    HandleTimeUp();
                }
            }
        }
    }

    private void OnDestroy() => UnsubscribeFromEvents();

    // ══════════════════════════════════════════
    // Public API — Level Control
    // ══════════════════════════════════════════

    /// <summary>Enter a level from the level selection screen</summary>
    public void StartLevel(int levelIndex)
    {
        LevelConfig cfg = LevelDatabase.Get(levelIndex);
        CurrentLevel   = levelIndex;
        CurrentConfig  = cfg;
        CurrentScore   = 0;
        TotalClearTime = 0f;

        // Timer initialization (-1 means infinite, represented by float.MaxValue internally)
        _levelTimer = cfg.TimeLimit > 0 ? cfg.TimeLimit : float.MaxValue;

        // Clear leftovers from previous scene
        BulletPool.Instance?.ClearAllActiveBullets();
        _enemySpawner?.ClearAllEnemies();

        // Reset/Regenerate map
        if (_mapGenerator != null)
            _mapGenerator.GenerateWithDensity(cfg.ObstacleDensity);

        // Reset player
        _playerHealth?.gameObject.SetActive(true);
        _playerHealth?.RefreshHealth();

        ChangeState(GameState.Playing);
        _enemySpawner?.StartLevel(cfg);

        // L5 Boss HUD
        if (cfg.HasBoss)
            BossHUDPanel.Show(true);

        Debug.Log($"[GameManager] Level {levelIndex} ({cfg.LevelName}) started. TimeLimit={cfg.TimeLimit}s");
    }

    // Backward compatibility for old calls
    public void StartLevel1() => StartLevel(1);
    public void StartLevel2() => StartLevel(2);

    /// <summary>Go to level selection screen</summary>
    public void GoToLevelSelect()
    {
        ChangeState(GameState.LevelSelect);
    }

    /// <summary>Return to main menu (from level select or other screens)</summary>
    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        ChangeState(GameState.MainMenu);
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Playing)
            ChangeState(GameState.Paused);
        else if (CurrentState == GameState.Paused)
            ChangeState(GameState.Playing);
    }

    public void AddScore(int pts)
    {
        CurrentScore += pts;
        Debug.Log($"[GameManager] +{pts} pts → Total: {CurrentScore}");
    }

    public void ReloadGame()
    {
        Time.timeScale = 1f;
        BossHUDPanel.Show(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void RestartCurrentLevel()
    {
        Debug.Log($"[GameManager] RestartCurrentLevel — L{CurrentLevel}");
        Time.timeScale = 1f;

        CurrentScore   = 0;
        TotalClearTime = 0f;

        _enemySpawner?.ClearAllEnemies();

        if (_playerHealth != null)
        {
            _playerHealth.gameObject.SetActive(true);
            _playerHealth.transform.position = PlayerSpawnPoint.Position;
            _playerHealth.ResetHealth();
        }
        else
        {
            ReloadGame();
            return;
        }

        ChangeState(GameState.Playing);

        LevelConfig cfg = LevelDatabase.Get(CurrentLevel);
        _levelTimer = cfg.TimeLimit > 0 ? cfg.TimeLimit : float.MaxValue;
        _enemySpawner?.StartLevel(cfg);

        if (cfg.HasBoss) BossHUDPanel.Show(true);

        Debug.Log($"[GameManager] RestartCurrentLevel done. State={CurrentState}");
    }

    public void QuitGame()
    {
        Debug.Log("[GameManager] Quit");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ══════════════════════════════════════════
    // Internal Event Handling
    // ══════════════════════════════════════════

    private void HandleAllEnemiesCleared()
    {
        if (CurrentState != GameState.Playing) return;

        // Unlock next level
        SaveSystem.UnlockLevel(CurrentLevel + 1);
        BossHUDPanel.Show(false);

        ChangeState(GameState.Victory);
        Debug.Log($"[GameManager] L{CurrentLevel} Cleared! Next level unlocked.");
    }

    private void HandlePlayerDied()
    {
        if (CurrentState != GameState.Playing) return;
        BossHUDPanel.Show(false);
        ChangeState(GameState.GameOver);
        Debug.Log("[GameManager] Player died → GameOver");
    }

    private void HandleTimeUp()
    {
        if (CurrentState != GameState.Playing) return;
        BossHUDPanel.Show(false);
        ChangeState(GameState.GameOver);
        Debug.Log("[GameManager] Time up → GameOver");
    }

    // ══════════════════════════════════════════
    // Physics Layers & Event Registration
    // ══════════════════════════════════════════

    private void SubscribeToEvents()
    {
        if (_enemySpawner != null) _enemySpawner.OnAllEnemiesCleared += HandleAllEnemiesCleared;
        if (_playerHealth != null) _playerHealth.OnPlayerDied         += HandlePlayerDied;
    }

    private void UnsubscribeFromEvents()
    {
        if (_enemySpawner != null) _enemySpawner.OnAllEnemiesCleared -= HandleAllEnemiesCleared;
        if (_playerHealth != null) _playerHealth.OnPlayerDied         -= HandlePlayerDied;
    }

    private void SetupPhysicsLayers()
    {
        int pl  = LayerMask.NameToLayer("Player");
        int el  = LayerMask.NameToLayer("Enemy");
        int pb  = LayerMask.NameToLayer("PlayerBullet");
        int eb  = LayerMask.NameToLayer("EnemyBullet");
        if (pl == -1 || el == -1 || pb == -1 || eb == -1)
        {
            Debug.LogWarning("[GameManager] Physics layers not fully configured. Please add them in Project Settings > Tags & Layers.");
            return;
        }
        Physics2D.IgnoreLayerCollision(pl, pb, true);
        Physics2D.IgnoreLayerCollision(el, eb, true);
        Physics2D.IgnoreLayerCollision(pb, eb, true);
        Physics2D.IgnoreLayerCollision(pb, pb, true);
        Physics2D.IgnoreLayerCollision(eb, eb, true);
    }

    private void ChangeState(GameState newState)
    {
        CurrentState = newState;
        switch (newState)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            default:
                Time.timeScale = 0f;
                break;
        }
        OnGameStateChanged?.Invoke(newState);
        Debug.Log($"[GameManager] State → {newState}");
    }
}
