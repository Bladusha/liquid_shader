using System;
using System.IO;
using EasyPeasyFirstPersonController;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using UnityEngine.Video;

public class LabIntroMenuController : MonoBehaviour
{
    private const string MenuId = "lab_intro";

    [Header("Content")]
    [SerializeField] private string videoFileName = "LabIntro.mp4";
    [SerializeField] private VideoClip editorVideoClip;
    [SerializeField, TextArea(3, 6)] private string descriptionText = "Посмотрите короткую инструкцию перед началом работы.";
    [SerializeField] private string closeButtonText = "Закрыть";
    [SerializeField] private string pauseButtonText = "Пауза";
    [SerializeField] private string resumeButtonText = "Продолжить";
    [SerializeField] private string titleText = "Вводный гайд";

    [Header("Prefab Bindings")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private Canvas canvas;
    [SerializeField] private RawImage videoSurface;
    [SerializeField] private TMP_Text titleLabel;
    [SerializeField] private TMP_Text descriptionLabel;
    [SerializeField] private TMP_Text statusLabel;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Player")]
    [SerializeField] private FirstPersonController firstPersonController;

    [Header("Behavior")]
    [SerializeField] private bool openOnStart;
    [SerializeField] private bool pauseGameWhileOpen = true;
    [SerializeField] private bool unlockCursorWhileOpen = true;
    [SerializeField] private bool closeWithEscape = true;

    private LabIntroVideoController videoController;
    private bool isOpen;
    private bool bindingsResolved;
    private float previousTimeScale;
    private CursorLockMode previousLockState;

    private void Awake()
    {
        ResolveBindings();
        SetMenuVisible(false);
    }

    private void Start()
    {
        EnsureVideoController();
        if (openOnStart)
        {
            OpenMenu();
        }
    }

    private void Update()
    {
        if (!isOpen)
        {
            return;
        }

        if (InputSystemCompat.GetKeyDown(KeyCode.Tab) || (closeWithEscape && InputSystemCompat.GetKeyDown(KeyCode.Escape)))
        {
            if (InputSystemCompat.GetKeyDown(KeyCode.Tab))
            {
                MenuVisibilityCoordinator.MarkTabHandled();
            }
            CloseMenu();
        }

        if (videoController != null)
        {
            videoController.UpdatePlayback();
        }
    }

    public void SetFirstPersonController(FirstPersonController controller)
    {
        firstPersonController = controller;
    }

    public void SetVideoFileName(string fileName)
    {
        videoFileName = LabIntroVideoController.NormalizeVideoFileName(fileName);
        if (videoController != null)
        {
            videoController.SetVideoFileName(videoFileName);
        }
        if (isOpen)
        {
            PrepareVideo();
        }
    }

    public void SetEditorVideoClip(VideoClip clip)
    {
        editorVideoClip = clip;
        if (videoController != null)
        {
            videoController.SetEditorVideoClip(clip);
        }
        if (isOpen)
        {
            PrepareVideo();
        }
    }

    public void OpenMenu()
    {
        if (isOpen)
        {
            return;
        }

        ResolveBindings();
        ApplyStaticText();
        EnsureEventSystem();

        previousTimeScale = Time.timeScale;
        previousLockState = Cursor.lockState;

        if (pauseGameWhileOpen)
        {
            Time.timeScale = 0f;
        }

        if (unlockCursorWhileOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }

        if (firstPersonController != null)
        {
            firstPersonController.DisableAllMovement();
        }

        isOpen = true;
        SetMenuVisible(true);
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, true);
        PrepareVideo();
    }

    public void CloseMenu()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;
        CleanupVideo();

        if (pauseGameWhileOpen)
        {
            Time.timeScale = previousTimeScale;
        }

        if (unlockCursorWhileOpen)
        {
            Cursor.lockState = previousLockState;
            Cursor.visible = previousLockState == CursorLockMode.Locked;
        }

        if (firstPersonController != null)
        {
            firstPersonController.EnableAllMovement();
        }

