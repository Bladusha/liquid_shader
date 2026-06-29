using EasyPeasyFirstPersonController;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RealHotkeyHintBootstrap
{
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
        if (!IsRealScene(scene.name))
        {
            return;
        }

        FirstPersonController controller = Object.FindAnyObjectByType<FirstPersonController>();
        if (controller == null)
        {
            return;
        }

        if (controller.GetComponent<RealHotkeyHintInstaller>() == null)
        {
            controller.gameObject.AddComponent<RealHotkeyHintInstaller>();
        }
    }

    private static bool IsRealScene(string sceneName)
    {
        return !string.IsNullOrWhiteSpace(sceneName)
            && sceneName.Contains("real", System.StringComparison.OrdinalIgnoreCase);
    }
}
