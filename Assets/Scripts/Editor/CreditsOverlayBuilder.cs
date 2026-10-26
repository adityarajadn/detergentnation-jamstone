#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class CreditsOverlayBuilder
{
    [MenuItem("Tools/Build Credits Overlay")]
    public static void Build()
    {
        // 1. Load MainMenu scene canvas
        GameObject canvasGo = GameObject.Find("MainMenuCanvas");
        if (canvasGo == null)
        {
            Debug.LogError("MainMenuCanvas not found in active scene!");
            return;
        }

        Transform existing = canvasGo.transform.Find("UI_Panel_Credits");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        // 2. Load UI sprites and back button prefab
        Sprite panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Bg.png");
        if (panelSprite == null)
        {
            // Try finding any Sprite in Assets/Sprites/UI
            string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { "Assets/Sprites/UI" });
            if (guids.Length > 0)
            {
                panelSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GUIDToAssetPath(guids[0]));
            }
        }

        GameObject backBtnPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/UI_Button_Back.prefab");
        TMP_FontAsset defaultFont = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (defaultFont == null)
        {
            string[] fontGuids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            if (fontGuids.Length > 0)
            {
                defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(fontGuids[0]));
            }
        }

        // 3. Create Root Overlay (UI_Panel_Credits) - Stretch to Canvas
        GameObject root = new GameObject("UI_Panel_Credits", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        root.transform.SetParent(canvasGo.transform, false);
        Undo.RegisterCreatedObjectUndo(root, "Create Credits Panel");

        RectTransform rootRt = root.GetComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.anchoredPosition = Vector2.zero;
        rootRt.sizeDelta = Vector2.zero;

        Image rootImg = root.GetComponent<Image>();
        rootImg.color = new Color(0.04f, 0.05f, 0.08f, 0.85f);
        rootImg.raycastTarget = true;

        // 4. Modal Box (CreditsModal)
        GameObject modal = new GameObject("CreditsModal", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        modal.transform.SetParent(root.transform, false);

        RectTransform modalRt = modal.GetComponent<RectTransform>();
        modalRt.anchorMin = new Vector2(0.5f, 0.5f);
        modalRt.anchorMax = new Vector2(0.5f, 0.5f);
        modalRt.pivot = new Vector2(0.5f, 0.5f);
        modalRt.sizeDelta = new Vector2(800f, 600f);
        modalRt.anchoredPosition = Vector2.zero;

        Image modalImg = modal.GetComponent<Image>();
        if (panelSprite != null)
        {
            modalImg.sprite = panelSprite;
            modalImg.type = Image.Type.Sliced;
        }
        modalImg.color = new Color(0.12f, 0.15f, 0.22f, 0.98f);

        // 5. Title Text
        GameObject titleGo = new GameObject("Title", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleGo.transform.SetParent(modal.transform, false);

        RectTransform titleRt = titleGo.GetComponent<RectTransform>();
        titleRt.anchorMin = new Vector2(0.5f, 1f);
        titleRt.anchorMax = new Vector2(0.5f, 1f);
        titleRt.pivot = new Vector2(0.5f, 1f);
        titleRt.anchoredPosition = new Vector2(0f, -30f);
        titleRt.sizeDelta = new Vector2(700f, 60f);

        TextMeshProUGUI titleTmp = titleGo.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) titleTmp.font = defaultFont;
        titleTmp.text = "CREDITS";
        titleTmp.fontSize = 42;
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color = Color.white;

        // 6. Scroll View Container
        GameObject scrollGo = new GameObject("ScrollView", typeof(RectTransform), typeof(CanvasRenderer), typeof(ScrollRect), typeof(Image));
        scrollGo.transform.SetParent(modal.transform, false);

        RectTransform scrollRt = scrollGo.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0.5f, 0.5f);
        scrollRt.anchorMax = new Vector2(0.5f, 0.5f);
        scrollRt.pivot = new Vector2(0.5f, 0.5f);
        scrollRt.anchoredPosition = new Vector2(0f, -5f);
        scrollRt.sizeDelta = new Vector2(700f, 400f);

        Image scrollImg = scrollGo.GetComponent<Image>();
        scrollImg.color = new Color(0.08f, 0.1f, 0.15f, 0.6f);

        ScrollRect scrollRect = scrollGo.GetComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 25f;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;

        // Viewport
        GameObject viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(RectMask2D));
        viewportGo.transform.SetParent(scrollGo.transform, false);

        RectTransform viewportRt = viewportGo.GetComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.pivot = new Vector2(0f, 1f);
        viewportRt.anchoredPosition = Vector2.zero;
        viewportRt.sizeDelta = new Vector2(-20f, -20f);
        viewportRt.anchoredPosition = new Vector2(10f, -10f);

        scrollRect.viewport = viewportRt;

        // Content
        GameObject contentGo = new GameObject("Content", typeof(RectTransform), typeof(ContentSizeFitter), typeof(VerticalLayoutGroup));
        contentGo.transform.SetParent(viewportGo.transform, false);

        RectTransform contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0f, 0f);

        VerticalLayoutGroup vlg = contentGo.GetComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.padding = new RectOffset(20, 20, 20, 20);

        ContentSizeFitter csf = contentGo.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        scrollRect.content = contentRt;

        // Credits Text item inside content
        GameObject textGo = new GameObject("CreditsText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textGo.transform.SetParent(contentGo.transform, false);

        TextMeshProUGUI creditsTmp = textGo.GetComponent<TextMeshProUGUI>();
        if (defaultFont != null) creditsTmp.font = defaultFont;
        creditsTmp.text = "<b>DETERGENT NATION: JAMSTONE</b>\n\n" +
                          "<size=28><color=#FFD700>DEVELOPMENT TEAM</color></size>\n\n" +
                          "<b>Lead Developer & Programmer:</b>\nAditya\n\n" +
                          "<b>Game Design & Architecture:</b>\nTeam Detergent\n\n" +
                          "<b>Art & Visual Effects:</b>\nTeam Detergent\n\n" +
                          "<b>Audio & Sound Effects:</b>\nTeam Detergent\n\n" +
                          "<size=28><color=#FFD700>SPECIAL THANKS</color></size>\n\n" +
                          "Universitas Brawijaya\n" +
                          "Game Jam Community\n" +
                          "All Playtesters & Supporters\n\n" +
                          "<i>Thank you for playing our game!</i>";
        creditsTmp.fontSize = 22;
        creditsTmp.alignment = TextAlignmentOptions.Center;
        creditsTmp.color = new Color(0.9f, 0.92f, 0.95f, 1f);
        creditsTmp.textWrappingMode = TextWrappingModes.Normal;

        // 7. Back/Close Button
        GameObject closeBtnGo;
        if (backBtnPrefab != null)
        {
            closeBtnGo = (GameObject)PrefabUtility.InstantiatePrefab(backBtnPrefab, modal.transform);
        }
        else
        {
            closeBtnGo = new GameObject("BackButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            closeBtnGo.transform.SetParent(modal.transform, false);
            GameObject btnLabel = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            btnLabel.transform.SetParent(closeBtnGo.transform, false);
            TextMeshProUGUI btnTmp = btnLabel.GetComponent<TextMeshProUGUI>();
            btnTmp.text = "BACK";
            btnTmp.fontSize = 24;
            btnTmp.alignment = TextAlignmentOptions.Center;
        }

        RectTransform closeRt = closeBtnGo.GetComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(0.5f, 0f);
        closeRt.anchorMax = new Vector2(0.5f, 0f);
        closeRt.pivot = new Vector2(0.5f, 0f);
        closeRt.anchoredPosition = new Vector2(0f, 25f);
        closeRt.sizeDelta = new Vector2(180f, 48f);

        Button closeBtn = closeBtnGo.GetComponent<Button>();
        if (closeBtn != null)
        {
            // Wire button click to close overlay
            closeBtn.onClick.RemoveAllListeners();
            // UnityEvent persistent call in editor
            UnityEditor.Events.UnityEventTools.AddBoolPersistentListener(closeBtn.onClick, root.SetActive, false);
        }

        // 8. Connect to MainMenuUI
        MainMenuUI menuUI = canvasGo.GetComponent<MainMenuUI>();
        if (menuUI != null)
        {
            SerializedObject so = new SerializedObject(menuUI);
            SerializedProperty credProp = so.FindProperty("credits_panel");
            if (credProp != null)
            {
                credProp.objectReferenceValue = root;
                so.ApplyModifiedProperties();
            }
        }

        // Save as Prefab
        string prefabPath = "Assets/Prefabs/UI/UI_Panel_Credits.prefab";
        PrefabUtility.SaveAsPrefabAssetAndConnect(root, prefabPath, InteractionMode.AutomatedAction);

        // Initially inactive
        root.SetActive(false);

        // Save scene
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvasGo.scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(canvasGo.scene);

        Debug.Log("UI_Panel_Credits successfully created, connected, and saved to " + prefabPath);
    }
}
#endif
