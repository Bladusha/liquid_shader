using UnityEngine;
using UnityEngine.SceneManagement;

public static class MenuCrosshairBootstrap
{
    private static GameObject root;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        EnsureForScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        EnsureForScene(scene);
    }

    private static void EnsureForScene(Scene scene)
    {
        if (IsRealScene(scene.name))
        {
            if (CrosshairPromptUI.Instance != null)
            {
                root = CrosshairPromptUI.Instance.gameObject;
            }
            else if (root == null)
            {
                root = new GameObject("MenuCrosshairBootstrap");
                Object.DontDestroyOnLoad(root);
            }

            if (root != null && root.GetComponent<CrosshairPromptUI>() == null)
            {
                root.AddComponent<CrosshairPromptUI>();
            }

            root.SetActive(true);
            CrosshairPromptUI ui = CrosshairPromptUI.Instance != null
                ? CrosshairPromptUI.Instance
                : root.GetComponent<CrosshairPromptUI>();
            if (ui != null)
            {
                ui.SetCursorStateControl(false);
                ui.SetRealSceneMode(true);
                ui.SetMenuEnabled(true);
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else if (IsMenuScene(scene.name))
        {
            if (CrosshairPromptUI.Instance != null)
            {
                root = CrosshairPromptUI.Instance.gameObject;
            }
            else if (root == null)
            {
                root = new GameObject("MenuCrosshairBootstrap");
                Object.DontDestroyOnLoad(root);
            }

            if (root != null && root.GetComponent<CrosshairPromptUI>() == null)
            {
                root.AddComponent<CrosshairPromptUI>();
            }

            root.SetActive(true);
            CrosshairPromptUI ui = CrosshairPromptUI.Instance != null
                ? CrosshairPromptUI.Instance
                : root.GetComponent<CrosshairPromptUI>();
            if (ui != null)
            {
                ui.SetCursorStateControl(true);
                ui.SetRealSceneMode(false);
                ui.SetMenuEnabled(true);
                ui.SetPromptVisible(false);
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = false;
        }
        else if (root != null)
        {
            CrosshairPromptUI ui = CrosshairPromptUI.Instance != null
                ? CrosshairPromptUI.Instance
                : root.GetComponent<CrosshairPromptUI>();
            if (ui != null)
            {
                ui.SetCursorStateControl(false);
                ui.SetRealSceneMode(false);
                ui.SetMenuEnabled(false);
            }

            root.SetActive(false);
        }
    }

    private static bool IsMenuScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName))
        {
            return false;
        }

        return sceneName.Contains("Menu", System.StringComparison.OrdinalIgnoreCase)
            || sceneName.Contains("Entry", System.StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRealScene(string sceneName)
    {
        return !string.IsNullOrWhiteSpace(sceneName)
            && sceneName.Contains("real", System.StringComparison.OrdinalIgnoreCase);
    }
}
