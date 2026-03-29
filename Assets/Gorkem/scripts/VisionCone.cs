using UnityEngine;

[RequireComponent(typeof(SecurityCamera))]
public class VisionCone : MonoBehaviour
{
    [Header("Koni Şekli")]
    [SerializeField] private int   horizontalRays = 30;
    [SerializeField] private int   verticalRays   = 10;
    [SerializeField] private float verticalFOV    = 40f;

    [Header("Renkler")]
    [SerializeField] private Color normalColor = new Color(0f, 1f, 0f, 0.25f);
    [SerializeField] private Color alertColor  = new Color(1f, 0f, 0f, 0.35f);

    [Header("Engel Maskeleri")]
    [SerializeField] private LayerMask obstacleMask = ~0;

    private SecurityCamera _secCam;
    private MeshFilter     _meshFilter;
    private MeshRenderer   _meshRenderer;
    private Mesh           _mesh;
    private Material       _material;

    private void Awake()
    {
        _secCam = GetComponent<SecurityCamera>();

        GameObject coneObj = new GameObject("VisionConeMesh");
        coneObj.transform.SetParent(transform, false);

        _meshFilter              = coneObj.AddComponent<MeshFilter>();
        _meshRenderer            = coneObj.AddComponent<MeshRenderer>();
        _mesh                    = new Mesh { name = "VisionCone" };
        _meshFilter.mesh         = _mesh;
        _material                = CreateTransparentMaterial();
        _meshRenderer.material   = _material;
        _meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _meshRenderer.receiveShadows    = false;
    }

    private void LateUpdate()
    {
        DrawVisionCone();
        _material.color = _secCam.PlayerDetected ? alertColor : normalColor;
    }

    private void DrawVisionCone()
    {
        float   range    = _secCam.DetectionRange;
        float   hFOV     = _secCam.FieldOfView;
        float   vFOV     = verticalFOV;
        Vector3 forward  = _secCam.FacingDirection;

        int hCount = horizontalRays + 1;
        int vCount = verticalRays   + 1;

        // Apex (uç nokta) + grid yüzeyi
        int totalVerts = 1 + hCount * vCount;
        Vector3[] verts = new Vector3[totalVerts];
        verts[0] = Vector3.zero; // Apex — kamera konumu (local)

        // Yatay ve dikey açılar için yerel eksenler
        Vector3 up    = transform.InverseTransformDirection(Vector3.up);
        Vector3 right = transform.InverseTransformDirection(transform.right);

        for (int v = 0; v < vCount; v++)
        {
            float vAngle = -vFOV * 0.5f + vFOV * v / verticalRays;

            for (int h = 0; h < hCount; h++)
            {
                float hAngle = -hFOV * 0.5f + hFOV * h / horizontalRays;

                // Yön: önce yatay sonra dikey döndür
                Vector3 localFwd   = transform.InverseTransformDirection(forward);
                Vector3 rotatedDir = Quaternion.AngleAxis(hAngle, up) *
                                     Quaternion.AngleAxis(vAngle, right) * localFwd;

                Vector3 worldDir = transform.TransformDirection(rotatedDir).normalized;
                float   dist     = range;

                if (Physics.Raycast(transform.position, worldDir, out RaycastHit hit, range, obstacleMask))
                    dist = hit.distance;

                verts[1 + v * hCount + h] = transform.InverseTransformPoint(
                    transform.position + worldDir * dist);
            }
        }

        // Üçgen sayısı:
        // Yüzey gridı:  horizontalRays * verticalRays * 6
        // Üst kenar:    horizontalRays * 3
        // Alt kenar:    horizontalRays * 3
        // Sol kenar:    verticalRays   * 3
        // Sağ kenar:    verticalRays   * 3
        int triCount = horizontalRays * verticalRays * 6
                     + horizontalRays * 3 * 2
                     + verticalRays   * 3 * 2;

        int[] tris = new int[triCount];
        int   t    = 0;

        // Dış yüzey gridı
        for (int v = 0; v < verticalRays; v++)
        {
            for (int h = 0; h < horizontalRays; h++)
            {
                int v00 = 1 + v       * hCount + h;
                int v10 = 1 + v       * hCount + h + 1;
                int v01 = 1 + (v + 1) * hCount + h;
                int v11 = 1 + (v + 1) * hCount + h + 1;

                // Her quad için 2 üçgen (her iki yönden görünsün diye çift yüzey)
                tris[t++] = v00; tris[t++] = v01; tris[t++] = v10;
                tris[t++] = v10; tris[t++] = v01; tris[t++] = v11;
            }
        }

        // Üst kenar (apex'ten ilk satıra)
        for (int h = 0; h < horizontalRays; h++)
        {
            int a = 1 + h;
            int b = 1 + h + 1;
            tris[t++] = 0; tris[t++] = b; tris[t++] = a;
        }

        // Alt kenar (apex'ten son satıra)
        for (int h = 0; h < horizontalRays; h++)
        {
            int a = 1 + verticalRays * hCount + h;
            int b = 1 + verticalRays * hCount + h + 1;
            tris[t++] = 0; tris[t++] = a; tris[t++] = b;
        }

        // Sol kenar (apex'ten ilk sütuna)
        for (int v = 0; v < verticalRays; v++)
        {
            int a = 1 + v       * hCount;
            int b = 1 + (v + 1) * hCount;
            tris[t++] = 0; tris[t++] = a; tris[t++] = b;
        }

        // Sağ kenar (apex'ten son sütuna)
        for (int v = 0; v < verticalRays; v++)
        {
            int a = 1 + v       * hCount + horizontalRays;
            int b = 1 + (v + 1) * hCount + horizontalRays;
            tris[t++] = 0; tris[t++] = b; tris[t++] = a;
        }

        _mesh.Clear();
        _mesh.vertices  = verts;
        _mesh.triangles = tris;
        _mesh.RecalculateNormals();
    }

    private Material CreateTransparentMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend",   0f);
        mat.SetInt("_SrcBlend",  (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend",  (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite",    0);
        mat.SetInt("_Cull",      0); // Double sided — her açıdan görünsün
        mat.renderQueue = 3000;
        mat.color       = normalColor;
        return mat;
    }
}
