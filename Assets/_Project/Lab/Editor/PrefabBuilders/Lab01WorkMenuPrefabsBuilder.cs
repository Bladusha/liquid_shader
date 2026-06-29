using System.IO;
using System.Collections.Generic;
using Lab.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class Lab01WorkMenuPrefabsBuilder
{
    private const string TablePrefabPath = "Assets/_Project/Lab/Prefabs/Lab01TableMenu.prefab";
    private const string CalculationPrefabPath = "Assets/_Project/Lab/Prefabs/Lab01CalculationMenu.prefab";
    private const string PanelLargeSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/UnityPng/panels/panel_large_header.png";
    private const string PanelSideSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/UnityPng/panels/panel_medium_side.png";
    private const string ButtonSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/UnityPng/buttons/button_primary_water_frame.png";
    private const string InputIdleSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/UnityPng/forms/input_idle.png";
    private const string InputFocusSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/UnityPng/forms/input_focus.png";
    private const string InputErrorSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/UnityPng/forms/input_error.png";
    private const string DropdownClosedSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/UnityPng/forms/dropdown_closed.png";
    private const string DropdownOpenSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/UnityPng/forms/dropdown_open.png";
    private const string TableRowHeaderSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/UnityPng/tables/table_row_header_1x1.png";
    private const string TableColumnHeaderSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/UnityPng/tables/table_column_header_2x1.png";
    private const string TableInputIdleSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/UnityPng/tables/table_input_2x1_idle.png";
    private const string TableInputFocusSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/UnityPng/tables/table_input_2x1_focus.png";
    private const string TableInputErrorSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/UnityPng/tables/table_input_2x1_error.png";
    private static readonly Color TextBlue = new Color(0.043f, 0.435f, 0.624f, 1f);
    private static readonly Color TextInputBlue = new Color(0.031f, 0.494f, 0.686f, 1f);
    private static readonly Color TextMuted = new Color(0.42f, 0.498f, 0.537f, 1f);

    [MenuItem("Tools/LiquidShader/Create Lab 01 Work Menu Prefabs")]
    public static void CreatePrefabs()
    {
        EnsureFolder("Assets/_Project/Lab/Prefabs");
        SavePrefab(BuildTablePrefabHierarchy(), TablePrefabPath);
        SavePrefab(BuildCalculationPrefabHierarchy(), CalculationPrefabPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath);
    }

    [MenuItem("Tools/LiquidShader/Create Lab 01 Table Menu Prefab")]
    public static void CreateTablePrefab()
    {
        EnsureFolder("Assets/_Project/Lab/Prefabs");
        SavePrefab(BuildTablePrefabHierarchy(), TablePrefabPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath);
    }

    [MenuItem("Tools/LiquidShader/Create Lab 01 Calculation Menu Prefab")]
    public static void CreateCalculationPrefab()
    {
        EnsureFolder("Assets/_Project/Lab/Prefabs");
        SavePrefab(BuildCalculationPrefabHierarchy(), CalculationPrefabPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(CalculationPrefabPath);
    }

    private static GameObject BuildTablePrefabHierarchy()
    {
        Color inputColor = HydrodynamicsUiTheme.Field;
        Color mutedTextColor = HydrodynamicsUiTheme.MutedText;

        GameObject root = new GameObject("Lab01TableMenu", typeof(RectTransform), typeof(Lab01TableMenuController));
        SetRect(root.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));

        CreateImage(root.transform, "Backdrop", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.72f, 0.74f, 0.75f, 0.58f));

        GameObject panel = CreateSpriteImage(root.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1580f, 650f), Vector2.zero, LoadSprite(PanelLargeSpritePath));
        panel.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);

        TMP_Text title = CreateText(panel.transform, "Title", "ТАБЛИЦА ИЗМЕРЕНИЙ", 42f, FontStyles.Bold, TextAlignmentOptions.Center, TextBlue);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(900f, 58f), new Vector2(220f, -70f), new Vector2(0.5f, 1f));

        TMP_Text subtitle = CreateText(panel.transform, "Subtitle", "2 строки данных, плотная сетка, внешняя обводка по краям", 24f, FontStyles.Bold, TextAlignmentOptions.Center, TextMuted);
        SetRect(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(860f, 36f), new Vector2(220f, -120f), new Vector2(0.5f, 1f));

        Button closeButton = CreateButton(panel.transform, "CloseButton", "Закрыть", new Vector2(180f, 60f), new Vector2(1330f, -565f), Color.white, TextBlue);
        SetRect(closeButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(180f, 60f), new Vector2(1330f, -565f), new Vector2(0.5f, 0.5f));

        Button calculationButton = CreateButton(panel.transform, "CalculationButton", "К расчётам", new Vector2(260f, 60f), new Vector2(1030f, -565f), Color.white, TextBlue);
        SetRect(calculationButton.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(260f, 60f), new Vector2(1030f, -565f), new Vector2(0.5f, 0.5f));

        GameObject dataPanel = CreateSpriteImage(panel.transform, "DataPanel", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 250f), new Vector2(92f, -180f), LoadSprite(PanelSideSpritePath));
        TMP_Text dataTitle = CreateText(dataPanel.transform, "DataTitle", "Данные", 30f, FontStyles.Bold, TextAlignmentOptions.TopLeft, TextBlue);
        SetRect(dataTitle.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        dataTitle.rectTransform.offsetMin = new Vector2(36f, 178f);
        dataTitle.rectTransform.offsetMax = new Vector2(-24f, -22f);

        GameObject snapshotBox = CreateSpriteImage(dataPanel.transform, "SnapshotBox", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(250f, 108f), new Vector2(0f, 38f), LoadSprite(InputIdleSpritePath));
        TMP_Text snapshotText = CreateText(snapshotBox.transform, "SnapshotText", "Выберите запись.", 20f, FontStyles.Normal, TextAlignmentOptions.TopLeft, mutedTextColor);
        SetFullRectWithPadding(snapshotText.rectTransform, 18f, 14f);

        Dropdown recordDropdown = CreateDropdown(panel.transform, "RecordedDataDropdown", new Vector2(270f, 54f), new Vector2(252f, -470f), inputColor);
        recordDropdown.ClearOptions();
        recordDropdown.AddOptions(new System.Collections.Generic.List<string> { "Записи не найдены" });
        recordDropdown.RefreshShownValue();

        GameObject tableBox = CreateImage(panel.transform, "TableRowBox", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(1080f, 250f), new Vector2(470f, -180f), Color.clear);
        RectTransform tableBoxRect = tableBox.GetComponent<RectTransform>();
        tableBoxRect.pivot = new Vector2(0f, 1f);

        GameObject headerRow = new GameObject("TableHeaderRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        headerRow.transform.SetParent(tableBox.transform, false);
        RectTransform headerRect = headerRow.GetComponent<RectTransform>();
        SetRect(headerRect, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero, new Vector2(0.5f, 1f));
        HorizontalLayoutGroup headerLayout = headerRow.GetComponent<HorizontalLayoutGroup>();
        headerLayout.spacing = 0f;
        headerLayout.padding = new RectOffset(0, 0, 0, 0);
        headerLayout.childAlignment = TextAnchor.MiddleLeft;
        headerLayout.childForceExpandHeight = false;
        headerLayout.childForceExpandWidth = false;

        string[] columnTitles = { "№", "d, м", "f, м2", "V, м3/с", "w, м/с", "t, °C", "ν, м2/с", "Re", "Режим" };
        float[] columnWidths = { 92f, 124f, 124f, 124f, 124f, 124f, 124f, 124f, 150f };
        string[] inputNames = { "AttemptInput", "DiameterInput", "AreaInput", "FlowInput", "VelocityInput", "TemperatureInput", "ViscosityInput", "ReynoldsInput", "RegimeInput" };

        for (int i = 0; i < columnTitles.Length; i++)
        {
            Sprite headerSprite = i == 0 ? LoadSprite(TableRowHeaderSpritePath) : LoadSprite(TableColumnHeaderSpritePath);
            GameObject headerCell = CreateSpriteImage(headerRow.transform, $"{inputNames[i]}Header", Vector2.zero, Vector2.one, new Vector2(columnWidths[i], 96f), Vector2.zero, headerSprite);
            TMP_Text headerLabel = CreateText(headerCell.transform, "Label", columnTitles[i], 28f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.08f, 0.18f, 0.24f, 1f));
            SetRect(headerLabel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            headerLabel.rectTransform.offsetMin = new Vector2(3f, 0f);
            headerLabel.rectTransform.offsetMax = new Vector2(-3f, 0f);
            headerLabel.textWrappingMode = TextWrappingModes.NoWrap;
            LayoutElement element = headerCell.AddComponent<LayoutElement>();
            element.preferredWidth = columnWidths[i];
            element.minWidth = columnWidths[i];
            element.preferredHeight = 96f;
            element.minHeight = 96f;
        }

        GameObject rowsRoot = new GameObject("ManualRowsRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
        rowsRoot.transform.SetParent(tableBox.transform, false);
        RectTransform rowsRect = rowsRoot.GetComponent<RectTransform>();
        SetRect(rowsRect, new Vector2(0f, 1f), new Vector2(1f, 1f), Vector2.zero, new Vector2(0f, -96f), new Vector2(0.5f, 1f));
        VerticalLayoutGroup rowsLayout = rowsRoot.GetComponent<VerticalLayoutGroup>();
        rowsLayout.spacing = 0f;
        rowsLayout.padding = new RectOffset(0, 0, 0, 0);
        rowsLayout.childAlignment = TextAnchor.UpperLeft;
        rowsLayout.childForceExpandHeight = false;
        rowsLayout.childForceExpandWidth = false;

        for (int rowIndex = 0; rowIndex < 2; rowIndex++)
        {
            GameObject rowObject = new GameObject($"TableInputRow{rowIndex + 1}", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            rowObject.transform.SetParent(rowsRoot.transform, false);
            HorizontalLayoutGroup rowLayout = rowObject.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 0f;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childForceExpandHeight = false;
            rowLayout.childForceExpandWidth = false;

            LayoutElement rowElement = rowObject.GetComponent<LayoutElement>();
            rowElement.preferredHeight = 68f;
            rowElement.minHeight = 68f;

            for (int i = 0; i < inputNames.Length; i++)
            {
                if (i == 0)
                {
                    CreateTableLabel(rowObject.transform, inputNames[i], (rowIndex + 1).ToString(), columnWidths[i], 68f);
                    continue;
                }

                TMP_InputField input = CreateTableInput(rowObject.transform, inputNames[i], i == 0 ? (rowIndex + 1).ToString() : string.Empty, columnWidths[i], 68f);
                input.contentType = i == inputNames.Length - 1 ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.DecimalNumber;
            }
        }

        root.GetComponent<Lab01TableMenuController>().Configure(
            tableBoxRect,
            snapshotText,
            null,
            recordDropdown,
            calculationButton,
            closeButton);

        return root;
    }

    private static GameObject BuildCalculationPrefabHierarchy()
    {
        Color inputColor = HydrodynamicsUiTheme.Field;

        GameObject root = new GameObject("Lab01CalculationMenu", typeof(RectTransform), typeof(Lab01CalculationMenuController));
        SetRect(root.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));

        CreateImage(root.transform, "Backdrop", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.73f, 0.76f, 0.77f, 0.56f));

        GameObject panel = CreateSpriteImage(root.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1320f, 790f), Vector2.zero, LoadSprite(PanelLargeSpritePath));
        TMP_Text title = CreateText(panel.transform, "Title", "ЛР №1: расчёты", 42f, FontStyles.Bold, TextAlignmentOptions.Center, TextBlue);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(760f, 72f), new Vector2(0f, -50f), new Vector2(0.5f, 1f));

        Button closeButton = CreateButton(panel.transform, "CloseButton", "Закрыть", new Vector2(210f, 68f), new Vector2(1135f, -720f), Color.white, TextBlue);

        GameObject snapshotBox = CreateSpriteImage(panel.transform, "SnapshotBox", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(360f, 220f), new Vector2(960f, -210f), LoadSprite(PanelSideSpritePath));
        TMP_Text snapshotText = CreateText(snapshotBox.transform, "SnapshotText", "", 18f, FontStyles.Normal, TextAlignmentOptions.TopLeft, TextMuted);
        SetFullRectWithPadding(snapshotText.rectTransform, 16f, 12f);

        Dropdown recordDropdown = CreateDropdown(panel.transform, "RecordedDataDropdown", new Vector2(340f, 56f), new Vector2(960f, -505f), inputColor);
        recordDropdown.ClearOptions();
        recordDropdown.AddOptions(new System.Collections.Generic.List<string> { "Записи не найдены" });
        recordDropdown.RefreshShownValue();

        TMP_InputField diameterInput = CreateInput(panel.transform, "DiameterInput", "d, м", new Vector2(330f, -150f), inputColor, 360f, 64f);
        TMP_InputField areaInput = CreateInput(panel.transform, "AreaInput", "f, м2", new Vector2(330f, -225f), inputColor, 360f, 64f);
        TMP_InputField flowInput = CreateInput(panel.transform, "FlowInput", "V, м3/с", new Vector2(330f, -300f), inputColor, 360f, 64f);
        TMP_InputField velocityInput = CreateInput(panel.transform, "VelocityInput", "w, м/с", new Vector2(330f, -375f), inputColor, 360f, 64f);
        TMP_InputField temperatureInput = CreateInput(panel.transform, "TemperatureInput", "t, °C", new Vector2(330f, -450f), inputColor, 360f, 64f);
        TMP_InputField viscosityInput = CreateInput(panel.transform, "ViscosityInput", "ν, м2/с", new Vector2(330f, -525f), inputColor, 360f, 64f);
        TMP_InputField reynoldsInput = CreateInput(panel.transform, "ReynoldsInput", "Re", new Vector2(330f, -600f), inputColor, 360f, 64f);

        Button calculateAreaButton = CreateButton(panel.transform, "CalculateAreaButton", "Проверить f", new Vector2(285f, 68f), new Vector2(690f, -225f), Color.white, TextBlue);
        Button calculateVelocityButton = CreateButton(panel.transform, "CalculateVelocityButton", "Проверить w", new Vector2(285f, 68f), new Vector2(690f, -375f), Color.white, TextBlue);
        Button calculateViscosityButton = CreateButton(panel.transform, "CalculateViscosityButton", "Проверить ν", new Vector2(285f, 68f), new Vector2(690f, -525f), Color.white, TextBlue);
        Button calculateReynoldsButton = CreateButton(panel.transform, "CalculateReynoldsButton", "Проверить Re", new Vector2(285f, 68f), new Vector2(690f, -600f), Color.white, TextBlue);

        Button submitButton = CreateButton(panel.transform, "SubmitToTableButton", "Внести данные в таблицу", new Vector2(390f, 68f), new Vector2(845f, -720f), Color.white, TextBlue);
        Button tableButton = CreateButton(panel.transform, "TableButton", "К таблице", new Vector2(260f, 68f), new Vector2(550f, -720f), Color.white, TextBlue);
        TMP_Text validationStatusText = CreateText(panel.transform, "ValidationStatusText", "Проверка не выполнена", 22f, FontStyles.Normal, TextAlignmentOptions.TopLeft, TextBlue);
        SetRect(validationStatusText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(520f, 36f), new Vector2(330f, -675f), new Vector2(0f, 1f));

        TMP_Text errorCounterText = CreateText(panel.transform, "ErrorCounterText", "Ошибок: 0", 18f, FontStyles.Normal, TextAlignmentOptions.TopLeft, TextMuted);
        SetRect(errorCounterText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(260f, 28f), new Vector2(330f, -708f), new Vector2(0f, 1f));

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
            closeButton,
            validationStatusText,
            errorCounterText);

        return root;
    }

    private static TMP_InputField CreateInput(Transform parent, string name, string placeholder, Vector2 anchoredPosition, Color backgroundColor)
    {
        return CreateInput(parent, name, placeholder, anchoredPosition, backgroundColor, 240f);
    }

    private static TMP_InputField CreateInput(Transform parent, string name, string placeholder, Vector2 anchoredPosition, Color backgroundColor, float width)
    {
        return CreateInput(parent, name, placeholder, anchoredPosition, backgroundColor, width, 42f);
    }

    private static TMP_InputField CreateInput(Transform parent, string name, string placeholder, Vector2 anchoredPosition, Color backgroundColor, float width, float height)
    {
        GameObject inputObject = CreateImage(parent, name, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(width, height), anchoredPosition, backgroundColor);
        Image inputImage = inputObject.GetComponent<Image>();
        inputImage.sprite = LoadSprite(InputIdleSpritePath);
        inputImage.color = Color.white;

        TMP_InputField input = inputObject.AddComponent<TMP_InputField>();
        input.targetGraphic = inputImage;
        input.colors = DesignerSelectableColors();
        input.spriteState = new SpriteState
        {
            highlightedSprite = LoadSprite(InputFocusSpritePath),
            pressedSprite = LoadSprite(InputFocusSpritePath),
            selectedSprite = LoadSprite(InputFocusSpritePath),
            disabledSprite = LoadSprite(InputErrorSpritePath)
        };

        LayoutElement layoutElement = inputObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = width;
        layoutElement.minWidth = width;
        layoutElement.preferredHeight = height;
        layoutElement.minHeight = height;

        TMP_Text text = CreateText(inputObject.transform, "Text", "", 22f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, TextInputBlue);
        SetFullRectWithPadding(text.rectTransform, 14f, 4f);

        TMP_Text placeholderText = CreateText(inputObject.transform, "Placeholder", placeholder, 22f, FontStyles.Italic, TextAlignmentOptions.MidlineLeft, TextMuted);
        SetFullRectWithPadding(placeholderText.rectTransform, 14f, 4f);

        input.textComponent = text;
        input.placeholder = placeholderText;
        return input;
    }

    private static TMP_InputField CreateTableInput(Transform parent, string name, string placeholder, float width, float height)
    {
        GameObject inputObject = CreateSpriteImage(parent, name, Vector2.zero, Vector2.one, new Vector2(width, height), Vector2.zero, LoadSprite(TableInputIdleSpritePath));
        LayoutElement layoutElement = inputObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = width;
        layoutElement.minWidth = width;
        layoutElement.preferredHeight = height;
        layoutElement.minHeight = height;

        Image inputImage = inputObject.GetComponent<Image>();
        inputImage.color = Color.white;

        TMP_InputField input = inputObject.AddComponent<TMP_InputField>();
        input.targetGraphic = inputImage;
        input.colors = DesignerSelectableColors();
        input.spriteState = new SpriteState
        {
            highlightedSprite = LoadSprite(TableInputFocusSpritePath),
            pressedSprite = LoadSprite(TableInputFocusSpritePath),
            selectedSprite = LoadSprite(TableInputFocusSpritePath),
            disabledSprite = LoadSprite(TableInputErrorSpritePath)
        };

        TMP_Text text = CreateText(inputObject.transform, "Text", "", 22f, FontStyles.Bold, TextAlignmentOptions.Center, TextInputBlue);
        SetFullRectWithPadding(text.rectTransform, 8f, 4f);

        TMP_Text placeholderText = CreateText(inputObject.transform, "Placeholder", placeholder, 22f, FontStyles.Bold, TextAlignmentOptions.Center, TextMuted);
        SetFullRectWithPadding(placeholderText.rectTransform, 8f, 4f);

        input.textComponent = text;
        input.placeholder = placeholderText;
        input.lineType = TMP_InputField.LineType.SingleLine;
        return input;
    }

    private static TMP_Text CreateTableLabel(Transform parent, string name, string text, float width, float height)
    {
        GameObject labelObject = CreateSpriteImage(parent, name, Vector2.zero, Vector2.one, new Vector2(width, height), Vector2.zero, LoadSprite(TableRowHeaderSpritePath));
        LayoutElement layoutElement = labelObject.AddComponent<LayoutElement>();
        layoutElement.preferredWidth = width;
        layoutElement.minWidth = width;
        layoutElement.preferredHeight = height;
        layoutElement.minHeight = height;

        Image labelImage = labelObject.GetComponent<Image>();
        labelImage.color = Color.white;

        TMP_Text label = CreateText(labelObject.transform, "Label", text, 24f, FontStyles.Bold, TextAlignmentOptions.Center, TextInputBlue);
        SetFullRectWithPadding(label.rectTransform, 8f, 4f);
        return label;
    }

    private static Dropdown CreateDropdown(Transform parent, string name, Vector2 size, Vector2 anchoredPosition, Color backgroundColor)
    {
        GameObject dropdownObject = CreateImage(parent, name, new Vector2(0f, 1f), new Vector2(0f, 1f), size, anchoredPosition, backgroundColor);
        Image dropdownImage = dropdownObject.GetComponent<Image>();
        dropdownImage.sprite = LoadSprite(DropdownClosedSpritePath);
        dropdownImage.color = Color.white;

        Dropdown dropdown = dropdownObject.AddComponent<Dropdown>();
        dropdown.targetGraphic = dropdownImage;

        dropdown.colors = DesignerSelectableColors();
        dropdown.spriteState = new SpriteState
        {
            highlightedSprite = LoadSprite(DropdownClosedSpritePath),
            pressedSprite = LoadSprite(DropdownClosedSpritePath),
            selectedSprite = LoadSprite(DropdownClosedSpritePath)
        };

        Text label = CreateUiText(dropdownObject.transform, "Label", "Записи не найдены", 18, FontStyle.Bold, TextAnchor.MiddleLeft, TextBlue);
        SetFullRectWithPadding(label.rectTransform, 12f, 6f);
        label.rectTransform.offsetMax = new Vector2(-28f, -6f);

        Text arrow = CreateUiText(dropdownObject.transform, "Arrow", "▼", 18, FontStyle.Bold, TextAnchor.MiddleCenter, TextBlue);
        SetRect(arrow.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(24f, 24f), new Vector2(-12f, 0f), new Vector2(1f, 0.5f));

        GameObject templateObject = CreateImage(dropdownObject.transform, "Template", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 140f), new Vector2(0f, -38f), HydrodynamicsUiTheme.Surface);
        Image templateImage = templateObject.GetComponent<Image>();
        templateImage.sprite = LoadSprite(DropdownOpenSpritePath);
        templateImage.color = Color.white;
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

        GameObject viewportObject = CreateImage(templateObject.transform, "Viewport", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0.92f, 0.98f, 1f, 0.35f));
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

        GameObject itemObject = CreateImage(contentObject.transform, "Item", Vector2.zero, Vector2.one, new Vector2(0f, 32f), Vector2.zero, Color.white);
        itemObject.GetComponent<Image>().sprite = LoadSprite(InputIdleSpritePath);
        Toggle itemToggle = itemObject.AddComponent<Toggle>();
        itemToggle.targetGraphic = itemObject.GetComponent<Image>();

        GameObject checkmarkObject = CreateImage(itemObject.transform, "Checkmark", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 18f), new Vector2(14f, 0f), Color.white);
        itemToggle.graphic = checkmarkObject.GetComponent<Image>();

        Text itemLabel = CreateUiText(itemObject.transform, "Item Label", "Запись номер n", 18, FontStyle.Normal, TextAnchor.MiddleLeft, TextMuted);
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

    private static GameObject CreateSpriteImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition, Sprite sprite)
    {
        GameObject imageObject = CreateImage(parent, name, anchorMin, anchorMax, size, anchoredPosition, Color.white);
        Image image = imageObject.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.preserveAspect = false;
        image.raycastTarget = false;
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
        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.sprite = LoadSprite(ButtonSpritePath);
        buttonImage.color = Color.white;

        Button button = buttonObject.AddComponent<Button>();
        button.colors = DesignerSelectableColors();
        button.spriteState = new SpriteState
        {
            highlightedSprite = LoadSprite(ButtonSpritePath),
            pressedSprite = LoadSprite(ButtonSpritePath),
            selectedSprite = LoadSprite(ButtonSpritePath)
        };

        TMP_Text labelText = CreateText(buttonObject.transform, "Label", label, 24f, FontStyles.Bold, TextAlignmentOptions.Center, textColor);
        SetRect(labelText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        labelText.rectTransform.offsetMin = Vector2.zero;
        labelText.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    private static ColorBlock DesignerSelectableColors()
    {
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.985f, 1f, 1f);
        colors.pressedColor = new Color(0.74f, 0.91f, 0.97f, 1f);
        colors.selectedColor = new Color(0.9f, 0.985f, 1f, 1f);
        colors.disabledColor = new Color(0.74f, 0.82f, 0.86f, 0.55f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        return colors;
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

    private static void AddHeaderBand(Transform parent, float width, float height)
    {
        GameObject header = CreateImage(parent, "WaterHeader", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(width, height), new Vector2(0f, -24f), HydrodynamicsUiTheme.Water);
        header.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
        header.GetComponent<Image>().raycastTarget = false;

        GameObject accent = CreateImage(parent, "OrangeAccent", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(128f, 6f), new Vector2(0f, -102f), HydrodynamicsUiTheme.Accent);
        accent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
        accent.GetComponent<Image>().raycastTarget = false;
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
