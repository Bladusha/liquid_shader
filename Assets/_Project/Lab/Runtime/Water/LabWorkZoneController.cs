using System.Collections;
using EasyPeasyFirstPersonController;
using UnityEngine;
using UnityEngine.UI;

public class LabWorkZoneController : MonoBehaviour
{
    public static LabWorkZoneController Instance { get; private set; }

    [Header("Zone")]
    [SerializeField] private Collider zoneTrigger;
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private bool usePositionFallback = true;
    [SerializeField] private float fallbackActivationRadius = 2.5f;
    [SerializeField] private float boundsCheckPadding = 0.15f;

    [Header("Keys")]
    [SerializeField] private KeyCode toggleKey = KeyCode.E;
    [SerializeField] private KeyCode exitKey = KeyCode.Escape;

    [Header("Camera View")]
    [SerializeField] private Transform viewingPoint;
    [SerializeField] private float transitionDuration = 0.35f;
    [SerializeField] private float viewingFov = 45f;

    [Header("Zoom")]
    [SerializeField] private float zoomMinFov = 25f;
    [SerializeField] private float zoomMaxFov = 60f;
    [SerializeField] private float zoomSensitivity = 25f;

    [Header("Mouse Interaction")]
    [SerializeField] private LayerMask raycastLayers = ~0;
    [SerializeField] private float maxDistance = 4f;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private bool ignoreZoneColliderInRaycast = true;
    [SerializeField] private bool useRendererFallbackSelection = true;

    [Header("Notifications")]
    [SerializeField] private CrosshairPromptUI crosshairUI;
    [SerializeField] private Canvas promptPrefabCanvas;

    [Header("Zone Entry Prompt")]
    [SerializeField] private InteractionPromptPrefabView zoneEntryPromptPrefab;
    [SerializeField] private string zoneEntryPromptResourcePath = "InteractionPrompts/Prefabs/Tooltip_Workzone_Enter";
    [SerializeField] private Vector2 zoneEntryPromptViewportPosition = new Vector2(0.5f, 0.18f);
    [SerializeField] private Vector2 zoneEntryPromptScreenOffset = Vector2.zero;
    [SerializeField] private Vector2 defaultZoneEntryPromptSize = new Vector2(760f, 96f);

    [Header("Control State")]
    [SerializeField] private bool controlsEnabled;

    private bool isPlayerInZone;
    private bool isEditing;
    private bool wasControllerEnabled;
    private Transform originalCameraParent;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private float originalCameraFov;
    private Camera playerCamera;
    private float originalGravity = 9.81f;
    private CharacterController characterController;
    private Coroutine cameraMoveRoutine;
    private Transform currentSelectedObject;
    private IHoverable currentHoverable;
    private IInteractionPromptProvider currentPromptProvider;
    private Outline currentOutlineComponent;
    private InteractionPromptPrefabView currentPromptPrefabInstance;
    private InteractionPromptPrefabView currentPromptPrefabSource;
    private InteractionPromptPrefabView zoneEntryPromptInstance;
    private InteractionPromptPrefabView zoneEntryPromptSource;
    private int triggerContactCount;
    private bool warnedMissingViewingPoint;
    private bool warnedMissingPlayerReferences;

    public bool IsPlayerInZone => isPlayerInZone;
    public bool IsWorkModeActive => isEditing;
    public bool AreControlsEnabled => controlsEnabled;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (zoneTrigger == null)
        {
            zoneTrigger = GetComponent<Collider>();
        }

