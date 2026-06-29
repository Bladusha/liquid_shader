using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public sealed class LiquidJetMesh : MonoBehaviour
{
    [Header("Source")]
    [SerializeField] private Transform sourceTransform;
    [SerializeField] private Vector3 localDirection = Vector3.forward;
    [SerializeField] private Vector3 localUp = Vector3.up;

    [Header("Shape")]
    [SerializeField, Min(0.001f)] private float length = 1.2f;
    [SerializeField, Min(0.001f)] private float startRadius = 0.045f;
    [SerializeField, Min(0.001f)] private float endRadius = 0.012f;
    [SerializeField, Range(3, 32)] private int radialSegments = 10;
    [SerializeField, Range(1, 128)] private int lengthSegments = 32;
    [SerializeField] private bool capStart = true;
    [SerializeField] private bool capEnd;

    [Header("Flow Path")]
    [SerializeField, Min(0f)] private float gravityDrop = 0.35f;
    [SerializeField, Min(0f)] private float sideDrift = 0.04f;
    [SerializeField, Min(0f)] private float turbulenceAmplitude = 0.015f;
    [SerializeField, Min(0.01f)] private float turbulenceFrequency = 4f;
    [SerializeField] private float turbulenceSeed = 1.37f;
    [SerializeField] private AnimationCurve bendDistribution = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private AnimationCurve radiusDistribution = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Animation")]
    [SerializeField] private bool animateJet = true;
    [SerializeField] private bool animateInEditMode = true;
    [SerializeField, Min(0f)] private float waveAmplitude = 0.035f;
    [SerializeField, Min(0.01f)] private float waveFrequency = 2.25f;
    [SerializeField] private float waveSpeed = 1.8f;
    [SerializeField, Range(0f, 0.5f)] private float pulseAmount = 0.08f;
    [SerializeField, Min(0.01f)] private float pulseFrequency = 3.2f;
    [SerializeField] private float pulseSpeed = 2.4f;
    [SerializeField, Range(0f, 0.5f)] private float surfaceWobbleAmount = 0.11f;
    [SerializeField] private float surfaceWobbleSpeed = 3.5f;
    [SerializeField] private AnimationCurve animationFade = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Texture Distribution")]
    [SerializeField] private AnimationCurve textureVDistribution = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField, Min(0.001f)] private float textureVTiling = 1f;
    [SerializeField] private float textureVOffset;

    [Header("Material Overrides")]
    [SerializeField] private bool forceFullFill = true;
    [SerializeField, Range(0f, 5f)] private float flowSpeed = 1.6f;
    [SerializeField, Range(0f, 1f)] private float surfaceOscillation = 0.65f;
    [SerializeField, Range(0f, 1f)] private float bubbleAmount = 0.18f;
    [SerializeField, Range(0f, 1f)] private float alpha = 0.68f;

    [Header("Activation")]
    [SerializeField] private WaterController fillController;
    [SerializeField] private bool autoDiscoverFillController = true;
    [SerializeField] private bool requireFullPipeFilled = true;
    [SerializeField] private bool showWithoutFillController = true;
    [SerializeField, Range(0f, 1f)] private float fullPipeThreshold = 0.995f;
    [SerializeField, Min(0.01f)] private float appearSpeed = 3.5f;
    [SerializeField, Min(0.01f)] private float disappearSpeed = 5f;
    [SerializeField] private ParticleSystem[] controlledParticles;
    [SerializeField] private bool autoDiscoverParticles = true;
    [SerializeField, Min(0f)] private float particleRateAtFullFlow = 35f;

    [Header("Editor")]
    [SerializeField] private bool rebuildInEditMode = true;

    private static readonly int FillLevelId = Shader.PropertyToID("_FillLevel");
    private static readonly int FlowSpeedId = Shader.PropertyToID("_FlowSpeed");
    private static readonly int SurfaceOscillationId = Shader.PropertyToID("_SurfaceOscillation");
    private static readonly int BubbleAmountId = Shader.PropertyToID("_BubbleAmount");
    private static readonly int AlphaId = Shader.PropertyToID("_Alpha");

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh generatedMesh;
    private MaterialPropertyBlock propertyBlock;
    private bool rebuildQueued;
    private double editorStartTime;
    private float visibility = 1f;

    public Transform SourceTransform
    {
        get => sourceTransform;
        set
        {
            sourceTransform = value;
            RebuildMesh();
        }
    }

    public float Length
    {
        get => length;
        set
        {
            length = Mathf.Max(0.001f, value);
            RebuildMesh();
        }
    }

    private void OnEnable()
    {
#if UNITY_EDITOR
        editorStartTime = UnityEditor.EditorApplication.timeSinceStartup;
        UnityEditor.EditorApplication.update -= EditorTick;
        UnityEditor.EditorApplication.update += EditorTick;
#endif
        EnsureComponents();
        ResolveFillController();
        ResolveControlledParticles();
        visibility = GetTargetVisibility();
        RebuildMesh();
        ApplyMaterialOverrides();
        ApplyParticleVisibility();
    }

    private void OnDisable()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.update -= EditorTick;
