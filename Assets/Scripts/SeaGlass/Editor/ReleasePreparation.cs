using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
namespace SeaGlass.Editor
{
    public static class ReleasePreparation
    {
        public static void Prepare()
        {
            var old=GameObject.Find("Golden-hour finishing volume");if(old)Object.DestroyImmediate(old);
            var go=new GameObject("Golden-hour finishing volume");var volume=go.AddComponent<Volume>();volume.isGlobal=true;
            string path="Assets/Data/GoldenHourVolume.asset";var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);if(!profile){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);}
            if(!profile.TryGet<Bloom>(out var bloom)){bloom=profile.Add<Bloom>();AssetDatabase.AddObjectToAsset(bloom,profile);}bloom.threshold.Override(.95f);bloom.intensity.Override(.35f);bloom.scatter.Override(.65f);
            if(!profile.TryGet<ColorAdjustments>(out var colors)){colors=profile.Add<ColorAdjustments>();AssetDatabase.AddObjectToAsset(colors,profile);}colors.postExposure.Override(.25f);colors.saturation.Override(5);colors.contrast.Override(5);
            volume.sharedProfile=profile;EditorUtility.SetDirty(profile);
            var camera=Object.FindFirstObjectByType<CoastalPlayer>().view;camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
            const string particlePath="Assets/Materials/SeaGlass/Foam and sparkle.mat";var particle=AssetDatabase.LoadAssetAtPath<Material>(particlePath);if(!particle){particle=new Material(Shader.Find("Sea Glass/Soft Feedback"));AssetDatabase.CreateAsset(particle,particlePath);}particle.shader=Shader.Find("Sea Glass/Soft Feedback");EditorUtility.SetDirty(particle);Object.FindFirstObjectByType<Feedback>().particleMaterial=particle;
            foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {
                if(r.transform.root.name.StartsWith("COAST")||r.transform.root.name.StartsWith("ART"))GameObjectUtility.SetStaticEditorFlags(r.gameObject,StaticEditorFlags.BatchingStatic);
                if(r.name=="Sea oat"||r.name=="Foam ribbon"||r.name.StartsWith("Socket"))r.shadowCastingMode=ShadowCastingMode.Off;
            }
            foreach(var col in Object.FindFirstObjectByType<UtilityTruck>().GetComponentsInChildren<Collider>())if(!(col is CharacterController))col.isTrigger=true;
            PlayerSettings.productName="Sea Glass Studio";PlayerSettings.companyName="Sea Glass Studio";PlayerSettings.bundleVersion="1.0.0";PlayerSettings.defaultScreenWidth=1920;PlayerSettings.defaultScreenHeight=1080;PlayerSettings.fullScreenMode=FullScreenMode.FullScreenWindow;
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/SeaGlassStudio.unity",true)};
            AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        }
    }
}
