using UnityEngine;
using UnityEngine.SceneManagement;

public static class DefaultLabStandBootstrap
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

        WaterController waterController = Object.FindAnyObjectByType<WaterController>();
        if (waterController == null)
        {
            return;
        }

        GameObject target = waterController.gameObject;

        if (target.GetComponent<DefaultLabStandInteractable>() == null)
        {
            target.AddComponent<DefaultLabStandInteractable>();
        }

        InteractionFeedback feedback = target.GetComponent<InteractionFeedback>();
        if (feedback == null)
        {
            feedback = target.AddComponent<InteractionFeedback>();
        }

        feedback.useOutline = false;

        if (string.IsNullOrWhiteSpace(feedback.hoverMessage))
        {
            feedback.hoverMessage = "Нажмите E, чтобы открыть параметры стенда";
        }

        if (string.IsNullOrWhiteSpace(feedback.activeMessage))
        {
            feedback.activeMessage = "Нажмите E или Esc, чтобы закрыть окно параметров";
        }

        if (target.GetComponent<Collider>() == null)
        {
            MeshFilter meshFilter = target.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
            {
                MeshCollider collider = target.AddComponent<MeshCollider>();
                collider.sharedMesh = meshFilter.sharedMesh;
            }
        }

    }

    private static bool IsRealScene(string sceneName)
    {
        return !string.IsNullOrWhiteSpace(sceneName)
            && sceneName.Contains("real", System.StringComparison.OrdinalIgnoreCase);
    }
}
