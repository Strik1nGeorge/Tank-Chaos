using System;
using UnityEngine;

/// <summary>
/// Player health system: 5 HP per life, 3 lives, with invincibility frames after taking damage.
/// Fires events for the HUD / UI to react to.
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    // ── Constants ──
    public  const int   MAX_HP          = 5;
    public  const int   MAX_LIVES       = 3;
    private const float INVINCIBLE_TIME = 1.5f;  // seconds of i-frames

    // ── State ──
    public int  CurrentHP    { get; private set; }
    public int  Lives        { get; private set; }
    public bool IsInvincible { get; private set; }

    private float _invincibleTimer;

    // ── Events (consumed by HUDManager) ──
    public event Action<int, int> OnHPChanged;    // (currentHP, maxHP)
    public event Action<int>      OnLivesChanged; // (lives)
    public event Action           OnPlayerDied;   // all lives lost
    public event Action           OnPlayerRespawn;// respawned with new life

    private SpriteRenderer _sr;
    private Color _originalColor = Color.white;

    private void Awake()
    {
        CurrentHP = MAX_HP;
        Lives     = MAX_LIVES;
        _sr = GetComponentInChildren<SpriteRenderer>();
        if (_sr != null) _originalColor = _sr.color;
    }

    private void Update()
    {
        if (IsInvincible)
        {
            _invincibleTimer -= Time.deltaTime;
            if (_invincibleTimer <= 0f)
            {
                IsInvincible = false;
                if (_sr != null) _sr.color = _originalColor; // Restore color
            }
        }
    }

    // ── Public API ──

    /// <summary>Fully resets HP and lives.</summary>
    public void ResetHealth()
    {
        CurrentHP    = MAX_HP;
        Lives        = MAX_LIVES;
        IsInvincible = false;
        _invincibleTimer = 0f;

        OnHPChanged?.Invoke(CurrentHP, MAX_HP);
        OnLivesChanged?.Invoke(Lives);
        Debug.Log("[PlayerHealth] Health fully reset.");
    }

    /// <summary>Applies damage (ignored while invincible).</summary>
    public void TakeDamage(int amount)
    {
        if (IsInvincible) return;
        if (amount <= 0)  return;

        CurrentHP -= amount;
        CurrentHP  = Mathf.Max(0, CurrentHP);

        VfxManager.SpawnFloatingText(transform.position, "-" + amount.ToString(), Color.red);

        OnHPChanged?.Invoke(CurrentHP, MAX_HP);

        if (CurrentHP <= 0)
            HandleDeath();
        else
            StartInvincibility();
    }

    /// <summary>Heals HP, clamped to MAX_HP.</summary>
    public void Heal(int amount)
    {
        if (amount <= 0) return;
        CurrentHP = Mathf.Min(MAX_HP, CurrentHP + amount);

        VfxManager.SpawnFloatingText(transform.position, "+" + amount.ToString(), Color.green);

        OnHPChanged?.Invoke(CurrentHP, MAX_HP);
    }

    /// <summary>Restores full HP and moves player to spawn point.</summary>
    public void RefreshHealth()
    {
        CurrentHP = MAX_HP;
        transform.position = PlayerSpawnPoint.Position;
        OnHPChanged?.Invoke(CurrentHP, MAX_HP);
        StartInvincibility();
    }

    /// <summary>Grants temporary invincibility (Shield buff).</summary>
    public void GrantShield(float duration)
    {
        IsInvincible = true;
        _invincibleTimer = Mathf.Max(_invincibleTimer, duration);
        if (_sr != null) _sr.color = Color.yellow; // Visual feedback
        VfxManager.SpawnFloatingText(transform.position, "SHIELD UP!", Color.yellow);
        Debug.Log($"[PlayerHealth] Granted shield for {duration}s");
    }

    // ── Internal ──
    private void HandleDeath()
    {
        Lives--;
        OnLivesChanged?.Invoke(Lives);

        if (Lives <= 0)
        {
            // Game over
            OnPlayerDied?.Invoke();
            gameObject.SetActive(false);
        }
        else
        {
            // Still has lives — respawn
            Respawn();
        }
    }

    private void Respawn()
    {
        CurrentHP = MAX_HP;

        // Return to the spawn point set by GameBootstrapper
        Vector3 spawnPos = PlayerSpawnPoint.Position;
        transform.position = spawnPos;

        StartInvincibility();
        OnHPChanged?.Invoke(CurrentHP, MAX_HP);
        OnPlayerRespawn?.Invoke();

        Debug.Log($"[PlayerHealth] Respawned @ {spawnPos}, Lives remaining: {Lives}");
    }

    private void StartInvincibility()
    {
        IsInvincible     = true;
        _invincibleTimer = INVINCIBLE_TIME;
    }
}

/// <summary>
/// Static spawn-point holder set by GameBootstrapper so PlayerHealth
/// can respawn without an Inspector reference.
/// </summary>
public static class PlayerSpawnPoint
{
    public static Vector3 Position { get; set; } = Vector3.zero;
}
