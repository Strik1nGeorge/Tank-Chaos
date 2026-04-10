using UnityEngine;

/// <summary>
/// Grants a temporary shield (invincibility) to the player upon collection.
/// </summary>
public class ShieldPickup : PickupBase
{
    protected override void ApplyEffect(PlayerHealth health, PlayerController controller)
    {
        // Grant a 5-second shield
        health.GrantShield(5f);
    }
}
