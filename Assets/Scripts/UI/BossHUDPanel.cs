using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Boss-specific HUD panel (used in Level 5).
/// Provides static API for BossEnemy to update health and phase info.
/// Auto-finds UI elements via Find if not set in Inspector.
/// </summary>
public class BossHUDPanel : MonoBehaviour
{
    private static BossHUDPanel _instance;

    [SerializeField] private Slider _healthSlider;
    [SerializeField] private Text   _bossNameText;
    [SerializeField] private Text   _phaseText;
    [SerializeField] private Image  _fillImage;

    // Phase colors
    private static readonly Color P1_COLOR = new Color(0.8f, 0.1f, 0.1f);
    private static readonly Color P2_COLOR = new Color(1.0f, 0.4f, 0.0f);
    private static readonly Color P3_COLOR = new Color(1.0f, 0.0f, 0.8f);

    private void Awake()
    {
        _instance = this;
        gameObject.SetActive(false);
        AutoFind();
    }

    private void AutoFind()
    {
        if (_healthSlider == null) _healthSlider = GetComponentInChildren<Slider>(true);
        if (_bossNameText == null)
        {
            Transform t = transform.Find("BossName");
            if (t != null) _bossNameText = t.GetComponent<Text>();
        }
        if (_phaseText == null)
        {
            Transform t = transform.Find("PhaseText");
            if (t != null) _phaseText = t.GetComponent<Text>();
        }
        if (_fillImage == null && _healthSlider != null)
            _fillImage = _healthSlider.fillRect?.GetComponent<Image>();
    }

    // --- Public static API ---

    public static void Show(bool visible)
    {
        EnsureInstance();
        if (_instance != null) _instance.gameObject.SetActive(visible);
    }

    public static void UpdateHealth(int current, int max)
    {
        EnsureInstance();
        if (_instance == null) return;
        _instance.AutoFind();

        float ratio = max > 0 ? (float)current / max : 0f;

        if (_instance._healthSlider != null)
            _instance._healthSlider.value = ratio;

        // Phase-based fill color
        if (_instance._fillImage != null)
        {
            if (ratio > 0.66f)      _instance._fillImage.color = P1_COLOR;
            else if (ratio > 0.33f) _instance._fillImage.color = P2_COLOR;
            else                    _instance._fillImage.color = P3_COLOR;
        }
    }

    public static void SetPhaseName(string phaseName)
    {
        EnsureInstance();
        if (_instance?._phaseText != null)
            _instance._phaseText.text = phaseName;
    }

    private static void EnsureInstance()
    {
        if (_instance == null)
        {
            BossHUDPanel found = FindFirstObjectByType<BossHUDPanel>(FindObjectsInactive.Include);
            if (found != null) _instance = found;
        }
    }
}