        SetMenuVisible(false);
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, false);
    }

    // --- Video delegation ---

    private void EnsureVideoController()
    {
        if (videoController != null)
        {
            return;
        }

        videoController = gameObject.AddComponent<LabIntroVideoController>();
        videoController.SetVideoFileName(videoFileName);
        videoController.SetEditorVideoClip(editorVideoClip);

        if (videoPlayer == null)
        {
            GameObject playerObject = new GameObject("VideoPlayer", typeof(VideoPlayer));
            playerObject.transform.SetParent(transform, false);
            videoPlayer = playerObject.GetComponent<VideoPlayer>();
        }

        videoController.Initialize(videoPlayer);
    }

    private void PrepareVideo()
    {
        if (videoPlayer == null)
        {
            return;
        }

        EnsureVideoController();
        HideStatus();

        if (videoSurface != null)
        {
            videoSurface.texture = null;
        }

        videoController.Prepare(() =>
        {
            if (videoSurface != null && videoPlayer.texture != null)
            {
                videoSurface.texture = videoPlayer.texture;
            }
            videoController.Play();
            UpdatePauseButtonText();
        });

        videoController.ErrorOccurred += OnVideoError;
        videoController.PlaybackFinished += OnVideoFinished;
    }

    private void CleanupVideo()
    {
        if (videoController != null)
        {
            videoController.ErrorOccurred -= OnVideoError;
            videoController.PlaybackFinished -= OnVideoFinished;
            videoController.Cleanup();
        }

        if (videoSurface != null)
        {
            videoSurface.texture = null;
        }
    }

    private void OnVideoError(string message)
    {
        ShowStatus("Ошибка воспроизведения: " + message);
    }

    private void OnVideoFinished()
    {
        ShowStatus("Инструкция завершена");
    }

    // --- UI bindings ---

    private void ResolveBindings()
    {
        if (bindingsResolved)
        {
            return;
        }

        if (menuRoot == null)
        {
            menuRoot = gameObject;
        }

        if (canvas == null)
        {
            canvas = GetComponentInParent<Canvas>();
        }

        if (videoSurface == null)
        {
            videoSurface = FindChildComponent<RawImage>("VideoSurface");
        }

        if (titleLabel == null)
        {
            titleLabel = FindChildComponent<TMP_Text>("Title");
        }

        if (descriptionLabel == null)
        {
            descriptionLabel = FindChildComponent<TMP_Text>("Description");
        }

        if (statusLabel == null)
        {
            statusLabel = FindChildComponent<TMP_Text>("Status");
        }

        if (closeButton == null)
        {
            closeButton = FindChildComponent<Button>("CloseButton");
        }

        if (pauseButton == null)
        {
            pauseButton = FindChildComponent<Button>("PauseButton");
        }

        if (videoPlayer == null)
        {
            videoPlayer = GetComponentInChildren<VideoPlayer>(true);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseMenu);
            closeButton.onClick.AddListener(CloseMenu);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(HandlePauseToggle);
            pauseButton.onClick.AddListener(HandlePauseToggle);
        }

        bindingsResolved = true;
    }

    private void ApplyStaticText()
    {
        SetText(titleLabel, titleText);
        SetText(descriptionLabel, descriptionText);
        SetButtonText(closeButton, closeButtonText);
        UpdatePauseButtonText();
        HideStatus();
    }

    private void HandlePauseToggle()
    {
        if (videoController == null)
        {
            return;
        }

        videoController.TogglePause();
        UpdatePauseButtonText();
    }

    // --- Fallback UI generation ---

    public void BuildFallbackUI()
    {
        if (menuRoot != null && menuRoot != gameObject)
        {
            return;
        }

        GameObject fallbackRoot = new GameObject("LabIntroMenuRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        fallbackRoot.transform.SetParent(transform, false);

        Canvas fallbackCanvas = fallbackRoot.GetComponent<Canvas>();
        fallbackCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fallbackCanvas.sortingOrder = 9000;

        CanvasScaler scaler = fallbackRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        CreateImage(fallbackRoot.transform, "Backdrop", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Color(0f, 0f, 0f, 0.72f));

        GameObject panel = CreateImage(fallbackRoot.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1120f, 720f), Vector2.zero, new Color(0.07f, 0.09f, 0.13f, 0.98f));

        titleLabel = CreateText(panel.transform, "Title", titleText, 42f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        SetRect(titleLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(880f, 56f), new Vector2(0f, -38f), new Vector2(0.5f, 1f));

        descriptionLabel = CreateText(panel.transform, "Description", descriptionText, 23f, FontStyles.Normal, TextAlignmentOptions.Center, new Color(0.72f, 0.80f, 0.90f, 1f));
        SetRect(descriptionLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(880f, 64f), new Vector2(0f, -92f), new Vector2(0.5f, 1f));

        GameObject videoFrame = CreateImage(panel.transform, "VideoFrame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1000f, 470f), new Vector2(0f, -10f), Color.black);

        GameObject surfaceObject = new GameObject("VideoSurface", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(AspectRatioFitter));
        surfaceObject.transform.SetParent(videoFrame.transform, false);
        RawImage surface = surfaceObject.GetComponent<RawImage>();
        surface.color = Color.white;
        AspectRatioFitter fitter = surfaceObject.GetComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        SetRect(surfaceObject.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        videoSurface = surface;

        GameObject playerObject = new GameObject("VideoPlayer", typeof(VideoPlayer));
        playerObject.transform.SetParent(transform, false);
        videoPlayer = playerObject.GetComponent<VideoPlayer>();

        statusLabel = CreateText(panel.transform, "Status", string.Empty, 20f, FontStyles.Normal, TextAlignmentOptions.Center, new Color(0.70f, 0.78f, 0.88f, 1f));
        SetRect(statusLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(880f, 44f), new Vector2(0f, 56f), new Vector2(0.5f, 0f));
        statusLabel.gameObject.SetActive(false);

        pauseButton = CreateButton(panel.transform, "PauseButton", pauseButtonText, new Vector2(0.35f, 0f), new Vector2(0.35f, 0f), new Vector2(220f, 52f), new Vector2(0f, 42f));
        closeButton = CreateButton(panel.transform, "CloseButton", closeButtonText, new Vector2(0.65f, 0f), new Vector2(0.65f, 0f), new Vector2(220f, 52f), new Vector2(0f, 42f));

        menuRoot = fallbackRoot;
        canvas = fallbackCanvas;
        bindingsResolved = false;
    }

    private Button CreateButton(Transform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition)
    {
        GameObject buttonObject = CreateImage(parent, name, anchorMin, anchorMax, size, anchoredPosition, new Color(0.12f, 0.46f, 0.82f, 1f));
        Button button = buttonObject.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.18f, 0.54f, 0.90f, 1f);
        colors.pressedColor = new Color(0.08f, 0.36f, 0.70f, 1f);
        button.colors = colors;

        TMP_Text text = CreateText(buttonObject.transform, "Label", label, 22f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        SetRect(text.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));

        return button;
    }

    private GameObject CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(parent, false);
        Image image = obj.GetComponent<Image>();
        image.color = color;
        SetRect(obj.GetComponent<RectTransform>(), anchorMin, anchorMax, size, anchoredPosition, new Vector2(0.5f, 0.5f));
        return obj;
    }

    private TMP_Text CreateText(Transform parent, string name, string content, float fontSize, FontStyles style, TextAlignmentOptions alignment, Color color)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        TMP_Text text = obj.GetComponent<TMP_Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        return text;
    }

    // --- UI helpers ---

    private void SetMenuVisible(bool visible)
    {
        if (menuRoot != null)
        {
            menuRoot.SetActive(visible);
        }
    }

    private void ShowStatus(string text)
    {
        if (statusLabel != null)
        {
            statusLabel.gameObject.SetActive(true);
            statusLabel.text = text;
        }
    }

    private void HideStatus()
    {
        if (statusLabel != null)
        {
            statusLabel.gameObject.SetActive(false);
        }
    }

    private void UpdatePauseButtonText()
    {
        if (pauseButton == null)
        {
            return;
        }

        bool paused = videoController != null && videoController.IsPaused;
        TMP_Text label = pauseButton.GetComponentInChildren<TMP_Text>(true);
        SetText(label, paused ? resumeButtonText : pauseButtonText);
    }

    private void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    private T FindChildComponent<T>(string objectName) where T : Component
    {
        Transform child = FindChildRecursive(transform, objectName);
        return child != null ? child.GetComponent<T>() : null;
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform result = FindChildRecursive(parent.GetChild(i), name);
            if (result != null)
            {
                return result;
            }
        }

        return null;
    }

    private static void SetText(TMP_Text label, string value)
    {
        if (label != null)
        {
            label.text = value;
        }
    }

    private static void SetButtonText(Button button, string text)
    {
        if (button == null)
        {
            return;
        }

        TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
        SetText(label, text);
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition, Vector2 pivot)
    {
        if (rect == null)
        {
            return;
        }

        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
    }
}
