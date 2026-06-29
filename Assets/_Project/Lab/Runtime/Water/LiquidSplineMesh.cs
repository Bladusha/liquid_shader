using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public sealed class LiquidSplineMesh : MonoBehaviour
{
    [Header("Spline")]
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField, Min(0)] private int splineIndex;

    [Header("Shape")]
    [SerializeField, Min(0.001f)] private float radius = 0.035f;
    [SerializeField, Min(0.001f)] private float verticalRadiusScale = 1f;
    [SerializeField, Range(3, 32)] private int radialSegments = 12;
    [SerializeField, Range(1, 256)] private int lengthSegments = 64;
    [SerializeField] private bool capEnds = true;

    [Header("Texture Distribution")]
    [Tooltip("Samples rings by real spline length instead of raw spline time. Keeps the liquid density visually even on uneven knots and bends.")]
    [SerializeField] private bool sampleByArcLength = true;
    [Tooltip("Higher values make arc-length sampling more accurate on tight curves.")]
    [SerializeField, Range(8, 1024)] private int arcLengthLookupSamples = 256;
    [Tooltip("Graph remapping from normalized pipe length to uv.y. Keep it mostly increasing from 0 to 1 when this material uses uv.y for FillLevel.")]
    [SerializeField] private AnimationCurve textureVDistribution = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [Tooltip("Multiplies the graph output. Keep at 1 when uv.y also controls FillLevel.")]
    [SerializeField, Min(0.001f)] private float textureVTiling = 1f;
    [Tooltip("Offsets the graph output. Keep at 0 when uv.y also controls FillLevel.")]
    [SerializeField] private float textureVOffset;

    [Header("Editor")]
    [SerializeField] private bool rebuildInEditMode = true;

    private MeshFilter meshFilter;
    private Mesh generatedMesh;
    private SplineContainer observedSplineContainer;
    private bool rebuildQueued;

    public SplineContainer SplineContainer
    {
        get => splineContainer;
        set
        {
            splineContainer = value;
            if (isActiveAndEnabled)
            {
                SubscribeToSplineChanges();
            }

            RebuildMesh();
        }
    }

    public float Radius
    {
        get => radius;
        set
        {
            radius = Mathf.Max(0.001f, value);
            RebuildMesh();
        }
    }

    private void OnEnable()
    {
        EnsureMesh();
        if (isActiveAndEnabled)
        {
            SubscribeToSplineChanges();
        }
        RebuildMesh();
    }

    private void OnDisable()
    {
        UnsubscribeFromSplineChanges();

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
        radius = Mathf.Max(0.001f, radius);
        verticalRadiusScale = Mathf.Max(0.001f, verticalRadiusScale);
        radialSegments = Mathf.Clamp(radialSegments, 3, 32);
        lengthSegments = Mathf.Clamp(lengthSegments, 1, 256);
        arcLengthLookupSamples = Mathf.Clamp(arcLengthLookupSamples, 8, 1024);
        textureVTiling = Mathf.Max(0.001f, textureVTiling);
        splineIndex = Mathf.Max(0, splineIndex);
        EnsureTextureCurve();

        if (isActiveAndEnabled)
        {
            SubscribeToSplineChanges();
        }

        if (!Application.isPlaying && !rebuildInEditMode)
        {
            return;
        }

        QueueRebuild();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying && !rebuildInEditMode)
        {
            return;
        }

        if (splineContainer != null && splineContainer.transform.hasChanged)
        {
            splineContainer.transform.hasChanged = false;
            QueueRebuild();
        }

        if (transform.hasChanged)
        {
            transform.hasChanged = false;
            QueueRebuild();
        }

        if (!rebuildQueued)
        {
            return;
        }

        rebuildQueued = false;
        RebuildMesh();
    }

    [ContextMenu("Rebuild Liquid Mesh")]
    public void RebuildMesh()
    {
        EnsureMesh();

        if (generatedMesh == null || splineContainer == null || splineContainer.Splines.Count == 0)
        {
            return;
        }

        int resolvedSplineIndex = Mathf.Clamp(splineIndex, 0, splineContainer.Splines.Count - 1);
        int ringVertexCount = radialSegments + 1;
        int ringCount = lengthSegments + 1;
        int sideVertexCount = ringVertexCount * ringCount;
        int capVertexCount = capEnds ? 2 : 0;
        int vertexCount = sideVertexCount + capVertexCount;
        int sideIndexCount = lengthSegments * radialSegments * 6;
        int capIndexCount = capEnds ? radialSegments * 6 : 0;

        var vertices = new Vector3[vertexCount];
        var normals = new Vector3[vertexCount];
        var uvs = new Vector2[vertexCount];
        var triangles = new int[sideIndexCount + capIndexCount];
        float[] lookupT = null;
        float[] lookupDistance = null;
        float totalLength = 0f;

        if (sampleByArcLength)
        {
            BuildArcLengthLookup(resolvedSplineIndex, out lookupT, out lookupDistance, out totalLength);
        }

        for (int ring = 0; ring < ringCount; ring++)
        {
            float pathPercent = ring / (float)lengthSegments;
            float t = sampleByArcLength
                ? FindSplineTAtNormalizedDistance(pathPercent, lookupT, lookupDistance, totalLength)
                : pathPercent;
            float textureV = EvaluateTextureV(pathPercent);

            SampleFrame(resolvedSplineIndex, t, out Vector3 center, out Vector3 forward, out Vector3 right, out Vector3 up);

            for (int radial = 0; radial < ringVertexCount; radial++)
            {
                float u = radial / (float)radialSegments;
                float angle = u * Mathf.PI * 2f;
                Vector3 radialNormal = right * Mathf.Cos(angle) + up * Mathf.Sin(angle);
                Vector3 offset = right * (Mathf.Cos(angle) * radius) + up * (Mathf.Sin(angle) * radius * verticalRadiusScale);
                int vertexIndex = ring * ringVertexCount + radial;

                vertices[vertexIndex] = transform.InverseTransformPoint(center + offset);
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

        if (capEnds)
        {
            int startCenterIndex = sideVertexCount;
            int endCenterIndex = sideVertexCount + 1;
            SampleFrame(resolvedSplineIndex, 0f, out Vector3 startCenter, out Vector3 startForward, out _, out _);
            SampleFrame(resolvedSplineIndex, 1f, out Vector3 endCenter, out Vector3 endForward, out _, out _);

            vertices[startCenterIndex] = transform.InverseTransformPoint(startCenter);
            vertices[endCenterIndex] = transform.InverseTransformPoint(endCenter);
            normals[startCenterIndex] = transform.InverseTransformDirection(-startForward).normalized;
            normals[endCenterIndex] = transform.InverseTransformDirection(endForward).normalized;
            uvs[startCenterIndex] = new Vector2(0.5f, EvaluateTextureV(0f));
            uvs[endCenterIndex] = new Vector2(0.5f, EvaluateTextureV(1f));

            int lastRingStart = lengthSegments * ringVertexCount;
            for (int radial = 0; radial < radialSegments; radial++)
            {
                triangles[triangleIndex++] = startCenterIndex;
                triangles[triangleIndex++] = radial + 1;
                triangles[triangleIndex++] = radial;

                triangles[triangleIndex++] = endCenterIndex;
                triangles[triangleIndex++] = lastRingStart + radial;
                triangles[triangleIndex++] = lastRingStart + radial + 1;
            }
        }

        generatedMesh.Clear();
        generatedMesh.name = "Generated Liquid Spline Mesh";
        generatedMesh.vertices = vertices;
        generatedMesh.normals = normals;
        generatedMesh.uv = uvs;
        generatedMesh.triangles = triangles;
        generatedMesh.RecalculateBounds();
    }

    [ContextMenu("Reset Texture Distribution")]
    public void ResetTextureDistribution()
    {
        textureVDistribution = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        textureVTiling = 1f;
        textureVOffset = 0f;
        RebuildMesh();
    }

    private void QueueRebuild()
    {
        rebuildQueued = true;
    }

    private void SubscribeToSplineChanges()
    {
        if (observedSplineContainer == splineContainer)
        {
            return;
        }

        UnsubscribeFromSplineChanges();
        observedSplineContainer = splineContainer;

        if (observedSplineContainer != null)
        {
            Spline.Changed += OnAnySplineChanged;
        }
    }

    private void UnsubscribeFromSplineChanges()
    {
        if (observedSplineContainer == null)
        {
            return;
        }

        Spline.Changed -= OnAnySplineChanged;
        observedSplineContainer = null;
    }

    private void OnAnySplineChanged(Spline spline, int knotIndex, SplineModification modificationType)
    {
        if (splineContainer == null || splineContainer.Splines.Count == 0)
        {
            return;
        }

        int resolvedSplineIndex = Mathf.Clamp(splineIndex, 0, splineContainer.Splines.Count - 1);
        if (!ReferenceEquals(spline, splineContainer.Splines[resolvedSplineIndex]))
        {
            return;
        }

        QueueRebuild();
    }

    private void BuildArcLengthLookup(int resolvedSplineIndex, out float[] lookupT, out float[] lookupDistance, out float totalLength)
    {
        int samples = Mathf.Max(arcLengthLookupSamples, lengthSegments * 2);
        lookupT = new float[samples + 1];
        lookupDistance = new float[samples + 1];
        totalLength = 0f;

        Vector3 previousPosition = Vector3.zero;
        for (int i = 0; i <= samples; i++)
        {
            float t = i / (float)samples;
            lookupT[i] = t;

            if (!splineContainer.Evaluate(resolvedSplineIndex, t, out float3 position, out _, out _))
            {
                lookupDistance[i] = totalLength;
                continue;
            }

            Vector3 currentPosition = (Vector3)position;
            if (i > 0)
            {
                totalLength += Vector3.Distance(previousPosition, currentPosition);
            }

            lookupDistance[i] = totalLength;
            previousPosition = currentPosition;
        }
    }

    private static float FindSplineTAtNormalizedDistance(float normalizedDistance, float[] lookupT, float[] lookupDistance, float totalLength)
    {
        if (lookupT == null || lookupDistance == null || lookupT.Length == 0 || totalLength <= 0.0001f)
        {
            return Mathf.Clamp01(normalizedDistance);
        }

        float targetDistance = Mathf.Clamp01(normalizedDistance) * totalLength;
        int low = 0;
        int high = lookupDistance.Length - 1;

        while (low < high)
        {
            int mid = (low + high) / 2;
            if (lookupDistance[mid] < targetDistance)
            {
                low = mid + 1;
            }
            else
            {
                high = mid;
            }
        }

        int upper = Mathf.Clamp(low, 1, lookupDistance.Length - 1);
        int lower = upper - 1;
        float segmentLength = lookupDistance[upper] - lookupDistance[lower];

        if (segmentLength <= 0.0001f)
        {
            return lookupT[upper];
        }

        float segmentT = Mathf.InverseLerp(lookupDistance[lower], lookupDistance[upper], targetDistance);
        return Mathf.Lerp(lookupT[lower], lookupT[upper], segmentT);
    }

    private float EvaluateTextureV(float pathPercent)
    {
        EnsureTextureCurve();
        float remapped = textureVDistribution.Evaluate(Mathf.Clamp01(pathPercent));
        return remapped * textureVTiling + textureVOffset;
    }

    private void EnsureTextureCurve()
    {
        if (textureVDistribution == null || textureVDistribution.length == 0)
        {
            textureVDistribution = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        }
    }

    private void SampleFrame(int resolvedSplineIndex, float t, out Vector3 center, out Vector3 forward, out Vector3 right, out Vector3 up)
    {
        if (!splineContainer.Evaluate(resolvedSplineIndex, t, out float3 position, out float3 tangent, out float3 upVector))
        {
            center = transform.position;
            forward = transform.forward;
            right = transform.right;
            up = transform.up;
            return;
        }

        center = (Vector3)position;
        forward = ((Vector3)tangent).normalized;
        up = ((Vector3)upVector).normalized;

        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = transform.forward;
        }

        if (up.sqrMagnitude < 0.0001f || Mathf.Abs(Vector3.Dot(forward, up)) > 0.98f)
        {
            up = Mathf.Abs(Vector3.Dot(forward, Vector3.up)) > 0.98f ? Vector3.right : Vector3.up;
        }

        right = Vector3.Cross(up, forward).normalized;
        up = Vector3.Cross(forward, right).normalized;
    }

    private void EnsureMesh()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();
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
            name = "Generated Liquid Spline Mesh"
        };
        generatedMesh.MarkDynamic();
        meshFilter.sharedMesh = generatedMesh;
    }
}
