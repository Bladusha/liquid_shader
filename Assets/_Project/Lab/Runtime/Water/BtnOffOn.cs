using UnityEngine;

public class BtnOffOn : MonoBehaviour, WorkzoneSelectionController.IInteractable, IHoverable, IInteractionPromptProvider
{
    [Header("Links")]
    [SerializeField] private LabWorkZoneController workZone;
    [SerializeField] private Transform buttonVisual;

    [Header("State")]
    [SerializeField] private bool isOn;

    [Header("Animation")]
    [SerializeField] private Vector3 pressedOffset = new Vector3(0f, -0.05f, 0f);
    [SerializeField] private float animSpeed = 6f;

    [Header("Hover")]
    [SerializeField] private bool useHoverOutline = true;
    [SerializeField] private Transform hoverOutlineTarget;
    [SerializeField] private Color hoverOutlineColor = Color.yellow;
    [SerializeField, Range(0f, 10f)] private float hoverOutlineWidth = 5f;
    [SerializeField] private Color onOutlineColor = Color.green;
    [SerializeField, Range(0f, 10f)] private float onOutlineWidth = 3f;

    [Header("Notifications")]
    [SerializeField] private InteractionPromptPrefabView promptPrefab;
    [SerializeField] private InteractionPromptPrefabView turnOnPromptPrefab;
    [SerializeField] private InteractionPromptPrefabView turnOffPromptPrefab;
    [SerializeField] private InteractionPromptPrefabView inactiveZonePromptPrefab;
    [SerializeField] private Transform promptAnchor;
    [SerializeField] private Vector2 promptScreenOffset = new Vector2(0f, 80f);

    private Vector3 startPos;
    private bool isHovered;
    private Outline runtimeOutline;
    private LabWorkZoneController WorkZone => LabWorkZoneController.Instance != null ? LabWorkZoneController.Instance : workZone;
    public bool IsInteractionActive => isOn;
    public InteractionPromptPrefabView InteractionPromptPrefab
    {
        get
        {
            LabWorkZoneController zone = WorkZone;
            if (zone == null || !zone.IsWorkModeActive)
            {
                return inactiveZonePromptPrefab != null ? inactiveZonePromptPrefab : promptPrefab;
            }

            if (isOn)
            {
                return turnOffPromptPrefab != null ? turnOffPromptPrefab : promptPrefab;
            }

            return turnOnPromptPrefab != null ? turnOnPromptPrefab : promptPrefab;
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

        ApplyState();
    }

    public void SetDefaultPromptPrefabs(
        InteractionPromptPrefabView turnOnPrefab,
        InteractionPromptPrefabView turnOffPrefab,
        InteractionPromptPrefabView inactivePrefab)
    {
        if (turnOnPromptPrefab == null)
        {
            turnOnPromptPrefab = turnOnPrefab;
        }

        if (turnOffPromptPrefab == null)
        {
            turnOffPromptPrefab = turnOffPrefab;
        }

        if (inactiveZonePromptPrefab == null)
        {
            inactiveZonePromptPrefab = inactivePrefab;
        }

        if (promptPrefab == null)
        {
            promptPrefab = turnOnPromptPrefab != null ? turnOnPromptPrefab : turnOffPromptPrefab;
        }
    }

    private void Update()
    {
        LabWorkZoneController zone = WorkZone;
        if (zone != null && isOn != zone.AreControlsEnabled)
        {
            isOn = zone.AreControlsEnabled;
            ApplyHoverOutline();
        }

        if (buttonVisual == null)
        {
            return;
        }

        Vector3 target = isOn ? (startPos + pressedOffset) : startPos;
        buttonVisual.localPosition = Vector3.Lerp(buttonVisual.localPosition, target, Time.deltaTime * animSpeed);
    }

    public void SetHoverState(bool active)
    {
        isHovered = active;
        ApplyHoverOutline();
    }

    public void Interact()
    {
        if (MenuVisibilityCoordinator.AnyMenuOpen)
        {
            return;
        }

        LabWorkZoneController zone = WorkZone;
        if (zone != null && !zone.IsWorkModeActive)
        {
            return;
        }

        Toggle();
    }

    public void Press()
    {
        Interact();
    }

    private void Toggle()
    {
        isOn = !isOn;
        ApplyState();
        SceneActivityLog.Add("Взаимодействие", $"BtnOFF/ON {name}: {(isOn ? "ON" : "OFF")}");
    }

    private void ApplyState()
    {
        LabWorkZoneController zone = WorkZone;
        if (zone != null)
        {
            zone.SetControlsEnabled(isOn);
        }

        ApplyHoverOutline();
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

        bool active = isHovered || isOn;
        outline.OutlineMode = Outline.Mode.OutlineAll;
        outline.OutlineColor = isOn ? onOutlineColor : hoverOutlineColor;
        outline.OutlineWidth = isOn ? onOutlineWidth : hoverOutlineWidth;
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
}
