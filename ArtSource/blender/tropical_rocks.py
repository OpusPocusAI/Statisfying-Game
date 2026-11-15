"""Rock shapes pass (owner, November 15: Tchia is the art target).

Rebuilds the coastal rock under the same names, so every placed copy in the scene updates in place:
- sea_stack_1..2: the Sunset Point and Shell Cove stacks.
- shore_rock_0..3: the boulder groups along the rocky points.
- faceted_rock_0..3: the ridge outcrops and beach stones.

The old rock was a decimated, flat-shaded lump: every facet the same size, hard edges everywhere, so it read as low-poly
geometry rather than stone. Stylised tropical rock reads as big, deliberate planes with softened edges, horizontal strata
that step out into ledges, a few deep vertical cracks and a wave-cut foot. Each rock here is shaped on a dense mesh
(strata, cracks, chisel planes, lumps), then voxel-remeshed so every part fuses into one surface (no boulders stuck
together), relaxed so the edges round over, decimated back to a game budget and shaded smooth across shallow angles
only, so the broad planes stay crisp and the edges soften.

Footprints and heights match the old meshes, so placements, scales and contact surf stay valid.
Run: blender -b --factory-startup -P tropical_rocks.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise


def smoothstep(a, b, x):
    t = max(0.0, min(1.0, (x - a) / (b - a)))
    return t * t * (3 - 2 * t)


def apply(o, md):
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.modifier_apply(modifier=md.name)


def finish(o, voxel, smooth, tris, angle=38):
    """Fuse into one surface, round the edges a little, bring it back to a game budget, shade by angle."""
    r = o.modifiers.new("r", "REMESH"); r.mode = "VOXEL"; r.voxel_size = voxel; apply(o, r)
    if smooth:
        s = o.modifiers.new("s", "SMOOTH"); s.factor = .5; s.iterations = smooth; apply(o, s)
    faces = len(o.data.polygons) * 2   # voxel remesh makes quads
    if faces > tris:
        d = o.modifiers.new("d", "DECIMATE"); d.ratio = tris / faces; apply(o, d)
    shade_auto(o, angle)


def chisel(o, cuts, seed, keep=.8, soft=.0):
    """Flatten against a few planes, so a round mass gets big, deliberate facets."""
    rnd = random.Random(seed)
    for _ in range(cuts):
        a = rnd.uniform(0, math.tau); el = rnd.uniform(-.1, .85)
        n = Vector((math.cos(a) * math.cos(el), math.sin(a) * math.cos(el), math.sin(el))).normalized()
        dmax = max(v.co.dot(n) for v in o.data.vertices)
        d = dmax * rnd.uniform(keep, keep + .1)
        for v in o.data.vertices:
            k = v.co.dot(n) - d
            if k > 0: v.co -= n * k * (1 - soft)


def cracks(rnd, count, depth, width):
    return [(rnd.uniform(0, math.tau), rnd.uniform(*width), rnd.uniform(*depth), rnd.uniform(.2, .9)) for _ in range(count)]


def crack_pull(a, t, cs, s):
    k = 1.0
    for (ca, w, d, reach) in cs:
        da = math.atan2(math.sin(a - ca), math.cos(a - ca))
        along = smoothstep(reach + .15, reach - .1, t) if reach < .8 else 1.0   # some cracks stop part way up
        k *= 1 - d * math.exp(-(da / w) ** 2) * along * (.7 + .3 * noise.noise(Vector((ca, t * 6, 0)) + s))
    return k


def strata(t, a, steps, phase, shelf, tilt):
    """Horizontal bands that step out into ledges; the band lines tilt a little and wander."""
    ts = (t + tilt * math.cos(a - phase)) * steps
    frac = ts - math.floor(ts)
    return 1 - shelf * (1 - smoothstep(0, .35, frac))


# ---------------------------------------------------------------- sea stacks (nominal ~1.2 wide, ~1.75 high, base z=0)
for k, (name, H, seed) in enumerate([("sea_stack_1", 1.72, 401), ("sea_stack_2", 1.8, 402)]):
    reset(); rnd = random.Random(seed)
    bpy.ops.mesh.primitive_cylinder_add(vertices=64, radius=.5, depth=H, location=(0, 0, H / 2))
    o = bpy.context.active_object
    bpy.ops.object.transform_apply(location=True)                        # base on z=0 in the mesh itself
    bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=40); bpy.ops.object.mode_set(mode="OBJECT")
    s = Vector((seed * .37, seed * .11, seed * .23))
    cs = cracks(rnd, 5, (.08, .16), (.05, .1))
    steps = rnd.randint(4, 5); phase = rnd.uniform(0, math.tau)
    ex, ey = rnd.uniform(1.0, 1.12), rnd.uniform(.88, 1.0)
    lean = Vector((rnd.uniform(-.08, .08), rnd.uniform(-.08, .08)))
    for v in o.data.vertices:
        t = max(0.0, min(1.0, v.co.z / H)); r = math.hypot(v.co.x, v.co.y)
        if r < 1e-5: continue
        a = math.atan2(v.co.y, v.co.x)
        rad = .58 * (1 - .32 * t)                                          # taper
        rad *= 1 + noise.fractal(Vector((math.cos(a) * 1.1, math.sin(a) * 1.1, t * 1.6)) + s, .55, 2.0, 3) * .26   # lumpy plan
        rad *= strata(t, a, steps, phase, .085, .05)                         # ledges
        rad *= crack_pull(a, t, cs, s)                                       # vertical cracks
        rad *= 1 - .16 * math.exp(-((t - .1) / .05) ** 2)                   # wave-cut notch at the waterline
        rad *= 1 + .06 * smoothstep(.0, .04, .04 - t)                        # a splayed foot
        q = r / .5 * rad
        v.co.x = math.cos(a) * q * ex + lean.x * t * t
        v.co.y = math.sin(a) * q * ey + lean.y * t * t
        if t > .995:                                                         # a broken, sloping top with a dip for grass
            v.co.z += noise.noise(Vector((v.co.x, v.co.y, 0)) * 3 + s) * .07 - .05 * (1 - r / .5) - v.co.x * .06
        v.co.z = max(v.co.z, 0.0)
    chisel(o, 7, seed + 9, keep=.86)                                         # a few broad planes, so the bands read as cut stone, not cushions
    for v in o.data.vertices: v.co.z = max(v.co.z, 0.0)
    finish(o, .018, 1, 5200, 34)
    assign(o, "rock")
    o.data.materials.append(mat("cap"))
    top = max(v.co.z for v in o.data.vertices)
    for p in o.data.polygons:                                                # grass on the summit, part of the same surface
        if p.normal.z > .82 and p.center.z > top - .22: p.material_index = 1
    export(o, name, ["rock", "cap"])


# ---------------------------------------------------------------- shore boulder groups (nominal ~1.3 x 1.2, ~0.65 high, base z=0)
def boulder(centre, size, seed):
    rnd = random.Random(seed)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=5, radius=1)
    o = bpy.context.active_object
    s = Vector((seed * 1.31, seed * .77, seed * 2.13))
    cs = cracks(rnd, 1, (.03, .06), (.1, .16)); phase = rnd.uniform(0, math.tau)   # shallow, broad: deep ones read as faces
    for v in o.data.vertices:
        p = v.co.copy(); a = math.atan2(p.y, p.x); t = (p.z + 1) / 2
        d = 1 + noise.fractal(p * .9 + s, .5, 2.0, 2) * .12
        d *= strata(t, a, 3, phase, .04, .06) * crack_pull(a, t, cs, s)
        v.co = Vector((p.x * size[0] * d, p.y * size[1] * d, p.z * size[2] * d))
    chisel(o, 9, seed, keep=.76)                     # mostly big planes
    for v in o.data.vertices:                       # a broad, gently sloping top plane
        lim = size[2] * .78 + v.co.x * .1
        if v.co.z > lim: v.co.z = lim + (v.co.z - lim) * .12
    o.rotation_euler = (rnd.uniform(-.18, .18), rnd.uniform(-.18, .18), rnd.uniform(0, math.tau))
    bpy.ops.object.transform_apply(rotation=True)
    o.location = Vector(centre) + Vector((0, 0, size[2] * .55)); bpy.ops.object.transform_apply(location=True)
    return o


for k in range(4):
    reset(); rnd = random.Random(2400 + k)
    parts = []
    big = (rnd.uniform(.44, .52), rnd.uniform(.36, .44), rnd.uniform(.44, .54))
    parts.append(boulder((0, 0, 0), big, 2400 + k * 11))
    n = rnd.randint(2, 3); a0 = rnd.uniform(0, math.tau)
    for i in range(n):
        a = a0 + i * math.tau / n + rnd.uniform(-.5, .5); r = rnd.uniform(.38, .5); sz = rnd.uniform(.17, .26)
        parts.append(boulder((math.cos(a) * r, math.sin(a) * r * .9, 0), (sz * 1.25, sz, sz * rnd.uniform(.8, 1.1)), 2400 + k * 11 + i + 1))
    rock = join(parts, "ShoreRock")
    for v in rock.data.vertices: v.co.z = max(v.co.z, 0.0)          # sits in the sand
    finish(rock, .014, 1, 2600, 34)
    assign(rock, "rock")
    export(rock, "shore_rock_%d" % k, ["rock"])


# ---------------------------------------------------------------- outcrops / stones (fit to +-0.47, like the old faceted_rock)
for k in range(4):
    reset(); rnd = random.Random(100 + k)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=5, radius=1)
    o = bpy.context.active_object
    s = Vector((k * 3.7, k * 1.3, k * 2.1)); cs = cracks(rnd, 3, (.06, .12), (.05, .1)); phase = rnd.uniform(0, math.tau)
    stretch = Vector((rnd.uniform(1.0, 1.35), rnd.uniform(.85, 1.1), rnd.uniform(.62, .8)))
    for v in o.data.vertices:
        p = v.co.copy(); a = math.atan2(p.y, p.x); t = (p.z + 1) / 2
        d = 1 + noise.fractal(p * 1.2 + s, .6, 2.0, 3) * .24
        d *= strata(t, a, 4, phase, .06, .05) * crack_pull(a, t, cs, s)
        v.co = Vector((p.x * d * stretch.x, p.y * d * stretch.y, p.z * d * stretch.z))
    chisel(o, 7, 100 + k, keep=.72)
    for v in o.data.vertices:
        if v.co.z < -.35: v.co.z = -.35 + (v.co.z + .35) * .1   # a flat base that sits on the ground
    finish(o, .016, 2, 1800, 38)
    vs = [v.co.copy() for v in o.data.vertices]
    mn = Vector((min(v.x for v in vs), min(v.y for v in vs), min(v.z for v in vs)))
    mx = Vector((max(v.x for v in vs), max(v.y for v in vs), max(v.z for v in vs)))
    c, sz = (mx + mn) / 2, mx - mn
    for v in o.data.vertices:
        p = v.co - c
        v.co = Vector((p.x / sz.x * .94, p.y / sz.y * .94, p.z / sz.z * .94))
    assign(o, "rock")
    export(o, "faceted_rock_%d" % k, ["rock"])
