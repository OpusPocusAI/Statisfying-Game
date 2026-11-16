using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace SeaGlass.Editor
{
    // Grounding pass (owner, November 16: close the known gaps before the playtest; second independent review, all three:
    // "objects don't sit in the ground", dig piles "flat biscuits", the boardwalk "a raft on the sand", fallen glass "a thick block").
    // - Dig mounds: sand_mound_0..3 rebuilt as soft heaps that feather into the sand (ArtSource/blender/soft_mounds.py), same
    //   names and bounds, so every mound updates in place.
    // - Contact shadows outdoors: a soft blob (the studio's Contact Shadow shader) on the sand under beach props, rocks, posts,
    //   palm trunks and every find (parented to the find, so it goes when the find is collected), lying along the ground's slope.
    // - Field glass: pieces in the sand and on the road are half as thick, closer to the worn bottle-wall shard than a slab.
    // - Boardwalk: the deck drops so its boards rest on the sand and the joists are buried; the hidden plank colliders move with it.
    // Repeatable: the shadow root is rebuilt; glass and boardwalk are set from fixed values, not offset again.
    public static class GroundingPass
    {
        const string Root = "ART • outdoor contact shadows";
        static readonly string[] Props = { "Slatted beach bench", "Cool box", "Tote bag", "Beach ball", "Tide worn timber", "Hero mooring post", "Hero sign post",
                                           "Open terracotta entry planter", "Coastal shrub", "Wind-shaped coastal stone", "Round parasol pole", "Beach pebble cluster", "Shore boulder" };

        [MenuItem("Sea Glass/Art Direction/Grounding: soft mounds, contact shadows, boardwalk, field glass")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath, OpenSceneMode.Single);
            for (int k = 0; k < 4; k++) { var f = Path.Combine(ModelLibraryImporter.SourceFolder, "sand_mound_" + k + ".json"); if (File.Exists(f)) ModelLibraryImporter.Import(f); }
            AssetDatabase.SaveAssets();
            Physics.SyncTransforms();

            // ---- Mounds sit into the sand: each one's base goes just below the lowest sand round its rim, so no edge hangs over a
            // dip (the beach undulates; a level base left lips floating on the low side).
            int seated = 0;
            foreach (var mf in Object.FindObjectsByType<MeshFilter>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(m => m.sharedMesh && m.sharedMesh.name.StartsWith("sand_mound_")))
            {
                var t = mf.transform; float low = float.MaxValue;
                for (int k = 0; k < 12; k++)
                {
                    var rim = t.TransformPoint(new Vector3(Mathf.Cos(k * Mathf.PI / 6) * .9f, 0, Mathf.Sin(k * Mathf.PI / 6) * .9f));
                    foreach (var h in Physics.RaycastAll(new Ray(rim + Vector3.up * 3, Vector3.down), 6)) if (Ground(h.collider)) low = Mathf.Min(low, h.point.y);
                }
                if (low == float.MaxValue) continue;
                float baseY = t.TransformPoint(Vector3.zero).y;
                t.position += Vector3.up * (low - .03f - baseY); seated++;
                // Over a buried find the heap must still cover the piece (it rests 0.06 below the find's origin, about 0.05 thick): its
                // crown stays at least 0.05 above the origin, so a seated mound is taller where it sits lower. The dig shrinks from this.
                var find = t.parent ? t.parent.GetComponent<ShoreItem>() : null;
                if (find && find.mound == t)
                {
                    float need = find.transform.position.y + .05f - t.position.y;          // mesh height is 1 unit
                    var s = t.localScale; if (s.y < need) t.localScale = new Vector3(s.x, need, s.z);
                }
            }

            // ---- Field glass: half as thick (the slab's local Z is its thickness).
            var items = Object.FindObjectsByType<ShoreItem>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var i in items.Where(i => i.glass && i.visual)) { float s = i.visual.localScale.x; i.visual.localScale = new Vector3(s, s, s * .5f); EditorUtility.SetDirty(i.visual); }

            // ---- Boardwalk: boards on the sand, joists buried.
            var deck = GameObject.Find("Hero boardwalk deck");
            float deckDrop = 0;
            if (deck)
            {
                var b = deck.GetComponent<Renderer>().bounds; float ground = float.MaxValue;
                foreach (var p in new[] { new Vector3(b.min.x + .5f, 0, b.min.z + .5f), new Vector3(b.max.x - .5f, 0, b.min.z + .5f), new Vector3(b.min.x + .5f, 0, b.max.z - .5f), new Vector3(b.max.x - .5f, 0, b.max.z - .5f), b.center })
                    foreach (var h in Physics.RaycastAll(new Ray(new Vector3(p.x, 5, p.z), Vector3.down), 10)) if (Ground(h.collider)) ground = Mathf.Min(ground, h.point.y);
                if (ground < float.MaxValue)
                {
                    float y = ground + .015f;                     // board undersides (0.02 below the deck origin) sit just in the sand
                    deckDrop = deck.transform.position.y - y;
                    var dp = deck.transform.position; deck.transform.position = new Vector3(dp.x, y, dp.z);
                    // The planks' colliders (hidden boxes) carry the walkway: they follow the deck down. Set from the original
                    // height (top 0.11) so a re-run does not lower them again.
                    foreach (var pl in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None).Where(t => t.name == "Boardwalk plank"))
                    { var p = pl.position; pl.position = new Vector3(p.x, y + .03f, p.z); }   // planks sat 0.03 above the deck origin
                }
            }

            // ---- Contact shadows.
            var old = GameObject.Find(Root); if (old) Object.DestroyImmediate(old);
            foreach (var t in Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Where(t => t.name == "Find contact shadow").ToList()) Object.DestroyImmediate(t.gameObject);
            var root = new GameObject(Root).transform;
            var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Round19/Outdoor contact shadow.mat");
            if (!mat) { mat = new Material(Shader.Find("Sea Glass/Contact Shadow")); AssetDatabase.CreateAsset(mat, "Assets/Art/Round19/Outdoor contact shadow.mat"); }
            mat.SetFloat("_Strength", .75f); mat.SetFloat("_Softness", .7f); EditorUtility.SetDirty(mat);
            var quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            int n = 0;
            bool Blob(Transform parent, Vector3 at, float sx, float sz, float yaw, float strength = 1)
            {
                if (CoastalPlayer.Indoors(at)) return false;
                RaycastHit best = default; bool found = false;
                foreach (var h in Physics.RaycastAll(new Ray(at + Vector3.up * 3, Vector3.down), 6).OrderBy(h => h.distance))
                    if (Ground(h.collider)) { best = h; found = true; break; }
                if (!found) return false;
                var go = new GameObject(parent == root ? "Contact shadow" : "Find contact shadow"); go.transform.SetParent(parent, true);
                go.transform.SetPositionAndRotation(best.point + best.normal * .012f, Quaternion.FromToRotation(Vector3.up, best.normal) * Quaternion.Euler(90, yaw, 0));
                go.transform.localScale = new Vector3(sx, sz, 1);
                // (finds are unscaled roots, so the local scale is the size)
                go.AddComponent<MeshFilter>().sharedMesh = quad;
                var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterial = mat; mr.shadowCastingMode = ShadowCastingMode.Off; mr.receiveShadows = false;
                n++; return true;
            }
            foreach (var r in Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r => r.enabled && Props.Contains(r.name)))
            {
                var b = r.bounds; if (b.size.x > 6 || b.size.z > 6) continue;
                bool post = r.name.Contains("post") || r.name.Contains("pole");
                float sx = post ? .55f : b.size.x * 1.35f + .3f, sz = post ? .55f : b.size.z * 1.35f + .3f;
                Blob(root, new Vector3(b.center.x, b.min.y, b.center.z), sx, sz, 0);
            }
            // Palm trunks: a soft pool at the foot (the crown's own shadow is the sun's).
            foreach (var f in Object.FindObjectsByType<MeshFilter>(FindObjectsSortMode.None).Where(f => f.sharedMesh && f.sharedMesh.name.StartsWith("palm_")))
                Blob(root, f.transform.position, 1.1f, 1.1f, 0);
            // Finds: under each piece, parented to the find so it disappears with it.
            foreach (var i in items.Where(i => i.visual))
            {
                var vr = i.visual.GetComponentInChildren<Renderer>(true); if (!vr) continue;
                var b = vr.bounds;
                Blob(i.transform, new Vector3(b.center.x, b.min.y, b.center.z), b.size.x * 1.5f + .2f, b.size.z * 1.5f + .2f, 0);
            }

            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene); AssetDatabase.SaveAssets();
            Debug.Log($"Grounding: mounds re-imported and {seated} seated, {items.Count(i => i.glass)} field glass pieces thinned, boardwalk lowered {deckDrop:F2} m, {n} contact shadows");
        }

        static bool Ground(Collider c) => !c.isTrigger && (c.name == "Beach surface" || c.name == "Continuous sand shelf" || c.name == "Inland dune ridge" || c.name.ToLower().Contains("sand") || c.name.ToLower().Contains("road"));
    }
}
