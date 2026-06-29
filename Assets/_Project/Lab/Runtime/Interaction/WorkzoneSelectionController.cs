using System.Reflection;
using EasyPeasyFirstPersonController;
using UnityEngine;
using UnityEngine.UI;

public class WorkzoneSelectionController : MonoBehaviour
{
    public static WorkzoneSelectionController Instance { get; private set; }

    public bool IsWorkModeActive => isSystemActive;
    public bool IsPlayerInZone => isPlayerInZone;

    public interface IInteractable
    {
        void Interact();
    }

    [Header("UI Settings")]
    public Canvas mainCanvas;
    public GameObject notificationUI;
    public GameObject menuPrefab;

    [Header("Zone Settings")]
    public Collider zoneTrigger;
    public KeyCode toggleModeKey = KeyCode.F;

    [Header("Plane Movement Settings")]
    public FirstPersonController fpsController;
    public float moveSpeed = 3f;
    public float fixedY = 1f;

    [Header("Movement Limits")]
    public float minX = -5f;
    public float maxX = 5f;
    public float minZ = -5f;
    public float maxZ = 5f;

    [SerializeField] private bool isPlayerInZone;
    [SerializeField] private bool isSystemActive;

    [Header("Raycast Settings")]
    public LayerMask raycastLayers = ~0;
    public float maxDistance = 4f;

    [Header("Interaction Settings")]
    public KeyCode interactionKey = KeyCode.E;
    public Color highlightColor = Color.yellow;

    [Header("Hover UI")]
    public CrosshairPromptUI crosshairUI;

