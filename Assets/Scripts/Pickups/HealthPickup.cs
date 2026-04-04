using UnityEngine;

/// <summary>
/// Health pickup: restores 1 HP to the player on contact.
/// </summary>
public class HealthPickup : PickupBase
{
    protected override void ApplyEffect(PlayerHealth health, PlayerController controller)
    {
        health.Heal(1);
        Debug.Log("[Pickup] ?1 ");
    }
}
