#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Menüden tıklayınca yeşil dolar renkli bir Particle System VFX prefab'ı üretir.
/// Gorkem > Create Dollar VFX Prefab
/// </summary>
public static class DollarVfxCreator
{
    private const string PrefabFolder = "Assets/Gorkem/prefabs";
    private const string PrefabPath   = PrefabFolder + "/DollarVFX.prefab";

    // Dolar banknotu yeşili (~#85BB65) ve parlak varyantlar
    private static readonly Color DollarGreen      = new Color(0.52f, 0.73f, 0.40f, 1f);
    private static readonly Color DollarGreenLight = new Color(0.70f, 0.95f, 0.55f, 1f);
    private static readonly Color DollarGreenDark  = new Color(0.22f, 0.45f, 0.22f, 1f);

    [MenuItem("Gorkem/Create Dollar VFX Prefab")]
    public static void CreatePrefab()
    {
        if (!Directory.Exists(PrefabFolder))
            Directory.CreateDirectory(PrefabFolder);

        GameObject root = new GameObject("DollarVFX");
        try
        {
            var ps = root.AddComponent<ParticleSystem>();
            ConfigureParticleSystem(ps);

            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            if (existing != null)
                AssetDatabase.DeleteAsset(PrefabPath);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);

            Debug.Log($"[DollarVfxCreator] Prefab oluşturuldu: {PrefabPath}");
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    private static void ConfigureParticleSystem(ParticleSystem ps)
    {
        var main = ps.main;
        main.duration                = 1.2f;
        main.loop                    = false;
        main.startLifetime           = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
        main.startSpeed              = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        main.startSize               = new ParticleSystem.MinMaxCurve(0.08f, 0.2f);
        main.startColor              = new ParticleSystem.MinMaxGradient(DollarGreenLight, DollarGreen);
        main.gravityModifier         = 0.35f;
        main.simulationSpace         = ParticleSystemSimulationSpace.World;
        main.stopAction              = ParticleSystemStopAction.Destroy;
        main.maxParticles            = 80;
        main.playOnAwake             = true;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30, 35) });

        var shape = ps.shape;
        shape.enabled    = true;
        shape.shapeType  = ParticleSystemShapeType.Sphere;
        shape.radius     = 0.15f;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient grad = new Gradient();
        grad.SetKeys(
            new[]
            {
                new GradientColorKey(DollarGreenLight, 0f),
                new GradientColorKey(DollarGreen,      0.5f),
                new GradientColorKey(DollarGreenDark,  1f)
            },
            new[]
            {
                new GradientAlphaKey(0f,   0f),
                new GradientAlphaKey(1f,   0.15f),
                new GradientAlphaKey(0.8f, 0.75f),
                new GradientAlphaKey(0f,   1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.2f),
            new Keyframe(0.25f, 1f),
            new Keyframe(1f, 0.4f)
        );
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        var velocityOverLifetime = ps.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space   = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.y       = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);

        var limitVelocity = ps.limitVelocityOverLifetime;
        limitVelocity.enabled = true;
        limitVelocity.dampen  = 0.3f;

        var trails = ps.trails;
        trails.enabled        = true;
        trails.ratio          = 0.4f;
        trails.lifetime       = new ParticleSystem.MinMaxCurve(0.15f, 0.3f);
        trails.minVertexDistance = 0.05f;
        trails.widthOverTrail = new ParticleSystem.MinMaxCurve(0.05f);
        trails.colorOverTrail = new ParticleSystem.MinMaxGradient(DollarGreenLight);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;

        // Default-Particle materyalini otomatik bul, bulamazsa Sprites/Default'a düş
        Material particleMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-ParticleSystem.mat");
        if (particleMat == null)
            particleMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
        if (particleMat == null)
        {
            Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particleShader == null) particleShader = Shader.Find("Particles/Standard Unlit");
            if (particleShader == null) particleShader = Shader.Find("Sprites/Default");
            if (particleShader != null)
                particleMat = new Material(particleShader) { name = "DollarVFX_Mat" };
        }

        if (particleMat != null)
            renderer.sharedMaterial = particleMat;

        Material trailMat = renderer.sharedMaterial;
        renderer.trailMaterial = trailMat;
    }
}
#endif
