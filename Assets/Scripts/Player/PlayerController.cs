using UnityEngine;
using UnityEngine.EventSystems;



/// <summary>
/// Player tank controller: movement (WASD), sprint (Left Shift), aiming, and firing.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerHealth))]
public class PlayerController : MonoBehaviour
{
    // ── Constants ──
    private const float BASE_SPEED       = 4f;
    private const float SPRINT_MULT      = 2.2f;
    private const float SPRINT_COOLDOWN  = 1.5f;
    private const float SPRINT_DURATION  = 0.4f;
    private const float FIRE_COOLDOWN    = 1.0f;
    private const float SHOTGUN_COOLDOWN = 0.8f;
    private const float BULLET_SPEED     = 10f;
    private const float SHOTGUN_SPEED    = 12f;
    public  const float WEAPON_TOGGLE_CD = 3.0f;
    public enum WeaponType { Cannon, Shotgun }

    // ── Cached references ──
    private Rigidbody2D  _rb;
    private PlayerHealth _health;
    private SpriteRenderer _sr;
    private Color _baseBodyColor;

    // ── Timers ──
    private float _fireTimer;
    private float _sprintCooldownTimer;
    private float _sprintDurationTimer;
    private bool  _isSprinting;
    private float _weaponToggleTimer;

    // Current weapon type
    public WeaponType CurrentWeapon { get; private set; } = WeaponType.Cannon;

    // HUD cooldown progress (0 = ready, 1 = just used)
    public float DashCooldownProgress
    {
        get
        {
            if (_isSprinting) return 1f;
            return _sprintCooldownTimer > 0 ? _sprintCooldownTimer / SPRINT_COOLDOWN : 0f;
        }
    }
    public float WeaponToggleProgress => _weaponToggleTimer > 0 ? _weaponToggleTimer / WEAPON_TOGGLE_CD : 0f;

    public  int  DamageMultiplier { get; set; } = 1;
    private float _damageBoostTimer;

    // Prevents firing when clicking on UI
    private bool  _wasPointerOverUI;

    // ── Unity lifecycle ──
    private void Awake()
    {
        _rb     = GetComponent<Rigidbody2D>();
        _health = GetComponent<PlayerHealth>();
        _sr     = GetComponent<SpriteRenderer>();

        // Top-down 2D: no gravity, no Z rotation, continuous collision
        _rb.gravityScale    = 0f;
        _rb.freezeRotation  = true;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        _baseBodyColor = SkinManager.GetBodyColor();
    }

    // ── Public API ──
    public void ApplySkin()
    {
        _baseBodyColor = SkinManager.GetBodyColor();
        
        Transform barrel = transform.Find("Barrel");
        if (barrel != null) barrel.GetComponent<SpriteRenderer>().color = SkinManager.GetBarrelColor();

        Transform trackL = transform.Find("TrackL");
        if (trackL != null) trackL.GetComponent<SpriteRenderer>().color = SkinManager.GetTrackColor();

        Transform trackR = transform.Find("TrackR");
        if (trackR != null) trackR.GetComponent<SpriteRenderer>().color = SkinManager.GetTrackColor();
    }

    private void Update()
    {
        // Skip input while paused
        if (Time.timeScale == 0f) return;

        HandleTimers();
        HandleSprint();
        HandleWeaponToggle();
        HandleFire();
        RotateTowardsMouse();
        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    // ── Movement ──
    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 dir = new Vector2(h, v).normalized;
        float   spd = _isSprinting ? BASE_SPEED * SPRINT_MULT : BASE_SPEED;

        _rb.velocity = dir * spd;
    }

    // ── Sprint ──
    private void HandleSprint()
    {
        if (_isSprinting)
        {
            if (_sprintDurationTimer <= 0f)
            {
                _isSprinting          = false;
                _sprintCooldownTimer  = SPRINT_COOLDOWN;
            }
            return;
        }

        bool canSprint = _sprintCooldownTimer <= 0f;
        if (canSprint && Input.GetKeyDown(KeyCode.LeftShift))
        {
            _isSprinting          = true;
            _sprintDurationTimer  = SPRINT_DURATION;
        }
    }

    // ── Weapon toggle ──
    private void HandleWeaponToggle()
    {
        // Still on cooldown
        if (_weaponToggleTimer > 0f) return;

        if (Input.GetMouseButtonDown(1)) // Right-click
        {
            CurrentWeapon = CurrentWeapon == WeaponType.Cannon ? WeaponType.Shotgun : WeaponType.Cannon;
            _weaponToggleTimer = WEAPON_TOGGLE_CD; // Start cooldown
        }
    }

    // ── Firing ──
    private void HandleFire()
    {
        // Ignore clicks on UI elements
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            _wasPointerOverUI = true;
            return;
        }

