using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Settings panel for adjusting BGM/SFX volume, mute toggle, and track switching.
/// References are resolved via Find in Init(); panel starts inactive.
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    [Header("UI  (Inspector ?")]
    [SerializeField] private Slider _bgmSlider;
    [SerializeField] private Slider _sfxSlider;
    [SerializeField] private Text   _bgmValueText;
    [SerializeField] private Text   _sfxValueText;
    [SerializeField] private Button _muteButton;
    [SerializeField] private Button _bgmSwitchButton;
    [SerializeField] private Text   _trackLabel;
    [SerializeField] private Button _backButton;

    private const string PREF_BGM = "Settings_BGM_Volume";
    private const string PREF_SFX = "Settings_SFX_Volume";

    private bool _isMuted;
    private float _preMuteBgm;
    private float _preMuteSfx;

    public void Init()
    {
        if (_bgmSlider == null)
            _bgmSlider       = FindInChildren<Slider>("BgmSlider");
        if (_sfxSlider == null)       _sfxSlider       = FindInChildren<Slider>("SfxSlider");
        if (_bgmValueText == null)    _bgmValueText    = FindInChildren<Text>("BgmValueText");
        if (_sfxValueText == null)    _sfxValueText    = FindInChildren<Text>("SfxValueText");
        if (_muteButton == null)      _muteButton      = FindInChildren<Button>("MuteBtn");
        if (_bgmSwitchButton == null) _bgmSwitchButton = FindInChildren<Button>("BgmSwitchBtn");
        if (_trackLabel == null)      _trackLabel      = FindInChildren<Text>("TrackLabel");
        if (_backButton == null)      _backButton      = FindInChildren<Button>("BackBtn");

        Debug.Log($"[Settings] Init: bgmSlider={_bgmSlider != null}, sfxSlider={_sfxSlider != null}, " +
                  $"bgmText={_bgmValueText != null}, sfxText={_sfxValueText != null}, " +
                  $"mute={_muteButton != null}, switch={_bgmSwitchButton != null}, back={_backButton != null}");

        float savedBgm = PlayerPrefs.GetFloat(PREF_BGM, 0.5f);
        float savedSfx = PlayerPrefs.GetFloat(PREF_SFX, 0.5f);

        // BGM slider setup
        if (_bgmSlider != null)
        {
            _bgmSlider.minValue = 0f;
            _bgmSlider.maxValue = 1f;
            _bgmSlider.value = savedBgm;
            _bgmSlider.onValueChanged.AddListener(OnBgmChanged);
        }

        // SFX slider setup
        if (_sfxSlider != null)
        {
            _sfxSlider.minValue = 0f;
            _sfxSlider.maxValue = 1f;
            _sfxSlider.value = savedSfx;
            _sfxSlider.onValueChanged.AddListener(OnSfxChanged);
        }

        // Mute toggle
        _muteButton?.onClick.AddListener(ToggleMute);

        // Switch BGM track
        _bgmSwitchButton?.onClick.AddListener(() =>
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.NextTrack();
                UpdateTrackLabel();
            }
        });

        // Back button
        _backButton?.onClick.AddListener(() =>
        {
            SaveSettings();
            gameObject.SetActive(false);
        });

        // Apply initial state
        ApplyVolume(savedBgm, savedSfx);
        UpdateValueTexts();
        UpdateTrackLabel();

        Debug.Log("[UI] SettingsPanel initialized");
    }

    private void ToggleMute()
    {
        _isMuted = !_isMuted;

        if (_isMuted)
        {
            _preMuteBgm = _bgmSlider != null ? _bgmSlider.value : 0.5f;
            _preMuteSfx = _sfxSlider != null ? _sfxSlider.value : 0.5f;
            if (_bgmSlider != null) _bgmSlider.value = 0f;
            if (_sfxSlider != null) _sfxSlider.value = 0f;

            Text btnText = _muteButton?.GetComponentInChildren<Text>();
            if (btnText != null) btnText.text = "UNMUTE";
        }
        else
        {
            if (_bgmSlider != null) _bgmSlider.value = _preMuteBgm;
            if (_sfxSlider != null) _sfxSlider.value = _preMuteSfx;

            Text btnText = _muteButton?.GetComponentInChildren<Text>();
            if (btnText != null) btnText.text = "MUTE ALL";
        }
    }

    private void OnBgmChanged(float val)
    {
        UpdateValueTexts();
        ApplyVolume(val, _sfxSlider != null ? _sfxSlider.value : 0.5f);
    }

    private void OnSfxChanged(float val)
    {
        UpdateValueTexts();
        ApplyVolume(_bgmSlider != null ? _bgmSlider.value : 0.5f, val);
    }

    private void UpdateValueTexts()
    {
        if (_bgmValueText != null && _bgmSlider != null)
            _bgmValueText.text = Mathf.RoundToInt(_bgmSlider.value * 100) + "%";
        if (_sfxValueText != null && _sfxSlider != null)
            _sfxValueText.text = Mathf.RoundToInt(_sfxSlider.value * 100) + "%";
    }

    private void UpdateTrackLabel()
    {
        if (_trackLabel != null && AudioManager.Instance != null)
            _trackLabel.text = $"Now Playing: {AudioManager.Instance.CurrentTrackName}";
    }

    private void ApplyVolume(float bgm, float sfx)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetVolume(bgm, sfx);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat(PREF_BGM, _bgmSlider != null ? _bgmSlider.value : 0.5f);
        PlayerPrefs.SetFloat(PREF_SFX, _sfxSlider != null ? _sfxSlider.value : 0.5f);
        PlayerPrefs.Save();
    }

    /// <summary>Finds a child component by name, including inactive objects.</summary>
    private T FindInChildren<T>(string name) where T : Component
    {
        Transform t = transform.Find(name);
        return t != null ? t.GetComponent<T>() : null;
    }
}
