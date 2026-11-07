"""Round 64 models (after the sixtieth review; all three: the beach bushes are "lumpy single blobs", "a pile of balls",
repeated everywhere; Tchia's coastal shrubs are layered, sunlit foliage):
- coastal_shrub_leafy, coastal_shrub_leafy_b, coastal_shrub_leafy_c: three different bushes (a broad low mound, a
  taller rounded bush, a two-lobed spreading one). Each is a lobed inner mass in tiers (broad lower clumps, smaller
  upper clumps) fused into one surface and smoothed, shingled all over with broad ovate leaves laid along the surface
  like scales and tilted out, so the silhouette breaks into leaf tips. Every vertex carries the bush's rounded volume
  normal (blended with the lobe it sits on), the usual trick for stylised foliage: the whole bush shades as one soft,
  lit form, lit on top and shaded underneath, instead of each leaf catching light at random (the old leaf cards read
  as dark specks). Leaves on the upper, sun-facing side use leaf_lit, the rest and the inner mass leaf_dark.
  Same footprint (about 1.2 m across, 0.7 m high), the same two material slots, origin at the base centre.
Run: blender -b --factory-startup -P round64.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise


def build(name, seed, layout):
    rnd = random.Random(seed)
    reset()
    parts = []

    def blob(c, r, h):
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=3, radius=1, location=c)
        o = bpy.context.active_object
        o.scale = (r, r, h); bpy.ops.object.transform_apply(scale=True)
        parts.append(o)

    for (c, r, h) in layout(rnd):
        blob(c, r, h)
    core = join(parts, "Core")
    bpy.context.view_layer.objects.active = core; core.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
    rm = core.modifiers.new("fuse", "REMESH"); rm.mode = "VOXEL"; rm.voxel_size = .03
    bpy.ops.object.modifier_apply(modifier="fuse")
    sm = core.modifiers.new("soften", "CORRECTIVE_SMOOTH"); sm.factor, sm.iterations = .7, 12
    bpy.ops.object.modifier_apply(modifier="soften")
    # Nothing below the ground.
    for v in core.data.vertices:
        if v.co.z < 0: v.co.z = 0
    core.data.update()
    xs = [v.co.x for v in core.data.vertices]; ys = [v.co.y for v in core.data.vertices]; zs = [v.co.z for v in core.data.vertices]
    cx, cy = (max(xs) + min(xs)) / 2, (max(ys) + min(ys)) / 2
    ax, ay, az = (max(xs) - min(xs)) / 2, (max(ys) - min(ys)) / 2, max(zs)
    C = Vector((cx, cy, -az * .2))   # below the middle, so the base faces out sideways, not down into its own shadow

    def volume_normal(p):
        q = Vector(((p.x - C.x) / ax, (p.y - C.y) / ay, (p.z - C.z) / (az * 1.1)))
        n = Vector((q.x / ax, q.y / ay, q.z / (az * 1.1)))
        return n.normalized() if n.length > 1e-6 else Vector((0, 0, 1))

    # Leaf sites sampled over the upper three quarters of the mass, with the lobe normal there.
    samples = [(v.co.copy(), v.normal.copy()) for v in core.data.vertices if v.co.z > az * .1]
    rnd.shuffle(samples)
    dec = core.modifiers.new("reduce", "DECIMATE"); dec.ratio = .25
    bpy.ops.object.modifier_apply(modifier="reduce")
    assign(core, "leaf_dark")
    for p in core.data.polygons: p.use_smooth = True

    leaves, leaf_normals = [], []
    up = Vector((0, 0, 1))
    n_leaves = 230
    used = 0
    for (p, n) in samples:
        if used >= n_leaves: break
        used += 1
        # Lay the leaf along the surface, pointing outward and a little downhill, tilted out of the surface.
        t = (n.cross(up)).cross(n)                       # toward the top of the lobe
        if t.length < .1: t = n.cross(Vector((1, 0, 0)))
        t.normalize()
        ang = rnd.uniform(-1.1, 1.1)
        side0 = n.cross(t).normalized()
        d = (-t * math.cos(ang) + side0 * math.sin(ang))   # down-slope, spread around
        d = (d + n * rnd.uniform(.35, .7)).normalized()   # tilted out, so leaf tips break the outline
        side = n.cross(d).normalized()
        L = rnd.uniform(.13, .2) * (1.1 if p.z > az * .5 else .95); Wd = L * rnd.uniform(.6, .72)
        base = p - n * .015
        cup = n * L * .06
        q1 = base + d * (L * .25); q2 = base + d * (L * .68); tip = base + d * L
        vs = [base, q1 + side * Wd * .45 - cup, q2 + side * Wd * .4 - cup, tip, q2 - side * Wd * .4 - cup, q1 - side * Wd * .45 - cup,
              base + d * (L * .5) + cup]
        faces = [(0, 1, 6), (1, 2, 6), (2, 3, 6), (3, 4, 6), (4, 5, 6), (5, 0, 6)]   # a cupped, ovate leaf with a midrib ridge
        me = bpy.data.meshes.new("leaf"); me.from_pydata([tuple(v) for v in vs], [], faces); me.update()
        lf = bpy.data.objects.new("leaf", me); bpy.context.collection.objects.link(lf)
        s2 = lf.modifiers.new("s", "SOLIDIFY"); s2.thickness = .005
        bpy.context.view_layer.objects.active = lf; bpy.ops.object.modifier_apply(modifier="s")
        vn = volume_normal(p)
        lit = vn.z + noise.noise(p * 5 + Vector((seed, 2, 3))) * .25 > .15
        assign(lf, "leaf_lit" if lit else "leaf_dark")
        for pp in lf.data.polygons: pp.use_smooth = True
        leaves.append(lf); leaf_normals.append(p)
    full = join([core] + leaves, name)
    # Volume normals: the whole bush shades as one rounded form; the inner mass keeps a little of its lobes.
    me = full.data
    lobe = {}
    for v in me.vertices:
        lobe[v.index] = v.normal.copy()
    custom = []
    for v in me.vertices:
        vn = volume_normal(v.co)
        k = .7
        n = (vn * k + lobe[v.index] * (1 - k)).normalized()
        custom.append(tuple(n))
    me.normals_split_custom_set_from_vertices(custom)
    # Keep the two slots in a fixed order.
    export(full, name, ["leaf_dark", "leaf_lit"])


def mound(rnd):          # a broad, low mound
    out = [((0, 0, .26), .5, .3)]
    for i in range(7):
        a = i / 7 * math.tau + rnd.uniform(-.3, .3); rr = rnd.uniform(.32, .46)
        out.append(((math.cos(a) * rr, math.sin(a) * rr, .2), rnd.uniform(.22, .3), rnd.uniform(.18, .24)))
    for i in range(4):
        a = i / 4 * math.tau + rnd.uniform(-.4, .4); rr = rnd.uniform(.05, .22)
        out.append(((math.cos(a) * rr, math.sin(a) * rr, rnd.uniform(.4, .5)), rnd.uniform(.2, .26), rnd.uniform(.15, .2)))
    return out


def rounded(rnd):        # a taller, rounded bush that builds up in tiers
    out = [((0, 0, .3), .42, .34)]
    for i in range(6):
        a = i / 6 * math.tau + rnd.uniform(-.3, .3); rr = rnd.uniform(.28, .38)
        out.append(((math.cos(a) * rr, math.sin(a) * rr, .2), rnd.uniform(.2, .26), rnd.uniform(.18, .22)))
    for i in range(4):
        a = i / 4 * math.tau + rnd.uniform(-.4, .4); rr = rnd.uniform(.08, .2)
        out.append(((math.cos(a) * rr, math.sin(a) * rr, rnd.uniform(.46, .56)), rnd.uniform(.2, .25), rnd.uniform(.16, .2)))
    out.append(((rnd.uniform(-.06, .06), rnd.uniform(-.06, .06), .66), .17, .13))
    return out


def spreading(rnd):      # two lobes side by side, one taller
    out = []
    for (ox, s) in ((-.24, 1.0), (.26, .8)):
        out.append(((ox, 0, .24 * s), .34 * s, .27 * s))
        for i in range(5):
            a = i / 5 * math.tau + rnd.uniform(-.3, .3); rr = rnd.uniform(.2, .3) * s
            out.append(((ox + math.cos(a) * rr, math.sin(a) * rr * .8, .17 * s), rnd.uniform(.17, .23) * s, rnd.uniform(.15, .19) * s))
        out.append(((ox + rnd.uniform(-.06, .06), rnd.uniform(-.05, .05), .44 * s), .19 * s, .15 * s))
    return out


build("coastal_shrub_leafy", 6400, mound)
build("coastal_shrub_leafy_b", 6401, rounded)
build("coastal_shrub_leafy_c", 6402, spreading)
