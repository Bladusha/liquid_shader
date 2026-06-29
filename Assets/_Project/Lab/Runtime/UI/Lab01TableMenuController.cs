using System;
using System.Collections.Generic;
using Lab.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Lab01TableMenuController : MonoBehaviour
{
    private const string MenuId = "lab_table";
    private const int EditableRowCount = 2;

    private static readonly string[] ColumnTitles =
    {
        "№", "d, м", "f, м2", "V, м3/с", "w, м/с", "t, °C", "ν, м2/с", "Re", "Режим"
    };

    private static readonly string[] InputNames =
    {
        "AttemptInput", "DiameterInput", "AreaInput", "FlowInput", "VelocityInput", "TemperatureInput", "ViscosityInput", "ReynoldsInput", "RegimeInput"
    };

    private static readonly float[] ColumnWidths = { 92f, 124f, 124f, 124f, 124f, 124f, 124f, 124f, 150f };
    private const float TableHeaderHeight = 96f;
    private const float TableCellHeight = 68f;
    private const float TableCellSpacing = 0f;
    private const float TableCellPadding = 0f;
    private static readonly Color InputTextColor = new Color(0.031f, 0.494f, 0.686f, 0.58f);
    private static readonly Color InputPlaceholderColor = new Color(0.25f, 0.43f, 0.50f, 0.38f);

    [Header("Resolved References")]
    [SerializeField] private RectTransform tableRowBox;
    [SerializeField] private TMP_Text snapshotText;
    [SerializeField] private TMP_Text calculatedDataText;
    [SerializeField] private Dropdown recordDropdown;
    [SerializeField] private Button calculationButton;
    [SerializeField] private Button closeButton;

    private readonly List<ManualRow> rows = new List<ManualRow>(EditableRowCount);
    private WaterController waterController;
    private Action closeRequested;
    private Action calculationRequested;
    private int lastSnapshotVersion = -1;
    private int selectedRecordIndex = -1;
    private bool layoutReady;

    private void Awake()
    {
        EnsureEditableLayout();
        RefreshBindings();
    }

    private void OnEnable()
    {
        MenuVisibilityCoordinator.MenuOpening += HandleOtherMenuOpening;
        EnsureEditableLayout();
        RefreshBindings();
    }

    private void OnDisable()
    {
        MenuVisibilityCoordinator.MenuOpening -= HandleOtherMenuOpening;
    }

    public void Configure(
        RectTransform tableBox,
        TMP_Text snapshot,
        TMP_Text calculatedData,
        Dropdown recordView,
        Button calculationBtn,
        Button close)
    {
        tableRowBox = tableBox;
        snapshotText = snapshot;
        calculatedDataText = calculatedData;
        recordDropdown = recordView;
        calculationButton = calculationBtn;
        closeButton = close;
        EnsureEditableLayout();
        RefreshBindings();
    }

    public void Open(WaterController controller, Action closeHandler, Action calculationHandler)
    {
        if (controller != null)
        {
            waterController = controller;
        }

        closeRequested = closeHandler;
        calculationRequested = calculationHandler;
        gameObject.SetActive(true);
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, true);
        EnsureEditableLayout();
        EnsureRecordDropdownOptions(true);
        RefreshView();
        lastSnapshotVersion = Lab01WorkSession.SnapshotVersion;
    }

    public void RefreshBindings()
    {
        Bind(calculationButton, OpenCalculations);
        Bind(closeButton, Close);

        if (recordDropdown != null)
        {
            recordDropdown.onValueChanged.RemoveListener(OnRecordDropdownChanged);
            recordDropdown.onValueChanged.AddListener(OnRecordDropdownChanged);
        }

        for (int i = 0; i < rows.Count; i++)
        {
            rows[i].Bind(OnRowInputChanged);
        }
    }

    private void Update()
    {
        if (!gameObject.activeInHierarchy)
        {
            return;
        }

        if (lastSnapshotVersion != Lab01WorkSession.SnapshotVersion)
        {
            lastSnapshotVersion = Lab01WorkSession.SnapshotVersion;
            EnsureRecordDropdownOptions(true);
            RefreshView();
        }
        else
        {
            UpdateRowAvailability();
        }

        if (InputSystemCompat.GetKeyDown(KeyCode.Tab) || InputSystemCompat.GetKeyDown(KeyCode.Escape))
        {
            if (InputSystemCompat.GetKeyDown(KeyCode.Tab))
            {
                MenuVisibilityCoordinator.MarkTabHandled();
            }

            CloseMenu();
        }
    }

    private void HandleOtherMenuOpening(string menuId)
    {
        if (menuId != MenuId)
        {
            CloseMenu();
        }
    }

    private void RefreshView()
    {
        if (waterController == null)
        {
            waterController = FindAnyObjectByType<WaterController>();
        }

        UpdateDataBlock();
        UpdateRowAvailability();

        if (calculationButton != null)
        {
            calculationButton.interactable = true;
        }
    }

    private void EnsureEditableLayout()
    {
        if (!layoutReady)
        {
            DisableLegacyBlocks();
        }

        ResolveMainReferences();
        if (tableRowBox == null)
        {
            return;
        }

        EnsureSidebarBlocks();
        EnsureTableRows();
        ResolveRowReferences();
        layoutReady = true;
    }

    private void ResolveMainReferences()
    {
        if (tableRowBox == null)
        {
            tableRowBox = FindRectTransform("TableRowBox");
        }

        if (recordDropdown == null)
        {
            recordDropdown = FindComponentInChildren<Dropdown>("RecordedDataDropdown");
        }

        if (snapshotText == null)
        {
            snapshotText = FindComponentInChildren<TMP_Text>("SnapshotText");
        }

        if (calculatedDataText == null)
        {
            calculatedDataText = FindComponentInChildren<TMP_Text>("CalculatedDataText");
        }

        if (calculationButton == null)
        {
            calculationButton = FindComponentInChildren<Button>("CalculationButton");
        }

        if (closeButton == null)
        {
            closeButton = FindComponentInChildren<Button>("CloseButton");
        }
    }

    private void EnsureSidebarBlocks()
    {
        RectTransform panel = FindRectTransform("Panel");
        if (panel == null)
        {
            return;
        }

        EnsureButtonVisual(calculationButton);
        EnsureButtonVisual(closeButton);

        GameObject snapshotBox = FindChild(panel, "SnapshotBox");
        if (snapshotBox != null)
        {
            Image image = snapshotBox.GetComponent<Image>();
            if (image != null)
            {
                image.color = Color.white;
            }
        }

        if (snapshotText == null)
        {
            if (snapshotBox == null)
            {
                if (!Application.isPlaying)
                {
                    return;
                }

                snapshotBox = CreateImage(panel, "SnapshotBox", new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, 270f), new Vector2(92f, -180f), Color.white);
                snapshotBox.GetComponent<RectTransform>().pivot = new Vector2(0f, 1f);
            }

            snapshotText = FindComponentInChildren<TMP_Text>("SnapshotText");
            if (snapshotText == null)
            {
                if (!Application.isPlaying)
                {
                    return;
                }

                snapshotText = CreateText(snapshotBox.transform, "SnapshotText", string.Empty, 16f, FontStyles.Normal, TextAlignmentOptions.TopLeft, HydrodynamicsUiTheme.MutedText);
                SetFullRectWithPadding(snapshotText.rectTransform, 18f, 16f);
            }
        }

        if (recordDropdown == null)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            recordDropdown = CreateDropdown(panel, "RecordedDataDropdown", new Vector2(270f, 54f), new Vector2(252f, -470f), Color.white);
        }
        else
        {
            Graphic targetGraphic = recordDropdown.targetGraphic;
            if (targetGraphic != null)
            {
                targetGraphic.color = Color.white;
            }
        }

        GameObject dataBox = FindChild(panel, "CalculatedDataBox");
        if (dataBox != null)
        {
            dataBox.SetActive(false);
        }
    }

    private void EnsureTableRows()
    {
        if (tableRowBox == null)
        {
            return;
        }

        Transform legacyText = tableRowBox.Find("TableRowText");
        if (legacyText != null)
        {
            legacyText.gameObject.SetActive(false);
        }

        RectTransform headerRow = FindRectTransform("TableHeaderRow", tableRowBox);
        if (headerRow == null)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            GameObject headerObject = new GameObject("TableHeaderRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            headerObject.transform.SetParent(tableRowBox, false);
            headerRow = headerObject.GetComponent<RectTransform>();
            headerRow.anchorMin = new Vector2(0f, 1f);
            headerRow.anchorMax = new Vector2(1f, 1f);
            headerRow.pivot = new Vector2(0.5f, 1f);
            headerRow.anchoredPosition = Vector2.zero;
            headerRow.sizeDelta = new Vector2(0f, TableHeaderHeight);

            HorizontalLayoutGroup layout = headerObject.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = TableCellSpacing;
            layout.padding = new RectOffset((int)TableCellPadding, (int)TableCellPadding, 0, 0);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;

            for (int i = 0; i < ColumnTitles.Length; i++)
            {
                TMP_Text headerLabel = CreateText(headerObject.transform, $"{InputNames[i]}Header", ColumnTitles[i], 28f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.08f, 0.18f, 0.24f, 1f));
                headerLabel.textWrappingMode = TextWrappingModes.NoWrap;
                LayoutElement element = headerLabel.gameObject.AddComponent<LayoutElement>();
                element.preferredWidth = ColumnWidths[i];
                element.minWidth = ColumnWidths[i];
                element.preferredHeight = TableHeaderHeight;
                element.minHeight = TableHeaderHeight;
            }
        }
        else
        {
            // Existing prefab layout is authoritative. Do not rewrite spacing,
            // padding, or sizing here because the menu is hand-tuned in Prefab Mode.
        }

        RectTransform rowsRoot = FindRectTransform("ManualRowsRoot", tableRowBox);
        if (rowsRoot == null)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            GameObject rowsRootObject = new GameObject("ManualRowsRoot", typeof(RectTransform), typeof(VerticalLayoutGroup));
            rowsRootObject.transform.SetParent(tableRowBox, false);
            rowsRoot = rowsRootObject.GetComponent<RectTransform>();
            rowsRoot.anchorMin = new Vector2(0f, 1f);
            rowsRoot.anchorMax = new Vector2(1f, 1f);
            rowsRoot.pivot = new Vector2(0.5f, 1f);
            rowsRoot.anchoredPosition = new Vector2(0f, -TableHeaderHeight);
            rowsRoot.sizeDelta = Vector2.zero;

            VerticalLayoutGroup layout = rowsRootObject.GetComponent<VerticalLayoutGroup>();
            layout.spacing = TableCellSpacing;
            layout.padding = new RectOffset((int)TableCellPadding, (int)TableCellPadding, 0, 0);
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;
        }
        else
        {
            // Existing prefab layout is authoritative. Do not rewrite spacing,
            // padding, or sizing here because the menu is hand-tuned in Prefab Mode.
        }

        for (int rowIndex = 0; rowIndex < EditableRowCount; rowIndex++)
        {
            string rowName = $"TableInputRow{rowIndex + 1}";
            RectTransform rowTransform = FindRectTransform(rowName, rowsRoot);
            if (rowTransform != null)
            {
                continue;
            }

            if (!Application.isPlaying)
            {
                return;
            }

            GameObject rowObject = new GameObject(rowName, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            rowObject.transform.SetParent(rowsRoot, false);
            rowTransform = rowObject.GetComponent<RectTransform>();

            LayoutElement rowLayout = rowObject.GetComponent<LayoutElement>();
            rowLayout.preferredHeight = TableCellHeight;
            rowLayout.minHeight = TableCellHeight;

            HorizontalLayoutGroup horizontal = rowObject.GetComponent<HorizontalLayoutGroup>();
            horizontal.spacing = TableCellSpacing;
            horizontal.childAlignment = TextAnchor.MiddleLeft;
            horizontal.childForceExpandHeight = false;
            horizontal.childForceExpandWidth = false;

            for (int i = 0; i < InputNames.Length; i++)
            {
                if (i == 0)
                {
                    CreateRowLabel(rowObject.transform, InputNames[i], (rowIndex + 1).ToString(), ColumnWidths[i]);
                    continue;
                }

                TMP_InputField input = CreateRowInput(rowObject.transform, InputNames[i], ColumnTitles[i], ColumnWidths[i]);
                input.contentType = i == InputNames.Length - 1 ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.DecimalNumber;
            }
        }

    }

    private void ResolveRowReferences()
    {
        rows.Clear();

        RectTransform rowsRoot = FindRectTransform("ManualRowsRoot", tableRowBox);
        if (rowsRoot == null)
        {
            return;
        }

        for (int i = 0; i < rowsRoot.childCount && rows.Count < EditableRowCount; i++)
        {
            Transform rowTransform = rowsRoot.GetChild(i);
            if (!rowTransform.gameObject.activeSelf)
            {
                continue;
            }

            ManualRow row = ManualRow.From(rowTransform);
            if (row.IsValid)
            {
                row.SetAttemptNumber(rows.Count + 1);
                row.ApplyTextStyle(InputTextColor, InputPlaceholderColor);
                rows.Add(row);
            }
        }
    }

    private void EnsureRecordDropdownOptions(bool preferLatest)
    {
        if (recordDropdown == null)
        {
            return;
        }

        recordDropdown.ClearOptions();

        int recordCount = Lab01WorkSession.RecordCount;
        if (recordCount == 0)
        {
            selectedRecordIndex = -1;
            recordDropdown.AddOptions(new List<string> { "Записи не найдены" });
            recordDropdown.interactable = false;
            recordDropdown.SetValueWithoutNotify(0);
            recordDropdown.RefreshShownValue();
            UpdateDataBlock();
            return;
        }

        recordDropdown.interactable = true;
        List<string> options = new List<string>(recordCount);
        for (int i = 0; i < recordCount; i++)
        {
            Lab01WorkSession.RecordEntry entry = Lab01WorkSession.RecordHistory[i];
            options.Add($"Запись номер {entry.AttemptNumber}");
        }

        recordDropdown.AddOptions(options);

        if (preferLatest || selectedRecordIndex < 0 || selectedRecordIndex >= recordCount)
        {
            selectedRecordIndex = recordCount - 1;
        }
        else
        {
            selectedRecordIndex = Mathf.Clamp(selectedRecordIndex, 0, recordCount - 1);
        }

        recordDropdown.SetValueWithoutNotify(selectedRecordIndex);
        recordDropdown.RefreshShownValue();
    }

    private void OnRecordDropdownChanged(int index)
    {
        selectedRecordIndex = index;
        RefreshView();
    }

    private void UpdateDataBlock()
    {
        if (snapshotText == null)
        {
            return;
        }

        if (!TryGetSelectedRecord(out Lab01WorkSession.RecordEntry entry))
        {
            snapshotText.text = "Данные\n\nВыберите запись.";
            return;
        }

        string measuredText =
            $"Исходная запись №{entry.AttemptNumber}\n" +
            $"d = {Lab01WorkSession.Format(entry.Measurements.pipeInnerDiameterMeters, "F3")} м\n" +
            $"V = {Lab01WorkSession.Format(entry.Measurements.volumetricFlowCubicMetersPerSecond, "F6")} м3/с\n" +
            $"t = {Lab01WorkSession.Format(entry.Measurements.temperatureCelsius, "F1")} °C";

        if (!Lab01WorkSession.TryGetSubmittedRow(entry.AttemptNumber, out Lab01WorkSession.SubmittedEntry submitted))
        {
            snapshotText.text =
                $"Данные\n\n" +
                $"{measuredText}\n\n" +
                "Расчёты ещё не подтверждены.";
            return;
        }

        snapshotText.text =
            $"Данные\n\n" +
            $"{measuredText}\n\n" +
            "Расчёты\n" +
            $"№ = {submitted.AttemptNumber}\n" +
            $"f = {Lab01WorkSession.Format(submitted.Area, "F6")} м2\n" +
            $"w = {Lab01WorkSession.Format(submitted.Velocity, "F3")} м/с\n" +
            $"ν = {Lab01WorkSession.Format(submitted.Viscosity, "E3")} м2/с\n" +
            $"Re = {Lab01WorkSession.Format(submitted.Reynolds, "F0")}\n" +
            $"Режим = {submitted.Regime}";
    }

    private void UpdateRowAvailability()
    {
        bool canEditCurrentRow = true;

        for (int i = 0; i < rows.Count; i++)
        {
            rows[i].SetInteractable(canEditCurrentRow);
            canEditCurrentRow = rows[i].IsComplete;
        }
    }

    private void OnRowInputChanged(string _)
    {
        UpdateRowAvailability();
    }

    private void OpenCalculations()
    {
        gameObject.SetActive(false);
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, false);
        calculationRequested?.Invoke();
    }

    public void CloseMenu()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        gameObject.SetActive(false);
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, false);
        closeRequested?.Invoke();
    }

    private void Close()
    {
        CloseMenu();
    }

    private void DisableLegacyBlocks()
    {
        SetInactiveIfFound("LiveDataBox");
        SetInactiveIfFound("StatusText");
    }

    private void SetInactiveIfFound(string objectName)
    {
        Transform target = transform.Find(objectName);
        if (target == null)
        {
            target = FindChildRecursive(transform, objectName);
        }

        if (target != null)
        {
            target.gameObject.SetActive(false);
        }
    }

    private static Transform FindChildRecursive(Transform parent, string objectName)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == objectName)
            {
                return child;
            }

            Transform found = FindChildRecursive(child, objectName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    private RectTransform FindRectTransform(string objectName, Transform root = null)
    {
        Transform parent = root != null ? root : transform;
        Transform target = FindChildRecursive(parent, objectName);
        return target != null ? target as RectTransform : null;
    }

    private T FindComponentInChildren<T>(string objectName) where T : Component
    {
        Transform target = FindChildRecursive(transform, objectName);
        return target != null ? target.GetComponent<T>() : null;
    }

    private static GameObject FindChild(Transform parent, string objectName)
    {
        Transform child = parent.Find(objectName);
        return child != null ? child.gameObject : null;
    }

    private static void EnsureButtonVisual(Button button)
    {
        if (button == null)
        {
            return;
        }

        Graphic targetGraphic = button.targetGraphic;
        if (targetGraphic != null)
        {
            targetGraphic.color = Color.white;
        }

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = Color.white;
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(1f, 1f, 1f, 0.55f);
        button.colors = colors;
        Lab.UI.HydrodynamicWaterButtonAnimator.EnsureOn(button);
    }

    private static void Bind(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private static GameObject CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        imageObject.GetComponent<Image>().color = color;
        return imageObject;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, float fontSize, FontStyles style, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
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

    private static Text CreateUiText(Transform parent, string name, string text, int fontSize, FontStyle style, TextAnchor alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);

        Text label = textObject.GetComponent<Text>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = style;
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

    private static TMP_InputField CreateRowInput(Transform parent, string name, string placeholder, float width)
    {
        GameObject inputObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement), typeof(TMP_InputField));
        inputObject.transform.SetParent(parent, false);

        Image image = inputObject.GetComponent<Image>();
        image.color = Color.white;

        LayoutElement layout = inputObject.GetComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.minWidth = width;
        layout.preferredHeight = TableCellHeight;
        layout.minHeight = TableCellHeight;

        TMP_InputField input = inputObject.GetComponent<TMP_InputField>();
        input.targetGraphic = image;
        input.contentType = name == "RegimeInput" ? TMP_InputField.ContentType.Standard : TMP_InputField.ContentType.DecimalNumber;

        TMP_Text text = CreateText(inputObject.transform, "Text", string.Empty, 16f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, InputTextColor);
        SetFullRectWithPadding(text.rectTransform, 10f, 6f);

        TMP_Text placeholderText = CreateText(inputObject.transform, "Placeholder", placeholder, 14f, FontStyles.Italic, TextAlignmentOptions.MidlineLeft, InputPlaceholderColor);
        SetFullRectWithPadding(placeholderText.rectTransform, 10f, 6f);

        input.textComponent = text;
        input.placeholder = placeholderText;
        input.lineType = TMP_InputField.LineType.SingleLine;
        return input;
    }

    private static TMP_Text CreateRowLabel(Transform parent, string name, string text, float width)
    {
        GameObject labelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        labelObject.transform.SetParent(parent, false);

        Image image = labelObject.GetComponent<Image>();
        image.color = Color.white;

        LayoutElement layout = labelObject.GetComponent<LayoutElement>();
        layout.preferredWidth = width;
        layout.minWidth = width;
        layout.preferredHeight = TableCellHeight;
        layout.minHeight = TableCellHeight;

        TMP_Text label = CreateText(labelObject.transform, "Label", text, 24f, FontStyles.Normal, TextAlignmentOptions.Center, InputTextColor);
        SetFullRectWithPadding(label.rectTransform, 10f, 6f);
        return label;
    }

    private static Dropdown CreateDropdown(Transform parent, string name, Vector2 size, Vector2 anchoredPosition, Color backgroundColor)
    {
        GameObject dropdownObject = CreateImage(parent, name, new Vector2(0f, 1f), new Vector2(0f, 1f), size, anchoredPosition, backgroundColor);
        RectTransform rect = dropdownObject.GetComponent<RectTransform>();
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;

        Dropdown dropdown = dropdownObject.AddComponent<Dropdown>();
        dropdown.targetGraphic = dropdownObject.GetComponent<Image>();

        dropdown.colors = HydrodynamicsUiTheme.ButtonColors(backgroundColor);

        Text label = CreateUiText(dropdownObject.transform, "Label", "Записи не найдены", 18, FontStyle.Normal, TextAnchor.MiddleLeft, HydrodynamicsUiTheme.Text);
        SetFullRectWithPadding(label.rectTransform, 12f, 6f);
        label.rectTransform.offsetMax = new Vector2(-28f, -6f);

        Text arrow = CreateUiText(dropdownObject.transform, "Arrow", "▼", 18, FontStyle.Bold, TextAnchor.MiddleCenter, HydrodynamicsUiTheme.WaterDark);
        SetRect(arrow.rectTransform, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(24f, 24f), new Vector2(-12f, 0f), new Vector2(1f, 0.5f));

        GameObject templateObject = CreateImage(dropdownObject.transform, "Template", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 140f), new Vector2(0f, -38f), HydrodynamicsUiTheme.Surface);
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

        GameObject itemObject = CreateImage(contentObject.transform, "Item", Vector2.zero, Vector2.one, new Vector2(0f, 32f), Vector2.zero, HydrodynamicsUiTheme.SurfaceMuted);
        Toggle itemToggle = itemObject.AddComponent<Toggle>();
        itemToggle.targetGraphic = itemObject.GetComponent<Image>();

        GameObject checkmarkObject = CreateImage(itemObject.transform, "Checkmark", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(18f, 18f), new Vector2(14f, 0f), Color.white);
        RectTransform checkRect = checkmarkObject.GetComponent<RectTransform>();
        checkRect.pivot = new Vector2(0.5f, 0.5f);
        checkRect.anchorMin = new Vector2(0f, 0.5f);
        checkRect.anchorMax = new Vector2(0f, 0.5f);
        checkRect.anchoredPosition = new Vector2(14f, 0f);
        itemToggle.graphic = checkmarkObject.GetComponent<Image>();

        Text itemLabel = CreateUiText(itemObject.transform, "Item Label", "Запись номер n", 18, FontStyle.Normal, TextAnchor.MiddleLeft, HydrodynamicsUiTheme.Text);
        SetFullRectWithPadding(itemLabel.rectTransform, 12f, 2f);

        scrollRect.viewport = viewportObject.GetComponent<RectTransform>();
        scrollRect.content = contentRect;

        dropdown.template = templateObject.GetComponent<RectTransform>();
        dropdown.captionText = label;
        dropdown.itemText = itemLabel;
        dropdown.alphaFadeSpeed = 0.15f;
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string> { "Записи не найдены" });
        dropdown.RefreshShownValue();

        return dropdown;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition, Vector2 pivot)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
    }

    private static void SetFullRectWithPadding(RectTransform rect, float horizontalPadding, float verticalPadding)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(horizontalPadding, verticalPadding);
        rect.offsetMax = new Vector2(-horizontalPadding, -verticalPadding);
    }

    private bool TryGetSelectedRecord(out Lab01WorkSession.RecordEntry entry)
    {
        entry = default;
        if (selectedRecordIndex < 0 || selectedRecordIndex >= Lab01WorkSession.RecordCount)
        {
            return false;
        }

        entry = Lab01WorkSession.RecordHistory[selectedRecordIndex];
        return true;
    }

    private readonly struct ManualRow
    {
        public readonly TMP_Text Attempt;
        public readonly TMP_InputField Diameter;
        public readonly TMP_InputField Area;
        public readonly TMP_InputField Flow;
        public readonly TMP_InputField Velocity;
        public readonly TMP_InputField Temperature;
        public readonly TMP_InputField Viscosity;
        public readonly TMP_InputField Reynolds;
        public readonly TMP_InputField Regime;

        public ManualRow(
            TMP_Text attempt,
            TMP_InputField diameter,
            TMP_InputField area,
            TMP_InputField flow,
            TMP_InputField velocity,
            TMP_InputField temperature,
            TMP_InputField viscosity,
            TMP_InputField reynolds,
            TMP_InputField regime)
        {
            Attempt = attempt;
            Diameter = diameter;
            Area = area;
            Flow = flow;
            Velocity = velocity;
            Temperature = temperature;
            Viscosity = viscosity;
            Reynolds = reynolds;
            Regime = regime;
        }

        public bool IsValid =>
            Attempt != null &&
            Diameter != null &&
            Area != null &&
            Flow != null &&
            Velocity != null &&
            Temperature != null &&
            Viscosity != null &&
            Reynolds != null &&
            Regime != null;

        public bool IsComplete =>
            HasValue(Diameter) &&
            HasValue(Area) &&
            HasValue(Flow) &&
            HasValue(Velocity) &&
            HasValue(Temperature) &&
            HasValue(Viscosity) &&
            HasValue(Reynolds) &&
            HasValue(Regime);

        public void Bind(UnityEngine.Events.UnityAction<string> handler)
        {
            Bind(Diameter, handler);
            Bind(Area, handler);
            Bind(Flow, handler);
            Bind(Velocity, handler);
            Bind(Temperature, handler);
            Bind(Viscosity, handler);
            Bind(Reynolds, handler);
            Bind(Regime, handler);
        }

        public void SetAttemptNumber(int number)
        {
            Attempt.text = number.ToString();
        }

        public void ApplyTextStyle(Color textColor, Color placeholderColor)
        {
            if (Attempt != null)
            {
                Attempt.color = textColor;
            }
            ApplyTextStyle(Diameter, textColor, placeholderColor);
            ApplyTextStyle(Area, textColor, placeholderColor);
            ApplyTextStyle(Flow, textColor, placeholderColor);
            ApplyTextStyle(Velocity, textColor, placeholderColor);
            ApplyTextStyle(Temperature, textColor, placeholderColor);
            ApplyTextStyle(Viscosity, textColor, placeholderColor);
            ApplyTextStyle(Reynolds, textColor, placeholderColor);
            ApplyTextStyle(Regime, textColor, placeholderColor);
        }

        public void SetInteractable(bool interactable)
        {
            SetInteractable(Diameter, interactable);
            SetInteractable(Area, interactable);
            SetInteractable(Flow, interactable);
            SetInteractable(Velocity, interactable);
            SetInteractable(Temperature, interactable);
            SetInteractable(Viscosity, interactable);
            SetInteractable(Reynolds, interactable);
            SetInteractable(Regime, interactable);
        }

        public static ManualRow From(Transform rowTransform)
        {
            return new ManualRow(
                FindLabel(rowTransform, "AttemptInput"),
                FindInput(rowTransform, "DiameterInput"),
                FindInput(rowTransform, "AreaInput"),
                FindInput(rowTransform, "FlowInput"),
                FindInput(rowTransform, "VelocityInput"),
                FindInput(rowTransform, "TemperatureInput"),
                FindInput(rowTransform, "ViscosityInput"),
                FindInput(rowTransform, "ReynoldsInput"),
                FindInput(rowTransform, "RegimeInput"));
        }

        private static TMP_InputField FindInput(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            return child != null ? child.GetComponent<TMP_InputField>() : null;
        }

        private static TMP_Text FindLabel(Transform parent, string name)
        {
            Transform child = parent.Find(name);
            return child != null ? child.GetComponentInChildren<TMP_Text>(true) : null;
        }

        private static bool HasValue(TMP_InputField input)
        {
            return input != null && !string.IsNullOrWhiteSpace(input.text);
        }

        private static void ApplyTextStyle(TMP_InputField input, Color textColor, Color placeholderColor)
        {
            if (input == null)
            {
                return;
            }

            if (input.textComponent != null)
            {
                input.textComponent.color = textColor;
            }

            if (input.placeholder is TMP_Text placeholder)
            {
                placeholder.color = placeholderColor;
            }
        }

        private static void Bind(TMP_InputField input, UnityEngine.Events.UnityAction<string> handler)
        {
            if (input == null)
            {
                return;
            }

            input.onValueChanged.RemoveListener(handler);
            input.onValueChanged.AddListener(handler);
        }

        private static void SetInteractable(TMP_InputField input, bool interactable)
        {
            if (input == null)
            {
                return;
            }

            if (input.readOnly)
            {
                input.interactable = false;
                if (input.targetGraphic is Image readOnlyImage)
                {
                    readOnlyImage.color = Color.white;
                }

                return;
            }

            input.interactable = interactable;
            if (input.targetGraphic is Image image)
            {
                image.color = interactable
                    ? Color.white
                    : new Color(1f, 1f, 1f, 0.55f);
            }
        }
    }
}
