using UnityEngine;

/// <summary>
/// AI controller for enemies. Uses Raycast for line-of-sight checks and simple
/// obstacle avoidance (no A* pathfinding). Includes direction smoothing to
/// prevent jitter/oscillation at obstacle edges.
/// </summary>
public class EnemyAI : MonoBehaviour
{
    // Tuning parameters
    public  float AttackRange   = 6.0f;  // Attack range threshold
    private float ChaseRange    = 8.0f;  // Chase activation range
    private float MemoryTime    = 5.0f;  // Seconds to remember player after losing sight
    private float ObstacleAvoid = 1.5f;  // Obstacle avoidance raycast distance (increased from 1.0)

    // Direction smoothing parameters
    private const float SMOOTH_SPEED      = 8f;   // Lerp speed for direction blending
    private const float DIR_HOLD_TIME     = 0.25f; // Minimum seconds to hold an avoidance direction

    // State
    private Transform _player;
    private float     _memoryTimer;
    private Vector2   _wanderDirection;
    private float     _wanderTimer;

    // Prevents instant swarming on spawn
    private float     _activationDelay;

    // Global swarm limit tracker
    private static System.Collections.Generic.HashSet<EnemyAI> _activeChasers = new System.Collections.Generic.HashSet<EnemyAI>();
    private const int MAX_CHASERS = 3;
    private bool _isChasingToken = false;

    // Smoothing state
    private Vector2 _currentDirection;     // The actual smoothed direction used for movement
    private Vector2 _targetDirection;      // The raw desired direction before smoothing
    private float   _avoidHoldTimer;       // Timer to hold avoidance direction

    public float DistanceToPlayer { get; private set; }
    public bool  HasLineOfSight   { get; private set; }

    private void Awake()
    {
        GameObject pObj = GameObject.FindGameObjectWithTag("Player");
        if (pObj != null) _player = pObj.transform;
        
        // Random 0.5 to 1.5 seconds wake up delay
        _activationDelay = Random.Range(0.5f, 1.5f);
    }

    private void Update()
    {
        if (_player == null) return;

        if (_activationDelay > 0f)
        {
            _activationDelay -= Time.deltaTime;
            return;
        }

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

        // Tick down the avoidance hold timer
        if (_avoidHoldTimer > 0f)
            _avoidHoldTimer -= Time.deltaTime;
    }

    private void OnDestroy()
    {
        ReleaseChaseToken();
    }

    private void ReleaseChaseToken()
    {
        if (_isChasingToken)
        {
            _activeChasers.Remove(this);
            _isChasingToken = false;
        }
    }

    // Movement decision

    /// <summary>Returns the next move direction. Called by EnemyBase subclasses.</summary>
    public Vector2 GetNextMoveDirection()
    {
        if (_player == null || _activationDelay > 0f) return Vector2.zero;

        // Determine raw target direction
        bool wantsToChase = HasLineOfSight || _memoryTimer > 0f;

        if (wantsToChase)
        {
            if (!_isChasingToken)
            {
                _activeChasers.RemoveWhere(e => e == null); // Clean up destroyed entities
                if (_activeChasers.Count < MAX_CHASERS)
                {
                    _activeChasers.Add(this);
                    _isChasingToken = true;
                }
            }
        }
        else
        {
            ReleaseChaseToken();
        }

        if (_isChasingToken)
        {
            // Chase mode
            Vector2 dirToPlayer = (_player.position - transform.position).normalized;
            _targetDirection = AvoidObstacles(dirToPlayer);
        }
        else
        {
            // Wander mode (either out of range/sight, or swarm limit reached)
            _targetDirection = HandleWander();
        }

        // Smooth the direction transition to prevent jitter
        if (_currentDirection == Vector2.zero)
            _currentDirection = _targetDirection;
        else
            _currentDirection = Vector2.Lerp(_currentDirection, _targetDirection, SMOOTH_SPEED * Time.deltaTime);

        // Re-normalize to prevent speed loss from lerp
        if (_currentDirection.sqrMagnitude > 0.01f)
            _currentDirection = _currentDirection.normalized;

        return _currentDirection;
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
        // If we're still holding a previous avoidance direction, keep it
        if (_avoidHoldTimer > 0f)
            return _targetDirection;

        int layerMask = LayerMask.GetMask("Default"); // Walls use Default layer
        // Cast at +30 and -30 degree offsets
        Vector2 dirLeft  = Quaternion.Euler(0, 0, 30f)  * desiredDirection;
        Vector2 dirRight = Quaternion.Euler(0, 0, -30f) * desiredDirection;

        bool hitCenter = Physics2D.Raycast(transform.position, desiredDirection, ObstacleAvoid, layerMask).collider != null;
        bool hitLeft   = Physics2D.Raycast(transform.position, dirLeft,  ObstacleAvoid, layerMask).collider != null;
        bool hitRight  = Physics2D.Raycast(transform.position, dirRight, ObstacleAvoid, layerMask).collider != null;

        // Obstacle detected — steer around it and hold direction
        if (hitCenter || hitLeft || hitRight)
        {
            Vector2 avoidDir;

            // Only right blocked, steer left
            if (!hitLeft && hitRight)
                avoidDir = (Quaternion.Euler(0, 0, 45f) * desiredDirection).normalized;
            else if (hitLeft && !hitRight)
                avoidDir = (Quaternion.Euler(0, 0, -45f) * desiredDirection).normalized;
            else
                // Both blocked, turn 90 degrees
                avoidDir = (Quaternion.Euler(0, 0, -90f) * desiredDirection).normalized;

            // Hold this avoidance direction for a minimum time to prevent oscillation
            _avoidHoldTimer = DIR_HOLD_TIME;
            return avoidDir;
        }

        return desiredDirection;
    }

    private Vector2 HandleWander()
    {
        _wanderTimer -= Time.deltaTime;
        if (_wanderTimer <= 0f)
        {
            // Pick a random direction every 2-4 seconds (increased from 1-3)
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            _wanderDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            _wanderTimer     = Random.Range(2f, 4f);
        }
        return AvoidObstacles(_wanderDirection);
    }
}
