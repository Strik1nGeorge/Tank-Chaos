using UnityEngine;

/// <summary>
/// Manages player tank skin customization.
/// Provides a preset color palette and persists choices via PlayerPrefs.
/// </summary>
public static class SkinManager
{
    private const string PREF_BODY   = "Skin_BodyColor";
    private const string PREF_TRACKS = "Skin_TrackColor";
    private const string PREF_BARREL = "Skin_BarrelColor";

    // Preset color palette
    public static readonly Color[] Palette = new Color[]
    {
        new Color(0.20f, 0.80f, 0.30f), // 1. Classic military green (default)
        new Color(0.85f, 0.70f, 0.20f), // 2. Desert sand
        new Color(0.80f, 0.20f, 0.20f), // 3. Alert red
        new Color(0.20f, 0.50f, 0.90f), // 4. Arctic blue
        new Color(0.55f, 0.20f, 0.80f), // 5. Electromagnetic purple
        new Color(0.20f, 0.20f, 0.20f), // 6. Stealth black
        new Color(0.90f, 0.90f, 0.90f), // 7. Silver white
    };

    // Default palette indices
    private const int DEFAULT_BODY   = 0; // Green
    private const int DEFAULT_TRACKS = 5; // Black
    private const int DEFAULT_BARREL = 5; // Black

    /// <summary>Returns the currently selected body color.</summary>
    public static Color GetBodyColor()
    {
        int index = PlayerPrefs.GetInt(PREF_BODY, DEFAULT_BODY);
        return Palette[Mathf.Clamp(index, 0, Palette.Length - 1)];
    }

    /// <summary>Returns the currently selected track color.</summary>
    public static Color GetTrackColor()
    {
        int index = PlayerPrefs.GetInt(PREF_TRACKS, DEFAULT_TRACKS);
        return Palette[Mathf.Clamp(index, 0, Palette.Length - 1)];
    }

    /// <summary>Returns the currently selected barrel color.</summary>
    public static Color GetBarrelColor()
    {
        int index = PlayerPrefs.GetInt(PREF_BARREL, DEFAULT_BARREL);
        return Palette[Mathf.Clamp(index, 0, Palette.Length - 1)];
    }

    /// <summary>Returns the raw palette indices.</summary>
    public static int GetBodyIndex()   => PlayerPrefs.GetInt(PREF_BODY, DEFAULT_BODY);
    public static int GetTrackIndex()  => PlayerPrefs.GetInt(PREF_TRACKS, DEFAULT_TRACKS);
    public static int GetBarrelIndex() => PlayerPrefs.GetInt(PREF_BARREL, DEFAULT_BARREL);

    /// <summary>Saves skin preferences to PlayerPrefs.</summary>
    public static void SaveSkin(int bodyIndex, int trackIndex, int barrelIndex)
    {
        PlayerPrefs.SetInt(PREF_BODY, bodyIndex);
        PlayerPrefs.SetInt(PREF_TRACKS, trackIndex);
        PlayerPrefs.SetInt(PREF_BARREL, barrelIndex);
        PlayerPrefs.Save();
        Debug.Log("[SkinManager] Skin saved");
    }
}
