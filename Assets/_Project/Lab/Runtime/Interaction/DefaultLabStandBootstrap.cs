using EasyPeasyFirstPersonController;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class DefaultLabStandBootstrap
{
    private const float InteractionReach = 12f;
    private static string[] realSceneNames = { "real" };

    /// <summary>
    /// Настраивает имена сцен, на которых будет автоматически создан DefaultLabStandInteractable.
    /// Вызвать до загрузки сцены.
    /// </summary>
    public static void SetRealSceneNames(params string[] names)
    {
        realSceneNames = names ?? new[] { "real" };
    }

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

        FirstPersonController playerController = Object.FindAnyObjectByType<FirstPersonController>();
        if (playerController != null)
        {
            playerController.interactionDistance = Mathf.Max(playerController.interactionDistance, InteractionReach);
        }

        WorkzoneSelectionController workzoneController = Object.FindAnyObjectByType<WorkzoneSelectionController>();
        if (workzoneController != null)
        {
            workzoneController.maxDistance = Mathf.Max(workzoneController.maxDistance, InteractionReach);
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
            feedback.hoverMessage = "Нажмите E, чтобы открыть паспорт стенда";
        }

        if (string.IsNullOrWhiteSpace(feedback.activeMessage))
        {
            feedback.activeMessage = "Паспорт стенда открыт";
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
        if (string.IsNullOrEmpty(sceneName))
        {
            return false;
        }

        for (int i = 0; i < realSceneNames.Length; i++)
        {
            if (string.Equals(sceneName, realSceneNames[i], System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
