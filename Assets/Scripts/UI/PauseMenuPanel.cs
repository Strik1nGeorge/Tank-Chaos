using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pause menu panel with Resume, Restart, and Main Menu buttons.
/// </summary>
public class PauseMenuPanel : MonoBehaviour
{
    [Header("UI  (Inspector ?")]
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _mainMenuButton;

    public void Init()
    {
        if (_resumeButton == null)
            _resumeButton    = FindButton("ResumeButton");
        if (_restartButton == null)   _restartButton   = FindButton("RestartButton");
        if (_mainMenuButton == null)  _mainMenuButton  = FindButton("QuitButton");

        _resumeButton?.onClick.AddListener(() =>
        {
            Debug.Log("[UI] Pause: RESUME");
            GameManager.Instance?.TogglePause();
        });

        // Restart current level
        _restartButton?.onClick.AddListener(() =>
        {
            Debug.Log("[UI] Pause: RESTART LEVEL");
            GameManager.Instance?.RestartCurrentLevel();
        });

        // Return to main menu
        _mainMenuButton?.onClick.AddListener(() =>
        {
            Debug.Log("[UI] Pause: MAIN MENU");
            GameManager.Instance?.ReloadGame();
        });
    }

    private Button FindButton(string name)
    {
        Transform t = transform.Find(name);
        return t != null ? t.GetComponent<Button>() : null;
    }
}
