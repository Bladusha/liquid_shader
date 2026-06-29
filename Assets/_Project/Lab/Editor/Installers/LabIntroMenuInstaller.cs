using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class LabIntroMenuInstaller
{
    private const string ScenePath = "Assets/_Project/Lab/LabScenes/real.unity";
    private const string RootObjectName = "LabIntroMenuBootstrap";
    private const string PrefabPath = "Assets/_Project/Lab/Prefabs/LabIntroMenu.prefab";

    [MenuItem("Tools/LiquidShader/Install Lab Intro Menu Into Real Scene")]
    public static void InstallIntoRealScene()
    {
        if (!File.Exists(ScenePath))
        {
            Debug.LogError($"Scene not found: {ScenePath}");
            return;
        }

        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        GameObject root = GameObject.Find(RootObjectName);

        if (root == null)
        {
            root = new GameObject(RootObjectName);
            Undo.RegisterCreatedObjectUndo(root, "Create Lab Intro Menu");
        }

        LabIntroMenuBootstrap bootstrap = root.GetComponent<LabIntroMenuBootstrap>();
        if (bootstrap == null)
        {
            bootstrap = Undo.AddComponent<LabIntroMenuBootstrap>(root);
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        if (prefab != null)
        {
            SerializedObject serialized = new SerializedObject(bootstrap);
            serialized.FindProperty("menuPrefab").objectReferenceValue = prefab;
            serialized.FindProperty("videoFileName").stringValue = "LabIntro.mp4";
            serialized.FindProperty("spawnOnStart").boolValue = true;
            serialized.FindProperty("openOnSpawn").boolValue = true;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        EditorUtility.SetDirty(root);
        EditorUtility.SetDirty(bootstrap);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);
        Debug.Log("Lab intro menu installed into real.unity.");
    }
}
