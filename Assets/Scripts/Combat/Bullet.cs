using UnityEngine;

/// <summary>
/// Bullet behaviour. Initialized via BulletPool's Init() after spawning.
/// Handles movement, lifetime, and collision with walls/enemies/player.
/// </summary>
public class Bullet : MonoBehaviour
{
    // --- Constants ---
    private const float DEFAULT_LIFETIME = 3f; 

    // --- Instance State ---
    private Vector2 _direction;
    private float   _speed;
    private int     _damage;
    private bool    _isPlayer;
    private float   _lifetimeTimer;

    private Rigidbody2D    _rb;
    private SpriteRenderer _sr;
    private Vector3        _originalScale;

    // --- Unity Lifecycle ---
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _originalScale = transform.localScale;
    }

    private void Update()
    {
        _lifetimeTimer -= Time.deltaTime;
        if (_lifetimeTimer <= 0f)
            ReturnToPool();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Wall collision
        if (other.CompareTag("Wall"))
        {
            // Check for destructible wall
            DestructibleWall wall = other.GetComponent<DestructibleWall>();
            if (wall != null)
            {
                wall.TakeDamage(_damage);
                AudioManager.Instance?.PlayExplosionSound();
                VfxManager.SpawnExplosion(transform.position, Color.gray, 5);
                // Optional camera shake (disabled)
                // CameraShake.TriggerShake(0.1f, 0.1f);
            }
            ReturnToPool();
            return;
        }

        if (_isPlayer)
        {
            // Player bullet hits enemy
            EnemyBase enemy = other.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(_damage);
                AudioManager.Instance?.PlayExplosionSound();
                VfxManager.SpawnExplosion(enemy.transform.position, new Color(1f, 0.3f, 0.3f), 8);
                ReturnToPool();
            }
        }
        else
        {
            // Enemy bullet hits player
            if (other.CompareTag("Player"))
            {
                PlayerHealth ph = other.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(_damage);
                    AudioManager.Instance?.PlayExplosionSound();
                    VfxManager.SpawnExplosion(ph.transform.position, new Color(0.3f, 0.7f, 1f), 10);
                    CameraShake.TriggerShake(0.3f, 0.2f);
                    ReturnToPool();
                }
            }
        }
    }

    // --- Public API ---

    /// <summary>
    /// Initialize bullet with direction, speed, damage, and ownership.
    /// </summary>
    public void Init(Vector2 direction, float speed, int damage, bool isPlayer, float maxLifetime = DEFAULT_LIFETIME, bool isShotgun = false)
    {
        _direction     = direction.normalized;
        _speed         = speed;
        _damage        = damage;
        _isPlayer      = isPlayer;
        _lifetimeTimer = maxLifetime;

        // Set collision layer based on ownership
        int layer = isPlayer ? LayerMask.NameToLayer("PlayerBullet") : LayerMask.NameToLayer("EnemyBullet");
        if (layer != -1) gameObject.layer = layer;

        // Set bullet appearance based on type
        if (_sr != null)
        {
            if (isPlayer)
            {
                if (isShotgun)
                {
                    // Shotgun pellet: small orange
                    _sr.color = new Color(1f, 0.4f, 0f);
                    transform.localScale = _originalScale * 0.5f;
                }
                else if (damage > 1)
                {
                    // Power shot: large orange
                    _sr.color = new Color(1f, 0.3f, 0f);
                    transform.localScale = _originalScale * 1.5f;
                }
                else
                {
                    // Normal player bullet: yellow
                    _sr.color = new Color(1f, 0.9f, 0.1f);
                    transform.localScale = _originalScale;
                }
            }
            else
            {
                // Enemy bullet: red
                _sr.color = new Color(1f, 0.2f, 0.2f);
                transform.localScale = _originalScale;
            }
        }

        gameObject.SetActive(true);

        if (_rb != null)
            _rb.velocity = _direction * _speed;
    }

    private void ReturnToPool()
    {
        if (_rb != null) _rb.velocity = Vector2.zero;
        BulletPool.Instance?.Return(gameObject);
    }
}
