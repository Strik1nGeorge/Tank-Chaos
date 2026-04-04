using UnityEngine;

/// <summary>
/// Abstract base class for all pickups.
/// Auto-destroys after a lifetime and triggers on player contact.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public abstract class PickupBase : MonoBehaviour
{
    private const float LIFETIME = 30f; // Despawn after 30 seconds
    private float _lifetimeTimer;

    protected virtual void Awake()
    {
        _lifetimeTimer = LIFETIME;

        // Ensure collider is a trigger
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    protected virtual void Update()
    {
        // Skip while paused
        if (Time.timeScale == 0f) return;

        _lifetimeTimer -= Time.deltaTime;
        if (_lifetimeTimer <= 0f)
        {
            Debug.Log($"[Pickup] ? {gameObject.name}");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            PlayerController controller = other.GetComponent<PlayerController>();

            if (health != null && controller != null)
            {
                ApplyEffect(health, controller);
                AudioManager.Instance?.PlayPowerupSound();
                Debug.Log($"[Pickup] ? {gameObject.name}");
                
                // Notify spawner so it can track active count
                PickupSpawner.Instance?.OnPickupCollected();
                
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Apply the pickup's effect to the player. Implemented by subclasses.
    /// </summary>
    protected abstract void ApplyEffect(PlayerHealth health, PlayerController controller);
}
