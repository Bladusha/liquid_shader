using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public static class LabIntroMenuPrefabBuilder
{
    private const string PrefabPath = "Assets/_Project/Lab/Prefabs/LabIntroMenu.prefab";

    [MenuItem("Tools/LiquidShader/Create Lab Intro Menu Prefab")]
    public static void CreatePrefab()
    {
        GameObject root = BuildHierarchy();
        GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Selection.activeObject = savedPrefab;
        Debug.Log($"Lab intro menu prefab created: {PrefabPath}");
    }

    private static GameObject BuildHierarchy()
    {
        Color backdropColor = new Color(0.01f, 0.015f, 0.025f, 0.78f);
        Color panelColor = new Color(0.07f, 0.09f, 0.13f, 0.98f);
        Color frameColor = new Color(0.015f, 0.02f, 0.03f, 1f);
        Color accentColor = new Color(0.12f, 0.46f, 0.82f, 1f);
        Color textColor = new Color(0.92f, 0.96f, 1f, 1f);
        Color mutedTextColor = new Color(0.70f, 0.78f, 0.88f, 1f);

        GameObject root = new GameObject("LabIntroMenu", typeof(RectTransform), typeof(LabIntroMenuController));
        SetRect(root.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));

        GameObject menuRoot = new GameObject("MenuRoot", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        menuRoot.transform.SetParent(root.transform, false);
        SetRect(menuRoot.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));

        Canvas canvas = menuRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        CanvasScaler scaler = menuRoot.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        CreateImage(menuRoot.transform, "Backdrop", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, backdropColor);

        GameObject panel = CreateImage(menuRoot.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1120f, 720f), Vector2.zero, panelColor);

        TMP_Text title = CreateText(panel.transform, "Title", "Вводный гайд", 42f, FontStyles.Bold, TextAlignmentOptions.Center, textColor);
        SetRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(880f, 56f), new Vector2(0f, -38f), new Vector2(0.5f, 1f));

        TMP_Text description = CreateText(panel.transform, "Description", "Посмотрите короткую инструкцию перед началом работы.", 23f, FontStyles.Normal, TextAlignmentOptions.Center, mutedTextColor);
        SetRect(description.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(880f, 64f), new Vector2(0f, -92f), new Vector2(0.5f, 1f));

        GameObject videoFrame = CreateImage(panel.transform, "VideoFrame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1000f, 470f), new Vector2(0f, -10f), frameColor);

        GameObject videoObject = new GameObject("VideoSurface", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(AspectRatioFitter));
        videoObject.transform.SetParent(videoFrame.transform, false);
        RectTransform videoRect = videoObject.GetComponent<RectTransform>();
        SetRect(videoRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
        videoRect.offsetMin = new Vector2(16f, 16f);
        videoRect.offsetMax = new Vector2(-16f, -16f);

        RawImage videoSurface = videoObject.GetComponent<RawImage>();
        videoSurface.color = Color.black;

        AspectRatioFitter aspectFitter = videoObject.GetComponent<AspectRatioFitter>();
        aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        aspectFitter.aspectRatio = 16f / 9f;

        TMP_Text status = CreateText(videoFrame.transform, "Status", "Загрузка видео...", 22f, FontStyles.Italic, TextAlignmentOptions.Center, mutedTextColor);
        SetRect(status.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));

        Button pauseButton = CreateButton(panel.transform, "PauseButton", "Пауза", new Vector2(220f, 58f), new Vector2(-130f, -322f), accentColor, textColor);
        Button closeButton = CreateButton(panel.transform, "CloseButton", "Закрыть", new Vector2(220f, 58f), new Vector2(130f, -322f), accentColor, textColor);

        GameObject playerObject = new GameObject("IntroVideoPlayer", typeof(VideoPlayer), typeof(AudioSource));
        playerObject.transform.SetParent(root.transform, false);
        VideoPlayer videoPlayer = playerObject.GetComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.isLooping = false;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.playbackSpeed = 1f;
        videoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;

        AssignControllerReferences(root.GetComponent<LabIntroMenuController>(), menuRoot, canvas, videoSurface, title, description, status, closeButton, pauseButton, videoPlayer);
        return root;
    }

    private static void AssignControllerReferences(
        LabIntroMenuController controller,
        GameObject menuRoot,
        Canvas canvas,
        RawImage videoSurface,
        TMP_Text title,
        TMP_Text description,
        TMP_Text status,
        Button closeButton,
        Button pauseButton,
        VideoPlayer videoPlayer)
    {
        SerializedObject serialized = new SerializedObject(controller);
        serialized.FindProperty("videoFileName").stringValue = "LabIntro.mp4";
        serialized.FindProperty("descriptionText").stringValue = "Посмотрите короткую инструкцию перед началом работы.";
        serialized.FindProperty("closeButtonText").stringValue = "Закрыть";
        serialized.FindProperty("pauseButtonText").stringValue = "Пауза";
        serialized.FindProperty("resumeButtonText").stringValue = "Продолжить";
        serialized.FindProperty("titleText").stringValue = "Вводный гайд";
        serialized.FindProperty("menuRoot").objectReferenceValue = menuRoot;
        serialized.FindProperty("canvas").objectReferenceValue = canvas;
        serialized.FindProperty("videoSurface").objectReferenceValue = videoSurface;
        serialized.FindProperty("titleLabel").objectReferenceValue = title;
        serialized.FindProperty("descriptionLabel").objectReferenceValue = description;
        serialized.FindProperty("statusLabel").objectReferenceValue = status;
        serialized.FindProperty("closeButton").objectReferenceValue = closeButton;
        serialized.FindProperty("pauseButton").objectReferenceValue = pauseButton;
        serialized.FindProperty("videoPlayer").objectReferenceValue = videoPlayer;
        serialized.FindProperty("openOnStart").boolValue = true;
        serialized.FindProperty("pauseGameWhileOpen").boolValue = false;
        serialized.FindProperty("unlockCursorWhileOpen").boolValue = true;
        serialized.FindProperty("closeWithEscape").boolValue = true;
        serialized.FindProperty("renderTextureSize").vector2IntValue = new Vector2Int(1920, 1080);
        serialized.FindProperty("loopVideo").boolValue = false;
        serialized.FindProperty("muteVideo").boolValue = true;
        serialized.FindProperty("useManualVideoClockWhenStalled").boolValue = true;
        serialized.FindProperty("aspectRatio").enumValueIndex = (int)VideoAspectRatio.FitInside;
        serialized.FindProperty("sortingOrder").intValue = 5000;
        serialized.ApplyModifiedPropertiesWithoutUndo();
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

    private static Button CreateButton(Transform parent, string name, string label, Vector2 size, Vector2 anchoredPosition, Color normalColor, Color textColor)
    {
        GameObject buttonObject = CreateImage(parent, name, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), size, anchoredPosition, normalColor);
        Button button = buttonObject.AddComponent<Button>();

        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = new Color(0.22f, 0.58f, 0.94f, 1f);
        colors.pressedColor = new Color(0.08f, 0.30f, 0.58f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        TMP_Text labelText = CreateText(buttonObject.transform, "Label", label, 22f, FontStyles.Bold, TextAlignmentOptions.Center, textColor);
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
}
