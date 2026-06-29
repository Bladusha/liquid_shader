using EasyPeasyFirstPersonController;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RealPauseMenuBootstrap
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

        RealPauseMenuInstaller installer = controller.GetComponent<RealPauseMenuInstaller>();
        if (installer == null)
        {
            installer = controller.gameObject.AddComponent<RealPauseMenuInstaller>();
        }

        CursorStateUtility.Apply(CursorLockMode.Locked, false);
    }

    private static bool IsRealScene(string sceneName)
    {
        return !string.IsNullOrWhiteSpace(sceneName)
            && sceneName.Contains("real", System.StringComparison.OrdinalIgnoreCase);
    }
}
