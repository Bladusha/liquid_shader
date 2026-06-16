using System.Collections.Generic;
using System.Reflection;
using EasyPeasyFirstPersonController;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractionMenuController : MonoBehaviour
{
    private const string MenuId = "player_interaction";

    [SerializeField] private GameObject menuPrefab;
    [SerializeField] private KeyCode toggleKey = KeyCode.None;
    [SerializeField] private MonoBehaviour[] buttonScripts = new MonoBehaviour[4];
    [SerializeField] private Canvas targetCanvas;

    private readonly List<MonoBehaviour> disabledComponents = new();
    private GameObject currentMenu;
    private FirstPersonController playerController;
    private bool isMenuOpen;
    private bool wasControllerEnabled = true;

    private void Start()
    {
        playerController = GetComponent<FirstPersonController>() ?? FindAnyObjectByType<FirstPersonController>();
        SetCursorState(true);
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
        if (menuId != MenuId && isMenuOpen)
        {
            CloseMenu();
        }
    }

    private void Update()
    {
        if (MenuVisibilityCoordinator.AnyMenuOpen && !isMenuOpen)
        {
            return;
        }

        if (InputSystemCompat.GetKeyDown(toggleKey))
        {
            ToggleMenu();
        }

        if (isMenuOpen && (InputSystemCompat.GetKeyDown(KeyCode.Escape) || InputSystemCompat.GetKeyDown(KeyCode.Tab)))
        {
            if (InputSystemCompat.GetKeyDown(KeyCode.Tab))
            {
                MenuVisibilityCoordinator.MarkTabHandled();
            }

            CloseMenu();
        }
    }

    public void OpenMenuManual()
    {
        if (!isMenuOpen)
        {
            OpenMenu();
        }
    }

    public void CloseMenuManual()
    {
        if (isMenuOpen)
        {
            CloseMenu();
        }
    }

    public bool IsMenuOpen() => isMenuOpen;

    private void ToggleMenu()
    {
        if (isMenuOpen)
        {
            CloseMenu();
            return;
        }

        OpenMenu();
    }

    private void OpenMenu()
    {
        if (menuPrefab == null)
        {
            Debug.LogError("Menu prefab is not assigned.", this);
            return;
        }

        if (currentMenu != null)
        {
            return;
        }

        if (playerController != null)
        {
            wasControllerEnabled = playerController.enabled;
        }

        currentMenu = Instantiate(menuPrefab, GetCanvas().transform);
        currentMenu.name = "PlayerInteractionMenu";
        SetupButtons();

        isMenuOpen = true;
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, true);
        DisablePlayerControls();
    }

    private void CloseMenu()
    {
        if (currentMenu != null)
        {
            Destroy(currentMenu);
            currentMenu = null;
        }

        isMenuOpen = false;
        MenuVisibilityCoordinator.SetMenuOpen(MenuId, false);
        EnablePlayerControls();
    }

    private void DisablePlayerControls()
    {
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        SetCursorState(false);
        disabledComponents.Clear();

        foreach (MonoBehaviour component in GetComponents<MonoBehaviour>())
        {
            if (component == null || component == this || !component.enabled)
            {
                continue;
            }

            string componentName = component.GetType().Name;
            if (componentName.Contains("Input") || componentName.Contains("Control") || componentName.Contains("Mouse"))
            {
                component.enabled = false;
                disabledComponents.Add(component);
            }
        }
    }

    private void EnablePlayerControls()
    {
        if (playerController != null)
        {
            playerController.enabled = wasControllerEnabled;
        }

        SetCursorState(true);

        foreach (MonoBehaviour component in disabledComponents)
        {
            if (component != null)
            {
                component.enabled = true;
            }
        }

        disabledComponents.Clear();
    }

    private void SetCursorState(bool locked)
    {
        Cursor.visible = false;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }

    private void SetupButtons()
    {
        if (currentMenu == null)
        {
            return;
        }

        Button[] buttons = currentMenu.GetComponentsInChildren<Button>(true);
        int count = Mathf.Min(buttons.Length, buttonScripts.Length);

        for (int i = 0; i < count; i++)
        {
            int buttonIndex = i;
            buttons[i].interactable = buttonScripts[i] != null;
            buttons[i].onClick.AddListener(() => OnButtonClick(buttonIndex));
        }
    }

    private void OnButtonClick(int buttonIndex)
    {
        if (buttonIndex < 0 || buttonIndex >= buttonScripts.Length)
        {
            return;
        }

        MonoBehaviour script = buttonScripts[buttonIndex];
        if (script == null)
        {
            return;
        }

        ExecuteScript(script);
        CloseMenu();
    }

    private static void ExecuteScript(MonoBehaviour script)
    {
        MethodInfo executeMethod = script.GetType().GetMethod("Execute");
        if (executeMethod != null && executeMethod.GetParameters().Length == 0)
        {
            executeMethod.Invoke(script, null);
            return;
        }

        MethodInfo onClickMethod = script.GetType().GetMethod("OnClick");
        if (onClickMethod != null && onClickMethod.GetParameters().Length == 0)
        {
            onClickMethod.Invoke(script, null);
        }
    }

    private Canvas GetCanvas()
    {
        if (targetCanvas != null && targetCanvas.gameObject.activeInHierarchy)
        {
            return targetCanvas;
        }

        targetCanvas = FindAnyObjectByType<Canvas>();
        if (targetCanvas != null)
        {
            return targetCanvas;
        }

        GameObject canvasObject = new("MenuCanvas");
        targetCanvas = canvasObject.AddComponent<Canvas>();
        targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObject.AddComponent<GraphicRaycaster>();

        return targetCanvas;
    }
}
