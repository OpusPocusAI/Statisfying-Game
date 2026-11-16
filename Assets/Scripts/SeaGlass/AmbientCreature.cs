using UnityEngine;

namespace SeaGlass
{
    // Ambient life (owner, November 16: Tchia is the art target). Seabirds wheel over the water, crabs scuttle on the wet sand
    // and dig in when you come close, butterflies flutter over the flowers and little fish dart in the clear shallows.
    // Each creature is one fused mesh (ArtSource/blender/ambient_life.py) animated by bending its own vertices: wings fold
    // about the shoulder, legs step, tails sweep. No colliders, nothing the player can bump into or interact with, and the
    // vertex work runs only while the creature is on screen. Placed by AmbientLifePass.
    [DisallowMultipleComponent]
    public class AmbientCreature : MonoBehaviour
    {
        public enum Kind { Seabird, Crab, Butterfly, Fish }
        public Kind kind;
        [Tooltip("Circuit centre (birds, fish) or home spot on the ground (crabs, butterflies).")] public Vector3 home;
        [Tooltip("Circuit radii across and along the coast (birds, fish); x = wander radius (crabs, butterflies).")] public Vector2 radius = new(6, 10);
        public float speed = 6, phase, direction = 1;
        [Tooltip("Crabs never wander seaward of this X (the water's edge).")] public float minX = -31;
        [Tooltip("Fish: offset of this fish within its school.")] public Vector3 schoolOffset;
        // Crabs and butterflies: the visible ground height around home, baked by AmbientLifePass (N x N samples over home +- groundHalf). Some low
        // beach shapes have no collider, so a raycast alone would let a crab walk under their surface.
        [HideInInspector] public float[] groundHeights; [HideInInspector] public int groundN; [HideInInspector] public float groundHalf;

        Mesh mesh; Vector3[] baseV, baseN, v, n; MeshRenderer view; System.Random rnd;
        float t, flapTimer, flapAngle, beat, heading;
        Vector3 vel, target; float timer, sink; int state; // crab: 0 pause, 1 walk, 2 flee, 3 hidden, 4 surfacing; butterfly: 0 fly, 1 rest
        static readonly RaycastHit[] Hits = new RaycastHit[8];

        void Awake()
        {
            var filter = GetComponent<MeshFilter>(); view = GetComponent<MeshRenderer>();
            if (!filter || !filter.sharedMesh) { enabled = false; return; }
            mesh = Instantiate(filter.sharedMesh); mesh.name = filter.sharedMesh.name + " (live)"; filter.sharedMesh = mesh;
            baseV = mesh.vertices; baseN = mesh.normals; v = (Vector3[])baseV.Clone(); n = (Vector3[])baseN.Clone();
            var b = mesh.bounds; b.Expand(b.size.magnitude); mesh.bounds = b;      // room for raised wings and steps, no per-frame recalculation
            mesh.MarkDynamic();
            rnd = new System.Random(GetInstanceID());
            t = phase; flapTimer = R(0, 4); heading = transform.eulerAngles.y;
            target = home; timer = R(.5f, 3);
        }

        float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);

        void Update() => Tick(Mathf.Min(Time.deltaTime, .1f));

        // Steps the creature forward by hand (editor review captures and tests), posing its mesh even when off screen.
        public void Advance(float seconds, float step = 1 / 30f)
        {
            if (!mesh) Awake();
            if (!enabled) return;
            stepping = true; for (float s = 0; s < seconds; s += step) Tick(step); stepping = false;
        }
        bool stepping;
        bool Seen => stepping || view.isVisible;

        void Tick(float dt)
        {
            t += dt;
            switch (kind)
            {
                case Kind.Seabird: Bird(dt); break;
                case Kind.Crab: Crab(dt); break;
                case Kind.Butterfly: Butterfly(dt); break;
                case Kind.Fish: Fish(dt); break;
            }
        }

        // ------------------------------------------------------------ seabird: a slow wheeling glide, bursts of wing beats
        Vector3 Circuit(float a, float lift) => home + new Vector3(Mathf.Cos(a) * radius.x, lift, Mathf.Sin(a) * radius.y);

