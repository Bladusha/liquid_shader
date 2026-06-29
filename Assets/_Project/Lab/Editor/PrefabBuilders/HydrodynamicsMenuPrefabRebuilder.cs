using UnityEditor;

public static class HydrodynamicsMenuPrefabRebuilder
{
    [MenuItem("Tools/LiquidShader/Rebuild Hydrodynamics Menu Prefabs")]
    public static void RebuildAll()
    {
        RealPauseMenuPrefabBuilder.CreatePrefab();
        RealHotkeyHintPrefabBuilder.CreatePrefab();
        Lab01WorkMenuPrefabsBuilder.CreatePrefabs();
        DefaultLabStandPanelPrefabBuilder.CreatePrefab();
    }
}
