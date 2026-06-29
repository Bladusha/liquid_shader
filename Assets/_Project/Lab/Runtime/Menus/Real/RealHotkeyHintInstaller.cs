using System.Collections.Generic;
using EasyPeasyFirstPersonController;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(FirstPersonController))]
public class RealHotkeyHintInstaller : MonoBehaviour
{
    public static RealHotkeyHintInstaller Instance { get; private set; }

    [System.Serializable]
    public class HotkeyEntry
    {
        public KeyCode key = KeyCode.Tab;
        public string label = "Пауза";
    }

    [Header("Scene")]
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private int sortingOrder = 6400;

    [Header("Input")]
    [SerializeField] private KeyCode toggleKey = KeyCode.K;
    [SerializeField] private bool canToggle = true;

    [Header("Prefab")]
    [SerializeField] private string prefabEditorPath = "Assets/_Project/Lab/Prefabs/RealHotkeyHints.prefab";
    [SerializeField] private string prefabResourcePath = "prefabs/RealHotkeyHints";

    [Header("Content")]
    [SerializeField] private bool overridePrefabPromptText;
    [SerializeField] private bool overridePrefabHotkeyLines;
    [SerializeField] private string promptText = "K - подсказки";
    [SerializeField] private HotkeyEntry[] hotkeys = new HotkeyEntry[]
    {
        new HotkeyEntry { key = KeyCode.Tab, label = "Пауза" },
        new HotkeyEntry { key = KeyCode.C, label = "Расчёты" },
        new HotkeyEntry { key = KeyCode.T, label = "Таблица" }
    };

