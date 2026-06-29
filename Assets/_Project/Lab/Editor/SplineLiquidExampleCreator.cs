using UnityEditor;
using UnityEngine;
using UnityEngine.Splines;

public static class SplineLiquidExampleCreator
{
    private const string WaterMaterialPath = "Assets/_Project/Lab/Art/Materials/water.mat";
    private const string DropletMaterialPath = "Assets/_Project/Lab/Art/Materials/water_droplet_particle.mat";

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

    [MenuItem("Tools/LiquidShader/Create Liquid Jet Example")]
    public static void CreateLiquidJetExample()
    {
        Transform selectedOutlet = Selection.activeTransform;
        GameObject root = new GameObject("Liquid Jet Example");
        Undo.RegisterCreatedObjectUndo(root, "Create Liquid Jet Example");

        GameObject outlet = new GameObject("Outlet");
        Undo.RegisterCreatedObjectUndo(outlet, "Create Liquid Jet Outlet");
        outlet.transform.SetParent(root.transform, false);
        if (selectedOutlet != null)
        {
            root.transform.position = selectedOutlet.position;
            root.transform.rotation = selectedOutlet.rotation;
            outlet.transform.localPosition = Vector3.zero;
            outlet.transform.localRotation = Quaternion.identity;
        }
        else
        {
            outlet.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            outlet.transform.localRotation = Quaternion.Euler(8f, 0f, 0f);
        }

        GameObject jet = new GameObject("Generated Jet Mesh");
        Undo.RegisterCreatedObjectUndo(jet, "Create Generated Jet Mesh");
        jet.transform.SetParent(root.transform, false);

        MeshRenderer renderer = jet.AddComponent<MeshRenderer>();
        jet.AddComponent<MeshFilter>();
        LiquidJetMesh liquidJetMesh = jet.AddComponent<LiquidJetMesh>();
        liquidJetMesh.SourceTransform = outlet.transform;
        liquidJetMesh.Length = 1.4f;

        Material waterMaterial = AssetDatabase.LoadAssetAtPath<Material>(WaterMaterialPath);
        if (waterMaterial != null)
        {
            renderer.sharedMaterial = waterMaterial;
        }

        CreateDroplets(outlet.transform, GetOrCreateDropletMaterial());

        Selection.activeGameObject = root;
        EditorGUIUtility.PingObject(root);

        Debug.Log("Created liquid jet example. Move Outlet to the pipe mouth, then tune Generated Jet Mesh and Jet Droplets.");
    }

