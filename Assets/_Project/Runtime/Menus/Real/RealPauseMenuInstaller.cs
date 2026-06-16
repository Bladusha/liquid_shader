using EasyPeasyFirstPersonController;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
[RequireComponent(typeof(FirstPersonController))]
public class RealPauseMenuInstaller : MonoBehaviour
{
    private const string MenuId = "real_pause";
    private const string LabTableMenuId = "lab_table";
    private const string LabCalculationMenuId = "lab_calculation";

    [Header("Scene")]
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private int menuSortingOrder = 6500;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private KeyCode labCalculationHotkey = KeyCode.C;
    [SerializeField] private KeyCode labTableHotkey = KeyCode.T;
    [SerializeField] private bool canToggle = true;

    [Header("Prefab")]
    [SerializeField] private string prefabEditorPath = "Assets/_Project/Lab/Prefabs/RealPauseMenu.prefab";
    [SerializeField] private string prefabResourcePath = "prefabs/RealPauseMenu";
    [SerializeField] private string labTableMenuEditorPath = "Assets/_Project/Lab/Prefabs/Lab01TableMenu.prefab";
    [SerializeField] private string labTableMenuPrefabResourcePath = "prefabs/Lab01TableMenu";
    [SerializeField] private string labCalculationMenuEditorPath = "Assets/_Project/Lab/Prefabs/Lab01CalculationMenu.prefab";
    [SerializeField] private string labCalculationMenuPrefabResourcePath = "prefabs/Lab01CalculationMenu";

    [Header("Behavior")]
    [SerializeField] private bool pauseGameWhileOpen = true;
    [SerializeField] private bool overridePrefabText;
    [SerializeField] private string runtimeTitleText = "Пауза";
    [SerializeField] private string runtimeInfoText = "Tab - открыть / закрыть";

    private GameObject menuInstance;
    private RealPauseMenuView menuView;
    private GameObject labTableMenuInstance;
    private Lab01TableMenuController labTableMenu;
    private GameObject labCalculationMenuInstance;
    private Lab01CalculationMenuController labCalculationMenu;
    private GameObject logsPanelInstance;
    private TMP_Text logsBodyText;
    private bool isOpen;
    private float previousTimeScale = 1f;
    private bool suppressMenuOpeningReaction;

    private void Awake()
    {
        if (playerController == null)
        {
            playerController = GetComponent<FirstPersonController>() ?? FindAnyObjectByType<FirstPersonController>();
        }
    }

    private void OnEnable()
    {
        MenuVisibilityCoordinator.MenuOpening += HandleOtherMenuOpening;
    }

    private void OnDisable()
    {
        MenuVisibilityCoordinator.MenuOpening -= HandleOtherMenuOpening;
    }

    private void HandleOtherMenuOpening(string menuId)
    {
        if (suppressMenuOpeningReaction)
        {
            return;
        }

        if (menuId != MenuId && isOpen)
        {
            CloseMenu();
        }
    }

    private void Update()
    {
        if (DefaultLabStandInteractable.AnyPanelOpen)
        {
            return;
        }

        if (IsTextInputFocused())
        {
            return;
        }

        if (canToggle && InputSystemCompat.GetKeyDown(labCalculationHotkey))
        {
            ToggleLabMenuFromHotkey(LabCalculationMenuId, OpenLabCalculations);
            return;
        }

        if (canToggle && InputSystemCompat.GetKeyDown(labTableHotkey))
        {
            ToggleLabMenuFromHotkey(LabTableMenuId, OpenLabTable);
            return;
        }

        if (InputSystemCompat.GetKeyDown(toggleKey))
        {
            if (MenuVisibilityCoordinator.WasTabHandledThisFrame)
            {
                return;
            }

            if (isOpen)
            {
                CloseMenu();
                return;
            }

            if (canToggle)
            {
                ToggleMenu();
            }
        }
    }

