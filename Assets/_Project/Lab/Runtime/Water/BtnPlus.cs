using UnityEngine;

public class BtnPlus : MonoBehaviour, WorkzoneSelectionController.IInteractable, IHoverable, IInteractionPromptProvider
{
    [Header("Связи")]
    public WaterController controller;
    public Transform buttonVisual;
    public Transform knobTransform;

    [Header("Добавка к текущему режиму")]
    [Range(-1f, 1f)] public float fillLevelDelta = 0f;
    [Range(-5f, 5f)] public float flowSpeedDelta = 0f;
    [Range(-1f, 1f)] public float temperatureDelta = 0f;
    [Range(-1f, 1f)] public float surfaceOscillationDelta = 0f;
    [Range(-8f, 8f)] public float bubbleSizeDelta = 0f;
    [Range(-1f, 1f)] public float bubbleAmountDelta = 0f;
    [Range(-1f, 1f)] public float frontTiltDelta = 0f;
    [Range(-0.2f, 0.2f)] public float frontFadeDelta = 0f;
    [Range(-1f, 1f)] public float alphaDelta = 0f;

    [Header("Режим настройки")]
    [SerializeField] private float rotationSensitivity = 0.35f;
    [Range(0f, 1f)] public float currentOpenAmount = 0f;
    public Axis rotationAxis = Axis.Y;
    public float minAngle = -90f;
    public float maxAngle = 90f;

    [Header("Анимация кнопки")]
    public Vector3 pressedOffset = new Vector3(0, -0.05f, 0);
    public float animSpeed = 6f;

    [Header("Hover")]
    [SerializeField] private bool useHoverOutline = true;
    [SerializeField] private Transform hoverOutlineTarget;
    [SerializeField] private Color hoverOutlineColor = Color.yellow;
    [SerializeField, Range(0f, 10f)] private float hoverOutlineWidth = 5f;
    [SerializeField] private Color activeOutlineColor = Color.green;
    [SerializeField, Range(0f, 10f)] private float activeOutlineWidth = 3f;

    [Header("Notifications")]
    [SerializeField] private InteractionPromptPrefabView promptPrefab;
    [SerializeField] private InteractionPromptPrefabView disabledPromptPrefab;
    [SerializeField] private InteractionPromptPrefabView hoverPromptPrefab;
    [SerializeField] private InteractionPromptPrefabView activePromptPrefab;
    [SerializeField] private Transform promptAnchor;
    [SerializeField] private Vector2 promptScreenOffset = new Vector2(0f, 80f);

    private Vector3 startPos;
    private bool isActivated;
    private bool isEditing;
    private bool isHovered;
    private Outline runtimeOutline;

    public enum Axis
    {
        X,
        Y,
        Z
    }

    public bool IsEditing => isEditing;
    public static bool AnyEditing => activeEditingButton != null;
    public static BtnPlus ActiveEditingButton => activeEditingButton;
    public bool IsInteractionActive => isEditing;
    public InteractionPromptPrefabView InteractionPromptPrefab
    {
        get
        {
            LabWorkZoneController workZone = LabWorkZoneController.Instance;
            if (workZone != null && !workZone.AreControlsEnabled)
            {
                return disabledPromptPrefab != null ? disabledPromptPrefab : promptPrefab;
            }

            if (isEditing)
            {
                return activePromptPrefab != null ? activePromptPrefab : promptPrefab;
            }

            return hoverPromptPrefab != null ? hoverPromptPrefab : promptPrefab;
        }
    }
    public Transform InteractionPromptAnchor => promptAnchor != null ? promptAnchor : transform;
    public Vector2 InteractionPromptScreenOffset => promptScreenOffset;
    private void Start()
    {
        if (buttonVisual != null)
        {
            startPos = buttonVisual.localPosition;
        }

        currentOpenAmount = Mathf.Clamp01(currentOpenAmount);
        ApplyKnobRotation();

        if (controller != null)
        {
            controller.SetPlusAmount(this, currentOpenAmount);
        }

        RefreshPressedState();
    }

    public void SetDefaultPromptPrefabs(
        InteractionPromptPrefabView disabledPrefab,
        InteractionPromptPrefabView hoverPrefab,
        InteractionPromptPrefabView activePrefab)
    {
        if (disabledPromptPrefab == null)
        {
            disabledPromptPrefab = disabledPrefab;
        }

        if (hoverPromptPrefab == null)
        {
            hoverPromptPrefab = hoverPrefab;
        }

        if (activePromptPrefab == null)
        {
            activePromptPrefab = activePrefab;
        }

        if (promptPrefab == null)
        {
            promptPrefab = hoverPromptPrefab != null ? hoverPromptPrefab : activePromptPrefab;
        }
    }

