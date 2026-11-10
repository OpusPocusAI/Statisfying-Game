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
            // Reward bursts: sparkle-shaped, rising softly, shrinking and fading out.
            if(particleMaterial){sparkle=new Material(particleMaterial){name="Reward sparkle"};sparkle.SetFloat("_Glint",1);sparkle.renderQueue=3100;}   // round 65: after the basin water, which drew over the reveal glints
            foreach(var ps in pool)
            {
                var main=ps.main;main.startSize=new ParticleSystem.MinMaxCurve(.07f,.16f);main.gravityModifier=-.05f;main.startLifetime=new ParticleSystem.MinMaxCurve(.6f,1.1f);
                var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.4f),new Keyframe(.15f,1),new Keyframe(1,0)));
                var fade=ps.colorOverLifetime;fade.enabled=true;var g=new Gradient();g.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(.9f,.5f),new GradientAlphaKey(0,1)});fade.color=g;
                var drag=ps.limitVelocityOverLifetime;drag.enabled=true;drag.drag=2.5f;
                if(sparkle)ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=sparkle;
            }
            foam=Emitter("Soap foam",2.6f,.05f,.03f,.075f,0,720,.06f);   // round 66: room for the fuller suds   // round 13: lasting clumps of suds, not specks
            // Round 19: each foam particle draws a clump of soap bubbles, turned at random (flat discs read as "flat white circles").
            if(particleMaterial){suds=new Material(particleMaterial){name="Soap suds"};suds.SetFloat("_Suds",1);suds.renderQueue=3100;// round 38: after the basin water (Transparent+50), whose multiply tinted the white suds blue
            foam.GetComponent<ParticleSystemRenderer>().sharedMaterial=suds;var fm=foam.main;fm.startRotation=new ParticleSystem.MinMaxCurve(0,Mathf.PI*2);}
            dust=Emitter("Sand puff",.7f,.9f,.16f,.32f,1.1f,100,.12f);
            // Round 65 (review 61, all three: "no splash, drips or runoff"): water flicked off the brush. Real gravity and
            // stretched along their flight, so each drop reads as a falling streak of water in a still, not a hanging dot.
            splash=Emitter("Brush splash",.5f,1f,.012f,.024f,1.4f,240,.02f);
            // Round 66 (review 62: "thin vertical blue streaks", "stray line particles" under the lifted piece): round drops. Stretched
            // along a fall seen from above, they read as lines.
            {var r=splash.GetComponent<ParticleSystemRenderer>();r.renderMode=ParticleSystemRenderMode.Billboard;
             // Round 96 (review 92: "no splash or drip shapes"): clear drops with a highlight, drawn after the basin water.
             if(particleMaterial){drop=new Material(particleMaterial){name="Water drop"};drop.SetFloat("_Drop",1);drop.renderQueue=3100;r.sharedMaterial=drop;}}
            // Round 66: the colour-revealed glints get their own world-space system. On the shared pool, the recycling station's
            // sparkles could pick the same pooled system a moment later and carry the glints off with it.
            revealGlints=Emitter("Reveal glints",1.2f,.2f,.035f,.075f,-.05f,80,.05f);
            {var main=revealGlints.main;var size=revealGlints.sizeOverLifetime;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.4f),new Keyframe(.15f,1),new Keyframe(1,0)));var drag=revealGlints.limitVelocityOverLifetime;drag.enabled=true;drag.drag=2.5f;if(sparkle)revealGlints.GetComponent<ParticleSystemRenderer>().sharedMaterial=sparkle;}
        }
        ParticleSystem foam,dust,splash,revealGlints;float nextDust;Material sparkle,suds,drop;
        // Soft, shaped particles (the Soft Feedback disc), shrinking and fading out over their life.
        ParticleSystem Emitter(string name,float life,float speed,float minSize,float maxSize,float gravity,int max,float radius)
        {
            var g=new GameObject(name);g.transform.SetParent(transform);var ps=g.AddComponent<ParticleSystem>();ps.Stop();
            var main=ps.main;main.playOnAwake=false;main.startLifetime=new ParticleSystem.MinMaxCurve(life*.6f,life);main.startSpeed=new ParticleSystem.MinMaxCurve(speed*.3f,speed);
            main.startSize=new ParticleSystem.MinMaxCurve(minSize,maxSize);main.gravityModifier=gravity;main.maxParticles=max;main.simulationSpace=ParticleSystemSimulationSpace.World;
            var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Hemisphere;shape.radius=radius;
            var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.6f),new Keyframe(.2f,1),new Keyframe(1,.2f)));
            var fade=ps.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(.9f,0),new GradientAlphaKey(.8f,.6f),new GradientAlphaKey(0,1)});fade.color=gradient;
            ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=particleMaterial;
            // Round 66: always simulated and playing (emission is off, so it only shows what is emitted). A stopped system kept stale
            // bounds at its own position and could be culled, so the reveal glints and drops sometimes never drew.
            main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;ps.Play();return ps;
        }
        // Brushing: small soap bubbles where the bristles meet the glass, plus the odd droplet.
        public void Foam(Vector3 at,float spread)
        {
            if(!foam)return;var p=new ParticleSystem.EmitParams();
            // A clump of suds: one larger head with smaller bubbles packed around it, drifting slowly outward.
            // Round 66 (review 62, all three: "little foam"): two clumps a call, larger, so suds build up around the brush and drift.
            // Round 97 (review 93, all three: the suds "popcorn clusters", "clumped white blobs", "white dots"): six small opaque particles
            // a clump read as separate white spots at basin scale. Now two large, part-transparent masses a call, overlapping their
            // neighbours, so the suds build into one soft heap of foam around the brush instead of a scatter of popcorn.
            for(int i=0;i<2;i++){var o=new Vector3(Random.Range(-spread,spread)*1.5f,.012f+Random.Range(0f,.01f),Random.Range(-spread,spread)*.9f);p.position=at+o;p.velocity=new Vector3(o.x,0,o.z)*.35f;p.startSize=Random.Range(.13f,.2f);p.startLifetime=Random.Range(2.4f,3.4f);p.rotation=Random.Range(0f,360f);p.startColor=SudsTint(.82f);foam.Emit(p,1);}
            // Round 96 (review 92: "no foam or film on the water"): a broad, faint patch of foam spreading on the water around the brush,
            // so the suds sit in a soapy film. Round 97: every call, a little denser (it went unnoticed).
            {p=new ParticleSystem.EmitParams();p.position=at+new Vector3(Random.Range(-spread,spread)*2f,.006f,Random.Range(-spread,spread)*1.4f);p.velocity=new Vector3(Random.Range(-.03f,.03f),0,Random.Range(-.03f,.03f));p.startSize=Random.Range(.24f,.36f);p.startLifetime=Random.Range(3f,4.2f);p.rotation=Random.Range(0f,360f);p.startColor=SudsTint(.42f);foam.Emit(p,1);}
            // Round 13: a low, short-lived splash. Round 32 (reviews: "no splashes or drips"): flicked droplets, so the scrub reads in a
            // still. Round 96: on the splash emitter (clear drops under real gravity); on the foam emitter they drew as foam.
            if(splash&&Random.value<.75f){splash.transform.position=at;p=new ParticleSystem.EmitParams();for(int d=0;d<3;d++){p.position=at+Vector3.up*.025f;p.velocity=new Vector3(Random.Range(-.45f,.45f),Random.Range(.5f,.85f),Random.Range(-.3f,.3f));p.startSize=Random.Range(.022f,.04f);p.startLifetime=Random.Range(.32f,.5f);p.startColor=Color.white;splash.Emit(p,1);}}
        }
        // A tiny random jitter in the suds colour seeds each clump's shape in the shader.
        static Color SudsTint(float alpha)=>new Color(Random.Range(.95f,1f),Random.Range(.97f,1f),1f,alpha);
        // Brushing: droplets thrown off the bristles in the direction of the stroke, arcing up and falling back into the bowl.
        public void Splash(Vector3 at,Vector3 strokeVelocity)
        {
            if(!splash)return;splash.transform.position=at;var p=new ParticleSystem.EmitParams();var flick=Vector3.ClampMagnitude(new Vector3(strokeVelocity.x,0,strokeVelocity.z),1.2f);
            int n=Random.Range(3,7);   // round 96: more and larger drops (they read as specks)
            for(int i=0;i<n;i++){p.position=at+new Vector3(Random.Range(-.03f,.03f),.02f,Random.Range(-.02f,.02f));p.velocity=flick*Random.Range(.35f,.8f)+new Vector3(Random.Range(-.18f,.18f),Random.Range(.45f,.85f),Random.Range(-.18f,.18f));p.startSize=Random.Range(.022f,.042f);p.startLifetime=Random.Range(.32f,.5f);p.startColor=Color.white;splash.Emit(p,1);}
        }
        // The colour-revealed moment: a ring of glints in the glass colour rising off the piece, a bright star at its centre,
        // and a crown of droplets as it lifts out of the water.
        public void Reveal(Vector3 at,Color colour,float radius)
        {
            var ring=revealGlints;ring.transform.position=at;if(splash)splash.transform.position=at;var p=new ParticleSystem.EmitParams();   // world space
            for(int i=0;i<22;i++){float a=i/22f*Mathf.PI*2+Random.Range(-.12f,.12f);var o=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a))*radius*Random.Range(.55f,1f);p.position=at+o;p.velocity=o.normalized*Random.Range(.15f,.35f)+Vector3.up*Random.Range(.25f,.55f);p.startSize=Random.Range(.035f,.075f);p.startLifetime=Random.Range(.9f,1.4f);p.startColor=Color.Lerp(colour,Color.white,Random.Range(.25f,.6f))*1.3f;ring.Emit(p,1);}
            p.position=at+Vector3.up*.03f;p.velocity=Vector3.up*.12f;p.startSize=.16f;p.startLifetime=.8f;p.startColor=Color.Lerp(colour,Color.white,.75f)*1.6f;ring.Emit(p,1);
            if(splash)for(int i=0;i<16;i++){float a=i/16f*Mathf.PI*2;var o=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));p=new ParticleSystem.EmitParams();p.position=at+o*radius*.8f;p.velocity=o*Random.Range(.25f,.45f)+Vector3.up*Random.Range(.7f,1.05f);p.startSize=Random.Range(.02f,.034f);p.startLifetime=Random.Range(.45f,.65f);p.startColor=new Color(.9f,.98f,1f,.9f);splash.Emit(p,1);}
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
            for(float t=0;t<1;t+=Time.deltaTime/.38f){float ease=1-Mathf.Pow(1-t,3);go.transform.position=Vector3.Lerp(from,to,ease)+Vector3.up*Mathf.Sin(t*Mathf.PI)*.18f;go.transform.localScale=scale*Mathf.Lerp(1,.4f,ease);yield return null;}
            Destroy(go);
        }
        void Update(){music.volume=musicVolume;if(!GameSession.Instance||GameSession.Instance.ControlsBlocked||!GameSession.Instance.truck.driving)engine.volume=0;}
        void OnDestroy(){if(sparkle)Destroy(sparkle);if(suds)Destroy(suds);if(drop)Destroy(drop);foreach(var clip in clips.Values)Destroy(clip);if(music&&music.clip)Destroy(music.clip);if(Instance==this)Instance=null;}
    }
}
