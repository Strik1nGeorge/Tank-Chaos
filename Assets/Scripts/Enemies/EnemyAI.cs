using UnityEngine;

/// <summary>
/// AI controller for enemies. Uses Raycast for line-of-sight checks and simple
/// obstacle avoidance (no A* pathfinding). Parameters are tunable in Inspector.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    // Tuning parameters
    public  float AttackRange   = 6.0f;  // Attack range threshold
    private float ChaseRange    = 8.0f;  // Chase activation range
    private float MemoryTime    = 5.0f;  // Seconds to remember player after losing sight
    private float ObstacleAvoid = 1.0f;  // Obstacle avoidance raycast distance

    // State
    private Transform _player;
    private float     _memoryTimer;
    private Vector2   _wanderDirection;
    private float     _wanderTimer;

    public float DistanceToPlayer { get; private set; }
    public bool  HasLineOfSight   { get; private set; }

    private void Awake()
    {
        // Find the player
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) _player = pObj.transform;
    }

    private void Update()
    {
        if (_player == null) return;

        DistanceToPlayer = Vector2.Distance(transform.position, _player.position);
        CheckLineOfSight();

        if (HasLineOfSight)
        {
            _memoryTimer = MemoryTime; // Reset memory timer on sight
        }
        else if (_memoryTimer > 0f)
        {
            _memoryTimer -= Time.deltaTime;
        }
    }

    // Movement decision

    /// <summary>Returns the next move direction. Called by EnemyBase subclasses.</summary>
    public Vector2 GetNextMoveDirection()
    {
        if (_player == null) return Vector2.zero;

        // Player visible or recently seen — chase
        if (HasLineOfSight || _memoryTimer > 0f)
        {
            Vector2 dirToPlayer = (_player.position - transform.position).normalized;
            return AvoidObstacles(dirToPlayer);
        }

        // Otherwise — wander
        return HandleWander();
    }

    private void CheckLineOfSight()
    {
        HasLineOfSight = false;
        if (DistanceToPlayer > ChaseRange && _memoryTimer <= 0f) return;

        Vector2 dir = (_player.position - transform.position).normalized;
        // Include Default (walls) and Player layers
        int layerMask = LayerMask.GetMask("Default", "Player"); 
        
        // Raycast toward the player, checking for walls
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, DistanceToPlayer, layerMask);
        
        // No wall hit means clear line of sight
        if (hit.collider == null || hit.collider.CompareTag("Player"))
        {
            HasLineOfSight = true;
        }
    }

    // Obstacle avoidance using side raycasts
    private Vector2 AvoidObstacles(Vector2 desiredDirection)
    {
        int layerMask = LayerMask.GetMask("Default"); // Walls use Default layer
        // Cast at +30 and -30 degree offsets
        Vector2 dirLeft  = Quaternion.Euler(0, 0, 30f)  * desiredDirection;
        Vector2 dirRight = Quaternion.Euler(0, 0, -30f) * desiredDirection;

        bool hitCenter = Physics2D.Raycast(transform.position, desiredDirection, ObstacleAvoid, layerMask).collider != null;
        bool hitLeft   = Physics2D.Raycast(transform.position, dirLeft,  ObstacleAvoid, layerMask).collider != null;
        bool hitRight  = Physics2D.Raycast(transform.position, dirRight, ObstacleAvoid, layerMask).collider != null;

        // Obstacle detected — steer around it
        if (hitCenter || hitLeft || hitRight)
        {
            // Only right blocked, steer left
            if (!hitLeft && hitRight)       return (Quaternion.Euler(0, 0, 45f) * desiredDirection).normalized;
            if (hitLeft && !hitRight)       return (Quaternion.Euler(0, 0, -45f) * desiredDirection).normalized;
            
            // Both blocked, turn 90 degrees
            return (Quaternion.Euler(0, 0, -90f) * desiredDirection).normalized;
        }

        return desiredDirection;
    }

    private Vector2 HandleWander()
    {
        _wanderTimer -= Time.deltaTime;
        if (_wanderTimer <= 0f)
        {
            // Pick a random direction every 1-3 seconds
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            _wanderDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            _wanderTimer     = Random.Range(1f, 3f);
        }
        return AvoidObstacles(_wanderDirection);
    }
}
