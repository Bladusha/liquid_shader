using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class RealPauseMenuPrefabBuilder
{
    private const string PrefabPath = "Assets/_Project/Lab01/Prefabs/RealPauseMenu.prefab";

    [MenuItem("Tools/LiquidShader/Create Real Pause Menu Prefab")]
    public static void CreatePrefab()
    {
        try
        {
            EnsureFolder("Assets/_Project/Lab01/Prefabs");

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
        GameObject root = new GameObject("RealPauseMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(RealPauseMenuView));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(560f, 520f);

        GameObject panel = CreatePanel(root.transform, "Panel", new Vector2(560f, 520f), new Color(0.08f, 0.09f, 0.12f, 0.96f));
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchoredPosition = Vector2.zero;

        TMP_Text title = CreateText(panel.transform, "Title", "Pause", new Vector2(0f, 205f), 44f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        TMP_Text info = CreateText(panel.transform, "Info", "Tab to close / open", new Vector2(0f, 160f), 22f, FontStyles.Normal, TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 0.8f));

        Button continueButton = CreateButton(panel.transform, "ContinueButton", "Continue", new Vector2(240f, 50f), new Vector2(0f, 92f));
        Button tableButton = CreateButton(panel.transform, "LabTableButton", "Table", new Vector2(240f, 50f), new Vector2(0f, 32f));
        Button calculationButton = CreateButton(panel.transform, "LabCalculationButton", "Calculations", new Vector2(240f, 50f), new Vector2(0f, -28f));
        Button logsButton = CreateButton(panel.transform, "LogsButton", "Logs", new Vector2(240f, 50f), new Vector2(0f, -88f));
        Button cursorTestButton = CreateButton(panel.transform, "CursorTestButton", "Cursor Test", new Vector2(240f, 50f), new Vector2(0f, -148f));
        TMP_Text cursorTestLabel = CreateText(panel.transform, "CursorTestLabel", "Clicks: 0", new Vector2(0f, -205f), 20f, FontStyles.Normal, TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 0.85f));

        RealPauseMenuView view = root.GetComponent<RealPauseMenuView>();
        view.Configure(title, info, continueButton, cursorTestButton, tableButton, calculationButton, logsButton, cursorTestLabel);

        return root;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        Image image = panel.GetComponent<Image>();
        image.color = color;
        return panel;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, Vector2 anchoredPosition, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(520f, 60f);

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
        image.color = new Color(0.18f, 0.45f, 0.82f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.24f, 0.56f, 0.92f, 1f);
        colors.pressedColor = new Color(0.14f, 0.34f, 0.62f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        TMP_Text labelText = CreateText(buttonObject.transform, "Label", label, Vector2.zero, 24f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        labelText.rectTransform.anchorMin = Vector2.zero;
        labelText.rectTransform.anchorMax = Vector2.one;
        labelText.rectTransform.offsetMin = Vector2.zero;
        labelText.rectTransform.offsetMax = Vector2.zero;
        return button;
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
