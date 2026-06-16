using EasyPeasyFirstPersonController;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DefaultLabStandInteractable : MonoBehaviour, WorkzoneSelectionController.IInteractable
{
    private const string MenuId = "default_lab_stand";

    private static DefaultLabStandInteractable activePanel;

    [Header("References")]
    [SerializeField] private WaterController waterController;
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private InteractionFeedback interactionFeedback;

    [Header("Input")]
    [SerializeField] private KeyCode closeKey = KeyCode.E;
    [SerializeField] private KeyCode alternateCloseKey = KeyCode.Escape;

    [Header("Prefab")]
    [SerializeField] private string editorPrefabPath = "Assets/_Project/Lab/Prefabs/DefaultLabStandPanel.prefab";
    [SerializeField] private string prefabResourcePath = "DefaultLabStandPanel";

    [Header("Layout")]
    [SerializeField] private int sortingOrder = 6450;
    [SerializeField] private Vector2 panelSize = new Vector2(720f, 520f);
    [SerializeField] private Vector2 bodySize = new Vector2(620f, 330f);

    [Header("Style")]
    [SerializeField] private string titleText = "Паспорт лабораторного стенда";
    [SerializeField] private Color backdropColor = new Color(0.02f, 0.03f, 0.05f, 0.72f);
    [SerializeField] private Color panelColor = new Color(0.09f, 0.11f, 0.15f, 0.96f);
    [SerializeField] private Color accentColor = new Color(0.16f, 0.58f, 0.88f, 1f);
    [SerializeField] private Color textColor = new Color(0.92f, 0.96f, 1f, 1f);
    [SerializeField] private Color secondaryTextColor = new Color(0.78f, 0.86f, 0.95f, 1f);

    private GameObject canvasObject;
    private GameObject rootObject;
    private DefaultLabStandPanelView panelView;
    private bool isOpen;
    private bool wasControllerEnabled;
    private string footerMessage;

    public static bool AnyPanelOpen => activePanel != null && activePanel.isOpen;

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        MenuVisibilityCoordinator.MenuOpening += HandleOtherMenuOpening;
    }

    private void OnDisable()
    {
        MenuVisibilityCoordinator.MenuOpening -= HandleOtherMenuOpening;

        if (!Application.isPlaying)
        {
            return;
        }

        if (isOpen)
        {
            ClosePanel();
        }
    }

    private void HandleOtherMenuOpening(string menuId)
    {
        if (menuId != MenuId && isOpen)
        {
            ClosePanel();
        }
    }

    private void Update()
    {
        if (!isOpen)
        {
            return;
        }

        RefreshPanelText();

        if (InputSystemCompat.GetKeyDown(closeKey) || InputSystemCompat.GetKeyDown(alternateCloseKey))
        {
            ClosePanel();
            return;
        }

        if (InputSystemCompat.GetKeyDown(KeyCode.Tab))
        {
            MenuVisibilityCoordinator.MarkTabHandled();
            ClosePanel();
        }
    }

    public void Interact()
    {
        if (MenuVisibilityCoordinator.AnyMenuOpen)
        {
            return;
        }

        if (isOpen)
        {
            ClosePanel();
            return;
        }

        OpenPanel();
    }

    private void OpenPanel()
    {
        ResolveReferences();

        if (activePanel != null && activePanel != this)
        {
            activePanel.ClosePanel();
        }

        EnsureUi();
        footerMessage = "E / Esc - закрыть";
        RefreshPanelText();

        activePanel = this;
        isOpen = true;

        if (rootObject != null)
        {
            rootObject.SetActive(true);
        }

        MenuVisibilityCoordinator.SetMenuOpen(MenuId, true);
        if (interactionFeedback != null)
        {
            interactionFeedback.SetActiveState(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        if (CrosshairPromptUI.Instance != null)
        {
            CrosshairPromptUI.Instance.SetCursorStateControl(false);
            CrosshairPromptUI.Instance.SetRealSceneMode(false);
            CrosshairPromptUI.Instance.SetPromptVisible(false);
            CrosshairPromptUI.Instance.SetMenuEnabled(true);
            CrosshairPromptUI.Instance.ClearPrompt();
            CrosshairPromptUI.Instance.ClearTarget();
        }

        if (WorkzoneSelectionController.Instance != null)
        {
            WorkzoneSelectionController.Instance.SetRaycastPaused(true);
        }

        if (playerController != null)
        {
            wasControllerEnabled = playerController.enabled;
            playerController.ClearInteractionState();
            playerController.SetMoveControl(false);
            playerController.DisableAllMovement();
            playerController.enabled = false;
        }
    }

    private void ClosePanel()
    {
        if (!isOpen)
        {
            return;
        }

        isOpen = false;

        if (rootObject != null)
        {
            rootObject.SetActive(false);
        }

        if (interactionFeedback != null)
        {
            interactionFeedback.SetActiveState(false);
        }

        if (playerController != null)
        {
            playerController.enabled = wasControllerEnabled;

            if (wasControllerEnabled)
            {
                playerController.SetMoveControl(true);
                playerController.EnableAllMovement();
            }
        }

        if (WorkzoneSelectionController.Instance != null)
        {
            WorkzoneSelectionController.Instance.SetRaycastPaused(false);
        }

        if (CrosshairPromptUI.Instance != null)
        {
            CrosshairPromptUI.Instance.SetMenuEnabled(true);
            CrosshairPromptUI.Instance.SetRealSceneMode(true);
            CrosshairPromptUI.Instance.SetCursorStateControl(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, false);

        if (activePanel == this)
        {
            activePanel = null;
        }
    }

    private void ResolveReferences()
    {
        if (waterController == null)
        {
            waterController = GetComponent<WaterController>() ?? GetComponentInParent<WaterController>() ?? FindAnyObjectByType<WaterController>();
        }

        if (playerController == null)
        {
            playerController = FindAnyObjectByType<FirstPersonController>();
        }

        if (interactionFeedback == null)
        {
            interactionFeedback = GetComponent<InteractionFeedback>() ?? GetComponentInParent<InteractionFeedback>();
        }
    }

    private void RefreshPanelText()
    {
        if (panelView == null)
        {
            return;
        }

        panelView.SetTitle(titleText);
        panelView.SetBody(BuildPanelBody());
        panelView.SetFooter(footerMessage);
    }

    private void RecordDataForCalculations()
    {
        ResolveReferences();

        if (waterController == null)
        {
            if (panelView != null)
            {
                footerMessage = "WaterController не найден";
                panelView.SetFooter(footerMessage);
            }
            return;
        }

        if (!Lab01WorkSession.TryRecord(waterController.CurrentMeasurements, out string error))
        {
            if (panelView != null)
            {
                footerMessage = error;
                panelView.SetFooter(footerMessage);
            }

            return;
        }

        if (panelView != null)
        {
            footerMessage = "Данные записаны для расчётов";
            panelView.SetFooter(footerMessage);
        }
    }

    private string BuildPanelBody()
    {
        if (waterController == null)
        {
            return "WaterController не найден. Невозможно рассчитать параметры лабораторной установки.";
        }

        return waterController.BuildLabTableSummary();
    }

    private void EnsureUi()
    {
        if (canvasObject != null && rootObject != null)
        {
            return;
        }

        canvasObject = GameObject.Find("DefaultLabStandCanvas");
        if (canvasObject == null)
        {
            canvasObject = new GameObject("DefaultLabStandCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        GameObject prefab = PrefabResolver.Load(editorPrefabPath, prefabResourcePath);
        rootObject = prefab != null
            ? Instantiate(prefab, canvasObject.transform)
            : BuildFallbackPanel(canvasObject.transform);

        rootObject.name = "DefaultLabStandPanel_Instance";
        panelView = rootObject.GetComponent<DefaultLabStandPanelView>();
        if (panelView != null)
        {
            panelView.RefreshBindings();
            panelView.SetCloseHandler(ClosePanel);
            panelView.SetRecordHandler(RecordDataForCalculations);
            panelView.SetButtonLabels("Записать данные", "Close");
        }

        RectTransform rootRect = rootObject.GetComponent<RectTransform>();
        if (rootRect != null)
        {
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
        }

        rootObject.SetActive(false);
    }

    private GameObject BuildFallbackPanel(Transform parent)
    {
        GameObject root = new GameObject("DefaultLabStandPanel", typeof(RectTransform), typeof(DefaultLabStandPanelView));
        root.transform.SetParent(parent, false);
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = Vector2.zero;
        rootRect.anchorMax = Vector2.one;
        rootRect.offsetMin = Vector2.zero;
        rootRect.offsetMax = Vector2.zero;

        CreateImage(root.transform, "Backdrop", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, backdropColor);

        GameObject panel = CreateImage(root.transform, "Panel", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), panelSize, Vector2.zero, panelColor);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.pivot = new Vector2(0.5f, 0.5f);

        GameObject accent = CreateImage(panel.transform, "Accent", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(panelSize.x - 72f, 6f), new Vector2(0f, -34f), accentColor);
        accent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);

        TMP_Text titleLabel = CreateText(panel.transform, "Title", titleText, 30f, FontStyles.Bold, TextAlignmentOptions.Center, textColor);
        SetRect(titleLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(panelSize.x - 80f, 48f), new Vector2(0f, -70f), new Vector2(0.5f, 1f));

        TMP_Text bodyLabel = CreateText(panel.transform, "Body", string.Empty, 22f, FontStyles.Normal, TextAlignmentOptions.TopLeft, textColor);
        bodyLabel.textWrappingMode = TextWrappingModes.Normal;
        bodyLabel.overflowMode = TextOverflowModes.Overflow;
        SetRect(bodyLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), bodySize, new Vector2(0f, -18f), new Vector2(0.5f, 0.5f));

        TMP_Text footerLabel = CreateText(panel.transform, "Footer", string.Empty, 18f, FontStyles.Bold, TextAlignmentOptions.Center, secondaryTextColor);
        SetRect(footerLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(panelSize.x - 220f, 34f), new Vector2(-52f, 32f), new Vector2(0.5f, 0f));

        Button recordButton = CreateButton(panel.transform, "RecordButton", "Записать данные", new Vector2(190f, 42f), new Vector2(-196f, 34f));
        Button closeButton = CreateButton(panel.transform, "CloseButton", "Close", new Vector2(132f, 42f), new Vector2(panelSize.x * 0.5f - 114f, 34f));

        DefaultLabStandPanelView view = root.GetComponent<DefaultLabStandPanelView>();
        view.Configure(titleLabel, bodyLabel, footerLabel, recordButton, closeButton);
        return root;
    }

    private GameObject CreateImage(Transform parent, string objectName, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition, Color color)
    {
        GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        return imageObject;
    }

    private Button CreateButton(Transform parent, string objectName, string label, Vector2 size, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.GetComponent<Image>();
        image.color = accentColor;

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = accentColor;
        colors.highlightedColor = new Color(0.22f, 0.68f, 0.98f, 1f);
        colors.pressedColor = new Color(0.10f, 0.38f, 0.62f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        TMP_Text labelText = CreateText(buttonObject.transform, "Label", label, 20f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        labelText.rectTransform.anchorMin = Vector2.zero;
        labelText.rectTransform.anchorMax = Vector2.one;
        labelText.rectTransform.offsetMin = Vector2.zero;
        labelText.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    private TMP_Text CreateText(Transform parent, string objectName, string text, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = fontStyle;
        textComponent.alignment = alignment;
        textComponent.color = color;
        textComponent.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
        {
            textComponent.font = TMP_Settings.defaultFontAsset;
        }

        return textComponent;
    }

    private void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 size, Vector2 anchoredPosition, Vector2 pivot)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;
    }
}
