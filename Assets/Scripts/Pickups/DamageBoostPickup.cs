using UnityEngine;

/// <summary>
/// Damage-boost pickup: doubles player damage for 5 seconds.
/// </summary>
public class DamageBoostPickup : PickupBase
{
    protected override void ApplyEffect(PlayerHealth health, PlayerController controller)
    {
        controller.ActivateDamageBoost(5f);
        Debug.Log("[Pickup] ");
    }
}