        void Bird(float dt)
        {
            float r = (radius.x + radius.y) * .5f, a = direction * t * speed / r;
            float swell = Mathf.Clamp(home.y / 12, .12f, 1);   // low skimmers barely rise and fall, high ones drift through metres
            float lift = (Mathf.Sin(t * .23f + phase) * 1.6f + Mathf.Sin(t * .61f + phase * 2) * .5f) * swell;
            var p = Circuit(a, lift); var ahead = Circuit(a + direction * .05f, (Mathf.Sin((t + .05f * r / speed) * .23f + phase) * 1.6f + Mathf.Sin(t * .61f + phase * 2) * .5f) * swell);
            var fwd = (ahead - p).normalized;
            float yaw = Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg, turn = Mathf.DeltaAngle(heading, yaw) / Mathf.Max(dt, .0001f); heading = yaw;
            float bank = Mathf.Clamp(-turn * .55f, -32, 32);
            transform.SetPositionAndRotation(p, Quaternion.Euler(-fwd.y * 40, yaw, 0) * Quaternion.Euler(0, 0, bank));
            // Mostly gliding on slightly raised wings; every few seconds three to five deep beats.
            flapTimer -= dt;
            if (flapTimer < 0) { beat = Mathf.Round(R(3, 5)); flapTimer = R(3.5f, 7.5f); }
            float target = 9;
            if (beat > 0) { flapAngle += dt * 2.7f; target = Mathf.Sin(flapAngle * Mathf.PI * 2) * 38 + 6; if (flapAngle >= 1) { flapAngle = 0; beat--; } }
            if (!Seen) return;
            Wings(.035f, .46f, Mathf.Lerp(LastWing, target, beat > 0 ? 1 : dt * 3), .7f);
        }
        float LastWing;

