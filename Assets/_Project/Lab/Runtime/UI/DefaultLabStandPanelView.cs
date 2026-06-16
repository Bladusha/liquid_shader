using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DefaultLabStandPanelView : MonoBehaviour
{
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text bodyLabel;
    [SerializeField] private TMP_Text footerLabel;
    [SerializeField] private Button recordButton;
    [SerializeField] private Button closeButton;

    public event Action CloseRequested;
    public event Action RecordRequested;

    private void Awake()
    {
        RefreshBindings();
    }

    public void Configure(TMP_Text title, TMP_Text body, TMP_Text footer, Button record, Button close)
    {
        titleLabel = title;
        bodyLabel = body;
        footerLabel = footer;
        recordButton = record;
        closeButton = close;
        RefreshBindings();
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

    public void SetButtonLabels(string recordLabel, string closeLabel)
    {
        SetButtonLabel(recordButton, recordLabel);
        SetButtonLabel(closeButton, closeLabel);
    }

    public void RefreshBindings()
    {
        if (closeButton == null)
        {
            BindRecordButton();
            return;
        }

        closeButton.onClick.RemoveListener(HandleCloseClicked);
        closeButton.onClick.AddListener(HandleCloseClicked);
        BindRecordButton();
    }

    private void BindRecordButton()
    {
        if (recordButton == null)
        {
            return;
        }

        recordButton.onClick.RemoveListener(HandleRecordClicked);
        recordButton.onClick.AddListener(HandleRecordClicked);
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
}
