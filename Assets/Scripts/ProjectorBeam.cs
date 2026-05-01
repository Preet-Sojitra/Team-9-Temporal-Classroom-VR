using UnityEngine;

// [ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProjectorBeam : MonoBehaviour
{
    [Header("Beam Shape")]
    public float beamLength = 5f;
    public float startRadius = 0.08f;
    public float endRadius = 1.8f;
    public int segments = 32;

    [Header("Beam Visuals")]
    public Color beamColor = new Color(0.85f, 0.9f, 1f, 0.3f);
    public float pulseSpeed = 1.2f;
    public float pulseAmount = 0.08f;

    private MeshRenderer _renderer;
    private Material _mat;
    private float _baseAlpha;

    // void Awake()
    void Awake()
    {
        GetComponent<MeshFilter>().mesh = BuildConeMesh();

        _mat = new Material(Shader.Find("Particles/Standard Unlit"));
        // Fallback shaders in case Particles/Standard Unlit isn't in build
        if (_mat.shader.name == "Hidden/InternalErrorShader")
            _mat = new Material(Shader.Find("Sprites/Default"));
        if (_mat.shader.name == "Hidden/InternalErrorShader")
            _mat = new Material(Shader.Find("Unlit/Color"));

        _mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        _mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One); // Additive blending = glow
        _mat.SetInt("_ZWrite", 0);
        _mat.renderQueue = 3000;
        _mat.color = beamColor;

        _renderer = GetComponent<MeshRenderer>();
        _renderer.material = _mat;
        _renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _renderer.receiveShadows = false;

        _baseAlpha = beamColor.a;
    }

    void Update()
    {
        if (_mat == null) return; // Safety for edit mode
        float pulse = Application.isPlaying ?
            Mathf.Sin(Time.time * pulseSpeed) * pulseAmount : 0f;
        Color c = beamColor;
        c.a = Mathf.Clamp01(_baseAlpha + pulse);
        _mat.color = c;
    }

    Mesh BuildConeMesh()
    {
        var mesh = new Mesh();
        int vCount = (segments + 1) * 2 + 2; // rings + caps
        var verts = new Vector3[vCount];
        var uvs = new Vector2[vCount];
        var colors = new Color[vCount];

        // Two rings: start (near projector) and end (screen)
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            // Near ring (full opacity)
            verts[i] = new Vector3(cos * startRadius, sin * startRadius, 0f);
            uvs[i] = new Vector2((float)i / segments, 0f);
            colors[i] = new Color(1, 1, 1, 1f);

            // Far ring (fade out toward screen)
            verts[i + segments + 1] = new Vector3(cos * endRadius, sin * endRadius, beamLength);
            uvs[i + segments + 1] = new Vector2((float)i / segments, 1f);
            colors[i + segments + 1] = new Color(1, 1, 1, 0.05f);
        }

        // Build triangles for the cone side
        var tris = new System.Collections.Generic.List<int>();
        for (int i = 0; i < segments; i++)
        {
            int a = i, b = i + 1;
            int c = i + segments + 1, d = i + segments + 2;

            // Two triangles per quad, facing INWARD so visible from inside
            tris.Add(a); tris.Add(c); tris.Add(b);
            tris.Add(b); tris.Add(c); tris.Add(d);

            // Also facing outward so visible from outside
            tris.Add(a); tris.Add(b); tris.Add(c);
            tris.Add(b); tris.Add(d); tris.Add(c);
        }

        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.colors = colors;
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    // Call this from ObjectMenu to turn beam on
    public static ProjectorBeam CreateBeam(Transform projector, Transform screen)
    {
        var go = new GameObject("ProjectorBeam");
        go.transform.SetParent(projector);
        go.transform.localPosition = Vector3.zero;

        // Point toward the screen
        go.transform.LookAt(screen.position);

        var beam = go.AddComponent<ProjectorBeam>();
        beam.beamLength = Vector3.Distance(projector.position, screen.position);
        return beam;
    }
}