        // Bend each wing up about the shoulder (local X is the span, Y up); the angle grows a little toward the tip.
        void Wings(float shoulder, float span, float degrees, float tipGain)
        {
            LastWing = degrees;
            for (int i = 0; i < baseV.Length; i++)
            {
                var p = baseV[i]; float ax = Mathf.Abs(p.x), d = ax - shoulder;
                if (d <= -.015f) { v[i] = p; n[i] = baseN[i]; continue; }
                float w = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(-.015f, .03f, d));
                float ang = degrees * Mathf.Deg2Rad * w * (1 + tipGain * Mathf.Max(0, d) / span), c = Mathf.Cos(ang), s = Mathf.Sin(ang), side = p.x < 0 ? -1 : 1;
                // Rotate (outward distance from the shoulder, height) about the shoulder line, mirrored for the left wing.
                v[i] = new Vector3(side * (shoulder + d * c - p.y * s), d * s + p.y * c, p.z);
                var q = baseN[i]; float nx = q.x * side;
                n[i] = new Vector3(side * (nx * c - q.y * s), nx * s + q.y * c, q.z);
            }
            mesh.vertices = v; mesh.normals = n;
        }

        // ------------------------------------------------------------ crab: scuttle sideways, pause, flee and dig in
        Transform Player => Camera.main ? Camera.main.transform : null;

        float Baked(float x, float z)
        {
            float u = Mathf.Clamp01((x - home.x + groundHalf) / (2 * groundHalf)) * (groundN - 1), v = Mathf.Clamp01((z - home.z + groundHalf) / (2 * groundHalf)) * (groundN - 1);
            int i = Mathf.Min((int)u, groundN - 2), j = Mathf.Min((int)v, groundN - 2); float fu = u - i, fv = v - j;
            float H(int a, int b) => groundHeights[b * groundN + a];
            return Mathf.Lerp(Mathf.Lerp(H(i, j), H(i + 1, j), fu), Mathf.Lerp(H(i, j + 1), H(i + 1, j + 1), fu), fv);
        }

        bool HasGrid => groundHeights != null && groundN > 1 && groundHeights.Length == groundN * groundN;

        bool Ground(Vector3 p, out Vector3 at, out Vector3 normal)
        {
            if (groundHeights != null && groundN > 1 && groundHeights.Length == groundN * groundN)
            {
                float y = Baked(p.x, p.z), e = .15f;
                normal = new Vector3(Baked(p.x - e, p.z) - Baked(p.x + e, p.z), 2 * e, Baked(p.x, p.z - e) - Baked(p.x, p.z + e)).normalized;
                at = new Vector3(p.x, y, p.z); return true;
            }
            at = p; normal = Vector3.up; float best = float.MinValue; bool found = false;
            int k = Physics.RaycastNonAlloc(p + Vector3.up * 1.5f, Vector3.down, Hits, 4, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < k; i++)
            {
                // Not the player, and not the top of a prop or rock (the creature never climbs above its home ground).
                var c = Hits[i].collider; if (c is CharacterController || Hits[i].point.y - p.y > .35f || Hits[i].point.y > home.y + .45f) continue;
                if (Hits[i].point.y > best) { best = Hits[i].point.y; at = Hits[i].point; normal = Hits[i].normal; found = true; }
            }
            return found;
        }

        void Crab(float dt)
        {
            var player = Player; var pos = transform.position;
            float near = player ? Vector2.Distance(new Vector2(player.position.x, player.position.z), new Vector2(pos.x, pos.z)) : 99;
            if (state < 2 && near < 3.4f) { state = 2; timer = R(.9f, 1.4f); var away = pos - (player ? player.position : pos); away.y = 0; target = pos + away.normalized * 3; var off = Vector3.ClampMagnitude(Flat(target - home), radius.x + 1); target = home + off; }   // flee, but never beyond the home patch (placement keeps it clear)
            float move = 0;
            switch (state)
            {
                case 0: timer -= dt; if (timer < 0) { state = 1; var o = Random.insideUnitCircle * radius.x; target = home + new Vector3(o.x, 0, o.y); timer = 4; } break;
                case 1: move = 1.1f; timer -= dt; if (timer < 0 || Flat(target - pos).magnitude < .1f) { state = 0; timer = R(1.2f, 4f); } break;
                case 2: move = 3.2f; timer -= dt; if (timer < 0) { state = 3; timer = R(6, 10); sink = 0; } break;
                case 3:
                    sink = Mathf.Min(1, sink + dt * 2.2f); timer -= dt;
                    if (sink >= 1) view.enabled = false;
                    if (timer < 0 && near > 7) { var o = Random.insideUnitCircle * radius.x; pos = home + new Vector3(o.x, 0, o.y); state = 4; view.enabled = true; }
                    break;
                case 4: sink = Mathf.Max(0, sink - dt * 1.6f); if (sink <= 0) { state = 0; timer = R(1, 3); } break;
            }
            var step = Flat(target - pos);
            if (move > 0 && step.sqrMagnitude > .0001f)
            {
                var dir = step.normalized; pos += dir * Mathf.Min(step.magnitude, move * dt);
                // Crabs walk sideways: turn so the side, not the face, leads (whichever side needs less turning).
                float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg - 90; if (Mathf.Abs(Mathf.DeltaAngle(heading, yaw + 180)) < Mathf.Abs(Mathf.DeltaAngle(heading, yaw))) yaw += 180;
                heading = Mathf.MoveTowardsAngle(heading, yaw, dt * (state == 2 ? 720 : 240));
            }
            pos.x = Mathf.Clamp(pos.x, minX, -21f);
            if (CoastalPlayer.Indoors(pos)) pos.x = -17f;
            Ground(pos, out var at, out var normal);
            pos.y = at.y - sink * .14f;
            transform.SetPositionAndRotation(pos, Quaternion.FromToRotation(Vector3.up, Vector3.Slerp(Vector3.up, normal, .6f)) * Quaternion.Euler(0, heading, 0));
            beat += dt * (move > 0 ? 6 + move * 5 : 0);
            if (!Seen || !view.enabled) return;
            // Legs: four a side step in alternating pairs; the claws lift a little while the crab stands.
            float wave = state == 0 ? Mathf.Max(0, Mathf.Sin(t * 1.7f + phase)) * .6f : 0;
            for (int i = 0; i < baseV.Length; i++)
            {
                var p = baseV[i]; float ax = Mathf.Abs(p.x);
                if (p.z > .06f && ax < .1f) { v[i] = p + new Vector3(0, wave * .014f * Mathf.InverseLerp(.06f, .12f, p.z), 0); continue; }   // claws
                if (ax < .08f) { v[i] = p; continue; }
                float w = Mathf.InverseLerp(.08f, .14f, ax), ph = beat + p.z * 45 + (p.x > 0 ? Mathf.PI : 0);
                v[i] = p + new Vector3(0, Mathf.Max(0, Mathf.Sin(ph)) * .016f * w, Mathf.Cos(ph) * .01f * w);
            }
            mesh.vertices = v;
        }

        static Vector3 Flat(Vector3 d) { d.y = 0; return d; }

        // ------------------------------------------------------------ butterfly: erratic flutter around its flowers, rests
        void Butterfly(float dt)
        {
            var pos = transform.position; timer -= dt;
            if (state == 0)
            {
                if (timer < 0 || (target - pos).sqrMagnitude < .04f)
                {
                    if (R(0, 1) < .18f && Ground(home + new Vector3(R(-radius.x, radius.x), 0, R(-radius.x, radius.x)), out var landAt, out _)) { target = landAt + Vector3.up * .02f; state = 1; timer = R(3, 7); }
                    else
                    {   // a point in the air over the ground there (owner playtest, Nov 16: they flew through the dunes)
                        var p = home + new Vector3(R(-radius.x, radius.x), 0, R(-radius.x, radius.x));
                        target = new Vector3(p.x, (HasGrid ? Baked(p.x, p.z) : home.y) + R(.35f, 1.3f), p.z); timer = R(1, 2.5f);
                    }
                }
                var want = (target - pos).normalized * speed + new Vector3(Mathf.Sin(t * 3.1f + phase), Mathf.Sin(t * 9.3f) * 1.4f, Mathf.Cos(t * 2.7f + phase)) * .6f;
                vel = Vector3.Lerp(vel, want, dt * 3);
                if (state == 1 && (target - pos).sqrMagnitude > .0025f) { vel = Vector3.Lerp(vel, (target - pos) * 3, dt * 6); }
                pos += vel * dt;
                // Never below the visible ground (baked by PlaytestFixesPass), except when settling onto a landing spot.
                if (HasGrid && state == 0) { float floor = Baked(pos.x, pos.z) + .18f; if (pos.y < floor) { pos.y = floor; if (vel.y < 0) vel.y = 0; } }
                if (Flat(vel).sqrMagnitude > .01f) heading = Mathf.MoveTowardsAngle(heading, Mathf.Atan2(vel.x, vel.z) * Mathf.Rad2Deg, dt * 360);
                transform.SetPositionAndRotation(pos, Quaternion.Euler(-vel.y * 12, heading, 0));
                beat += dt * 11; if (!Seen) return;
                Wings(.0035f, .05f, Mathf.Lerp(-12, 70, .5f + .5f * Mathf.Sin(beat * Mathf.PI * 2)), 0);
            }
            else
            {
                // Settled: drift down onto the spot, then slowly open and close the wings.
                if ((target - pos).sqrMagnitude > .0004f) { pos = Vector3.MoveTowards(pos, target, dt * .8f); transform.position = pos; }
                if (timer < 0) { state = 0; timer = 0; vel = Vector3.up * .8f; }
                if (!Seen) return;
                Wings(.0035f, .05f, Mathf.Lerp(20, 82, .5f + .5f * Mathf.Sin(t * 1.3f + phase)), 0);
            }
        }

        // ------------------------------------------------------------ fish: a school loops the shallows, darting now and then
        void Fish(float dt)
        {
            float r = (radius.x + radius.y) * .5f;
            float surge = 1 + .9f * Mathf.Max(0, Mathf.Sin(t * .8f + phase * .3f)) * Mathf.Max(0, Mathf.Sin(t * 2.3f));
            beat += dt * speed / r * surge;
            float a = direction * (beat + phase * .02f);
            var centre = home + new Vector3(Mathf.Cos(a) * radius.x, 0, Mathf.Sin(a * 1.3f) * radius.y);
            var ahead = home + new Vector3(Mathf.Cos(a + direction * .03f) * radius.x, 0, Mathf.Sin((a + direction * .03f) * 1.3f) * radius.y);
            var fwd = Flat(ahead - centre).normalized; var side = new Vector3(fwd.z, 0, -fwd.x);
            var p = centre + side * schoolOffset.x + fwd * schoolOffset.z + Vector3.up * (schoolOffset.y + Mathf.Sin(t * 1.1f + phase) * .04f);
            float yaw = Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg;
            transform.SetPositionAndRotation(p, Quaternion.Euler(0, yaw + Mathf.Sin(t * 7 * surge + phase) * 4, 0));
            if (!Seen) return;
            float f = t * 9 * surge + phase + schoolOffset.x * 40 + schoolOffset.z * 23;
            for (int i = 0; i < baseV.Length; i++)
            {
                var q = baseV[i]; float w = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.05f, -.15f, q.z));
                v[i] = new Vector3(q.x + Mathf.Sin(f - q.z * 22) * .022f * w, q.y, q.z);
            }
            mesh.vertices = v;
        }

        void OnDestroy() { if (mesh) Destroy(mesh); }
    }
}