    private void Update()
    {
        if (buttonVisual != null)
        {
            Vector3 target = isActivated ? (startPos + pressedOffset) : startPos;
            buttonVisual.localPosition = Vector3.Lerp(buttonVisual.localPosition, target, Time.deltaTime * animSpeed);
        }

        if (!isEditing)
        {
            return;
        }

        if (!CanUseInCurrentWorkZone())
        {
            ExitEditMode();
            return;
        }

        if (!InputSystemCompat.GetMouseButton(0))
        {
            ExitEditMode();
            return;
        }

        HandleEditingInput();
    }

    public void Interact()
    {
        if (MenuVisibilityCoordinator.AnyMenuOpen)
        {
            return;
        }

        if (!CanUseInCurrentWorkZone())
        {
            return;
        }

        if (isEditing)
        {
            ExitEditMode();
            return;
        }

        EnterEditMode();
    }

    public void Press()
    {
        Interact();
    }

    public void SetPressedVisual(bool pressed)
    {
        isActivated = pressed;
        ApplyHoverOutline();
    }

    public void SetHoverState(bool active)
    {
        isHovered = active;
        ApplyHoverOutline();
    }

    private void EnterEditMode()
    {
        if (controller == null || !CanUseInCurrentWorkZone())
        {
            return;
        }

        if (activeEditingButton != null && activeEditingButton != this)
        {
            activeEditingButton.ExitEditMode();
        }

        activeEditingButton = this;
        isEditing = true;

        RefreshPressedState();
    }

    private bool CanUseInCurrentWorkZone()
    {
        LabWorkZoneController workZone = LabWorkZoneController.Instance;
        return workZone == null || (workZone.IsWorkModeActive && workZone.AreControlsEnabled);
    }

    private void ExitEditMode()
    {
        if (!isEditing)
        {
            return;
        }

        isEditing = false;
        SceneActivityLog.Add("Взаимодействие", $"BtnPlus {name}: {Mathf.RoundToInt(currentOpenAmount * 100f)}%");
        if (activeEditingButton == this)
        {
            activeEditingButton = null;
        }

        RefreshPressedState();
    }

    private void ApplyHoverOutline()
    {
        if (!useHoverOutline)
        {
            return;
        }

        Outline outline = GetOrCreateOutline();
        if (outline == null)
        {
            return;
        }

        bool active = isHovered || isEditing || currentOpenAmount > 0.001f;
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = isEditing ? activeOutlineColor : hoverOutlineColor;
        outline.OutlineWidth = isEditing ? activeOutlineWidth : hoverOutlineWidth;
        outline.enabled = active;
    }

    private Outline GetOrCreateOutline()
    {
        Transform target = hoverOutlineTarget != null ? hoverOutlineTarget : transform;
        if (target == null)
        {
            return null;
        }

        if (runtimeOutline == null)
        {
            runtimeOutline = target.GetComponent<Outline>();
            if (runtimeOutline == null)
            {
                runtimeOutline = target.gameObject.AddComponent<Outline>();
            }
        }

        return runtimeOutline;
    }

    private void HandleEditingInput()
    {
        if (!InputSystemCompat.GetMouseButton(0))
        {
            return;
        }

        float mouseX = InputSystemCompat.GetAxis("Mouse X");
        if (Mathf.Abs(mouseX) <= 0.0001f)
        {
            return;
        }

        currentOpenAmount = Mathf.Clamp01(currentOpenAmount + mouseX * rotationSensitivity * Time.deltaTime);
        ApplyKnobRotation();

        if (controller != null)
        {
            controller.SetPlusAmount(this, currentOpenAmount);
        }

        RefreshPressedState();
    }

    private void ApplyKnobRotation()
    {
        if (knobTransform == null)
        {
            return;
        }

        float angle = Mathf.Lerp(minAngle, maxAngle, currentOpenAmount);
        Vector3 currentEuler = knobTransform.localEulerAngles;

        switch (rotationAxis)
        {
            case Axis.X:
                knobTransform.localRotation = Quaternion.Euler(angle, currentEuler.y, currentEuler.z);
                break;
            case Axis.Y:
                knobTransform.localRotation = Quaternion.Euler(currentEuler.x, angle, currentEuler.z);
                break;
            case Axis.Z:
                knobTransform.localRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, angle);
                break;
        }
    }

    private void RefreshPressedState()
    {
        SetPressedVisual(isEditing || currentOpenAmount > 0.001f);
    }

    private static BtnPlus activeEditingButton;

    public static void ForceCloseActiveEditing()
    {
        if (activeEditingButton != null)
        {
            activeEditingButton.ExitEditMode();
        }
    }
}