        if (zoneTrigger != null)
        {
            zoneTrigger.isTrigger = true;
        }
    }

    private void Start()
    {
        ResolvePlayerReferences();
        ResolveCrosshairUI();
        ResolveZoneEntryPromptPrefab();
    }

    private void OnEnable()
    {
        MenuVisibilityCoordinator.AnyMenuStateChanged += HandleAnyMenuStateChanged;
    }

    private void Update()
    {
        RefreshZonePresenceFromPlayerPosition();

        if (!isPlayerInZone)
        {
            return;
        }

        if (!isEditing)
        {
            if (MenuVisibilityCoordinator.AnyMenuOpen)
            {
                HideZoneEntryPrompt();
                return;
            }

            ShowZoneEntryPrompt();

            if (InputSystemCompat.GetKeyDown(toggleKey))
            {
                EnterWorkMode();
            }

            return;
        }

        if (InputSystemCompat.GetKeyDown(exitKey) || InputSystemCompat.GetKeyDown(toggleKey))
        {
            ExitWorkMode();
            return;
        }

        if (MenuVisibilityCoordinator.AnyMenuOpen)
        {
            ClearSelection();
            return;
        }

        CheckForSelection();

        if (InputSystemCompat.GetMouseButtonDown(0) && currentSelectedObject != null)
        {
            TryInteractWithSelectedObject();
        }

        HandleZoomInput();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsPlayerCollider(other))
        {
            return;
        }

        triggerContactCount++;
        EnterPlayerZone();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsPlayerCollider(other))
        {
            return;
        }

        triggerContactCount = Mathf.Max(0, triggerContactCount - 1);
        if (triggerContactCount == 0)
        {
            LeavePlayerZone();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsPlayerCollider(other))
        {
            return;
        }

        triggerContactCount = Mathf.Max(1, triggerContactCount);
        EnterPlayerZone();
    }

    private void EnterPlayerZone()
    {
        if (isPlayerInZone)
        {
            return;
        }

        isPlayerInZone = true;

        if (!isEditing)
        {
            ShowZoneEntryPrompt();
        }
    }

    private void LeavePlayerZone()
    {
        if (!isPlayerInZone)
        {
            return;
        }

        isPlayerInZone = false;
        HideZoneEntryPrompt();

        if (isEditing)
        {
            ExitWorkMode();
        }
    }

    private void OnDisable()
    {
        MenuVisibilityCoordinator.AnyMenuStateChanged -= HandleAnyMenuStateChanged;
        triggerContactCount = 0;
        isPlayerInZone = false;
        HideZoneEntryPrompt();
        ClearInteractionPrompt();
    }

    private void HandleAnyMenuStateChanged(bool anyMenuOpen)
    {
        if (anyMenuOpen || !isEditing)
        {
            return;
        }

        CursorStateUtility.Apply(CursorLockMode.None, false);
        LabSceneCrosshairBootstrap.OnInteractionStarted();
        PrepareInteractionPromptUi();
    }

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;

        if (!controlsEnabled)
        {
            BtnPlus.ForceCloseActiveEditing();
        }
    }

    public void ToggleControlsEnabled()
    {
        SetControlsEnabled(!controlsEnabled);
    }

    private void EnterWorkMode()
    {
        ResolvePlayerReferences();

        if (isEditing || playerController == null || playerCamera == null)
        {
            if (!warnedMissingPlayerReferences)
            {
                warnedMissingPlayerReferences = true;
                Debug.LogWarning($"{nameof(LabWorkZoneController)} on {name}: cannot enter work mode because playerController or playerCamera was not found.");
            }

            return;
        }

        isEditing = true;
        wasControllerEnabled = playerController.enabled;
        SetControlsEnabled(false);
        HideZoneEntryPrompt();

        originalCameraParent = playerCamera.transform.parent;
        originalCameraPosition = playerCamera.transform.position;
        originalCameraRotation = playerCamera.transform.rotation;
        originalCameraFov = playerCamera.fieldOfView;

        if (playerController != null)
        {
            originalGravity = playerController.gravity;
            playerController.SetMoveControl(false);
            playerController.DisableAllMovement();
            playerController.enabled = false;
        }

        CursorStateUtility.Apply(CursorLockMode.None, false);
        Vector3 targetPosition = playerCamera.transform.position;
        Quaternion targetRotation = playerCamera.transform.rotation;
        if (viewingPoint != null)
        {
            targetPosition = viewingPoint.position;
            targetRotation = viewingPoint.rotation;
        }
        else if (!warnedMissingViewingPoint)
        {
            warnedMissingViewingPoint = true;
            Debug.LogWarning($"{nameof(LabWorkZoneController)} on {name}: viewingPoint is not assigned. Camera will be fixed at its current position.");
        }

        float targetViewingFov = Mathf.Clamp(viewingFov, zoomMinFov, zoomMaxFov);
        StartMoveCamera(targetPosition, targetRotation, targetViewingFov);
        LabSceneCrosshairBootstrap.OnInteractionStarted();
        PrepareInteractionPromptUi();
    }

    private void ExitWorkMode()
    {
        if (!isEditing)
        {
            return;
        }

        isEditing = false;
        SetControlsEnabled(false);
        ClearSelection();

        StartMoveCamera(originalCameraPosition, originalCameraRotation, originalCameraFov, true);
    }

    private void FinishExitMode()
    {
        if (playerCamera != null)
        {
            playerCamera.transform.SetParent(originalCameraParent);
        }

        if (playerController != null)
        {
            playerController.enabled = wasControllerEnabled;

            if (playerController.enabled)
            {
                playerController.gravity = originalGravity;
                playerController.SetMoveControl(true);
                playerController.EnableAllMovement();
            }
        }

        CursorStateUtility.Apply(CursorLockMode.Locked, false);
        LabSceneCrosshairBootstrap.OnInteractionEnded();

        if (isPlayerInZone)
        {
            ShowZoneEntryPrompt();
        }
    }

    private void HandleZoomInput()
    {
        if (playerCamera == null)
        {
            return;
        }

        float scroll = InputSystemCompat.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) <= 0.0001f)
        {
            return;
        }

        float nextFov = playerCamera.fieldOfView - scroll * zoomSensitivity;
        playerCamera.fieldOfView = Mathf.Clamp(nextFov, zoomMinFov, zoomMaxFov);
    }

    private void CheckForSelection()
    {
        if (playerCamera == null)
        {
            ClearSelection();
            return;
        }

        Ray ray = playerCamera.ScreenPointToRay(InputSystemCompat.GetMousePosition());
        Vector2 mousePosition = InputSystemCompat.GetMousePosition();
        if (!TrySelectFromRay(ray) && !TrySelectFromRendererBounds(mousePosition))
        {
            ClearSelection();
        }
    }

    private bool TrySelectFromRay(Ray ray)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray, maxDistance, raycastLayers, QueryTriggerInteraction.Collide);
        if (hits == null || hits.Length == 0)
        {
            return false;
        }

        System.Array.Sort(hits, CompareRaycastHits);
        foreach (RaycastHit hit in hits)
        {
            if (ShouldIgnoreRaycastHit(hit))
            {
                continue;
            }

            if (TrySelectObject(hit.transform))
            {
                return true;
            }
        }

        return false;
    }

    private int CompareRaycastHits(RaycastHit left, RaycastHit right)
    {
        return left.distance.CompareTo(right.distance);
    }

    private bool ShouldIgnoreRaycastHit(RaycastHit hit)
    {
        Collider hitCollider = hit.collider;
        if (hitCollider == null)
        {
            return true;
        }

        return ignoreZoneColliderInRaycast && zoneTrigger != null && hitCollider == zoneTrigger;
    }

    private bool TrySelectFromRendererBounds(Vector2 mousePosition)
    {
        if (!useRendererFallbackSelection || playerCamera == null)
        {
            return false;
        }

        MonoBehaviour[] behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Transform bestTarget = null;
        float bestDistance = float.PositiveInfinity;

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null || !(behaviour is WorkzoneSelectionController.IInteractable))
            {
                continue;
            }

            if (!TryGetScreenRect(behaviour.transform, out Rect screenRect, out float distance))
            {
                continue;
            }

            if (!screenRect.Contains(mousePosition) || distance >= bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            bestTarget = behaviour.transform;
        }

        return bestTarget != null && TrySelectObject(bestTarget);
    }

    private bool TryGetScreenRect(Transform target, out Rect screenRect, out float distance)
    {
        screenRect = default;
        distance = float.PositiveInfinity;

        Renderer[] renderers = target != null ? target.GetComponentsInChildren<Renderer>() : null;
        if (renderers == null || renderers.Length == 0)
        {
            return false;
        }

        bool hasBounds = false;
        Bounds bounds = default;
        foreach (Renderer rendererComponent in renderers)
        {
            if (rendererComponent == null || !rendererComponent.enabled)
            {
                continue;
            }

            if (!hasBounds)
            {
                bounds = rendererComponent.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(rendererComponent.bounds);
            }
        }

        if (!hasBounds)
        {
            return false;
        }

        Vector3 centerScreen = playerCamera.WorldToScreenPoint(bounds.center);
        if (centerScreen.z <= 0f)
        {
            return false;
        }

        distance = Vector3.Distance(playerCamera.transform.position, bounds.center);
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        Vector3[] corners =
        {
            new Vector3(min.x, min.y, min.z),
            new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z),
            new Vector3(min.x, max.y, max.z),
            new Vector3(max.x, min.y, min.z),
            new Vector3(max.x, min.y, max.z),
            new Vector3(max.x, max.y, min.z),
            new Vector3(max.x, max.y, max.z)
        };

        bool hasScreenPoint = false;
        Vector2 screenMin = Vector2.zero;
        Vector2 screenMax = Vector2.zero;
        for (int i = 0; i < corners.Length; i++)
        {
            Vector3 screenPoint = playerCamera.WorldToScreenPoint(corners[i]);
            if (screenPoint.z <= 0f)
            {
                continue;
            }

            Vector2 point = new Vector2(screenPoint.x, screenPoint.y);
            if (!hasScreenPoint)
            {
                screenMin = point;
                screenMax = point;
                hasScreenPoint = true;
            }
            else
            {
                screenMin = Vector2.Min(screenMin, point);
                screenMax = Vector2.Max(screenMax, point);
            }
        }

        if (!hasScreenPoint)
        {
            return false;
        }

        screenRect = Rect.MinMaxRect(screenMin.x, screenMin.y, screenMax.x, screenMax.y);
        return screenRect.width > 0f && screenRect.height > 0f;
    }

    private bool TrySelectObject(Transform target)
    {
        if (target == null)
        {
            return false;
        }

        IHoverable hoverable = FindHoverable(target);
        IInteractionPromptProvider promptProvider = FindPromptProvider(target);
        Outline outline = hoverable == null ? target.GetComponentInParent<Outline>() : null;
        WorkzoneSelectionController.IInteractable interactable = FindInteractable(target);
        if (outline == null && hoverable == null && interactable == null)
        {
            return false;
        }

        if (target == currentSelectedObject)
        {
            currentHoverable?.SetHoverState(true);
            UpdateInteractionPrompt();
            return true;
        }

        ClearSelection();
        currentSelectedObject = target;
        currentHoverable = hoverable;
        currentPromptProvider = promptProvider;
        currentOutlineComponent = outline;
        currentHoverable?.SetHoverState(true);

        if (currentOutlineComponent != null)
        {
            currentOutlineComponent.OutlineMode = Outline.Mode.OutlineAll;
            currentOutlineComponent.OutlineColor = highlightColor;
            currentOutlineComponent.OutlineWidth = 5f;
            currentOutlineComponent.enabled = true;
        }

        UpdateInteractionPrompt();
        return true;
    }

    private void ClearSelection()
    {
        if (currentHoverable != null)
        {
            currentHoverable.SetHoverState(false);
            currentHoverable = null;
        }

        if (currentOutlineComponent != null)
        {
            currentOutlineComponent.enabled = false;
            currentOutlineComponent = null;
        }

        currentSelectedObject = null;
        currentPromptProvider = null;
        ClearInteractionPrompt();
    }

    private void TryInteractWithSelectedObject()
    {
        if (MenuVisibilityCoordinator.AnyMenuOpen || currentSelectedObject == null)
        {
            return;
        }

        BtnOffOn offOn = currentSelectedObject.GetComponentInParent<BtnOffOn>(true);
        if (offOn != null)
        {
            offOn.Interact();
            UpdateInteractionPrompt();
            return;
        }

        BtnPlus plus = currentSelectedObject.GetComponentInParent<BtnPlus>(true);
        if (plus != null)
        {
            plus.Interact();
            UpdateInteractionPrompt();
            return;
        }

        WorkzoneSelectionController.IInteractable interactable = FindInteractable(currentSelectedObject);
        if (interactable == null)
        {
            return;
        }

        interactable.Interact();
        UpdateInteractionPrompt();
    }

    private void ShowZoneEntryPrompt()
    {
        if (isEditing || MenuVisibilityCoordinator.AnyMenuOpen)
        {
            HideZoneEntryPrompt();
            return;
        }

        EnsurePromptPrefabCanvas();
        if (promptPrefabCanvas == null)
        {
            return;
        }

        if (zoneEntryPromptPrefab != null)
        {
            if (zoneEntryPromptInstance == null || zoneEntryPromptSource != zoneEntryPromptPrefab)
            {
                HideZoneEntryPrompt();
                zoneEntryPromptSource = zoneEntryPromptPrefab;
                zoneEntryPromptInstance = Instantiate(zoneEntryPromptPrefab, promptPrefabCanvas.transform);
                zoneEntryPromptInstance.name = $"{zoneEntryPromptPrefab.name} (Zone Entry Runtime)";
            }
        }
        else if (zoneEntryPromptInstance == null || zoneEntryPromptSource != null)
        {
            HideZoneEntryPrompt();
            zoneEntryPromptSource = null;
            zoneEntryPromptInstance = CreateDefaultZoneEntryPrompt();
        }

        if (zoneEntryPromptInstance == null)
        {
            return;
        }

        zoneEntryPromptInstance.Show(false);
    }

    private void HideZoneEntryPrompt()
    {
        if (zoneEntryPromptInstance == null)
        {
            return;
        }

        zoneEntryPromptInstance.Hide();
        Destroy(zoneEntryPromptInstance.gameObject);
        zoneEntryPromptInstance = null;
        zoneEntryPromptSource = null;
    }

    private InteractionPromptPrefabView CreateDefaultZoneEntryPrompt()
    {
        GameObject root = new GameObject("ZoneEntryPromptRuntime", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        root.transform.SetParent(promptPrefabCanvas.transform, false);

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.sizeDelta = defaultZoneEntryPromptSize;
        rootRect.pivot = new Vector2(0.5f, 0.5f);

        Image background = root.GetComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.78f);

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(root.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24f, 12f);
        textRect.offsetMax = new Vector2(-24f, -12f);

        Text text = textObject.GetComponent<Text>();
        text.text = string.Empty;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 28;
        text.color = new Color(1f, 0.74f, 0.24f, 1f);
        text.raycastTarget = false;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
        {
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        return root.AddComponent<InteractionPromptPrefabView>();
    }

    private void ResolveZoneEntryPromptPrefab()
    {
        if (zoneEntryPromptPrefab != null || string.IsNullOrWhiteSpace(zoneEntryPromptResourcePath))
        {
            return;
        }

        zoneEntryPromptPrefab = Resources.Load<InteractionPromptPrefabView>(zoneEntryPromptResourcePath);
    }

    private void PrepareInteractionPromptUi()
    {
        ResolveCrosshairUI();
        EnsurePromptPrefabCanvas();

        if (crosshairUI != null)
        {
            crosshairUI.SetPromptVisible(true);
        }

        ClearInteractionPrompt();
    }

    private void UpdateInteractionPrompt()
    {
        if (currentSelectedObject == null || currentPromptProvider == null)
        {
            ClearInteractionPrompt();
            return;
        }

        bool active = currentPromptProvider.IsInteractionActive;
        InteractionPromptPrefabView promptPrefab = currentPromptProvider.InteractionPromptPrefab;
        if (promptPrefab != null)
        {
            ClearCrosshairPrompt();
            ShowPromptPrefab(promptPrefab, active);
            return;
        }

        HidePromptPrefab();
        ResolveCrosshairUI();
        if (crosshairUI == null)
        {
            return;
        }

        crosshairUI.SetPromptVisible(true);
        crosshairUI.SetCrosshairState(true, active);
        crosshairUI.SetWorldTarget(currentSelectedObject.gameObject, playerCamera, string.Empty, active);
    }

    private void ClearInteractionPrompt()
    {
        ClearCrosshairPrompt();
        HidePromptPrefab();
    }

    private void ClearCrosshairPrompt()
    {
        if (crosshairUI == null)
        {
            return;
        }

        crosshairUI.ClearTarget();
        crosshairUI.ClearPrompt();
        crosshairUI.SetCrosshairState(false, false);
    }

    private void ShowPromptPrefab(InteractionPromptPrefabView promptPrefab, bool active)
    {
        EnsurePromptPrefabCanvas();
        if (promptPrefabCanvas == null)
        {
            return;
        }

        if (currentPromptPrefabInstance == null || currentPromptPrefabSource != promptPrefab)
        {
            HidePromptPrefab();
            currentPromptPrefabSource = promptPrefab;
            currentPromptPrefabInstance = Instantiate(promptPrefab, promptPrefabCanvas.transform);
            currentPromptPrefabInstance.name = $"{promptPrefab.name} (Runtime)";
        }

        currentPromptPrefabInstance.Show(active);
    }

    private void HidePromptPrefab()
    {
        if (currentPromptPrefabInstance != null)
        {
            currentPromptPrefabInstance.Hide();
            Destroy(currentPromptPrefabInstance.gameObject);
            currentPromptPrefabInstance = null;
        }

        currentPromptPrefabSource = null;
    }

    private void ResolveCrosshairUI()
    {
        if (crosshairUI != null)
        {
            return;
        }

        crosshairUI = CrosshairPromptUI.Instance;
        if (crosshairUI == null)
        {
            crosshairUI = FindAnyObjectByType<CrosshairPromptUI>();
        }

        if (crosshairUI == null)
        {
            GameObject uiObject = new GameObject("CrosshairPromptUI");
            crosshairUI = uiObject.AddComponent<CrosshairPromptUI>();
        }
        else if (!crosshairUI.gameObject.activeSelf)
        {
            crosshairUI.gameObject.SetActive(true);
            crosshairUI.SetMenuEnabled(true);
        }
    }

    private void EnsurePromptPrefabCanvas()
    {
        if (promptPrefabCanvas != null)
        {
            return;
        }

        GameObject canvasObject = new GameObject("InteractionPromptPrefabCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);

        promptPrefabCanvas = canvasObject.GetComponent<Canvas>();
        promptPrefabCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        promptPrefabCanvas.sortingOrder = 10001;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
    }

    private void ResolvePlayerReferences()
    {
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<FirstPersonController>();
        }

        if (playerController == null)
        {
            return;
        }

        if (playerCamera == null)
        {
            playerCamera = playerController.GetComponentInChildren<Camera>();
        }

        if (characterController == null)
        {
            characterController = playerController.GetComponent<CharacterController>();
        }
    }

    private void RefreshZonePresenceFromPlayerPosition()
    {
        if (!usePositionFallback || isEditing)
        {
            return;
        }

        if (triggerContactCount > 0)
        {
            EnterPlayerZone();
            return;
        }

        ResolvePlayerReferences();
        if (playerController == null)
        {
            return;
        }

        bool isInside = IsPlayerPositionInsideZone();
        if (isInside)
        {
            EnterPlayerZone();
        }
        else
        {
            LeavePlayerZone();
        }
    }

    private bool IsPlayerPositionInsideZone()
    {
        if (zoneTrigger == null)
        {
            Vector3 playerPosition = playerController != null ? playerController.transform.position : transform.position;
            return (transform.position - playerPosition).sqrMagnitude <= fallbackActivationRadius * fallbackActivationRadius;
        }

        if (playerController != null && IsPointInsideZoneOrBounds(playerController.transform.position))
        {
            return true;
        }

        if (characterController != null)
        {
            Bounds bounds = characterController.bounds;
            if (IsPointInsideZoneOrBounds(bounds.center))
            {
                return true;
            }

            Vector3 lowerPoint = new Vector3(bounds.center.x, bounds.min.y + boundsCheckPadding, bounds.center.z);
            return IsPointInsideZoneOrBounds(lowerPoint);
        }

        return false;
    }

    private bool IsPointInsideZoneOrBounds(Vector3 point)
    {
        Vector3 closestPoint = zoneTrigger.ClosestPoint(point);
        float sqrDistance = (closestPoint - point).sqrMagnitude;
        if (sqrDistance <= boundsCheckPadding * boundsCheckPadding)
        {
            return true;
        }

        Bounds expandedBounds = zoneTrigger.bounds;
        expandedBounds.Expand(boundsCheckPadding * 2f);
        return expandedBounds.Contains(point);
    }

    private static WorkzoneSelectionController.IInteractable FindInteractable(Transform target)
    {
        MonoBehaviour[] behaviours = target != null ? target.GetComponentsInParent<MonoBehaviour>(true) : null;
        if (behaviours == null)
        {
            return null;
        }

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is WorkzoneSelectionController.IInteractable interactable)
            {
                return interactable;
            }
        }

        return null;
    }

    private static IHoverable FindHoverable(Transform target)
    {
        MonoBehaviour[] behaviours = target != null ? target.GetComponentsInParent<MonoBehaviour>(true) : null;
        if (behaviours == null)
        {
            return null;
        }

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IHoverable hoverable)
            {
                return hoverable;
            }
        }

        return null;
    }

    private static IInteractionPromptProvider FindPromptProvider(Transform target)
    {
        MonoBehaviour[] behaviours = target != null ? target.GetComponentsInParent<MonoBehaviour>(true) : null;
        if (behaviours == null)
        {
            return null;
        }

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IInteractionPromptProvider promptProvider)
            {
                return promptProvider;
            }
        }

        return null;
    }

    private void StartMoveCamera(Vector3 targetPosition, Quaternion targetRotation, float targetFov, bool isReturning = false)
    {
        if (cameraMoveRoutine != null)
        {
            StopCoroutine(cameraMoveRoutine);
        }

        cameraMoveRoutine = StartCoroutine(MoveCamera(targetPosition, targetRotation, targetFov, isReturning));
    }

    private IEnumerator MoveCamera(Vector3 targetPosition, Quaternion targetRotation, float targetFov, bool isReturning = false)
    {
        if (playerCamera == null)
        {
            yield break;
        }

        playerCamera.transform.SetParent(null);

        float elapsed = 0f;
        Vector3 startPosition = playerCamera.transform.position;
        Quaternion startRotation = playerCamera.transform.rotation;
        float startFov = playerCamera.fieldOfView;

        while (elapsed < transitionDuration)
        {
            float t = elapsed / transitionDuration;
            t = t * t * (3f - 2f * t);

            playerCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            playerCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            playerCamera.fieldOfView = Mathf.Lerp(startFov, targetFov, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        playerCamera.transform.position = targetPosition;
        playerCamera.transform.rotation = targetRotation;
        playerCamera.fieldOfView = targetFov;
        cameraMoveRoutine = null;

        if (isReturning)
        {
            FinishExitMode();
        }
    }

    private bool IsPlayerCollider(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        ResolvePlayerReferences();

        if (characterController != null && other == characterController)
        {
            return true;
        }

        if (playerController != null && other.transform.IsChildOf(playerController.transform))
        {
            return true;
        }

        FirstPersonController hitController = other.GetComponentInParent<FirstPersonController>();
        if (hitController == null)
        {
            return false;
        }

        if (playerController == null)
        {
            playerController = hitController;
            ResolvePlayerReferences();
            return true;
        }

        return hitController == playerController;
    }
}
