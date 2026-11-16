"""Denser, layered plants pass (owner, November 15: Tchia is the art target).

Stylised tropical coasts read in layers: tall, leaning coconut palms over a mid layer of big-leaved plants and shrubs, with
ferns and grass at the ground. The old palms were short and assembled (torus rings on a tube, sphere crown, sphere nuts,
single-strip fronds); the coast had no understory between grass and shrub. This script builds, all original:
- palm_0..2 (same names, so placed palms update in place): taller, slender curved trunks with the growth rings carved into
  one surface, a fused crown boss and nut cluster, and 16-20 full pinnate fronds whose leaflets droop off the midrib.
- broadleaf_0..1: a clump of big paddle-shaped leaves on arching stems, the mid layer.
- fern_0..1: a low rosette of arching pinnate fronds, the ground layer.
Run: blender -b --factory-startup -P tropical_plants.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise


def smoothstep(a, b, x):
    t = max(0.0, min(1.0, (x - a) / (b - a)))
    return t * t * (3 - 2 * t)


def bezier(pts, t):
    p0, p1, p2, p3 = [Vector(p) for p in pts]
    u = 1 - t
    return p0 * u ** 3 + p1 * 3 * u * u * t + p2 * 3 * u * t * t + p3 * t ** 3


def mesh_object(bm, name, material):
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(o)
    assign(o, material)
    return o


def fused_lump(lumps, voxel, material):
    """Lumps (centre, radius, squash) voxel-merged into one smooth surface."""
    parts = []
    for (c, r, sq) in lumps:
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=3, radius=r, location=c)
        o = bpy.context.active_object; o.scale = (1, 1, sq); bpy.ops.object.transform_apply(location=False, scale=True)
        parts.append(o)
    o = join(parts, "lump")
    md = o.modifiers.new("r", "REMESH"); md.mode = "VOXEL"; md.voxel_size = voxel
    bpy.context.view_layer.objects.active = o; bpy.ops.object.modifier_apply(modifier="r")
    d = o.modifiers.new("d", "DECIMATE"); d.ratio = min(1.0, 260 / max(1, len(o.data.polygons)))
    bpy.ops.object.modifier_apply(modifier="d")
    for p in o.data.polygons: p.use_smooth = True
    assign(o, material)
    return o


def pinnate(bm, base, yaw, pitch, length, droop, leaflets, leaf_len, leaf_w, seed, gap=.0, curl=.0, k=3):
    """A pinnate frond: a thin midrib arching out and down, with tapered leaflets angled forward and drooping off both
    sides. Everything is one connected strip system (leaflets start on the rib)."""
    rnd = random.Random(seed)
    fwd = Vector((math.cos(yaw), math.sin(yaw), 0)); side = Vector((-math.sin(yaw), math.cos(yaw), 0)); up = Vector((0, 0, 1))
    segs = 14
    def rib(t):
        r = length * t
        z = length * (math.sin(pitch) * t - droop * t * t)
        return base + fwd * (r * math.cos(pitch) * .9 + r * .1) + up * z
    pts = [rib(s / segs) for s in range(segs + 1)]
    # midrib: a narrow two-sided strip, upright (so it reads from below as a stalk)
    prev = None
    for i, p in enumerate(pts):
        w = .025 * (1 - i / segs) + .006
        a = bm.verts.new(p - side * w); b = bm.verts.new(p + side * w)
        if prev: bm.faces.new((prev[0], prev[1], b, a))
        prev = (a, b)
    # leaflets
    for j in range(leaflets):
        t = .12 + .86 * (j + rnd.uniform(-.3, .3)) / leaflets
        if gap and rnd.random() < gap: continue                     # the odd split in the comb
        i = min(segs - 1, int(t * segs)); u = t * segs - i
        p = pts[i].lerp(pts[i + 1], u); tang = (pts[i + 1] - pts[i]).normalized()
        n_side = tang.cross(up).normalized()
        L = leaf_len * (math.sin(math.pi * min(1, t * 1.05)) ** .6) * rnd.uniform(.85, 1.1) + .04
        W = leaf_w * (1 - .4 * t)
        for sgn in (-1, 1):
            # leaflets sweep forward along the rib and hang down (V section), more so toward the tip
            dirn = (n_side * sgn * .78 + tang * .55 + up * (-.35 - .45 * t - curl)).normalized()
            pass
            row = []
            for s in range(k + 1):
                f = s / k
                c = p + dirn * L * f + Vector((0, 0, -.18 * L * f * f))          # tip droops
                w = W * math.sin(math.pi * min(1, (f + .08) * .95)) * (1 - .6 * f) + .004
                row.append((bm.verts.new(c - tang * w * .5), bm.verts.new(c + tang * w * .5)))
            for s in range(k):
                bm.faces.new((row[s][0], row[s][1], row[s + 1][1], row[s + 1][0]))


def blade(bm, base, yaw, pitch, length, droop, teeth, leaf_len, seed, gap=.0):
    """A palm frond as one broad painted blade (palms and wet sand pass, Nov 16; reviews: thin pinnate leaflets aliased and
    shimmered at every distance). Each side of the midrib is one surface: leaflets fused into a web for the inner half of
    their length, then splitting into bold, rounded teeth, so the frond reads as a solid comb with a zig-zag edge rather than
    two dozen sub-pixel slivers. Swept forward and hanging down in a V like the old leaflets."""
    rnd = random.Random(seed)
    fwd = Vector((math.cos(yaw), math.sin(yaw), 0)); up = Vector((0, 0, 1))
    def rib(t):
        r = length * t; z = length * (math.sin(pitch) * t - droop * t * t)
        return base + fwd * (r * math.cos(pitch) * .9 + r * .1) + up * z
    n = teeth * 2
    ts = [.06 + .94 * i / n for i in range(n + 1)]
    pts = [rib(t) for t in ts]
    # midrib, as before (reads from below as a stalk)
    prev = None
    for i, t in enumerate(ts):
        p = pts[i]; side = (pts[min(i + 1, n)] - pts[max(i - 1, 0)]).normalized().cross(up).normalized(); w = .03 * (1 - t) + .008
        a = bm.verts.new(p - side * w); b = bm.verts.new(p + side * w)
        if prev: bm.faces.new((prev[0], prev[1], b, a))
        prev = (a, b)
    jitter = [rnd.uniform(.88, 1.1) for _ in range(n + 1)]
    for sgn in (-1, 1):
        rows = []
        for i, t in enumerate(ts):
            p = pts[i]; tang = (pts[min(i + 1, n)] - pts[max(i - 1, 0)]).normalized()
            n_side = tang.cross(up).normalized()
            L = leaf_len * (math.sin(math.pi * min(1, t * 1.03)) ** .55) * jitter[i] + .05
            dirn = (n_side * sgn * .8 + tang * .5 + up * (-.38 - .45 * t)).normalized()
            tip = i % 2 == 1 and not (gap and rnd.random() < gap)
            reach = L if tip else L * .5                                   # teeth at odd samples, notches between
            mid = p + dirn * L * .42 + Vector((0, 0, -.05 * L))
            out = p + dirn * reach + Vector((0, 0, -.18 * reach * reach / max(L, .01)))
            rows.append((bm.verts.new(p + dirn * .01), bm.verts.new(mid), bm.verts.new(out)))
        for i in range(n):
            a, b = rows[i], rows[i + 1]
            bm.faces.new((a[0], a[1], b[1], b[0])); bm.faces.new((a[1], a[2], b[2], b[1]))


# ---------------------------------------------------------------- palms (8-9.5 m, curved, leaning toward +X)
for k in range(3):
    reset(); rnd = random.Random(1200 + k)
    H = rnd.uniform(7.6, 9.2); lean = rnd.uniform(1.6, 2.8); side = rnd.uniform(-.5, .5)
    ctrl = [(0, 0, 0), (lean * .55, side * .4, H * .3), (lean * .95, side * .8, H * .65), (lean, side, H)]   # bows out low, rises straight
    bm = bmesh.new(); ring_n = 14; segs = 56; rings = []
    for s in range(segs + 1):
        t = s / segs; c = bezier(ctrl, t)
        tan = (bezier(ctrl, min(1, t + .01)) - bezier(ctrl, max(0, t - .01))).normalized()
        a1 = tan.cross(Vector((0, 1, 0))).normalized(); a2 = tan.cross(a1).normalized()
        r = .23 - .09 * t + .15 * smoothstep(.06, 0, t)                        # taper, flared foot
        h = t * H; ridge = (h / .24) % 1.0                                     # growth rings carved into the trunk itself
        r *= 1 + .07 * smoothstep(.75, 1.0, ridge) - .03 * smoothstep(.0, .3, ridge)
        r *= 1 + noise.noise(Vector((c.x * 2, c.y * 2, h * .7 + k))) * .04
        ring = []
        for i in range(ring_n):
            a = i / ring_n * math.tau
            ring.append(bm.verts.new(c + (a1 * math.cos(a) + a2 * math.sin(a)) * r))
        rings.append(ring)
    for s in range(segs):
        for i in range(ring_n):
            j = (i + 1) % ring_n
            bm.faces.new((rings[s][i], rings[s][j], rings[s + 1][j], rings[s + 1][i]))
    bm.faces.new(list(reversed(rings[0]))); bm.faces.new(rings[-1])
    trunk = mesh_object(bm, "trunk", "bark")
    for p in trunk.data.polygons: p.use_smooth = True
    top = Vector(ctrl[-1])
    parts = [trunk]
    # crown boss (frond bases) and a nut cluster, each fused into one lump
    parts.append(fused_lump([(top + Vector((0, 0, .1)), .3, .9), (top + Vector((.08, .05, .3)), .22, .9), (top + Vector((0, 0, -.12)), .24, .8)], .05, "bark_ring"))
    nuts = []
    for c in range(rnd.randint(4, 6)):
        a = c * 2.2 + rnd.uniform(-.3, .3)
        nuts.append((top + Vector((math.cos(a) * .26, math.sin(a) * .26, -.2 - rnd.uniform(0, .14))), .13, 1.1))
    parts.append(fused_lump(nuts, .035, "nut"))
    # fronds: lower ones hang, upper ones arch, a couple of young spears stand up
    bm = bmesh.new(); nf = rnd.randint(19, 23)
    for f in range(nf):
        tier = f % 3
        yaw = f / nf * math.tau * 1.0 + rnd.uniform(-.18, .18) + tier * .7
        pitch = (.75, .5, .2)[tier] + rnd.uniform(-.1, .1)
        droop = (.42, .62, .8)[tier] + rnd.uniform(-.08, .1)
        L = rnd.uniform(2.9, 3.7) * (.9 if tier == 0 else 1)
        blade(bm, top + Vector((0, 0, .2 + .06 * (2 - tier))), yaw, pitch, L, droop, 13, .95, 7000 + k * 50 + f, gap=.06)
    for f in range(2):
        pinnate(bm, top + Vector((0, 0, .35)), rnd.uniform(0, math.tau), 1.25, 1.6, .1, 18, .45, .06, 7900 + k * 5 + f, curl=-.2)
    fr = mesh_object(bm, "fronds", "frond")
    for v in fr.data.vertices:                                                  # a little irregularity along the comb
        v.co.z += noise.noise(v.co * 3 + Vector((k * 7, 0, 0))) * .05
    for p in fr.data.polygons: p.use_smooth = True
    palm = join(parts + [fr], "Palm")
    export(palm, "palm_%d" % k, ["bark", "bark_ring", "frond", "nut"])


# ---------------------------------------------------------------- broadleaf clump (~1.6-2 m across, ~1.2-1.5 m high)
def paddle(bm, base, yaw, rise, stem_len, blade_len, blade_w, seed):
    """An arching stem ending in a broad paddle-shaped blade folded along its midrib, tip hanging."""
    rnd = random.Random(seed)
    fwd = Vector((math.cos(yaw), math.sin(yaw), 0)); side = Vector((-math.sin(yaw), math.cos(yaw), 0)); up = Vector((0, 0, 1))
    stem = [base + fwd * stem_len * .45 * (s / 6) ** 1.4 + up * stem_len * rise * math.sin(math.pi * .5 * s / 6) for s in range(7)]
    # stem: a thin triangular tube
    prev = None
    for i, c in enumerate(stem):
        r = .03 * (1 - .5 * i / 6)
        tan = (stem[min(6, i + 1)] - stem[max(0, i - 1)]).normalized(); b2 = tan.cross(side).normalized()
        ring = [bm.verts.new(c + (side * math.cos(a) + b2 * math.sin(a)) * r) for a in (0, 2.1, 4.2)]
        if prev:
            for q in range(3): bm.faces.new((prev[q], prev[(q + 1) % 3], ring[(q + 1) % 3], ring[q]))
        prev = ring
    tip = stem[-1]
    tilt = rnd.uniform(-.15, .35)
    rows = 9; cols = 3
    grid = []
    for s in range(rows + 1):
        f = s / rows
        centre = tip + fwd * blade_len * f * math.cos(tilt) + up * blade_len * (math.sin(tilt) * f - .55 * f * f)
        w = blade_w * (math.sin(math.pi * min(1, f * .92 + .06)) ** .7) * (1 - .25 * f) + .01
        row = []
        for c in range(-cols, cols + 1):
            x = c / cols
            fold = -abs(x) * w * .28 + .04 * w * math.cos(x * 6 + f * 5 + seed)      # V fold and a slight ripple
            row.append(bm.verts.new(centre + side * x * w + up * fold))
        grid.append(row)
    for s in range(rows):
        for c in range(2 * cols):
            bm.faces.new((grid[s][c], grid[s][c + 1], grid[s + 1][c + 1], grid[s + 1][c]))


for k in range(2):
    reset(); rnd = random.Random(1300 + k)
    dark, lit = bmesh.new(), bmesh.new()
    n = rnd.randint(8, 11)
    for i in range(n):
        yaw = i / n * math.tau + rnd.uniform(-.25, .25)
        inner = i % 3 == 0
        paddle(lit if inner else dark, Vector((rnd.uniform(-.06, .06), rnd.uniform(-.06, .06), 0)), yaw,
               rnd.uniform(1.6, 2.2) if inner else rnd.uniform(.9, 1.4), rnd.uniform(.6, .85),
               rnd.uniform(.7, .95), rnd.uniform(.32, .42), 1300 + k * 20 + i)
    a = mesh_object(dark, "dark", "leaf_dark"); b = mesh_object(lit, "lit", "leaf_lit")
    o = join([a, b], "Broadleaf")
    for p in o.data.polygons: p.use_smooth = True
    export(o, "broadleaf_%d" % k, ["leaf_dark", "leaf_lit"])


# ---------------------------------------------------------------- fern (~1.4 m across, ~0.6 m high)
for k in range(2):
    reset(); rnd = random.Random(1400 + k)
    dark, lit = bmesh.new(), bmesh.new()
    n = rnd.randint(11, 14)
    for i in range(n):
        yaw = i / n * math.tau + rnd.uniform(-.2, .2)
        young = i % 4 == 0
        pinnate(lit if young else dark, Vector((0, 0, .02)), yaw, rnd.uniform(.9, 1.15) if young else rnd.uniform(.6, .85),
                rnd.uniform(.65, .9), rnd.uniform(.75, 1.0), 15, .22, .06, 1400 + k * 30 + i, k=2)
    a = mesh_object(dark, "dark", "leaf_dark"); b = mesh_object(lit, "lit", "leaf_lit")
    o = join([a, b], "Fern")
    for p in o.data.polygons: p.use_smooth = True
    export(o, "fern_%d" % k, ["leaf_dark", "leaf_lit"])
