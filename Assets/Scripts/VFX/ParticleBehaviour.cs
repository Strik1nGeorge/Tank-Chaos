using UnityEngine;

/// <summary>
/// Simple particle that moves, spins, shrinks over its lifetime, then self-destructs.
/// </summary>
public class ParticleBehaviour : MonoBehaviour
{
    private Vector2 _velocity;
    private float _damping;
    private float _lifeTime;
    private float _timer;
    private Vector3 _initialScale;

    /// <summary>
    /// Initialize the particle with velocity, damping, and lifetime.
    /// </summary>
    public void Init(Vector2 initialVelocity, float damping, float lifeTime)
    {
        _velocity = initialVelocity;
        _damping = damping;
        _lifeTime = lifeTime;
        _timer = lifeTime;
        _initialScale = transform.localScale;
    }

    private void Update()
    {
        // Move by velocity
        transform.position += (Vector3)(_velocity * Time.deltaTime);
        
        // Damp velocity over time
        _velocity = Vector2.Lerp(_velocity, Vector2.zero, _damping * Time.deltaTime);

        // Spin for visual effect
        transform.Rotate(0, 0, 360f * Time.deltaTime);

        // Shrink over lifetime (reaches zero at end)
        _timer -= Time.deltaTime;
        float progress = Mathf.Clamp01(_timer / _lifeTime);
        transform.localScale = _initialScale * progress;

        // Destroy when lifetime expires
        if (_timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
