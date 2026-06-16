using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class DefaultLabStandPanelPrefabBuilder
{
    private const string PrefabPath = "Assets/_Project/Lab01/Prefabs/DefaultLabStandPanel.prefab";

    [MenuItem("Tools/LiquidShader/Create Default Lab Stand Panel Prefab")]
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
                Debug.Log($"Default lab stand panel prefab created: {PrefabPath}");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            }
            else
            {
                Debug.LogError("Failed to create default lab stand panel prefab.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to create default lab stand panel prefab.\n{ex}");
        }
    }

    private static GameObject BuildPrefabHierarchy()
    {
        Vector2 panelSize = new Vector2(720f, 520f);
        Vector2 bodySize = new Vector2(620f, 330f);
        Color panelColor = new Color(0.09f, 0.11f, 0.15f, 0.96f);
        Color backdropColor = new Color(0.02f, 0.03f, 0.05f, 0.72f);
        Color accentColor = new Color(0.16f, 0.58f, 0.88f, 1f);
        Color textColor = new Color(0.92f, 0.96f, 1f, 1f);
        Color secondaryTextColor = new Color(0.78f, 0.86f, 0.95f, 1f);

        GameObject root = new GameObject("DefaultLabStandPanel", typeof(RectTransform), typeof(DefaultLabStandPanelView));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        CreateImage(root.transform, "Backdrop", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, backdropColor);

        GameObject panel = CreateImage(root.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), panelSize, Vector2.zero, panelColor);
        panel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

        GameObject accent = CreateImage(panel.transform, "Accent", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(panelSize.x - 72f, 6f), new Vector2(0f, -34f), accentColor);
        accent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);

        TMP_Text title = CreateText(panel.transform, "Title", "Паспорт лабораторного стенда", 30f, FontStyles.Bold, TextAlignmentOptions.Center, textColor);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(panelSize.x - 80f, 48f), new Vector2(0f, -70f), new Vector2(0.5f, 1f));

        TMP_Text body = CreateText(panel.transform, "Body", "Данные появятся во время запуска сцены.", 22f, FontStyles.Normal, TextAlignmentOptions.TopLeft, textColor);
        body.textWrappingMode = TextWrappingModes.Normal;
        body.overflowMode = TextOverflowModes.Overflow;
        SetRect(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), bodySize, new Vector2(0f, -18f), new Vector2(0.5f, 0.5f));

        TMP_Text footer = CreateText(panel.transform, "Footer", "E / Esc - закрыть", 18f, FontStyles.Bold, TextAlignmentOptions.Center, secondaryTextColor);
        SetRect(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(panelSize.x - 220f, 34f), new Vector2(-52f, 32f), new Vector2(0.5f, 0f));

        Button recordButton = CreateButton(panel.transform, "RecordButton", "Записать данные", new Vector2(190f, 42f), new Vector2(-196f, 34f), accentColor);
        Button closeButton = CreateButton(panel.transform, "CloseButton", "Close", new Vector2(132f, 42f), new Vector2(panelSize.x * 0.5f - 114f, 34f), accentColor);

        DefaultLabStandPanelView view = root.GetComponent<DefaultLabStandPanelView>();
        view.Configure(title, body, footer, recordButton, closeButton);
        return root;
    }

    private static GameObject CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return imageObject;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

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

    private static Button CreateButton(Transform parent, string name, string label, Vector2 size, Vector2 anchoredPosition, Color accentColor)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.GetComponent<Image>();
        image.color = accentColor;

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = accentColor;
        colors.highlightedColor = new Color(0.22f, 0.68f, 0.98f, 1f);
        colors.pressedColor = new Color(0.10f, 0.38f, 0.62f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        TMP_Text labelText = CreateText(buttonObject.transform, "Label", label, 20f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        labelText.rectTransform.anchorMin = Vector2.zero;
        labelText.rectTransform.anchorMax = Vector2.one;
        labelText.rectTransform.offsetMin = Vector2.zero;
        labelText.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition, Vector2 pivot)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
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
