using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaGlass.Editor
{
    // Walkable points pass (owner, November 16: close the known gaps before the playtest; "you can see the sandy points but not
    // walk onto them"). Since the coastline pass the headlands run out to sea as sandy points with palms, but the invisible wall
    // at X = -33 still stopped the player at the old line.
    // - The seaward wall (one 238 m box at X = -33) is replaced by a chain of short wall pieces that follows the authored
    //   waterline (CoastShape.ShoreX) 0.9 m inland of the water, so you can walk down the wet sand in the coves and out along
    //   each point to its tip, and still never into the sea. The inland wall is unchanged.
    // - The ground out there gets collision: the wet-sand strip and the sand-point lobes.
    // - What you can now reach is solid: the boulders and cove-landmark rocks whose footprint lies landward of the wall, and
    //   the palms on the points (trunk capsules, like the grove palms).
    // Finds, their spawns and the studio are untouched. Repeatable: the wall chain is rebuilt; added colliders are reused.
    public static class WalkablePointsPass
    {
        const string Root = "COAST • waterline boundary";
        const float Inset = .9f, Step = 2f;

        [MenuItem("Sea Glass/Art Direction/Walkable points: waterline boundary")]
        public static void Apply()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != CoastBuilder.ScenePath) scene = EditorSceneManager.OpenScene(CoastBuilder.ScenePath, OpenSceneMode.Single);

            // The old seaward wall: kept in the scene (renderer already off), its collider switched off.
            var oldWall = Object.FindObjectsByType<BoxCollider>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault(c => c.name == "Coastal boundary" && c.transform.position.x < 0);
            float z0 = -39, z1 = 199, height = 10, yMid = 3;
            if (oldWall)
            {
                var b = oldWall.bounds; z0 = b.min.z; z1 = b.max.z; height = b.size.y; yMid = b.center.y;
                oldWall.enabled = false;
            }

            var old = GameObject.Find(Root); if (old) Object.DestroyImmediate(old);
            var root = new GameObject(Root).transform;
            int pieces = 0;
            for (float z = z0; z < z1 - .01f; z += Step)
            {
                float za = z, zb = Mathf.Min(z + Step, z1);
                var a = new Vector3(CoastShape.ShoreX(za) + Inset, 0, za); var b = new Vector3(CoastShape.ShoreX(zb) + Inset, 0, zb);
                var along = (b - a); float len = along.magnitude; along /= len;
                var seaward = new Vector3(-along.z, 0, along.x); if (seaward.x > 0) seaward = -seaward;   // points out to sea (-X)
                var go = new GameObject("Waterline wall"); go.transform.SetParent(root);
                go.transform.SetPositionAndRotation((a + b) * .5f + seaward * .5f + Vector3.up * yMid, Quaternion.LookRotation(along, Vector3.up));
                var box = go.AddComponent<BoxCollider>(); box.size = new Vector3(1, height, len + .4f);   // overlapping ends, no gaps at bends
                pieces++;
            }

            // Ground collision on the wet strip and the sand points.
            int ground = 0;
            foreach (var name in new[] { "Soft wet-sand shoreline" })
            {
                var go = GameObject.Find(name); if (!go) continue;
                var mc = go.GetComponent<MeshCollider>(); if (!mc) mc = go.AddComponent<MeshCollider>();
                mc.sharedMesh = go.GetComponent<MeshFilter>().sharedMesh; ground++;
            }
            var points = GameObject.Find("ART • sand points");
            if (points)
                foreach (var mf in points.GetComponentsInChildren<MeshFilter>().Where(f => f.name == "Sand point"))
                {
                    var mc = mf.GetComponent<MeshCollider>(); if (!mc) mc = mf.gameObject.AddComponent<MeshCollider>();
                    mc.sharedMesh = mf.sharedMesh; ground++;
                }

            // Solid rocks and palms within reach (landward of the new wall).
            int solid = 0;
            foreach (var rootName in new[] { "ART • rocky points", "ART • cove landmarks" })
            {
                var r = GameObject.Find(rootName); if (!r) continue;
                foreach (var mr in r.GetComponentsInChildren<MeshRenderer>())
                {
                    var bb = mr.bounds; float wall = CoastShape.ShoreX(bb.center.z) + Inset;
                    if (bb.max.x < wall - .2f || mr.GetComponent<Collider>()) continue;
                    var mf = mr.GetComponent<MeshFilter>(); if (!mf || !mf.sharedMesh) continue;
                    mr.gameObject.AddComponent<MeshCollider>().sharedMesh = mf.sharedMesh; solid++;
                }
            }
            if (points)
                foreach (var palm in points.GetComponentsInChildren<Transform>().Where(t => t.name == "Point palm"))
                {
                    if (palm.GetComponent<Collider>()) continue;
                    var c = palm.gameObject.AddComponent<CapsuleCollider>(); var s = Mathf.Max(palm.lossyScale.y, .01f);
                    c.radius = .28f / s; c.height = 3f / s; c.center = new Vector3(0, 1.5f / s, 0); solid++;
                }

            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            Debug.Log($"Walkable points: {pieces} waterline wall pieces replace the X -33 wall, {ground} ground surfaces solid, {solid} rocks and palms solid");
        }
    }
}
