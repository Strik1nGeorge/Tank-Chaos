using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen message panel for Victory and Game Over states.
/// References are set via Inspector or resolved at runtime as fallback.
/// </summary>
public class ScreenMessagePanel : MonoBehaviour
{
    [Header("UI References (Assign in Inspector)")]
    [SerializeField] private Text       _titleText;
    [SerializeField] private Text       _subtitleText;
    [SerializeField] private Text       _bodyText;
    [SerializeField] private GameObject _nextLevelBtnGO;
    [SerializeField] private GameObject _retryBtnGO;
    [SerializeField] private GameObject _restartBtnGO;
    [SerializeField] private GameObject _quitBtnGO;
    [SerializeField] private GameObject _inputFieldGO;
    [SerializeField] private InputField _nameInput;
    [SerializeField] private GameObject _saveBtnGO;

    public void Init()
    {
        // Auto-bind fallback
        if (_titleText == null)     _titleText     = FindText("TitleText");
        if (_subtitleText == null)  _subtitleText  = FindText("SubtitleText");
        if (_bodyText == null)      _bodyText      = FindText("BodyText");
        if (_nextLevelBtnGO == null) _nextLevelBtnGO = FindGO("ButtonContainer/NextLevelBtn");
        if (_retryBtnGO == null)    _retryBtnGO    = FindGO("ButtonContainer/RetryBtn");
        if (_restartBtnGO == null)  _restartBtnGO  = FindGO("ButtonContainer/RestartBtn");
        if (_quitBtnGO == null)     _quitBtnGO     = FindGO("ButtonContainer/QuitBtn");
        if (_inputFieldGO == null)  _inputFieldGO  = FindGO("NameInput");
        if (_saveBtnGO == null)     _saveBtnGO     = FindGO("SaveBtn");

        if (_nameInput == null && _inputFieldGO != null)
            _nameInput = _inputFieldGO.GetComponent<InputField>();

        // Bind button actions
        BindButton(_nextLevelBtnGO, () =>
        {
            int next = (GameManager.Instance?.CurrentLevel ?? 1) + 1;
            GameManager.Instance?.StartLevel(next);
        });
        BindButton(_retryBtnGO,    () => { GameManager.Instance?.RestartCurrentLevel(); });
        BindButton(_restartBtnGO,  () => { GameManager.Instance?.ReloadGame(); });
        BindButton(_quitBtnGO,     () => { GameManager.Instance?.QuitGame(); });
        BindButton(_saveBtnGO, () =>
        {
            int   lvl   = GameManager.Instance != null ? GameManager.Instance.CurrentLevel  : 1;
            int   score = GameManager.Instance != null ? GameManager.Instance.CurrentScore  : 0;
            float time  = GameManager.Instance != null ? GameManager.Instance.TotalClearTime: 0f;
            string name = (_nameInput != null && _nameInput.text.Length > 0) ? _nameInput.text : "PLAYER";
            SaveSystem.AddAndSaveEntry(name, score, time, lvl);
            
            // Disable save controls after saving
            if (_saveBtnGO != null)
            {
                Button b = _saveBtnGO.GetComponent<Button>();
                if (b != null) b.interactable = false;
            }
            if (_nameInput != null) _nameInput.interactable = false;
            
            Debug.Log("[UI] Score saved successfully.");
        });
    }

    public void Setup(GameState state)
    {
        int   score = GameManager.Instance != null ? GameManager.Instance.CurrentScore   : 0;
        int   level = GameManager.Instance != null ? GameManager.Instance.CurrentLevel   : 1;
        float time  = GameManager.Instance != null ? GameManager.Instance.TotalClearTime : 0f;

        // Hide all buttons first
        SetActive(_nextLevelBtnGO, false);
        SetActive(_retryBtnGO,     false);
        SetActive(_restartBtnGO,   false);
        SetActive(_quitBtnGO,      false);
        SetActive(_inputFieldGO,   false);
        SetActive(_saveBtnGO,      false);
        if (_bodyText != null) _bodyText.text = "";

        switch (state)
        {
            case GameState.Victory:
                bool isFinalLevel = (level >= LevelDatabase.TotalLevels);
                
                // Read bonus breakdown from GameManager
                int killScore  = GameManager.Instance != null ? GameManager.Instance.KillScore  : 0;
                int timeBonus  = GameManager.Instance != null ? GameManager.Instance.TimeBonus  : 0;
                int livesBonus = GameManager.Instance != null ? GameManager.Instance.LivesBonus : 0;

                string scoreBreakdown = $"Kill Score: {killScore}\n" +
                                        $"Time Bonus: +{timeBonus}\n" +
                                        $"Lives Bonus: +{livesBonus}\n" +
                                        $"──────────\n" +
                                        $"Total: {score}";

                // Show save input
                SetActive(_inputFieldGO, true);
                SetActive(_saveBtnGO,    true);
                if (_saveBtnGO != null) 
                {
                    Button b = _saveBtnGO.GetComponent<Button>();
                    if (b != null) b.interactable = true;
                }
                if (_nameInput != null) _nameInput.interactable = true;

                if (isFinalLevel)
                {
                    // Final level (L3) victory
                    SetTitle("TITAN DEFEATED!", new Color(1f, 0.85f, 0.1f));
                    SetSubtitle("You conquered all 3 levels!", Color.white);
                    SetBody($"{scoreBreakdown}\n\nTime: {time:F1}s\nRecord your legendary run!");
                    SetActive(_restartBtnGO, true);
                    SetActive(_quitBtnGO,    true);
                }
                else
                {
                    // Normal level (L1-L4) victory
                    LevelConfig nextCfg = LevelDatabase.Get(level + 1);
                    SetTitle($"LEVEL {level} CLEAR!", new Color(0.3f, 1f, 0.4f));
                    SetSubtitle($"Next: {nextCfg.LevelName}", Color.white);
                    SetBody($"{scoreBreakdown}\n\nTime: {time:F1}s");
                    SetActive(_nextLevelBtnGO, true);
                    SetActive(_restartBtnGO,   true);
                }
                break;

            case GameState.GameOver:
                SetTitle("GAME OVER", new Color(1f, 0.2f, 0.2f));
                SetSubtitle("You have been defeated...", new Color(1f, 0.6f, 0.4f));
                SetBody($"Your tank was destroyed on Level {level}!\nFinal Score: {score}");
                SetActive(_retryBtnGO,   true);
                SetActive(_restartBtnGO, true);
                SetActive(_quitBtnGO,    true);
                break;
        }

        Debug.Log($"[UI] ScreenMessagePanel setup: state={state}, level={level}, score={score}");
    }

    // --- Helper methods ---

    private void SetTitle(string text, Color color)
    {
        if (_titleText == null) return;
        _titleText.text  = text;
        _titleText.color = color;
    }

    private void SetSubtitle(string text, Color color)
    {
        if (_subtitleText == null) return;
        _subtitleText.text  = text;
        _subtitleText.color = color;
    }

    private void SetBody(string text)
    {
        if (_bodyText == null) return;
        _bodyText.text = text;
    }

    private void BindButton(GameObject go, System.Action action)
    {
        if (go == null) return;
        Button btn = go.GetComponent<Button>();
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => action?.Invoke());
    }

    private void SetActive(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }

    private Text FindText(string name)
    {
        Transform t = transform.Find(name);
        return t != null ? t.GetComponent<Text>() : null;
    }

    private GameObject FindGO(string name)
    {
        Transform t = transform.Find(name);
        return t != null ? t.gameObject : null;
    }
}
