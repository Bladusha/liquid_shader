using System;
using System.Collections;
using EasyPeasyFirstPersonController;
using UnityEngine;

public class SlitWidthInteractionController : MonoBehaviour, WorkzoneSelectionController.IInteractable
{
    [Header("Work Zone Integration")]
    public bool requireWorkMode = true;

    [Header("��������� ����� ��������")]
    public string playerPrefsKey = "SlitWidth";
    public float minWidth = 0.01f;
    public float maxWidth = 1.0f;
    public float defaultValue = 0.1f;

    [Header("��������� ����������")]
    public float sensitivity = 0.005f;

    [Header("��������� �������� �������")]
    public Transform knobTransform;
    public float minAngle = -90f;
    public float maxAngle = 90f;
    public Axis rotationAxis = Axis.Y;

    [Header("��������� ������")]
    public Transform viewingPoint;
    public float transitionDuration = 0.5f;
    public float viewingFOV = 40f;

    [Header("�������")]
    public Action<float> OnWidthChanged;

    private float _currentWidth;
    private bool _isEditing = false;
    private Outline _outline;

    private FirstPersonController _playerController;
    private Camera _playerCamera;

    private Vector3 _originalPos;
    private Quaternion _originalRot;
    private Transform _originalParent;
    private float _originalFOV;
    private Coroutine _cameraMoveRoutine;

    public enum Axis { X, Y, Z }

    private void Awake()
    {
        _outline = GetComponent<Outline>();
        if (_outline != null)
        {
            _outline.enabled = false;
        }
    }

    void Start()
    {
        _currentWidth = PlayerPrefs.GetFloat(playerPrefsKey, defaultValue);

        _playerController = FindAnyObjectByType<FirstPersonController>();
        if (_playerController != null)
        {
            _playerCamera = _playerController.GetComponentInChildren<Camera>();
        }

        UpdateKnobRotation();
        OnWidthChanged?.Invoke(_currentWidth);
    }

    void Update()
    {
        bool isWorkModeActive = !requireWorkMode || (WorkzoneSelectionController.Instance != null && WorkzoneSelectionController.Instance.IsWorkModeActive);

        if (!isWorkModeActive)
        {
            if (_isEditing) ExitEditMode();
            if (_outline != null && _outline.enabled) _outline.enabled = false;
            return;
        }

        if (_isEditing)
        {
            HandleMouseInput();

            if (InputSystemCompat.GetKeyDown(KeyCode.E) || InputSystemCompat.GetKeyDown(KeyCode.Escape))
            {
                ExitEditMode();
            }
        }
    }

    public void Interact() => OnInteract();

    public void OnInteract()
    {
        if (MenuVisibilityCoordinator.AnyMenuOpen)
        {
            return;
        }

        if (_isEditing) return;

        if (requireWorkMode)
        {
            if (WorkzoneSelectionController.Instance == null || !WorkzoneSelectionController.Instance.IsWorkModeActive)
            {
                Debug.Log("�������������� ����������: ������� � ������� ����� (F)!");
                return;
            }
        }

        EnterEditMode();
    }

    public float GetCurrentValue() => _currentWidth;

    public void SetValue(float newValue)
    {
        _currentWidth = Mathf.Clamp(newValue, minWidth, maxWidth);
        UpdateKnobRotation();
        SaveValue();
        OnWidthChanged?.Invoke(_currentWidth);
    }

    void EnterEditMode()
    {
        if (_playerController == null || _playerCamera == null || viewingPoint == null)
        {
            return;
        }

        _isEditing = true;

        if (WorkzoneSelectionController.Instance != null)
        {
            WorkzoneSelectionController.Instance.SetRaycastPaused(true);
        }

        _playerController.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _originalParent = _playerCamera.transform.parent;
        _originalPos = _playerCamera.transform.position;
        _originalRot = _playerCamera.transform.rotation;
        _originalFOV = _playerCamera.fieldOfView;

        StartMoveCamera(viewingPoint.position, viewingPoint.rotation, viewingFOV);

        if (_outline != null) _outline.enabled = false;
        Debug.Log("����� ��������� ������: ���");
    }

    void ExitEditMode()
    {
        _isEditing = false;
        Debug.Log("����� ��������� ������: ����");
        StartMoveCamera(_originalPos, _originalRot, _originalFOV, true);
    }

    void FinishExit()
    {
        _playerCamera.transform.SetParent(_originalParent);
        if (_playerController != null) _playerController.enabled = true;

        if (WorkzoneSelectionController.Instance != null)
        {
            WorkzoneSelectionController.Instance.SetRaycastPaused(false);
        }
    }

    void HandleMouseInput()
    {
        float mouseX = InputSystemCompat.GetAxis("Mouse X");

        if (Mathf.Abs(mouseX) > 0.001f)
        {
            _currentWidth += mouseX * sensitivity;
            _currentWidth = Mathf.Clamp(_currentWidth, minWidth, maxWidth);

            UpdateKnobRotation();
            SaveValue();
            OnWidthChanged?.Invoke(_currentWidth);
        }
    }

    void UpdateKnobRotation()
    {
        if (knobTransform == null) return;

        float t = Mathf.InverseLerp(minWidth, maxWidth, _currentWidth);
        float angle = Mathf.Lerp(minAngle, maxAngle, t);

        Vector3 currentRot = knobTransform.localEulerAngles;
        switch (rotationAxis)
        {
            case Axis.X: knobTransform.localRotation = Quaternion.Euler(angle, currentRot.y, currentRot.z); break;
            case Axis.Y: knobTransform.localRotation = Quaternion.Euler(currentRot.x, angle, currentRot.z); break;
            case Axis.Z: knobTransform.localRotation = Quaternion.Euler(currentRot.x, currentRot.y, angle); break;
        }
    }

    void SaveValue()
    {
        PlayerPrefs.SetFloat(playerPrefsKey, _currentWidth);
    }

    private void StartMoveCamera(Vector3 targetPos, Quaternion targetRot, float targetFOV, bool isReturning = false)
    {
        if (_cameraMoveRoutine != null)
        {
            StopCoroutine(_cameraMoveRoutine);
        }

        _cameraMoveRoutine = StartCoroutine(MoveCamera(targetPos, targetRot, targetFOV, isReturning));
    }

    IEnumerator MoveCamera(Vector3 targetPos, Quaternion targetRot, float targetFOV, bool isReturning = false)
    {
        _playerCamera.transform.SetParent(null);

        float elapsed = 0f;
        Vector3 startPos = _playerCamera.transform.position;
        Quaternion startRot = _playerCamera.transform.rotation;
        float startFOV = _playerCamera.fieldOfView;

        while (elapsed < transitionDuration)
        {
            float t = elapsed / transitionDuration;
            t = t * t * (3f - 2f * t);

            _playerCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            _playerCamera.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
            _playerCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        _playerCamera.transform.position = targetPos;
        _playerCamera.transform.rotation = targetRot;
        _playerCamera.fieldOfView = targetFOV;
        _cameraMoveRoutine = null;

        if (isReturning) FinishExit();
    }
}
