using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Editor tool: builds the full game UI hierarchy in one click.
/// </summary>
public class UISceneBuilder
{
    private static Font _font;
    private static Sprite _whiteSprite;

    private static Font GetFont()
    {
        if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return _font;
    }

    private static Sprite GetWhiteSprite()
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

    [MenuItem("Tools/Game Tools/Build UI Scene")]
    public static void BuildUIScene()
    {
        // Prevent duplicate creation
        if (GameObject.Find("GameCanvas") != null)
        {
            Debug.LogWarning("[UISceneBuilder] GameCanvas already exists");
            return;
        }

        // -----------------------------------------------
        // Root Canvas
        // -----------------------------------------------
        GameObject canvasGO = new GameObject("GameCanvas", typeof(RectTransform));
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Attach UIManager script
        canvasGO.AddComponent<UIManager>();

        // -----------------------------------------------
        // 1. MainMenuPanel
        // -----------------------------------------------
        GameObject mainMenu = CreateFullPanel(canvasGO.transform, "MainMenuPanel");
        mainMenu.AddComponent<MainMenuPanel>();

        CreateImage(mainMenu.transform, "Background", Fill(), new Color(0.05f, 0.08f, 0.1f, 0.85f));

        GameObject titleText = CreateText(mainMenu.transform, "TitleText", "TANK CHAOS 2D", 120, new Color(0.2f, 0.9f, 0.3f), new Vector2(0, 260));
        Outline outline = titleText.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.8f);
        outline.effectDistance = new Vector2(6, -6);

        CreateButton(mainMenu.transform, "StartButton",    "START GAME",   new Vector2(0, -10),  new Vector2(400, 70), new Color(0.15f, 0.5f, 0.2f));
        CreateButton(mainMenu.transform, "GarageBtn",      "GARAGE",       new Vector2(0, -90),  new Vector2(400, 70), new Color(0.2f, 0.45f, 0.7f));
        CreateButton(mainMenu.transform, "LeaderboardBtn", "LEADERBOARDS", new Vector2(0, -170), new Vector2(400, 70), new Color(0.6f, 0.4f, 0.1f));
        CreateButton(mainMenu.transform, "SettingsBtn",    "SETTINGS",     new Vector2(0, -250), new Vector2(400, 70), new Color(0.4f, 0.4f, 0.5f));
        CreateButton(mainMenu.transform, "QuitButton",     "QUIT TO OS",   new Vector2(0, -330), new Vector2(400, 70), new Color(0.6f, 0.15f, 0.15f));

        // -----------------------------------------------
        // 2. LevelSelectPanel
        // -----------------------------------------------
        GameObject levelSelect = CreateFullPanel(canvasGO.transform, "LevelSelectPanel");
        levelSelect.AddComponent<LevelSelectPanel>();

        CreateImage(levelSelect.transform, "Background", Fill(), new Color(0.04f, 0.06f, 0.1f, 0.97f));

        // Title
        GameObject lsTitle = CreateText(levelSelect.transform, "TitleText", "SELECT LEVEL", 70, new Color(0.3f, 0.85f, 1f), new Vector2(0, 270));
        lsTitle.GetComponent<Text>().fontStyle = FontStyle.Bold;

        // Level button container (LevelSelectPanel script fills buttons at runtime)
        CreateText(levelSelect.transform, "SubText", "Complete each level to unlock the next", 20, new Color(0.5f, 0.65f, 0.8f), new Vector2(0, 220));

