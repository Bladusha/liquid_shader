using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class Lab01WorkMenuPrefabsBuilder
{
    private const string TablePrefabPath = "Assets/_Project/Lab01/Prefabs/Lab01TableMenu.prefab";
    private const string CalculationPrefabPath = "Assets/_Project/Lab01/Prefabs/Lab01CalculationMenu.prefab";

    [MenuItem("Tools/LiquidShader/Create Lab 01 Work Menu Prefabs")]
    public static void CreatePrefabs()
    {
        EnsureFolder("Assets/_Project/Lab01/Prefabs");
        SavePrefab(BuildTablePrefabHierarchy(), TablePrefabPath);
        SavePrefab(BuildCalculationPrefabHierarchy(), CalculationPrefabPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath);
    }

    [MenuItem("Tools/LiquidShader/Create Lab 01 Table Menu Prefab")]
    public static void CreateTablePrefab()
    {
        EnsureFolder("Assets/_Project/Lab01/Prefabs");
        SavePrefab(BuildTablePrefabHierarchy(), TablePrefabPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath);
    }

    [MenuItem("Tools/LiquidShader/Create Lab 01 Calculation Menu Prefab")]
    public static void CreateCalculationPrefab()
    {
        EnsureFolder("Assets/_Project/Lab01/Prefabs");
        SavePrefab(BuildCalculationPrefabHierarchy(), CalculationPrefabPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(CalculationPrefabPath);
    }

    private static GameObject BuildTablePrefabHierarchy()
    {
        Color panelColor = new Color(0.08f, 0.10f, 0.14f, 0.98f);
        Color sectionColor = new Color(0.12f, 0.15f, 0.20f, 0.94f);
        Color accentColor = new Color(0.16f, 0.58f, 0.88f, 1f);
        Color textColor = new Color(0.92f, 0.96f, 1f, 1f);
        Color mutedTextColor = new Color(0.74f, 0.82f, 0.92f, 1f);

        GameObject root = new GameObject("Lab01TableMenu", typeof(RectTransform), typeof(Lab01TableMenuController));
        SetRect(root.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));

        CreateImage(root.transform, "Backdrop", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.05f, 0.72f));

        GameObject panel = CreateImage(root.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(920f, 600f), Vector2.zero, panelColor);
        TMP_Text title = CreateText(panel.transform, "Title", "ЛР №1: таблица", 32f, FontStyles.Bold, TextAlignmentOptions.Center, textColor);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(700f, 52f), new Vector2(0f, -34f), new Vector2(0.5f, 1f));

        Button closeButton = CreateButton(panel.transform, "CloseButton", "Close", new Vector2(120f, 42f), new Vector2(370f, -42f), accentColor, textColor);
        SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(120f, 42f), new Vector2(370f, -42f), new Vector2(0.5f, 1f));

        GameObject tableBox = CreateImage(panel.transform, "TableRowBox", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(848f, 200f), new Vector2(36f, -300f), sectionColor);
        TMP_Text tableRowText = CreateText(tableBox.transform, "TableRowText", "№ записи | d, м | f, м2 | V, м3/с | w, м/с | t, °C | ν, м2/с | Re | Режим", 16f, FontStyles.Normal, TextAlignmentOptions.TopLeft, mutedTextColor);
        tableRowText.textWrappingMode = TextWrappingModes.Normal;
        SetFullRectWithPadding(tableRowText.rectTransform, 18f, 14f);

        Button calculationButton = CreateButton(panel.transform, "CalculationButton", "К расчётам", new Vector2(260f, 54f), new Vector2(590f, -118f), accentColor, textColor);

        root.GetComponent<Lab01TableMenuController>().Configure(
            null,
            tableRowText,
            null,
            calculationButton,
            closeButton);

        return root;
    }

    private static GameObject BuildCalculationPrefabHierarchy()
    {
        Color panelColor = new Color(0.08f, 0.10f, 0.14f, 0.98f);
        Color sectionColor = new Color(0.12f, 0.15f, 0.20f, 0.94f);
        Color inputColor = new Color(0.06f, 0.075f, 0.10f, 1f);
        Color accentColor = new Color(0.16f, 0.58f, 0.88f, 1f);
        Color textColor = new Color(0.92f, 0.96f, 1f, 1f);
        Color mutedTextColor = new Color(0.74f, 0.82f, 0.92f, 1f);

        GameObject root = new GameObject("Lab01CalculationMenu", typeof(RectTransform), typeof(Lab01CalculationMenuController));
        SetRect(root.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));

        CreateImage(root.transform, "Backdrop", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.02f, 0.03f, 0.05f, 0.72f));

        GameObject panel = CreateImage(root.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1080f, 760f), Vector2.zero, panelColor);
        TMP_Text title = CreateText(panel.transform, "Title", "ЛР №1: расчёты", 32f, FontStyles.Bold, TextAlignmentOptions.Center, textColor);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(760f, 52f), new Vector2(0f, -34f), new Vector2(0.5f, 1f));

        Button closeButton = CreateButton(panel.transform, "CloseButton", "Close", new Vector2(120f, 42f), new Vector2(460f, -42f), accentColor, textColor);
        SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(120f, 42f), new Vector2(460f, -42f), new Vector2(0.5f, 1f));

        GameObject snapshotBox = CreateImage(panel.transform, "SnapshotBox", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(340f, 142f), new Vector2(36f, -98f), sectionColor);
        TMP_Text snapshotText = CreateText(snapshotBox.transform, "SnapshotText", "", 20f, FontStyles.Normal, TextAlignmentOptions.TopLeft, mutedTextColor);
        SetFullRectWithPadding(snapshotText.rectTransform, 16f, 12f);

        Dropdown recordDropdown = CreateDropdown(panel.transform, "RecordedDataDropdown", new Vector2(340f, 38f), new Vector2(340f, 38f), inputColor);
        recordDropdown.ClearOptions();
        recordDropdown.AddOptions(new System.Collections.Generic.List<string> { "Записи не найдены" });
        recordDropdown.RefreshShownValue();

        TMP_InputField diameterInput = CreateInput(panel.transform, "DiameterInput", "d, м", new Vector2(420f, -112f), inputColor);
        TMP_InputField areaInput = CreateInput(panel.transform, "AreaInput", "f, м2", new Vector2(420f, -176f), inputColor);
        TMP_InputField flowInput = CreateInput(panel.transform, "FlowInput", "V, м3/с", new Vector2(420f, -240f), inputColor);
        TMP_InputField velocityInput = CreateInput(panel.transform, "VelocityInput", "w, м/с", new Vector2(420f, -304f), inputColor);
        TMP_InputField temperatureInput = CreateInput(panel.transform, "TemperatureInput", "t, °C", new Vector2(420f, -368f), inputColor);
        TMP_InputField viscosityInput = CreateInput(panel.transform, "ViscosityInput", "ν, м2/с", new Vector2(420f, -432f), inputColor);
        TMP_InputField reynoldsInput = CreateInput(panel.transform, "ReynoldsInput", "Re", new Vector2(420f, -496f), inputColor);

        Button calculateAreaButton = CreateButton(panel.transform, "CalculateAreaButton", "Высчитать f", new Vector2(230f, 44f), new Vector2(710f, -176f), accentColor, textColor);
        Button calculateVelocityButton = CreateButton(panel.transform, "CalculateVelocityButton", "Высчитать w", new Vector2(230f, 44f), new Vector2(710f, -304f), accentColor, textColor);
        Button calculateViscosityButton = CreateButton(panel.transform, "CalculateViscosityButton", "Высчитать ν", new Vector2(230f, 44f), new Vector2(710f, -432f), accentColor, textColor);
        Button calculateReynoldsButton = CreateButton(panel.transform, "CalculateReynoldsButton", "Высчитать Re", new Vector2(230f, 44f), new Vector2(710f, -496f), accentColor, textColor);

        Button submitButton = CreateButton(panel.transform, "SubmitToTableButton", "Внести данные в таблицу", new Vector2(330f, 54f), new Vector2(664f, -638f), accentColor, textColor);
        Button tableButton = CreateButton(panel.transform, "TableButton", "К таблице", new Vector2(220f, 54f), new Vector2(410f, -710f), accentColor, textColor);

        root.GetComponent<Lab01CalculationMenuController>().Configure(
            snapshotText,
            recordDropdown,
            diameterInput,
            areaInput,
            flowInput,
            velocityInput,
            temperatureInput,
            viscosityInput,
            reynoldsInput,
            calculateAreaButton,
            calculateVelocityButton,
            calculateViscosityButton,
            calculateReynoldsButton,
            submitButton,
            tableButton,
            closeButton);

        return root;
    }

    private static TMP_InputField CreateInput(Transform parent, string name, string placeholder, Vector2 anchoredPosition, Color backgroundColor)
    {
        GameObject inputObject = CreateImage(parent, name, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(240f, 42f), anchoredPosition, backgroundColor);
        TMP_InputField input = inputObject.AddComponent<TMP_InputField>();
        input.targetGraphic = inputObject.GetComponent<Image>();

        TMP_Text text = CreateText(inputObject.transform, "Text", "", 19f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, Color.white);
        SetFullRectWithPadding(text.rectTransform, 14f, 4f);

        TMP_Text placeholderText = CreateText(inputObject.transform, "Placeholder", placeholder, 18f, FontStyles.Italic, TextAlignmentOptions.MidlineLeft, new Color(1f, 1f, 1f, 0.42f));
        SetFullRectWithPadding(placeholderText.rectTransform, 14f, 4f);

        input.textComponent = text;
        input.placeholder = placeholderText;
        return input;
    }

    private static Dropdown CreateDropdown(Transform parent, string name, Vector2 size, Vector2 anchoredPosition, Color backgroundColor)
    {
        GameObject dropdownObject = CreateImage(parent, name, new Vector2(0f, 1f), new Vector2(0f, 1f), size, anchoredPosition, backgroundColor);
        Dropdown dropdown = dropdownObject.AddComponent<Dropdown>();
        dropdown.targetGraphic = dropdownObject.GetComponent<Image>();

        ColorBlock colors = dropdown.colors;
        colors.normalColor = backgroundColor;
        colors.highlightedColor = new Color(0.22f, 0.68f, 0.98f, 1f);
        colors.pressedColor = new Color(0.10f, 0.38f, 0.62f, 1f);
        colors.selectedColor = colors.highlightedColor;
        dropdown.colors = colors;

        Text label = CreateUiText(dropdownObject.transform, "Label", "Записи не найдены", 18, FontStyle.Bold, TextAnchor.MiddleLeft, Color.white);
        SetFullRectWithPadding(label.rectTransform, 12f, 6f);
        label.rectTransform.offsetMax = new Vector2(-28f, -6f);

        Text arrow = CreateUiText(dropdownObject.transform, "Arrow", "▼", 18, FontStyle.Bold, TextAnchor.MiddleCenter, Color.white);
        SetRect(arrow.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(24f, 24f), new Vector2(-12f, 0f), new Vector2(1f, 0.5f));

        GameObject templateObject = CreateImage(dropdownObject.transform, "Template", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 140f), new Vector2(0f, -38f), new Color(0.08f, 0.10f, 0.14f, 0.98f));
        Canvas templateCanvas = templateObject.AddComponent<Canvas>();
        templateCanvas.overrideSorting = true;
        templateCanvas.sortingOrder = 10000;
        CanvasGroup templateGroup = templateObject.AddComponent<CanvasGroup>();
        templateGroup.alpha = 1f;
        templateGroup.interactable = true;
        templateGroup.blocksRaycasts = true;
        templateGroup.ignoreParentGroups = true;
        templateObject.SetActive(false);

        ScrollRect scrollRect = templateObject.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        GameObject viewportObject = CreateImage(templateObject.transform, "Viewport", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(1f, 1f, 1f, 0.03f));
        viewportObject.GetComponent<Image>().raycastTarget = false;
        Mask mask = viewportObject.AddComponent<Mask>();
        mask.showMaskGraphic = false;

        GameObject contentObject = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentObject.transform.SetParent(viewportObject.transform, false);
        RectTransform contentRect = contentObject.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.offsetMin = Vector2.zero;
        contentRect.offsetMax = Vector2.zero;

        VerticalLayoutGroup layout = contentObject.GetComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 2f;

        ContentSizeFitter fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject itemObject = CreateImage(contentObject.transform, "Item", Vector2.zero, Vector2.one, new Vector2(0f, 32f), Vector2.zero, new Color(0.12f, 0.15f, 0.20f, 0.94f));
        Toggle itemToggle = itemObject.AddComponent<Toggle>();
        itemToggle.targetGraphic = itemObject.GetComponent<Image>();

        GameObject checkmarkObject = CreateImage(itemObject.transform, "Checkmark", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 18f), new Vector2(14f, 0f), Color.white);
        itemToggle.graphic = checkmarkObject.GetComponent<Image>();

        Text itemLabel = CreateUiText(itemObject.transform, "Item Label", "Запись номер n", 18, FontStyle.Normal, TextAnchor.MiddleLeft, Color.white);
        SetFullRectWithPadding(itemLabel.rectTransform, 12f, 2f);

        scrollRect.viewport = viewportObject.GetComponent<RectTransform>();
        scrollRect.content = contentRect;

        dropdown.template = templateObject.GetComponent<RectTransform>();
        dropdown.captionText = label;
        dropdown.itemText = itemLabel;
        dropdown.alphaFadeSpeed = 0.15f;

        return dropdown;
    }

    private static GameObject CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        SetRect(imageObject.GetComponent<RectTransform>(), anchorMin, anchorMax, size, anchoredPosition, new Vector2(0.5f, 0.5f));
        imageObject.GetComponent<Image>().color = color;
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
        label.textWrappingMode = TextWrappingModes.Normal;

        if (TMP_Settings.defaultFontAsset != null)
        {
            label.font = TMP_Settings.defaultFontAsset;
        }

        return label;
    }

    private static Text CreateUiText(Transform parent, string name, string text, int fontSize, FontStyle fontStyle, TextAnchor alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);

        Text label = textObject.GetComponent<Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font != null)
        {
            label.font = font;
        }

        return label;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 size, Vector2 anchoredPosition, Color normalColor, Color textColor)
    {
        GameObject buttonObject = CreateImage(parent, name, new Vector2(0f, 1f), new Vector2(0f, 1f), size, anchoredPosition, normalColor);
        Button button = buttonObject.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = new Color(0.22f, 0.68f, 0.98f, 1f);
        colors.pressedColor = new Color(0.10f, 0.38f, 0.62f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        TMP_Text labelText = CreateText(buttonObject.transform, "Label", label, 19f, FontStyles.Bold, TextAlignmentOptions.Center, textColor);
        SetRect(labelText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        labelText.rectTransform.offsetMin = Vector2.zero;
        labelText.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    private static void SetFullRectWithPadding(RectTransform rect, float horizontal, float vertical)
    {
        SetRect(rect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        rect.offsetMin = new Vector2(horizontal, vertical);
        rect.offsetMax = new Vector2(-horizontal, -vertical);
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition, Vector2 pivot)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
    }

    private static void SavePrefab(GameObject root, string path)
    {
        try
        {
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);

            if (savedPrefab != null)
            {
                Debug.Log($"Lab 01 menu prefab created: {path}");
            }
            else
            {
                Debug.LogError($"Failed to create Lab 01 menu prefab: {path}");
            }
        }
        catch (System.Exception ex)
        {
            Object.DestroyImmediate(root);
            Debug.LogError($"Failed to create Lab 01 menu prefab: {path}\n{ex}");
        }
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
