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
    [SerializeField] private string calculationMenuEditorPath = "Assets/_Project/Lab/Prefabs/Lab01CalculationMenu.prefab";
    [SerializeField] private string calculationMenuResourcePath = "prefabs/Lab01CalculationMenu";
    [SerializeField] private string tableMenuEditorPath = "Assets/_Project/Lab/Prefabs/Lab01TableMenu.prefab";
    [SerializeField] private string tableMenuResourcePath = "prefabs/Lab01TableMenu";

    [Header("Layout")]
    [SerializeField] private int sortingOrder = 6450;
    [SerializeField] private Vector2 panelSize = new Vector2(820f, 560f);
    [SerializeField] private Vector2 bodySize = new Vector2(650f, 278f);

    [Header("Style")]
    [SerializeField] private string titleText = "Паспорт лабораторного стенда";
    [SerializeField] private Color backdropColor = new Color(0.82f, 0.86f, 0.88f, 0.38f);
    [SerializeField] private Color panelColor = new Color(0.96f, 0.99f, 1f, 0.98f);
    [SerializeField] private Color accentColor = new Color(0.20f, 0.72f, 0.92f, 0.92f);
    [SerializeField] private Color textColor = new Color(0.07f, 0.18f, 0.24f, 1f);
    [SerializeField] private Color secondaryTextColor = new Color(0.34f, 0.47f, 0.55f, 1f);

    private GameObject canvasObject;
    private GameObject rootObject;
    private DefaultLabStandPanelView panelView;
    private GameObject calculationMenuObject;
    private Lab01CalculationMenuController calculationMenuController;
    private GameObject tableMenuObject;
    private Lab01TableMenuController tableMenuController;
    private bool isOpen;
    private bool wasControllerEnabled;
    private bool controllerStateCaptured;
    private string footerMessage;
    private string defaultFooterMessage;
    private bool recordNotificationActive;
    private float recordNotificationEndsAt;

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

        UpdateRecordNotification();
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
        defaultFooterMessage = "E / Esc - закрыть";
        footerMessage = defaultFooterMessage;
        recordNotificationActive = false;
        recordNotificationEndsAt = 0f;
        panelView?.HideRecordNotification();
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

        CursorStateUtility.Apply(CursorLockMode.None, false);

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
            controllerStateCaptured = true;
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

        MenuVisibilityCoordinator.SetMenuOpen(MenuId, false);
        RestoreGameplayState();

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

    private bool RecordDataForCalculations()
    {
        ResolveReferences();

        if (waterController == null)
        {
            if (panelView != null)
            {
                footerMessage = "WaterController не найден";
                panelView.SetFooter(footerMessage);
                panelView.HideRecordNotification();
            }
            return false;
        }

        if (!Lab01WorkSession.TryRecord(waterController.CurrentMeasurements, out string error))
        {
            if (panelView != null)
            {
                footerMessage = error;
                panelView.SetFooter(footerMessage);
                panelView.HideRecordNotification();
            }

            return false;
        }

        return true;
    }

    private void RecordDataAndNotify()
    {
        if (RecordDataForCalculations())
        {
            ShowRecordNotification();
        }
    }

    private void ShowRecordNotification()
    {
        if (panelView == null)
        {
            return;
        }

        string template = panelView.GetRecordSuccessMessageTemplate();
        if (string.IsNullOrWhiteSpace(template))
        {
            template = "Запись номер {0} сохранена!";
        }

        string message = string.Format(template, Lab01WorkSession.CurrentAttemptNumber);
        float duration = Mathf.Max(0f, panelView.GetRecordSuccessMessageDuration());

        footerMessage = defaultFooterMessage;
        panelView.SetFooter(footerMessage);
        panelView.ShowRecordNotification(message);
        recordNotificationActive = duration > 0f;
        recordNotificationEndsAt = Time.unscaledTime + duration;
    }

    private void UpdateRecordNotification()
    {
        if (!recordNotificationActive || Time.unscaledTime < recordNotificationEndsAt)
        {
            return;
        }

        recordNotificationActive = false;
        panelView?.HideRecordNotification();
    }

    private void OpenCalculationsMenu()
    {
        if (isOpen)
        {
            ClosePanel();
        }

        EnsureCalculationMenu();
        if (calculationMenuController == null)
        {
            return;
        }

        EnterMenuState();
        calculationMenuController.Open(GetWaterController(), CloseCalculationsMenu, OpenTableMenu);
    }

    private void OpenTableMenu()
    {
        EnsureTableMenu();
        if (tableMenuController == null)
        {
            return;
        }

        EnterMenuState();
        tableMenuController.Open(GetWaterController(), CloseTableMenu, OpenCalculationsMenu);
    }

    private void CloseCalculationsMenu()
    {
        RestoreGameplayState();
    }

    private void CloseTableMenu()
    {
        RestoreGameplayState();
    }

    private void EnsureCalculationMenu()
    {
        if (calculationMenuController != null)
        {
            return;
        }

        Canvas canvas = canvasObject != null ? canvasObject.GetComponent<Canvas>() : FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        GameObject prefab = PrefabResolver.Load(calculationMenuEditorPath, calculationMenuResourcePath);
        if (prefab == null)
        {
            return;
        }

        calculationMenuObject = Instantiate(prefab, canvas.transform);
        calculationMenuObject.name = "Lab01CalculationMenu_Instance";
        calculationMenuController = calculationMenuObject.GetComponent<Lab01CalculationMenuController>();
        if (calculationMenuController == null)
        {
            return;
        }

        RectTransform rect = calculationMenuObject.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        calculationMenuController.RefreshBindings();
        calculationMenuObject.SetActive(false);
    }

    private void EnsureTableMenu()
    {
        if (tableMenuController != null)
        {
            return;
        }

        Canvas canvas = canvasObject != null ? canvasObject.GetComponent<Canvas>() : FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        GameObject prefab = PrefabResolver.Load(tableMenuEditorPath, tableMenuResourcePath);
        if (prefab == null)
        {
            return;
        }

        tableMenuObject = Instantiate(prefab, canvas.transform);
        tableMenuObject.name = "Lab01TableMenu_Instance";
        tableMenuController = tableMenuObject.GetComponent<Lab01TableMenuController>();
        if (tableMenuController == null)
        {
            return;
        }

        RectTransform rect = tableMenuObject.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        tableMenuController.RefreshBindings();
        tableMenuObject.SetActive(false);
    }

    private void EnterMenuState()
    {
        CursorStateUtility.Apply(CursorLockMode.None, false);

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
            if (!controllerStateCaptured)
            {
                wasControllerEnabled = playerController.enabled;
                controllerStateCaptured = true;
            }

            playerController.ClearInteractionState();
            playerController.SetMoveControl(false);
            playerController.DisableAllMovement();
            playerController.enabled = false;
        }
    }

    private void RestoreGameplayState()
    {
        bool labWorkZoneActive = LabWorkZoneController.Instance != null && LabWorkZoneController.Instance.IsWorkModeActive;

        if (playerController != null && !labWorkZoneActive)
        {
            playerController.enabled = wasControllerEnabled;

            if (wasControllerEnabled)
            {
                playerController.SetMoveControl(true);
                playerController.EnableAllMovement();
            }
        }
        controllerStateCaptured = false;

        if (WorkzoneSelectionController.Instance != null)
        {
            WorkzoneSelectionController.Instance.SetRaycastPaused(false);
        }

        if (CrosshairPromptUI.Instance != null)
        {
            CrosshairPromptUI.Instance.SetMenuEnabled(true);
            CrosshairPromptUI.Instance.SetCursorStateControl(false);
        }

        if (labWorkZoneActive)
        {
            CursorStateUtility.Apply(CursorLockMode.None, false);
            LabSceneCrosshairBootstrap.OnInteractionStarted();
            return;
        }

        if (CrosshairPromptUI.Instance != null)
        {
            CrosshairPromptUI.Instance.SetRealSceneMode(true);
        }

        CursorStateUtility.Apply(CursorLockMode.Locked, false);
    }

    private WaterController GetWaterController()
    {
        ResolveReferences();
        return waterController != null ? waterController : FindAnyObjectByType<WaterController>();
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
            panelView.SetRecordHandler(RecordDataAndNotify);
            panelView.SetCalculationHandler(OpenCalculationsMenu);
            panelView.SetButtonLabels("Записать данные", "Перейти к расчётам", "Закрыть");
            panelView.HideRecordNotification();
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

        GameObject accent = CreateImage(panel.transform, "Accent", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(180f, 5f), new Vector2(0f, -112f), accentColor);
        accent.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);

        TMP_Text titleLabel = CreateText(panel.transform, "Title", titleText, 28f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.043f, 0.435f, 0.624f, 1f));
        SetRect(titleLabel.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(680f, 54f), new Vector2(0f, -38f), new Vector2(0.5f, 1f));

        TMP_Text bodyLabel = CreateText(panel.transform, "Body", string.Empty, 18f, FontStyles.Normal, TextAlignmentOptions.TopLeft, textColor);
        bodyLabel.textWrappingMode = TextWrappingModes.Normal;
        bodyLabel.overflowMode = TextOverflowModes.Overflow;
        SetRect(bodyLabel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), bodySize, new Vector2(0f, -18f), new Vector2(0.5f, 0.5f));

        TMP_Text footerLabel = CreateText(panel.transform, "Footer", string.Empty, 18f, FontStyles.Bold, TextAlignmentOptions.Center, secondaryTextColor);
        SetRect(footerLabel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(panelSize.x - 220f, 34f), new Vector2(-52f, 32f), new Vector2(0.5f, 0f));

        GameObject notificationRoot = CreateImage(panel.transform, "RecordNotification", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(360f, 44f), new Vector2(0f, 96f), new Color(0.13f, 0.29f, 0.20f, 0.96f));
        notificationRoot.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0f);

        TMP_Text notificationLabel = CreateText(notificationRoot.transform, "Message", "Запись номер 1 сохранена!", 18f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        notificationLabel.textWrappingMode = TextWrappingModes.NoWrap;
        notificationLabel.overflowMode = TextOverflowModes.Ellipsis;
        notificationLabel.rectTransform.anchorMin = Vector2.zero;
        notificationLabel.rectTransform.anchorMax = Vector2.one;
        notificationLabel.rectTransform.offsetMin = new Vector2(16f, 8f);
        notificationLabel.rectTransform.offsetMax = new Vector2(-16f, -8f);
        notificationRoot.SetActive(false);

        Button recordButton = CreateButton(panel.transform, "RecordButton", "Записать данные", new Vector2(210f, 56f), new Vector2(-230f, 32f));
        Button calculationButton = CreateButton(panel.transform, "CalculationButton", "Перейти к расчётам", new Vector2(260f, 56f), new Vector2(0f, 32f));
        Button closeButton = CreateButton(panel.transform, "CloseButton", "Закрыть", new Vector2(160f, 56f), new Vector2(230f, 32f));

        DefaultLabStandPanelView view = root.GetComponent<DefaultLabStandPanelView>();
        view.Configure(titleLabel, bodyLabel, footerLabel, notificationRoot, notificationLabel, recordButton, calculationButton, closeButton);
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
        image.color = Color.white;

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Color.white;
        colors.pressedColor = Color.white;
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(1f, 1f, 1f, 0.55f);
        button.colors = colors;

        TMP_Text labelText = CreateText(buttonObject.transform, "Label", label, 17f, FontStyles.Bold, TextAlignmentOptions.Center, new Color(0.043f, 0.435f, 0.624f, 1f));
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
