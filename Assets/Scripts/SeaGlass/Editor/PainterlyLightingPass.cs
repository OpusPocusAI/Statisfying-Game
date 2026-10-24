using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SeaGlass.Editor
{
    // Art direction pass 1 (Documentation/ART_DIRECTION.md, section 5.1): golden-hour lighting,
    // purple-tinted shadows, graded sky and post-processing. Repeatable.
    public static class PainterlyLightingPass
    {
        const string LitShader = "Universal Render Pipeline/Lit";
        const string PainterlyShader = "Sea Glass/Painterly Lit";

        // Ground-level surfaces: a height gradient would darken them everywhere.
        static readonly HashSet<string> Flat = new HashSet<string>
        {
            "Apricot sand", "Dry dune sand", "Warm tidal sand", "Honey floorboards", "Sun-washed floorboard",
            "Limewashed ceiling", "Ceiling joint", "Coastal stone", "Purple shadows", "Warm chalk",
        };

        // Foliage and rock get a stronger base-to-crown gradient.
        static readonly Dictionary<string, Vector2> Gradient = new Dictionary<string, Vector2>
        {
            { "Blue green plants", new Vector2(.38f, 4f) }, { "Dune grass shadow", new Vector2(.35f, .9f) },
            { "Dune grass sage", new Vector2(.35f, .9f) }, { "Sage canopy", new Vector2(.35f, 5f) },
            { "Wet coastal slate", new Vector2(.3f, 3f) }, { "Sunset sandstone", new Vector2(.3f, 6f) },
            { "Sun washed sandstone", new Vector2(.3f, 6f) }, { "Shell cove limestone", new Vector2(.28f, 3f) },
            { "Chalk rock crown", new Vector2(.2f, 3f) }, { "Tidal blue grey", new Vector2(.3f, 2f) },
            { "Tidal sage crown", new Vector2(.25f, 2f) }, { "Weathered studio teal", new Vector2(.18f, 3.5f) },
        };

        [MenuItem("Sea Glass/Art Direction/1 Lighting, sky and post")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (EditorApplication.isPlaying || scene.path != CoastBuilder.ScenePath)
                throw new System.InvalidOperationException("Open SeaGlassStudio in Edit Mode first.");

            ConvertMaterials();
            var look = EnsureLook();
            ConfigureSunAndAmbient();
            ConfigureSky();
            ConfigurePost();
            ConfigureOcclusion();
            look.Apply();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
        }

        public static int ConvertMaterials()
        {
            var painterly = Shader.Find(PainterlyShader);
            int converted = 0;
            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { "Assets/Materials", "Assets/Art" }))
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(guid));
                if (!material) continue;
                bool isLit = material.shader.name == LitShader;
                if (!isLit && material.shader != painterly) continue;
                if (isLit && material.GetFloat("_Surface") > 0.5f) continue;

                if (isLit)
                {
                    var colour = material.GetColor("_BaseColor");
                    var map = material.GetTexture("_BaseMap");
                    var scale = material.GetTextureScale("_BaseMap");
                    var offset = material.GetTextureOffset("_BaseMap");
                    float smoothness = material.GetFloat("_Smoothness");
                    var emission = material.IsKeywordEnabled("_EMISSION") ? material.GetColor("_EmissionColor") : Color.black;
                    material.shader = painterly;
                    material.SetColor("_BaseColor", colour);
                    material.SetTexture("_BaseMap", map);
                    material.SetTextureScale("_BaseMap", scale);
                    material.SetTextureOffset("_BaseMap", offset);
                    material.SetFloat("_Smoothness", smoothness);
                    material.SetColor("_EmissionColor", emission);
                    converted++;
                }

                if (Flat.Contains(material.name)) { material.SetFloat("_GradientStrength", 0); material.SetFloat("_Rim", .25f); }
                else if (Gradient.TryGetValue(material.name, out var g)) { material.SetFloat("_GradientStrength", g.x); material.SetFloat("_GradientHeight", g.y); }
                else { material.SetFloat("_GradientStrength", .2f); material.SetFloat("_GradientHeight", 2f); }
                material.SetFloat("_Variation", material.name.Contains("sand") ? .1f : .07f);
                EditorUtility.SetDirty(material);
            }
            return converted;
        }

        static PainterlyLook EnsureLook()
        {
            var look = Object.FindFirstObjectByType<PainterlyLook>();
            if (!look) look = new GameObject("Painterly look").AddComponent<PainterlyLook>();
            look.shadowTint = new Color(.46f, .36f, .6f);
            look.shadowBrightness = .62f;
            look.skyFill = .32f;
            look.terminator = .02f;
            look.terminatorSoftness = .12f;
            look.sunStrength = 1.05f;
            look.rimColour = new Color(1f, .74f, .48f);
            look.rimStrength = .35f;
            look.variationScale = .35f;
            look.hueDrift = .35f;
            look.occlusion = .85f;
            look.sheen = .6f;
            EditorUtility.SetDirty(look);
            return look;
        }

        static void ConfigureSunAndAmbient()
        {
            var sun = RenderSettings.sun;
            if (sun)
            {
                // Low sun over the sea to the west-north-west: backlit dunes and a sun path on the
                // water when walking north, and warm light through the studio's west windows.
                sun.transform.rotation = Quaternion.Euler(22f, 121f, 0f);
                sun.color = new Color(1f, .82f, .6f);
                sun.intensity = 1.4f;
                sun.shadowStrength = 1f;
                sun.shadows = LightShadows.Soft;
                EditorUtility.SetDirty(sun);
            }
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.58f, .6f, .86f);
            RenderSettings.ambientEquatorColor = new Color(.86f, .64f, .58f);
            RenderSettings.ambientGroundColor = new Color(.36f, .28f, .4f);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.94f, .74f, .64f);
            RenderSettings.fogDensity = .0042f;
        }

        static void ConfigureSky()
        {
            var sky = RenderSettings.skybox;
            if (!sky) return;
            sky.shader = Shader.Find("Sea Glass/Painted Sky");
            sky.SetColor("_Horizon", new Color(.99f, .87f, .66f));
            sky.SetColor("_MidSky", new Color(.95f, .67f, .52f));
            sky.SetColor("_Zenith", new Color(.6f, .54f, .72f));
            sky.SetColor("_SunColour", new Color(1f, .93f, .74f));
            sky.SetColor("_SunGlow", new Color(1f, .62f, .32f));
            sky.SetColor("_CloudLit", new Color(1f, .89f, .74f));
            sky.SetColor("_CloudShade", new Color(.74f, .57f, .67f));
            sky.SetFloat("_CloudCover", .52f);
            sky.SetFloat("_CloudScale", .75f);
            EditorUtility.SetDirty(sky);
        }

        static T Effect<T>(VolumeProfile profile) where T : VolumeComponent
        {
            if (!profile.TryGet<T>(out var effect)) { effect = profile.Add<T>(); AssetDatabase.AddObjectToAsset(effect, profile); }
            effect.active = true;
            return effect;
        }

        static void ConfigurePost()
        {
            var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Data/GoldenHourVolume.asset");
            var tone = Effect<Tonemapping>(profile); tone.mode.Override(TonemappingMode.Neutral);
            var colours = Effect<ColorAdjustments>(profile);
            colours.postExposure.Override(-.1f); colours.contrast.Override(18); colours.saturation.Override(6);
            colours.colorFilter.Override(Color.white);
            var smh = Effect<ShadowsMidtonesHighlights>(profile);
            smh.shadows.Override(new Vector4(.95f, .9f, 1.12f, -.04f));
            smh.midtones.Override(new Vector4(1f, .98f, .97f, 0));
            smh.highlights.Override(new Vector4(1.05f, 1f, .9f, .02f));
            var bloom = Effect<Bloom>(profile); bloom.threshold.Override(1f); bloom.intensity.Override(.45f); bloom.scatter.Override(.68f);
            bloom.tint.Override(new Color(1f, .86f, .7f));
            var vignette = Effect<Vignette>(profile); vignette.intensity.Override(.2f); vignette.smoothness.Override(.45f);
            vignette.color.Override(new Color(.2f, .12f, .22f));
            EditorUtility.SetDirty(profile);
        }

        static void ConfigureOcclusion()
        {
            var data = AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PC_Renderer.asset");
            if (!data) return;
            foreach (var feature in data.rendererFeatures)
            {
                if (!feature || feature.GetType().Name != "ScreenSpaceAmbientOcclusion") continue;
                var so = new SerializedObject(feature);
                so.FindProperty("m_Settings.Intensity").floatValue = 1.1f;
                so.FindProperty("m_Settings.Radius").floatValue = .45f;
                so.FindProperty("m_Settings.DirectLightingStrength").floatValue = .3f;
                so.FindProperty("m_Settings.Source").enumValueIndex = 1;
                so.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(feature);
            }
        }
    }
}
