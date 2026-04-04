using UnityEngine;

/// <summary>
/// Drop shadow effect. Creates a child SpriteRenderer that mirrors the parent sprite.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class DropShadow : MonoBehaviour
{
    public Vector3 Offset = new Vector3(0.15f, -0.15f, 0f); // Shadow offset
    public Color ShadowColor = new Color(0f, 0f, 0f, 0.35f);

    private SpriteRenderer _targetSr;
    private SpriteRenderer _shadowSr;
    private Transform _shadowTransform;

    private void Start() // Use Start so SkinManager can apply sprites first
    {
        _targetSr = GetComponent<SpriteRenderer>();
        if (_targetSr == null || _targetSr.sprite == null) return;

        // Create shadow child object
        GameObject shadowGO = new GameObject("DropShadow");
        _shadowTransform = shadowGO.transform;
        
        // Parent to this transform
        _shadowTransform.SetParent(transform, false);

        _shadowSr = shadowGO.AddComponent<SpriteRenderer>();
        _shadowSr.sprite = _targetSr.sprite;
        _shadowSr.color = ShadowColor;
        _shadowSr.sortingOrder = _targetSr.sortingOrder - 1; // Render behind main sprite
        }

    private void LateUpdate()
    {
        if (_targetSr == null || _shadowSr == null) return;
        
        // 1. Sync sprite if it changed (e.g. skin swap)
        if (_shadowSr.sprite != _targetSr.sprite)
            _shadowSr.sprite = _targetSr.sprite;

        // 2. Sync rotation and apply offset
        _shadowTransform.rotation = _targetSr.transform.rotation;
        _shadowTransform.position = transform.position + Offset;
    }
}
