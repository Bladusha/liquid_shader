using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class RealHotkeyHintPrefabBuilder
{
    private const string PrefabPath = "Assets/_Project/Lab/Prefabs/RealHotkeyHints.prefab";
    private const string ExpandedPrefabPath = "Assets/_Project/Lab/Prefabs/RealHotkeyHintsOpen.prefab";
    private const string PromptSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/mini_ui/hotkey_k_show_hints 1.png";
    private const string ExpandedSpritePath = "Assets/_Project/Lab/MenuDesign/HydrodynamicsWaterUiPack/mini_ui/hotkey_corner_menu 2.png";

    [MenuItem("Tools/LiquidShader/Create Real Hotkey Hint Prefabs")]
    public static void CreatePrefab()
    {
        try
        {
            EnsureFolder("Assets/_Project/Lab/Prefabs");

            GameObject root = BuildPrefabHierarchy();
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            GameObject expandedRoot = BuildExpandedPrefabHierarchy();
            GameObject savedExpandedPrefab = PrefabUtility.SaveAsPrefabAsset(expandedRoot, ExpandedPrefabPath);
            Object.DestroyImmediate(expandedRoot);

            if (savedPrefab != null && savedExpandedPrefab != null)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                Debug.Log($"Real hotkey hint prefabs created: {PrefabPath}, {ExpandedPrefabPath}");
            }
            else
            {
                Debug.LogError("Failed to create real hotkey hint prefabs.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to create real hotkey hint prefabs.\n{ex}");
        }
    }

    private static GameObject BuildPrefabHierarchy()
    {
        GameObject root = new GameObject("RealHotkeyHints", typeof(RectTransform), typeof(CanvasRenderer), typeof(RealHotkeyHintView));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 0f);
        rootRect.anchorMax = new Vector2(0f, 0f);
        rootRect.pivot = new Vector2(0f, 0f);
        rootRect.sizeDelta = new Vector2(560f, 680f);

        GameObject promptPanel = CreateImagePanel(root.transform, "PromptPanel", LoadSprite(PromptSpritePath), new Vector2(430f, 134f));
        RectTransform promptRect = promptPanel.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0f, 0f);
        promptRect.anchorMax = new Vector2(0f, 0f);
        promptRect.pivot = new Vector2(0f, 0f);
        promptRect.anchoredPosition = Vector2.zero;

        GameObject detailsPanel = CreateImagePanel(root.transform, "DetailsPanel", LoadSprite(ExpandedSpritePath), new Vector2(560f, 680f));
        RectTransform detailsRect = detailsPanel.GetComponent<RectTransform>();
        detailsRect.anchorMin = new Vector2(0f, 0f);
        detailsRect.anchorMax = new Vector2(0f, 0f);
        detailsRect.pivot = new Vector2(0f, 0f);
        detailsRect.anchoredPosition = Vector2.zero;

        RealHotkeyHintView view = root.GetComponent<RealHotkeyHintView>();
        view.Configure(promptPanel, detailsPanel);
        view.SetExpandedInstant(false);

        return root;
    }

    private static GameObject BuildExpandedPrefabHierarchy()
    {
        GameObject root = CreateImagePanel(null, "RealHotkeyHintsOpen", LoadSprite(ExpandedSpritePath), new Vector2(560f, 680f));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 0f);
        rootRect.anchorMax = new Vector2(0f, 0f);
        rootRect.pivot = new Vector2(0f, 0f);
        rootRect.anchoredPosition = Vector2.zero;
        return root;
    }

    private static GameObject CreateImagePanel(Transform parent, string name, Sprite sprite, Vector2 size)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        if (parent != null)
        {
            panel.transform.SetParent(parent, false);
        }

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        Image image = panel.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.color = Color.white;
        image.raycastTarget = false;
        return panel;
    }

    private static Sprite LoadSprite(string path)
    {
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            throw new FileNotFoundException($"Sprite asset was not found: {path}", path);
        }

        return sprite;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
        string folderName = Path.GetFileName(path);
        if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }

        AssetDatabase.CreateFolder(parent ?? "Assets", folderName);
    }
}

