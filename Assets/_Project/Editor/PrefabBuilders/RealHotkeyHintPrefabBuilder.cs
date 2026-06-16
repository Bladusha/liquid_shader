using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class RealHotkeyHintPrefabBuilder
{
    private const string PrefabPath = "Assets/_Project/Lab01/Prefabs/RealHotkeyHints.prefab";

    [MenuItem("Tools/LiquidShader/Create Real Hotkey Hint Prefab")]
    public static void CreatePrefab()
    {
        try
        {
            EnsureFolder("Assets/_Project/Lab01/Prefabs");

            GameObject root = BuildPrefabHierarchy();
            GameObject savedPrefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);

            if (savedPrefab != null)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
                Debug.Log($"Real hotkey hint prefab created: {PrefabPath}");
            }
            else
            {
                Debug.LogError("Failed to create real hotkey hint prefab.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to create real hotkey hint prefab.\n{ex}");
        }
    }

    private static GameObject BuildPrefabHierarchy()
    {
        GameObject root = new GameObject("RealHotkeyHints", typeof(RectTransform), typeof(CanvasRenderer), typeof(RealHotkeyHintView));
        RectTransform rootRect = root.GetComponent<RectTransform>();
        rootRect.anchorMin = new Vector2(0f, 0f);
        rootRect.anchorMax = new Vector2(0f, 0f);
        rootRect.pivot = new Vector2(0f, 0f);
        rootRect.sizeDelta = new Vector2(420f, 156f);

        GameObject promptPanel = CreatePanel(root.transform, "PromptPanel", new Vector2(360f, 34f), new Color(0.08f, 0.09f, 0.12f, 0.82f));
        RectTransform promptRect = promptPanel.GetComponent<RectTransform>();
        promptRect.anchorMin = new Vector2(0f, 0f);
        promptRect.anchorMax = new Vector2(0f, 0f);
        promptRect.pivot = new Vector2(0f, 0f);
        promptRect.anchoredPosition = new Vector2(0f, 0f);

        TMP_Text promptLabel = CreateText(promptPanel.transform, "PromptLabel", "Press K for more hotkeys", 18f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, Color.white);

        GameObject detailsPanel = CreatePanel(root.transform, "DetailsPanel", new Vector2(360f, 102f), new Color(0.08f, 0.09f, 0.12f, 0.92f));
        RectTransform detailsRect = detailsPanel.GetComponent<RectTransform>();
        detailsRect.anchorMin = new Vector2(0f, 0f);
        detailsRect.anchorMax = new Vector2(0f, 0f);
        detailsRect.pivot = new Vector2(0f, 0f);
        detailsRect.anchoredPosition = new Vector2(0f, 40f);

        TMP_Text detailsLabel = CreateText(detailsPanel.transform, "DetailsLabel", "TAB - Pause", 20f, FontStyles.Normal, TextAlignmentOptions.TopLeft, Color.white);

        RealHotkeyHintView view = root.GetComponent<RealHotkeyHintView>();
        view.Configure(promptLabel, detailsPanel, detailsLabel);
        view.SetPrompt("Press K for more hotkeys");
        view.SetHotkeyLines(new[] { "TAB - Pause" });
        view.SetExpanded(false);

        return root;
    }

    private static GameObject CreatePanel(Transform parent, string name, Vector2 size, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = size;

        Image image = panel.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return panel;
    }

    private static TMP_Text CreateText(Transform parent, string name, string text, float fontSize, FontStyles fontStyle, TextAlignmentOptions alignment, Color color)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(12f, 6f);
        rect.offsetMax = new Vector2(-12f, -6f);

        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = fontSize;
        label.fontStyle = fontStyle;
        label.alignment = alignment;
        label.color = color;
        label.raycastTarget = false;

        if (TMP_Settings.defaultFontAsset != null)
        {
            label.font = TMP_Settings.defaultFontAsset;
        }

        return label;
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
