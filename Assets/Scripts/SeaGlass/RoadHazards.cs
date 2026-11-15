using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace SeaGlass
{
    // Drive pass (owner playtest, November 14: the drive was "slow, pointless, a chore"; the owner chose a faster truck plus loose
    // cargo on a bumpy road). Potholes and wind-rippled sand lie across the coast road. Take them fast with a loaded truck and
    // pieces bounce out of the crates onto the road, where they wait to be picked up again. Slowed for, or steered round, they
    // cost nothing. The hazards are built at runtime from the road's own materials, so the scene is untouched.
    public class RoadHazards : MonoBehaviour
    {
        public const float SafeSpeed=6.5f,HardSpeed=10f;
        public class Hazard{public Vector3 at;public float radius,length;public bool ripples,inside;public float lastHit=-9;}
        public readonly List<Hazard> hazards=new();
        public static RoadHazards Instance;
        Dictionary<string,ShoreItem> items;bool warned;float rattle,top;
        GameSession S=>GameSession.Instance;
        void Awake(){Instance=this;}
        void OnDestroy(){if(Instance==this)Instance=null;}
        void Start()
        {
            var road=GameObject.Find("Coast road");if(!road)return;
            items=FindObjectsByType<ShoreItem>(FindObjectsInactive.Include,FindObjectsSortMode.None).ToDictionary(i=>i.id);
            var rr=road.GetComponent<Renderer>();top=rr.bounds.max.y;var roadMat=rr.sharedMaterial;
            var sand=FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None).Where(r=>r.name=="Sand road shoulder").Select(r=>r.sharedMaterial).FirstOrDefault()??roadMat;
            var floor=new Material(roadMat){name="Pothole floor"};floor.SetColor("_BaseColor",new Color(.30f,.27f,.29f));
            var lip=new Material(roadMat){name="Pothole lip"};lip.SetColor("_BaseColor",new Color(.63f,.59f,.58f));
            var root=new GameObject("Road hazards").transform;
            // Markers sit on the centre line every 9 m (z = -16 + 9k); ripples fall between them and potholes stay off the line.
            Pothole(root,new Vector3(13.4f,top,38),1f,floor,lip,1);
            Ripples(root,51.6f,5.4f,sand,2);
            Pothole(root,new Vector3(16.8f,top,71),1.05f,floor,lip,3);
            Pothole(root,new Vector3(12.6f,top,88),.9f,floor,lip,4);
            Pothole(root,new Vector3(17.4f,top,89.5f),.95f,floor,lip,5);
            Ripples(root,105.7f,5.2f,sand,6);
            Pothole(root,new Vector3(14f,top,126),1f,floor,lip,7);
        }
        void Update()
        {
            if(items==null||!S||S.ControlsBlocked)return;var t=S.truck;if(!t||!t.driving)return;
            float s=Mathf.Abs(t.Speed);var p=t.transform.position;bool rippling=false;
            foreach(var h in hazards)
            {
                if(h.ripples)
                {
                    bool now=Mathf.Abs(p.z-h.at.z)<h.length*.5f;rippling|=now;
                    if(now&&!h.inside)Hit(t,h,s);h.inside=now;continue;
                }
                if(Time.time-h.lastHit<1.5f)continue;
                if(t.wheels.Any(w=>{var d=w.position-h.at;d.y=0;return d.magnitude<h.radius*.85f;})){h.lastHit=Time.time;Hit(t,h,s);}
            }
            // Driving across ripples rattles the whole way over.
            if(rippling&&s>1.5f&&Time.time>rattle){rattle=Time.time+.1f;t.Jolt(.22f*Mathf.Clamp01(s/8));Feedback.Instance?.Play("sand",.5f+s*.03f);}
        }
        void Hit(UtilityTruck t,Hazard h,float s)
        {
            float k=Mathf.Clamp01(s/UtilityTruck.TopSpeed);
            t.Jolt((h.ripples?.45f:.6f)+k*1.1f);if(s>SafeSpeed)t.Bump(h.ripples?.88f:.92f);
            Feedback.Instance?.Play("metal",.5f+k*.25f);
            if(s<SafeSpeed)return;
            Spill(t,(h.ripples?2:1)+(s>=HardSpeed?1:0));
        }
        // Throws up to n loaded pieces out of the crates onto the road behind the truck. Returns how many fell.
        public int Spill(UtilityTruck t,int n)
        {
            if(items==null)return 0;
            var load=S.progress.items.Where(r=>r.stage==ItemStage.Truck&&items.ContainsKey(r.id)).OrderBy(_=>Random.value).Take(n).ToList();
            if(load.Count==0)return 0;
            var tr=t.transform;var bed=tr.position-tr.forward*1.6f+Vector3.up*1.5f;var cc=t.GetComponent<CharacterController>();
            for(int i=0;i<load.Count;i++)
            {
                var r=load[i];
                var land=tr.position-tr.forward*Random.Range(3f,5.5f)+tr.right*Random.Range(-2.2f,2.2f);land.x=Mathf.Clamp(land.x,11.2f,18.8f);
                land.y=top;
                foreach(var hit in Physics.RaycastAll(land+Vector3.up*4,Vector3.down,8,~0,QueryTriggerInteraction.Ignore).OrderBy(x=>x.distance))
                    if(hit.collider!=cc&&!hit.collider.GetComponentInParent<ShoreItem>()){land=hit.point;break;}
                r.stage=ItemStage.Shore;r.spilled=true;r.spot=land;
                StartCoroutine(Tumble(items[r.id],bed+tr.right*Random.Range(-.4f,.4f),land,i*.09f));
            }
            t.RefreshCargo();S.Save();
            S.Notify("Bump! "+(load.Count==1?"A piece":load.Count+" pieces")+" bounced out of the crate"+(warned?".":". Ease off over potholes and sand ripples."));warned=true;
            return load.Count;
        }
        IEnumerator Tumble(ShoreItem item,Vector3 from,Vector3 to,float delay)
        {
            if(delay>0)yield return new WaitForSeconds(delay);
            var mf=item.visual?item.visual.GetComponentInChildren<MeshFilter>(true):null;var mr=mf?mf.GetComponent<MeshRenderer>():null;
            if(mf&&mr)
            {
                var go=new GameObject("Bouncing cargo");go.AddComponent<MeshFilter>().sharedMesh=mf.sharedMesh;var ren=go.AddComponent<MeshRenderer>();ren.sharedMaterials=mr.sharedMaterials;
                if(item.glass){var b=new MaterialPropertyBlock();b.SetColor("_BaseColor",GameCatalog.Palette[item.colorIndex]);ren.SetPropertyBlock(b);}
                go.transform.localScale=mf.transform.lossyScale;var spin=Random.onUnitSphere*Random.Range(540f,900f);var start=mf.transform.rotation;
                for(float t=0;t<1;t+=Time.deltaTime/.7f)
                {
                    go.transform.position=Vector3.Lerp(from,to+Vector3.up*.06f,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*1.4f;
                    go.transform.rotation=Quaternion.Euler(spin*t)*start;yield return null;
                }
                Destroy(go);
            }
            item.Refresh();
            Feedback.Instance?.Dust(to);Feedback.Instance?.Burst(to+Vector3.up*.1f,item.glass?GameCatalog.Palette[item.colorIndex]:new Color(1f,.92f,.78f),6);
            Feedback.Instance?.Play(item.glass?"glass":item.id.EndsWith("0")?"metal":"plastic",1.15f);
        }
        public static int Fallen(GameSession s)=>s.progress.items.Count(r=>r.spilled&&r.stage==ItemStage.Shore);
        public static ShoreItem NearestFallen(GameSession s,Vector3 from)=>
            Object.FindObjectsByType<ShoreItem>(FindObjectsSortMode.None).Where(i=>i.Record.spilled&&i.Record.stage==ItemStage.Shore).OrderBy(i=>(i.transform.position-from).sqrMagnitude).FirstOrDefault();

        // ---- meshes: each hazard is one fused surface (no stacked shapes) that tucks under the road top at its edge.
        void Pothole(Transform root,Vector3 at,float radius,Material floor,Material lip,int seed)
        {
            var rng=new System.Random(seed*31+7);float R()=>(float)rng.NextDouble();
            const int A=30;float[] t={0,.16f,.32f,.48f,.62f,.74f,.86f,1};float[] y={.0035f,.004f,.003f,.0045f,.006f,.02f,.026f,-.006f};
            float p1=R()*6.3f,p2=R()*6.3f,p3=R()*6.3f;
            var v=new List<Vector3>{new(0,y[0],0)};var uv=new List<Vector2>{Vector2.zero};
            for(int k=1;k<t.Length;k++)for(int a=0;a<A;a++)
            {
                float th=a*Mathf.PI*2/A,w=1+.2f*Mathf.Sin(th*2+p1)+.11f*Mathf.Sin(th*3+p2)+.06f*Mathf.Sin(th*5+p3);
                float crumble=k>=6?(R()-.5f)*.16f:(R()-.5f)*.04f;float rad=radius*t[k]*(w+crumble);
                var q=new Vector3(Mathf.Cos(th)*rad*1.18f,y[k]+(k<5?(R()-.5f)*.002f:0),Mathf.Sin(th)*rad);v.Add(q);uv.Add(new Vector2(q.x,q.z));
            }
            int Ring(int k,int a)=>1+(k-1)*A+(a%A);
            var inner=new List<int>();var outer=new List<int>();
            for(int a=0;a<A;a++)inner.AddRange(new[]{0,Ring(1,a+1),Ring(1,a)});
            for(int k=1;k<t.Length-1;k++)for(int a=0;a<A;a++)
                (k<5?inner:outer).AddRange(new[]{Ring(k,a),Ring(k,a+1),Ring(k+1,a+1),Ring(k,a),Ring(k+1,a+1),Ring(k+1,a)});
            var m=new Mesh{name="Pothole"};m.SetVertices(v);m.SetUVs(0,uv);m.subMeshCount=2;m.SetTriangles(inner,0);m.SetTriangles(outer,1);m.RecalculateNormals();m.RecalculateBounds();
            var go=new GameObject("Pothole");go.transform.SetParent(root);go.transform.position=at;go.transform.rotation=Quaternion.Euler(0,R()*360,0);
            go.AddComponent<MeshFilter>().sharedMesh=m;var mr=go.AddComponent<MeshRenderer>();mr.sharedMaterials=new[]{floor,lip};mr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            hazards.Add(new Hazard{at=at,radius=radius*1.1f});
        }
        void Ripples(Transform root,float z,float length,Material sand,int seed)
        {
            var rng=new System.Random(seed*53+11);float R()=>(float)rng.NextDouble();
            const float width=8.9f;const int nx=60,nz=72;float p1=R()*6.3f,p2=R()*6.3f,p3=R()*6.3f,p4=R()*6.3f,p5=R()*6.3f;
            float Smooth(float x){x=Mathf.Clamp01(x);return x*x*(3-2*x);}
            var v=new Vector3[nx*nz];var uv=new Vector2[nx*nz];
            for(int j=0;j<nz;j++)for(int i=0;i<nx;i++)
            {
                float x=-width/2+width*i/(nx-1),zz=-length*.75f+length*1.5f*j/(nz-1);
                // Ragged drift ends: the drift's reach varies across the road, and both edges feather into the shoulders.
                float half=length*.5f*(.8f+.14f*Mathf.Sin(x*1.3f+p1)+.07f*Mathf.Sin(x*3.4f+p2));
                float f=Smooth((half-Mathf.Abs(zz+.3f*Mathf.Sin(x*.8f+p3)))/1.1f);
                f*=Smooth((width/2-Mathf.Abs(x)-.15f*Mathf.Sin(zz*2.1f+p4))/.7f);
                // Wheels have worn two lower tracks through it.
                float track=1-.4f*(Mathf.Exp(-Mathf.Pow((x-1.05f)/.45f,2))+Mathf.Exp(-Mathf.Pow((x+1.05f)/.45f,2)));
                float ridge=.5f+.5f*Mathf.Sin((zz+.2f*Mathf.Sin(x*1.6f+p5))*Mathf.PI*2/.85f);
                v[j*nx+i]=new Vector3(x,-.007f+f*track*(.016f+.05f*ridge),zz);uv[j*nx+i]=new Vector2(x,zz);
            }
            var tri=new List<int>();
            for(int j=0;j<nz-1;j++)for(int i=0;i<nx-1;i++){int a=j*nx+i;tri.AddRange(new[]{a,a+nx,a+nx+1,a,a+nx+1,a+1});}
            var m=new Mesh{name="Sand ripples"};m.vertices=v;m.uv=uv;m.SetTriangles(tri,0);m.RecalculateNormals();m.RecalculateBounds();
            var go=new GameObject("Sand ripples across the road");go.transform.SetParent(root);go.transform.position=new Vector3(15,top,z);
            go.AddComponent<MeshFilter>().sharedMesh=m;var mr=go.AddComponent<MeshRenderer>();mr.sharedMaterial=sand;mr.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            hazards.Add(new Hazard{at=go.transform.position,length=length,ripples=true});
        }
    }
}
