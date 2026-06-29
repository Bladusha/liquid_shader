using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Интеграция CrosshairPromptUI с лабораторными сценами.
/// При загрузке сцены "real" включает режим прицела (курсор зафиксирован в центре).
/// При открытии меню — режим курсора, при закрытии — режим прицела.
/// </summary>
public static class LabSceneCrosshairBootstrap
{
    private static string[] targetSceneNames = { "real" };
    private static bool isSceneActive;

    public static void SetTargetScenes(params string[] names)
    {
        targetSceneNames = names ?? new[] { "real" };
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        MenuVisibilityCoordinator.MenuOpening -= OnMenuOpening;
        MenuVisibilityCoordinator.MenuOpening += OnMenuOpening;
        MenuVisibilityCoordinator.AnyMenuStateChanged -= OnAnyMenuStateChanged;
        MenuVisibilityCoordinator.AnyMenuStateChanged += OnAnyMenuStateChanged;
        CheckScene(SceneManager.GetActiveScene());
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckScene(scene);
    }

    private static void CheckScene(Scene scene)
    {
        bool isTarget = false;
        for (int i = 0; i < targetSceneNames.Length; i++)
        {
            if (string.Equals(scene.name, targetSceneNames[i], System.StringComparison.OrdinalIgnoreCase))
            {
                isTarget = true;
                break;
            }
        }

        if (isTarget)
        {
            ActivateCrosshair();
        }
        else
        {
            DeactivateCrosshair();
        }
    }

    private static void ActivateCrosshair()
    {
        if (isSceneActive)
        {
            return;
        }

        isSceneActive = true;

        CrosshairPromptUI ui = CrosshairPromptUI.Instance;
        if (ui == null)
        {
            return;
        }

        ui.SetMenuEnabled(true);
        ui.SetCursorStateControl(false);
        ui.SetRealSceneMode(true);
    }

    private static void DeactivateCrosshair()
    {
        if (!isSceneActive)
        {
            return;
        }

        isSceneActive = false;

        CrosshairPromptUI ui = CrosshairPromptUI.Instance;
        if (ui == null)
        {
            return;
        }

        ui.SetRealSceneMode(false);
    }

    private static void OnMenuOpening(string menuId)
    {
        if (!isSceneActive)
        {
            return;
        }

        CrosshairPromptUI ui = CrosshairPromptUI.Instance;
        if (ui == null)
        {
            return;
        }

        ui.SetRealSceneMode(false);
    }

    private static void OnAnyMenuStateChanged(bool anyOpen)
    {
        if (!isSceneActive)
        {
            return;
        }

        CrosshairPromptUI ui = CrosshairPromptUI.Instance;
        if (ui == null)
        {
            return;
        }

        if (anyOpen)
        {
            ui.SetRealSceneMode(false);
        }
        else
        {
            ui.SetRealSceneMode(true);
        }
    }

    /// <summary>
    /// Вызвать при входе в режим взаимодействия (BtnPlus и т.д.).
    /// Переключает на курсор.
    /// </summary>
    public static void OnInteractionStarted()
    {
        if (!isSceneActive)
        {
            return;
        }

        CrosshairPromptUI ui = CrosshairPromptUI.Instance;
        if (ui == null)
        {
            return;
        }

        ui.SetRealSceneMode(false);
    }

    /// <summary>
    /// Вызвать при выходе из режима взаимодействия.
    /// Возвращает прицел.
    /// </summary>
    public static void OnInteractionEnded()
    {
        if (!isSceneActive)
        {
            return;
        }

        if (MenuVisibilityCoordinator.AnyMenuOpen)
        {
            return;
        }

        CrosshairPromptUI ui = CrosshairPromptUI.Instance;
        if (ui == null)
        {
            return;
        }

        ui.SetRealSceneMode(true);
    }
}
