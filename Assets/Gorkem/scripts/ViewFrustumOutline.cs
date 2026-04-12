using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Kamera frustum'u içinde kalan Renderer'ların outline genişliğini ayarlar.
/// Material: Gorkem/URP/GreenOutline ve MaterialPropertyBlock ile _OutlineWidth kullanır.
/// </summary>
public class ViewFrustumOutline : MonoBehaviour
{
    private static readonly int OutlineWidthId = Shader.PropertyToID("_OutlineWidth");

    [Header("Kamera")]
    [SerializeField] private Camera targetCamera;

    [Header("Hedefler")]
    [SerializeField] private bool useTag = true;
    [SerializeField] private string outlineableTag = "Outlineable";
    [SerializeField] private List<Renderer> extraRenderers = new List<Renderer>();

    [Header("Outline")]
    [SerializeField] private float outlineWidthWhenVisible = 0.02f;

    private readonly List<Renderer> _targets = new List<Renderer>();
    private readonly MaterialPropertyBlock _mpb = new MaterialPropertyBlock();
    private Plane[] _frustumPlanes = new Plane[6];

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = GetComponent<Camera>();

        RebuildTargetList();
    }

    private void OnEnable()
    {
        RebuildTargetList();
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            return;

        GeometryUtility.CalculateFrustumPlanes(targetCamera, _frustumPlanes);

        for (int i = 0; i < _targets.Count; i++)
        {
            Renderer r = _targets[i];
            if (r == null)
                continue;

            bool inView = GeometryUtility.TestPlanesAABB(_frustumPlanes, r.bounds);
            float w = inView ? outlineWidthWhenVisible : 0f;

            r.GetPropertyBlock(_mpb);
            _mpb.SetFloat(OutlineWidthId, w);
            r.SetPropertyBlock(_mpb);
        }
    }

    [ContextMenu("Hedef listesini yenile")]
    public void RebuildTargetList()
    {
        _targets.Clear();

        if (useTag && !string.IsNullOrEmpty(outlineableTag))
        {
            try
            {
                GameObject[] tagged = GameObject.FindGameObjectsWithTag(outlineableTag);
                for (int i = 0; i < tagged.Length; i++)
                {
                    Renderer[] rs = tagged[i].GetComponentsInChildren<Renderer>(true);
                    for (int j = 0; j < rs.Length; j++)
                        _targets.Add(rs[j]);
                }
            }
            catch (UnityException)
            {
                // Etiket Project Settings'te tanımlı değilse FindGameObjectsWithTag hata verir.
            }
        }

        for (int i = 0; i < extraRenderers.Count; i++)
        {
            if (extraRenderers[i] != null && !_targets.Contains(extraRenderers[i]))
                _targets.Add(extraRenderers[i]);
        }
    }
}
