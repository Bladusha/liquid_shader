using System.IO;
using Lab.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class DefaultLabStandPanelPrefabBuilder
{
    private const string PrefabPath = "Assets/_Project/Lab/Prefabs/DefaultLabStandPanel.prefab";

    [MenuItem("Tools/LiquidShader/Create Default Lab Stand Panel Prefab")]
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
        Color panelColor = HydrodynamicsUiTheme.Panel;
        Color backdropColor = HydrodynamicsUiTheme.Backdrop;
        Color accentColor = HydrodynamicsUiTheme.Water;
        Color textColor = HydrodynamicsUiTheme.Text;
        Color secondaryTextColor = HydrodynamicsUiTheme.MutedText;

        GameObject root = new GameObject("DefaultLabStandPanel", typeof(RectTransform), typeof(DefaultLabStandPanelView));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        CreateImage(root.transform, "Backdrop", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, backdropColor);

        GameObject panel = CreateImage(root.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), panelSize, Vector2.zero, panelColor);
        panel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

        GameObject header = CreateImage(panel.transform, "WaterHeader", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(panelSize.x - 48f, 68f), new Vector2(0f, -24f), HydrodynamicsUiTheme.Water);
        header.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);

        GameObject accent = CreateImage(panel.transform, "Accent", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(132f, 6f), new Vector2(0f, -104f), HydrodynamicsUiTheme.Accent);
        accent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);

        TMP_Text title = CreateText(panel.transform, "Title", "Паспорт лабораторного стенда", 30f, FontStyles.Bold, TextAlignmentOptions.Center, HydrodynamicsUiTheme.TextOnDark);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(panelSize.x - 80f, 48f), new Vector2(0f, -38f), new Vector2(0.5f, 1f));

        GameObject bodySurface = CreateImage(panel.transform, "BodySurface", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(640f, 310f), new Vector2(0f, -18f), HydrodynamicsUiTheme.Surface);
        bodySurface.GetComponent<Image>().raycastTarget = false;

        TMP_Text body = CreateText(panel.transform, "Body", "Данные появятся во время запуска сцены.", 22f, FontStyles.Normal, TextAlignmentOptions.TopLeft, textColor);
        body.textWrappingMode = TextWrappingModes.Normal;
        body.overflowMode = TextOverflowModes.Overflow;
        SetRect(body.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), bodySize, new Vector2(0f, -18f), new Vector2(0.5f, 0.5f));

        TMP_Text footer = CreateText(panel.transform, "Footer", "E / Esc - закрыть", 18f, FontStyles.Bold, TextAlignmentOptions.Center, secondaryTextColor);
        SetRect(footer.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(panelSize.x - 220f, 34f), new Vector2(-52f, 32f), new Vector2(0.5f, 0f));

        GameObject notificationRoot = CreateImage(panel.transform, "RecordNotification", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(380f, 46f), new Vector2(0f, 96f), HydrodynamicsUiTheme.WaterDark);
        notificationRoot.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);

        TMP_Text notificationLabel = CreateText(notificationRoot.transform, "Message", "Запись номер 1 сохранена!", 18f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        notificationLabel.textWrappingMode = TextWrappingModes.NoWrap;
        notificationLabel.overflowMode = TextOverflowModes.Ellipsis;
        notificationLabel.rectTransform.anchorMin = Vector2.zero;
        notificationLabel.rectTransform.anchorMax = Vector2.one;
        notificationLabel.rectTransform.offsetMin = new Vector2(16f, 8f);
        notificationLabel.rectTransform.offsetMax = new Vector2(-16f, -8f);
        notificationRoot.SetActive(false);

        Button recordButton = CreateButton(panel.transform, "RecordButton", "Записать данные", new Vector2(190f, 44f), new Vector2(-196f, 34f), accentColor);
        Button calculationButton = CreateButton(panel.transform, "CalculationButton", "Перейти к расчётам", new Vector2(224f, 44f), new Vector2(0f, 34f), accentColor);
        Button closeButton = CreateButton(panel.transform, "CloseButton", "Закрыть", new Vector2(132f, 44f), new Vector2(panelSize.x * 0.5f - 114f, 34f), HydrodynamicsUiTheme.Accent);

        DefaultLabStandPanelView view = root.GetComponent<DefaultLabStandPanelView>();
        view.Configure(title, body, footer, notificationRoot, notificationLabel, recordButton, calculationButton, closeButton);
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
        button.colors = HydrodynamicsUiTheme.ButtonColors(accentColor);

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
