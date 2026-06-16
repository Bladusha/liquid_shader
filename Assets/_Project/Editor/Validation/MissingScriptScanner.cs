using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class MissingScriptScanner
{
    [MenuItem("Tools/LiquidShader/Scan Open Scenes For Missing Scripts")]
    public static void ScanOpenScenes()
    {
        List<string> findings = new List<string>();

        for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
        {
            Scene scene = SceneManager.GetSceneAt(sceneIndex);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                continue;
            }

            foreach (GameObject root in scene.GetRootGameObjects())
            {
                ScanObject(root.transform, scene.path, findings);
            }
        }

        if (findings.Count == 0)
        {
            Debug.Log("MissingScriptScanner: no missing scripts found in open scenes.");
            return;
        }

        foreach (string finding in findings)
        {
            Debug.LogWarning(finding);
        }

        Debug.LogWarning($"MissingScriptScanner: found {findings.Count} objects with missing scripts.");
    }

    private static void ScanObject(Transform root, string scenePath, List<string> findings)
    {
        GameObject go = root.gameObject;
        int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
        if (missing > 0)
        {
            findings.Add($"{scenePath}: {GetHierarchyPath(go.transform)} has {missing} missing script component(s).");
        }

        for (int i = 0; i < root.childCount; i++)
        {
            ScanObject(root.GetChild(i), scenePath, findings);
        }
    }

    private static string GetHierarchyPath(Transform transform)
    {
        List<string> parts = new List<string>();
        while (transform != null)
        {
            parts.Add(transform.name);
            transform = transform.parent;
        }

        parts.Reverse();
        return string.Join("/", parts);
    }
}
