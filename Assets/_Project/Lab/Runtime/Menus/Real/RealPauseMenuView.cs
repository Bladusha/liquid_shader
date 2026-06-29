using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RealPauseMenuView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text infoLabel;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button cursorTestButton;
    [SerializeField] private Button labTableButton;
    [SerializeField] private Button labCalculationButton;
    [SerializeField] private Button logsButton;
    [SerializeField] private TMP_Text cursorTestLabel;

    private int cursorTestClicks;

    public event Action ContinueRequested;
    public event Action CursorTestRequested;
    public event Action LabTableRequested;
    public event Action LabCalculationRequested;
    public event Action LogsRequested;
    public bool HasLabButtons => labTableButton != null && labCalculationButton != null;
    public bool HasLogsButton => logsButton != null;

    private void Awake()
    {
        WireButtons();
    }

    private void OnEnable()
    {
        UpdateCursorTestLabel();
    }

    public void SetTitle(string value)
    {
        if (titleLabel != null)
        {
            titleLabel.text = value;
        }
    }

    public void SetInfo(string value)
    {
        if (infoLabel != null)
        {
            infoLabel.text = value;
        }
    }

    public void IncrementCursorTest()
    {
        cursorTestClicks++;
        UpdateCursorTestLabel();
    }

    public void SetCursorTestCount(int value)
    {
        cursorTestClicks = Mathf.Max(0, value);
        UpdateCursorTestLabel();
    }

    public void Configure(
        TMP_Text title,
        TMP_Text info,
        Button continueBtn,
        Button cursorBtn,
        Button tableBtn,
        Button calculationBtn,
        Button logsBtn,
        TMP_Text cursorLabel)
    {
        titleLabel = title;
        infoLabel = info;
        continueButton = continueBtn;
        cursorTestButton = cursorBtn;
        labTableButton = tableBtn;
        labCalculationButton = calculationBtn;
        logsButton = logsBtn;
        cursorTestLabel = cursorLabel;
        RefreshBindings();
    }

    public void Configure(
        TMP_Text title,
        TMP_Text info,
        Button continueBtn,
        Button cursorBtn,
        Button tableBtn,
        Button calculationBtn,
        TMP_Text cursorLabel)
    {
        Configure(title, info, continueBtn, cursorBtn, tableBtn, calculationBtn, null, cursorLabel);
    }

    public void Configure(
        TMP_Text title,
        TMP_Text info,
        Button continueBtn,
        Button cursorBtn,
        TMP_Text cursorLabel)
    {
        Configure(title, info, continueBtn, cursorBtn, null, null, cursorLabel);
    }

    public void SetContinueHandler(Action handler)
    {
        ContinueRequested = handler;
    }

    public void SetCursorTestHandler(Action handler)
    {
        CursorTestRequested = handler;
    }

    public void SetLabButtons(Button tableButton, Button calculationButton)
    {
        labTableButton = tableButton;
        labCalculationButton = calculationButton;
        RefreshBindings();
    }

    public void SetLogsButton(Button button)
    {
        logsButton = button;
        RefreshBindings();
    }

    public void SetLabTableHandler(Action handler)
    {
        LabTableRequested = handler;
    }

    public void SetLabCalculationHandler(Action handler)
    {
        LabCalculationRequested = handler;
    }

    public void SetLogsHandler(Action handler)
    {
        LogsRequested = handler;
    }

    public void RefreshBindings()
    {
        WireButtons();
        UpdateCursorTestLabel();
    }

    private void WireButtons()
    {
        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(HandleContinueClicked);
            continueButton.onClick.AddListener(HandleContinueClicked);
            Lab.UI.HydrodynamicWaterButtonAnimator.EnsureOn(continueButton);
        }

        if (cursorTestButton != null)
        {
            cursorTestButton.onClick.RemoveListener(HandleCursorTestClicked);
            cursorTestButton.onClick.AddListener(HandleCursorTestClicked);
            Lab.UI.HydrodynamicWaterButtonAnimator.EnsureOn(cursorTestButton);
        }

        if (labTableButton != null)
        {
            labTableButton.onClick.RemoveListener(HandleLabTableClicked);
            labTableButton.onClick.AddListener(HandleLabTableClicked);
            Lab.UI.HydrodynamicWaterButtonAnimator.EnsureOn(labTableButton);
        }

        if (labCalculationButton != null)
        {
            labCalculationButton.onClick.RemoveListener(HandleLabCalculationClicked);
            labCalculationButton.onClick.AddListener(HandleLabCalculationClicked);
            Lab.UI.HydrodynamicWaterButtonAnimator.EnsureOn(labCalculationButton);
        }

        if (logsButton != null)
        {
            logsButton.onClick.RemoveListener(HandleLogsClicked);
            logsButton.onClick.AddListener(HandleLogsClicked);
            Lab.UI.HydrodynamicWaterButtonAnimator.EnsureOn(logsButton);
        }
    }

    private void HandleContinueClicked()
    {
        ContinueRequested?.Invoke();
    }

    private void HandleCursorTestClicked()
    {
        IncrementCursorTest();
        CursorTestRequested?.Invoke();
    }

    private void HandleLabTableClicked()
    {
        LabTableRequested?.Invoke();
    }

    private void HandleLabCalculationClicked()
    {
        LabCalculationRequested?.Invoke();
    }

    private void HandleLogsClicked()
    {
        LogsRequested?.Invoke();
    }

    private void UpdateCursorTestLabel()
    {
        if (cursorTestLabel != null)
        {
            cursorTestLabel.text = $"Clicks: {cursorTestClicks}";
        }
    }
}
