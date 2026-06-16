using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Lab01TableMenuController : MonoBehaviour
{
    private const string MenuId = "lab_table";

    [SerializeField] private TMP_Text liveDataText;
    [SerializeField] private TMP_Text tableRowText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button calculationButton;
    [SerializeField] private Button closeButton;

    private WaterController waterController;
    private Action closeRequested;
    private Action calculationRequested;

    private void Awake()
    {
        DisableLegacyBlocks();
        ConfigureTableText();
    }

    public void Configure(
        TMP_Text liveData,
        TMP_Text tableRow,
        TMP_Text status,
        Button calculationBtn,
        Button close)
    {
        liveDataText = liveData;
        tableRowText = tableRow;
        statusText = status;
        calculationButton = calculationBtn;
        closeButton = close;
        DisableLegacyBlocks();
        ConfigureTableText();
        RefreshBindings();
    }

    private void OnEnable()
    {
        MenuVisibilityCoordinator.MenuOpening += HandleOtherMenuOpening;
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

    public void Open(WaterController controller, Action closeHandler, Action calculationHandler)
    {
        waterController = controller;
        closeRequested = closeHandler;
        calculationRequested = calculationHandler;
        gameObject.SetActive(true);
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, true);
        DisableLegacyBlocks();
        ConfigureTableText();
        RefreshView();
    }

    public void RefreshBindings()
    {
        Bind(calculationButton, OpenCalculations);
        Bind(closeButton, Close);
    }

    private void Update()
    {
        if (gameObject.activeInHierarchy)
        {
            RefreshView();

            if (InputSystemCompat.GetKeyDown(KeyCode.Tab) || InputSystemCompat.GetKeyDown(KeyCode.Escape))
            {
                if (InputSystemCompat.GetKeyDown(KeyCode.Tab))
                {
                    MenuVisibilityCoordinator.MarkTabHandled();
                }

                CloseMenu();
            }
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

    private void RefreshView()
    {
        if (waterController == null)
        {
            waterController = FindAnyObjectByType<WaterController>();
        }

        SetText(tableRowText, Lab01WorkSession.BuildSubmittedTableText());

        if (calculationButton != null)
        {
            calculationButton.interactable = Lab01WorkSession.HasValidSnapshot;
        }
    }

    private void Close()
    {
        CloseMenu();
    }

    private static void SetText(TMP_Text label, string value)
    {
        if (label != null)
        {
            label.text = value;
        }
    }

    private void DisableLegacyBlocks()
    {
        SetInactiveIfFound("LiveDataBox");
        SetInactiveIfFound("StatusText");

        if (liveDataText != null)
        {
            liveDataText.gameObject.SetActive(false);
        }

        if (statusText != null)
        {
            statusText.gameObject.SetActive(false);
        }
    }

    private void ConfigureTableText()
    {
        if (tableRowText == null)
        {
            return;
        }

        tableRowText.fontSize = 16f;
        tableRowText.alignment = TextAlignmentOptions.TopLeft;
        tableRowText.textWrappingMode = TextWrappingModes.Normal;
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
}
