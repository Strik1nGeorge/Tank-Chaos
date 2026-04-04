using UnityEngine;

/// <summary>
/// Spawns pickups at random empty positions on the map.
/// Alternates between health and damage-boost pickups.
/// Builds sprites procedurally — no Inspector assets needed.
/// </summary>
public class PickupSpawner : MonoBehaviour
{
    public static PickupSpawner Instance { get; private set; }

    private const int   MAX_ACTIVE_PICKUPS = 2;   // Max simultaneous pickups on map
    private const float SPAWN_INTERVAL_MIN = 15f; // Min seconds between spawns
    private const float SPAWN_INTERVAL_MAX = 20f; // Max seconds between spawns

    private int   _activePickups = 0;
    private float _spawnTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        ResetSpawnTimer();
    }

    private void Update()
    {
        // Only spawn during Level 2 gameplay
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameState.Playing) return;
        if (GameManager.Instance.CurrentLevel != 2) return;

        if (_activePickups >= MAX_ACTIVE_PICKUPS) return;

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            SpawnRandomPickup();
            ResetSpawnTimer();
        }
    }

    public void OnPickupCollected()
    {
        _activePickups--;
        if (_activePickups < 0) _activePickups = 0;
    }

    private void ResetSpawnTimer()
    {
        _spawnTimer = Random.Range(SPAWN_INTERVAL_MIN, SPAWN_INTERVAL_MAX);
    }

    private bool _spawnHealthNext = true;

    private void SpawnRandomPickup()
    {
        Vector2 spawnPos = FindEmptySpawnPosition();
        if (spawnPos == Vector2.zero)
        {
            Debug.LogWarning("[PickupSpawner] ");
            return;
        }

        // Alternate between health and damage-boost
        bool isHealth = _spawnHealthNext;
        _spawnHealthNext = !_spawnHealthNext;
        
        string pickupName = isHealth ? "HealthPickup" : "DamageBoostPickup";
        
        GameObject go = new GameObject(pickupName);
        go.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);
        
        // Default layer so OnTriggerEnter2D works with Player tag
        go.layer = 0; 
        
        // Colored square sprite
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = BuildSquareSprite();
        // Green for health, blue for damage boost
        sr.color = isHealth ? new Color(0.2f, 0.9f, 0.2f) : new Color(0.2f, 0.4f, 0.9f); 
        sr.sortingOrder = 4; // Above ground tiles
        // Scale to 0.6x0.6
        go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

        // Add collider
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        // col.isTrigger is set by PickupBase.Awake

        // Attach the appropriate pickup component
        if (isHealth)
            go.AddComponent<HealthPickup>();
        else
            go.AddComponent<DamageBoostPickup>();

        _activePickups++;
        Debug.Log($"[Pickup]  {pickupName} ?{spawnPos} (=? =)");
    }

    /// <summary>
    /// Finds a random position not overlapping existing colliders (similar to EnemySpawner).
    /// </summary>
    private Vector2 FindEmptySpawnPosition()
    {
        int maxAttempts = 50;
        int layerMask = LayerMask.GetMask("Default", "Player", "Enemy"); // Avoid occupied cells
        for (int i = 0; i < maxAttempts; i++)
        {
            float x = Random.Range(1.5f, 21.5f);
            float y = Random.Range(1.5f, 21.5f);
            Vector2 attempt = new Vector2(x, y);

            Collider2D hit = Physics2D.OverlapCircle(attempt, 0.3f, layerMask);
            if (hit == null)
            {
                return attempt;
            }
        }
        return Vector2.zero;
    }

    private static Sprite BuildSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
