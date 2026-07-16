using UnityEngine;

/// <summary>
/// Grants a temporary laser weapon (piercing, high damage) to the player upon collection.
/// </summary>
public class LaserPickup : PickupBase
{
    protected override void ApplyEffect(PlayerHealth health, PlayerController controller)
    {
        // Grant a 5-second laser weapon
        controller.ActivateLaser(5f);
    }
}
