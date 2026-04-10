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
    // Set to a positive value to force a fixed seed (useful for debugging)
    private const int   FIXED_SEED         = -1;
    private const float DESTRUCTIBLE_RATIO = 0.6f;

    // Tile colors (floor only — walls use procedural textures now)
    private static readonly Color COLOR_FLOOR = new Color(0.55f, 0.75f, 0.40f); // green

    // --- Internal state ---
    private Transform _mapRoot;
    private float _currentDensityMult = 1.0f;

    // --- Cached procedural sprites ---
    private static Sprite _cachedFloorSprite;
    private static Sprite _cachedBrickSprite;
    private static Sprite _cachedCrackedSprite;

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

        int seed = FIXED_SEED > 0 ? FIXED_SEED : System.Environment.TickCount;
        Random.InitState(seed);
        Debug.Log($"[MapGen] Seed={seed}");

        PlaceFloor();
        PlaceBoundaryWalls();
        PlaceInternalObstacles();
    }

    // --- Map construction helpers ---

    private void DestroyExistingMap()
    {
        GameObject old = GameObject.Find("MapRoot");
        if (old != null) Destroy(old);
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
                SpawnTile("Floor", r, c, false, COLOR_FLOOR, -0.1f, TileType.Floor);
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
                    SpawnTile("Wall", r, c, true, Color.white, 0f, TileType.Brick);
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
                        SpawnTile("Destructible", r, c, true, Color.white, 0f, TileType.Cracked);
                    }
                    else
                    {
                        SpawnTile("Obstacle", r, c, true, Color.white, 0f, TileType.Brick);
                    }
                }
            }
        }
    }

    // --- Tile type enum ---
    private enum TileType { Floor, Brick, Cracked }

    /// <summary>
    /// Create a single tile GameObject at the given grid position.
    /// </summary>
    private void SpawnTile(string label, int row, int col, bool hasCollider, Color color, float zOffset, TileType tileType)
    {
        GameObject go = new GameObject($"{label}_{row}_{col}");
        go.transform.SetParent(_mapRoot, false);

        // World position from grid coordinates
        float x = col * CELL_SIZE;
        float y = row * CELL_SIZE;
        go.transform.position = new Vector3(x, y, zOffset);

        // Sprite setup
        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        switch (tileType)
        {
            case TileType.Brick:
                sr.sprite = GetBrickSprite();
                sr.color = Color.white; // Texture already has brick colors
                break;
            case TileType.Cracked:
                sr.sprite = GetCrackedSprite();
                sr.color = Color.white; // Texture already has cracked colors
                break;
            default:
                sr.sprite = GetFloorSprite();
                sr.color = color;
                break;
        }
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

    // ═══════════════════════════════════════════
    // Procedural Texture Generation (cached)
    // ═══════════════════════════════════════════

    /// <summary>Simple 1x1 white sprite for floor tiles.</summary>
    private static Sprite GetFloorSprite()
    {
        if (_cachedFloorSprite != null) return _cachedFloorSprite;

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        _cachedFloorSprite = Sprite.Create(
            tex,
            new Rect(0, 0, 1, 1),
            new Vector2(0.5f, 0.5f),
            1f
        );
        return _cachedFloorSprite;
    }

    /// <summary>Procedurally generated red brick texture (32x32).</summary>
    private static Sprite GetBrickSprite()
    {
        if (_cachedBrickSprite != null) return _cachedBrickSprite;

        const int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[size * size];

        // Mortar color (cement/grout)
        Color32 mortar = new Color32(180, 165, 145, 255);

        // Fill with mortar first
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = mortar;

        // Brick layout: 4 rows of bricks
        // Each brick row is 6 pixels tall with 1 pixel mortar between
        // Row pattern: alternating offset every other row
        int brickH = 6;
        int brickW = 14;
        int mortarSize = 1;

        int rowIndex = 0;
        for (int y = 0; y < size; y++)
        {
            // Check if this y is on a horizontal mortar line
            int rowY = y % (brickH + mortarSize);
            if (rowY >= brickH)
            {
                if (rowY == brickH) rowIndex++;
                continue; // mortar row — already filled
            }

            // Determine brick offset for staggering
            int offset = (rowIndex % 2 == 0) ? 0 : brickW / 2 + mortarSize;

            for (int x = 0; x < size; x++)
            {
                int adjustedX = (x + offset) % (brickW + mortarSize);

                // Vertical mortar gap
                if (adjustedX >= brickW)
                    continue; // mortar column — already filled

                // Generate brick color with slight variation
                int brickId = ((x + offset) / (brickW + mortarSize)) + rowIndex * 7;
                System.Random rng = new System.Random(brickId * 31 + 17);
                byte r = (byte)(160 + rng.Next(40));   // 160-200 red
                byte g = (byte)(70  + rng.Next(30));    // 70-100 green
                byte b = (byte)(55  + rng.Next(25));    // 55-80 blue

                // Add per-pixel noise for texture feel
                int noise = (x * 7 + y * 13 + brickId * 3) % 15 - 7;
                r = (byte)Mathf.Clamp(r + noise, 120, 220);
                g = (byte)Mathf.Clamp(g + noise / 2, 50, 110);
                b = (byte)Mathf.Clamp(b + noise / 2, 40, 90);

                pixels[y * size + x] = new Color32(r, g, b, 255);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;

        _cachedBrickSprite = Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            size  // pixels per unit = size → 1 unit = 1 cell
        );
        return _cachedBrickSprite;
    }

    /// <summary>Procedurally generated cracked/wooden texture for destructible walls (32x32).</summary>
    private static Sprite GetCrackedSprite()
    {
        if (_cachedCrackedSprite != null) return _cachedCrackedSprite;

        const int size = 32;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color32[] pixels = new Color32[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Base: warm sandy/wooden tone
                int hash = (x * 73 + y * 137 + x * y * 3) % 256;
                byte baseR = (byte)(165 + (hash % 20));     // 165-185
                byte baseG = (byte)(130 + (hash % 15));     // 130-145
                byte baseB = (byte)(85  + (hash % 12));     // 85-97

                // Wood grain effect (horizontal lines with noise)
                int grain = (y * 3 + (x / 4)) % 8;
                if (grain < 2)
                {
                    baseR = (byte)Mathf.Clamp(baseR - 15, 100, 255);
                    baseG = (byte)Mathf.Clamp(baseG - 12, 80, 255);
                    baseB = (byte)Mathf.Clamp(baseB - 8, 60, 255);
                }

                // Crack lines (diagonal patterns)
                bool isCrack = false;
                // Main diagonal crack
                if (Mathf.Abs(x - y) <= 1 && x > 8 && x < 24)
                    isCrack = true;
                // Secondary crack
                if (Mathf.Abs(x - (size - 1 - y)) <= 0 && x > 4 && x < 16)
                    isCrack = true;
                // Small branching crack
                if (y == size / 2 && x > 12 && x < 22)
                    isCrack = true;

                if (isCrack)
                {
                    baseR = (byte)Mathf.Clamp(baseR - 50, 70, 255);
                    baseG = (byte)Mathf.Clamp(baseG - 45, 55, 255);
                    baseB = (byte)Mathf.Clamp(baseB - 30, 35, 255);
                }

                // Edge darkening (border effect to make tiles pop)
                if (x == 0 || x == size - 1 || y == 0 || y == size - 1)
                {
                    baseR = (byte)(baseR * 0.7f);
                    baseG = (byte)(baseG * 0.7f);
                    baseB = (byte)(baseB * 0.7f);
                }

                pixels[y * size + x] = new Color32(baseR, baseG, baseB, 255);
            }
        }

        tex.SetPixels32(pixels);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;

        _cachedCrackedSprite = Sprite.Create(
            tex,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            size
        );
        return _cachedCrackedSprite;
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
