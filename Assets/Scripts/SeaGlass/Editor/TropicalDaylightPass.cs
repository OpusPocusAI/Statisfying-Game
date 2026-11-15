using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace SeaGlass.Editor
{
    // Light and colour pass (owner, November 15: Tchia is the art target and the top priority). The scene moves from a peach
    // golden hour to bright tropical daylight: a higher, near-white sun; a deep blue sky with white cumulus shaded lilac-blue;
    // cool, airy blue haze instead of peach fog; lighter blue-violet shadows; a punchier, more saturated grade. Repeatable.
    public static class TropicalDaylightPass
    {
        [MenuItem("Sea Glass/Art Direction/Light and colour: tropical daylight")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath, OpenSceneMode.Single);

            // ---- Sun: late morning, high and almost white, keeping its compass bearing so the shadows still fall across the beach.
            var sun = RenderSettings.sun;
            if (sun)
            {
                sun.transform.rotation = Quaternion.Euler(52, 128, 0);
                sun.color = new Color(1f, .96f, .88f);
                sun.intensity = 1.3f;
                sun.shadowStrength = .8f;
                EditorUtility.SetDirty(sun); EditorUtility.SetDirty(sun.transform);
            }

            // ---- Ambient: blue sky above, a pale airy horizon, warm sand bounce below.
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.56f, .72f, 1f);
            RenderSettings.ambientEquatorColor = new Color(.78f, .86f, .94f);
            RenderSettings.ambientGroundColor = new Color(.56f, .48f, .42f);

            // ---- Haze: thin and sky-blue, so distance turns airy rather than peach.
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.72f, .85f, .97f);
            RenderSettings.fogDensity = .0030f;

            // ---- Sky: deep blue zenith, bright blue middle, pale near-white horizon; white clouds with lilac-blue undersides.
            var sky = RenderSettings.skybox;
            if (sky && sky.shader.name == "Sea Glass/Painted Sky")
            {
                sky.SetColor("_Zenith", new Color(.25f, .45f, .80f));
                sky.SetColor("_MidSky", new Color(.5f, .72f, .94f));
                sky.SetColor("_Horizon", new Color(.84f, .93f, 1f));
                sky.SetColor("_SunColour", new Color(1f, .98f, .92f));
                sky.SetColor("_SunGlow", new Color(1f, .93f, .78f));
                sky.SetColor("_CloudLit", new Color(1f, 1f, .98f));
                sky.SetColor("_CloudShade", new Color(.66f, .72f, .9f));
                sky.SetFloat("_CloudCover", .55f);
                EditorUtility.SetDirty(sky);
            }

            // ---- Painterly shading: lighter blue-violet shadows, a cleaner light side, a soft cool-white rim.
            foreach (var look in Object.FindObjectsByType<PainterlyLook>(FindObjectsSortMode.None))
            {
                look.shadowTint = new Color(.40f, .46f, .80f);
                look.shadowBrightness = .5f;
                look.skyFill = .34f;
                look.sunStrength = 1.1f;
                look.terminatorSoftness = .12f;
                look.rimColour = new Color(1f, .96f, .88f);
                look.rimStrength = .22f;
                look.Apply(); EditorUtility.SetDirty(look);
            }

            // ---- Grade: brighter, more saturated, gently contrasty; shadows lean blue, highlights stay warm-white.
            foreach (var v in Object.FindObjectsByType<Volume>(FindObjectsSortMode.None))
            {
                var p = v.sharedProfile; if (!p) continue;
                if (p.TryGet(out Bloom bloom)) { Set(bloom.tint, new Color(1f, .97f, .9f)); Set(bloom.intensity, .5f); Set(bloom.threshold, 1.05f); }
                if (p.TryGet(out ColorAdjustments ca)) { Set(ca.postExposure, -.08f); Set(ca.contrast, 26f); Set(ca.saturation, 30f); }
                if (p.TryGet(out ShadowsMidtonesHighlights smh))
                {
                    Set(smh.shadows, new Vector4(.92f, .97f, 1.14f, -.04f));
                    Set(smh.midtones, new Vector4(1f, 1f, 1f, 0f));
                    Set(smh.highlights, new Vector4(1.03f, 1.01f, .96f, .03f));
                }
                if (p.TryGet(out Vignette vig)) { Set(vig.color, new Color(.1f, .14f, .28f)); Set(vig.intensity, .14f); }
                EditorUtility.SetDirty(p);
            }

            // ---- Sand: its base colours were near-neutral because the peach light used to warm them. Under white daylight they
            // washed out to grey-white, so the sand now carries its own warm golden cream.
            Tint("Assets/Materials/SeaGlass/Apricot sand.mat", new Color(1f, .9f, .73f));
            Tint("Assets/Art/ShoreTransition/Warm tidal sand.mat", new Color(.99f, .88f, .71f));
            Tint("Assets/Art/ShorelinePolish/Dry dune sand.mat", new Color(1f, .9f, .73f));
            Tint("Assets/Art/Round10/Footprint sand.mat", new Color(.94f, .82f, .65f));
            Tint("Assets/Art/Beach/Turned damp sand.mat", new Color(.88f, .74f, .56f));

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("Tropical daylight pass applied.");
        }

        static void Tint(string path, Color c)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!m) { Debug.LogWarning("Tropical daylight pass: missing " + path); return; }
            m.SetColor("_BaseColor", c); EditorUtility.SetDirty(m);
        }
        static void Set<T>(VolumeParameter<T> p, T value) { p.overrideState = true; p.value = value; }
    }
}
