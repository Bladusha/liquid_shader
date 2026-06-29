using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PositionSliderMenuController : MonoBehaviour
{
    private const string MenuId = "position_slider";

    [Header("UI References")]
    public Slider positionSlider;
    public TMP_Text valueText;
    public TMP_Text titleText;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Slider Settings")]
    public float minValue;
    public float maxValue = 100f;
    public float initialValue = 50f;
    public bool wholeNumbers;

    [Header("Display Settings")]
    public string valuePrefix = "Value: ";
    public string valueSuffix = string.Empty;
    public int decimalPlaces = 1;

    [Header("Cursor Settings")]
    public bool unlockCursor = true;
    public bool restoreCursorOnClose = true;
    public CursorLockMode cursorLockModeOnClose = CursorLockMode.Locked;

    public event Action<float> OnPositionChanged;
    public event Action<float> OnPositionConfirmed;
    public event Action OnMenuCanceled;

    private readonly List<MonoBehaviour> disabledScriptCache = new();
    private MonoBehaviour[] disabledScripts;
    private float currentValue;
    private float initialSliderValue;
    private bool isClosing;
    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisibility;

    public float CurrentValue => currentValue;
    public float MinValue => minValue;
    public float MaxValue => maxValue;
    public bool IsWholeNumbers => wholeNumbers;
    public string CurrentDisplayText => valueText != null ? valueText.text : string.Empty;

    private void Awake()
    {
        previousCursorLockState = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;
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

    private void Start()
    {
        InitializeUI();
        SetupEventListeners();
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, true);
        ManageCursor(true);
        DisablePlayerControls();
    }

    private void Update()
    {
        if (InputSystemCompat.GetKeyDown(KeyCode.Escape) || InputSystemCompat.GetKeyDown(KeyCode.Tab))
        {
            if (InputSystemCompat.GetKeyDown(KeyCode.Tab))
            {
                MenuVisibilityCoordinator.MarkTabHandled();
            }

            CancelPosition();
        }

        if (InputSystemCompat.GetKeyDown(KeyCode.Return) || InputSystemCompat.GetKeyDown(KeyCode.KeypadEnter))
        {
            ConfirmPosition();
        }
    }

    private void OnDestroy()
    {
        if (!isClosing)
        {
            ManageCursor(false);
            EnablePlayerControls();
        }

        MenuVisibilityCoordinator.SetMenuOpen(MenuId, false);

        if (positionSlider != null)
        {
            positionSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.RemoveListener(ConfirmPosition);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveListener(CancelPosition);
        }
    }

    public void ConfirmPosition()
    {
        OnPositionConfirmed?.Invoke(currentValue);
        CloseMenuInternal();
    }

    public void CancelPosition()
    {
        if (positionSlider != null)
        {
            positionSlider.value = initialSliderValue;
        }

        OnMenuCanceled?.Invoke();
        CloseMenuInternal();
    }

    public void CloseMenu()
    {
        CancelPosition();
    }

    public void SetValue(float value)
    {
        float clampedValue = Mathf.Clamp(value, minValue, maxValue);
        currentValue = clampedValue;

        if (positionSlider != null)
        {
            positionSlider.value = clampedValue;
        }
        else
        {
            UpdateValueText();
        }
    }

    public void SetRange(float min, float max)
    {
        minValue = min;
        maxValue = max;

        if (positionSlider != null)
        {
            positionSlider.minValue = minValue;
            positionSlider.maxValue = maxValue;
            positionSlider.value = Mathf.Clamp(positionSlider.value, minValue, maxValue);
        }

        currentValue = Mathf.Clamp(currentValue, minValue, maxValue);
        UpdateValueText();
    }

    public void SetWholeNumbers(bool useWholeNumbers)
    {
        wholeNumbers = useWholeNumbers;
        if (positionSlider != null)
        {
            positionSlider.wholeNumbers = useWholeNumbers;
        }

        UpdateValueText();
    }

    public float GetNormalizedValue()
    {
        return maxValue == minValue ? 0f : Mathf.InverseLerp(minValue, maxValue, currentValue);
    }

    public float GetPercentageValue()
    {
        return GetNormalizedValue() * 100f;
    }

    public void SetTextPrefix(string prefix)
    {
        valuePrefix = prefix;
        UpdateValueText();
    }

    public void SetTextSuffix(string suffix)
    {
        valueSuffix = suffix;
        UpdateValueText();
    }

    public void SetDecimalPlaces(int places)
    {
        decimalPlaces = Mathf.Clamp(places, 0, 5);
        UpdateValueText();
    }

    public void SetTitle(string title)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
    }

    public void ResetToInitial()
    {
        SetValue(initialValue);
    }

    public void SetSliderInteractable(bool interactable)
    {
        if (positionSlider != null)
        {
            positionSlider.interactable = interactable;
        }
    }

    public void SetConfirmButtonInteractable(bool interactable)
    {
        if (confirmButton != null)
        {
            confirmButton.interactable = interactable;
        }
    }

    public void SetCancelButtonInteractable(bool interactable)
    {
        if (cancelButton != null)
        {
            cancelButton.interactable = interactable;
        }
    }

    public static PositionSliderMenuController CreateMenu(GameObject prefab, Transform parent = null)
    {
        if (prefab == null)
        {
            Debug.LogError("Prefab is not assigned.");
            return null;
        }

        GameObject menuInstance = Instantiate(prefab, parent);
        menuInstance.name = "PositionSliderMenu";

        RectTransform rectTransform = menuInstance.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
        }

        return menuInstance.GetComponent<PositionSliderMenuController>();
    }

    public static PositionSliderMenuController CreateMenuWithSettings(
        GameObject prefab,
        float min,
        float max,
        float initial,
        string title = "Adjust Value",
        Transform parent = null)
    {
        PositionSliderMenuController menu = CreateMenu(prefab, parent);
        if (menu != null)
        {
            menu.minValue = min;
            menu.maxValue = max;
            menu.initialValue = initial;
            menu.SetTitle(title);
            menu.SetValue(initial);
        }

        return menu;
    }

    private void InitializeUI()
    {
        currentValue = initialValue;
        initialSliderValue = initialValue;

        if (positionSlider != null)
        {
            positionSlider.minValue = minValue;
            positionSlider.maxValue = maxValue;
            positionSlider.value = initialValue;
            positionSlider.wholeNumbers = wholeNumbers;
        }

        if (titleText != null && string.IsNullOrEmpty(titleText.text))
        {
            titleText.text = "Adjust Value";
        }

        UpdateValueText();
    }

    private void SetupEventListeners()
    {
        if (positionSlider != null)
        {
            positionSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(ConfirmPosition);
        }

        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(CancelPosition);
        }
    }

    private void OnSliderValueChanged(float newValue)
    {
        currentValue = newValue;
        UpdateValueText();
        OnPositionChanged?.Invoke(currentValue);
    }

    private void UpdateValueText()
    {
        if (valueText == null)
        {
            return;
        }

        string format = wholeNumbers ? "F0" : "F" + decimalPlaces;
        valueText.text = $"{valuePrefix}{currentValue.ToString(format)}{valueSuffix}";
        valueText.fontSize = 16;
        valueText.alignment = TextAlignmentOptions.Center;
    }

    private void CloseMenuInternal()
    {
        if (isClosing)
        {
            return;
        }

        isClosing = true;
        ManageCursor(false);
        EnablePlayerControls();
        Destroy(gameObject);
    }

    private void ManageCursor(bool openMenu)
    {
        if (openMenu && unlockCursor)
        {
            CursorStateUtility.Apply(CursorLockMode.None, false);
        }
        else if (restoreCursorOnClose)
        {
            CursorStateUtility.Apply(cursorLockModeOnClose, false);
        }
        else
        {
            CursorStateUtility.Apply(previousCursorLockState, previousCursorVisibility);
        }
    }

    private void DisablePlayerControls()
    {
        disabledScriptCache.Clear();

        foreach (MonoBehaviour script in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (script == this || script == null || !script.enabled)
            {
                continue;
            }

            string scriptName = script.GetType().Name;
            bool isPlayerControl = scriptName.Contains("PlayerController") ||
                                   scriptName.Contains("FirstPerson") ||
                                   scriptName.Contains("MouseLook") ||
                                   scriptName.Contains("CameraController") ||
                                   scriptName.Contains("CharacterController") ||
                                   scriptName.Contains("CameraMovement");

            if (isPlayerControl)
            {
                script.enabled = false;
                disabledScriptCache.Add(script);
            }
        }

        disabledScripts = disabledScriptCache.ToArray();
    }

    private void EnablePlayerControls()
    {
        if (disabledScripts == null)
        {
            return;
        }

        foreach (MonoBehaviour script in disabledScripts)
        {
            if (script != null)
            {
                script.enabled = true;
            }
        }

        disabledScripts = null;
        disabledScriptCache.Clear();
    }
}
