namespace EasyPeasyFirstPersonController
{
    using UnityEngine;
    using CanvasScaler = UnityEngine.UI.CanvasScaler;
    using GraphicRaycaster = UnityEngine.UI.GraphicRaycaster;
    using Image = UnityEngine.UI.Image;
    using Text = UnityEngine.UI.Text;

    public partial class FirstPersonController : MonoBehaviour
    {
        private bool movementInputEnabled = true;
        private bool lookInputEnabled = true;

        [Header("Settings")]
        public float walkSpeed = 3f;
        public float sprintSpeed = 5f;
        public float crouchSpeed = 1.5f;
        public float jumpSpeed = 4f;
        public float gravity = 9.81f;
        public float slideDuration = 0.7f;
        public float slideSpeed = 6f;
        public float mouseSensitivity = 2f;
        public float strafeTiltAmount = 2f;

        [Header("References")]
        public Transform playerCamera;
        public Transform cameraParent;
        public Transform groundCheck;
        public LayerMask groundMask;

        [HideInInspector] public CharacterController characterController;
        [HideInInspector] public IInputManager input;
        [HideInInspector] public Vector3 moveDirection;
        [HideInInspector] public bool isGrounded;

        private PlayerBaseState currentState;
        private PlayerStateFactory states;
        private float xRotation = 0f;
        private float currentTilt;
        private float tiltVelocity;

        [Header("Interaction Settings")]
        public float interactionDistance = 3f;
        public LayerMask interactionLayer = ~0;
        public Color interactionNotificationTextColor = Color.white;
        public Color interactionNotificationBackgroundColor = new Color(0f, 0f, 0f, 0.72f);
        public Vector2 interactionNotificationSize = new Vector2(520f, 80f);
        public float interactionNotificationBottomOffset = 42f;
        public float interactionNotificationSlideSpeed = 10f;

        public PlayerBaseState CurrentState { get => currentState; set => currentState = value; }

        [Header("Visual Settings")]
        public float normalFov = 60f;
        public float sprintFov = 75f;
        public float slideFovBoost = 5f;
        public float fovChangeSpeed = 8f;
        public float bobAmount = 0.001f;
        public float bobSpeed = 10f;
        public float recoilReturnSpeed = 5f;

        [HideInInspector] public Camera cam;
        [HideInInspector] public float targetFov;
        [HideInInspector] public float currentBobIntensity;
        [HideInInspector] public float currentBobSpeed;
        [HideInInspector] public float targetTilt;

        private float bobTimer;
        private float fovVelocity;
        private float originalCamY;
        private Transform currentInteractionTarget;
        private InteractionFeedback currentInteractionFeedback;
        private RectTransform interactionNotificationRoot;
        private Text interactionNotificationLabel;
        private bool interactionNotificationVisible;

        [Header("Height Settings")]
        public float standingCameraHeight = 1.75f;
        public float crouchingCameraHeight = 1f;
        public float crouchingCharacterControllerHeight = 1f;
        [HideInInspector] public float standingCharacterControllerHeight = 1.8f;
        [HideInInspector] public Vector3 standingCharacterControllerCenter = new Vector3(0, 0.9f, 0);
        [HideInInspector] public float targetCameraY;

        [Header("Ledge Settings")]
        public LayerMask ledgeLayer;
        public float ledgeDetectionDistance = 1f;
        private float landingMomentum;

        [Header("Swimming Settings")]
        public float swimSpeed = 4f;
        public float swimSprintSpeed = 6f;
        public float waterDrag = 2f;
        public LayerMask waterMask;
        [HideInInspector] public bool isInWater;

        [Header("Visual Preferences")]
        public bool useFovKick = true;
        public bool useHeadBob = true;
        public bool useCameraTilt = true;
        public bool useClimbTilt = true;

        [Header("Debug")]
        public bool currentStateDebug = true;

        void OnGUI()
        {
            if (currentState != null && Application.isEditor && currentStateDebug)
                GUILayout.Label("Current State: " + currentState.GetType().Name);
        }

