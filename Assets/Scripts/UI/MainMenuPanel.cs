using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Main menu panel with navigation buttons (Start, Garage, Leaderboard, Settings, Quit).
/// References are set via Inspector or resolved at runtime as fallback.
/// </summary>
public class MainMenuPanel : MonoBehaviour
{
    [Header("UI  (Inspector ?")]
    [SerializeField] private RectTransform _titleRect;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _garageButton;
    [SerializeField] private Button _leaderboardButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _quitButton;

    private System.Action _onStartGame;
    private System.Action _onOpenLeaderboard;
    private System.Action _onOpenGarage;
    private System.Action _onOpenSettings;

    private void Awake()
    {
        if (_titleRect == null)
        {
            Transform t = transform.Find("TitleText");
            if (t != null) _titleRect = t.GetComponent<RectTransform>();
        }
        if (_startButton == null)       _startButton       = FindButton("StartButton");
        if (_garageButton == null)       _garageButton       = FindButton("GarageBtn");
        if (_leaderboardButton == null)  _leaderboardButton  = FindButton("LeaderboardBtn");
        if (_settingsButton == null)     _settingsButton     = FindButton("SettingsBtn");
        if (_quitButton == null)         _quitButton         = FindButton("QuitButton");
    }

    public void Init(System.Action onStartGame, System.Action onOpenLeaderboard, System.Action onOpenGarage, System.Action onOpenSettings)
    {
        _onStartGame       = onStartGame;
        _onOpenLeaderboard = onOpenLeaderboard;
        _onOpenGarage      = onOpenGarage;
        _onOpenSettings    = onOpenSettings;

        _startButton?.onClick.AddListener(() =>
        {
            Debug.Log("[UI] Clicked START GAME ?Level Select");
            _onStartGame?.Invoke();
        });

        _garageButton?.onClick.AddListener(() =>
        {
            Debug.Log("[UI] Clicked GARAGE");
            _onOpenGarage?.Invoke();
        });

        _leaderboardButton?.onClick.AddListener(() =>
        {
            Debug.Log("[UI] Clicked LEADERBOARDS");
            _onOpenLeaderboard?.Invoke();
        });

        _settingsButton?.onClick.AddListener(() =>
        {
            Debug.Log("[UI] Clicked SETTINGS");
            _onOpenSettings?.Invoke();
        });

        _quitButton?.onClick.AddListener(() =>
        {
            Debug.Log("[UI] Clicked QUIT");
            Application.Quit();
        });
    }

    private void Update()
    {
        if (_titleRect != null)
        {
            float bounceY = 260f + Mathf.Sin(Time.unscaledTime * 2f) * 15f;
            _titleRect.anchoredPosition = new Vector2(0, bounceY);
        }
    }

    private Button FindButton(string name)
    {
        Transform t = transform.Find(name);
        return t != null ? t.GetComponent<Button>() : null;
    }
}
