using System.IO;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SeaGlass.Editor
{
    // Review captures with the game camera's settings and post-processing, but without the first-person
    // tool models parented to it (in Edit Mode they would all show at once). Used for the art reviews.
    public static class ReviewCapture
    {
        public static string Shot(string folder, string name, Vector3 position, Vector3 target, int width = 1600, int height = 900)
        {
            var src = GameObject.Find("First-person camera").GetComponent<Camera>();
            var go = new GameObject("Review capture camera");
            try
            {
                var cam = go.AddComponent<Camera>(); cam.CopyFrom(src);
                var ad = go.AddComponent<UniversalAdditionalCameraData>();
                var sad = src.GetComponent<UniversalAdditionalCameraData>();
                ad.renderPostProcessing = sad == null || sad.renderPostProcessing;
                ad.antialiasing = sad != null ? sad.antialiasing : AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                ad.renderShadows = true; ad.requiresDepthTexture = true; ad.requiresColorTexture = true;
                var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB) { antiAliasing = 4 };
                cam.targetTexture = rt;
                cam.transform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position));
                // Empty mosaic sockets get their faint picture tint at runtime (MosaicSocket.Update); show the
                // same here, or the idle panels render as black slabs that no player sees.
                var sockets = Object.FindObjectsByType<MosaicSocket>(FindObjectsSortMode.None);
                var block = new MaterialPropertyBlock();
                foreach (var s in sockets)
                {
                    var r = s.GetComponent<Renderer>(); if (!r || (s.piece && s.piece.activeSelf)) continue;
                    r.GetPropertyBlock(block); block.SetColor("_BaseColor", MosaicSocket.IdleTint(s.color)); r.SetPropertyBlock(block);
                }
                cam.Render();
                foreach (var s in sockets) { var r = s.GetComponent<Renderer>(); if (r) r.SetPropertyBlock(null); }
                RenderTexture.active = rt;
                var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
                tex.ReadPixels(new Rect(0, 0, width, height), 0, 0); tex.Apply();
                var dir = Path.Combine(Application.dataPath, "..", folder); Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, name + ".png");
                File.WriteAllBytes(path, tex.EncodeToPNG());
                Object.DestroyImmediate(tex); RenderTexture.active = null; cam.targetTexture = null; rt.Release();
                return Path.GetFullPath(path);
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
