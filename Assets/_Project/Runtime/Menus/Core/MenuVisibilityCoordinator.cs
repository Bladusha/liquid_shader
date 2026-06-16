using System.Collections.Generic;
using System;

public static class MenuVisibilityCoordinator
{
    private static readonly HashSet<string> openMenus = new();
    private static int tabHandledFrame = -1;

    public static event Action<string> MenuOpening;
    public static event Action<bool> AnyMenuStateChanged;

    public static bool AnyMenuOpen => openMenus.Count > 0;
    public static bool WasTabHandledThisFrame => tabHandledFrame == UnityEngine.Time.frameCount;

    public static void SetMenuOpen(string menuId, bool isOpen)
    {
        if (string.IsNullOrWhiteSpace(menuId))
        {
            return;
        }

        if (isOpen)
        {
            if (openMenus.Contains(menuId))
            {
                return;
            }

            MenuOpening?.Invoke(menuId);
            openMenus.Add(menuId);
        }
        else
        {
            if (!openMenus.Remove(menuId))
            {
                return;
            }
        }

        RealHotkeyHintInstaller.SetHiddenByMenu(openMenus.Count > 0);
        AnyMenuStateChanged?.Invoke(openMenus.Count > 0);
    }

    public static void MarkTabHandled()
    {
        tabHandledFrame = UnityEngine.Time.frameCount;
    }

}