        // Back to main menu button
        GameObject lsBackBtn = CreateButton(levelSelect.transform, "BackBtn", "< MAIN MENU", new Vector2(0, -300), new Vector2(300, 55), new Color(0.3f, 0.3f, 0.4f));
        lsBackBtn.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => GameManager.Instance?.ReloadGame());

        levelSelect.SetActive(false);


        // -----------------------------------------------
        // 3. PauseMenuPanel
        // -----------------------------------------------
        GameObject pauseMenu = CreateFullPanel(canvasGO.transform, "PauseMenuPanel");
        pauseMenu.AddComponent<PauseMenuPanel>();

        CreateImage(pauseMenu.transform, "Background", Fill(), new Color(0, 0, 0, 0.7f));
        CreateText(pauseMenu.transform, "TitleText", "GAME PAUSED", 60, Color.white, new Vector2(0, 150));
        CreateButton(pauseMenu.transform, "ResumeButton", "RESUME", new Vector2(0, 20), new Vector2(300, 60), new Color(0.15f, 0.5f, 0.2f));
        CreateButton(pauseMenu.transform, "RestartButton", "RESTART LEVEL", new Vector2(0, -60), new Vector2(300, 60), new Color(0.3f, 0.35f, 0.5f));
        CreateButton(pauseMenu.transform, "QuitButton", "MAIN MENU", new Vector2(0, -140), new Vector2(300, 60), new Color(0.4f, 0.2f, 0.2f));

        pauseMenu.SetActive(false);

        // -----------------------------------------------
        // 4. HUDPanel
        // -----------------------------------------------
        GameObject hud = CreateFullPanel(canvasGO.transform, "HUDPanel");
        hud.AddComponent<HUDPanel>();

        // Top bar
        GameObject topBar = CreateImage(hud.transform, "TopBar", AnchorTop(60), new Color(0, 0, 0, 0.5f));
        CreateText(topBar.transform, "HealthText", "LIVES: 10 | HP: 3", 30, Color.red, Anchored(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(300, 60)), TextAnchor.MiddleLeft);
        CreateText(topBar.transform, "TimeText", "03:00", 36, Color.white, Centered(new Vector2(200, 60)));
        CreateText(topBar.transform, "ScoreText", "SCORE: 0", 30, Color.yellow, Anchored(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-20, 0), new Vector2(300, 60)), TextAnchor.MiddleRight);

        // Bottom-left skill container
        GameObject skillContainer = CreateEmpty(hud.transform, "SkillContainer");
        RectTransform scRect = skillContainer.GetComponent<RectTransform>();
        scRect.anchorMin = scRect.anchorMax = scRect.pivot = Vector2.zero;
        scRect.anchoredPosition = new Vector2(25, 25);

        // Dash Slot
        GameObject dashBorder = CreateImage(skillContainer.transform, "DashBorder", BottomLeft(104, 104, new Vector2(-2, -2)), new Color(0f, 0.5f, 1f, 0.4f));
        GameObject dashBg = CreateImage(dashBorder.transform, "DashBg", Centered(new Vector2(100, 100)), new Color(0.08f, 0.08f, 0.1f, 0.95f));
        CreateText(dashBg.transform, "KeyText", "[SHIFT]", 14, Color.gray, new Vector2(0, 30));
        GameObject dashIcon = CreateText(dashBg.transform, "IconText", ">>", 28, new Color(0f, 0.8f, 1f), Vector2.zero);
        dashIcon.GetComponent<Text>().fontStyle = FontStyle.Bold;
        CreateText(dashBg.transform, "NameText", "DASH", 16, Color.white, new Vector2(0, -30));

        GameObject dashMask = CreateImage(dashBg.transform, "DashMask", Fill(), new Color(0, 0, 0, 0.85f));
        Image dashMaskImg = dashMask.GetComponent<Image>();
        dashMaskImg.type = Image.Type.Filled;
        dashMaskImg.fillMethod = Image.FillMethod.Radial360;
        dashMaskImg.fillAmount = 0;
        GameObject dashTimer = CreateText(dashMask.transform, "DashTimerText", "", 28, Color.white, Vector2.zero);
        dashTimer.GetComponent<Text>().fontStyle = FontStyle.Bold;

        // Weapon Slot
        GameObject weaponBorder = CreateImage(skillContainer.transform, "WeaponBorder", BottomLeft(124, 104, new Vector2(118, -2)), new Color(1f, 0.6f, 0f, 0.4f));
        GameObject weaponBg = CreateImage(weaponBorder.transform, "WeaponBg", Centered(new Vector2(120, 100)), new Color(0.08f, 0.08f, 0.1f, 0.95f));
        CreateText(weaponBg.transform, "KeyText", "[RMB]", 14, Color.gray, new Vector2(0, 30));
        GameObject weaponLabel = CreateText(weaponBg.transform, "WeaponLabel", "CANNON", 20, Color.yellow, new Vector2(0, -10));
        weaponLabel.GetComponent<Text>().fontStyle = FontStyle.Bold;

        GameObject weaponMask = CreateImage(weaponBg.transform, "WeaponMask", Fill(), new Color(0, 0, 0, 0.75f));
        Image weaponMaskImg = weaponMask.GetComponent<Image>();
        weaponMaskImg.type = Image.Type.Filled;
        weaponMaskImg.fillMethod = Image.FillMethod.Radial360;
        weaponMaskImg.fillAmount = 0;
        GameObject weaponTimer = CreateText(weaponMask.transform, "WeaponTimerText", "", 28, Color.white, Vector2.zero);
        weaponTimer.GetComponent<Text>().fontStyle = FontStyle.Bold;

        hud.SetActive(false);

        // -----------------------------------------------
        // 5. ScreenMessagePanel
        // -----------------------------------------------
        GameObject msgPanel = CreateFullPanel(canvasGO.transform, "ScreenMessagePanel");
        msgPanel.AddComponent<ScreenMessagePanel>();

        CreateImage(msgPanel.transform, "Background", Fill(), new Color(0, 0, 0, 0.85f));
        
        // Text elements (positioned top to bottom)
        CreateText(msgPanel.transform, "TitleText", "MESSAGE", 80, Color.white, new Vector2(0, 280));
        CreateText(msgPanel.transform, "SubtitleText", "", 36, Color.yellow, new Vector2(0, 200));
        CreateText(msgPanel.transform, "BodyText", "", 24, new Color(0.85f, 0.85f, 0.85f), new Vector2(0, 130));

        // Victory-specific interaction area (Input + Save)
        CreateInputField(msgPanel.transform, "NameInput", "ENTER YOUR NAME...", new Vector2(0, 45), new Vector2(400, 60));
        CreateButton(msgPanel.transform, "SaveBtn", "SAVE PERFORMANCE", new Vector2(0, -25), new Vector2(400, 60), new Color(0.1f, 0.4f, 0.15f));

        // Bottom flow buttons
        CreateButton(msgPanel.transform, "NextLevelBtn", "NEXT LEVEL", new Vector2(0, -100), new Vector2(360, 60), new Color(0.15f, 0.5f, 0.2f));
        CreateButton(msgPanel.transform, "RetryBtn", "TRY AGAIN", new Vector2(0, -170), new Vector2(360, 60), new Color(0.3f, 0.35f, 0.5f));
        CreateButton(msgPanel.transform, "RestartBtn", "MAIN MENU", new Vector2(0, -170), new Vector2(360, 60), new Color(0.25f, 0.25f, 0.3f));
        CreateButton(msgPanel.transform, "QuitBtn", "QUIT TO DESKTOP", new Vector2(0, -240), new Vector2(360, 60), new Color(0.5f, 0.15f, 0.15f));

        msgPanel.SetActive(false);


        // -----------------------------------------------
        // 6. LeaderboardPanel
        // -----------------------------------------------
        GameObject lbPanel = CreateFullPanel(canvasGO.transform, "LeaderboardPanel");
        lbPanel.AddComponent<LeaderboardPanel>();

        // Main background (deep blue)
        CreateImage(lbPanel.transform, "Background", Fill(), new Color(0.04f, 0.05f, 0.08f, 1f));
        
        // Decorative background glow
        GameObject bgGlow = CreateImage(lbPanel.transform, "Glow", Centered(new Vector2(1000, 800)), new Color(0.1f, 0.3f, 0.6f, 0.15f));
        
        // Main board panel (glass feel)
        GameObject boardBg = CreateImage(lbPanel.transform, "BoardBg", Centered(new Vector2(960, 720)), new Color(0.07f, 0.09f, 0.13f, 0.95f));
        
        // Neon border (orange glow)
        GameObject border = CreateImage(boardBg.transform, "NeonBorder", Centered(new Vector2(964, 724)), new Color(1f, 0.5f, 0f, 0.3f));
        border.transform.SetAsFirstSibling();
        
        // L-shaped corner decorations
        float cw = 30f, ct = 4f;
        CreateImage(boardBg.transform, "Corner_TL_H", Centered(new Vector2(cw, ct), new Vector2(-480 + cw/2, 360 - ct/2)), new Color(1f, 0.55f, 0f, 0.8f));
        CreateImage(boardBg.transform, "Corner_TL_V", Centered(new Vector2(ct, cw), new Vector2(-480 + ct/2, 360 - cw/2)), new Color(1f, 0.55f, 0f, 0.8f));
        
        CreateImage(boardBg.transform, "Corner_TR_H", Centered(new Vector2(cw, ct), new Vector2(480 - cw/2, 360 - ct/2)), new Color(1f, 0.55f, 0f, 0.8f));
        CreateImage(boardBg.transform, "Corner_TR_V", Centered(new Vector2(ct, cw), new Vector2(480 - ct/2, 360 - cw/2)), new Color(1f, 0.55f, 0f, 0.8f));
        
        CreateImage(boardBg.transform, "Corner_BL_H", Centered(new Vector2(cw, ct), new Vector2(-480 + cw/2, -360 + ct/2)), new Color(1f, 0.55f, 0f, 0.8f));
        CreateImage(boardBg.transform, "Corner_BL_V", Centered(new Vector2(ct, cw), new Vector2(-480 + ct/2, -360 + cw/2)), new Color(1f, 0.55f, 0f, 0.8f));
        
        CreateImage(boardBg.transform, "Corner_BR_H", Centered(new Vector2(cw, ct), new Vector2(480 - cw/2, -360 + ct/2)), new Color(1f, 0.55f, 0f, 0.8f));
        CreateImage(boardBg.transform, "Corner_BR_V", Centered(new Vector2(ct, cw), new Vector2(480 - ct/2, -360 + cw/2)), new Color(1f, 0.55f, 0f, 0.8f));

        // Top horizontal divider line
        CreateImage(boardBg.transform, "TopLine", Centered(new Vector2(960, 2), new Vector2(0, 350)), new Color(1f, 0.5f, 0f, 0.5f));

        GameObject lbTitle = CreateText(boardBg.transform, "TitleText", "HALL OF FAME", 50, new Color(1f, 0.85f, 0.3f), new Vector2(0, 315));
        lbTitle.GetComponent<Text>().fontStyle = FontStyle.Bold;

        // Table header (X positions adjusted for full date column)
        float[] xPos = { -370f, -170f, 60f, 220f, 385f };
        string[] headers = { "RANK", "PLAYER", "TIME", "SCORE", "DATE" };
        for (int i = 0; i < 5; i++)
        {
            GameObject h = CreateText(boardBg.transform, $"Header_{headers[i]}", headers[i], 20, new Color(0.5f, 0.5f, 0.5f), new Vector2(xPos[i], 195));
            RectTransform hr = h.GetComponent<RectTransform>();
            hr.sizeDelta = new Vector2(150, 40);
            h.GetComponent<Text>().fontStyle = FontStyle.Bold;
        }

        // Level tab row (script fills tab buttons at runtime)
        GameObject tabRow = CreateEmpty(boardBg.transform, "TabRow");
        RectTransform tabRect = tabRow.GetComponent<RectTransform>();
        tabRect.anchorMin = tabRect.anchorMax = new Vector2(0.5f, 1f);
        tabRect.pivot = new Vector2(0.5f, 1f);
        tabRect.sizeDelta = new Vector2(700, 50);
        tabRect.anchoredPosition = new Vector2(0, -90);

        // Data rows container (Top-Center anchor so list follows header)
        GameObject entriesContainer = CreateEmpty(boardBg.transform, "EntriesContainer");
        RectTransform ecRect = entriesContainer.GetComponent<RectTransform>();
        ecRect.anchorMin = ecRect.anchorMax = new Vector2(0.5f, 0.5f);
        ecRect.pivot = new Vector2(0.5f, 1f); // Top-aligned pivot
        ecRect.sizeDelta = new Vector2(940, 460);
        ecRect.anchoredPosition = new Vector2(0, 165);

        // Back button
        GameObject backBtn = CreateButton(boardBg.transform, "BackBtn", "BACK TO SYSTEM", new Vector2(0, -310), new Vector2(300, 50), new Color(0.2f, 0.22f, 0.25f));
        backBtn.GetComponent<Image>().color = new Color(0.15f, 0.15f, 0.18f, 0.9f);
        CreateImage(backBtn.transform, "BtnGlow", Fill(), new Color(1f, 0.5f, 0f, 0.1f)).transform.SetAsFirstSibling();

        lbPanel.SetActive(false);

        // -----------------------------------------------
        // 7. GaragePanel
        // -----------------------------------------------
        GameObject garagePanel = CreateFullPanel(canvasGO.transform, "GaragePanel");
        garagePanel.AddComponent<GaragePanel>();

        CreateImage(garagePanel.transform, "Background", Fill(), new Color(0.1f, 0.12f, 0.15f, 0.95f));
        CreateText(garagePanel.transform, "TitleText", "GARAGE / CUSTOMIZATION", 60, Color.white, new Vector2(0, 350));

        // Tank preview
        GameObject previewRoot = CreateEmpty(garagePanel.transform, "TankPreview");
        previewRoot.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 150);

        CreateImage(previewRoot.transform, "Body", Centered(new Vector2(100, 100)), Color.blue);
        CreateImage(previewRoot.transform, "TrackL", Centered(new Vector2(20, 110), new Vector2(-45, 0)), Color.gray);
        CreateImage(previewRoot.transform, "TrackR", Centered(new Vector2(20, 110), new Vector2(45, 0)), Color.gray);
        CreateImage(previewRoot.transform, "Barrel", Centered(new Vector2(20, 50), new Vector2(0, 40)), Color.gray);

        // Palette row labels (buttons generated at runtime)
        CreateText(garagePanel.transform, "BodyLabel", "BODY COLOR", 30, Color.gray, new Vector2(-300, -50));
        CreateText(garagePanel.transform, "TracksLabel", "TRACKS COLOR", 30, Color.gray, new Vector2(-300, -150));
        CreateText(garagePanel.transform, "BarrelLabel", "BARREL COLOR", 30, Color.gray, new Vector2(-300, -250));

        // Palette button dynamic containers
        CreateEmpty(garagePanel.transform, "BodyPaletteRow").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -50);
        CreateEmpty(garagePanel.transform, "TracksPaletteRow").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -150);
        CreateEmpty(garagePanel.transform, "BarrelPaletteRow").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -250);

        CreateButton(garagePanel.transform, "SaveBtn", "SAVE & EXIT", new Vector2(0, -380), new Vector2(300, 60), new Color(0.15f, 0.5f, 0.2f));

        garagePanel.SetActive(false);

        // -----------------------------------------------
        // 8. SettingsPanel
        // -----------------------------------------------
        GameObject settingsPanel = CreateFullPanel(canvasGO.transform, "SettingsPanel");
        settingsPanel.AddComponent<SettingsPanel>();

        CreateImage(settingsPanel.transform, "Background", Fill(), new Color(0.05f, 0.05f, 0.08f, 0.95f));
        CreateText(settingsPanel.transform, "TitleText", "SETTINGS", 60, Color.white, new Vector2(0, 250));

        // BGM Volume Slider
        CreateText(settingsPanel.transform, "BgmLabel", "BGM VOLUME", 30, Color.gray, new Vector2(-200, 100));
        CreateSlider(settingsPanel.transform, "BgmSlider", new Vector2(100, 100), new Vector2(400, 30), new Color(0.2f, 0.6f, 0.3f));
        CreateText(settingsPanel.transform, "BgmValueText", "50%", 26, Color.white, new Vector2(380, 100));

        // SFX Volume Slider
        CreateText(settingsPanel.transform, "SfxLabel", "SFX VOLUME", 30, Color.gray, new Vector2(-200, 20));
        CreateSlider(settingsPanel.transform, "SfxSlider", new Vector2(100, 20), new Vector2(400, 30), new Color(0.6f, 0.4f, 0.2f));
        CreateText(settingsPanel.transform, "SfxValueText", "50%", 26, Color.white, new Vector2(380, 20));

        // Mute All button
        CreateButton(settingsPanel.transform, "MuteBtn", "MUTE ALL", new Vector2(-120, -80), new Vector2(220, 55), new Color(0.5f, 0.15f, 0.15f));

        // Switch BGM button
        CreateButton(settingsPanel.transform, "BgmSwitchBtn", "SWITCH BGM", new Vector2(120, -80), new Vector2(220, 55), new Color(0.15f, 0.35f, 0.5f));

        // BGM Track display
        CreateText(settingsPanel.transform, "TrackLabel", "Current: Track 1", 22, new Color(0.6f, 0.8f, 1f), new Vector2(0, -140));

        CreateButton(settingsPanel.transform, "BackBtn", "BACK", new Vector2(0, -220), new Vector2(300, 60));

        settingsPanel.SetActive(false);

        // -----------------------------------------------
        // BossHUDPanel (L5 specific, anchored bottom center)
        // -----------------------------------------------
        GameObject bossHUD = new GameObject("BossHUDPanel", typeof(RectTransform));
        bossHUD.transform.SetParent(canvasGO.transform, false);
        RectTransform bossHUDRect = bossHUD.GetComponent<RectTransform>();
        bossHUDRect.anchorMin = new Vector2(0.2f, 0f);
        bossHUDRect.anchorMax = new Vector2(0.8f, 0f);
        bossHUDRect.pivot     = new Vector2(0.5f, 0f);
        bossHUDRect.offsetMin = new Vector2(0, 10);
        bossHUDRect.offsetMax = new Vector2(0, 100);
        bossHUD.AddComponent<BossHUDPanel>();

        // Boss name label
        CreateText(bossHUD.transform, "BossName", "TITAN", 28, new Color(1f, 0.3f, 0.3f),
            Anchored(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, 5), new Vector2(400, 30)));

        // Boss phase label
        CreateText(bossHUD.transform, "PhaseText", "Phase 1 - Hunting", 18, new Color(1f, 0.7f, 0.3f),
            Anchored(new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -20), new Vector2(400, 25)));

        // Health bar slider
        CreateSlider(bossHUD.transform, "BossHealthSlider",
            new Vector2(0, 0), new Vector2(0, 0),
            new Color(0.8f, 0.1f, 0.1f),
            useAnchors: true);

        bossHUD.SetActive(false);

        // -----------------------------------------------
        // EventSystem
        // -----------------------------------------------
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // Mark scene as modified
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        Selection.activeGameObject = canvasGO;

        Debug.Log("[UISceneBuilder] UI hierarchy generated");
    }

    // -----------------------------------------------
    // UI factory helper methods
    // -----------------------------------------------

    private static GameObject CreateFullPanel(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        return go;
    }

    private static GameObject CreateEmpty(Transform parent, string name)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    private static GameObject CreateImage(Transform parent, string name, RectData rd, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        ApplyRect(go, rd);
        Image img = go.AddComponent<Image>();
        img.sprite = GetWhiteSprite();
        img.color = color;

        // Background blocks clicks to prevent pass-through; decorative Images do not
        if (name != "Background")
            img.raycastTarget = false;

        return go;
    }

    private static GameObject CreateText(Transform parent, string name, string content, int fontSize, Color color, Vector2 pos)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.sizeDelta = new Vector2(800, 200);
        r.anchoredPosition = pos;
        Text t = go.AddComponent<Text>();
        t.font = GetFont();
        t.text = content;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        return go;
    }

    private static GameObject CreateText(Transform parent, string name, string content, int fontSize, Color color, RectData rd, TextAnchor align = TextAnchor.MiddleCenter)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        ApplyRect(go, rd);
        Text t = go.AddComponent<Text>();
        t.font = GetFont();
        t.text = content;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = align;
        t.raycastTarget = false;
        return go;
    }

    private static GameObject CreateButton(Transform parent, string name, string label, Vector2 pos, Vector2 size, Color? bgColor = null)
    {
        Color bg = bgColor ?? new Color(0.2f, 0.2f, 0.2f, 0.9f);
        
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.sizeDelta = size;
        r.anchoredPosition = pos;
        Image img = go.AddComponent<Image>();
        img.sprite = GetWhiteSprite();
        img.color = bg;
        go.AddComponent<Button>().targetGraphic = img;

        GameObject textGO = new GameObject("Label", typeof(RectTransform));
        textGO.transform.SetParent(go.transform, false);
        RectTransform tr = textGO.GetComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;
        Text t = textGO.AddComponent<Text>();
        t.font = GetFont();
        t.text = label;
        t.fontSize = (int)(size.y * 0.4f);
        t.color = Color.white;
        t.alignment = TextAnchor.MiddleCenter;

        return go;
    }

    private static GameObject CreateInputField(Transform parent, string name, string placeholder, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        r.sizeDelta = size;
        r.anchoredPosition = pos;
        Image img = go.AddComponent<Image>();
        img.sprite = GetWhiteSprite();
        img.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        // Placeholder
        GameObject phGO = new GameObject("Placeholder", typeof(RectTransform));
        phGO.transform.SetParent(go.transform, false);
        RectTransform phR = phGO.GetComponent<RectTransform>();
        phR.anchorMin = Vector2.zero; phR.anchorMax = Vector2.one;
        phR.offsetMin = new Vector2(10, 0); phR.offsetMax = new Vector2(-10, 0);
        Text phT = phGO.AddComponent<Text>();
        phT.font = GetFont(); phT.text = placeholder; phT.fontSize = 30;
        phT.color = new Color(0.7f, 0.7f, 0.7f, 0.5f); phT.alignment = TextAnchor.MiddleLeft;

        // Text
        GameObject tGO = new GameObject("Text", typeof(RectTransform));
        tGO.transform.SetParent(go.transform, false);
        RectTransform tR = tGO.GetComponent<RectTransform>();
        tR.anchorMin = Vector2.zero; tR.anchorMax = Vector2.one;
        tR.offsetMin = new Vector2(10, 0); tR.offsetMax = new Vector2(-10, 0);
        Text aT = tGO.AddComponent<Text>();
        aT.font = GetFont(); aT.text = ""; aT.fontSize = 30;
        aT.color = Color.white; aT.alignment = TextAnchor.MiddleLeft;

        InputField inpF = go.AddComponent<InputField>();
        inpF.textComponent = aT;
        inpF.placeholder = phT;

        return go;
    }

    private static GameObject CreateSlider(Transform parent, string name, Vector2 pos, Vector2 size, Color fillColor, bool useAnchors = false)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform r = go.GetComponent<RectTransform>();
        if (useAnchors)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(10, 5);
            r.offsetMax = new Vector2(-10, -30);
        }
        else
        {
            r.sizeDelta = size;
            r.anchoredPosition = pos;
        }

        // Slider track background
        GameObject bgGO = new GameObject("Background", typeof(RectTransform));
        bgGO.transform.SetParent(go.transform, false);
        RectTransform bgR = bgGO.GetComponent<RectTransform>();
        bgR.anchorMin = new Vector2(0, 0.25f); bgR.anchorMax = new Vector2(1, 0.75f);
        bgR.offsetMin = Vector2.zero; bgR.offsetMax = Vector2.zero;
        Image bgImg = bgGO.AddComponent<Image>();
        bgImg.sprite = GetWhiteSprite();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        bgImg.raycastTarget = false;

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(go.transform, false);
        RectTransform faR = fillArea.GetComponent<RectTransform>();
        faR.anchorMin = new Vector2(0, 0.25f); faR.anchorMax = new Vector2(1, 0.75f);
        faR.offsetMin = new Vector2(5, 0); faR.offsetMax = new Vector2(-5, 0);

        GameObject fill = new GameObject("Fill", typeof(RectTransform));
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fR = fill.GetComponent<RectTransform>();
        fR.sizeDelta = new Vector2(10, 0);
        fR.anchorMin = Vector2.zero; fR.anchorMax = new Vector2(0, 1);
        fR.pivot = new Vector2(0, 0.5f);
        fR.offsetMin = Vector2.zero; fR.offsetMax = new Vector2(0, 0);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.sprite = GetWhiteSprite();
        fillImg.color = fillColor;
        fillImg.raycastTarget = false;

        // Handle Slide Area
        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(go.transform, false);
        RectTransform haR = handleArea.GetComponent<RectTransform>();
        haR.anchorMin = Vector2.zero; haR.anchorMax = Vector2.one;
        haR.offsetMin = new Vector2(10, 0); haR.offsetMax = new Vector2(-10, 0);

        GameObject handle = new GameObject("Handle", typeof(RectTransform));
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform hR = handle.GetComponent<RectTransform>();
        hR.anchorMin = new Vector2(0, 0); hR.anchorMax = new Vector2(0, 1);
        hR.sizeDelta = new Vector2(20, 0);
        hR.anchoredPosition = Vector2.zero;
        Image handleImg = handle.AddComponent<Image>();
        handleImg.sprite = GetWhiteSprite();
        handleImg.color = Color.white;

        // Slider Component
        Slider slider = go.AddComponent<Slider>();
        slider.fillRect = fR;
        slider.handleRect = hR;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = 0.5f;

        return go;
    }

    // -----------------------------------------------
    // RectTransform layout helpers
    // -----------------------------------------------
    private struct RectData
    {
        public Vector2 anchorMin, anchorMax, pivot, anchoredPosition, sizeDelta;
    }

    private static RectData Fill() => new RectData {
        anchorMin = Vector2.zero, anchorMax = Vector2.one,
        pivot = new Vector2(0.5f, 0.5f), anchoredPosition = Vector2.zero, sizeDelta = Vector2.zero
    };

    private static RectData AnchorTop(float height) => new RectData {
        anchorMin = new Vector2(0, 1), anchorMax = new Vector2(1, 1),
        pivot = new Vector2(0.5f, 1), anchoredPosition = Vector2.zero, sizeDelta = new Vector2(0, height)
    };

    private static RectData Centered(Vector2 size, Vector2? pos = null) => new RectData {
        anchorMin = new Vector2(0.5f, 0.5f), anchorMax = new Vector2(0.5f, 0.5f),
        pivot = new Vector2(0.5f, 0.5f), anchoredPosition = pos ?? Vector2.zero, sizeDelta = size
    };

    private static RectData BottomLeft(float w, float h, Vector2 pos) => new RectData {
        anchorMin = Vector2.zero, anchorMax = Vector2.zero,
        pivot = Vector2.zero, anchoredPosition = pos, sizeDelta = new Vector2(w, h)
    };

    private static RectData Anchored(Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size) => new RectData {
        anchorMin = aMin, anchorMax = aMax, pivot = pivot, anchoredPosition = pos, sizeDelta = size
    };

    private static void ApplyRect(GameObject go, RectData rd)
    {
        RectTransform r = go.GetComponent<RectTransform>();
        r.anchorMin = rd.anchorMin;
        r.anchorMax = rd.anchorMax;
        r.pivot = rd.pivot;
        r.anchoredPosition = rd.anchoredPosition;
        r.sizeDelta = rd.sizeDelta;
    }
}
