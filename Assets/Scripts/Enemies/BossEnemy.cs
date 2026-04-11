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
    private const int   BOSS_MAX_HP       = 45;
    private const float PHASE1_SPEED      = 1.0f;
    private const float PHASE2_SPEED      = 1.8f;
    private const float PHASE3_SPEED      = 3.0f;
    private const float PHASE1_FIRE_RATE  = 2.0f;
    private const float PHASE2_FIRE_RATE  = 1.0f;
    private const float PHASE3_FIRE_RATE  = 0.7f;

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

        // Boss is slightly smaller than before (was 2x, now 1.7x)
        transform.localScale = Vector3.one * 1.7f;

        SetupVisuals();
        _fireTimer = PHASE1_FIRE_RATE;

        // Initialize BossHUD
        BossHUDPanel.UpdateHealth(Health, _maxHealth);
        BossHUDPanel.SetPhaseName("Phase 1  Hunting");
        Debug.Log("[BossEnemy] Initialized. Sent Health and Phase updates to BossHUDPanel.");
    }

    protected override void SetupVisuals()
    {
        if (_sr != null)
            _sr.color = BOSS_COLOR;
    }

    protected override void HandleMovement()
    {
        if (_player == null || _rb == null) return;

        Vector2 toPlayer  = (Vector2)_player.position - (Vector2)transform.position;
        Vector2 seekDir   = toPlayer.normalized;
        int     wallMask  = LayerMask.GetMask("Default");

        // ── Line-of-Sight check ────────────────────────────────────────────
        // Cast directly toward the player. If nothing blocks us, chase directly.
        bool hasLOS = true;
        RaycastHit2D losHit = Physics2D.Raycast(transform.position, seekDir, toPlayer.magnitude, wallMask);
        if (losHit.collider != null && losHit.collider.CompareTag("Wall"))
            hasLOS = false;

        Vector2 moveDir;

        if (hasLOS)
        {
            // Direct chase — player is visible
            moveDir      = seekDir;
            _wanderTimer = 0f; // Reset wander so we re-evaluate immediately when LOS is lost
        }
        else
        {
            // ── Open-Space Wandering ────────────────────────────────────────
            // Re-evaluate best open direction periodically (not every frame)
            _wanderTimer -= Time.deltaTime;
            if (_wanderTimer <= 0f)
            {
                _wanderDir   = FindBestOpenDirection(seekDir, wallMask);
                _wanderTimer = 0.55f; // Re-evaluate every 0.55 s
            }
            moveDir = _wanderDir;
        }

        // ── Smooth direction transition ────────────────────────────────────
        // Rotate current movement direction toward target at a fixed turn rate.
        // This prevents snap-turning when swapping between chase and wander.
        float turnSpeed = hasLOS ? 5.0f : 3.0f; // Chase turns tighter than wander
        _currentMoveDir = Vector2.MoveTowards(_currentMoveDir, moveDir, turnSpeed * Time.deltaTime).normalized;

        _rb.velocity = _currentMoveDir * Speed;

        if (_currentMoveDir.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(_currentMoveDir.y, _currentMoveDir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }

    // Movement state
    private float   _wanderTimer   = 0f;
    private Vector2 _wanderDir     = Vector2.up;
    private Vector2 _currentMoveDir= Vector2.up; // Actual interpolated direction

    /// <summary>
    /// Sample 8 equally-spaced directions and score each by:
    ///   score = clearDistance × max(0, dot(dir, toPlayer))
    /// This finds a direction that has open space AND is generally toward the player.
    /// </summary>
    private Vector2 FindBestOpenDirection(Vector2 toPlayerDir, int wallMask)
    {
        Vector2 bestDir   = _wanderDir; // Keep last direction as fallback
        float   bestScore = -1f;
        float   probeLen  = 4f; // How far ahead to look per candidate direction

        for (int i = 0; i < 8; i++)
        {
            float   angle    = i * 45f;
            Vector2 candidate = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad));

            RaycastHit2D hit  = Physics2D.Raycast(transform.position, candidate, probeLen, wallMask);
            float clearDist   = (hit.collider != null) ? hit.distance : probeLen;

            // Dot product with toPlayer: prefer directions that move generally toward player
            float alignment   = Mathf.Max(0f, Vector2.Dot(candidate, toPlayerDir));

            // Score blends openness (70%) and player alignment (30%)
            float score = clearDist * (0.7f + alignment * 0.3f);

            if (score > bestScore)
            {
                bestScore = score;
                bestDir   = candidate;
            }
        }

        Debug.Log($"[Boss] LOS blocked — wandering toward {bestDir} (score:{bestScore:F2})");
        return bestDir;
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
        Debug.Log($"[Boss] Phase -> {phase}  HP={Health}/{_maxHealth}  Speed={Speed}");
    }

    // Shooting
    private void FireAtPlayer()
    {
        Vector2 targetPos = _player.position;
        Vector2 dirToPlayer = (targetPos - (Vector2)transform.position).normalized;
        
        // Aiming check: boss's 'up' direction (where it faces) must be aligned with dirToPlayer
        // transform.up is the top of the sprite (default facing direction in top-down 2D)
        if (Vector2.Dot(transform.up, dirToPlayer) < 0.9f) // ~25 deg tolerance
        {
            // Not facing player yet, skip firing this tick
            return;
        }

        AudioManager.Instance?.PlayShootSound();

        if (_currentPhase >= 2)
        {
            // Spread shot
            float spread = _currentPhase >= 3 ? 30f : 18f;
            FireBulletAt((Vector2)transform.position + Rotate(dirToPlayer, -spread), BOSS_COLOR);
            FireBulletAt(targetPos, BOSS_COLOR);
            FireBulletAt((Vector2)transform.position + Rotate(dirToPlayer, spread), BOSS_COLOR);
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
        // Boss-specific cleanup
        BossHUDPanel.UpdateHealth(0, _maxHealth);
        BossHUDPanel.Show(false);

        // Extra boss explosion VFX (larger particle count)
        VfxManager.SpawnExplosion(transform.position, BOSS_COLOR, 20);

        // Delegate to base for score, spawner notification, standard VFX, and destruction
        base.Die();
    }
}
