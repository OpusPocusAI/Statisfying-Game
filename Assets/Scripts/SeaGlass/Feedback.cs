using System.Collections.Generic;
using UnityEngine;
namespace SeaGlass
{
    public class Feedback : MonoBehaviour
    {
        public static Feedback Instance; public float effectsVolume=.65f,musicVolume=.25f; public Material particleMaterial;
        // Round 114: the brushing suds as lit mesh particles (foam_clump, FoamClump.mat; set by Round114Pass).
        public Mesh foamMesh; public Material foamMaterial;
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
            // Round 114 (review 110, all three: the suds "flat white cut-out shapes", "a flat decal splat, not volumetric"): heaps of foam
            // as real geometry standing up off the water, lit, each turned its own way about the vertical.
            if(foamMesh&&foamMaterial){var fr=foam.GetComponent<ParticleSystemRenderer>();fr.renderMode=ParticleSystemRenderMode.Mesh;fr.mesh=foamMesh;fr.sharedMaterial=foamMaterial;fr.alignment=ParticleSystemRenderSpace.World;var fm=foam.main;fm.startRotation3D=true;
             var fs=foam.sizeOverLifetime;fs.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.35f),new Keyframe(.15f,1),new Keyframe(.85f,.95f),new Keyframe(1,0)));}
            dust=Emitter("Sand puff",.7f,.9f,.16f,.32f,1.1f,100,.12f);
            // Round 65 (review 61, all three: "no splash, drips or runoff"): water flicked off the brush. Real gravity and
            // stretched along their flight, so each drop reads as a falling streak of water in a still, not a hanging dot.
            splash=Emitter("Brush splash",.5f,1f,.012f,.024f,1.4f,240,.02f);
            // Round 66 (review 62: "thin vertical blue streaks", "stray line particles" under the lifted piece): round drops. Stretched
            // along a fall seen from above, they read as lines.
            {var r=splash.GetComponent<ParticleSystemRenderer>();r.renderMode=ParticleSystemRenderMode.Billboard;
             // Round 96 (review 92: "no splash or drip shapes"): clear drops with a highlight, drawn after the basin water.
             if(particleMaterial){drop=new Material(particleMaterial){name="Water drop"};drop.SetFloat("_Drop",1);drop.renderQueue=3100;r.sharedMaterial=drop;}}
            // Round 98: drips off the lifted piece, under light gravity so a string of them hangs between the glass and the water.
            drips=Emitter("Glass drips",.7f,.1f,.024f,.04f,.35f,120,.01f);
            {var size=drips.sizeOverLifetime;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.7f),new Keyframe(.25f,1),new Keyframe(1,.8f)));if(drop){var tear=new Material(drop){name="Water teardrop"};tear.SetFloat("_Drop",2);drips.GetComponent<ParticleSystemRenderer>().sharedMaterial=tear;}}   // round 114: falling drops drawn as teardrops
            // Round 66: the colour-revealed glints get their own world-space system. On the shared pool, the recycling station's
            // sparkles could pick the same pooled system a moment later and carry the glints off with it.
            revealGlints=Emitter("Reveal glints",1.2f,.2f,.035f,.075f,-.05f,80,.05f);
            {var main=revealGlints.main;var size=revealGlints.sizeOverLifetime;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.4f),new Keyframe(.15f,1),new Keyframe(1,0)));var drag=revealGlints.limitVelocityOverLifetime;drag.enabled=true;drag.drag=2.5f;if(sparkle)revealGlints.GetComponent<ParticleSystemRenderer>().sharedMaterial=sparkle;}
            // Round 104 (review 100, all three: the suds "stamped circle clumps", "cotton-ball spheres"; no splashes or drips seen): soap
            // foam on water is a flat, lacy raft, a network of fine bubble walls with the water showing through between them, lying on
            // the surface; and a falling drop shows in a still by the ring it spreads on the water. Both lie flat on the water.
            film=Flat("Foam film",3.8f,160,"_Lace",new AnimationCurve(new Keyframe(0,.8f),new Keyframe(.3f,1),new Keyframe(1,1.05f)));
            motes=Emitter("Light motes",4f,0,.03f,.06f,0,600,.01f);
            {var size=motes.sizeOverLifetime;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.2f),new Keyframe(.25f,1),new Keyframe(.8f,1),new Keyframe(1,0)));
             var fade=motes.colorOverLifetime;var g=new Gradient();g.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.2f),new GradientAlphaKey(.9f,.75f),new GradientAlphaKey(0,1)});fade.color=g;
             if(particleMaterial){var m=new Material(particleMaterial){name="Light motes"};m.SetFloat("_Mote",1);m.renderQueue=3100;   /* round 113: flecks of light, not round dots */motes.GetComponent<ParticleSystemRenderer>().sharedMaterial=m;}}
            rings=Flat("Drip rings",.9f,120,"_Ring",new AnimationCurve(new Keyframe(0,.45f),new Keyframe(1,1)));
        }
        ParticleSystem film,rings,motes;
        // Round 106 (review 102, all three: nothing happens in 06 or 08; "generic four-point stars"): a glint that runs across the clean
        // piece in the palm, a short, soft flash of its colour, not a stock star; and slow motes of coloured light drifting in the
        // light of a finished mosaic, as dust does in a coloured window's beam.
        public void Twinkle(Vector3 at,Color colour,float size)
        {
            if(!revealGlints)return;var p=new ParticleSystem.EmitParams();p.position=at;p.velocity=Vector3.up*.004f;p.startSize=size;
            p.rotation=Random.Range(-25f,25f);p.startLifetime=Random.Range(.35f,.6f);p.startColor=Color.Lerp(colour,Color.white,.55f)*1.2f;revealGlints.Emit(p,1);
        }
        public void Mote(Vector3 at,Color colour)
        {
            if(!motes)return;var p=new ParticleSystem.EmitParams();p.position=at;
            p.velocity=new Vector3(Random.Range(-.03f,.03f),Random.Range(.02f,.07f),Random.Range(-.03f,.03f));p.startSize=Random.Range(.17f,.36f);   /* round 113: a slender fleck needs more length than a round dot to read */   // seen from across the room (.025-.06 were specks)
            p.rotation=Random.Range(0f,360f);p.angularVelocity=Random.Range(-25f,25f);   // round 113: each fleck turns at its own angle and slowly spins
            p.startLifetime=Random.Range(2.5f,4f);var c=Color.Lerp(colour,Color.white,.3f)*1.2f;c.a=Random.Range(.6f,.95f);p.startColor=c;motes.Emit(p,1);
        }
        // A system whose particles lie flat on the water (world-aligned quads turned to face up), drawn with one Soft Feedback mode.
        ParticleSystem Flat(string name,float life,int max,string mode,AnimationCurve grow)
        {
            var ps=Emitter(name,life,0,.1f,.2f,0,max,.01f);var main=ps.main;main.startRotation3D=true;
            var size=ps.sizeOverLifetime;size.size=new ParticleSystem.MinMaxCurve(1,grow);
            var r=ps.GetComponent<ParticleSystemRenderer>();r.alignment=ParticleSystemRenderSpace.World;
            if(particleMaterial){var m=new Material(particleMaterial){name=name};m.SetFloat(mode,1);m.renderQueue=3090;r.sharedMaterial=m;}
            return ps;
        }
        static Vector3 FlatTurn()=>new Vector3(90*Mathf.Deg2Rad,Random.Range(0f,Mathf.PI*2),0);
        // A ring spreading on the water where a drop lands.
        public void Ripple(Vector3 at,float size)
        {
            if(!rings)return;var p=new ParticleSystem.EmitParams();p.position=at;p.velocity=Vector3.zero;p.rotation3D=FlatTurn()*Mathf.Rad2Deg;
            p.startSize=size*Random.Range(.85f,1.15f);p.startLifetime=Random.Range(.7f,1f);p.startColor=Color.white;rings.Emit(p,1);
        }
        ParticleSystem foam,dust,splash,revealGlints,drips;float nextDust;Material sparkle,suds,drop;
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
            // Round 104: one smaller mass a call (heaps of large masses read as cotton balls); the raft of lace below carries the foam.
            for(int i=0;i<1;i++){var o=new Vector3(Random.Range(-spread,spread)*1.5f,.01f+Random.Range(0f,.006f),Random.Range(-spread,spread)*.9f);p.position=at+o;p.velocity=new Vector3(o.x,0,o.z)*.35f;p.startSize=Random.Range(.08f,.13f);p.startLifetime=Random.Range(2.4f,3.4f);p.rotation=Random.Range(0f,360f);p.startColor=SudsTint(.82f);
                if(foamMesh&&foamMaterial){if(Random.value>.75f)continue;p.rotation3D=new Vector3(Random.Range(-8f,8f),Random.Range(0f,360f),Random.Range(-8f,8f));p.startSize=Random.Range(.085f,.15f);p.position=at+new Vector3(o.x,.004f,o.z);p.startColor=Color.white;}   // round 114: a heap sitting in the water
                foam.Emit(p,1);}
            // Round 96 (review 92: "no foam or film on the water"): a broad, faint patch of foam spreading on the water around the brush,
            // so the suds sit in a soapy film. Round 97: every call, a little denser (it went unnoticed).
            // Round 104: the film is a flat raft of lace on the water (it was a faint upright disc).
            if(film){p=new ParticleSystem.EmitParams();p.position=at+new Vector3(Random.Range(-spread,spread)*2.4f,.004f,Random.Range(-spread,spread)*1.8f);p.velocity=new Vector3(Random.Range(-.04f,.04f),0,Random.Range(-.04f,.04f));p.startSize=Random.Range(.16f,.26f);p.startLifetime=Random.Range(3f,4.4f);p.rotation3D=FlatTurn()*Mathf.Rad2Deg;p.startColor=SudsTint(.9f);film.Emit(p,1);}
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
            // Round 98 (review 94, all three: "evenly scattered", "evenly ringed" stars): glints catch on the wet glass itself, scattered
            // over the piece and just off its edge, mostly tiny with a few large ones, each turned its own way; not a ring around it.
            // They start at the height the piece lifts to, and since the glass writes depth, each is pulled a little toward the eye so it
            // sits in front of the lifted, tilted piece instead of inside it.
            var cam=Camera.main;
            for(int i=0;i<20;i++){var o=Random.insideUnitCircle*radius*1.1f;p.position=at+new Vector3(o.x,Random.Range(.07f,.16f),o.y);
                if(cam)p.position+=(cam.transform.position-p.position).normalized*.12f;
                p.velocity=new Vector3(o.x,0,o.y).normalized*Random.Range(.02f,.1f)+Vector3.up*Random.Range(.03f,.12f);
                bool big=i<5;p.startSize=big?Random.Range(.14f,.2f):Random.Range(.035f,.065f);p.rotation=Random.Range(-25f,25f);p.startLifetime=Random.Range(.8f,1.3f);p.startColor=Color.Lerp(colour,Color.white,Random.Range(.2f,.5f))*1.3f;ring.Emit(p,1);}
            // Round 104: the water the piece leaves rings outward around it.
            for(int i=0;i<3;i++)Ripple(new Vector3(at.x,at.y-.035f,at.z),radius*(1.6f+i*.7f));
            p=new ParticleSystem.EmitParams();p.position=at+Vector3.up*.03f;p.velocity=Vector3.up*.08f;p.startSize=.18f;p.rotation=12;p.startLifetime=.8f;p.startColor=Color.Lerp(colour,Color.white,.7f)*1.6f;ring.Emit(p,1);
            // A crown of water thrown up as the piece lifts: larger, higher-contrast drops (they went unseen).
            if(splash)for(int i=0;i<18;i++){float a=i/18f*Mathf.PI*2+Random.Range(-.1f,.1f);var o=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));p=new ParticleSystem.EmitParams();p.position=at+o*radius*.8f;p.velocity=o*Random.Range(.2f,.4f)+Vector3.up*Random.Range(.6f,.95f);p.startSize=Random.Range(.03f,.05f);p.startLifetime=Random.Range(.5f,.7f);p.startColor=Color.white;splash.Emit(p,1);}
        }
        // Round 98 (review 94, all three: "no splashes, drips or runoff"): water dripping off the lifted piece's lower edge, falling
        // slowly enough to hang in a still as a string of drops between the glass and the water.
        public void Drip(Vector3 at,float waterY)
        {
            if(!drips)return;drips.transform.position=at;var p=new ParticleSystem.EmitParams();p.position=at;p.velocity=new Vector3(Random.Range(-.02f,.02f),Random.Range(-.15f,-.05f),Random.Range(-.02f,.02f));
            // Round 104: each drop lands on the water as a spreading ring, the part of a drip that reads in a still.
            if(Random.value<.5f)Ripple(new Vector3(at.x,waterY+.003f,at.z),Random.Range(.07f,.12f));
            p.startSize=Random.Range(.045f,.07f);p.startLifetime=Random.Range(.45f,.7f);p.startColor=Color.white;drips.Emit(p,1);   // round 114: larger teardrops (they went unseen)
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