    public void ToggleMenu()
    {
        if (isOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        if (isOpen)
        {
            return;
        }

        previousTimeScale = Time.timeScale;

        if (pauseGameWhileOpen)
        {
            Time.timeScale = 0f;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;

        if (playerController != null)
        {
            playerController.SetMoveControl(false);
            playerController.DisableAllMovement();
        }

        EnsureMenuInstance();
        if (menuInstance == null)
        {
            if (pauseGameWhileOpen)
            {
                Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerController != null)
            {
                playerController.EnableAllMovement();
                playerController.SetMoveControl(true);
            }

            return;
        }

        BindMenu();
        menuInstance.SetActive(true);

        MenuVisibilityCoordinator.SetMenuOpen(MenuId, true);
        isOpen = true;
    }

    public void CloseMenu()
    {
        if (!isOpen)
        {
            return;
        }

        if (menuInstance != null)
        {
            menuInstance.SetActive(false);
        }

        if (labTableMenuInstance != null)
        {
            labTableMenuInstance.SetActive(false);
        }

        MenuVisibilityCoordinator.SetMenuOpen(LabTableMenuId, false);

        if (labCalculationMenuInstance != null)
        {
            labCalculationMenuInstance.SetActive(false);
        }

        MenuVisibilityCoordinator.SetMenuOpen(LabCalculationMenuId, false);

        if (logsPanelInstance != null)
        {
            logsPanelInstance.SetActive(false);
        }

        if (pauseGameWhileOpen)
        {
            Time.timeScale = previousTimeScale <= 0f ? 1f : previousTimeScale;
        }

        MenuVisibilityCoordinator.SetMenuOpen(MenuId, false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null)
        {
            playerController.EnableAllMovement();
            playerController.SetMoveControl(true);
        }

        isOpen = false;
    }

    private void EnsureMenuInstance()
    {
        if (menuInstance != null)
        {
            return;
        }

        GameObject prefab = PrefabResolver.Load(prefabEditorPath, prefabResourcePath);
        Canvas canvas = GetTargetCanvas();
        if (canvas == null)
        {
            Debug.LogError("RealPauseMenuInstaller: target canvas was not found.", this);
            return;
        }

        if (prefab == null)
        {
            Debug.LogWarning($"RealPauseMenuInstaller: prefab '{prefabResourcePath}' was not found in Resources, using runtime fallback.", this);
            menuInstance = BuildFallbackMenu(canvas.transform);
        }
        else
        {
            menuInstance = Instantiate(prefab, canvas.transform);
            menuInstance.name = "RealPauseMenu_Instance";
        }

        var rect = menuInstance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
        }

        menuView = menuInstance.GetComponent<RealPauseMenuView>();
        if (menuView != null)
        {
            menuView.RefreshBindings();
            EnsureLabButtonsInPauseMenu();
            EnsureLogsButtonInPauseMenu();
        }
    }

    private void BindMenu()
    {
        if (menuView == null)
        {
            return;
        }

        menuView.SetContinueHandler(CloseMenu);
        menuView.SetCursorTestHandler(() => { });
        menuView.SetLabTableHandler(OpenLabTable);
        menuView.SetLabCalculationHandler(OpenLabCalculations);
        menuView.SetLogsHandler(OpenLogsPanel);
        if (overridePrefabText)
        {
            menuView.SetTitle(runtimeTitleText);
            menuView.SetInfo(runtimeInfoText);
        }
    }

    private void ToggleLabMenuFromHotkey(string menuId, System.Action openLabMenu)
    {
        if (openLabMenu == null)
        {
            return;
        }

        if (IsLabMenuOpen(menuId))
        {
            CloseMenu();
            return;
        }

        if (MenuVisibilityCoordinator.AnyMenuOpen && !isOpen && !IsLabMenuOpen())
        {
            return;
        }

        if (!isOpen)
        {
            OpenMenu();
        }

        if (!isOpen)
        {
            return;
        }

        openLabMenu.Invoke();
    }

    private void OpenLabTable()
    {
        EnsureLabTableInstance();
        if (labTableMenu == null)
        {
            return;
        }

        suppressMenuOpeningReaction = true;
        try
        {
            if (menuInstance != null)
            {
                menuInstance.SetActive(false);
            }

            if (labCalculationMenuInstance != null)
            {
                labCalculationMenuInstance.SetActive(false);
            }

            MenuVisibilityCoordinator.SetMenuOpen(LabCalculationMenuId, false);
            labTableMenu.Open(FindAnyObjectByType<WaterController>(), ReturnToPauseMenu, OpenLabCalculations);
            MenuVisibilityCoordinator.SetMenuOpen(LabTableMenuId, true);
        }
        finally
        {
            suppressMenuOpeningReaction = false;
        }
    }

    private void OpenLabCalculations()
    {
        EnsureLabCalculationInstance();
        if (labCalculationMenu == null)
        {
            return;
        }

        suppressMenuOpeningReaction = true;
        try
        {
            if (menuInstance != null)
            {
                menuInstance.SetActive(false);
            }

            if (labTableMenuInstance != null)
            {
                labTableMenuInstance.SetActive(false);
            }

            MenuVisibilityCoordinator.SetMenuOpen(LabTableMenuId, false);
            labCalculationMenu.Open(FindAnyObjectByType<WaterController>(), ReturnToPauseMenu, OpenLabTable);
            MenuVisibilityCoordinator.SetMenuOpen(LabCalculationMenuId, true);
        }
        finally
        {
            suppressMenuOpeningReaction = false;
        }
    }

    private void ReturnToPauseMenu()
    {
        if (isOpen && menuInstance != null)
        {
            menuInstance.SetActive(true);
        }
    }

    private void OpenLogsPanel()
    {
        EnsureLogsPanelInstance();
        if (logsPanelInstance == null)
        {
            return;
        }

        UpdateLogsPanelText();

        if (menuInstance != null)
        {
            menuInstance.SetActive(false);
        }

        logsPanelInstance.SetActive(true);
    }

    private void CloseLogsPanel()
    {
        if (logsPanelInstance != null)
        {
            logsPanelInstance.SetActive(false);
        }

        if (isOpen && menuInstance != null)
        {
            menuInstance.SetActive(true);
        }
    }

    private void EnsureLogsPanelInstance()
    {
        if (logsPanelInstance != null)
        {
            return;
        }

        Canvas canvas = GetTargetCanvas();
        if (canvas == null)
        {
            return;
        }

        logsPanelInstance = BuildLogsPanel(canvas.transform);
        StretchToCanvas(logsPanelInstance);
        logsPanelInstance.SetActive(false);
    }

    private void UpdateLogsPanelText()
    {
        if (logsBodyText != null)
        {
            logsBodyText.text = SceneActivityLog.BuildDisplayText();
        }
    }

    private void EnsureLabTableInstance()
    {
        if (labTableMenu != null)
        {
            return;
        }

        Canvas canvas = GetTargetCanvas();
        GameObject prefab = PrefabResolver.Load(labTableMenuEditorPath, labTableMenuPrefabResourcePath);
        if (prefab == null)
        {
            Debug.LogError($"RealPauseMenuInstaller: prefab '{labTableMenuPrefabResourcePath}' was not found in Resources.", this);
            return;
        }

        labTableMenuInstance = Instantiate(prefab, canvas.transform);
        labTableMenuInstance.name = "Lab01TableMenu_Instance";
        labTableMenu = labTableMenuInstance.GetComponent<Lab01TableMenuController>();
        if (labTableMenu == null)
        {
            Debug.LogError("RealPauseMenuInstaller: Lab01 table prefab does not contain Lab01TableMenuController.", this);
            return;
        }

        StretchToCanvas(labTableMenuInstance);
        labTableMenu.RefreshBindings();
        labTableMenuInstance.SetActive(false);
    }

    private void EnsureLabCalculationInstance()
    {
        if (labCalculationMenu != null)
        {
            return;
        }

        Canvas canvas = GetTargetCanvas();
        GameObject prefab = PrefabResolver.Load(labCalculationMenuEditorPath, labCalculationMenuPrefabResourcePath);
        if (prefab == null)
        {
            Debug.LogError($"RealPauseMenuInstaller: prefab '{labCalculationMenuPrefabResourcePath}' was not found in Resources.", this);
            return;
        }

        labCalculationMenuInstance = Instantiate(prefab, canvas.transform);
        labCalculationMenuInstance.name = "Lab01CalculationMenu_Instance";
        labCalculationMenu = labCalculationMenuInstance.GetComponent<Lab01CalculationMenuController>();
        if (labCalculationMenu == null)
        {
            Debug.LogError("RealPauseMenuInstaller: Lab01 calculation prefab does not contain Lab01CalculationMenuController.", this);
            return;
        }

        StretchToCanvas(labCalculationMenuInstance);
        labCalculationMenu.RefreshBindings();
        labCalculationMenuInstance.SetActive(false);
    }

    private static void StretchToCanvas(GameObject instance)
    {
        RectTransform rect = instance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }

    private bool IsLabMenuOpen()
    {
        return (labTableMenuInstance != null && labTableMenuInstance.activeSelf) ||
            (labCalculationMenuInstance != null && labCalculationMenuInstance.activeSelf);
    }

    private bool IsLabMenuOpen(string menuId)
    {
        return menuId switch
        {
            LabTableMenuId => labTableMenuInstance != null && labTableMenuInstance.activeSelf,
            LabCalculationMenuId => labCalculationMenuInstance != null && labCalculationMenuInstance.activeSelf,
            _ => false
        };
    }

    private static bool IsTextInputFocused()
    {
        GameObject selectedObject = EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null;
        if (selectedObject == null)
        {
            return false;
        }

        TMP_InputField tmpInput = selectedObject.GetComponent<TMP_InputField>();
        if (tmpInput != null && tmpInput.isFocused)
        {
            return true;
        }

        InputField input = selectedObject.GetComponent<InputField>();
        return input != null && input.isFocused;
    }

    private void EnsureLabButtonsInPauseMenu()
    {
        if (menuView == null || menuView.HasLabButtons || menuInstance == null)
        {
            return;
        }

        Transform parent = menuInstance.transform.Find("Panel") ?? menuInstance.transform;
        Button tableButton = CreateButton(parent, "LabTableButton", "Table", new Vector2(240f, 48f), new Vector2(0f, -145f));
        Button calculationButton = CreateButton(parent, "LabCalculationButton", "Calculations", new Vector2(240f, 48f), new Vector2(0f, -205f));
        menuView.SetLabButtons(tableButton, calculationButton);
    }

    private void EnsureLogsButtonInPauseMenu()
    {
        if (menuView == null || menuView.HasLogsButton || menuInstance == null)
        {
            return;
        }

        Transform parent = menuInstance.transform.Find("Panel") ?? menuInstance.transform;
        Button logsButton = CreateButton(parent, "LogsButton", "Logs", new Vector2(240f, 48f), new Vector2(0f, -265f));
        menuView.SetLogsButton(logsButton);
    }

    private Canvas GetTargetCanvas()
    {
        if (targetCanvas != null)
        {
            return targetCanvas;
        }

        GameObject existing = GameObject.Find("RealPauseCanvas");
        if (existing != null)
        {
            targetCanvas = existing.GetComponent<Canvas>();
            if (targetCanvas != null)
            {
                return targetCanvas;
            }
        }

        GameObject canvasObject = new GameObject("RealPauseCanvas");
        targetCanvas = canvasObject.AddComponent<Canvas>();
        targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        targetCanvas.sortingOrder = menuSortingOrder;
        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();
        EnsureEventSystem();
        return targetCanvas;
    }

    private static void EnsureEventSystem()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    private GameObject BuildFallbackMenu(Transform parent)
    {
        GameObject root = new GameObject("RealPauseMenu", typeof(RectTransform), typeof(CanvasRenderer), typeof(RealPauseMenuView));
        root.transform.SetParent(parent, false);

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0.5f, 0.5f);
        rootRect.anchorMax = new Vector2(0.5f, 0.5f);
        rootRect.pivot = new Vector2(0.5f, 0.5f);
        rootRect.sizeDelta = new Vector2(560f, 520f);

        GameObject panel = CreatePanel(root.transform, "Panel", new Vector2(560f, 520f), new Color(0.08f, 0.09f, 0.12f, 0.96f));
        panel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        TMP_Text title = CreateText(panel.transform, "Title", "Пауза", new Vector2(0f, 205f), 44f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        TMP_Text info = CreateText(panel.transform, "Info", "Tab - открыть / закрыть", new Vector2(0f, 160f), 22f, FontStyles.Normal, TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 0.8f));
        Button continueButton = CreateButton(panel.transform, "ContinueButton", "Продолжить", new Vector2(240f, 50f), new Vector2(0f, 92f));
        Button tableButton = CreateButton(panel.transform, "LabTableButton", "Таблица", new Vector2(240f, 50f), new Vector2(0f, 32f));
        Button calculationButton = CreateButton(panel.transform, "LabCalculationButton", "Расчёты", new Vector2(240f, 50f), new Vector2(0f, -28f));
        Button logsButton = CreateButton(panel.transform, "LogsButton", "Логи", new Vector2(240f, 50f), new Vector2(0f, -88f));
        Button cursorTestButton = CreateButton(panel.transform, "CursorTestButton", "Тест", new Vector2(240f, 50f), new Vector2(0f, -148f));
        TMP_Text cursorTestLabel = CreateText(panel.transform, "CursorTestLabel", "Clicks: 0", new Vector2(0f, -205f), 20f, FontStyles.Normal, TextAlignmentOptions.Center, new Color(1f, 1f, 1f, 0.85f));

        RealPauseMenuView view = root.GetComponent<RealPauseMenuView>();
        view.Configure(title, info, continueButton, cursorTestButton, tableButton, calculationButton, logsButton, cursorTestLabel);
        view.SetContinueHandler(CloseMenu);
        view.SetCursorTestHandler(() => { });

        return root;
    }

