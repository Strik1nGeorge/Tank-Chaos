using UnityEngine;

/// <summary>
/// A wall tile that can be destroyed. Has 1 HP — any hit destroys it.
/// Destroyed walls open new paths on the map.
/// </summary>
public class DestructibleWall : MonoBehaviour
{
    private int _hp = 1;

    /// <summary>Reduces HP and destroys the wall when it reaches zero.</summary>
    public void TakeDamage(int amount)
    {
        _hp -= amount;
        if (_hp <= 0)
        {
            Destroy(gameObject);
        }
    }
}
