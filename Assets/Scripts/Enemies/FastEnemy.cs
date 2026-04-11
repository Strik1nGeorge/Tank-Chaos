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
            // NEW: Only fire if roughly aimed at the player
            Vector2 toPlayer = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            float angleToPlayer = Vector2.Angle(transform.up, toPlayer);

            if (_fireCooldown <= 0f && angleToPlayer < 10f)
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
        Vector2 targetFaceDir = moveDir;
        if (_ai.DistanceToPlayer <= _ai.AttackRange && _ai.HasLineOfSight)
        {
            targetFaceDir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
        }

        if (targetFaceDir.sqrMagnitude > 0.01f)
        {
            float targetAngle = Mathf.Atan2(targetFaceDir.y, targetFaceDir.x) * Mathf.Rad2Deg - 90f;
            Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);

            // NEW: Smooth rotation (Fast enemies turn quicker than basic ones)
            float turnSpeed = 360f; // degrees per second
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, turnSpeed * Time.deltaTime);
        }
    }
}
