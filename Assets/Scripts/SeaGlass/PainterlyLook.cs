using UnityEngine;

namespace SeaGlass
{
    // Scene-wide values for the "Sea Glass/Painterly Lit" shader. See Documentation/ART_DIRECTION.md.
    [ExecuteAlways]
    public class PainterlyLook : MonoBehaviour
    {
        [Header("Shadow side")]
        [ColorUsage(false)] public Color shadowTint = new Color(0.42f, 0.33f, 0.56f);
        [Range(0, 1.5f)] public float shadowBrightness = 0.62f;
        [Range(0, 1)] public float skyFill = 0.35f;

        [Header("Light side")]
        [Range(-0.5f, 0.5f)] public float terminator = 0.02f;
        [Range(0.01f, 0.5f)] public float terminatorSoftness = 0.14f;
        [Range(0, 3)] public float sunStrength = 1.15f;

        [Header("Finish")]
        [ColorUsage(false)] public Color rimColour = new Color(1f, 0.72f, 0.45f);
        [Range(0, 1)] public float rimStrength = 0.35f;
        [Range(0.01f, 2)] public float variationScale = 0.35f;
        [Range(0, 1)] public float hueDrift = 0.35f;
        [Range(0, 1)] public float occlusion = 0.85f;
        [Range(0, 2)] public float sheen = 0.6f;

        static readonly int ShadowTint = Shader.PropertyToID("_SG_ShadowTint");
        static readonly int Terminator = Shader.PropertyToID("_SG_Terminator");
        static readonly int Rim = Shader.PropertyToID("_SG_Rim");
        static readonly int Paint = Shader.PropertyToID("_SG_Paint");
        static readonly int SunDir = Shader.PropertyToID("_SG_SunDir");

        void OnEnable() => Apply();
        void OnValidate() => Apply();
#if UNITY_EDITOR
        void Update() { if (!Application.isPlaying) Apply(); }
#endif

        public void Apply()
        {
            Shader.SetGlobalVector(ShadowTint, new Vector4(shadowTint.r, shadowTint.g, shadowTint.b, shadowBrightness));
            Shader.SetGlobalVector(Terminator, new Vector4(terminator, terminatorSoftness, sunStrength, skyFill));
            Shader.SetGlobalVector(Rim, new Vector4(rimColour.r, rimColour.g, rimColour.b, rimStrength));
            Shader.SetGlobalVector(Paint, new Vector4(variationScale, hueDrift, occlusion, sheen));
            var sun = RenderSettings.sun;
            if (sun) Shader.SetGlobalVector(SunDir, -sun.transform.forward);
        }
    }
}
