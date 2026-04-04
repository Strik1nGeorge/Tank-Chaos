using UnityEngine;

/// <summary>
/// Generates a 23x23 tile-based map with boundary walls, obstacles,
/// and destructible walls. Call Generate() to build the map.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    // --- Constants ---
    private const int   MAP_WIDTH          = 23;
    private const int   MAP_HEIGHT         = 23;
    private const float CELL_SIZE          = 1f;
    private const float OBSTACLE_DENSITY   = 0.18f;
    private const int   RANDOM_SEED        = 42;
    private const float DESTRUCTIBLE_RATIO = 0.6f;

    // Tile colors
    private static readonly Color COLOR_BOUNDARY = new Color(0.20f, 0.20f, 0.20f); // dark gray
    private static readonly Color COLOR_OBSTACLE = new Color(0.35f, 0.25f, 0.10f); // brown
    private static readonly Color COLOR_DESTRUCT = new Color(0.60f, 0.45f, 0.25f); // light brown
    private static readonly Color COLOR_FLOOR    = new Color(0.55f, 0.75f, 0.40f); // green

    // --- Internal state ---
    private Transform _mapRoot;
    private float _currentDensityMult = 1.0f;

    // --- Public API ---

    /// <summary>Generate the map with default density.</summary>
    public void Generate()
    {
        GenerateWithDensity(1.0f);
    }

    /// <summary>Generate the map with a density multiplier (1.0 = default).</summary>
    public void GenerateWithDensity(float densityMultiplier)
    {
        _currentDensityMult = densityMultiplier;
        DestroyExistingMap();
        CreateMapRoot();

        Random.InitState(RANDOM_SEED);

        PlaceFloor();
        PlaceBoundaryWalls();
        PlaceInternalObstacles();
    }

    // --- Map construction helpers ---

    private void DestroyExistingMap()
    {
        GameObject old = GameObject.Find("MapRoot");
        if (old != null) DestroyImmediate(old);
    }

    private void CreateMapRoot()
    {
        GameObject root = new GameObject("MapRoot");
        _mapRoot = root.transform;
        _mapRoot.position = Vector3.zero;
    }

    /// <summary>Fill the entire grid with floor tiles.</summary>
    private void PlaceFloor()
    {
        for (int r = 0; r < MAP_HEIGHT; r++)
        {
            for (int c = 0; c < MAP_WIDTH; c++)
            {
                SpawnTile("Floor", r, c, false, COLOR_FLOOR, -0.1f);
            }
        }
    }

    /// <summary>Place indestructible walls along the map border.</summary>
    private void PlaceBoundaryWalls()
    {
        for (int r = 0; r < MAP_HEIGHT; r++)
        {
            for (int c = 0; c < MAP_WIDTH; c++)
            {
                bool isBorder = (r == 0 || r == MAP_HEIGHT - 1 ||
                                 c == 0 || c == MAP_WIDTH  - 1);
                if (isBorder)
                {
                    SpawnTile("Wall", r, c, true, COLOR_BOUNDARY, 0f);
                }
            }
        }
    }

    /// <summary>Scatter obstacles inside the map, skipping the center 3x3 spawn area.</summary>
    private void PlaceInternalObstacles()
    {
        int centerR = MAP_HEIGHT / 2;
        int centerC = MAP_WIDTH  / 2;

        for (int r = 1; r < MAP_HEIGHT - 1; r++)
        {
            for (int c = 1; c < MAP_WIDTH - 1; c++)
            {
                // Keep the center 3x3 area clear for player spawn
                if (Mathf.Abs(r - centerR) <= 1 && Mathf.Abs(c - centerC) <= 1)
                    continue;

                if (Random.value < OBSTACLE_DENSITY * _currentDensityMult)
                {
                    bool isDestructible = Random.value < DESTRUCTIBLE_RATIO;
                    if (isDestructible)
                    {
                        SpawnTile("Destructible", r, c, true, COLOR_DESTRUCT, 0f);
                    }
                    else
                    {
                        SpawnTile("Obstacle", r, c, true, COLOR_OBSTACLE, 0f);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Create a single tile GameObject at the given grid position.
    /// </summary>
    /// <param name="label">Name shown in the Hierarchy</param>
    /// <param name="row">0-based row index</param>
    /// <param name="col">0-based column index</param>
    /// <param name="hasCollider">Whether to add a BoxCollider2D</param>
    /// <param name="color">SpriteRenderer tint color</param>
    /// <param name="zOffset">Z-axis offset for layering</param>
    private void SpawnTile(string label, int row, int col, bool hasCollider, Color color, float zOffset)
    {
        GameObject go = new GameObject($"{label}_{row}_{col}");
        go.transform.SetParent(_mapRoot, false);

        // World position from grid coordinates
        float x = col * CELL_SIZE;
        float y = row * CELL_SIZE;
        go.transform.position = new Vector3(x, y, zOffset);

        // Sprite setup using a 1x1 white square
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = GetWhiteSquareSprite();
        sr.color  = color;
        sr.sortingOrder = hasCollider ? 1 : 0;

        // Collider for walls and obstacles
        if (hasCollider)
        {
            BoxCollider2D col2d = go.AddComponent<BoxCollider2D>();
            col2d.size = Vector2.one * CELL_SIZE;

            // Visual drop shadow
            go.AddComponent<DropShadow>();
        }

        // Tag walls and destructibles so bullets can detect them
        if (label == "Wall" || label == "Obstacle")
        {
            go.tag = "Wall"; // requires "Wall" tag in Unity Tags
        }
        else if (label == "Destructible")
        {
            go.tag = "Wall";
            go.AddComponent<DestructibleWall>();
        }
    }

    /// <summary>
    /// Returns a runtime-generated 1x1 white sprite.
    /// </summary>
    private static Sprite GetWhiteSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(
            tex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),   // pivot at center
            1f                          // 1 pixel per unit = 1 cell
        );
    }

    // --- Properties ---

    /// <summary>World-space center of the map.</summary>
    public Vector2 MapCenter => new Vector2(
        (MAP_WIDTH  - 1) * CELL_SIZE * 0.5f,
        (MAP_HEIGHT - 1) * CELL_SIZE * 0.5f
    );

    public int Width  => MAP_WIDTH;
    public int Height => MAP_HEIGHT;
}
