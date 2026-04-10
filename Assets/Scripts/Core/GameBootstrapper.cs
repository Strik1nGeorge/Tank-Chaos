using UnityEngine;

/// <summary>
/// Creates a persistent root GameObject in the Hierarchy on Play,
/// then initializes all core systems (map, camera, BulletPool, etc.).
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    // Called before scene load to bootstrap the game.
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void ManualBoot()
    {
        // Create a persistent root GameObject
        GameObject root = new GameObject("[GameBootstrapper]");
        DontDestroyOnLoad(root);
        root.AddComponent<GameBootstrapper>();
    }

    // --- Fields ---
    private MapGenerator _mapGenerator;
    private Camera       _mainCamera;

    // --- Unity Lifecycle ---
    private void Awake()
    {
        Debug.Log("[GameBootstrapper] Awake initialization started");

        InitCamera();
        InitMapGenerator();
        GenerateMap();
        FocusCameraOnMap();
        InitBulletPool();
        SpawnPlayer();
        InitEnemySpawner();
        InitPickupSpawner();
        InitGameManager();
        InitUIManager();
        InitAudioManager();
        InitVfxManager();

        Debug.Log("[GameBootstrapper] Initialization complete");
    }

    // --- Camera Setup ---
    private void InitCamera()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            camGO.tag   = "MainCamera";
            _mainCamera = camGO.AddComponent<Camera>();
        }

        // Attach CameraShake if missing
        if (_mainCamera.gameObject.GetComponent<CameraShake>() == null)
        {
            _mainCamera.gameObject.AddComponent<CameraShake>();
        }

        _mainCamera.orthographic     = true;
        _mainCamera.orthographicSize = 13f;
        _mainCamera.backgroundColor  = new Color(0.10f, 0.12f, 0.15f);
        _mainCamera.clearFlags       = CameraClearFlags.SolidColor;
        _mainCamera.nearClipPlane    = -10f;
        _mainCamera.farClipPlane     =  10f;
    }

    // --- Map ---
    private void InitMapGenerator()
    {
        _mapGenerator = GetComponent<MapGenerator>() ?? gameObject.AddComponent<MapGenerator>();
    }

    private void GenerateMap()
    {
        _mapGenerator.Generate();
        Debug.Log($"[GameBootstrapper] Map generated: {_mapGenerator.Width}x{_mapGenerator.Height}");
    }

    private void FocusCameraOnMap()
    {
        Vector2 center = _mapGenerator.MapCenter;
        _mainCamera.transform.position = new Vector3(center.x, center.y, -10f);
    }

    // --- Bullet Pool ---
    private void InitBulletPool()
    {
        if (FindObjectOfType<BulletPool>() != null) return;
        GameObject poolGO = new GameObject("BulletPool");
        poolGO.transform.SetParent(transform);
        poolGO.AddComponent<BulletPool>();
    }

    // --- Player ---
    private void SpawnPlayer()
    {
        Vector2 spawnPos2D = _mapGenerator.MapCenter;
        Vector3 spawnPos   = new Vector3(spawnPos2D.x, spawnPos2D.y, 0f);
        PlayerSpawnPoint.Position = spawnPos;

        // Load Player prefab from Resources/Prefabs
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Player");
        if (prefab != null)
        {
            GameObject player = Object.Instantiate(prefab, spawnPos, Quaternion.identity);
            player.name = "Player";

            // Apply skin colors
            SpriteRenderer sr = player.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = SkinManager.GetBodyColor();

            Transform barrel = player.transform.Find("Barrel");
            if (barrel != null) barrel.GetComponent<SpriteRenderer>().color = SkinManager.GetBarrelColor();

            Transform trackL = player.transform.Find("TrackL");
            if (trackL != null) trackL.GetComponent<SpriteRenderer>().color = SkinManager.GetTrackColor();

            Transform trackR = player.transform.Find("TrackR");
            if (trackR != null) trackR.GetComponent<SpriteRenderer>().color = SkinManager.GetTrackColor();

            Debug.Log($"[GameBootstrapper] Player Prefab spawned at {spawnPos}");
        }
        else
        {
            Debug.LogError("[GameBootstrapper] Missing Player Prefab! Use Tools > Game Tools > Build Prefabs");
        }
    }

    // --- Enemy / Pickup Spawners ---
    private void InitEnemySpawner()
    {
        if (FindObjectOfType<EnemySpawner>() != null) return;
        GameObject spawnerGO = new GameObject("EnemySpawner");
        spawnerGO.transform.SetParent(transform);
        spawnerGO.AddComponent<EnemySpawner>();
    }

    private void InitPickupSpawner()
    {
        if (FindObjectOfType<PickupSpawner>() != null) return;
        GameObject spawnerGO = new GameObject("PickupSpawner");
        spawnerGO.transform.SetParent(transform);
        spawnerGO.AddComponent<PickupSpawner>();
    }

    private void InitGameManager()
    {
        if (FindObjectOfType<GameManager>() != null) return;
        // GameManager.Start() will locate the Spawner and Player
        GameObject gmGO = new GameObject("GameManager");
        gmGO.transform.SetParent(transform);
        gmGO.AddComponent<GameManager>();
    }

    private void InitUIManager()
    {
        // UIManager lives on a GameCanvas built by the Editor tool;
        // we only verify it exists here.
        UIManager existing = FindObjectOfType<UIManager>();
        if (existing != null)
        {
            Debug.Log("[GameBootstrapper] UIManager found");
            return;
        }

        Debug.LogError("[GameBootstrapper] UIManager not found. Please build UI via Tools > Game Tools > Build UI Scene");
    }

    private void InitAudioManager()
    {
        if (FindObjectOfType<AudioManager>() != null) return;
        GameObject audioGO = new GameObject("AudioManager");
        audioGO.transform.SetParent(transform);
        audioGO.AddComponent<AudioManager>();
    }

    private void InitVfxManager()
    {
        if (FindObjectOfType<VfxManager>() != null) return;
        GameObject vfxGO = new GameObject("VfxManager");
        vfxGO.transform.SetParent(transform);
        vfxGO.AddComponent<VfxManager>();
    }

}

