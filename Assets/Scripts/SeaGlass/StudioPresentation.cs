using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SeaGlass
{
    // Inventory remains authoritative; these meshes are only a readable physical display.
    public class StudioPresentation : MonoBehaviour
    {
        readonly Dictionary<string,GameObject> pieces=new();
        GameSession session;
        void Start()
        {
            session=GameSession.Instance;
            var trays=FindObjectsByType<SortingTray>(FindObjectsSortMode.None);
            var sockets=FindObjectsByType<MosaicSocket>(FindObjectsSortMode.None);
            var counts=new int[6];
            // Round 13: sorted pieces match the spare pebbles already in the trays (they were about 3x larger).
            var spares=FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name.StartsWith("Spare ")&&r.name.EndsWith(" glass")).Select(r=>Mathf.Max(r.bounds.size.x,r.bounds.size.z)).OrderBy(x=>x).ToArray();
            float pieceSize=spares.Length>0?spares[spares.Length/2]*1.1f:.14f;
            foreach(var def in session.catalog.glass)
            {
                var tray=trays.First(t=>t.color==def.color);int index=counts[(int)def.color]++;
                var go=new GameObject("Sorted "+def.id);go.transform.SetParent(transform,false);
                // Use world dimensions: trays themselves have non-uniform transform scales.
                go.transform.position=tray.transform.position+new Vector3((index%2-.5f)*.2f,.035f+(index/6)*.03f,(index/2%3-1)*.22f);   // round 15: resting on the felt among the heap (was 12 cm up)
                go.transform.rotation=Quaternion.Euler(90+(index%3-1)*7,index*29,0);
                var mesh=def.PlateMesh;   // round 115: the thin plates, like the spares (Round115Pass)
                float size=pieceSize/Mathf.Max(mesh.bounds.size.x,mesh.bounds.size.y);
                go.transform.localScale=Vector3.one*size;
                go.AddComponent<MeshFilter>().sharedMesh=mesh;
                var mr=go.AddComponent<MeshRenderer>();mr.sharedMaterial=sockets.First(s=>s.glassId==def.id).surface.sharedMaterial;MarkPlate(mr);
                pieces.Add(def.id,go);
            }
            // Round 115: the tray spares are thin plates too; their colour materials are shared with the beach pebbles, so the plate
            // shading is switched on per renderer.
            foreach(var r in FindObjectsByType<MeshRenderer>(FindObjectsInactive.Include,FindObjectsSortMode.None))if(r.name.StartsWith("Spare ")&&r.name.EndsWith(" glass"))MarkPlate(r);
            session.Changed+=Refresh;Refresh();
        }
        static MaterialPropertyBlock plateBlock;
        static void MarkPlate(Renderer r){plateBlock??=new MaterialPropertyBlock();r.GetPropertyBlock(plateBlock);plateBlock.SetFloat("_Plate",1);r.SetPropertyBlock(plateBlock);}
        void Refresh(){foreach(var pair in pieces)pair.Value.SetActive(session.progress.items.Any(i=>i.id==pair.Key&&i.stage==ItemStage.Sorted));}
        void OnDestroy(){if(session)session.Changed-=Refresh;}
    }
}
