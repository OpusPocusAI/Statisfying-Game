using System.Collections.Generic;
using UnityEngine;
namespace SeaGlass
{
    public class Feedback : MonoBehaviour
    {
        public static Feedback Instance; public float effectsVolume=.65f,musicVolume=.25f; public Material particleMaterial;
        AudioSource effects,engine,music; readonly Dictionary<string,AudioClip> clips=new(); ParticleSystem[] pool;int cursor;float nextAction;
        void Awake()
        {
            Instance=this;effects=gameObject.AddComponent<AudioSource>();engine=gameObject.AddComponent<AudioSource>();music=gameObject.AddComponent<AudioSource>();
            string[] names={"sand","plastic","metal","glass","water","sort","engine","recycle","upgrade","complete"};
            for(int j=0;j<names.Length;j++)clips[names[j]]=Tone(names[j],j);
            engine.clip=clips["engine"];engine.loop=true;engine.volume=0;engine.Play();
            music.clip=MakeMusic();music.loop=true;music.volume=musicVolume;music.Play();
            pool=new ParticleSystem[6];for(int i=0;i<pool.Length;i++)
            {var g=new GameObject("Pooled foam and sparkle");g.transform.SetParent(transform);var ps=g.AddComponent<ParticleSystem>();ps.Stop();var main=ps.main;main.playOnAwake=false;main.startLifetime=.7f;main.startSpeed=1.3f;main.startSize=.07f;main.gravityModifier=.2f;main.maxParticles=60;var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.12f;
                ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=particleMaterial;pool[i]=ps;}
            foam=Emitter("Soap foam",1.3f,.12f,.022f,.05f,-.04f,120,.08f);
            dust=Emitter("Sand puff",.7f,.9f,.16f,.32f,1.1f,100,.12f);
        }
        ParticleSystem foam,dust;float nextDust;
        // Soft, shaped particles (the Soft Feedback disc), shrinking and fading out over their life.
        ParticleSystem Emitter(string name,float life,float speed,float minSize,float maxSize,float gravity,int max,float radius)
        {
            var g=new GameObject(name);g.transform.SetParent(transform);var ps=g.AddComponent<ParticleSystem>();ps.Stop();
            var main=ps.main;main.playOnAwake=false;main.startLifetime=new ParticleSystem.MinMaxCurve(life*.6f,life);main.startSpeed=new ParticleSystem.MinMaxCurve(speed*.3f,speed);
            main.startSize=new ParticleSystem.MinMaxCurve(minSize,maxSize);main.gravityModifier=gravity;main.maxParticles=max;main.simulationSpace=ParticleSystemSimulationSpace.World;
            var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Hemisphere;shape.radius=radius;
            var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.6f),new Keyframe(.2f,1),new Keyframe(1,.2f)));
            var fade=ps.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(.9f,0),new GradientAlphaKey(.8f,.6f),new GradientAlphaKey(0,1)});fade.color=gradient;
            ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=particleMaterial;return ps;
        }
        // Brushing: small soap bubbles where the bristles meet the glass, plus the odd droplet.
        public void Foam(Vector3 at,float spread)
        {
            if(!foam)return;var p=new ParticleSystem.EmitParams();
            for(int i=0;i<4;i++){p.position=at+new Vector3(Random.Range(-spread,spread),.02f,Random.Range(-spread,spread)*.6f);p.startColor=new Color(.94f,.98f,1f,.9f);foam.Emit(p,1);}
            if(Random.value<.35f){p.position=at+Vector3.up*.05f;p.velocity=new Vector3(Random.Range(-.6f,.6f),Random.Range(.8f,1.4f),Random.Range(-.4f,.4f));p.startSize=.025f;p.startColor=new Color(.7f,.88f,.9f,.8f);foam.Emit(p,1);}
        }
        // Digging: puffs of sand kicked up from the mound.
        public void Dust(Vector3 at)
        {
            if(!dust||Time.time<nextDust)return;nextDust=Time.time+.14f;var p=new ParticleSystem.EmitParams();
            for(int i=0;i<5;i++){p.position=at+new Vector3(Random.Range(-.15f,.15f),.08f,Random.Range(-.15f,.15f));p.velocity=new Vector3(Random.Range(-.5f,.5f),Random.Range(.7f,1.4f),Random.Range(-.5f,.5f));p.startColor=new Color(1f,.95f,.84f,.75f);dust.Emit(p,1);}
        }
        static AudioClip Tone(string name,int index)
        {
            const int rate=22050;float duration=name=="complete"?2.5f:name=="engine"?1:.28f;var data=new float[(int)(rate*duration)];var rng=new System.Random(index+47);
            for(int i=0;i<data.Length;i++){float t=(float)i/rate;float envelope=name=="engine"?.12f:Mathf.Exp(-t*(name=="complete"?1.8f:14));float f= name=="engine"?55: name=="complete"?261.63f:220+index*63;
                float v=Mathf.Sin(2*Mathf.PI*f*t)+.3f*Mathf.Sin(2*Mathf.PI*f*2.01f*t);
                if(name=="complete"||name=="upgrade")v+=.5f*Mathf.Sin(2*Mathf.PI*f*1.25f*t)+.4f*Mathf.Sin(2*Mathf.PI*f*1.5f*t);
                if(name=="sand"||name=="water"||name=="recycle")v=(float)(rng.NextDouble()*2-1)*.7f;
                data[i]=v*envelope*.19f;
            }var clip=AudioClip.Create("Original "+name,data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        static AudioClip MakeMusic()
        {
            const int rate=22050;var data=new float[rate*16];float[] notes={130.81f,164.81f,196,246.94f,146.83f,174.61f,220,196};
            for(int i=0;i<data.Length;i++){float t=(float)i/rate;int beat=(int)(t/2);float local=t%2;float env=Mathf.Sin(Mathf.PI*local/2)*.06f;data[i]=(Mathf.Sin(t*notes[beat]*Mathf.PI*2)+.5f*Mathf.Sin(t*notes[beat]*2*Mathf.PI*2))*env;}
            var clip=AudioClip.Create("Original tidal arpeggio",data.Length,1,rate,false);clip.SetData(data,0);return clip;
        }
        public void Play(string sound,float pitch=1){if(!clips.ContainsKey(sound))return;effects.pitch=pitch;effects.PlayOneShot(clips[sound],effectsVolume);}
        public void LoopAction(string sound){if(Time.time<nextAction)return;nextAction=Time.time+.16f;Play(sound,Random.Range(.9f,1.1f));}
        public void Engine(float throttle){engine.volume=effectsVolume*.18f;engine.pitch=.6f+throttle*.8f;if(throttle>.2f)LoopAction("metal");}
        public void Burst(Vector3 at,Color color,int count=12){var p=pool[cursor++%pool.Length];p.transform.position=at;var main=p.main;main.startColor=color;p.Emit(count);}
        public void Fly(Mesh mesh,Material material,Color color,Vector3 from,Vector3 to,Vector3 scale,Quaternion rotation)
        {if(mesh&&material)StartCoroutine(Flight(mesh,material,color,from,to,scale,rotation));}
        System.Collections.IEnumerator Flight(Mesh mesh,Material material,Color color,Vector3 from,Vector3 to,Vector3 scale,Quaternion rotation)
        {
            var go=new GameObject("Glass transfer");go.transform.SetParent(transform);go.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;var b=new MaterialPropertyBlock();b.SetColor("_BaseColor",color);renderer.SetPropertyBlock(b);
            go.transform.rotation=rotation;
            for(float t=0;t<1;t+=Time.deltaTime/.38f){float ease=1-Mathf.Pow(1-t,3);go.transform.position=Vector3.Lerp(from,to,ease)+Vector3.up*Mathf.Sin(t*Mathf.PI)*.18f;go.transform.localScale=scale*Mathf.Lerp(1,.6f,ease);yield return null;}
            Destroy(go);
        }
        void Update(){music.volume=musicVolume;if(!GameSession.Instance||GameSession.Instance.ControlsBlocked||!GameSession.Instance.truck.driving)engine.volume=0;}
        void OnDestroy(){foreach(var clip in clips.Values)Destroy(clip);if(music&&music.clip)Destroy(music.clip);if(Instance==this)Instance=null;}
    }
}
