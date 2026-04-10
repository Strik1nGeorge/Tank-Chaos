using UnityEngine;
using System.Collections.Generic;


/// <summary>
/// Manages all game audio: BGM loaded from Resources/Audio/BGM/ and
/// procedurally synthesized SFX via OnAudioFilterRead.
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    // --- BGM Fields ---
    private AudioSource _bgmSource;
    private AudioClip[] _bgmClips;
    private string[]    _bgmNames;
    private int         _currentTrackIndex = 0;
    private int         _victoryTrackIndex = -1; // Index of "Triumph of the Dawn"

    // --- SFX Synthesis Fields ---
    private AudioSource _sfxSource; // driven by OnAudioFilterRead
    private float _sampleRate;
    private System.Random _sysRandom = new System.Random();

    private class SfxVoice
    {
        public int Type; // 0:Shoot, 1:Explosion, 2:Powerup
        public float Phase;
        public float Duration;
        public float TimeAlive;
        public float BaseFreq;
    }
    private List<SfxVoice> _sfxVoices = new List<SfxVoice>();

    // --- Volume Settings ---
    private float _bgmVolume = 0.5f;
    private float _sfxVolume = 0.5f;

    public float BgmVolume => _bgmVolume;
    public float SfxVolume => _sfxVolume;
    public string CurrentTrackName => (_bgmNames != null && _bgmNames.Length > 0)
        ? _bgmNames[_currentTrackIndex]
        : "No Tracks";

    // --- Initialization ---
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        _sampleRate = AudioSettings.outputSampleRate;

        // --- BGM AudioSource setup (on a child object to avoid OnAudioFilterRead conflict) ---
        GameObject bgmGO = new GameObject("BGM_Source");
        bgmGO.transform.SetParent(transform);
        _bgmSource = bgmGO.AddComponent<AudioSource>();
        _bgmSource.playOnAwake = false;
        _bgmSource.loop = true;
        _bgmSource.spatialBlend = 0f;
        _bgmSource.volume = _bgmVolume;

        // --- SFX AudioSource (procedural synthesis via OnAudioFilterRead on main object) ---
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.playOnAwake = true;
        _sfxSource.loop = true;
        _sfxSource.spatialBlend = 0f;
        _sfxSource.volume = 1f; // SFX gain is applied per-voice in synthesis callback
        _sfxSource.Play();

        // Load BGM tracks from Resources
        LoadBGMTracks();

        // Restore saved volume settings
        _bgmVolume = PlayerPrefs.GetFloat("Settings_BGM_Volume", 0.5f);
        _sfxVolume = PlayerPrefs.GetFloat("Settings_SFX_Volume", 0.5f);
        _bgmSource.volume = _bgmVolume;

        // Play first track on startup
        if (_bgmClips.Length > 0)
        {
            PlayTrack(0);
        }
    }

    // --- BGM Track Management ---

    private void LoadBGMTracks()
    {
        // Load all AudioClips from Resources/Audio/BGM/
        AudioClip[] loaded = Resources.LoadAll<AudioClip>("Audio/BGM");

        if (loaded != null && loaded.Length > 0)
        {
            System.Array.Sort(loaded, (a, b) => string.Compare(a.name, b.name));
            _bgmClips = loaded;
            _bgmNames = new string[loaded.Length];
            for (int i = 0; i < loaded.Length; i++)
            {
                // Format display name (replace underscores with spaces)
                _bgmNames[i] = loaded[i].name.Replace("_", " ");

                // Find the victory track by name
                if (loaded[i].name.Contains("Triumph"))
                    _victoryTrackIndex = i;
            }
            Debug.Log($"[Audio] Loaded {loaded.Length} BGM tracks: {string.Join(", ", _bgmNames)}");
            if (_victoryTrackIndex >= 0)
                Debug.Log($"[Audio] Victory track found at index {_victoryTrackIndex}: {_bgmNames[_victoryTrackIndex]}");
        }
        else
        {
            Debug.LogWarning("[Audio] Resources/Audio/BGM/ is empty. Using synth fallback tracks.");
            _bgmClips = new AudioClip[0];
            _bgmNames = new string[] { "Synth Fallback" };
        }
    }

    private void PlayTrack(int index)
    {
        if (_bgmClips.Length == 0) return;

        _currentTrackIndex = index % _bgmClips.Length;
        _bgmSource.clip = _bgmClips[_currentTrackIndex];
        _bgmSource.Play();
        Debug.Log($"[Audio] Now playing: {_bgmNames[_currentTrackIndex]}");
    }

    // --- Public API ---

    /// <summary>Set BGM and SFX volume (0~1).</summary>
    public void SetVolume(float bgm, float sfx)
    {
        _bgmVolume = Mathf.Clamp01(bgm);
        _sfxVolume = Mathf.Clamp01(sfx);
        _bgmSource.volume = _bgmVolume;
        Debug.Log($"[Audio] Volume: BGM={_bgmVolume:F2}, SFX={_sfxVolume:F2}");
    }

    /// <summary>Switch to the next BGM track.</summary>
    public void NextTrack()
    {
        if (_bgmClips.Length > 0)
        {
            PlayTrack(_currentTrackIndex + 1);
        }
    }

    /// <summary>Play a specific BGM track by index (used by GameManager.StartLevel).</summary>
    public void PlayTrackByIndex(int index)
    {
        if (_bgmClips.Length == 0) return;
        PlayTrack(index % _bgmClips.Length);
    }

    /// <summary>Play the victory fanfare track ("Triumph of the Dawn"). Non-looping.</summary>
    public void PlayVictoryTrack()
    {
        if (_bgmClips.Length == 0 || _victoryTrackIndex < 0)
        {
            Debug.LogWarning("[Audio] Victory track not found, keeping current BGM");
            return;
        }

        _bgmSource.loop = false; // Play once as fanfare
        _bgmSource.clip = _bgmClips[_victoryTrackIndex];
        _bgmSource.Play();
        Debug.Log($"[Audio] Victory fanfare: {_bgmNames[_victoryTrackIndex]}");
    }

    /// <summary>Resume the standard gameplay track by index (restores looping).</summary>
    public void ResumeGameplayTrack(int trackIndex)
    {
        _bgmSource.loop = true;
        PlayTrackByIndex(trackIndex);
    }

    /// <summary>Play the shoot sound effect.</summary>
    public void PlayShootSound()
    {
        lock (_sfxVoices)
        {
            _sfxVoices.Add(new SfxVoice { Type = 0, Phase = 0, Duration = 0.15f, TimeAlive = 0f, BaseFreq = 880f });
        }
    }

    /// <summary>Play the explosion sound effect.</summary>
    public void PlayExplosionSound()
    {
        lock (_sfxVoices)
        {
            _sfxVoices.Add(new SfxVoice { Type = 1, Phase = 0, Duration = 0.4f, TimeAlive = 0f });
        }
    }

    /// <summary>Play the powerup sound effect.</summary>
    public void PlayPowerupSound()
    {
        lock (_sfxVoices)
        {
            _sfxVoices.Add(new SfxVoice { Type = 2, Phase = 0, Duration = 0.3f, TimeAlive = 0f, BaseFreq = 400f });
        }
    }

    // --- SFX Synthesis (runs on audio thread via _sfxSource) ---
    private void OnAudioFilterRead(float[] data, int channels)
    {
        lock (_sfxVoices)
        {
            if (_sfxVoices.Count == 0) return;

            for (int i = 0; i < data.Length; i += channels)
            {
                float output = 0f;

                for (int v = _sfxVoices.Count - 1; v >= 0; v--)
                {
                    SfxVoice voice = _sfxVoices[v];
                    float sfxOutput = 0f;

                    if (voice.Type == 0) // Shoot
                    {
                        float currentFreq = voice.BaseFreq * (1f - (voice.TimeAlive / voice.Duration));
                        voice.Phase += currentFreq / _sampleRate;
                        if (voice.Phase > 1f) voice.Phase -= 1f;
                        sfxOutput = Mathf.Sin(voice.Phase * 2f * Mathf.PI);
                    }
                    else if (voice.Type == 1) // Explosion
                    {
                        sfxOutput = (float)(_sysRandom.NextDouble() * 2.0 - 1.0);
                    }
                    else if (voice.Type == 2) // Powerup
                    {
                        float currentFreq = voice.BaseFreq + (voice.TimeAlive * 1000f);
                        voice.Phase += currentFreq / _sampleRate;
                        if (voice.Phase > 1f) voice.Phase -= 1f;
                        sfxOutput = (voice.Phase < 0.5f) ? 1f : -1f;
                    }

                    float envelope = 1f - (voice.TimeAlive / voice.Duration);
                    sfxOutput *= envelope * 0.85f * _sfxVolume; // Apply volume envelope
                    output += sfxOutput;

                    voice.TimeAlive += 1f / _sampleRate;
                    if (voice.TimeAlive >= voice.Duration)
                    {
                        _sfxVoices.RemoveAt(v);
                    }
                }

                output = Mathf.Clamp(output * 0.45f, -1f, 1f); // Clamp total SFX output
                for (int c = 0; c < channels; c++)
                {
                    data[i + c] += output; // Additive mix into the SFX AudioSource stream
                }
            }
        }
    }
}