    [MenuItem("Tools/LiquidShader/Fix Selected Jet Droplet Materials")]
    public static void FixSelectedJetDropletMaterials()
    {
        Material dropletMaterial = GetOrCreateDropletMaterial();
        int fixedCount = 0;
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects != null && selectedObjects.Length > 0)
        {
            foreach (GameObject selected in selectedObjects)
            {
                fixedCount += AssignDropletMaterial(selected.GetComponentsInChildren<ParticleSystemRenderer>(true), dropletMaterial);
            }
        }
        else
        {
            ParticleSystemRenderer[] renderers = Object.FindObjectsByType<ParticleSystemRenderer>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None);
            fixedCount = AssignDropletMaterial(renderers, dropletMaterial);
        }

        if (fixedCount == 0)
        {
            LiquidJetMesh[] jets = FindLiquidJets(selectedObjects);
            foreach (LiquidJetMesh jet in jets)
            {
                Transform parent = jet.SourceTransform != null ? jet.SourceTransform : jet.transform;
                ParticleSystemRenderer createdRenderer = CreateDroplets(parent, dropletMaterial);
                fixedCount += AssignDropletMaterial(new[] { createdRenderer }, dropletMaterial);
            }
        }

        if (fixedCount == 0)
        {
            Debug.LogWarning("No ParticleSystemRenderer or LiquidJetMesh components were found in the selection or open scene. Create a jet first with Tools > LiquidShader > Create Liquid Jet Example.");
            return;
        }

        Debug.Log($"Assigned droplet material using shader '{dropletMaterial.shader.name}' to {fixedCount} particle renderer(s).");
    }

    private static LiquidJetMesh[] FindLiquidJets(GameObject[] selectedObjects)
    {
        if (selectedObjects != null && selectedObjects.Length > 0)
        {
            var jets = new System.Collections.Generic.List<LiquidJetMesh>();
            foreach (GameObject selected in selectedObjects)
            {
                jets.AddRange(selected.GetComponentsInChildren<LiquidJetMesh>(true));
                LiquidJetMesh selectedJet = selected.GetComponentInParent<LiquidJetMesh>();
                if (selectedJet != null && !jets.Contains(selectedJet))
                {
                    jets.Add(selectedJet);
                }
            }

            return jets.ToArray();
        }

        return Object.FindObjectsByType<LiquidJetMesh>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None);
    }

    private static ParticleSystemRenderer CreateDroplets(Transform parent, Material dropletMaterial)
    {
        GameObject droplets = new GameObject("Jet Droplets");
        Undo.RegisterCreatedObjectUndo(droplets, "Create Jet Droplets");
        droplets.transform.SetParent(parent, false);

        ParticleSystem particleSystem = droplets.AddComponent<ParticleSystem>();
        var main = particleSystem.main;
        main.duration = 2f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.75f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 1.7f);
        main.startSize = new ParticleSystem.MinMaxCurve(0.008f, 0.026f);
        main.startColor = new Color(0.62f, 0.9f, 1f, 0.58f);
        main.gravityModifier = 0.55f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particleSystem.emission;
        emission.rateOverTime = 35f;

        var shape = particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 7f;
        shape.radius = 0.035f;

        var velocity = particleSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
        velocity.y = new ParticleSystem.MinMaxCurve(-0.15f, -0.35f);
        velocity.z = new ParticleSystem.MinMaxCurve(0.15f, 0.45f);

        ParticleSystemRenderer particleRenderer = droplets.GetComponent<ParticleSystemRenderer>();
        particleRenderer.sharedMaterial = dropletMaterial;
        particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        particleRenderer.sortMode = ParticleSystemSortMode.Distance;
        EditorUtility.SetDirty(droplets);
        return particleRenderer;
    }

    private static int AssignDropletMaterial(ParticleSystemRenderer[] renderers, Material dropletMaterial)
    {
        int fixedCount = 0;

        foreach (ParticleSystemRenderer renderer in renderers)
        {
            Undo.RecordObject(renderer, "Fix Jet Droplet Material");
            renderer.sharedMaterial = dropletMaterial;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortMode = ParticleSystemSortMode.Distance;
            EditorUtility.SetDirty(renderer);
            fixedCount++;
        }

        return fixedCount;
    }

    private static Material GetOrCreateDropletMaterial()
    {
        Shader shader = Shader.Find("Custom/WaterDropletParticle");
        if (shader == null)
        {
            Debug.LogWarning("Custom/WaterDropletParticle shader is not imported yet. Reimport Assets/_Project/Lab/Art/Shaders/WaterDropletParticle.shader, then run the fixer again.");
            shader = Shader.Find("Universal Render Pipeline/Unlit");
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(DropletMaterialPath);
        if (material != null)
        {
            if (shader != null && material.shader != shader)
            {
                material.shader = shader;
                ConfigureDropletMaterial(material);
                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssets();
            }

            return material;
        }

        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        material = new Material(shader)
        {
            name = "water_droplet_particle"
        };

        ConfigureDropletMaterial(material);
        AssetDatabase.CreateAsset(material, DropletMaterialPath);
        AssetDatabase.SaveAssets();
        return material;
    }

    private static void ConfigureDropletMaterial(Material material)
    {
        Color dropletColor = new Color(0.62f, 0.9f, 1f, 0.58f);
        SetColorIfPresent(material, "_BaseColor", dropletColor);
        SetColorIfPresent(material, "_Color", dropletColor);
        SetFloatIfPresent(material, "_Softness", 0.18f);
        SetFloatIfPresent(material, "_Surface", 1f);
        SetFloatIfPresent(material, "_Blend", 0f);
        SetFloatIfPresent(material, "_ZWrite", 0f);
        SetFloatIfPresent(material, "_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        SetFloatIfPresent(material, "_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
    }

    private static void SetColorIfPresent(Material material, string propertyName, Color value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetColor(propertyName, value);
        }
    }

    private static void SetFloatIfPresent(Material material, string propertyName, float value)
    {
        if (material.HasProperty(propertyName))
        {
            material.SetFloat(propertyName, value);
        }
    }
}
