using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD panel displaying health, time, score, and skill cooldowns.
/// References are set via Inspector or resolved at runtime as fallback.
/// </summary>
public class HUDPanel : MonoBehaviour
{
    [Header(" HUD (Inspector ?")]
    [SerializeField] private Text _healthText;
    [SerializeField] private Text _timeText;
    [SerializeField] private Text _scoreText;

    [Header(" (Inspector ?")]
    [SerializeField] private Image _dashMask;
    [SerializeField] private Text  _dashTimerText;
    [SerializeField] private Text  _weaponLabel;
    [SerializeField] private Image _weaponMask;
    [SerializeField] private Text  _weaponTimerText;

    private PlayerHealth     _playerHealth;
    private PlayerController _playerCtrl;

    public void Init()
    {
        // Auto-bind fallback via hierarchy path
        if (_healthText == null) _healthText = FindText("TopBar/HealthText");
        if (_timeText == null)   _timeText   = FindText("TopBar/TimeText");
        if (_scoreText == null)  _scoreText  = FindText("TopBar/ScoreText");

        if (_dashMask == null)       _dashMask       = FindImage("SkillContainer/DashBorder/DashBg/DashMask");
        if (_dashTimerText == null)   _dashTimerText   = FindText("SkillContainer/DashBorder/DashBg/DashMask/DashTimerText");
        if (_weaponLabel == null)    _weaponLabel    = FindText("SkillContainer/WeaponBorder/WeaponBg/WeaponLabel");
        if (_weaponMask == null)     _weaponMask     = FindImage("SkillContainer/WeaponBorder/WeaponBg/WeaponMask");
        if (_weaponTimerText == null) _weaponTimerText = FindText("SkillContainer/WeaponBorder/WeaponBg/WeaponMask/WeaponTimerText");

        Debug.Log("[UI] HUDPanel initialized with SerializeField bindings");
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // Try to find the player
        if (_playerHealth == null)
        {
            GameObject pObj = GameObject.FindGameObjectWithTag("Player");
            if (pObj != null)
            {
                _playerHealth = pObj.GetComponent<PlayerHealth>();
                _playerCtrl = pObj.GetComponent<PlayerController>();
            }
        }

        // Health display
        if (_playerHealth != null)
        {
            _healthText.text = $"LIVES: {_playerHealth.Lives} | HP: {_playerHealth.CurrentHP}";
        }
        else
        {
            _healthText.text = "LIVES: 0 | HP: 0";
        }

        // Timer display
        float timerDisplay;
        if (GameManager.Instance.CurrentConfig != null && GameManager.Instance.CurrentConfig.TimeLimit > 0)
        {
            // Countdown mode
            timerDisplay = Mathf.Max(0, GameManager.Instance.TimeRemaining);
        }
        else
        {
            // Elapsed time mode
            timerDisplay = GameManager.Instance.TotalClearTime;
        }

        int m = Mathf.FloorToInt(timerDisplay / 60f);
        int s = Mathf.FloorToInt(timerDisplay % 60f);
        _timeText.text = $"{m:00}:{s:00}";

        // Score display
        _scoreText.text = $"SCORE: {GameManager.Instance.CurrentScore}";

        // Skill cooldown icons
        if (_playerCtrl != null)
        {
            // Dash
            float dProg = _playerCtrl.DashCooldownProgress;
            _dashMask.gameObject.SetActive(dProg > 0f);
            if (dProg > 0f)
            {
                _dashMask.fillAmount = dProg;
                _dashTimerText.text = (dProg * 1.5f).ToString("F1");
            }

            // Weapon Toggle Cooldown
            bool isCannon = _playerCtrl.CurrentWeapon == PlayerController.WeaponType.Cannon;
            _weaponLabel.text = isCannon ? "CANNON" : "SHOTGUN";
            _weaponLabel.color = isCannon ? Color.yellow : new Color(1f, 0.4f, 0f);

            float wProg = _playerCtrl.WeaponToggleProgress;
            _weaponMask.gameObject.SetActive(wProg > 0f);
            if (wProg > 0f)
            {
                _weaponMask.fillAmount = wProg;
                _weaponTimerText.text = (wProg * PlayerController.WEAPON_TOGGLE_CD).ToString("F1");
            }
        }
    }

    private Text FindText(string path)
    {
        Transform t = transform.Find(path);
        return t != null ? t.GetComponent<Text>() : null;
    }

    private Image FindImage(string path)
    {
        Transform t = transform.Find(path);
        return t != null ? t.GetComponent<Image>() : null;
    }
}
