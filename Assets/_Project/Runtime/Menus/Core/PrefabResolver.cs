using UnityEngine;

public static class PrefabResolver
{
    private const string LegacyLabPrefabFolder = "Assets/_Project/Lab01/Prefabs/";
    private const string CurrentLabPrefabFolder = "Assets/_Project/Lab/Prefabs/";

    public static GameObject Load(string editorAssetPath, string resourcesPath)
    {
#if UNITY_EDITOR
        if (!string.IsNullOrWhiteSpace(editorAssetPath))
        {
            GameObject editorPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(editorAssetPath);
            if (editorPrefab != null)
            {
                return editorPrefab;
            }

            string migratedPath = GetMigratedEditorPath(editorAssetPath);
            if (migratedPath != editorAssetPath)
            {
                editorPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(migratedPath);
                if (editorPrefab != null)
                {
                    return editorPrefab;
                }
            }

            editorPrefab = FindEditorPrefabByName(editorAssetPath);
            if (editorPrefab != null)
            {
                return editorPrefab;
            }
        }
#endif

        if (!string.IsNullOrWhiteSpace(resourcesPath))
        {
            return Resources.Load<GameObject>(resourcesPath);
        }

        return null;
    }

#if UNITY_EDITOR
    private static string GetMigratedEditorPath(string editorAssetPath)
    {
        return editorAssetPath.StartsWith(LegacyLabPrefabFolder)
            ? CurrentLabPrefabFolder + editorAssetPath.Substring(LegacyLabPrefabFolder.Length)
            : editorAssetPath;
    }

    private static GameObject FindEditorPrefabByName(string editorAssetPath)
    {
        string prefabName = System.IO.Path.GetFileNameWithoutExtension(editorAssetPath);
        if (string.IsNullOrWhiteSpace(prefabName))
        {
            return null;
        }

        string[] guids = UnityEditor.AssetDatabase.FindAssets($"{prefabName} t:Prefab", new[] { CurrentLabPrefabFolder.TrimEnd('/') });
        foreach (string guid in guids)
        {
            string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(assetPath) != prefabName)
            {
                continue;
            }

            GameObject prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab != null)
            {
                return prefab;
            }
        }

        return null;
    }
#endif
}
