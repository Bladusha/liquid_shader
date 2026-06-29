using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class RealPauseMenuPrefabBuilder
{
    private const string PrefabPath = "Assets/_Project/Lab/Prefabs/RealPauseMenu.prefab";
    private const string PanelSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/UnityPng/panels/panel_large_header.png";
    private const string ButtonSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/UnityPng/buttons/button_primary_water_frame.png";

    private static readonly Color TextBlue = new Color(0.043f, 0.435f, 0.624f, 1f);
    private static readonly Color TextMuted = new Color(0.42f, 0.498f, 0.537f, 1f);

    [MenuItem("Tools/LiquidShader/Create Real Pause Menu Prefab")]
    public static void CreatePrefab()
    {
        try
        {
            EnsureFolder("Assets/_Project/Lab/Prefabs");

            GameObject root = BuildPrefabHierarchy();
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            if (savedPrefab != null)
            {
                Debug.Log($"Real pause menu prefab created: {PrefabPath}");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }
            else
            {
                Debug.LogError("Failed to create real pause menu prefab.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to create real pause menu prefab.\n{ex}");
        }
    }

    private static GameObject BuildPrefabHierarchy()
    {
        GameObject root = new GameObject("RealPauseMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(RealPauseMenuView), typeof(CanvasGroup));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(640f, 540f);

        GameObject panel = CreateImagePanel(root.transform, "Panel", new Vector2(640f, 540f), LoadSprite(PanelSpritePath));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchoredPosition = Vector2.zero;

        TMP_Text title = CreateText(panel.transform, "Title", "Пауза", new Vector2(0f, 206f), new Vector2(560f, 72f), 52f, FontStyles.Bold, TextAlignmentOptions.Center, TextBlue);
        TMP_Text info = CreateText(panel.transform, "Info", "TAB - закрыть / открыть", new Vector2(0f, 154f), new Vector2(560f, 44f), 23f, FontStyles.Normal, TextAlignmentOptions.Center, TextMuted);

        Button continueButton = CreateButton(panel.transform, "ContinueButton", "Продолжить", new Vector2(380f, 92f), new Vector2(0f, 88f));
        Button tableButton = CreateButton(panel.transform, "LabTableButton", "Таблица", new Vector2(380f, 92f), new Vector2(0f, 18f));
        Button calculationButton = CreateButton(panel.transform, "LabCalculationButton", "Расчёты", new Vector2(380f, 92f), new Vector2(0f, -52f));
        Button logsButton = CreateButton(panel.transform, "LogsButton", "Журнал", new Vector2(380f, 92f), new Vector2(0f, -122f));
        Button cursorTestButton = CreateButton(panel.transform, "CursorTestButton", "Тест курсора", new Vector2(380f, 92f), new Vector2(0f, -192f));
        TMP_Text cursorTestLabel = CreateText(panel.transform, "CursorTestLabel", "Clicks: 0", new Vector2(0f, -250f), new Vector2(560f, 42f), 20f, FontStyles.Normal, TextAlignmentOptions.Center, TextMuted);

        RealPauseMenuView view = root.GetComponent<RealPauseMenuView>();
        view.Configure(title, info, continueButton, cursorTestButton, tableButton, calculationButton, logsButton, cursorTestLabel);

        return root;
    }

    private static GameObject CreateImagePanel(Transform parent, string name, Vector2 size, Sprite sprite)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        Image image = panel.GetComponent<Image>();
        image.sprite = sprite;
        image.color = Color.white;
        image.raycastTarget = false;
        return panel;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, Vector2 anchoredPosition, Vector2 size, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
        {
            label.font = TMP_Settings.defaultFontAsset;
        }

        return label;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 size, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = LoadSprite(ButtonSpritePath);
        image.color = Color.white;
        image.raycastTarget = true;

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.88f, 0.975f, 1f, 1f);
        colors.pressedColor = new Color(0.73f, 0.91f, 0.97f, 1f);
        colors.selectedColor = new Color(0.88f, 0.975f, 1f, 1f);
        colors.disabledColor = new Color(0.75f, 0.83f, 0.86f, 0.55f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        TMP_Text labelText = CreateText(buttonObject.transform, "Label", label, Vector2.zero, size, 27f, FontStyles.Bold, TextAlignmentOptions.Center, TextBlue);
        labelText.rectTransform.anchorMin = Vector2.zero;
        labelText.rectTransform.anchorMax = Vector2.one;
        labelText.rectTransform.offsetMin = Vector2.zero;
        labelText.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            throw new FileNotFoundException($"Sprite asset was not found: {path}", path);
        }

        return sprite;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folderName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent ?? "Assets", folderName);
    }
}
