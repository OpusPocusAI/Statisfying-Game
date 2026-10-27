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
                go.transform.position=tray.transform.position+new Vector3((index%2-.5f)*.26f,.12f+(index/6)*.07f,(index/2%3-1)*.3f);
                go.transform.rotation=Quaternion.Euler(90+(index%3-1)*7,index*29,0);
                float size=pieceSize/Mathf.Max(def.HeldMesh.bounds.size.x,def.HeldMesh.bounds.size.y);
                go.transform.localScale=Vector3.one*size;
                go.AddComponent<MeshFilter>().sharedMesh=def.HeldMesh;
                go.AddComponent<MeshRenderer>().sharedMaterial=sockets.First(s=>s.glassId==def.id).surface.sharedMaterial;
                pieces.Add(def.id,go);
            }
            session.Changed+=Refresh;Refresh();
        }
        void Refresh(){foreach(var pair in pieces)pair.Value.SetActive(session.progress.items.Any(i=>i.id==pair.Key&&i.stage==ItemStage.Sorted));}
        void OnDestroy(){if(session)session.Changed-=Refresh;}
    }
}
