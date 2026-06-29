using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CrosshairPromptUI : MonoBehaviour
{
    public enum CursorStyle
    {
        Corners,
        Full
    }

    public static CrosshairPromptUI Instance { get; private set; }

    [Header("Cursor")]
    [SerializeField] private CursorStyle style = CursorStyle.Corners;
    [SerializeField] private float dotSize = 10f;
    [SerializeField] private float outlineSpace = 10f;
    [SerializeField] private float outlineSize = 4f;
    [SerializeField] private Color dotColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField] private Color outlineColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField] private bool useBlend = true;
    [SerializeField] private Vector2 topLeftHoverPadding = new Vector2(-13f, 5f);
    [SerializeField] private Vector2 topRightHoverPadding = new Vector2(5f, 5f);
    [SerializeField] private Vector2 bottomLeftHoverPadding = new Vector2(-13f, -12f);
    [SerializeField] private Vector2 bottomRightHoverPadding = new Vector2(5f, -12f);
    [SerializeField] private float hoverLerp = 0.15f;
    [SerializeField] private float sizeLerp = 0.15f;
    [SerializeField] private float rotationLerp = 0.15f;
    [SerializeField] private float idleSpinSpeed = 90f;
    [SerializeField] private float hoverScale = 1.0f;
    [SerializeField] private float idleScale = 0.8f;
    [SerializeField] private float cornerDistance = 14f;
    [SerializeField] private float cornerHoverDistanceMultiplier = 1f;
    [SerializeField] private float cornerHoverGap = 0f;
    [SerializeField] private float cornerIdleOrbitSpeed = 90f;
    [SerializeField] private float cornerIdleScale = 0.9f;
    [SerializeField] private float cornerHoverScale = 1f;
    [SerializeField] private float cornerActiveScale = 1.08f;
    [SerializeField] private bool showPrompt = true;
    [SerializeField] private Vector2 promptSize = new Vector2(900f, 120f);
    [SerializeField] private Vector2 promptOffset = new Vector2(0f, 90f);
    [SerializeField] private Color promptBackground = new Color(0f, 0f, 0f, 0.45f);
    [SerializeField] private Color promptTextColor = new Color(1f, 0.5f, 0f, 1f);
    [SerializeField] private string idleText = "";
    [SerializeField] private string defaultPrompt = "Наведите взгляд на объект для взаимодействия";

    private Canvas canvas;
    private RectTransform canvasRect;
    private RectTransform dotRoot;
    private RawImage dotImage;
    private static Texture2D circleTexture;
    private readonly List<SatelliteCorner> satellites = new();
    private RectTransform outlineRoot;
    private readonly List<RectTransform> outlineSegments = new();
    private RectTransform promptRoot;
    private TMP_Text promptText;
    private PointerEventData pointerEventData;
    private readonly List<RaycastResult> raycastResults = new();
    private bool cursorStateCaptured;
    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisible;
    private bool enabledForMenu = true;
    private bool controlCursorState = true;
    private bool realSceneMode;
    private bool hasExternalTarget;
    private Vector2 externalTargetCenter;
    private Vector2 externalTargetSize;
    private string externalPrompt = "";
    private bool externalActive;
    private bool hasManualState;
    private bool manualHovering;
    private bool manualActive;
    private float currentRotation;
    private Vector2 currentCenter;
    private Vector2 currentSize;
    private bool currentHovering;
    private bool currentTyping;
    private bool currentActive;
    private float cornerOrbitRotation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildUi();
        SetMenuEnabled(true);
        ClearTarget();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        RestoreCursorState();
    }

    private void Update()
    {
        if (!enabledForMenu || canvas == null)
        {
            return;
        }

        Vector2 mousePosition = GetMousePosition();
        bool gameplayLockedMode = realSceneMode && Cursor.lockState == CursorLockMode.Locked;
        HoverTarget target = gameplayLockedMode
            ? HoverTarget.None
            : hasExternalTarget
                ? new HoverTarget(externalTargetCenter, externalTargetSize, externalPrompt, externalActive, true, false)
                : DetectHoverTarget(mousePosition);

        bool actualHovering = target.IsHovering;
        bool hovering = actualHovering || (hasManualState && manualHovering);
        bool active = target.IsActive || (hasManualState && manualActive);

        currentHovering = hovering;
        currentTyping = target.IsTyping;
        currentActive = active;

        if (showPrompt)
        {
            if (target.ShouldShowPrompt)
            {
                ShowPrompt(string.IsNullOrWhiteSpace(target.Prompt) ? defaultPrompt : target.Prompt);
            }
            else
            {
                ClearPrompt();
            }
        }

        Vector2 targetCenter = gameplayLockedMode
            ? new Vector2(Screen.width * 0.5f, Screen.height * 0.5f)
            : actualHovering ? target.Center : mousePosition;
        Vector2 targetSize = actualHovering ? target.Size : new Vector2(outlineSpace, outlineSpace);
        AnimateTo(mousePosition, targetCenter, targetSize, hovering, target.IsTyping, active, gameplayLockedMode);
    }

    public void SetMenuEnabled(bool enabled)
    {
        enabledForMenu = enabled;
        if (canvas != null)
        {
            canvas.enabled = enabled;
        }

        if (enabled)
        {
            ClearTarget();
            ClearPrompt();
            if (controlCursorState)
            {
                CaptureCursorState();
                CursorStateUtility.Apply(CursorLockMode.None, false);
            }
        }
        else
        {
            if (controlCursorState)
            {
                RestoreCursorState();
            }
        }
    }

    public void SetCursorStateControl(bool enabled)
    {
        controlCursorState = enabled;
    }

    public void SetRealSceneMode(bool enabled)
    {
        realSceneMode = enabled;
        if (enabled)
        {
            SetPromptVisible(false);
        }
    }

    public void ShowPrompt(string text)
    {
        if (!showPrompt || promptText == null)
        {
            return;
        }

        promptText.text = string.IsNullOrWhiteSpace(text) ? defaultPrompt : text;
        promptRoot.gameObject.SetActive(true);
    }

    public void ClearPrompt()
    {
        if (!showPrompt || promptText == null)
        {
            return;
        }

        promptText.text = idleText;
        promptRoot.gameObject.SetActive(false);
    }

    public void SetPromptVisible(bool visible)
    {
        showPrompt = visible;
        if (!visible && promptRoot != null)
        {
            if (promptText != null)
            {
                promptText.text = idleText;
            }

            promptRoot.gameObject.SetActive(false);
        }
    }

    public void ClearTarget()
    {
        hasExternalTarget = false;
        externalPrompt = "";
        externalActive = false;
        externalTargetCenter = Vector2.zero;
        externalTargetSize = Vector2.zero;
        hasManualState = false;
        manualHovering = false;
        manualActive = false;
    }

    private void CaptureCursorState()
    {
        if (cursorStateCaptured)
        {
            return;
        }

        previousCursorLockState = Cursor.lockState;
        previousCursorVisible = Cursor.visible;
        cursorStateCaptured = true;
    }

    private void RestoreCursorState()
    {
        if (!cursorStateCaptured)
        {
            return;
        }

        CursorStateUtility.Apply(previousCursorLockState, previousCursorVisible);
        cursorStateCaptured = false;
    }

    public void SetCrosshairState(bool hovering, bool active)
    {
        hasManualState = true;
        manualHovering = hovering;
        manualActive = active;
    }

    public void SetExternalTarget(Vector2 center, Vector2 size, string prompt, bool active)
    {
        hasExternalTarget = true;
        externalTargetCenter = center;
        externalTargetSize = size;
        externalPrompt = prompt;
        externalActive = active;
    }

    public void SetWorldTarget(GameObject targetObject, Camera camera, string prompt, bool active)
    {
        if (targetObject == null)
        {
            ClearTarget();
            return;
        }

        Bounds bounds;
        if (!TryGetWorldBounds(targetObject, out bounds))
        {
            Vector3 fallback = camera != null ? camera.WorldToScreenPoint(targetObject.transform.position) : Vector3.zero;
            SetExternalTarget(new Vector2(fallback.x, fallback.y), new Vector2(outlineSpace, outlineSpace), prompt, active);
            return;
        }

        if (camera == null)
        {
            camera = Camera.main;
        }

        if (camera == null)
        {
            SetExternalTarget(new Vector2(Screen.width * 0.5f, Screen.height * 0.5f), new Vector2(outlineSpace, outlineSpace), prompt, active);
            return;
        }

        Vector2 center = camera.WorldToScreenPoint(bounds.center);
        Vector2 size = ProjectBoundsToScreenSize(bounds, camera);
        if (size == Vector2.zero)
        {
            size = new Vector2(outlineSpace, outlineSpace);
        }

        SetExternalTarget(center, size, prompt, active);
    }

    private void BuildUi()
    {
        GameObject canvasObject = new GameObject("CrosshairPromptUI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasRect = canvasObject.GetComponent<RectTransform>();

        dotRoot = CreateRect("Dot", canvasObject.transform, Vector2.zero, Vector2.zero, Vector2.one * dotSize);
        dotImage = CreateFilledGraphic(dotRoot, dotColor);
        BuildSatellites(dotRoot);

        if (style == CursorStyle.Full)
        {
            outlineRoot = CreateRect("Outline", canvasObject.transform, Vector2.zero, Vector2.zero, Vector2.one * outlineSpace);
            BuildOutlineSegments(outlineRoot);
        }
        else
        {
            outlineRoot = null;
        }

        if (useBlend)
        {
            // Unity UI does not support CSS mix-blend-mode directly.
            // This flag is kept for inspector parity with CrosshairJs.
        }

        promptRoot = CreateRect("Prompt", canvasObject.transform, promptOffset, new Vector2(0.5f, 0f), promptSize);
        Image promptBackgroundImage = promptRoot.gameObject.AddComponent<Image>();
        promptBackgroundImage.color = promptBackground;

        GameObject promptTextObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        promptTextObject.transform.SetParent(promptRoot, false);
        RectTransform textRect = promptTextObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(24f, 16f);
        textRect.offsetMax = new Vector2(-24f, -16f);

        promptText = promptTextObject.GetComponent<TextMeshProUGUI>();
        promptText.fontSize = 28f;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.color = promptTextColor;
        promptText.raycastTarget = false;
        promptRoot.gameObject.SetActive(false);
    }

    private void BuildOutlineSegments(RectTransform parent)
    {
        if (style == CursorStyle.Full)
        {
            outlineSegments.Add(CreateBar(parent, "Top", new Vector2(0f, 0.5f), new Vector2(1f, outlineSize), new Vector2(0.5f, 1f)));
            outlineSegments.Add(CreateBar(parent, "Bottom", new Vector2(0f, -0.5f), new Vector2(1f, outlineSize), new Vector2(0.5f, 0f)));
            outlineSegments.Add(CreateBar(parent, "Left", new Vector2(-0.5f, 0f), new Vector2(outlineSize, 1f), new Vector2(1f, 0.5f)));
            outlineSegments.Add(CreateBar(parent, "Right", new Vector2(0.5f, 0f), new Vector2(outlineSize, 1f), new Vector2(0f, 0.5f)));
            return;
        }

        outlineSegments.Add(CreateBar(parent, "TopLeftH", new Vector2(-0.5f, 0.5f), new Vector2(0.5f, outlineSize), new Vector2(0f, 1f)));
        outlineSegments.Add(CreateBar(parent, "TopLeftV", new Vector2(-0.5f, 0.5f), new Vector2(outlineSize, 0.5f), new Vector2(0f, 1f)));

        outlineSegments.Add(CreateBar(parent, "TopRightH", new Vector2(0.5f, 0.5f), new Vector2(0.5f, outlineSize), new Vector2(1f, 1f)));
        outlineSegments.Add(CreateBar(parent, "TopRightV", new Vector2(0.5f, 0.5f), new Vector2(outlineSize, 0.5f), new Vector2(1f, 1f)));

        outlineSegments.Add(CreateBar(parent, "BottomLeftH", new Vector2(-0.5f, -0.5f), new Vector2(0.5f, outlineSize), new Vector2(0f, 0f)));
        outlineSegments.Add(CreateBar(parent, "BottomLeftV", new Vector2(-0.5f, -0.5f), new Vector2(outlineSize, 0.5f), new Vector2(0f, 0f)));

        outlineSegments.Add(CreateBar(parent, "BottomRightH", new Vector2(0.5f, -0.5f), new Vector2(0.5f, outlineSize), new Vector2(1f, 0f)));
        outlineSegments.Add(CreateBar(parent, "BottomRightV", new Vector2(0.5f, -0.5f), new Vector2(outlineSize, 0.5f), new Vector2(1f, 0f)));
    }

    private void BuildSatellites(RectTransform parent)
    {
        satellites.Clear();
        satellites.Add(CreateSatelliteCorner("SatelliteTopLeft", parent, new Vector2(-cornerDistance, cornerDistance), CornerOrientation.TopLeft));
        satellites.Add(CreateSatelliteCorner("SatelliteTopRight", parent, new Vector2(cornerDistance, cornerDistance), CornerOrientation.TopRight));
        satellites.Add(CreateSatelliteCorner("SatelliteBottomLeft", parent, new Vector2(-cornerDistance, -cornerDistance), CornerOrientation.BottomLeft));
        satellites.Add(CreateSatelliteCorner("SatelliteBottomRight", parent, new Vector2(cornerDistance, -cornerDistance), CornerOrientation.BottomRight));
    }

    private SatelliteCorner CreateSatelliteCorner(string name, RectTransform parent, Vector2 anchoredPosition, CornerOrientation orientation)
    {
        RectTransform root = CreateRect(name, parent, anchoredPosition, new Vector2(0.5f, 0.5f), Vector2.zero);

        float armLength = 12f;
        float armGap = outlineSize * 0.5f;
        Vector2 horizontalSize = new Vector2(armLength, outlineSize);
        Vector2 verticalSize = new Vector2(outlineSize, armLength);
        Vector2 horizontalPivot;
        Vector2 verticalPivot;
        Vector2 horizontalOffset;
        Vector2 verticalOffset;

        switch (orientation)
        {
            case CornerOrientation.TopLeft:
                horizontalPivot = new Vector2(0f, 0.5f);
                verticalPivot = new Vector2(0.5f, 1f);
                horizontalOffset = new Vector2(armGap, 0f);
                verticalOffset = new Vector2(0f, -armGap);
                break;
            case CornerOrientation.TopRight:
                horizontalPivot = new Vector2(1f, 0.5f);
                verticalPivot = new Vector2(0.5f, 1f);
                horizontalOffset = new Vector2(-armGap, 0f);
                verticalOffset = new Vector2(0f, -armGap);
                break;
            case CornerOrientation.BottomLeft:
                horizontalPivot = new Vector2(0f, 0.5f);
                verticalPivot = new Vector2(0.5f, 0f);
                horizontalOffset = new Vector2(armGap, 0f);
                verticalOffset = new Vector2(0f, armGap);
                break;
            default:
                horizontalPivot = new Vector2(1f, 0.5f);
                verticalPivot = new Vector2(0.5f, 0f);
                horizontalOffset = new Vector2(-armGap, 0f);
                verticalOffset = new Vector2(0f, armGap);
                break;
        }

        RawImage horizontal = CreateBarGraphic(root, $"{name}_H", horizontalSize, horizontalPivot);
        RawImage vertical = CreateBarGraphic(root, $"{name}_V", verticalSize, verticalPivot);
        RawImage joint = CreateBarGraphic(root, $"{name}_Joint", Vector2.one * outlineSize, new Vector2(0.5f, 0.5f));
        horizontal.rectTransform.anchoredPosition = horizontalOffset;
        vertical.rectTransform.anchoredPosition = verticalOffset;
        joint.rectTransform.anchoredPosition = Vector2.zero;
        return new SatelliteCorner(root, horizontal, vertical, joint);
    }

    private static RectTransform CreateRect(string name, Transform parent, Vector2 anchoredPosition, Vector2 pivot, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
        return rect;
    }

    private RawImage CreateFilledGraphic(RectTransform parent, Color color)
    {
        GameObject go = new GameObject("Graphic", typeof(RectTransform), typeof(RawImage));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        RawImage image = go.GetComponent<RawImage>();
        image.texture = GetCircleTexture();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static Texture2D GetCircleTexture()
    {
        if (circleTexture != null)
        {
            return circleTexture;
        }

        const int textureSize = 64;
        const float edgeSoftness = 1.5f;
        float radius = (textureSize - 2f) * 0.5f;
        Vector2 center = new Vector2((textureSize - 1f) * 0.5f, (textureSize - 1f) * 0.5f);
        Color[] pixels = new Color[textureSize * textureSize];

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.Clamp01(radius - distance + edgeSoftness);
                pixels[y * textureSize + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        circleTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
        {
            name = "CrosshairCircleDot",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };
        circleTexture.SetPixels(pixels);
        circleTexture.Apply(false, true);
        return circleTexture;
    }

    private static RectTransform CreateBar(RectTransform parent, string name, Vector2 anchor, Vector2 size, Vector2 pivot)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(RawImage));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = pivot;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        RawImage image = go.GetComponent<RawImage>();
        image.texture = Texture2D.whiteTexture;
        image.color = Color.white;
        image.raycastTarget = false;
        return rect;
    }

    private static RawImage CreateBarGraphic(RectTransform parent, string name, Vector2 size, Vector2 pivot)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(RawImage));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = pivot;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
        RawImage image = go.GetComponent<RawImage>();
        image.texture = Texture2D.whiteTexture;
        image.color = Color.white;
        image.raycastTarget = false;
        return image;
    }

    private HoverTarget DetectHoverTarget(Vector2 mousePosition)
    {
        if (EventSystem.current == null)
        {
            return HoverTarget.None;
        }

        if (pointerEventData == null)
        {
            pointerEventData = new PointerEventData(EventSystem.current);
        }

        pointerEventData.Reset();
        pointerEventData.position = mousePosition;
        raycastResults.Clear();
        EventSystem.current.RaycastAll(pointerEventData, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            GameObject go = result.gameObject;
            if (go == null)
            {
                continue;
            }

            if (!TryBuildHoverTarget(go, mousePosition, out HoverTarget target))
            {
                continue;
            }

            return target;
        }

        return HoverTarget.None;
    }

    private bool TryBuildHoverTarget(GameObject go, Vector2 mousePosition, out HoverTarget target)
    {
        target = HoverTarget.None;

        InteractionFeedback feedback = go.GetComponentInParent<InteractionFeedback>();
        if (feedback != null)
        {
            RectTransform rect = feedback.GetComponent<RectTransform>();
            if (rect != null)
            {
                Vector2 center = GetRectCenter(rect);
                Vector2 size = GetRectScreenSize(rect);
                if (size == Vector2.zero)
                {
                    size = new Vector2(outlineSpace, outlineSpace);
                }

                target = new HoverTarget(center, size, feedback.HoverMessage, feedback.IsActive, true, feedback.IsActive);
                return true;
            }
        }

        TMP_InputField inputField = go.GetComponentInParent<TMP_InputField>();
        if (inputField != null && inputField.interactable)
        {
            RectTransform rect = inputField.GetComponent<RectTransform>();
            Vector2 center = rect != null ? GetRectCenter(rect) : mousePosition;
            Vector2 size = rect != null ? GetRectScreenSize(rect) : new Vector2(outlineSpace, outlineSpace);
            if (size == Vector2.zero)
            {
                size = new Vector2(outlineSpace, outlineSpace);
            }

            bool typing = EventSystem.current.currentSelectedGameObject == inputField.gameObject;
            target = new HoverTarget(center, size, typing ? "Ввод текста" : defaultPrompt, typing, true, typing);
            return true;
        }

        Selectable selectable = go.GetComponentInParent<Selectable>();
        if (selectable != null && selectable.IsInteractable())
        {
            RectTransform rect = selectable.GetComponent<RectTransform>();
            Vector2 center = rect != null ? GetRectCenter(rect) : mousePosition;
            Vector2 size = rect != null ? GetRectScreenSize(rect) : new Vector2(outlineSpace, outlineSpace);
            if (size == Vector2.zero)
            {
                size = new Vector2(outlineSpace, outlineSpace);
            }

            string prompt = selectable is Button ? "Нажмите для взаимодействия" : defaultPrompt;
            target = new HoverTarget(center, size, prompt, false, true, false);
            return true;
        }

        return false;
    }

    private void AnimateTo(Vector2 mousePosition, Vector2 targetCenter, Vector2 targetSize, bool hovering, bool typing, bool active, bool gameplayLockedMode)
    {
        Vector2 centeredMouse = ScreenToCanvas(mousePosition);
        Vector2 centeredTarget = ScreenToCanvas(targetCenter);

        currentCenter = Vector2.Lerp(currentCenter, hovering ? centeredTarget : centeredMouse, hoverLerp);

        Vector2 paddedSize = hovering
            ? targetSize + GetMaxHoverPadding() * 2f
            : new Vector2(outlineSpace, outlineSpace);
        currentSize = Vector2.Lerp(currentSize == Vector2.zero ? paddedSize : currentSize, paddedSize, sizeLerp);

        dotRoot.anchoredPosition = centeredMouse;
        dotRoot.sizeDelta = Vector2.one * dotSize * (active ? 1.05f : hovering ? 1f : 0.9f);
        dotImage.color = active ? outlineColor : hovering ? outlineColor : dotColor;
        UpdateSatellites(mousePosition, targetCenter, targetSize, hovering, active, gameplayLockedMode);

        if (outlineRoot != null)
        {
            outlineRoot.anchoredPosition = currentCenter;
            outlineRoot.sizeDelta = currentSize;

            if (typing)
            {
                currentRotation = Mathf.LerpAngle(currentRotation, 0f, rotationLerp);
            }
            else if (hovering)
            {
                float nearest180 = Mathf.Round(currentRotation / 180f) * 180f;
                currentRotation = Mathf.LerpAngle(currentRotation, nearest180, rotationLerp);
            }
            else
            {
                currentRotation += idleSpinSpeed * Time.unscaledDeltaTime;
            }

            outlineRoot.localRotation = Quaternion.Euler(0f, 0f, currentRotation);

            float targetScale = hovering ? hoverScale : idleScale;
            if (active)
            {
                targetScale = 1f;
            }
            outlineRoot.localScale = Vector3.one * targetScale;

            UpdateOutlineVisualState(hovering, active);
        }
    }

    private void UpdateSatellites(Vector2 mousePosition, Vector2 targetCenter, Vector2 targetSize, bool hovering, bool active, bool gameplayLockedMode)
    {
        Vector2 mouseCanvas = ScreenToCanvas(mousePosition);
        Vector2 targetCanvas = ScreenToCanvas(targetCenter);
        Vector2 hoverOffset = targetCanvas - mouseCanvas;
        Vector2 halfSize = targetSize * 0.5f;
        float idleRadius = Mathf.Max(cornerDistance, outlineSize * 2f);
        if (!hovering && !gameplayLockedMode)
        {
            cornerOrbitRotation += cornerIdleOrbitSpeed * Time.unscaledDeltaTime;
        }

        Vector2[] baseOffsets =
        {
            new Vector2(-GetHoverRadius(halfSize.x), GetHoverRadius(halfSize.y)),
            new Vector2(GetHoverRadius(halfSize.x), GetHoverRadius(halfSize.y)),
            new Vector2(-GetHoverRadius(halfSize.x), -GetHoverRadius(halfSize.y)),
            new Vector2(GetHoverRadius(halfSize.x), -GetHoverRadius(halfSize.y))
        };

        Vector2[] hoverOffsets =
        {
            topLeftHoverPadding,
            topRightHoverPadding,
            bottomLeftHoverPadding,
            bottomRightHoverPadding
        };

        Vector2[] idleBaseOffsets =
        {
            new Vector2(-idleRadius, idleRadius),
            new Vector2(idleRadius, idleRadius),
            new Vector2(-idleRadius, -idleRadius),
            new Vector2(idleRadius, -idleRadius)
        };

        float orbitRotation = gameplayLockedMode ? 0f : cornerOrbitRotation;
        float orbitRadians = orbitRotation * Mathf.Deg2Rad;
        float orbitCos = Mathf.Cos(orbitRadians);
        float orbitSin = Mathf.Sin(orbitRadians);

        Vector2[] idleCorners =
        {
            RotateOffset(idleBaseOffsets[0], orbitCos, orbitSin),
            RotateOffset(idleBaseOffsets[1], orbitCos, orbitSin),
            RotateOffset(idleBaseOffsets[2], orbitCos, orbitSin),
            RotateOffset(idleBaseOffsets[3], orbitCos, orbitSin)
        };

        for (int i = 0; i < satellites.Count; i++)
        {
            SatelliteCorner corner = satellites[i];
            if (corner.Root == null)
            {
                continue;
            }

            Vector2 targetPosition;

            if (hovering)
            {
                targetPosition = hoverOffset + baseOffsets[i] + hoverOffsets[i] * cornerHoverDistanceMultiplier;
            }
            else
            {
                targetPosition = idleCorners[i];
            }

            if (hovering)
            {
                corner.Root.anchoredPosition = targetPosition;
                corner.Root.localRotation = Quaternion.Lerp(corner.Root.localRotation, Quaternion.identity, 0.18f);
                corner.Root.localScale = Vector3.one * (active ? cornerActiveScale : cornerHoverScale);
            }
            else
            {
                corner.Root.anchoredPosition = Vector2.Lerp(corner.Root.anchoredPosition, targetPosition, 0.18f);
                corner.Root.localRotation = gameplayLockedMode
                    ? Quaternion.identity
                    : Quaternion.Euler(0f, 0f, orbitRotation);
                corner.Root.localScale = Vector3.one * cornerIdleScale;
            }

            Color color = active ? outlineColor : hovering ? outlineColor : dotColor;
            if (corner.Horizontal != null)
            {
                corner.Horizontal.color = color;
            }

            if (corner.Vertical != null)
            {
                corner.Vertical.color = color;
            }

            if (corner.Joint != null)
            {
                corner.Joint.color = color;
            }
        }
    }

    private void UpdateOutlineVisualState(bool hovering, bool active)
    {
        if (style == CursorStyle.Full)
        {
            foreach (RectTransform segment in outlineSegments)
            {
                if (segment != null)
                {
                    RawImage img = segment.GetComponent<RawImage>();
                    if (img != null)
                    {
                        img.color = active ? outlineColor : outlineColor;
                    }
                }
            }

            return;
        }

        float cornerScale = hovering || active ? 1f : 0.8f;
        foreach (RectTransform segment in outlineSegments)
        {
            if (segment != null)
            {
                segment.localScale = Vector3.one * cornerScale;
                RawImage img = segment.GetComponent<RawImage>();
                if (img != null)
                {
                    img.color = active ? outlineColor : outlineColor;
                }
            }
        }
    }

    private static Vector2 RotateOffset(Vector2 offset, float cos, float sin)
    {
        return new Vector2(
            offset.x * cos - offset.y * sin,
            offset.x * sin + offset.y * cos);
    }

    private Vector2 GetMaxHoverPadding()
    {
        return new Vector2(
            Mathf.Max(Mathf.Abs(topLeftHoverPadding.x), Mathf.Abs(topRightHoverPadding.x), Mathf.Abs(bottomLeftHoverPadding.x), Mathf.Abs(bottomRightHoverPadding.x)),
            Mathf.Max(Mathf.Abs(topLeftHoverPadding.y), Mathf.Abs(topRightHoverPadding.y), Mathf.Abs(bottomLeftHoverPadding.y), Mathf.Abs(bottomRightHoverPadding.y)));
    }

    private float GetHoverRadius(float halfSize)
    {
        return Mathf.Max(outlineSpace * 0.55f, halfSize + cornerHoverGap);
    }

    private Vector2 ScreenToCanvas(Vector2 screenPoint)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPoint, null, out Vector2 localPoint);
        return localPoint;
    }

    private Vector2 GetRectCenter(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
        return (min + max) * 0.5f;
    }

    private Vector2 GetRectScreenSize(RectTransform rect)
    {
        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);
        Vector2 min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
        return new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
    }

    private static bool TryGetWorldBounds(GameObject targetObject, out Bounds bounds)
    {
        bounds = new Bounds(targetObject.transform.position, Vector3.zero);
        bool hasBounds = false;

        Renderer[] renderers = targetObject.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer renderer in renderers)
        {
            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        Collider[] colliders = targetObject.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            if (!hasBounds)
            {
                bounds = collider.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(collider.bounds);
            }
        }

        return hasBounds;
    }

    private Vector2 ProjectBoundsToScreenSize(Bounds bounds, Camera camera)
    {
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

        Vector2 minScreen = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 maxScreen = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        bool any = false;

        foreach (Vector3 corner in corners)
        {
            Vector3 screen = camera.WorldToScreenPoint(corner);
            if (screen.z < 0f)
            {
                continue;
            }

            any = true;
            minScreen = Vector2.Min(minScreen, new Vector2(screen.x, screen.y));
            maxScreen = Vector2.Max(maxScreen, new Vector2(screen.x, screen.y));
        }

        if (!any)
        {
            return Vector2.zero;
        }

        return maxScreen - minScreen;
    }

    private Vector2 GetMousePosition()
    {
        if (realSceneMode && Cursor.lockState == CursorLockMode.Locked)
        {
            return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        }

        Mouse mouse = Mouse.current;
        if (mouse != null)
        {
            return mouse.position.ReadValue();
        }

        return new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    private readonly struct HoverTarget
    {
        public static readonly HoverTarget None = new HoverTarget(Vector2.zero, Vector2.zero, "", false, false, false);
        public readonly Vector2 Center;
        public readonly Vector2 Size;
        public readonly string Prompt;
        public readonly bool IsActive;
        public readonly bool IsHovering;
        public readonly bool IsTyping;

        public bool ShouldShowPrompt => IsHovering || IsTyping;

        public HoverTarget(Vector2 center, Vector2 size, string prompt, bool active, bool hovering, bool typing)
        {
            Center = center;
            Size = size;
            Prompt = prompt;
            IsActive = active;
            IsHovering = hovering;
            IsTyping = typing;
        }
    }

    private readonly struct SatelliteCorner
    {
        public readonly RectTransform Root;
        public readonly RawImage Horizontal;
        public readonly RawImage Vertical;
        public readonly RawImage Joint;

        public SatelliteCorner(RectTransform root, RawImage horizontal, RawImage vertical, RawImage joint)
        {
            Root = root;
            Horizontal = horizontal;
            Vertical = vertical;
            Joint = joint;
        }
    }

    private enum CornerOrientation
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
}
