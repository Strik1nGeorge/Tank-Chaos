using UnityEngine;

/// <summary>
/// Spawns pickups at random empty positions on the map.
/// Alternates between health and damage-boost pickups.
/// Builds sprites procedurally — no Inspector assets needed.
/// </summary>
public class PickupSpawner : MonoBehaviour
{
    public static PickupSpawner Instance { get; private set; }

    private int   _maxActivePickups = 2;   // Max simultaneous pickups on map
    private float _spawnIntervalMin = 15f; // Min seconds between spawns
    private float _spawnIntervalMax = 20f; // Max seconds between spawns

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
        // Only spawn during Level 2+ gameplay
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameState.Playing) return;
        
        int currentLevel = GameManager.Instance.CurrentLevel;
        if (currentLevel < 2) return;

        // Adjust parameters based on level
        if (currentLevel == 2)
        {
            _maxActivePickups = 3;
            _spawnIntervalMin = 8f;
            _spawnIntervalMax = 12f;
        }
        else if (currentLevel >= 3)
        {
            _maxActivePickups = 4;
            _spawnIntervalMin = 5f;
            _spawnIntervalMax = 10f;
        }

        if (_activePickups >= _maxActivePickups) return;

        _spawnTimer -= Time.deltaTime;
        if (_spawnTimer <= 0f)
        {
            SpawnRandomPickup(currentLevel);
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
        _spawnTimer = Random.Range(_spawnIntervalMin, _spawnIntervalMax);
    }

    private bool _spawnHealthNext = true;

    private void SpawnRandomPickup(int level)
    {
        Vector2 spawnPos = FindEmptySpawnPosition();
        if (spawnPos == Vector2.zero)
        {
            Debug.LogWarning("[PickupSpawner] Could not find empty spawn position");
            return;
        }

        // Determine pickup type
        // 0: Health, 1: DamageBoost, 2: Shield (Level 3+ only)
        int pickupType = 0;
        
        if (level < 3)
        {
            pickupType = _spawnHealthNext ? 0 : 1;
            _spawnHealthNext = !_spawnHealthNext;
        }
        else
        {
            // Level 3 random pool: 40% Health, 40% Damage, 20% Shield
            float rand = Random.value;
            if (rand < 0.4f) pickupType = 0;
            else if (rand < 0.8f) pickupType = 1;
            else pickupType = 2;
        }
        
        string pickupName = "";
        Color pickupColor = Color.white;
        
        switch (pickupType)
        {
            case 0:
                pickupName = "HealthPickup";
                pickupColor = new Color(0.2f, 0.9f, 0.2f); // Green
                break;
            case 1:
                pickupName = "DamageBoostPickup";
                pickupColor = new Color(0.2f, 0.4f, 0.9f); // Blue
                break;
            case 2:
                pickupName = "ShieldPickup";
                pickupColor = new Color(0.9f, 0.8f, 0.1f); // Yellow
                break;
        }
        
        GameObject go = new GameObject(pickupName);
        go.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);
        
        // Default layer so OnTriggerEnter2D works with Player tag
        go.layer = 0; 
        
        // Colored square sprite
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = BuildSquareSprite();
        sr.color = pickupColor;
        sr.sortingOrder = 4; // Above ground tiles
        // Scale to 0.6x0.6
        go.transform.localScale = new Vector3(0.6f, 0.6f, 1f);

        // Add collider
        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        // col.isTrigger is set by PickupBase.Awake

        // Attach the appropriate pickup component
        if (pickupType == 0) go.AddComponent<HealthPickup>();
        else if (pickupType == 1) go.AddComponent<DamageBoostPickup>();
        else if (pickupType == 2) go.AddComponent<ShieldPickup>();

        _activePickups++;
        Debug.Log($"[Pickup] Spawned {pickupName} at {spawnPos} (Active: {_activePickups})");
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

    private static Sprite _cachedSquareSprite;
    private static Sprite BuildSquareSprite()
    {
        if (_cachedSquareSprite != null) return _cachedSquareSprite;

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        _cachedSquareSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        return _cachedSquareSprite;
    }
}