    private GameObject menuInstance;
    private RealHotkeyHintView menuView;
    private bool menuBuiltFromFallback;
    private bool isExpanded;
    private bool suppressedByMenu;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (playerController == null)
        {
            playerController = GetComponent<FirstPersonController>() ?? FindAnyObjectByType<FirstPersonController>();
        }
    }

    private void Start()
    {
        EnsureMenuInstance();
        ApplyState();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (suppressedByMenu)
        {
            return;
        }

        if (canToggle && InputSystemCompat.GetKeyDown(toggleKey))
        {
            ToggleHints();
        }
    }

    public void ToggleHints()
    {
        if (suppressedByMenu)
        {
            return;
        }

        isExpanded = !isExpanded;
        EnsureMenuInstance();
        ApplyState();
    }

    public static void SetHiddenByMenu(bool hidden)
    {
        if (Instance == null)
        {
            return;
        }

        Instance.SetHiddenState(hidden);
    }

    private void SetHiddenState(bool hidden)
    {
        suppressedByMenu = hidden;

        if (menuInstance != null)
        {
            menuInstance.SetActive(!hidden);
        }

        if (!hidden)
        {
            EnsureMenuInstance();
            ApplyState();
        }
    }

    private void EnsureMenuInstance()
    {
        if (menuInstance != null)
        {
            return;
        }

        Canvas canvas = GetTargetCanvas();
        if (canvas == null)
        {
            Debug.LogError("RealHotkeyHintInstaller: target canvas was not found.", this);
            return;
        }

        GameObject prefab = PrefabResolver.Load(prefabEditorPath, prefabResourcePath);
        if (prefab != null)
        {
            menuInstance = Instantiate(prefab, canvas.transform);
            menuInstance.name = "RealHotkeyHints_Instance";
            menuBuiltFromFallback = false;
        }
        else
        {
            menuInstance = BuildFallbackMenu(canvas.transform);
            menuBuiltFromFallback = true;
        }

        RectTransform rect = menuInstance.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = new Vector2(24f, 24f);
        }

        menuView = menuInstance.GetComponent<RealHotkeyHintView>();
        if (menuView != null)
        {
            ApplyContent();
            menuView.SetExpanded(isExpanded);
        }

        menuInstance.SetActive(!suppressedByMenu);
    }

    private void ApplyState()
    {
        if (menuView == null)
        {
            return;
        }

        ApplyContent();
        menuView.SetExpanded(isExpanded);
    }

    private void ApplyContent()
    {
        if (menuView == null)
        {
            return;
        }

        if (menuBuiltFromFallback || overridePrefabPromptText)
        {
            menuView.SetPrompt(promptText);
        }

        if (menuBuiltFromFallback || overridePrefabHotkeyLines)
        {
            menuView.SetHotkeyLines(BuildHotkeyLines());
        }
    }

    private List<string> BuildHotkeyLines()
    {
        List<string> lines = new();
        if (hotkeys == null)
        {
            return lines;
        }

        foreach (HotkeyEntry entry in hotkeys)
        {
            if (entry == null)
            {
                continue;
            }

            string keyLabel = entry.key.ToString().ToUpperInvariant();
            string actionLabel = string.IsNullOrWhiteSpace(entry.label) ? "Action" : entry.label.Trim();
            lines.Add($"{keyLabel} - {actionLabel}");
        }

        return lines;
    }

    private Canvas GetTargetCanvas()
    {
        if (targetCanvas != null)
        {
            return targetCanvas;
        }

        GameObject existing = GameObject.Find("RealHotkeyCanvas");
        if (existing != null)
        {
            targetCanvas = existing.GetComponent<Canvas>();
            if (targetCanvas != null)
            {
                return targetCanvas;
            }
        }

        GameObject canvasObject = new GameObject("RealHotkeyCanvas");
        targetCanvas = canvasObject.AddComponent<Canvas>();
        targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        targetCanvas.sortingOrder = sortingOrder;
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
        GameObject root = new GameObject("RealHotkeyHints", typeof(RectTransform), typeof(CanvasRenderer), typeof(RealHotkeyHintView));
        root.transform.SetParent(parent, false);

        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 0f);
        rootRect.anchorMax = new Vector2(0f, 0f);
        rootRect.pivot = new Vector2(0f, 0f);
        rootRect.sizeDelta = new Vector2(420f, 156f);

        GameObject promptPanel = CreatePanel(root.transform, "PromptPanel", new Vector2(360f, 34f), new Color(0.08f, 0.09f, 0.12f, 0.82f));
        RectTransform promptRect = promptPanel.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0f, 0f);
        promptRect.anchorMax = new Vector2(0f, 0f);
        promptRect.pivot = new Vector2(0f, 0f);
        promptRect.anchoredPosition = new Vector2(0f, 0f);

        TMP_Text promptLabel = CreateText(promptPanel.transform, "PromptLabel", promptText, Vector2.zero, 18f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, Color.white);

        GameObject detailsPanel = CreatePanel(root.transform, "DetailsPanel", new Vector2(360f, 102f), new Color(0.08f, 0.09f, 0.12f, 0.92f));
        RectTransform detailsRect = detailsPanel.GetComponent<RectTransform>();
        detailsRect.anchorMin = new Vector2(0f, 0f);
        detailsRect.anchorMax = new Vector2(0f, 0f);
        detailsRect.pivot = new Vector2(0f, 0f);
        detailsRect.anchoredPosition = new Vector2(0f, 40f);

        TMP_Text detailsLabel = CreateText(detailsPanel.transform, "DetailsLabel", string.Join("\n", BuildHotkeyLines()), Vector2.zero, 20f, FontStyles.Normal, TextAlignmentOptions.TopLeft, Color.white);

        RealHotkeyHintView view = root.GetComponent<RealHotkeyHintView>();
        view.Configure(promptLabel, detailsPanel, detailsLabel);
        view.SetPrompt(promptText);
        view.SetHotkeyLines(BuildHotkeyLines());
        view.SetExpanded(false);

        return root;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        Image image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return panel;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, Vector2 anchoredPosition, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(12f, 6f);
        rect.offsetMax = new Vector2(-12f, -6f);
        rect.anchoredPosition = anchoredPosition;

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
}
