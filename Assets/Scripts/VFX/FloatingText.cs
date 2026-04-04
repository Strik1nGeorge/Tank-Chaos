using UnityEngine;

/// <summary>
/// Floating text using 3D TextMesh. Rises upward and fades out over its lifetime.
/// </summary>
[RequireComponent(typeof(TextMesh))]
public class FloatingText : MonoBehaviour
{
    private TextMesh _tm;
    private float _lifetime;
    private float _timer;
    private float _floatSpeed;
    private Color _origColor;

    public void Init(string text, Color color, float lifetime = 1.0f, float speed = 1.5f)
    {
        _tm = GetComponent<TextMesh>();
        
        // Configure for 2D display
        _tm.anchor = TextAnchor.MiddleCenter;
        _tm.alignment = TextAlignment.Center;
        _tm.characterSize = 0.05f; 
        _tm.fontSize = 80; // Large font size for crisp text at small characterSize
        _tm.text = text;
        
        // Render above other sprites
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.sortingOrder = 20;

        _tm.color = color;
        _origColor = color;
        
        _lifetime = lifetime;
        _timer = lifetime;
        _floatSpeed = speed;

        // Slight random offset for variety
        transform.position += new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.1f, 0.1f), 0f);
    }

    private void Update()
    {
        if (_tm == null) return;

        _timer -= Time.deltaTime;
        
        // Float upward
        transform.Translate(Vector3.up * _floatSpeed * Time.deltaTime);

        // Fade out alpha over the last 50% of lifetime
        float alpha = Mathf.Clamp01(_timer / (_lifetime * 0.5f));
        _tm.color = new Color(_origColor.r, _origColor.g, _origColor.b, alpha * _origColor.a);

        if (_timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
