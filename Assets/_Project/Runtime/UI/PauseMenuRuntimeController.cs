using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PauseMenuRuntimeController : MonoBehaviour
{
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private bool canPause = true;
    [SerializeField] private GameObject pauseMenuPrefab;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private bool closeOtherMenusBeforeOpen = true;

    private static PauseMenuRuntimeController instance;

    private GameObject currentPauseMenuInstance;
    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisibility;
    private bool isPaused;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        if (canPause && InputSystemCompat.GetKeyDown(pauseKey))
        {
            TogglePause();
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }

        if (isPaused)
        {
            ResumeGame();
        }
    }

    public static void CloseAllMenus()
    {
        instance?.ClearAllMenusAndResume();
    }

    public void TogglePause()
    {
        if (isPaused)
        {
            ClearAllMenusAndResume();
            return;
        }

        PauseGame();
    }

    public void PauseGame()
    {
        if (pauseMenuPrefab == null)
        {
            Debug.LogError("Pause menu prefab is not assigned.", this);
            return;
        }

        if (closeOtherMenusBeforeOpen)
        {
            DestroyMenusInScene();
        }

        Canvas canvas = GetTargetCanvas();
        if (canvas == null)
        {
            Debug.LogError("Unable to resolve target Canvas for pause menu.", this);
            return;
        }

        previousCursorLockState = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;

        currentPauseMenuInstance = Instantiate(pauseMenuPrefab, canvas.transform);
        currentPauseMenuInstance.name = "PauseMenu_Active";

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        Time.timeScale = 0f;
        isPaused = true;

        BindResumeControls();
    }

    public void ClearAllMenusAndResume()
    {
        DestroyMenusInScene();
        ResumeGame();
    }

    public void ResumeGame()
    {
        Cursor.lockState = previousCursorLockState;
        Cursor.visible = false;
        Time.timeScale = 1f;
        isPaused = false;
    }

    private void BindResumeControls()
    {
        if (currentPauseMenuInstance == null)
        {
            return;
        }

        foreach (Button button in currentPauseMenuInstance.GetComponentsInChildren<Button>(true))
        {
            if (MatchesResumeButton(button))
            {
                button.onClick.AddListener(ClearAllMenusAndResume);
                return;
            }
        }
    }

    private static bool MatchesResumeButton(Button button)
    {
        if (button == null)
        {
            return false;
        }

        if (ContainsMenuKeyword(button.name))
        {
            return true;
        }

        Text text = button.GetComponentInChildren<Text>(true);
        return text != null && ContainsMenuKeyword(text.text);
    }

    private static bool ContainsMenuKeyword(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        string lower = value.ToLowerInvariant();
        return lower.Contains("resume") || lower.Contains("continue") || lower.Contains("продолж");
    }

    private void DestroyMenusInScene()
    {
        if (currentPauseMenuInstance != null)
        {
            Destroy(currentPauseMenuInstance);
            currentPauseMenuInstance = null;
        }

        foreach (GameObject obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (!ShouldDestroyMenuObject(obj))
            {
                continue;
            }

            Destroy(obj);
        }
    }

    private bool ShouldDestroyMenuObject(GameObject obj)
    {
        if (obj == null || obj == gameObject || obj.scene.name == null)
        {
            return false;
        }

        string name = obj.name;
        if (name.Contains("EventSystem") || name.Contains("Canvas") || name.Contains("GraphicRaycaster"))
        {
            return false;
        }

        if (ContainsMenuObjectKeyword(name))
        {
            return true;
        }

        Transform parent = obj.transform.parent;
        return parent != null && ContainsMenuObjectKeyword(parent.name);
    }

    private static bool ContainsMenuObjectKeyword(string value)
    {
        return value.Contains("Menu") ||
               value.Contains("UI_Panel") ||
               value.Contains("Popup") ||
               value.Contains("Dialog") ||
               value.Contains("Window");
    }

    private Canvas GetTargetCanvas()
    {
        if (targetCanvas != null)
        {
            return targetCanvas;
        }

        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay ||
                canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                targetCanvas = canvas;
                return canvas;
            }
        }

        targetCanvas = CreateFallbackCanvas();
        return targetCanvas;
    }

    private static Canvas CreateFallbackCanvas()
    {
        GameObject canvasObject = new("DynamicPauseCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        if (EventSystem.current == null)
        {
            GameObject eventSystemObject = new("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        return canvas;
    }
}
