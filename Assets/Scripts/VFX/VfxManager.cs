using UnityEngine;

/// <summary>
/// VFX manager singleton. Spawns explosions, muzzle flashes, and floating text.
/// </summary>
public class VfxManager : MonoBehaviour
{
    public static VfxManager Instance { get; private set; }

    private Sprite _squareSprite;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Build a runtime square sprite for particles
        _squareSprite = BuildSquareSprite();
    }

    /// <summary>
    /// Spawn an explosion of square particles.
    /// </summary>
    /// <param name="position">World position of the explosion.</param>
    /// <param name="color">Base color of the particles.</param>
    /// <param name="count">Number of particles to spawn.</param>
    public static void SpawnExplosion(Vector2 position, Color color, int count = 8)
    {
        if (Instance == null) return;

        for (int i = 0; i < count; i++)
        {
            // Create particle GameObject
            GameObject pGO = new GameObject("Particle");
            pGO.transform.position = position;

            // Random scale (0.08 to 0.2)
            float randomScale = Random.Range(0.08f, 0.2f);
            pGO.transform.localScale = new Vector3(randomScale, randomScale, 1f);

            // Add SpriteRenderer
            SpriteRenderer sr = pGO.AddComponent<SpriteRenderer>();
            sr.sprite = Instance._squareSprite;
            // Slightly vary the color shade
            float shade = Random.Range(0.8f, 1.2f);
            sr.color = new Color(
                Mathf.Clamp01(color.r * shade),
                Mathf.Clamp01(color.g * shade),
                Mathf.Clamp01(color.b * shade),
                1f
            );
            sr.sortingOrder = 10; // Render above background

            // Add particle behaviour
            ParticleBehaviour pb = pGO.AddComponent<ParticleBehaviour>();
            
            // Random direction and speed
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            float randomSpeed = Random.Range(2f, 6f);
            Vector2 initialVelocity = randomDir * randomSpeed;

            // Initialize with velocity, damping, and lifetime
            pb.Init(initialVelocity, 5f, Random.Range(0.3f, 0.6f));
        }
    }

    private Sprite BuildSquareSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }

    /// <summary>
    /// Spawn a muzzle flash effect at the given position and rotation.
    /// </summary>
    public static void SpawnMuzzleFlash(Vector3 position, Quaternion rotation)
    {
        if (Instance == null) return;

        GameObject flashGO = new GameObject("MuzzleFlash");
        // Align flash to barrel direction
        flashGO.transform.rotation = rotation;
        
        float stretch = Random.Range(1.2f, 1.6f);
        float flashHeight = 0.4f * stretch;
        
        // Offset the flash forward by half its height so the base of the fire starts exactly at the 'position' (barrel tip)
        flashGO.transform.position = position + flashGO.transform.up * (flashHeight * 0.5f);
        flashGO.transform.localScale = new Vector3(0.25f, flashHeight, 1f);

        SpriteRenderer sr = flashGO.AddComponent<SpriteRenderer>();
        sr.sprite = Instance._squareSprite;
        sr.color = new Color(1f, 0.9f, 0.2f, 1f);
        sr.sortingOrder = 15;

        // Flash fades very quickly (~3-4 frames)
        ParticleBehaviour pb = flashGO.AddComponent<ParticleBehaviour>();
        // Move forward slightly, disable spinning for directional flashes
        pb.Init(flashGO.transform.up * 2f, 10f, 0.06f, false);
    }

    /// <summary>
    /// Spawn floating text (damage numbers, score popups, etc.).
    /// </summary>
    public static void SpawnFloatingText(Vector3 position, string text, Color color, float lifetime = 1.0f, float floatSpeed = 1.5f)
    {
        if (Instance == null) return;

        GameObject textGO = new GameObject("FloatingText");
        textGO.transform.position = position;

        TextMesh tm = textGO.AddComponent<TextMesh>();
        FloatingText ft = textGO.AddComponent<FloatingText>();
        
        ft.Init(text, color, lifetime, floatSpeed);
    }
}
