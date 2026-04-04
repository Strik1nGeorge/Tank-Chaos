using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

/// <summary>
/// Static helper for building UI elements in code without the Inspector.
/// </summary>
public static class UIBuilder
{
    private static Font _defaultFont;
    public static Font DefaultFont
    {
        get
        {
            if (_defaultFont == null)
            {
                _defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            return _defaultFont;
        }
    }

    private static Sprite _whiteSprite;
    public static Sprite WhiteSprite
    {
        get
        {
            if (_whiteSprite == null)
            {
                Texture2D tex = new Texture2D(2, 2);
                tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
                tex.Apply();
                _whiteSprite = Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f));
            }
            return _whiteSprite;
        }
    }

    /// <summary>Ensures an EventSystem exists for UI input.</summary>
    public static void EnsureEventSystem()
    {
        if (Object.FindAnyObjectByType<EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }
    }

    /// <summary>Creates a screen-space overlay Canvas.</summary>
    public static GameObject CreateCanvas(string name, int sortOrder = 10)
    {
        GameObject canvasGO = new GameObject(name, typeof(RectTransform));
        
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        return canvasGO;
    }

    /// <summary>Creates a full-screen panel with a background color.</summary>
    public static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.sprite = WhiteSprite;
        img.color = color;

        return panel;
    }

    /// <summary>Creates a text element.</summary>
    public static GameObject CreateText(Transform parent, string name, string content, int fontSize, Color color, Vector2 anchoredPosition)
    {
        GameObject textGO = new GameObject(name, typeof(RectTransform));
        textGO.transform.SetParent(parent, false);

        RectTransform rect = textGO.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(800, 200);
        rect.anchoredPosition = anchoredPosition;

        Text text = textGO.AddComponent<Text>();
        text.font = DefaultFont;
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;

        return textGO;
    }

    /// <summary>Creates a button with a label.</summary>
    public static GameObject CreateButton(Transform parent, string name, string labelText, Vector2 anchoredPosition, Vector2 size, UnityAction onClickAction)
    {
        GameObject btnGO = new GameObject(name, typeof(RectTransform));
        btnGO.transform.SetParent(parent, false);

        RectTransform rect = btnGO.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        // Background image
        Image img = btnGO.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
        // Button component
        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = img;
        if (onClickAction != null)
        {
            btn.onClick.AddListener(onClickAction);
        }

        // Label text child
        GameObject textGO = new GameObject("Label", typeof(RectTransform));
        textGO.transform.SetParent(btnGO.transform, false);
        
        RectTransform txtRect = textGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero;
        txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = Vector2.zero;
        txtRect.offsetMax = Vector2.zero;

        Text text = textGO.AddComponent<Text>();
        text.font = DefaultFont;
        text.text = labelText;
        text.fontSize = (int)(size.y * 0.4f);
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;

        return btnGO;
    }

    /// <summary>Creates an input field with placeholder text.</summary>
    public static GameObject CreateInputField(Transform parent, string name, string placeholderText, Vector2 anchoredPosition, Vector2 size)
    {
        // 1. Root GameObject
        GameObject inputGO = new GameObject(name, typeof(RectTransform));
        inputGO.transform.SetParent(parent, false);

        RectTransform rect = inputGO.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        // Background
        Image bgImg = inputGO.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // 2. Placeholder text
        GameObject placeholderGO = CreateText(inputGO.transform, "Placeholder", placeholderText, 30, new Color(0.7f, 0.7f, 0.7f, 0.5f), Vector2.zero);
        RectTransform placeRect = placeholderGO.GetComponent<RectTransform>();
        placeRect.anchorMin = Vector2.zero; placeRect.anchorMax = Vector2.one;
        placeRect.offsetMin = new Vector2(10, 0); placeRect.offsetMax = new Vector2(-10, 0);
        Text phText = placeholderGO.GetComponent<Text>();
        phText.alignment = TextAnchor.MiddleLeft;

        // 3. Actual input text
        GameObject textGO = CreateText(inputGO.transform, "Text", "", 30, Color.white, Vector2.zero);
        RectTransform txtRect = textGO.GetComponent<RectTransform>();
        txtRect.anchorMin = Vector2.zero; txtRect.anchorMax = Vector2.one;
        txtRect.offsetMin = new Vector2(10, 0); txtRect.offsetMax = new Vector2(-10, 0);
        Text actualText = textGO.GetComponent<Text>();
        actualText.alignment = TextAnchor.MiddleLeft;

        // 4. InputField component
        InputField inputField = inputGO.AddComponent<InputField>();
        inputField.textComponent = actualText;
        inputField.placeholder   = phText;
        
        return inputGO;
    }
}