        private void Awake()
        {
            cam = playerCamera.GetComponent<Camera>();
            targetFov = normalFov;
            targetCameraY = standingCameraHeight;
            originalCamY = standingCameraHeight;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            characterController = GetComponent<CharacterController>();
            standingCharacterControllerHeight = characterController.height;
            standingCharacterControllerCenter = characterController.center;
            input = GetComponent<IInputManager>();
            states = new PlayerStateFactory(this);

            currentState = states.Grounded();
            currentState.EnterState();
            EnsureInteractionNotification();
        }

        private void Update()
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, groundMask, QueryTriggerInteraction.Ignore);

            if (movementInputEnabled)
            {
                currentState.UpdateState();
            }
            else
            {
                moveDirection = Vector3.zero;
            }

            if (lookInputEnabled)
            {
                HandleRotation();
            }

            UpdateVisuals();
            UpdateInteractionOutline();
            HandleInteraction();
            UpdateInteractionNotification();
            UpdateInteractionNotificationVisual();
        }

        public void SetMoveControl(bool enabled)
        {
            lookInputEnabled = enabled;
        }

        public void DisableAllMovement()
        {
            movementInputEnabled = false;
            moveDirection = Vector3.zero;
        }

        public void EnableAllMovement()
        {
            movementInputEnabled = true;
        }

        public void ClearInteractionState()
        {
            ClearInteractionOutline();
            SetInteractionNotification(null);

            if (interactionNotificationRoot != null)
            {
                interactionNotificationRoot.anchoredPosition = GetNotificationHiddenPosition();
            }
        }

        private void HandleInteraction()
        {
            if (!InputSystemCompat.GetKeyDown(KeyCode.E))
            {
                return;
            }

            if (playerCamera == null)
            {
                return;
            }

            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayer))
            {
                return;
            }

            BtnSet setButton = hit.collider.GetComponentInParent<BtnSet>();
            if (setButton != null)
            {
                setButton.Press();
                NotifyInteractionFeedback(hit.collider);
                return;
            }

            BtnPlus plusButton = hit.collider.GetComponentInParent<BtnPlus>();
            if (plusButton != null)
            {
                plusButton.Press();
                NotifyInteractionFeedback(hit.collider);
                return;
            }

            MonoBehaviour[] behaviours = hit.collider.GetComponentsInParent<MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is WorkzoneSelectionController.IInteractable interactable)
                {
                    interactable.Interact();
                    NotifyInteractionFeedback(hit.collider);
                    return;
                }
            }
        }

        private void NotifyInteractionFeedback(Collider hitCollider)
        {
            InteractionFeedback feedback = hitCollider.GetComponentInParent<InteractionFeedback>();
            if (feedback != null)
            {
                feedback.SetActiveState(true);
            }
        }

        private void UpdateInteractionOutline()
        {
            if (playerCamera == null)
            {
                ClearInteractionOutline();
                return;
            }

            if (BtnPlus.AnyEditing || (WorkzoneSelectionController.Instance != null && WorkzoneSelectionController.Instance.IsWorkModeActive))
            {
                if (currentInteractionFeedback != null && !currentInteractionFeedback.IsActive)
                {
                    ClearInteractionOutline();
                }
                return;
            }

            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, interactionLayer))
            {
                ClearInteractionOutline();
                return;
            }

            Transform target = ResolveInteractionTarget(hit.collider);
            if (target == null)
            {
                ClearInteractionOutline();
                return;
            }

            if (currentInteractionTarget == target && currentInteractionFeedback != null)
            {
                return;
            }

            ClearInteractionOutline();

            currentInteractionTarget = target;
            currentInteractionFeedback = target.GetComponent<InteractionFeedback>();
            if (currentInteractionFeedback == null)
            {
                currentInteractionFeedback = target.GetComponentInParent<InteractionFeedback>();
            }

            currentInteractionFeedback?.SetHoverState(true);
        }

        private void UpdateInteractionNotification()
        {
            if (BtnPlus.AnyEditing)
            {
                InteractionFeedback activeFeedback = BtnPlus.ActiveEditingButton != null
                    ? BtnPlus.ActiveEditingButton.GetComponentInParent<InteractionFeedback>()
                    : null;
                SetInteractionNotification(activeFeedback != null ? activeFeedback.ActiveMessage : "Hold LMB to rotate crane\nPress E to exit");
                return;
            }

            if (WorkzoneSelectionController.Instance != null && WorkzoneSelectionController.Instance.IsWorkModeActive)
            {
                SetInteractionNotification(null);
                return;
            }

            if (currentInteractionTarget == null || currentInteractionFeedback == null)
            {
                SetInteractionNotification(null);
                return;
            }

            SetInteractionNotification(currentInteractionFeedback.HoverMessage);
        }

        private void SetInteractionNotification(string message)
        {
            interactionNotificationVisible = !string.IsNullOrEmpty(message);

            if (interactionNotificationLabel != null)
            {
                interactionNotificationLabel.text = message ?? string.Empty;
            }
        }

        private void EnsureInteractionNotification()
        {
            if (interactionNotificationRoot != null)
            {
                return;
            }

            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("InteractionNotificationCanvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }

            GameObject panelObject = new GameObject("InteractionNotification");
            panelObject.transform.SetParent(canvas.transform, false);

            Image panelImage = panelObject.AddComponent<Image>();
            panelImage.color = interactionNotificationBackgroundColor;

            interactionNotificationRoot = panelObject.GetComponent<RectTransform>();
            interactionNotificationRoot.anchorMin = new Vector2(0.5f, 0f);
            interactionNotificationRoot.anchorMax = new Vector2(0.5f, 0f);
            interactionNotificationRoot.pivot = new Vector2(0.5f, 0f);
            interactionNotificationRoot.sizeDelta = interactionNotificationSize;

            GameObject textObject = new GameObject("Label");
            textObject.transform.SetParent(panelObject.transform, false);

            interactionNotificationLabel = textObject.AddComponent<Text>();
            interactionNotificationLabel.text = string.Empty;
            interactionNotificationLabel.alignment = TextAnchor.MiddleCenter;
            interactionNotificationLabel.color = interactionNotificationTextColor;
            interactionNotificationLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            interactionNotificationLabel.resizeTextForBestFit = true;
            interactionNotificationLabel.resizeTextMinSize = 16;
            interactionNotificationLabel.resizeTextMaxSize = 26;

            RectTransform labelRect = textObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(20f, 10f);
            labelRect.offsetMax = new Vector2(-20f, -10f);

            interactionNotificationRoot.anchoredPosition = GetNotificationHiddenPosition();
        }

        private void UpdateInteractionNotificationVisual()
        {
            if (interactionNotificationRoot == null)
            {
                return;
            }

            Vector2 targetPosition = interactionNotificationVisible ? GetNotificationVisiblePosition() : GetNotificationHiddenPosition();
            float t = 1f - Mathf.Exp(-interactionNotificationSlideSpeed * Time.deltaTime);
            interactionNotificationRoot.anchoredPosition = Vector2.Lerp(interactionNotificationRoot.anchoredPosition, targetPosition, t);
        }

        private Vector2 GetNotificationVisiblePosition()
        {
            return new Vector2(0f, interactionNotificationBottomOffset);
        }

        private Vector2 GetNotificationHiddenPosition()
        {
            return new Vector2(0f, -interactionNotificationSize.y - 30f);
        }

        private Transform ResolveInteractionTarget(Collider hitCollider)
        {
            if (hitCollider == null)
            {
                return null;
            }

            BtnSet setButton = hitCollider.GetComponentInParent<BtnSet>();
            if (setButton != null)
            {
                return setButton.transform;
            }

            BtnPlus plusButton = hitCollider.GetComponentInParent<BtnPlus>();
            if (plusButton != null)
            {
                return plusButton.transform;
            }

            InteractionFeedback feedback = hitCollider.GetComponentInParent<InteractionFeedback>();
            if (feedback != null)
            {
                return feedback.transform;
            }

            MonoBehaviour[] behaviours = hitCollider.GetComponentsInParent<MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is WorkzoneSelectionController.IInteractable)
                {
                    return behaviour.transform;
                }
            }

            return null;
        }

        private void ClearInteractionOutline()
        {
            if (currentInteractionFeedback != null)
            {
                currentInteractionFeedback.ClearAll();
            }

            currentInteractionTarget = null;
            currentInteractionFeedback = null;
        }

        private void HandleRotation()
        {
            float mouseX = input.lookInput.x * mouseSensitivity;
            float mouseY = input.lookInput.y * mouseSensitivity;

            transform.Rotate(Vector3.up * mouseX);

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            float strafeTilt = useCameraTilt ? (-input.moveInput.x * strafeTiltAmount) : 0;
            float combinedTargetTilt = (useCameraTilt ? targetTilt : 0) + strafeTilt;

            currentTilt = Mathf.SmoothDamp(currentTilt, combinedTargetTilt, ref tiltVelocity, 0.1f);
            playerCamera.localRotation = Quaternion.Euler(xRotation, 0, currentTilt);
        }

        public void UpdateVisuals()
        {
            if (!useFovKick)
            {
                targetFov = normalFov;
            }
            cam.fieldOfView = Mathf.SmoothDamp(cam.fieldOfView, targetFov, ref fovVelocity, 1f / fovChangeSpeed);

            landingMomentum = Mathf.Lerp(landingMomentum, 0, Time.deltaTime * 10f);
            float newY = Mathf.Lerp(cameraParent.localPosition.y, targetCameraY, Time.deltaTime * 8f);

            if (useHeadBob && characterController.velocity.magnitude > 0.1f && isGrounded)
            {
                bobTimer += Time.deltaTime * currentBobSpeed;
                float bobOffset = Mathf.Sin(bobTimer) * currentBobIntensity;
                cameraParent.localPosition = new Vector3(cameraParent.localPosition.x, newY + bobOffset, cameraParent.localPosition.z);
            }
            else
            {
                bobTimer = 0;
                cameraParent.localPosition = new Vector3(cameraParent.localPosition.x, newY, cameraParent.localPosition.z);
            }
        }
        public bool HasCeiling()
        {
            float radius = characterController.radius * 0.9f;
            Vector3 origin = transform.position + Vector3.up * (characterController.height - radius);
            float checkDistance = standingCharacterControllerHeight - characterController.height + 0.1f;

            return Physics.SphereCast(origin, radius, Vector3.up, out _, checkDistance, groundMask, QueryTriggerInteraction.Ignore);
        }
        public bool CheckLedge(out Vector3 climbPosition)
        {
            climbPosition = Vector3.zero;
            RaycastHit wallHit;
            Vector3 wallOrigin = transform.position + Vector3.up * 1.5f;

            if (Physics.Raycast(wallOrigin, transform.forward, out wallHit, ledgeDetectionDistance, ledgeLayer, QueryTriggerInteraction.Ignore))
            {
                Vector3 ledgeOrigin = wallOrigin + Vector3.up * 0.6f + transform.forward * 0.2f;
                RaycastHit ledgeHit;

                if (!Physics.Raycast(ledgeOrigin, transform.forward, 0.5f, groundMask))
                {
                    if (Physics.Raycast(ledgeOrigin + transform.forward * 0.4f, Vector3.down, out ledgeHit, 1f, groundMask))
                    {
                        climbPosition = ledgeHit.point + Vector3.up * 1f;
                        return true;
                    }
                }
            }
            return false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & waterMask) != 0)
            {
                isInWater = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (((1 << other.gameObject.layer) & waterMask) != 0)
            {
                isInWater = false;
            }
        }

    }
}
