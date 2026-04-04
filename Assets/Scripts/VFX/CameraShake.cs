using UnityEngine;

/// <summary>
/// Camera shake effect singleton. Attached to the main camera by GameBootstrapper.
/// </summary>
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private float _shakeIntensity = 0f;
    private float _shakeDuration = 0f;
    private Vector3 _originalPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // Store original position (keep z = -10)
        _originalPosition = transform.localPosition;
    }

    private void Update()
    {
        if (Time.timeScale == 0) return; // Skip when paused

        if (_shakeDuration > 0)
        {
            // Apply random offset
            Vector2 offset = Random.insideUnitCircle * _shakeIntensity;
            transform.localPosition = new Vector3(_originalPosition.x + offset.x, _originalPosition.y + offset.y, _originalPosition.z);

            // Decrease remaining duration
            _shakeDuration -= Time.deltaTime;
        }
        else
        {
            // Reset to original position
            _shakeDuration = 0f;
            transform.localPosition = _originalPosition;
        }
    }

    /// <summary>
    /// Trigger a camera shake.
    /// </summary>
    /// <param name="intensity">Shake strength (recommended 0.1~0.5).</param>
    /// <param name="duration">Duration in seconds.</param>
    public static void TriggerShake(float intensity, float duration)
    {
        if (Instance != null)
        {
            Instance._shakeIntensity = intensity;
            Instance._shakeDuration = duration;
        }
    }
}
