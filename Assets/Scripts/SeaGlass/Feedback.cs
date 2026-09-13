using System.Collections.Generic;
using UnityEngine;
namespace SeaGlass
{
    public class Feedback : MonoBehaviour
    {
        public static Feedback Instance; public float effectsVolume=.65f,musicVolume=.25f;
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
                var mat=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));ps.GetComponent<ParticleSystemRenderer>().material=mat;pool[i]=ps;}
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
        void Update(){music.volume=musicVolume;if(!GameSession.Instance||!GameSession.Instance.truck.driving)engine.volume=0;}
        void OnDestroy(){foreach(var clip in clips.Values)Destroy(clip);if(music&&music.clip)Destroy(music.clip);if(Instance==this)Instance=null;}
    }
}
