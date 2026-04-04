using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Garage panel for customizing tank skin colors (body, tracks, barrel).
/// References are resolved via Find in Init(); panel starts inactive.
/// </summary>
public class GaragePanel : MonoBehaviour
{
    [Header("UI  (Inspector ?")]
    [SerializeField] private Image  _previewBody;
    [SerializeField] private Image  _previewBarrel;
    [SerializeField] private Image  _previewTrackL;
    [SerializeField] private Image  _previewTrackR;
    [SerializeField] private Button _saveButton;

    [Header(" (Inspector ?")]
    [SerializeField] private RectTransform _bodyPaletteRow;
    [SerializeField] private RectTransform _tracksPaletteRow;
    [SerializeField] private RectTransform _barrelPaletteRow;

    private int _currentBodyIndex;
    private int _currentTrackIndex;
    private int _currentBarrelIndex;

    private System.Action _onClose;

    public void Init(System.Action onClose)
    {
        _onClose = onClose;

        if (_previewBody == null)
            _previewBody        = FindImage("TankPreview/Body");
        if (_previewTrackL == null)      _previewTrackL      = FindImage("TankPreview/TrackL");
        if (_previewTrackR == null)      _previewTrackR      = FindImage("TankPreview/TrackR");
        if (_previewBarrel == null)      _previewBarrel      = FindImage("TankPreview/Barrel");
        if (_saveButton == null)         _saveButton         = FindButton("SaveBtn");
        if (_bodyPaletteRow == null)     _bodyPaletteRow     = FindRect("BodyPaletteRow");
        if (_tracksPaletteRow == null)   _tracksPaletteRow   = FindRect("TracksPaletteRow");
        if (_barrelPaletteRow == null)   _barrelPaletteRow   = FindRect("BarrelPaletteRow");

        Debug.Log($"[Garage] Init: body={_previewBody != null}, save={_saveButton != null}, " +
                  $"bodyRow={_bodyPaletteRow != null}, tracksRow={_tracksPaletteRow != null}, barrelRow={_barrelPaletteRow != null}");

        // Load saved skin indices
        _currentBodyIndex   = SkinManager.GetBodyIndex();
        _currentTrackIndex  = SkinManager.GetTrackIndex();
        _currentBarrelIndex = SkinManager.GetBarrelIndex();
        UpdatePreviewColors();

        // Create color palette buttons
        GeneratePaletteButtons(_bodyPaletteRow, (idx) => { _currentBodyIndex = idx; UpdatePreviewColors(); });
        GeneratePaletteButtons(_tracksPaletteRow, (idx) => { _currentTrackIndex = idx; UpdatePreviewColors(); });
        GeneratePaletteButtons(_barrelPaletteRow, (idx) => { _currentBarrelIndex = idx; UpdatePreviewColors(); });

        // Save and close
        _saveButton?.onClick.AddListener(() =>
        {
            SkinManager.SaveSkin(_currentBodyIndex, _currentTrackIndex, _currentBarrelIndex);

            PlayerController pc = FindObjectOfType<PlayerController>();
            if (pc != null) pc.ApplySkin();

            _onClose?.Invoke();
        });
    }

    private void GeneratePaletteButtons(RectTransform container, System.Action<int> onSelectColor)
    {
        if (container == null) return;

        // Clear existing buttons
        foreach (Transform child in container) Destroy(child.gameObject);

        int colorCount = SkinManager.Palette.Length;
        float startX = -150f;
        float spacing = 60f;

        for (int i = 0; i < colorCount; i++)
        {
            int cachedIndex = i;
            Color colorHex = SkinManager.Palette[i];

            GameObject cBtn = new GameObject($"Color_{i}", typeof(RectTransform));
            cBtn.transform.SetParent(container, false);

            RectTransform r = cBtn.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(40, 40);
            r.anchoredPosition = new Vector2(startX + (i * spacing), 0);

            Image img = cBtn.AddComponent<Image>();
            img.color = colorHex;

            Button btn = cBtn.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => { onSelectColor?.Invoke(cachedIndex); });
        }
    }

    private void UpdatePreviewColors()
    {
        if (_previewBody != null)   _previewBody.color   = SkinManager.Palette[_currentBodyIndex];
        if (_previewTrackL != null)  _previewTrackL.color = SkinManager.Palette[_currentTrackIndex];
        if (_previewTrackR != null)  _previewTrackR.color = SkinManager.Palette[_currentTrackIndex];
        if (_previewBarrel != null)  _previewBarrel.color = SkinManager.Palette[_currentBarrelIndex];
    }

    private Image FindImage(string path)
    {
        Transform t = transform.Find(path);
        return t != null ? t.GetComponent<Image>() : null;
    }

    private Button FindButton(string name)
    {
        Transform t = transform.Find(name);
        return t != null ? t.GetComponent<Button>() : null;
    }

    private RectTransform FindRect(string name)
    {
        Transform t = transform.Find(name);
        return t != null ? t.GetComponent<RectTransform>() : null;
    }
}
