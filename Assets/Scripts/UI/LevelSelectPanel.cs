using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Level select panel showing 5 level buttons with unlock-based availability.
/// Reads SaveSystem.GetMaxUnlockedLevel() on Init().
/// </summary>
public class LevelSelectPanel : MonoBehaviour
{
    private System.Action<int> _onLevelSelected;
    private System.Action _onBack;
    public void Init(System.Action<int> onLevelSelected, System.Action onBack = null)
    {
        _onLevelSelected = onLevelSelected;
        _onBack = onBack;

        Refresh();
        WireBackButton();
    }

    /// <summary>Refreshes the level button unlock states without re-wiring callbacks.</summary>
    public void Refresh()
    {
        // Clear old buttons from previous calls
        foreach (Transform child in transform)
            if (child.name.StartsWith("LevelBtn_")) Destroy(child.gameObject);

        BuildLevelButtons();
    }

    // Bind back button at runtime (Editor AddListener is not serialized)
    private void WireBackButton()
    {
        Transform t = transform.Find("BackBtn");
        if (t == null) return;

        Button btn = t.GetComponent<Button>();
        if (btn == null) return;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => _onBack?.Invoke());
    }

    private void BuildLevelButtons()
    {
        int maxUnlocked = SaveSystem.GetMaxUnlockedLevel();
        int total = LevelDatabase.TotalLevels;

        for (int i = 1; i <= total; i++)
        {
            LevelConfig cfg = LevelDatabase.Get(i);
            bool unlocked = (i <= maxUnlocked);
            CreateLevelButton(i, cfg, unlocked);
        }

        Debug.Log($"[LevelSelect] Built {total} buttons. Max unlocked: {maxUnlocked}");
    }

    private void CreateLevelButton(int levelIndex, LevelConfig cfg, bool unlocked)
    {
        // Container
        GameObject go = new GameObject($"LevelBtn_{levelIndex}", typeof(RectTransform));
        go.transform.SetParent(transform, false);

        RectTransform r = go.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(700, 80);
        r.anchoredPosition = new Vector2(0, 160f - (levelIndex - 1) * 95f);

        // Background
        Image bg = go.AddComponent<Image>();
        bg.color = unlocked
            ? new Color(0.15f, 0.3f, 0.5f, 0.9f)
            : new Color(0.15f, 0.15f, 0.15f, 0.7f);

        // Divider line next to level number
        GameObject divider = new GameObject("Divider", typeof(RectTransform));
        divider.transform.SetParent(go.transform, false);
        RectTransform rDiv = divider.GetComponent<RectTransform>();
        rDiv.sizeDelta = new Vector2(2, 52);
        rDiv.anchoredPosition = new Vector2(-265, 0);
        Image divImg = divider.AddComponent<Image>();
        divImg.color = unlocked
            ? new Color(0.3f, 0.55f, 0.8f, 0.55f)
            : new Color(0.3f, 0.3f, 0.3f, 0.4f);
        divImg.raycastTarget = false;

        // Button interaction
        Button btn = go.AddComponent<Button>();
        btn.targetGraphic = bg;
        btn.interactable = unlocked;

        if (unlocked)
        {
            ColorBlock cb = btn.colors;
            cb.highlightedColor = new Color(0.25f, 0.5f, 0.8f);
            cb.pressedColor = new Color(0.1f, 0.2f, 0.35f);
            btn.colors = cb;

            int captured = levelIndex;
            btn.onClick.AddListener(() => _onLevelSelected?.Invoke(captured));
        }

        // Level number (left column)
        GameObject numGO = new GameObject("Num", typeof(RectTransform));
        numGO.transform.SetParent(go.transform, false);
        RectTransform rNum = numGO.GetComponent<RectTransform>();
        rNum.sizeDelta = new Vector2(60, 60);
        rNum.anchoredPosition = new Vector2(-300, 0);
        Text numTxt = numGO.AddComponent<Text>();
        numTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        numTxt.text = $"L{levelIndex}";
        numTxt.fontSize = 26;
        numTxt.fontStyle = FontStyle.Bold;
        numTxt.color = unlocked ? new Color(0.55f, 0.85f, 1f) : new Color(0.4f, 0.4f, 0.4f);
        numTxt.alignment = TextAnchor.MiddleCenter;
        numTxt.raycastTarget = false;

        // Level name
        GameObject nameGO = new GameObject("Name", typeof(RectTransform));
        nameGO.transform.SetParent(go.transform, false);
        RectTransform rName = nameGO.GetComponent<RectTransform>();
        rName.sizeDelta = new Vector2(430, 34);
        rName.anchoredPosition = new Vector2(-42, 13);
        Text nameTxt = nameGO.AddComponent<Text>();
        nameTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameTxt.text = cfg.LevelName;
        nameTxt.fontSize = 22;
        nameTxt.fontStyle = FontStyle.Bold;
        nameTxt.color = unlocked ? Color.white : new Color(0.35f, 0.35f, 0.35f);
        nameTxt.alignment = TextAnchor.MiddleLeft;
        nameTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
        nameTxt.raycastTarget = false;

        // Subtitle
        GameObject subGO = new GameObject("Sub", typeof(RectTransform));
        subGO.transform.SetParent(go.transform, false);
        RectTransform rSub = subGO.GetComponent<RectTransform>();
        rSub.sizeDelta = new Vector2(430, 24);
        rSub.anchoredPosition = new Vector2(-42, -15);
        Text subTxt = subGO.AddComponent<Text>();
        subTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        subTxt.text = unlocked ? cfg.LevelSubtitle : "Complete previous level to unlock";
        subTxt.fontSize = 15;
        subTxt.color = unlocked ? new Color(0.65f, 0.82f, 1f) : new Color(0.35f, 0.35f, 0.35f);
        subTxt.alignment = TextAnchor.MiddleLeft;
        subTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
        subTxt.raycastTarget = false;

        // Boss tag
        if (cfg.HasBoss && unlocked)
        {
            GameObject bossGO = new GameObject("BossTag", typeof(RectTransform));
            bossGO.transform.SetParent(go.transform, false);
            RectTransform rBoss = bossGO.GetComponent<RectTransform>();
            rBoss.sizeDelta = new Vector2(100, 30);
            rBoss.anchoredPosition = new Vector2(265, 0);
            Text bossTxt = bossGO.AddComponent<Text>();
            bossTxt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            bossTxt.text = "BOSS";
            bossTxt.fontSize = 17;
            bossTxt.fontStyle = FontStyle.Bold;
            bossTxt.color = new Color(1f, 0.3f, 0.3f);
            bossTxt.alignment = TextAnchor.MiddleCenter;
            bossTxt.raycastTarget = false;
        }
    }
}
