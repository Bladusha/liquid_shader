using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DefaultLabStandPanelView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text bodyLabel;
    [SerializeField] private TMP_Text footerLabel;
    [SerializeField] private GameObject recordNotificationRoot;
    [SerializeField] private TMP_Text recordNotificationLabel;
    [SerializeField] private Button recordButton;
    [SerializeField] private Button calculationButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private string recordSuccessMessageTemplate = "Запись номер {0} сохранена!";
    [SerializeField, Min(0f)] private float recordSuccessMessageDuration = 2.5f;

    public event Action CloseRequested;
    public event Action RecordRequested;
    public event Action CalculationRequested;

    private void Awake()
    {
        RefreshBindings();
    }

    public void Configure(
        TMP_Text title,
        TMP_Text body,
        TMP_Text footer,
        GameObject notificationRoot,
        TMP_Text notificationLabel,
        Button record,
        Button calculation,
        Button close)
    {
        titleLabel = title;
        bodyLabel = body;
        footerLabel = footer;
        recordNotificationRoot = notificationRoot;
        recordNotificationLabel = notificationLabel;
        recordButton = record;
        calculationButton = calculation;
        closeButton = close;
        RefreshBindings();
    }

    public void Configure(TMP_Text title, TMP_Text body, TMP_Text footer, Button record, Button close)
    {
        Configure(title, body, footer, null, null, record, null, close);
    }

    public void Configure(
        TMP_Text title,
        TMP_Text body,
        TMP_Text footer,
        Button record,
        Button calculation,
        Button close)
    {
        Configure(title, body, footer, null, null, record, calculation, close);
    }

    public void SetTitle(string value)
    {
        if (titleLabel != null)
        {
            titleLabel.text = value;
        }
    }

    public void SetBody(string value)
    {
        if (bodyLabel != null)
        {
            bodyLabel.text = value;
        }
    }

    public void SetFooter(string value)
    {
        if (footerLabel != null)
        {
            footerLabel.text = value;
        }
    }

    public void SetCloseHandler(Action handler)
    {
        CloseRequested = handler;
    }

    public void SetRecordHandler(Action handler)
    {
        RecordRequested = handler;
    }

    public void SetCalculationHandler(Action handler)
    {
        CalculationRequested = handler;
    }

    public void SetButtonLabels(string recordLabel, string calculationLabel, string closeLabel)
    {
        SetButtonLabel(recordButton, recordLabel);
        SetButtonLabel(calculationButton, calculationLabel);
        SetButtonLabel(closeButton, closeLabel);
    }

    public void SetButtonLabels(string recordLabel, string closeLabel)
    {
        SetButtonLabels(recordLabel, null, closeLabel);
    }

    public string GetRecordSuccessMessageTemplate()
    {
        return recordSuccessMessageTemplate;
    }

    public float GetRecordSuccessMessageDuration()
    {
        return recordSuccessMessageDuration;
    }

    public void ShowRecordNotification(string message)
    {
        if (recordNotificationLabel != null)
        {
            recordNotificationLabel.text = message;
        }

        if (recordNotificationRoot != null)
        {
            recordNotificationRoot.SetActive(true);
        }
    }

    public void HideRecordNotification()
    {
        if (recordNotificationRoot != null)
        {
            recordNotificationRoot.SetActive(false);
        }
    }

    public void RefreshBindings()
    {
        BindRecordButton();
        BindCalculationButton();

        if (closeButton == null)
        {
            return;
        }

        closeButton.onClick.RemoveListener(HandleCloseClicked);
        closeButton.onClick.AddListener(HandleCloseClicked);
        Lab.UI.HydrodynamicWaterButtonAnimator.EnsureOn(closeButton);
    }

    private void BindRecordButton()
    {
        if (recordButton == null)
        {
            return;
        }

        recordButton.onClick.RemoveListener(HandleRecordClicked);
        recordButton.onClick.AddListener(HandleRecordClicked);
        Lab.UI.HydrodynamicWaterButtonAnimator.EnsureOn(recordButton);
    }

    private void BindCalculationButton()
    {
        if (calculationButton == null)
        {
            return;
        }

        calculationButton.onClick.RemoveListener(HandleCalculationClicked);
        calculationButton.onClick.AddListener(HandleCalculationClicked);
        Lab.UI.HydrodynamicWaterButtonAnimator.EnsureOn(calculationButton);
    }

    private static void SetButtonLabel(Button button, string value)
    {
        if (button == null)
        {
            return;
        }

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = value;
        }
    }

    private void HandleCloseClicked()
    {
        CloseRequested?.Invoke();
    }

    private void HandleRecordClicked()
    {
        RecordRequested?.Invoke();
    }

    private void HandleCalculationClicked()
    {
        CalculationRequested?.Invoke();
    }
}
