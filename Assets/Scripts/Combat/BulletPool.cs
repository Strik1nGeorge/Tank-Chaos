using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Object pool for bullets. Created by GameBootstrapper during SpawnPlayer / InitBulletPool().
/// Use Get() / Return() to acquire and release bullet instances.
/// </summary>
public class BulletPool : MonoBehaviour
{
    // --- Singleton ---
    public static BulletPool Instance { get; private set; }

    // --- Constants ---
    private const int   POOL_SIZE    = 20;
    private const float BULLET_SIZE  = 0.15f; // CircleCollider2D radius

    // --- Internal State ---
    private readonly Queue<GameObject> _pool = new Queue<GameObject>();
    private Transform _poolRoot;

    // --- Unity Lifecycle ---
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _poolRoot = transform;
        PrewarmPool();
    }

    // --- Public API ---

    /// <summary>Get a bullet from the pool (or create one if empty).</summary>
    public GameObject Get()
    {
        GameObject bullet = _pool.Count > 0 ? _pool.Dequeue() : CreateBullet();
        bullet.SetActive(true);
        return bullet;
    }

    /// <summary>Return a bullet to the pool.</summary>
    public void Return(GameObject bullet)
    {
        if (bullet == null) return;
        bullet.SetActive(false);
        bullet.transform.SetParent(_poolRoot, false);
        _pool.Enqueue(bullet);
    }

    /// <summary>Deactivate and return all active bullets to the pool.</summary>
    public void ClearAllActiveBullets()
    {
        // Collect active bullets first, then return them to avoid modifying the hierarchy during iteration
        List<GameObject> activeBullets = new List<GameObject>();
        foreach (Transform child in _poolRoot)
        {
            if (child.gameObject.activeSelf)
                activeBullets.Add(child.gameObject);
        }

        foreach (var b in activeBullets)
            Return(b);
    }

    // --- Internal Helpers ---

    private void PrewarmPool()
    {
        for (int i = 0; i < POOL_SIZE; i++)
        {
            GameObject b = CreateBullet();
            b.SetActive(false);
            _pool.Enqueue(b);
        }
        Debug.Log($"[BulletPool]  {POOL_SIZE}");
    }

    /// <summary>Create a new bullet instance. Uses the prefab if available, otherwise builds one from code.</summary>
    private GameObject CreateBullet()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Bullet");
        GameObject go;

        if (prefab != null)
        {
            go = Object.Instantiate(prefab, _poolRoot);
            go.name = "Bullet";
        }
        else
        {
            // Fallback: build bullet from code when no prefab exists
            go = new GameObject("Bullet");
            go.transform.SetParent(_poolRoot, false);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSquareSprite();
            sr.color = new Color(1f, 0.9f, 0.1f);
            sr.sortingOrder = 10;
            go.transform.localScale = Vector3.one * BULLET_SIZE * 2f;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = BULLET_SIZE;
            col.isTrigger = true;

            go.AddComponent<Bullet>();
        }

        return go;
    }

    private static Sprite MakeSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
