using UnityEngine;

/// <summary>
/// Fast enemy type. Higher speed and fire rate, but lower health.
/// Uses EnemyAI for pathfinding and obstacle avoidance.
/// </summary>
[RequireComponent(typeof(EnemyAI))]
public class FastEnemy : EnemyBase
{
    private const float BASE_SPEED  = 3.2f;
    private const int   BASE_HEALTH = 1;
    private const int   BASE_DAMAGE = 1;
    private const int   SCORE       = 20;

    private const float FIRE_RATE = 1.2f; // Seconds between shots
    private float _fireCooldown;

    protected override void Awake()
    {
        base.Awake();
        Health     = BASE_HEALTH;
        Speed      = BASE_SPEED;
        Damage     = BASE_DAMAGE;
        ScoreValue = SCORE;

        SetupVisuals();
    }

    protected override void SetupVisuals()
    {
        if (_sr == null) return;
        _sr.color = new Color(1.0f, 0.5f, 0.0f); // Orange tint
        
        transform.localScale = new Vector3(0.8f, 0.8f, 1f);
    }

    protected override void HandleAction()
    {
        if (_ai == null || _player == null) return;

        // Fire when in range and has line of sight
        if (_ai.DistanceToPlayer <= _ai.AttackRange && _ai.HasLineOfSight)
        {
            if (_fireCooldown <= 0f)
            {
                FireBulletAt(_player.position, new Color(1.0f, 0.6f, 0.2f));
                _fireCooldown = FIRE_RATE;
            }
        }

        if (_fireCooldown > 0f)
            _fireCooldown -= Time.deltaTime;
    }

    protected override void HandleMovement()
    {
        if (_ai == null || _player == null) return;

        Vector2 moveDir = _ai.GetNextMoveDirection();
        if (moveDir != Vector2.zero)
        {
            _rb.velocity = moveDir * Speed;
        }
        else
        {
            _rb.velocity = Vector2.zero;
        }

        // Face player if shooting/in range, else face move direction
        if (_ai.DistanceToPlayer <= _ai.AttackRange && _ai.HasLineOfSight)
        {
            Vector2 faceDir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            if (faceDir.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(faceDir.y, faceDir.x) * Mathf.Rad2Deg - 90f;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
        else if (moveDir != Vector2.zero)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
}