        if (_wasPointerOverUI)
        {
            if (!Input.GetMouseButton(0))
            {
                _wasPointerOverUI = false;
            }
            return;
        }

        if (_fireTimer > 0f) return;
        if (!Input.GetMouseButton(0)) return;

        // Calculate fire direction toward mouse
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - transform.position).normalized;

        // Fire based on current weapon
        if (CurrentWeapon == WeaponType.Cannon)
        {
            FireSingleBullet(dir);
            _fireTimer = FIRE_COOLDOWN;
        }
        else
        {
            FireShotgun(dir);
            _fireTimer = SHOTGUN_COOLDOWN;
        }
    }

    private void FireSingleBullet(Vector2 dir)
    {
        GameObject bullet = BulletPool.Instance?.Get();
        if (bullet != null)
        {
            bullet.transform.position = transform.position + (Vector3)(dir * 0.6f);
            bullet.transform.rotation = Quaternion.identity;

            Bullet b = bullet.GetComponent<Bullet>();
            b?.Init(dir, BULLET_SPEED, DamageMultiplier, isPlayer: true);

            VfxManager.SpawnMuzzleFlash(bullet.transform.position, transform.rotation);
            AudioManager.Instance?.PlayShootSound();
        }
    }

    private void FireShotgun(Vector2 dir)
    {
        float[] spreadAngles = { -15f, 0f, 15f };
        
        foreach (float angleOffset in spreadAngles)
        {
            GameObject bullet = BulletPool.Instance?.Get();
            if (bullet != null)
            {
                bullet.transform.position = transform.position + (Vector3)(dir * 0.6f);
                bullet.transform.rotation = Quaternion.identity;

                // Calculate spread direction
                float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                float finalAngle = baseAngle + angleOffset;
                Vector2 spreadDir = new Vector2(Mathf.Cos(finalAngle * Mathf.Deg2Rad), Mathf.Sin(finalAngle * Mathf.Deg2Rad));

                Bullet b = bullet.GetComponent<Bullet>();
                // Shotgun pellets last 0.3s
                b?.Init(spreadDir, SHOTGUN_SPEED, DamageMultiplier, isPlayer: true, 0.3f, isShotgun: true);
            }
        }

        Vector3 spawnPos = transform.position + (Vector3)(dir * 0.6f);
        VfxManager.SpawnMuzzleFlash(spawnPos, transform.rotation);
        // Extra muzzle flashes for spread effect
        VfxManager.SpawnMuzzleFlash(spawnPos, transform.rotation * Quaternion.Euler(0, 0, 15f));
        VfxManager.SpawnMuzzleFlash(spawnPos, transform.rotation * Quaternion.Euler(0, 0, -15f));
        
        AudioManager.Instance?.PlayShootSound();
    }

    // ── Aiming ──
    private void RotateTowardsMouse()
    {
        // Skip while paused
        if (Time.timeScale == 0f) return;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;
        Vector2 dir = (mouseWorld - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // ── Visuals ──
    private void UpdateVisuals()
    {
        if (_sr == null) return;

        // Default body color
        Color targetColor = _baseBodyColor;

        // Flash orange while damage boost is active
        if (DamageMultiplier > 1)
        {
            if (Mathf.Sin(Time.time * 25f) > 0)
            {
                targetColor = Color.Lerp(_baseBodyColor, new Color(1f, 0.4f, 0f), 0.7f);
            }
        }

        // Flicker alpha while invincible
        if (_health != null && _health.IsInvincible)
        {
            targetColor.a = (Mathf.Sin(Time.time * 35f) > 0) ? 0.2f : 0.8f;
        }
        else
        {
            targetColor.a = 1f;
        }

        _sr.color = targetColor;
    }

    // ── Timer management ──
    private void HandleTimers()
    {
        if (_fireTimer > 0f) _fireTimer -= Time.deltaTime;
        if (_sprintCooldownTimer > 0f) _sprintCooldownTimer -= Time.deltaTime;
        if (_sprintDurationTimer > 0f) _sprintDurationTimer -= Time.deltaTime;
        if (_weaponToggleTimer > 0f) _weaponToggleTimer -= Time.deltaTime;
        if (_damageBoostTimer > 0f)
        {
            _damageBoostTimer -= Time.deltaTime;
            if (_damageBoostTimer <= 0f)
            {
                DamageMultiplier = 1;
                Debug.Log("[PlayerController] Damage boost ended");
            }
        }
    }

    // ── Damage boost API ──

    /// <summary>Doubles damage for the given duration in seconds.</summary>
    public void ActivateDamageBoost(float duration)
    {
        DamageMultiplier  = 2;
        _damageBoostTimer = duration;
        Debug.Log($"[PlayerController] ?{DamageMultiplier}?{duration}s");
    }
}
