using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Lab01CalculationMenuController : MonoBehaviour
{
    private const string MenuId = "lab_calculation";

    [SerializeField] private TMP_Text snapshotText;
    [SerializeField] private Dropdown recordDropdown;
    [SerializeField] private string emptyRecordsText = "Записи не найдены";
    [SerializeField] private TMP_InputField diameterInput;
    [SerializeField] private TMP_InputField areaInput;
    [SerializeField] private TMP_InputField flowInput;
    [SerializeField] private TMP_InputField velocityInput;
    [SerializeField] private TMP_InputField temperatureInput;
    [SerializeField] private TMP_InputField viscosityInput;
    [SerializeField] private TMP_InputField reynoldsInput;
    [SerializeField] private Button calculateAreaButton;
    [SerializeField] private Button calculateVelocityButton;
    [SerializeField] private Button calculateViscosityButton;
    [SerializeField] private Button calculateReynoldsButton;
    [SerializeField] private Button submitToTableButton;
    [SerializeField] private Button tableButton;
    [SerializeField] private Button closeButton;

    private WaterController waterController;
    private Action closeRequested;
    private Action tableRequested;
    private int lastSnapshotVersion = -1;
    private int selectedRecordIndex = -1;
    private readonly Dictionary<int, CalculationDraft> draftsByAttempt = new Dictionary<int, CalculationDraft>();
    private bool isRestoringDraft;

    public void Configure(
        TMP_Text snapshot,
        Dropdown recordView,
        TMP_InputField diameter,
        TMP_InputField area,
        TMP_InputField flow,
        TMP_InputField velocity,
        TMP_InputField temperature,
        TMP_InputField viscosity,
        TMP_InputField reynolds,
        Button calculateArea,
        Button calculateVelocity,
        Button calculateViscosity,
        Button calculateReynolds,
        Button submitToTable,
        Button table,
        Button close)
    {
        snapshotText = snapshot;
        recordDropdown = recordView;
        diameterInput = diameter;
        areaInput = area;
        flowInput = flow;
        velocityInput = velocity;
        temperatureInput = temperature;
        viscosityInput = viscosity;
        reynoldsInput = reynolds;
        calculateAreaButton = calculateArea;
        calculateVelocityButton = calculateVelocity;
        calculateViscosityButton = calculateViscosity;
        calculateReynoldsButton = calculateReynolds;
        submitToTableButton = submitToTable;
        tableButton = table;
        closeButton = close;
        RefreshBindings();
    }

    private void Awake()
    {
        RefreshBindings();
    }

    private void OnEnable()
    {
        MenuVisibilityCoordinator.MenuOpening += HandleOtherMenuOpening;
        RefreshBindings();
    }

    private void OnDisable()
    {
        MenuVisibilityCoordinator.MenuOpening -= HandleOtherMenuOpening;
    }

    private void HandleOtherMenuOpening(string menuId)
    {
        if (menuId != MenuId)
        {
            CloseMenu();
        }
    }

    public void Open(WaterController controller, Action closeHandler, Action tableHandler)
    {
        if (controller != null)
        {
            waterController = controller;
        }

        closeRequested = closeHandler;
        tableRequested = tableHandler;
        gameObject.SetActive(true);
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, true);
        EnsureRecordDropdownOptions(true);
        RefreshSnapshotView();
        RefreshCalculationView();
        UpdateCalculationState();
        lastSnapshotVersion = Lab01WorkSession.SnapshotVersion;
    }

    public void RefreshBindings()
    {
        Bind(calculateAreaButton, CalculateArea);
        Bind(calculateVelocityButton, CalculateVelocity);
        Bind(calculateViscosityButton, CalculateViscosity);
        Bind(calculateReynoldsButton, CalculateReynolds);
        Bind(submitToTableButton, SubmitToTable);
        Bind(tableButton, OpenTable);
        Bind(closeButton, Close);
        BindInput(diameterInput);
        BindInput(areaInput);
        BindInput(flowInput);
        BindInput(velocityInput);
        BindInput(temperatureInput);
        BindInput(viscosityInput);
        BindInput(reynoldsInput);

        if (recordDropdown != null)
        {
            recordDropdown.onValueChanged.RemoveListener(OnRecordDropdownChanged);
            recordDropdown.onValueChanged.AddListener(OnRecordDropdownChanged);
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
            RefreshSnapshotView();
            RefreshCalculationView();
            UpdateCalculationState();
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

    private static void Bind(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void BindInput(TMP_InputField input)
    {
        if (input == null)
        {
            return;
        }

        input.onValueChanged.RemoveListener(OnInputChanged);
        input.onValueChanged.AddListener(OnInputChanged);
    }

    private void OnInputChanged(string _)
    {
        SaveCurrentDraft();
        UpdateCalculationState();
    }

    private void RefreshSnapshotView()
    {
        UpdateRecordViewText();
    }

    private void RefreshCalculationView()
    {
        RestoreCurrentDraft();
    }

    private void CalculateArea()
    {
        if (!TryRead(diameterInput, out float d))
        {
            return;
        }

        if (!Lab01WorkSession.TrySolveArea(d, out float area, out _))
        {
            return;
        }

        SetInput(areaInput, area, "F6");
        SaveCurrentDraft();
        UpdateCalculationState();
    }

    private void CalculateVelocity()
    {
        if (!TryRead(flowInput, out float v) || !TryRead(areaInput, out float f) || f <= 0f)
        {
            return;
        }

        if (!Lab01WorkSession.TrySolveVelocity(v, f, out float velocity, out _))
        {
            return;
        }

        SetInput(velocityInput, velocity, "F3");
        SaveCurrentDraft();
        UpdateCalculationState();
    }

    private void CalculateViscosity()
    {
        if (!TryRead(temperatureInput, out float t))
        {
            return;
        }

        if (!Lab01WorkSession.TrySolveViscosity(t, out float viscosity, out _))
        {
            return;
        }

        SetInput(viscosityInput, viscosity, "E3");
        SaveCurrentDraft();
        UpdateCalculationState();
    }

    private void CalculateReynolds()
    {
        if (!TryRead(velocityInput, out float w) || !TryRead(diameterInput, out float d) || !TryRead(viscosityInput, out float viscosity) || viscosity <= 0f)
        {
            return;
        }

        if (!Lab01WorkSession.TrySolveReynolds(w, d, viscosity, out float reynolds, out _))
        {
            return;
        }

        SetInput(reynoldsInput, reynolds, "F0");
        SaveCurrentDraft();
        UpdateCalculationState();
    }

    private void SubmitToTable()
    {
        if (!TryGetSelectedRecord(out Lab01WorkSession.RecordEntry selectedRecord))
        {
            UpdateCalculationState();
            return;
        }

        if (!TryRead(diameterInput, out float d) ||
            !TryRead(areaInput, out float f) ||
            !TryRead(flowInput, out float v) ||
            !TryRead(velocityInput, out float w) ||
            !TryRead(temperatureInput, out float t) ||
            !TryRead(viscosityInput, out float viscosity) ||
            !TryRead(reynoldsInput, out float reynolds))
        {
            return;
        }

        if (!Lab01WorkSession.TrySubmitRecord(selectedRecord.AttemptNumber, selectedRecord.Measurements, d, f, v, w, t, viscosity, reynolds, out _))
        {
            return;
        }

        SaveCurrentDraft();
        OpenTable();
    }

    private void UpdateCalculationState()
    {
        SetInteractable(submitToTableButton, CanSubmitCurrentInputs());
    }

    private void EnsureRecordDropdownOptions(bool preferLatest)
    {
        if (recordDropdown == null)
        {
            return;
        }

        SaveCurrentDraft();
        recordDropdown.ClearOptions();

        int recordCount = Lab01WorkSession.RecordCount;
        if (recordCount == 0)
        {
            selectedRecordIndex = -1;
            recordDropdown.AddOptions(new System.Collections.Generic.List<string> { emptyRecordsText });
            recordDropdown.interactable = false;
            recordDropdown.SetValueWithoutNotify(0);
            recordDropdown.RefreshShownValue();
            RestoreCurrentDraft();
            UpdateCalculationState();
            return;
        }

        recordDropdown.interactable = true;
        System.Collections.Generic.List<string> options = new System.Collections.Generic.List<string>(recordCount);
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
        UpdateRecordViewText();
        RestoreCurrentDraft();
        UpdateCalculationState();
    }

    private void OnRecordDropdownChanged(int index)
    {
        SaveCurrentDraft();
        selectedRecordIndex = index;
        UpdateRecordViewText();
        RestoreCurrentDraft();
        UpdateCalculationState();
    }

    private void UpdateRecordViewText()
    {
        if (snapshotText == null)
        {
            return;
        }

        if (Lab01WorkSession.RecordCount == 0)
        {
            SetText(snapshotText, string.Empty);
            return;
        }

        selectedRecordIndex = Mathf.Clamp(selectedRecordIndex, 0, Lab01WorkSession.RecordCount - 1);
        Lab01WorkSession.RecordEntry entry = Lab01WorkSession.RecordHistory[selectedRecordIndex];
        SetText(snapshotText,
            $"d = {Lab01WorkSession.Format(entry.Measurements.pipeInnerDiameterMeters, "F3")} м\n" +
            $"V = {Lab01WorkSession.Format(entry.Measurements.volumetricFlowCubicMetersPerSecond, "F6")} м3/с\n" +
            $"t = {Lab01WorkSession.Format(entry.Measurements.temperatureCelsius, "F1")} °C");
    }

    private void OpenTable()
    {
        SaveCurrentDraft();
        gameObject.SetActive(false);
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, false);
        tableRequested?.Invoke();
    }

    public void CloseMenu()
    {
        if (!gameObject.activeSelf)
        {
            return;
        }

        SaveCurrentDraft();
        gameObject.SetActive(false);
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, false);
        closeRequested?.Invoke();
    }

    private void Close()
    {
        CloseMenu();
    }

    private void ClearInputs()
    {
        SetInputText(diameterInput, string.Empty);
        SetInputText(areaInput, string.Empty);
        SetInputText(flowInput, string.Empty);
        SetInputText(velocityInput, string.Empty);
        SetInputText(temperatureInput, string.Empty);
        SetInputText(viscosityInput, string.Empty);
        SetInputText(reynoldsInput, string.Empty);
    }

    private static bool TryRead(TMP_InputField input, out float value)
    {
        value = 0f;
        if (input == null || string.IsNullOrWhiteSpace(input.text))
        {
            return false;
        }

        return float.TryParse(input.text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static void SetInput(TMP_InputField input, float value, string format)
    {
        SetInputText(input, Lab01WorkSession.Format(value, format));
    }

    private static void SetInputText(TMP_InputField input, string value)
    {
        if (input != null)
        {
            input.text = value;
        }
    }

    private static void SetText(TMP_Text label, string value)
    {
        if (label != null)
        {
            label.text = value;
        }
    }

    private static void SetInteractable(Button button, bool interactable)
    {
        if (button != null)
        {
            button.interactable = interactable;
        }
    }

    private void SaveCurrentDraft()
    {
        if (isRestoringDraft || !TryGetSelectedAttemptNumber(out int attemptNumber))
        {
            return;
        }

        draftsByAttempt[attemptNumber] = new CalculationDraft(
            GetInputText(diameterInput),
            GetInputText(areaInput),
            GetInputText(flowInput),
            GetInputText(velocityInput),
            GetInputText(temperatureInput),
            GetInputText(viscosityInput),
            GetInputText(reynoldsInput));
    }

    private void RestoreCurrentDraft()
    {
        isRestoringDraft = true;

        try
        {
            if (!TryGetSelectedAttemptNumber(out int attemptNumber))
            {
                ClearInputs();
                return;
            }

            if (!draftsByAttempt.TryGetValue(attemptNumber, out CalculationDraft draft))
            {
                ClearInputs();
                return;
            }

            SetInputText(diameterInput, draft.Diameter);
            SetInputText(areaInput, draft.Area);
            SetInputText(flowInput, draft.Flow);
            SetInputText(velocityInput, draft.Velocity);
            SetInputText(temperatureInput, draft.Temperature);
            SetInputText(viscosityInput, draft.Viscosity);
            SetInputText(reynoldsInput, draft.Reynolds);
        }
        finally
        {
            isRestoringDraft = false;
        }
    }

    private bool TryGetSelectedAttemptNumber(out int attemptNumber)
    {
        attemptNumber = 0;
        if (!TryGetSelectedRecord(out Lab01WorkSession.RecordEntry selectedRecord))
        {
            return false;
        }

        attemptNumber = selectedRecord.AttemptNumber;
        return true;
    }

    private bool TryGetSelectedRecord(out Lab01WorkSession.RecordEntry selectedRecord)
    {
        selectedRecord = default;
        if (selectedRecordIndex < 0 || selectedRecordIndex >= Lab01WorkSession.RecordCount)
        {
            return false;
        }

        selectedRecord = Lab01WorkSession.RecordHistory[selectedRecordIndex];
        return true;
    }

    private bool CanSubmitCurrentInputs()
    {
        return TryGetSelectedRecord(out _) &&
            TryRead(diameterInput, out _) &&
            TryRead(areaInput, out _) &&
            TryRead(flowInput, out _) &&
            TryRead(velocityInput, out _) &&
            TryRead(temperatureInput, out _) &&
            TryRead(viscosityInput, out _) &&
            TryRead(reynoldsInput, out _);
    }

    private static string GetInputText(TMP_InputField input)
    {
        return input != null ? input.text : string.Empty;
    }

    private readonly struct CalculationDraft
    {
        public CalculationDraft(string diameter, string area, string flow, string velocity, string temperature, string viscosity, string reynolds)
        {
            Diameter = diameter;
            Area = area;
            Flow = flow;
            Velocity = velocity;
            Temperature = temperature;
            Viscosity = viscosity;
            Reynolds = reynolds;
        }

        public readonly string Diameter;
        public readonly string Area;
        public readonly string Flow;
        public readonly string Velocity;
        public readonly string Temperature;
        public readonly string Viscosity;
        public readonly string Reynolds;
    }
}
