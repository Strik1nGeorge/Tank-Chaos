using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor tool: one-click generation of Player / Enemy / Bullet prefabs.
/// Saves a white square texture as a persistent .png asset so prefab
/// SpriteRenderers reference it correctly without runtime patching.
/// </summary>
public class PrefabBuilder
{
    private static Sprite _savedSprite;

    [MenuItem("Tools/Game Tools/Build Prefabs")]
    public static void BuildAllPrefabs()
    {
        // Ensure directories exist
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Prefabs");
        EnsureFolder("Assets/Resources/Sprites");

        _savedSprite = CreateOrLoadWhiteSquareAsset();

        BuildPlayerPrefab();
        BuildBasicEnemyPrefab();
        BuildFastEnemyPrefab();
        BuildBossEnemyPrefab();
        BuildBulletPrefab();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[PrefabBuilder] All Prefabs generated to Assets/Resources/Prefabs/");
    }

    // -----------------------------------------------
    // Persistent white square texture (fixes prefab serialization)
    // -----------------------------------------------
    private static Sprite CreateOrLoadWhiteSquareAsset()
    {
        string texPath = "Assets/Resources/Sprites/WhiteSquare.png";

        Sprite existing = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
        if (existing != null) return existing;

        // Create 4x4 white texture and save as PNG
        Texture2D tex = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[16];
        for (int i = 0; i < 16; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();

        byte[] pngData = tex.EncodeToPNG();
        File.WriteAllBytes(texPath, pngData);
        Object.DestroyImmediate(tex);

        AssetDatabase.Refresh();

        // Set import settings: Point filter, Pixels Per Unit = 4, Sprite mode
        TextureImporter importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 4;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(texPath);
        Debug.Log("[PrefabBuilder] WhiteSquare.png created");
        return sprite;
    }

    // -----------------------------------------------
    // Player Prefab
    // -----------------------------------------------
    private static void BuildPlayerPrefab()
    {
        GameObject player = new GameObject("Player");
        player.tag = "Player";
        int pLayer = LayerMask.NameToLayer("Player");
        if (pLayer != -1) player.layer = pLayer;

        SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
        sr.sprite = _savedSprite;
        sr.color = new Color(0.2f, 0.5f, 0.8f);
        sr.sortingOrder = 5;
        player.AddComponent<DropShadow>();

        // Barrel
        GameObject barrel = CreateChild(player, "Barrel", new Vector3(0f, 0.4f, 0f), new Vector3(0.2f, 0.5f, 1f));
        barrel.GetComponent<SpriteRenderer>().color = new Color(0.3f, 0.3f, 0.3f);
        barrel.GetComponent<SpriteRenderer>().sortingOrder = 7;

        GameObject trackL = CreateChild(player, "TrackL", new Vector3(-0.45f, 0f, 0f), new Vector3(0.2f, 1.1f, 1f));
        trackL.GetComponent<SpriteRenderer>().color = new Color(0.25f, 0.25f, 0.25f);
        trackL.GetComponent<SpriteRenderer>().sortingOrder = 6;

        GameObject trackR = CreateChild(player, "TrackR", new Vector3(0.45f, 0f, 0f), new Vector3(0.2f, 1.1f, 1f));
        trackR.GetComponent<SpriteRenderer>().color = new Color(0.25f, 0.25f, 0.25f);
        trackR.GetComponent<SpriteRenderer>().sortingOrder = 6;

        // Physics
        Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        player.AddComponent<CircleCollider2D>().radius = 0.4f;

        // Logic
        player.AddComponent<PlayerHealth>();
        player.AddComponent<PlayerController>();

        SavePrefab(player, "Assets/Resources/Prefabs/Player.prefab");
        Object.DestroyImmediate(player);
        Debug.Log("[PrefabBuilder] Player.prefab done");
    }

    // -----------------------------------------------
    // Enemy Prefabs
    // -----------------------------------------------
    private static void BuildBasicEnemyPrefab()
    {
        GameObject enemy = BuildEnemyBase("BasicEnemy");
        enemy.AddComponent<BasicEnemy>();
        SavePrefab(enemy, "Assets/Resources/Prefabs/BasicEnemy.prefab");
        Object.DestroyImmediate(enemy);
        Debug.Log("[PrefabBuilder] BasicEnemy.prefab done");
    }

    private static void BuildFastEnemyPrefab()
    {
        GameObject enemy = BuildEnemyBase("FastEnemy");
        enemy.AddComponent<FastEnemy>();
        SavePrefab(enemy, "Assets/Resources/Prefabs/FastEnemy.prefab");
        Object.DestroyImmediate(enemy);
        Debug.Log("[PrefabBuilder] FastEnemy.prefab done");
    }

    private static void BuildBossEnemyPrefab()
    {
        GameObject boss = new GameObject("BossEnemy");
        boss.tag = "Enemy";
        int eLayer = LayerMask.NameToLayer("Enemy");
        if (eLayer != -1) boss.layer = eLayer;

        // Boss visuals: 2x scale, dark red
        SpriteRenderer sr = boss.AddComponent<SpriteRenderer>();
        sr.sprite = _savedSprite;
        sr.color = new Color(0.6f, 0.05f, 0.05f);
        sr.sortingOrder = 5;
        boss.transform.localScale = Vector3.one * 2f;

        // Barrel (thicker)
        GameObject barrel = CreateChild(boss, "Barrel", new Vector3(0f, 0.45f, 0f), new Vector3(0.25f, 0.5f, 1f));
        barrel.GetComponent<SpriteRenderer>().color = new Color(0.3f, 0.0f, 0.0f);
        barrel.GetComponent<SpriteRenderer>().sortingOrder = 6;

        // Physics
        Rigidbody2D rb = boss.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        boss.AddComponent<CircleCollider2D>().radius = 0.5f;

        boss.AddComponent<BossEnemy>();

        SavePrefab(boss, "Assets/Resources/Prefabs/BossEnemy.prefab");
        Object.DestroyImmediate(boss);
        Debug.Log("[PrefabBuilder] BossEnemy.prefab done");
    }

    // -----------------------------------------------
    // Bullet Prefab
    // -----------------------------------------------
    private static void BuildBulletPrefab()
    {
        GameObject bullet = new GameObject("Bullet");
        SpriteRenderer sr = bullet.AddComponent<SpriteRenderer>();
        sr.sprite = _savedSprite;
        sr.color = new Color(1f, 0.9f, 0.1f);
        sr.sortingOrder = 10;
        bullet.transform.localScale = new Vector3(0.3f, 0.3f, 1f);

        Rigidbody2D rb = bullet.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        bullet.AddComponent<CircleCollider2D>().radius = 0.15f;
        bullet.GetComponent<CircleCollider2D>().isTrigger = true;
        bullet.AddComponent<Bullet>();

        SavePrefab(bullet, "Assets/Resources/Prefabs/Bullet.prefab");
        Object.DestroyImmediate(bullet);
        Debug.Log("[PrefabBuilder] Bullet.prefab done");
    }

    // -----------------------------------------------
    // Shared enemy skeleton
    // -----------------------------------------------
    private static GameObject BuildEnemyBase(string name)
    {
        GameObject enemy = new GameObject(name);
        enemy.tag = "Enemy";
        int eLayer = LayerMask.NameToLayer("Enemy");
        if (eLayer != -1) enemy.layer = eLayer;

        SpriteRenderer sr = enemy.AddComponent<SpriteRenderer>();
        sr.sprite = _savedSprite;
        sr.sortingOrder = 5;
        enemy.AddComponent<DropShadow>();

        // Barrel
        GameObject barrel = CreateChild(enemy, "Barrel", new Vector3(0f, 0.4f, 0f), new Vector3(0.2f, 0.4f, 1f));
        barrel.GetComponent<SpriteRenderer>().color = new Color(0.2f, 0.2f, 0.2f);
        barrel.GetComponent<SpriteRenderer>().sortingOrder = 6;

        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        enemy.AddComponent<CircleCollider2D>().radius = 0.4f;
        enemy.AddComponent<EnemyAI>();

        return enemy;
    }

    // -----------------------------------------------
    // Utility methods
    // -----------------------------------------------
    private static GameObject CreateChild(GameObject parent, string name, Vector3 localPos, Vector3 localScale)
    {
        GameObject child = new GameObject(name);
        child.transform.SetParent(parent.transform, false);
        child.transform.localPosition = localPos;
        child.transform.localScale = localScale;
        SpriteRenderer sr = child.AddComponent<SpriteRenderer>();
        sr.sprite = _savedSprite;
        child.AddComponent<DropShadow>();
        return child;
    }

    private static void SavePrefab(GameObject obj, string path)
    {
        PrefabUtility.SaveAsPrefabAsset(obj, path);
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path).Replace("\\", "/");
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