    private bool isRaycastPaused;
    private float defaultGravity = 9.81f;
    private CharacterController characterController;
    private Transform currentSelectedObject;
    private InteractionFeedback currentFeedback;
    private Outline currentOutlineComponent;
    private IInteractable currentInteractable;
    private GameObject currentMenuInstance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        MenuVisibilityCoordinator.AnyMenuStateChanged += HandleAnyMenuStateChanged;
    }

    private void OnDisable()
    {
        MenuVisibilityCoordinator.AnyMenuStateChanged -= HandleAnyMenuStateChanged;
    }

    private void Start()
    {
        if (mainCanvas == null)
        {
            mainCanvas = FindAnyObjectByType<Canvas>();
        }

        if (fpsController == null)
        {
            fpsController = FindAnyObjectByType<FirstPersonController>();
        }

        if (fpsController != null)
        {
            characterController = fpsController.GetComponent<CharacterController>();
        }

        if (notificationUI != null)
        {
            notificationUI.SetActive(false);
        }

        if (crosshairUI == null)
        {
            crosshairUI = CrosshairPromptUI.Instance;
        }

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

        if (crosshairUI != null)
        {
            crosshairUI.SetPromptVisible(true);
        }

        CursorStateUtility.Apply(CursorLockMode.Locked, false);

        UpdateNotificationVisibility();
    }

    private void Update()
    {
        HandleZoneLogic();

        if (!isSystemActive)
        {
            return;
        }

        HandlePlaneInput();

        if (MenuVisibilityCoordinator.AnyMenuOpen)
        {
            ClearSelection();
            return;
        }

        if (isRaycastPaused)
        {
            return;
        }

        CheckForSelection();
        if (InputSystemCompat.GetKeyDown(interactionKey) && currentSelectedObject != null)
        {
            TryInteractWithObject();
        }
    }

    private void LateUpdate()
    {
        if (isSystemActive && fpsController != null)
        {
            ForcePlayerPosition();
        }
    }

    public void SetRaycastPaused(bool paused)
    {
        isRaycastPaused = paused;
        if (paused)
        {
            ClearSelection();
            if (crosshairUI != null)
            {
                crosshairUI.SetCrosshairState(false, false);
                crosshairUI.ClearPrompt();
                crosshairUI.ClearTarget();
            }
        }
    }

    private void HandleZoneLogic()
    {
        if (zoneTrigger != null && isPlayerInZone && InputSystemCompat.GetKeyDown(toggleModeKey))
        {
            ToggleSystemState(!isSystemActive);
        }
    }

    private void ToggleSystemState(bool state)
    {
        isSystemActive = state;
        isRaycastPaused = false;

        if (fpsController == null)
        {
            return;
        }

        if (isSystemActive)
        {
            defaultGravity = fpsController.gravity;
            fpsController.gravity = 0f;
            if (characterController != null)
            {
                characterController.enabled = false;
            }

            fpsController.SetMoveControl(false);
            fpsController.DisableAllMovement();
            ForcePlayerPosition();
        }
        else
        {
            fpsController.gravity = defaultGravity;
            if (characterController != null)
            {
                characterController.enabled = true;
            }

            fpsController.SetMoveControl(true);
            fpsController.EnableAllMovement();
            ClearSelection();
        }

        if (notificationUI != null)
        {
            UpdateNotificationVisibility();
        }
    }

    private void HandlePlaneInput()
    {
        if (fpsController == null || isRaycastPaused)
        {
            return;
        }

        float inputX = InputSystemCompat.GetAxis("Horizontal");
        float inputZ = InputSystemCompat.GetAxis("Vertical");
        if (Mathf.Abs(inputX) <= 0.01f && Mathf.Abs(inputZ) <= 0.01f)
        {
            return;
        }

        Transform cameraTransform = fpsController.playerCamera;
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (forward * inputZ + right * inputX).normalized;
        fpsController.transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }

    private void ForcePlayerPosition()
    {
        if (fpsController == null)
        {
            return;
        }

        Vector3 position = fpsController.transform.position;
        fpsController.transform.position = new Vector3(
            Mathf.Clamp(position.x, minX, maxX),
            fixedY,
            Mathf.Clamp(position.z, minZ, maxZ));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (zoneTrigger != null && other == zoneTrigger)
        {
            isPlayerInZone = true;
            UpdateNotificationVisibility();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (zoneTrigger != null && other == zoneTrigger)
        {
            isPlayerInZone = false;
            if (isSystemActive)
            {
                ToggleSystemState(false);
            }

            if (notificationUI != null)
            {
                UpdateNotificationVisibility();
            }
        }
    }

    private void HandleAnyMenuStateChanged(bool anyMenuOpen)
    {
        UpdateNotificationVisibility();
    }

    private void UpdateNotificationVisibility()
    {
        if (notificationUI == null)
        {
            return;
        }

        bool shouldShow = !MenuVisibilityCoordinator.AnyMenuOpen && isSystemActive && isPlayerInZone;
        notificationUI.SetActive(shouldShow);
    }

    private void CheckForSelection()
    {
        if (fpsController == null || fpsController.playerCamera == null)
        {
            ClearSelection();
            return;
        }

        Transform cameraTransform = fpsController.playerCamera;
        bool found = Physics.Raycast(cameraTransform.position, cameraTransform.forward, out RaycastHit hit, maxDistance, raycastLayers)
            && TrySelectObject(hit.transform);

        if (!found)
        {
            ClearSelection();
        }
    }

    private bool TrySelectObject(Transform target)
    {
        InteractionFeedback feedback = target.GetComponentInParent<InteractionFeedback>();
        Outline outline = target.GetComponentInParent<Outline>();
        IInteractable interactable = FindInteractable(target);
        if (outline == null && feedback == null && interactable == null)
        {
            return false;
        }

        if (target == currentSelectedObject)
        {
            if (currentFeedback != null)
            {
                currentFeedback.SetHoverState(true);
                if (crosshairUI != null)
                {
                    crosshairUI.ShowPrompt(currentFeedback.HoverMessage);
                    crosshairUI.SetCrosshairState(true, currentFeedback.IsActive);
                    UpdateCrosshairTarget(target, currentFeedback.HoverMessage, currentFeedback.IsActive);
                }
            }
            else if (crosshairUI != null)
            {
                string prompt = "Press E to interact";
                crosshairUI.ShowPrompt(prompt);
                crosshairUI.SetCrosshairState(true, false);
                UpdateCrosshairTarget(target, prompt, false);
            }

            return true;
        }

        ClearSelection();
        currentSelectedObject = target;
        currentFeedback = feedback;
        currentOutlineComponent = outline;
        currentInteractable = interactable;
        if (currentFeedback != null)
        {
            currentFeedback.SetHoverState(true);
            if (crosshairUI != null)
            {
                crosshairUI.ShowPrompt(currentFeedback.HoverMessage);
                crosshairUI.SetCrosshairState(true, currentFeedback.IsActive);
                UpdateCrosshairTarget(target, currentFeedback.HoverMessage, currentFeedback.IsActive);
            }
        }
        else if (crosshairUI != null)
        {
            string prompt = interactable != null ? "Нажмите E для взаимодействия" : "Press E to interact";
            crosshairUI.ShowPrompt(prompt);
            crosshairUI.SetCrosshairState(true, false);
            UpdateCrosshairTarget(target, prompt, false);
        }

        if (outline != null)
        {
            outline.OutlineMode = Outline.Mode.OutlineAll;
            outline.OutlineColor = highlightColor;
            outline.OutlineWidth = 5f;
            outline.enabled = true;
        }
        CreateMenu(target);
        return true;
    }

    private void CreateMenu(Transform target)
    {
        if (currentMenuInstance != null)
        {
            Destroy(currentMenuInstance);
        }

        if (menuPrefab == null || mainCanvas == null)
        {
            return;
        }

        currentMenuInstance = Instantiate(menuPrefab, mainCanvas.transform);
        RectTransform rectTransform = currentMenuInstance.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localPosition = Vector3.zero;
        }

        SetupMenu(target);
    }

    private void SetupMenu(Transform target)
    {
        foreach (Text text in currentMenuInstance.GetComponentsInChildren<Text>(true))
        {
            if (text.name.Contains("Name"))
            {
                text.text = target.name;
            }
        }

        foreach (Button button in currentMenuInstance.GetComponentsInChildren<Button>(true))
        {
            if (button.name.Contains("Interact"))
            {
                button.onClick.AddListener(TryInteractWithObject);
            }
        }
    }

    private void TryInteractWithObject()
    {
        if (MenuVisibilityCoordinator.AnyMenuOpen)
        {
            return;
        }

        if (currentSelectedObject == null)
        {
            return;
        }

        IInteractable interactable = currentInteractable != null ? currentInteractable : FindInteractable(currentSelectedObject);
        if (interactable != null)
        {
            if (!(interactable is BtnPlus) && !(interactable is BtnSet))
            {
                SceneActivityLog.Add("Взаимодействие", $"Объект: {GetLogTargetName(currentSelectedObject)}");
            }

            interactable.Interact();
        }
        else
        {
            SceneActivityLog.Add("Взаимодействие", $"Объект: {GetLogTargetName(currentSelectedObject)}");

            MonoBehaviour behaviour = FindInteractableBehaviour(currentSelectedObject);
            MethodInfo method = behaviour?.GetType().GetMethod("OnInteract");
            if (method != null)
            {
                method.Invoke(behaviour, null);
            }
        }

        if (currentMenuInstance != null)
        {
            Destroy(currentMenuInstance);
            currentMenuInstance = null;
        }

        if (crosshairUI != null)
        {
            crosshairUI.SetCrosshairState(false, false);
            crosshairUI.ClearPrompt();
            crosshairUI.ClearTarget();
        }
    }

    private void ClearSelection()
    {
        if (currentFeedback != null)
        {
            currentFeedback.ClearHoverState();
            currentFeedback = null;
        }

        if (currentOutlineComponent != null)
        {
            currentOutlineComponent.enabled = false;
            currentOutlineComponent = null;
        }

        if (currentMenuInstance != null)
        {
            Destroy(currentMenuInstance);
            currentMenuInstance = null;
        }

        currentSelectedObject = null;
        currentInteractable = null;

        if (crosshairUI != null)
        {
            crosshairUI.SetCrosshairState(false, false);
            crosshairUI.ClearPrompt();
            crosshairUI.ClearTarget();
        }
    }

    private void UpdateCrosshairTarget(Transform target, string prompt, bool active)
    {
        if (crosshairUI == null || target == null)
        {
            return;
        }

        Camera camera = null;
        if (fpsController != null && fpsController.playerCamera != null)
        {
            camera = fpsController.playerCamera.GetComponent<Camera>();
        }

        crosshairUI.SetWorldTarget(target.gameObject, camera, prompt, active);
    }

    private static IInteractable FindInteractable(Transform target)
    {
        MonoBehaviour[] behaviours = target != null ? target.GetComponentsInParent<MonoBehaviour>(true) : null;
        if (behaviours == null)
        {
            return null;
        }

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IInteractable interactable)
            {
                return interactable;
            }
        }

        return null;
    }

    private static MonoBehaviour FindInteractableBehaviour(Transform target)
    {
        MonoBehaviour[] behaviours = target != null ? target.GetComponentsInParent<MonoBehaviour>(true) : null;
        if (behaviours == null)
        {
            return null;
        }

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IInteractable)
            {
                return behaviour;
            }
        }

        return null;
    }

    private static string GetLogTargetName(Transform target)
    {
        if (target == null)
        {
            return "неизвестный объект";
        }

        InteractionFeedback feedback = target.GetComponentInParent<InteractionFeedback>();
        if (feedback != null && !string.IsNullOrWhiteSpace(feedback.HoverMessage))
        {
            return $"{target.name} ({feedback.HoverMessage})";
        }

        return target.name;
    }
}
