using UnityEngine;

/// <summary>
/// Damage-boost pickup: increases cannon damage to x3 for 5 seconds.
/// </summary>
public class DamageBoostPickup : PickupBase
{
    protected override void ApplyEffect(PlayerHealth health, PlayerController controller)
    {
        controller.ActivateDamageBoost(5f);
        Debug.Log("[Pickup] Damage boost activated");
    }
}
