using UnityEngine;

/// <summary>
/// UIManager — Global UI management hub.
/// Added: Support for LevelSelectPanel state.
/// GameState.Victory replaces old VictoryL1/VictoryL2 enums.
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("Panel References (Assign in Inspector)")]
    [SerializeField] private MainMenuPanel      _mainMenu;
    [SerializeField] private LevelSelectPanel   _levelSelectPanel;
    [SerializeField] private PauseMenuPanel     _pauseMenu;
    [SerializeField] private HUDPanel           _hudPanel;
    [SerializeField] private ScreenMessagePanel _msgPanel;
    [SerializeField] private LeaderboardPanel   _leaderboardPanel;
    [SerializeField] private GaragePanel        _garagePanel;
    [SerializeField] private SettingsPanel      _settingsPanel;

    private void Awake()
    {
        if (_mainMenu == null)         _mainMenu          = FindInactive<MainMenuPanel>();
        if (_levelSelectPanel == null) _levelSelectPanel  = FindInactive<LevelSelectPanel>();
        if (_pauseMenu == null)        _pauseMenu         = FindInactive<PauseMenuPanel>();
        if (_hudPanel == null)         _hudPanel          = FindInactive<HUDPanel>();
        if (_msgPanel == null)         _msgPanel          = FindInactive<ScreenMessagePanel>();
        if (_leaderboardPanel == null) _leaderboardPanel  = FindInactive<LeaderboardPanel>();
        if (_garagePanel == null)      _garagePanel       = FindInactive<GaragePanel>();
        if (_settingsPanel == null)    _settingsPanel     = FindInactive<SettingsPanel>();

        Debug.Log($"[UIManager] Panels found: MainMenu={_mainMenu != null}, LevelSelect={_levelSelectPanel != null}, " +
                  $"Pause={_pauseMenu != null}, HUD={_hudPanel != null}, Msg={_msgPanel != null}");
    }

    private void Start()
    {
        UIBuilder.EnsureEventSystem();

        // ── MainMenu Initialization ──────────────────────
        _mainMenu?.Init(
            onStartGame: () =>
            {
                // START button → Level Selection
                GameManager.Instance?.GoToLevelSelect();
            },
            onOpenLeaderboard: () =>
            {
                _leaderboardPanel?.gameObject.SetActive(true);
                _leaderboardPanel?.transform.SetAsLastSibling();
            },
            onOpenGarage: () =>
            {
                _garagePanel?.gameObject.SetActive(true);
                _garagePanel?.transform.SetAsLastSibling();
            },
            onOpenSettings: () =>
            {
                _settingsPanel?.gameObject.SetActive(true);
                _settingsPanel?.transform.SetAsLastSibling();
            }
        );

        // ── LevelSelect Initialization ──────────────────
        _levelSelectPanel?.Init(
            onLevelSelected: levelIndex => GameManager.Instance?.StartLevel(levelIndex),
            onBack: () => GameManager.Instance?.GoToMainMenu()
        );

        // ── Other Panels Initialization ──────────────────────
        _pauseMenu?.Init();
        _hudPanel?.Init();
        _msgPanel?.Init();
        _leaderboardPanel?.Init();
        _garagePanel?.Init(() => _garagePanel.gameObject.SetActive(false));
        _settingsPanel?.Init();

        // ── Default Hidden ────────────────────────────
        SetActive(_levelSelectPanel, false);
        SetActive(_pauseMenu,        false);
        SetActive(_hudPanel,         false);
        SetActive(_msgPanel,         false);
        SetActive(_leaderboardPanel, false);
        SetActive(_garagePanel,      false);
        SetActive(_settingsPanel,    false);

        // ── Subscribe to State Events ────────────────────────
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
            HandleGameStateChanged(GameManager.Instance.CurrentState);
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        Debug.Log($"[UIManager] State → {state}");

        // Main Menu
        SetActive(_mainMenu, state == GameState.MainMenu);

        // Level Select
        if (_levelSelectPanel != null)
        {
            bool show = state == GameState.LevelSelect;
            _levelSelectPanel.gameObject.SetActive(show);
            if (show) _levelSelectPanel.Init(
                onLevelSelected: lvl => GameManager.Instance?.StartLevel(lvl),
                onBack: () => GameManager.Instance?.GoToMainMenu()
            ); // Refresh unlock status
        }

        // Pause
        SetActive(_pauseMenu, state == GameState.Paused);

        // HUD (kept during gameplay and pause)
        bool showHUD = (state == GameState.Playing || state == GameState.Paused);
        SetActive(_hudPanel, showHUD);

        // Result Screen
        bool showMsg = (state == GameState.Victory || state == GameState.GameOver);
        if (_msgPanel != null)
        {
            _msgPanel.gameObject.SetActive(showMsg);
            if (showMsg) _msgPanel.Setup(state);
        }

        // Popups: Auto-close when not in Main Menu
        if (state != GameState.MainMenu)
        {
            SetActive(_settingsPanel,    false);
            SetActive(_leaderboardPanel, false);
            SetActive(_garagePanel,      false);
        }
    }

    private void SetActive<T>(T comp, bool active) where T : MonoBehaviour
    {
        if (comp != null) comp.gameObject.SetActive(active);
    }

    private T FindInactive<T>() where T : MonoBehaviour
    {
        T[] all = FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        return all.Length > 0 ? all[0] : null;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
    }
}
