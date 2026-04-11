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
    private bool    _isShotgun;
    private bool    _isLaser;
    private float   _lifetimeTimer;
    private float   _distanceTraveled;

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

        if (_isPlayer && !_isShotgun)
        {
            _distanceTraveled += _speed * Time.deltaTime;
            // Laser has same distance limit, or maybe slightly longer? 21f is map diagonal, perfectly fine.
            if (_distanceTraveled >= 21f)
            {
                Dissipate();
                return;
            }
        }

        if (_lifetimeTimer <= 0f)
            Dissipate();
    }

    private void FixedUpdate()
    {
        // Laser is long enough that it rarely tunnels; skipping check for performance.
        if (_isLaser || _rb == null) return;

        // Predictive Collision Check (Anti-Tunneling)
        // Check slightly ahead of the bullet's current trajectory
        float castDist = _speed * Time.fixedDeltaTime + 0.15f; 
        int wallMask = LayerMask.GetMask("Default");

        RaycastHit2D hit = Physics2D.Raycast(transform.position, _direction, castDist, wallMask);
        if (hit.collider != null && hit.collider.CompareTag("Wall"))
        {
            // We caught the bullet before it swept past the wall
            transform.position = hit.point;
            OnTriggerEnter2D(hit.collider);
        }
    }

    private void Dissipate()
    {
        // Spawn a small poof of particles matching the bullet's color to show it ran out of energy
        Color puffColor = _sr != null ? _sr.color : Color.gray;
        // Dim the color slightly for dissipation
        puffColor.a = 0.6f; 
        VfxManager.SpawnExplosion(transform.position, puffColor, 3);
        
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
                if (!_isLaser) ReturnToPool();
                return;
            }

            // Indestructible Wall
            if (_isLaser)
            {
                // Laser pierces straight through indestructible walls, do nothing!
                return;
            }
            else if (_isPlayer && !_isShotgun)
            {
                // Approximate the normal of the 1x1 block
                Vector2 diff = transform.position - other.transform.position;
                Vector2 normal = Mathf.Abs(diff.x) > Mathf.Abs(diff.y) 
                    ? new Vector2(Mathf.Sign(diff.x), 0) 
                    : new Vector2(0, Mathf.Sign(diff.y));

                _direction = Vector2.Reflect(_direction, normal).normalized;
                _rb.velocity = _direction * _speed;
            }
            else
            {
                ReturnToPool();
            }
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
                if (!_isLaser) ReturnToPool();
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
                    // Only shake camera if player actually takes damage (not invincible)
                    bool wasInvincible = ph.IsInvincible;
                    ph.TakeDamage(_damage);
                    AudioManager.Instance?.PlayExplosionSound();
                    VfxManager.SpawnExplosion(ph.transform.position, new Color(0.3f, 0.7f, 1f), 10);
                    if (!wasInvincible)
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
    public void Init(Vector2 direction, float speed, int damage, bool isPlayer, float maxLifetime = DEFAULT_LIFETIME, bool isShotgun = false, bool isLaser = false)
    {
        _direction     = direction.normalized;
        _speed         = speed;
        _damage        = damage;
        _isPlayer      = isPlayer;
        _isShotgun     = isShotgun;
        _isLaser       = isLaser;
        _lifetimeTimer = maxLifetime;
        _distanceTraveled = 0f;

        // Set collision layer based on ownership
        int layer = isPlayer ? LayerMask.NameToLayer("PlayerBullet") : LayerMask.NameToLayer("EnemyBullet");
        if (layer != -1) gameObject.layer = layer;

        // Set bullet appearance based on type
        if (_sr != null)
        {
            if (isPlayer)
            {
                if (_isLaser)
                {
                    // Laser bullet: Cyan and long
                    _sr.color = new Color(0.2f, 0.9f, 0.9f);
                    transform.localScale = new Vector3(_originalScale.x * 0.4f, _originalScale.y * 3.5f, _originalScale.z);
                    // Match rotation to direction so it looks like a laser beam
                    float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0, 0, angle - 90f); // -90 because sprite default is up
                }
                else if (isShotgun)
                {
                    if (damage > 1)
                    {
                        // Boosted shotgun pellet: orange-red, slightly bigger
                        _sr.color = new Color(1f, 0.3f, 0f);
                        transform.localScale = _originalScale * 0.7f;
                    }
                    else
                    {
                        // Normal shotgun pellet: small black
                        _sr.color = new Color(0.1f, 0.1f, 0.1f);
                        transform.localScale = _originalScale * 0.5f;
                    }
                }
                else if (damage > 1)
                {
                    // Power shot: large orange
                    _sr.color = new Color(1f, 0.3f, 0f);
                    transform.localScale = _originalScale * 1.5f;
                }
                else
                {
                    // Normal player bullet: black cannonball
                    _sr.color = new Color(0.1f, 0.1f, 0.1f);
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
