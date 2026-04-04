using UnityEngine;

/// <summary>
/// BossEnemy - Level 5 boss.
/// Has independent AI with three phases:
/// Phase 1 (HP > 66%): Chase + single shot
/// Phase 2 (HP > 33%): Speed up + spread shot
/// Phase 3 (HP &lt;= 33%): Rampage + fast spread shot
/// Spawned by EnemySpawner; health bar shown by BossHUDPanel.
/// </summary>
public class BossEnemy : EnemyBase
{
    // Boss constants
    private const int   BOSS_MAX_HP       = 60;
    private const float PHASE1_SPEED      = 1.5f;
    private const float PHASE2_SPEED      = 2.8f;
    private const float PHASE3_SPEED      = 4.5f;
    private const float PHASE1_FIRE_RATE  = 2.0f;
    private const float PHASE2_FIRE_RATE  = 1.0f;
    private const float PHASE3_FIRE_RATE  = 0.4f;

    private int   _maxHealth;
    private int   _currentPhase = 0; // 0 = not initialized
    private float _fireTimer;
    private static readonly Color BOSS_COLOR = new Color(0.6f, 0.05f, 0.05f); // Dark red

    // Initialization
    protected override void Awake()
    {
        base.Awake();

        Health     = BOSS_MAX_HP;
        _maxHealth = BOSS_MAX_HP;
        Speed      = PHASE1_SPEED;
        Damage     = 2;
        ScoreValue = 200;

        // Boss is 2x scale
        transform.localScale = Vector3.one * 2f;

        SetupVisuals();
        _fireTimer = PHASE1_FIRE_RATE;

        // Initialize BossHUD
        BossHUDPanel.UpdateHealth(Health, _maxHealth);
        BossHUDPanel.SetPhaseName("Phase 1  Hunting");
    }

    protected override void SetupVisuals()
    {
        if (_sr != null)
            _sr.color = BOSS_COLOR;
    }

    protected override void HandleMovement()
    {
        if (_player == null || _rb == null) return;

        Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        _rb.velocity = dir * Speed;
    }

    protected override void HandleAction()
    {
        if (_player == null) return;

        _fireTimer -= Time.deltaTime;
        if (_fireTimer <= 0f)
        {
            FireAtPlayer();
            _fireTimer = GetFireRate();
        }

        // Check for phase transition
        UpdatePhase();
    }

    // Phase management
    private void UpdatePhase()
    {
        float hpRatio = (float)Health / _maxHealth;
        int newPhase;

        if      (hpRatio > 0.66f) newPhase = 1;
        else if (hpRatio > 0.33f) newPhase = 2;
        else                      newPhase = 3;

        if (newPhase != _currentPhase)
        {
            _currentPhase = newPhase;
            OnPhaseChanged(newPhase);
        }
    }

    private void OnPhaseChanged(int phase)
    {
        switch (phase)
        {
            case 1:
                Speed = PHASE1_SPEED;
                BossHUDPanel.SetPhaseName("Phase 1  Hunting");
                break;
            case 2:
                Speed = PHASE2_SPEED;
                BossHUDPanel.SetPhaseName("Phase 2  Berserker");
                // Phase transition VFX
                VfxManager.SpawnFloatingText(transform.position, "PHASE 2!", new Color(1f,0.5f,0f), 2f, 2f);
                break;
            case 3:
                Speed = PHASE3_SPEED;
                BossHUDPanel.SetPhaseName("Phase 3  RAMPAGE");
                VfxManager.SpawnFloatingText(transform.position, "FINAL PHASE!", Color.red, 2.5f, 2f);
                break;
        }

        // Notify spawner of phase change
        EnemySpawner.Instance?.OnBossPhaseChanged(phase);
        Debug.Log($"[Boss] Phase ?{phase}  HP={Health}/{_maxHealth}  Speed={Speed}");
    }

    // Shooting
    private void FireAtPlayer()
    {
        Vector2 targetPos = _player.position;
        AudioManager.Instance?.PlayShootSound();

        if (_currentPhase >= 2)
        {
            // Spread shot
            float spread = _currentPhase >= 3 ? 30f : 18f;
            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            FireBulletAt((Vector2)transform.position + Rotate(dir, -spread), BOSS_COLOR);
            FireBulletAt(targetPos, BOSS_COLOR);
            FireBulletAt((Vector2)transform.position + Rotate(dir, spread), BOSS_COLOR);
        }
        else
        {
            FireBulletAt(targetPos, BOSS_COLOR);
        }
    }

    private float GetFireRate()
    {
        switch (_currentPhase)
        {
            case 3: return PHASE3_FIRE_RATE;
            case 2: return PHASE2_FIRE_RATE;
            default: return PHASE1_FIRE_RATE;
        }
    }

    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
    }

    // Damage / Death overrides
    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        // Update HUD after taking damage
        BossHUDPanel.UpdateHealth(Mathf.Max(0, Health), _maxHealth);
    }

    protected override void Die()
    {
        BossHUDPanel.UpdateHealth(0, _maxHealth);
        BossHUDPanel.Show(false);

        GameManager.Instance?.AddScore(ScoreValue);
        VfxManager.SpawnFloatingText(transform.position + Vector3.up, "+200", Color.yellow, 2f, 2f);
        VfxManager.SpawnExplosion(transform.position, BOSS_COLOR, 30);
        AudioManager.Instance?.PlayExplosionSound();

        EnemySpawner.Instance?.OnEnemyDied(this);
        Destroy(gameObject);
    }
}
