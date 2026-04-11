using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for all enemy types.
/// Stats are initialized in each subclass's Awake. AI behavior is delegated to EnemyAI.
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    // Stats ? initialized in subclass Awake
    public int   Health     { get; protected set; }
    public float Speed      { get; protected set; }
    public int   Damage     { get; protected set; }
    public int   ScoreValue { get; protected set; }

    // Cached component references
    protected Rigidbody2D    _rb;
    protected SpriteRenderer _sr;
    protected EnemyAI        _ai;       // AI controller
    protected Transform      _player;   // Player transform

    // Unity lifecycle
    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _ai = GetComponent<EnemyAI>();

        // Configure Rigidbody2D for top-down physics
        _rb.gravityScale    = 0f;
        _rb.freezeRotation  = true;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Find the player
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) _player = pObj.transform;
    }

    protected virtual void Update()
    {
        if (_player == null) return;
        HandleAction();
    }

    protected virtual void FixedUpdate()
    {
        if (_player == null) return;
        HandleMovement();
    }

    // Abstract methods for subclasses

    /// <summary>Movement logic, driven by EnemyAI.</summary>
    protected abstract void HandleMovement();
    
    /// <summary>Action logic (e.g., shooting).</summary>
    protected abstract void HandleAction();

    /// <summary>Configure sprite appearance.</summary>
    protected abstract void SetupVisuals();

    // Public API

    /// <summary>Multiply health for armored variant (Level 4).</summary>
    public void SetArmorMultiplier(int multiplier)
    {
        Health *= multiplier;
        Debug.Log($"[{name}] Armored! HP={Health}");
    }

    protected bool _isDead = false;

    public virtual void TakeDamage(int amount)
    {
        if (_isDead) return;

        Health -= amount;
        StartCoroutine(FlashWhite());

        VfxManager.SpawnFloatingText(transform.position, "-" + amount.ToString(), Color.red);

        if (Health <= 0)
        {
            _isDead = true;
            Die();
        }
    }

    protected virtual void Die()
    {
        // Award score
        GameManager.Instance?.AddScore(ScoreValue);
        
        VfxManager.SpawnFloatingText(transform.position + Vector3.up * 0.5f, "+" + ScoreValue.ToString(), Color.yellow, 1.5f, 2.0f);

        // Try to drop an item
        PickupSpawner.Instance?.TryDropPickup(transform.position);

        // Notify spawner
        EnemySpawner.Instance?.OnEnemyDied(this);

        // Explosion VFX and SFX
        VfxManager.SpawnExplosion(transform.position, _sr.color, 12);
        AudioManager.Instance?.PlayExplosionSound();

        Destroy(gameObject);
    }

    // Flash sprite white on hit
    private IEnumerator FlashWhite()
    {
        if (_sr == null) yield break;
        Color orig = _sr.color;
        _sr.color = Color.white;
        yield return new WaitForSeconds(0.1f);
        _sr.color = orig;
    }

    // Fires a bullet toward the target position
    protected void FireBulletAt(Vector2 targetPos, Color bulletColor)
    {
        GameObject bullet = BulletPool.Instance?.Get();
        if (bullet != null)
        {
            Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
            
            // Adjust the spawn offset based on the enemy's scale to ensure it spawns at the barrel tip
            float spawnOffset = 0.65f * transform.localScale.x;
            bullet.transform.position = transform.position + (Vector3)(dir * spawnOffset);
            
            Bullet b = bullet.GetComponent<Bullet>();
            b?.Init(dir, 8f, Damage, isPlayer: false); // isPlayer = false
            
            bullet.GetComponent<SpriteRenderer>().color = bulletColor;

            // Muzzle flash VFX
            VfxManager.SpawnMuzzleFlash(bullet.transform.position, transform.rotation);
        }
    }
}
