using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Leaderboard panel showing top scores for each of the 5 levels with tab navigation.
/// References are set via Inspector or resolved at runtime as fallback.
/// </summary>
public class LeaderboardPanel : MonoBehaviour
{
    [Header("UI  (Inspector ?")]
    [SerializeField] private Transform _entriesContainer;
    [SerializeField] private Button    _backButton;

    private int _selectedLevel = 1;
    private Button[] _tabButtons = new Button[5];

    public void Init()
    {
        // Fallback bindings
        if (_backButton == null)
        {
            Transform t = transform.Find("BoardBg/BackBtn");
            if (t != null) _backButton = t.GetComponent<Button>();
        }

        if (_entriesContainer == null)
        {
            Transform t = transform.Find("BoardBg/EntriesContainer");
            if (t != null) _entriesContainer = t;
        }

        _backButton?.onClick.AddListener(() => gameObject.SetActive(false));

        BuildTabButtons();
        Debug.Log("[Leaderboard] Init done");
    }

    private void OnEnable()
    {
        transform.SetAsLastSibling();
        RefreshEntries(_selectedLevel);
    }

    // --- Tab buttons ---

    private void BuildTabButtons()
    {
        // Find tab row container
        Transform tabRow = transform.Find("BoardBg/TabRow");
        if (tabRow == null) return;

        // Clear existing tabs
        foreach (Transform child in tabRow) Destroy(child.gameObject);

        for (int i = 0; i < 5; i++)
        {
            int levelIdx = i + 1;
            GameObject go = new GameObject($"Tab_{levelIdx}", typeof(RectTransform));
            go.transform.SetParent(tabRow, false);

            RectTransform r = go.GetComponent<RectTransform>();
            r.sizeDelta = new Vector2(130, 45);
            r.anchoredPosition = new Vector2(-260 + i * 130, 0);

            Image bg = go.AddComponent<Image>();
            bg.color = (levelIdx == _selectedLevel)
                ? new Color(0.3f, 0.55f, 0.85f)
                : new Color(0.12f, 0.12f, 0.15f);

            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = bg;

            GameObject textGO = new GameObject("Label", typeof(RectTransform));
            textGO.transform.SetParent(go.transform, false);
            RectTransform rText = textGO.GetComponent<RectTransform>();
            rText.sizeDelta = new Vector2(130, 45);
            rText.anchoredPosition = Vector2.zero;
            Text t = textGO.AddComponent<Text>();
            t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text      = $"L{levelIdx}";
            t.fontSize  = 20;
            t.fontStyle = FontStyle.Bold;
            t.color     = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.raycastTarget = false;

            _tabButtons[i] = btn;
            btn.onClick.AddListener(() =>
            {
                _selectedLevel = levelIdx;
                UpdateTabColors(tabRow);
                RefreshEntries(levelIdx);
            });
        }
    }

    private void UpdateTabColors(Transform tabRow)
    {
        int i = 0;
        foreach (Transform tab in tabRow)
        {
            Image img = tab.GetComponent<Image>();
            if (img != null)
                img.color = (i + 1 == _selectedLevel)
                    ? new Color(0.3f, 0.55f, 0.85f)
                    : new Color(0.12f, 0.12f, 0.15f);
            i++;
        }
    }

    // --- Entry rows ---

    private void RefreshEntries(int levelIndex)
    {
        if (_entriesContainer == null) return;

        foreach (Transform child in _entriesContainer) Destroy(child.gameObject);

        List<LeaderboardEntry> entries = SaveSystem.LoadLeaderboard(levelIndex);

        if (entries.Count == 0)
        {
            CreateRow(_entriesContainer, 0, "---", "No records yet!", "---", "---", "---");
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            LeaderboardEntry e = entries[i];
            CreateRow(_entriesContainer, i,
                $"#{i + 1}",
                e.PlayerName,
                e.Score.ToString(),
                $"{e.ClearTime:F1}s",
                e.DateString);
        }
    }

    private void CreateRow(Transform parent, int rank, string rankStr, string name, string score, string time, string date)
    {
        GameObject row = new GameObject($"Row_{rank}", typeof(RectTransform));
        row.transform.SetParent(parent, false);

        RectTransform r = row.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 1f);
        r.pivot = new Vector2(0.5f, 1f);
        r.sizeDelta = new Vector2(940, 45);
        r.anchoredPosition = new Vector2(0, -rank * 48f);

        // Alternating row background
        Image bg = row.AddComponent<Image>();
        bg.color = (rank % 2 == 0)
            ? new Color(0.1f, 0.12f, 0.15f, 0.9f)
            : new Color(0.07f, 0.09f, 0.12f, 0.9f);
        bg.raycastTarget = false;

        // Gold / silver / bronze rank colors
        Color rankColor = rank == 0 ? new Color(1f, 0.85f, 0.2f)
                        : rank == 1 ? new Color(0.75f, 0.75f, 0.75f)
                        : rank == 2 ? new Color(0.8f, 0.5f, 0.25f)
                        : Color.white;

        // X Positions matched to UISceneBuilder: { -370f, -170f, 60f, 220f, 385f }
        CreateCell(row.transform, "Rank",  rankStr, -370f, 80,  rankColor);
        CreateCell(row.transform, "Name",  name,    -170f, 240, Color.white);
        CreateCell(row.transform, "Time",  time,      60f, 120, new Color(0.6f, 0.9f, 1f));
        CreateCell(row.transform, "Score", score,    220f, 120, new Color(1f, 0.9f, 0.4f));

        // Date column
        CreateCell(row.transform, "Date",  date,     385f, 160, new Color(0.6f, 0.6f, 0.6f));
    }

    private void CreateCell(Transform parent, string id, string text, float x, float width, Color color)
    {
        GameObject go = new GameObject(id, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(width, 40);
        r.anchoredPosition = new Vector2(x, 0);
        Text t = go.AddComponent<Text>();
        t.font      = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.text      = text;
        t.fontSize  = 18;
        t.color     = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
    }
}