#endif

        if (generatedMesh == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(generatedMesh);
        }
        else
        {
            DestroyImmediate(generatedMesh);
        }

        generatedMesh = null;
    }

    private void OnValidate()
    {
        length = Mathf.Max(0.001f, length);
        startRadius = Mathf.Max(0.001f, startRadius);
        endRadius = Mathf.Max(0.001f, endRadius);
        radialSegments = Mathf.Clamp(radialSegments, 3, 32);
        lengthSegments = Mathf.Clamp(lengthSegments, 1, 128);
        textureVTiling = Mathf.Max(0.001f, textureVTiling);
        turbulenceFrequency = Mathf.Max(0.01f, turbulenceFrequency);
        waveFrequency = Mathf.Max(0.01f, waveFrequency);
        pulseFrequency = Mathf.Max(0.01f, pulseFrequency);
        appearSpeed = Mathf.Max(0.01f, appearSpeed);
        disappearSpeed = Mathf.Max(0.01f, disappearSpeed);
        EnsureCurves();

        if (!Application.isPlaying && !rebuildInEditMode)
        {
            return;
        }

        QueueRebuild();
        ApplyMaterialOverrides();
        ApplyParticleVisibility();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying && !rebuildInEditMode)
        {
            return;
        }

        if (sourceTransform != null && sourceTransform.hasChanged)
        {
            sourceTransform.hasChanged = false;
            QueueRebuild();
        }

        if (transform.hasChanged)
        {
            transform.hasChanged = false;
            QueueRebuild();
        }

        float previousVisibility = visibility;
        UpdateVisibility();
        if (!Mathf.Approximately(previousVisibility, visibility))
        {
            QueueRebuild();
        }

        if (ShouldAnimate())
        {
            QueueRebuild();
        }

        if (rebuildQueued)
        {
            rebuildQueued = false;
            RebuildMesh();
        }

        ApplyMaterialOverrides();
        ApplyParticleVisibility();
    }

    [ContextMenu("Rebuild Jet Mesh")]
    public void RebuildMesh()
    {
        EnsureComponents();
        EnsureCurves();
        float time = GetAnimationTime();

        int ringVertexCount = radialSegments + 1;
        int ringCount = lengthSegments + 1;
        int sideVertexCount = ringVertexCount * ringCount;
        int capVertexCount = (capStart ? 1 : 0) + (capEnd ? 1 : 0);
        int vertexCount = sideVertexCount + capVertexCount;
        int sideIndexCount = lengthSegments * radialSegments * 6;
        int capIndexCount = (capStart ? radialSegments * 3 : 0) + (capEnd ? radialSegments * 3 : 0);

        var vertices = new Vector3[vertexCount];
        var normals = new Vector3[vertexCount];
        var uvs = new Vector2[vertexCount];
        var triangles = new int[sideIndexCount + capIndexCount];

        GetSourceFrame(out Vector3 origin, out Vector3 forward, out Vector3 right, out Vector3 up);

        for (int ring = 0; ring < ringCount; ring++)
        {
            float t = ring / (float)lengthSegments;
            Vector3 center = EvaluateCenter(origin, forward, right, up, t, time);
            float radius = EvaluateRadius(t, time) * visibility;
            float textureV = EvaluateTextureV(t);

            for (int radial = 0; radial < ringVertexCount; radial++)
            {
                float u = radial / (float)radialSegments;
                float angle = u * Mathf.PI * 2f;
                float wobble = EvaluateSurfaceWobble(t, u, time);
                Vector3 radialNormal = right * Mathf.Cos(angle) + up * Mathf.Sin(angle);
                int vertexIndex = ring * ringVertexCount + radial;

                vertices[vertexIndex] = transform.InverseTransformPoint(center + radialNormal * radius * wobble);
                normals[vertexIndex] = transform.InverseTransformDirection(radialNormal).normalized;
                uvs[vertexIndex] = new Vector2(u, textureV);
            }
        }

        int triangleIndex = 0;
        for (int ring = 0; ring < lengthSegments; ring++)
        {
            int current = ring * ringVertexCount;
            int next = (ring + 1) * ringVertexCount;

            for (int radial = 0; radial < radialSegments; radial++)
            {
                triangles[triangleIndex++] = current + radial;
                triangles[triangleIndex++] = next + radial;
                triangles[triangleIndex++] = current + radial + 1;

                triangles[triangleIndex++] = current + radial + 1;
                triangles[triangleIndex++] = next + radial;
                triangles[triangleIndex++] = next + radial + 1;
            }
        }

        if (capStart)
        {
            int centerIndex = sideVertexCount;
            vertices[centerIndex] = transform.InverseTransformPoint(EvaluateCenter(origin, forward, right, up, 0f, time));
            normals[centerIndex] = transform.InverseTransformDirection(-forward).normalized;
            uvs[centerIndex] = new Vector2(0.5f, EvaluateTextureV(0f));

            for (int radial = 0; radial < radialSegments; radial++)
            {
                triangles[triangleIndex++] = centerIndex;
                triangles[triangleIndex++] = radial + 1;
                triangles[triangleIndex++] = radial;
            }
        }

        if (capEnd)
        {
            int centerIndex = sideVertexCount + (capStart ? 1 : 0);
            int lastRingStart = lengthSegments * ringVertexCount;
            Vector3 endCenter = EvaluateCenter(origin, forward, right, up, 1f, time);
            Vector3 endForward = (endCenter - EvaluateCenter(origin, forward, right, up, 0.98f, time)).normalized;

            vertices[centerIndex] = transform.InverseTransformPoint(endCenter);
            normals[centerIndex] = transform.InverseTransformDirection(endForward).normalized;
            uvs[centerIndex] = new Vector2(0.5f, EvaluateTextureV(1f));

            for (int radial = 0; radial < radialSegments; radial++)
            {
                triangles[triangleIndex++] = centerIndex;
                triangles[triangleIndex++] = lastRingStart + radial;
                triangles[triangleIndex++] = lastRingStart + radial + 1;
            }
        }

        generatedMesh.Clear();
        generatedMesh.name = "Generated Liquid Jet Mesh";
        generatedMesh.vertices = vertices;
        generatedMesh.normals = normals;
        generatedMesh.uv = uvs;
        generatedMesh.triangles = triangles;
        generatedMesh.RecalculateBounds();
    }

    private Vector3 EvaluateCenter(Vector3 origin, Vector3 forward, Vector3 right, Vector3 up, float t, float time)
    {
        float bend = bendDistribution.Evaluate(Mathf.Clamp01(t));
        float noiseFade = Mathf.SmoothStep(0f, 1f, t);
        float sideNoise = Mathf.Sin((t * turbulenceFrequency + turbulenceSeed) * Mathf.PI * 2f) * turbulenceAmplitude * noiseFade;
        float verticalNoise = Mathf.Cos((t * turbulenceFrequency * 1.37f + turbulenceSeed) * Mathf.PI * 2f) * turbulenceAmplitude * 0.45f * noiseFade;
        float animatedFade = ShouldAnimate() ? Mathf.Clamp01(animationFade.Evaluate(Mathf.Clamp01(t))) : 0f;
        float wavePhase = t * waveFrequency * Mathf.PI * 2f + time * waveSpeed + turbulenceSeed;
        float sideWave = Mathf.Sin(wavePhase) * waveAmplitude * animatedFade;
        float verticalWave = Mathf.Cos(wavePhase * 1.31f + 0.7f) * waveAmplitude * 0.55f * animatedFade;

        return origin
            + forward * (length * t)
            - up * (gravityDrop * bend * t)
            + right * (sideDrift * bend + sideNoise + sideWave)
            + up * (verticalNoise + verticalWave);
    }

    private float EvaluateRadius(float t, float time)
    {
        float distribution = Mathf.Clamp01(radiusDistribution.Evaluate(Mathf.Clamp01(t)));
        float radius = Mathf.Lerp(endRadius, startRadius, distribution);

        if (!ShouldAnimate())
        {
            return radius;
        }

        float fade = Mathf.Clamp01(animationFade.Evaluate(Mathf.Clamp01(t)));
        float pulsePhase = t * pulseFrequency * Mathf.PI * 2f - time * pulseSpeed + turbulenceSeed;
        float pulse = 1f + Mathf.Sin(pulsePhase) * pulseAmount * fade;
        return Mathf.Max(0.001f, radius * pulse);
    }

    private float EvaluateSurfaceWobble(float t, float u, float time)
    {
        float staticWobble = Mathf.Sin((t * turbulenceFrequency + u + turbulenceSeed) * Mathf.PI * 2f) * 0.08f * Mathf.Clamp01(turbulenceAmplitude * 12f);

        if (!ShouldAnimate())
        {
            return 1f + staticWobble;
        }

        float fade = Mathf.Clamp01(animationFade.Evaluate(Mathf.Clamp01(t)));
        float animatedWobble = Mathf.Sin((t * waveFrequency + u * 2.3f + turbulenceSeed) * Mathf.PI * 2f + time * surfaceWobbleSpeed)
            * surfaceWobbleAmount
            * fade;

        return Mathf.Max(0.2f, 1f + staticWobble + animatedWobble);
    }

    private float EvaluateTextureV(float t)
    {
        float remapped = textureVDistribution.Evaluate(Mathf.Clamp01(t));
        return remapped * textureVTiling + textureVOffset;
    }

    private void GetSourceFrame(out Vector3 origin, out Vector3 forward, out Vector3 right, out Vector3 up)
    {
        Transform source = sourceTransform != null ? sourceTransform : transform;
        origin = source.position;
        forward = source.TransformDirection(localDirection).normalized;
        up = source.TransformDirection(localUp).normalized;

        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = source.forward;
        }

        if (up.sqrMagnitude < 0.0001f || Mathf.Abs(Vector3.Dot(forward, up)) > 0.98f)
        {
            up = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.98f ? Vector3.right : Vector3.up;
        }

        right = Vector3.Cross(up, forward).normalized;
        up = Vector3.Cross(forward, right).normalized;
    }

    private void ApplyMaterialOverrides()
    {
        EnsureComponents();

        if (meshRenderer == null)
        {
            return;
        }

        if (propertyBlock == null)
        {
            propertyBlock = new MaterialPropertyBlock();
        }
        meshRenderer.GetPropertyBlock(propertyBlock);

        if (forceFullFill)
        {
            propertyBlock.SetFloat(FillLevelId, 1f);
        }

        propertyBlock.SetFloat(FlowSpeedId, flowSpeed);
        propertyBlock.SetFloat(SurfaceOscillationId, surfaceOscillation);
        propertyBlock.SetFloat(BubbleAmountId, bubbleAmount);
        propertyBlock.SetFloat(AlphaId, alpha * visibility);
        meshRenderer.SetPropertyBlock(propertyBlock);

        meshRenderer.enabled = visibility > 0.001f;
    }

    private void UpdateVisibility()
    {
        float targetVisibility = GetTargetVisibility();
        float speed = targetVisibility > visibility ? appearSpeed : disappearSpeed;
        float deltaTime = Application.isPlaying ? Time.deltaTime : 1f / 60f;
        visibility = Mathf.MoveTowards(visibility, targetVisibility, speed * deltaTime);
    }

    private float GetTargetVisibility()
    {
        if (!requireFullPipeFilled)
        {
            return 1f;
        }

        ResolveFillController();
        if (fillController == null)
        {
            return showWithoutFillController ? 1f : 0f;
        }

        return fillController.CurrentMeasurements.fillFraction >= fullPipeThreshold ? 1f : 0f;
    }

    private void ResolveFillController()
    {
        if (fillController != null || !autoDiscoverFillController)
        {
            return;
        }

        fillController = FindFirstObjectByType<WaterController>(FindObjectsInactive.Include);
    }

    private void ApplyParticleVisibility()
    {
        ResolveControlledParticles();

        if (controlledParticles == null)
        {
            controlledParticles = new ParticleSystem[0];
        }

        for (int i = 0; i < controlledParticles.Length; i++)
        {
            ParticleSystem particleSystem = controlledParticles[i];
            if (particleSystem == null)
            {
                continue;
            }

            var emission = particleSystem.emission;
            emission.rateOverTime = particleRateAtFullFlow * visibility;

            if (visibility > 0.001f)
            {
                if (!particleSystem.isPlaying)
                {
                    particleSystem.Play();
                }
            }
            else if (particleSystem.isPlaying)
            {
                particleSystem.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    private void ResolveControlledParticles()
    {
        if (!autoDiscoverParticles || (controlledParticles != null && controlledParticles.Length > 0))
        {
            return;
        }

        if (sourceTransform != null)
        {
            controlledParticles = sourceTransform.GetComponentsInChildren<ParticleSystem>(true);
            if (controlledParticles.Length > 0)
            {
                return;
            }
        }

        controlledParticles = GetComponentsInChildren<ParticleSystem>(true);
        if (controlledParticles == null)
        {
            controlledParticles = new ParticleSystem[0];
        }
    }

    private void QueueRebuild()
    {
        rebuildQueued = true;
    }

    private void EnsureCurves()
    {
        if (bendDistribution == null || bendDistribution.length == 0)
        {
            bendDistribution = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        if (radiusDistribution == null || radiusDistribution.length == 0)
        {
            radiusDistribution = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        }

        if (textureVDistribution == null || textureVDistribution.length == 0)
        {
            textureVDistribution = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }

        if (animationFade == null || animationFade.length == 0)
        {
            animationFade = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }
    }

    private bool ShouldAnimate()
    {
        return animateJet && (Application.isPlaying || animateInEditMode);
    }

    private float GetAnimationTime()
    {
        if (!ShouldAnimate())
        {
            return 0f;
        }

        if (Application.isPlaying)
        {
            return Time.time;
        }

#if UNITY_EDITOR
        return (float)(UnityEditor.EditorApplication.timeSinceStartup - editorStartTime);
#else
        return 0f;
#endif
    }

#if UNITY_EDITOR
    private void EditorTick()
    {
        if (Application.isPlaying || !isActiveAndEnabled || !animateJet || !animateInEditMode)
        {
            return;
        }

        UpdateVisibility();
        RebuildMesh();
        ApplyMaterialOverrides();
        ApplyParticleVisibility();
        UnityEditor.SceneView.RepaintAll();
    }
#endif

    private void EnsureComponents()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        if (generatedMesh != null)
        {
            if (meshFilter.sharedMesh != generatedMesh)
            {
                meshFilter.sharedMesh = generatedMesh;
            }

            return;
        }

        generatedMesh = new Mesh
        {
            name = "Generated Liquid Jet Mesh"
        };
        generatedMesh.MarkDynamic();
        meshFilter.sharedMesh = generatedMesh;
    }
}
