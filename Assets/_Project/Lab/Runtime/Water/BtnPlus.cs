using System.Collections;
using EasyPeasyFirstPersonController;
using UnityEngine;

public class BtnPlus : MonoBehaviour, WorkzoneSelectionController.IInteractable
{
    [Header("Связи")]
    public WaterController controller;
    public Transform buttonVisual;
    public Transform knobTransform;
    public Transform viewingPoint;
    public FirstPersonController playerController;

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
    public KeyCode exitKey = KeyCode.E;
    public float rotationSensitivity = 0.35f;
    public float transitionDuration = 0.35f;
    public float viewingFov = 45f;
    [Range(0f, 1f)] public float currentOpenAmount = 0f;
    public Axis rotationAxis = Axis.Y;
    public float minAngle = -90f;
    public float maxAngle = 90f;

    [Header("Анимация кнопки")]
    public Vector3 pressedOffset = new Vector3(0, -0.05f, 0);
    public float animSpeed = 6f;

    private static BtnPlus activeEditingButton;

    private Vector3 startPos;
    private bool isActivated;
    private bool isEditing;
    private Camera playerCamera;
    private Transform originalCameraParent;
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private float originalCameraFov;
    private Coroutine cameraMoveRoutine;
    private bool wasControllerEnabled;

    public enum Axis
    {
        X,
        Y,
        Z
    }

    public bool IsEditing => isEditing;
    public static BtnPlus ActiveEditingButton => activeEditingButton;
    public static bool AnyEditing => activeEditingButton != null;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = FindAnyObjectByType<FirstPersonController>();
        }

        if (playerController != null)
        {
            playerCamera = playerController.GetComponentInChildren<Camera>();
        }
    }

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

        HandleEditingInput();

        if (InputSystemCompat.GetKeyDown(exitKey) || InputSystemCompat.GetKeyDown(KeyCode.Escape))
        {
            ExitEditMode();
        }
    }

    public void Interact()
    {
        if (MenuVisibilityCoordinator.AnyMenuOpen)
        {
            return;
        }

        Press();
    }

    public void Press()
    {
        if (MenuVisibilityCoordinator.AnyMenuOpen && !isEditing)
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

    public void SetPressedVisual(bool pressed)
    {
        isActivated = pressed;
    }

    private void EnterEditMode()
    {
        if (controller == null || playerController == null || playerCamera == null || viewingPoint == null)
        {
            return;
        }

        if (activeEditingButton != null && activeEditingButton != this)
        {
            activeEditingButton.ExitEditMode();
        }

        activeEditingButton = this;
        isEditing = true;
        wasControllerEnabled = playerController.enabled;

        originalCameraParent = playerCamera.transform.parent;
        originalCameraPosition = playerCamera.transform.position;
        originalCameraRotation = playerCamera.transform.rotation;
        originalCameraFov = playerCamera.fieldOfView;

        playerController.SetMoveControl(false);
        playerController.DisableAllMovement();
        playerController.enabled = false;

        if (WorkzoneSelectionController.Instance != null)
        {
            WorkzoneSelectionController.Instance.SetRaycastPaused(true);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;

        StartMoveCamera(viewingPoint.position, viewingPoint.rotation, viewingFov);
        RefreshPressedState();
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

        InteractionFeedback feedback = GetComponentInParent<InteractionFeedback>();
        if (feedback != null)
        {
            feedback.SetActiveState(false);
        }

        StartMoveCamera(originalCameraPosition, originalCameraRotation, originalCameraFov, true);
        RefreshPressedState();
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
            playerController.SetMoveControl(true);
            playerController.EnableAllMovement();
        }

        if (WorkzoneSelectionController.Instance != null)
        {
            WorkzoneSelectionController.Instance.SetRaycastPaused(false);
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        RefreshPressedState();
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
}