    private GameObject BuildLogsPanel(Transform parent)
    {
        GameObject root = new GameObject("SceneLogsPanel", typeof(RectTransform));
        root.transform.SetParent(parent, false);

        CreateStretchPanel(root.transform, "Backdrop", new Color(0.02f, 0.03f, 0.05f, 0.72f));

        GameObject panel = CreatePanel(root.transform, "Panel", new Vector2(900f, 620f), new Color(0.08f, 0.09f, 0.12f, 0.97f));
        panel.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        TMP_Text title = CreateText(panel.transform, "Title", "Scene Logs", new Vector2(0f, 260f), 34f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        title.rectTransform.sizeDelta = new Vector2(820f, 52f);

        GameObject viewport = CreatePanel(panel.transform, "LogViewport", new Vector2(800f, 420f), new Color(0.04f, 0.05f, 0.07f, 0.95f));
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchoredPosition = new Vector2(0f, 16f);
        Mask mask = viewport.AddComponent<Mask>();
        mask.showMaskGraphic = true;

        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(TextMeshProUGUI));
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 1f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(-40f, 0f);
        logsBodyText = content.GetComponent<TextMeshProUGUI>();
        logsBodyText.text = string.Empty;
        logsBodyText.fontSize = 20f;
        logsBodyText.alignment = TextAlignmentOptions.TopLeft;
        logsBodyText.color = new Color(0.9f, 0.95f, 1f, 1f);
        logsBodyText.raycastTarget = false;
        logsBodyText.textWrappingMode = TextWrappingModes.Normal;
        logsBodyText.overflowMode = TextOverflowModes.Overflow;
        logsBodyText.enableAutoSizing = false;

        ScrollRect scrollRect = viewport.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        Button closeButton = CreateButton(panel.transform, "CloseLogsButton", "Close", new Vector2(180f, 46f), new Vector2(0f, -264f));
        closeButton.onClick.AddListener(CloseLogsPanel);
        return root;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;

        Image image = panel.GetComponent<Image>();
        image.color = color;
        return panel;
    }

    private static GameObject CreateStretchPanel(Transform parent, string name, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = panel.GetComponent<Image>();
        image.color = color;
        return panel;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, Vector2 anchoredPosition, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = new Vector2(520f, 60f);

        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
        {
            label.font = TMP_Settings.defaultFontAsset;
        }

        return label;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 size, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPosition;

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.18f, 0.45f, 0.82f, 1f);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = image.color;
        colors.highlightedColor = new Color(0.24f, 0.56f, 0.92f, 1f);
        colors.pressedColor = new Color(0.14f, 0.34f, 0.62f, 1f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        TMP_Text labelText = CreateText(buttonObject.transform, "Label", label, Vector2.zero, 24f, FontStyles.Bold, TextAlignmentOptions.Center, Color.white);
        labelText.rectTransform.anchorMin = Vector2.zero;
        labelText.rectTransform.anchorMax = Vector2.one;
        labelText.rectTransform.offsetMin = Vector2.zero;
        labelText.rectTransform.offsetMax = Vector2.zero;
        return button;
    }
}
