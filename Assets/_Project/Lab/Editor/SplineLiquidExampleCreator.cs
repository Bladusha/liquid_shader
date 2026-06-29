using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public static class SplineLiquidExampleCreator
{
    private const string WaterMaterialPath = "Assets/_Project/Lab/Art/Materials/water.mat";

    [MenuItem("Tools/LiquidShader/Create Spline Liquid Example")]
    public static void CreateSplineLiquidExample()
    {
        GameObject root = new GameObject("Spline Liquid Example");
        Undo.RegisterCreatedObjectUndo(root, "Create Spline Liquid Example");

        SplineContainer splineContainer = root.AddComponent<SplineContainer>();
        splineContainer.Spline.Clear();
        splineContainer.Spline.Add(new BezierKnot(new Vector3(-1.5f, 1.0f, 0.0f)), TangentMode.AutoSmooth);
        splineContainer.Spline.Add(new BezierKnot(new Vector3(-0.5f, 1.25f, 0.75f)), TangentMode.AutoSmooth);
        splineContainer.Spline.Add(new BezierKnot(new Vector3(0.45f, 0.85f, 0.65f)), TangentMode.AutoSmooth);
        splineContainer.Spline.Add(new BezierKnot(new Vector3(1.5f, 1.1f, 0.0f)), TangentMode.AutoSmooth);

        GameObject liquid = new GameObject("Generated Liquid Mesh");
        Undo.RegisterCreatedObjectUndo(liquid, "Create Generated Liquid Mesh");
        liquid.transform.SetParent(root.transform, false);

        MeshRenderer renderer = liquid.AddComponent<MeshRenderer>();
        liquid.AddComponent<MeshFilter>();
        LiquidSplineMesh liquidSplineMesh = liquid.AddComponent<LiquidSplineMesh>();
        liquidSplineMesh.SplineContainer = splineContainer;
        liquidSplineMesh.Radius = 0.08f;

        Material waterMaterial = AssetDatabase.LoadAssetAtPath<Material>(WaterMaterialPath);
        if (waterMaterial != null)
        {
            renderer.sharedMaterial = waterMaterial;
        }

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);

        Debug.Log("Created spline liquid example. Move the spline knots to match the real pipe path; the generated mesh keeps uv.y as 0..1 along the spline.");

    }
}
