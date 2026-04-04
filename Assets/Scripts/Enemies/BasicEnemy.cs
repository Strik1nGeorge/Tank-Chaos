using UnityEngine;

/// <summary>
/// Basic enemy type. Moves toward the player and fires periodically.
/// Uses EnemyAI for pathfinding and obstacle avoidance.
/// </summary>
[RequireComponent(typeof(EnemyAI))]
public class BasicEnemy : EnemyBase
{
    private const float BASE_SPEED  = 1.5f;
    private const int   BASE_HEALTH = 2;
    private const int   BASE_DAMAGE = 1;
    private const int   SCORE       = 10;

    private const float FIRE_RATE = 2.0f; // Seconds between shots
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
        _sr.color = new Color(0.8f, 0.2f, 0.2f); // Red tint
    }

    protected override void HandleAction()
    {
        if (_ai == null || _player == null) return;

        // Fire at player when in range and has line of sight
        if (_ai.DistanceToPlayer <= _ai.AttackRange && _ai.HasLineOfSight)
        {
            if (_fireCooldown <= 0f)
            {
                FireBulletAt(_player.position, Color.red);
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

            // Rotate to face movement direction
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        else
        {
            _rb.velocity = Vector2.zero;
        }
    }
}
