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
    [SerializeField] private bool openOnStart = true;
    [SerializeField] private bool pauseGameWhileOpen = false;
    [SerializeField] private bool unlockCursorWhileOpen = true;
    [SerializeField] private bool closeWithEscape = true;

    [Header("Video")]
    [SerializeField] private Vector2Int renderTextureSize = new Vector2Int(1920, 1080);
    [SerializeField] private bool loopVideo = false;
    [SerializeField] private bool muteVideo = true;
    [SerializeField] private bool useManualVideoClockWhenStalled = true;
    [SerializeField] private VideoAspectRatio aspectRatio = VideoAspectRatio.FitInside;

    [Header("Canvas")]
    [SerializeField] private int sortingOrder = 5000;

    private RenderTexture videoTexture;
    private bool isOpen;
    private bool videoPausedByUser;
    private bool usingManualVideoClock;
    private double manualVideoTime;
    private double lastObservedVideoTime;
    private long manualVideoFrame;
    private long lastObservedVideoFrame = -1;
    private double manualFrameAccumulator;
    private float stalledPlaybackSeconds;
    private float previousTimeScale = 1f;
    private CursorLockMode previousLockState;

    private void Awake()
    {
        ResolveBindings();
        ApplyStaticText();
        BindButtons();

        if (firstPersonController == null)
        {
            firstPersonController = FindAnyObjectByType<FirstPersonController>();
        }

        SetMenuVisible(false);
    }

    private void OnEnable()
    {
        MenuVisibilityCoordinator.MenuOpening += HandleOtherMenuOpening;
        BindButtons();
    }

    private void OnDisable()
    {
        MenuVisibilityCoordinator.MenuOpening -= HandleOtherMenuOpening;
    }

    private void Start()
    {
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

        UpdateVideoPlayback();
    }

    private void OnDestroy()
    {
        CleanupVideo();
    }

    public void SetFirstPersonController(FirstPersonController controller)
    {
        firstPersonController = controller;
    }

    public void SetVideoFileName(string fileName)
    {
        videoFileName = NormalizeVideoFileName(fileName);
        if (isOpen)
        {
            PrepareVideo();
        }
    }

    public void SetEditorVideoClip(VideoClip clip)
    {
        editorVideoClip = clip;
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
            firstPersonController.SetMoveControl(false);
            firstPersonController.DisableAllMovement();
        }

        SetMenuVisible(true);
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, true);
        isOpen = true;
        PrepareVideo();
    }

    public void CloseMenu()
    {
        if (!isOpen)
        {
            return;
        }

        CleanupVideo();
        SetMenuVisible(false);

        if (firstPersonController != null)
        {
            firstPersonController.EnableAllMovement();
            firstPersonController.SetMoveControl(true);
        }

        if (pauseGameWhileOpen)
        {
            Time.timeScale = previousTimeScale;
        }

        Cursor.lockState = previousLockState;
        Cursor.visible = false;

        MenuVisibilityCoordinator.SetMenuOpen(MenuId, false);
        isOpen = false;
    }

    private void HandleOtherMenuOpening(string menuId)
    {
        if (menuId != MenuId && isOpen)
        {
            CloseMenu();
        }
    }

    private void ResolveBindings()
    {
        if (menuRoot == null)
        {
            Transform found = transform.Find("MenuRoot");
            menuRoot = found != null ? found.gameObject : gameObject;
        }

        if (canvas == null)
        {
            canvas = GetComponentInChildren<Canvas>(true);
        }

        if (canvas != null)
        {
            canvas.sortingOrder = sortingOrder;
        }

        if (videoSurface == null)
        {
            videoSurface = FindChildComponent<RawImage>("VideoSurface");
        }

        if (titleLabel == null)
        {
            titleLabel = FindChildText("Title");
        }

        if (descriptionLabel == null)
        {
            descriptionLabel = FindChildText("Description");
        }

        if (statusLabel == null)
        {
            statusLabel = FindChildText("Status");
        }

        if (closeButton == null)
        {
            closeButton = FindChildComponent<Button>("CloseButton");
        }

        if (pauseButton == null)
        {
            pauseButton = FindChildComponent<Button>("PauseButton");
        }

        if (canvas == null || videoSurface == null || closeButton == null)
        {
            CreateFallbackVisual();
        }

        if (videoPlayer == null)
        {
            videoPlayer = GetComponentInChildren<VideoPlayer>(true);
        }

        if (videoPlayer == null)
        {
            GameObject playerObject = new GameObject("IntroVideoPlayer", typeof(VideoPlayer));
            playerObject.transform.SetParent(transform, false);
            videoPlayer = playerObject.GetComponent<VideoPlayer>();
        }
    }

    private void CreateFallbackVisual()
    {
        GameObject fallbackRoot = new GameObject("MenuRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        fallbackRoot.transform.SetParent(transform, false);
        menuRoot = fallbackRoot;

        RectTransform rootRect = fallbackRoot.GetComponent<RectTransform>();
        SetRect(rootRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));

        canvas = fallbackRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

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
        RectTransform surfaceRect = surfaceObject.GetComponent<RectTransform>();
        SetRect(surfaceRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        surfaceRect.offsetMin = new Vector2(16f, 16f);
        surfaceRect.offsetMax = new Vector2(-16f, -16f);
        videoSurface = surfaceObject.GetComponent<RawImage>();
        videoSurface.color = Color.black;

        AspectRatioFitter fitter = surfaceObject.GetComponent<AspectRatioFitter>();
        fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        fitter.aspectRatio = 16f / 9f;

        statusLabel = CreateText(videoFrame.transform, "Status", "Загрузка видео...", 22f, FontStyles.Italic, TextAlignmentOptions.Center, new Color(0.72f, 0.80f, 0.90f, 1f));
        SetRect(statusLabel.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));

        pauseButton = CreateButton(panel.transform, "PauseButton", pauseButtonText, new Vector2(220f, 58f), new Vector2(-130f, -322f));
        closeButton = CreateButton(panel.transform, "CloseButton", closeButtonText, new Vector2(220f, 58f), new Vector2(130f, -322f));
    }

    private void ApplyStaticText()
    {
        SetText(titleLabel, titleText);
        SetText(descriptionLabel, descriptionText);

        if (closeButton != null)
        {
            TMP_Text closeLabel = closeButton.GetComponentInChildren<TMP_Text>(true);
            SetText(closeLabel, closeButtonText);
        }

        UpdatePauseButtonText();
    }

    private void BindButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseMenu);
            closeButton.onClick.AddListener(CloseMenu);
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(ToggleVideoPause);
            pauseButton.onClick.AddListener(ToggleVideoPause);
        }
    }

    private void PrepareVideo()
    {
        CleanupVideo();

        if (videoSurface == null || videoPlayer == null)
        {
            ShowStatus("Видео-поверхность не назначена");
            return;
        }

        int width = Mathf.Max(16, renderTextureSize.x);
        int height = Mathf.Max(16, renderTextureSize.y);
        videoTexture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32)
        {
            name = "LabIntroMenuVideoTexture"
        };
        videoTexture.Create();

        videoSurface.texture = videoTexture;
        videoSurface.color = Color.white;

        videoPlayer.Stop();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = videoTexture;
        videoPlayer.isLooping = loopVideo;
        videoPlayer.aspectRatio = aspectRatio;
        videoPlayer.skipOnDrop = true;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.playbackSpeed = 1f;
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
        videoPlayer.sendFrameReadyEvents = true;

        ConfigureAudio();
        videoPlayer.prepareCompleted -= HandleVideoPrepared;
        videoPlayer.errorReceived -= HandleVideoError;
        videoPlayer.prepareCompleted += HandleVideoPrepared;
        videoPlayer.errorReceived += HandleVideoError;

        if (editorVideoClip != null)
        {
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = editorVideoClip;
        }
        else
        {
            string resolvedPath = ResolveVideoPath(videoFileName);
            if (string.IsNullOrWhiteSpace(resolvedPath))
            {
                ShowStatus("Видео не найдено");
                return;
            }

            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = resolvedPath;
        }

        ShowStatus("Загрузка видео...");
        videoPausedByUser = false;
        usingManualVideoClock = false;
        manualVideoTime = 0d;
        lastObservedVideoTime = 0d;
        manualVideoFrame = 0;
        lastObservedVideoFrame = -1;
        manualFrameAccumulator = 0d;
        stalledPlaybackSeconds = 0f;
        UpdatePauseButtonText();
        videoPlayer.Prepare();
    }

    private void ConfigureAudio()
    {
        if (videoPlayer == null)
        {
            return;
        }

        if (muteVideo)
        {
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            return;
        }

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        AudioSource audioSource = videoPlayer.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = videoPlayer.gameObject.AddComponent<AudioSource>();
        }

        audioSource.mute = false;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        videoPlayer.SetTargetAudioSource(0, audioSource);
    }

    private void HandleVideoPrepared(VideoPlayer source)
    {
        HideStatus();
        manualVideoTime = source != null ? source.time : 0d;
        lastObservedVideoTime = manualVideoTime;
        manualVideoFrame = source != null && source.frame >= 0 ? source.frame : 0;
        lastObservedVideoFrame = manualVideoFrame;
        manualFrameAccumulator = manualVideoFrame;
        stalledPlaybackSeconds = 0f;

        if (source != null && !videoPausedByUser)
        {
            source.Play();
        }
    }

    private void UpdateVideoPlayback()
    {
        if (videoPlayer == null || !videoPlayer.isPrepared)
        {
            return;
        }

        if (videoPausedByUser)
        {
            HoldVideoPauseFrame();
            return;
        }

        if (IsVideoAtEnd())
        {
            return;
        }

        if (!videoPlayer.isPlaying)
        {
            videoPlayer.Play();
        }

        if (!useManualVideoClockWhenStalled)
        {
            return;
        }

        double currentTime = videoPlayer.time;
        long currentFrame = videoPlayer.frame;
        bool hasFrameInfo = videoPlayer.frameCount > 0;
        bool timeAdvanced = Mathf.Abs((float)(currentTime - lastObservedVideoTime)) > 0.0001f;
        bool frameAdvanced = currentFrame >= 0 && currentFrame != lastObservedVideoFrame;
        bool visualAdvanced = hasFrameInfo ? frameAdvanced : timeAdvanced;

        if (!usingManualVideoClock && visualAdvanced)
        {
            lastObservedVideoTime = currentTime;
            if (currentFrame >= 0)
            {
                lastObservedVideoFrame = currentFrame;
                manualVideoFrame = currentFrame;
                manualFrameAccumulator = currentFrame;
            }

            stalledPlaybackSeconds = 0f;
            return;
        }

        stalledPlaybackSeconds += Time.unscaledDeltaTime;
        if (!usingManualVideoClock && stalledPlaybackSeconds < 0.35f)
        {
            return;
        }

        usingManualVideoClock = true;
        AdvanceManualVideoClock(currentTime);
    }

    private void AdvanceManualVideoClock(double currentTime)
    {
        double frameRate = videoPlayer.frameRate > 0d ? videoPlayer.frameRate : 30d;
        long frameCount = videoPlayer.frameCount > 0 ? (long)videoPlayer.frameCount : 0;

        manualFrameAccumulator = Math.Max(manualFrameAccumulator, manualVideoFrame) + Time.unscaledDeltaTime * frameRate;
        manualVideoFrame = Math.Max(0, (long)manualFrameAccumulator);
        manualVideoTime = manualVideoFrame / frameRate;

        if (frameCount > 0 && manualVideoFrame >= frameCount)
        {
            if (videoPlayer.isLooping)
            {
                manualVideoFrame = 0;
                manualFrameAccumulator = 0d;
                manualVideoTime = 0d;
            }
            else
            {
                manualVideoFrame = frameCount - 1;
                manualFrameAccumulator = manualVideoFrame;
                manualVideoTime = videoPlayer.length > 0d ? videoPlayer.length : manualVideoFrame / frameRate;
            }
        }

        if (videoPlayer.length > 0d && manualVideoTime >= videoPlayer.length)
        {
            if (videoPlayer.isLooping)
            {
                manualVideoTime = 0d;
                manualVideoFrame = 0;
                manualFrameAccumulator = 0d;
            }
            else
            {
                manualVideoTime = videoPlayer.length;
            }
        }

        if (frameCount > 0)
        {
            videoPlayer.frame = manualVideoFrame;
        }
        else
        {
            videoPlayer.time = Math.Max(manualVideoTime, currentTime + Time.unscaledDeltaTime);
        }

        videoPlayer.Play();
        lastObservedVideoTime = videoPlayer.time;
        lastObservedVideoFrame = videoPlayer.frame >= 0 ? videoPlayer.frame : manualVideoFrame;
    }

    private void ToggleVideoPause()
    {
        if (videoPlayer == null)
        {
            return;
        }

        if (!videoPlayer.isPrepared)
        {
            videoPausedByUser = !videoPausedByUser;
            UpdatePauseButtonText();
            return;
        }

        if (videoPausedByUser || !videoPlayer.isPlaying)
        {
            videoPausedByUser = false;
            videoPlayer.playbackSpeed = 1f;
            videoPlayer.time = manualVideoTime;
            lastObservedVideoTime = manualVideoTime;
            lastObservedVideoFrame = videoPlayer.frame >= 0 ? videoPlayer.frame : manualVideoFrame;
            manualFrameAccumulator = manualVideoFrame;
            stalledPlaybackSeconds = 0f;
            videoPlayer.Play();
        }
        else
        {
            videoPausedByUser = true;
            manualVideoTime = videoPlayer.time;
            manualVideoFrame = videoPlayer.frame >= 0 ? videoPlayer.frame : manualVideoFrame;
            manualFrameAccumulator = manualVideoFrame;
            lastObservedVideoTime = manualVideoTime;
            lastObservedVideoFrame = manualVideoFrame;
            stalledPlaybackSeconds = 0f;
            videoPlayer.playbackSpeed = 0f;
            videoPlayer.Pause();
        }

        UpdatePauseButtonText();
    }

    private void HoldVideoPauseFrame()
    {
        if (videoPlayer == null)
        {
            return;
        }

        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
        }

        videoPlayer.playbackSpeed = 0f;
        if (manualVideoFrame >= 0 && videoPlayer.frameCount > 0)
        {
            videoPlayer.frame = manualVideoFrame;
        }
        else
        {
            videoPlayer.time = manualVideoTime;
        }

        lastObservedVideoTime = manualVideoTime;
        lastObservedVideoFrame = manualVideoFrame;
        stalledPlaybackSeconds = 0f;
    }

    private void HandleVideoError(VideoPlayer source, string message)
    {
        ShowStatus("Видео не удалось загрузить");
        Debug.LogError($"LabIntroMenu: video error: {message}", this);
    }

    private void CleanupVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= HandleVideoPrepared;
            videoPlayer.errorReceived -= HandleVideoError;
            videoPlayer.playbackSpeed = 1f;
            videoPlayer.Stop();
            videoPlayer.targetTexture = null;
        }

        if (videoSurface != null)
        {
            videoSurface.texture = null;
        }

        videoPausedByUser = false;
        usingManualVideoClock = false;
        manualVideoTime = 0d;
        lastObservedVideoTime = 0d;
        manualVideoFrame = 0;
        lastObservedVideoFrame = -1;
        manualFrameAccumulator = 0d;
        stalledPlaybackSeconds = 0f;
        UpdatePauseButtonText();

        if (videoTexture != null)
        {
            if (videoTexture.IsCreated())
            {
                videoTexture.Release();
            }

            Destroy(videoTexture);
            videoTexture = null;
        }
    }

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

        TMP_Text label = pauseButton.GetComponentInChildren<TMP_Text>(true);
        SetText(label, videoPausedByUser ? resumeButtonText : pauseButtonText);
    }

    private bool IsVideoAtEnd()
    {
        return videoPlayer != null &&
            !videoPlayer.isLooping &&
            videoPlayer.frameCount > 0 &&
            videoPlayer.frame >= (long)videoPlayer.frameCount - 1;
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

    private TMP_Text FindChildText(string objectName)
    {
        Transform child = FindChildRecursive(transform, objectName);
        return child != null ? child.GetComponent<TMP_Text>() : null;
    }

    private static Transform FindChildRecursive(Transform parent, string objectName)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.name == objectName)
            {
                return child;
            }

            Transform found = FindChildRecursive(child, objectName);
            if (found != null)
            {
                return found;
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

    private static GameObject CreateImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        SetRect(imageObject.GetComponent<RectTransform>(), anchorMin, anchorMax, size, anchoredPosition, new Vector2(0.5f, 0.5f));
        imageObject.GetComponent<Image>().color = color;
        return imageObject;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.Normal;

        if (TMP_Settings.defaultFontAsset != null)
        {
            label.font = TMP_Settings.defaultFontAsset;
        }

        return label;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 size, Vector2 anchoredPosition)
    {
        GameObject buttonObject = CreateImage(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, anchoredPosition, new Color(0.12f, 0.46f, 0.82f, 1f));
        Button button = buttonObject.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.12f, 0.46f, 0.82f, 1f);
        colors.highlightedColor = new Color(0.22f, 0.58f, 0.94f, 1f);
        colors.pressedColor = new Color(0.08f, 0.30f, 0.58f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        TMP_Text labelText = CreateText(buttonObject.transform, "Label", label, 22f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        SetRect(labelText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        labelText.rectTransform.offsetMin = Vector2.zero;
        labelText.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition, Vector2 pivot)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
    }

    private static string ResolveVideoPath(string fileName)
    {
        string normalized = NormalizeVideoFileName(fileName);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        string streamingAssetsFile = Path.Combine(Application.streamingAssetsPath, normalized).Replace('\\', '/');
        if (!File.Exists(streamingAssetsFile))
        {
            return string.Empty;
        }

        return new Uri(streamingAssetsFile).AbsoluteUri;
    }

    private static string NormalizeVideoFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return "LabIntro.mp4";
        }

        string normalized = Path.GetFileName(fileName.Trim());
        return string.IsNullOrWhiteSpace(normalized) ? "LabIntro.mp4" : normalized;
    }
}
