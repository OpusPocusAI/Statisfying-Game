"""Round 24 models (after the twenty-second review, three reviewers averaged):
- shore_rock_0..3: weathered boulder groups replacing rock_stack_* ("stacked boxes", "box-like shore rocks").
  Each is one big rounded boulder chiselled by a few large planar cuts (Lonely Mountains: Downhill), with two
  or three smaller boulders leaning against it. Nominal ~1 x 1 footprint, ~1 high, base on z=0, like rock_stack.
- far_isle_0..2: islands as one cliff mass with vertical fluting and a wave-cut foot, under a grassy cap that
  drapes over the cliff edge, replacing far_island_* (their stacked strata read as a pink stripe).
  Nominal 1 x ~0.85 footprint, ~0.45 high, base on z=0, like far_island.
Run: blender -b --factory-startup -P round24.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise


def chisel(o, cuts, seed, keep=.78):
    """Flatten the mesh against a few random planes, so a round mass gets big flat facets."""
    random.seed(seed)
    for _ in range(cuts):
        a = random.uniform(0, math.tau); el = random.uniform(-.15, .9)
        n = Vector((math.cos(a) * math.cos(el), math.sin(a) * math.cos(el), math.sin(el))).normalized()
        dmax = max(v.co.dot(n) for v in o.data.vertices)
        d = dmax * random.uniform(keep, keep + .12)
        for v in o.data.vertices:
            k = v.co.dot(n) - d
            if k > 0:
                v.co -= n * k


def boulder(centre, size, seed, sink=.18, tilt=.25):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=3, radius=1)
    o = bpy.context.active_object
    s = Vector((seed * 1.31, seed * .77, seed * 2.13))
    for v in o.data.vertices:
        p = v.co.copy()
        d = 1 + noise.fractal(p * 1.2 + s, .55, 2.0, 3) * .22
        v.co = Vector((p.x * size[0] * d, p.y * size[1] * d, p.z * size[2] * d))
    chisel(o, 7, seed, keep=.72)
    # Flat, slightly sloping top facet: weathered rock rarely ends in a point.
    for v in o.data.vertices:
        lim = size[2] * .8 + v.co.x * .12
        if v.co.z > lim: v.co.z = lim + (v.co.z - lim) * .15
    o.rotation_euler = (random.uniform(-tilt, tilt), random.uniform(-tilt, tilt), random.uniform(0, math.tau))
    bpy.ops.object.transform_apply(rotation=True)
    for v in o.data.vertices:                         # sit in the sand: cut the underside flat
        v.co.z = max(v.co.z, -size[2] * (1 - sink * 2))
    o.location = Vector(centre) + Vector((0, 0, size[2] * (1 - sink * 2)))
    bpy.ops.object.transform_apply(location=True)
    dec = o.modifiers.new("d", "DECIMATE"); dec.ratio = .45
    bpy.ops.object.modifier_apply(modifier="d")
    assign(o, "rock")
    shade_auto(o, 32)
    return o


for k in range(4):
    reset(); random.seed(2400 + k)
    parts = []
    big = (random.uniform(.42, .5), random.uniform(.34, .42), random.uniform(.36, .46))
    parts.append(boulder((0, 0, 0), big, 2400 + k * 11, tilt=.18))
    n = random.randint(2, 3)
    a0 = random.uniform(0, math.tau)
    for i in range(n):
        a = a0 + i * math.tau / n + random.uniform(-.5, .5)
        r = random.uniform(.36, .5)
        sz = random.uniform(.16, .26)
        parts.append(boulder((math.cos(a) * r, math.sin(a) * r * .9, 0), (sz * 1.2, sz, sz * random.uniform(.75, 1.05)), 2400 + k * 11 + i + 1))
    for i in range(random.randint(2, 4)):            # loose cobbles at the foot
        a = random.uniform(0, math.tau); r = random.uniform(.5, .66); sz = random.uniform(.05, .09)
        parts.append(boulder((math.cos(a) * r, math.sin(a) * r, 0), (sz * 1.3, sz, sz * .8), 2500 + k * 7 + i, sink=.25))
    rock = join(parts, "ShoreRock")
    export(rock, "shore_rock_%d" % k, ["rock"])


def outline(a, seed):
    """Island plan radius: lumpy, with a bay or two."""
    s = Vector((seed * .9, seed * 1.4, 0))
    r = 1 + noise.noise(Vector((math.cos(a) * 1.4, math.sin(a) * 1.4, 0)) + s) * .2
    r -= .14 * math.exp(-((math.atan2(math.sin(a - seed), math.cos(a - seed))) / .35) ** 2)   # a bay
    return r


for k in range(3):
    reset(); random.seed(2450 + k)
    seed = 2450 + k
    R, D = .5, random.uniform(.78, .9)                # half-width; depth ratio
    # Cliff: a fluted cylinder whose top height varies around the island.
    bpy.ops.mesh.primitive_cylinder_add(vertices=48, radius=1, depth=1, location=(0, 0, 0))
    cl = bpy.context.active_object
    bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=5); bpy.ops.object.mode_set(mode="OBJECT")
    s = Vector((seed * .3, seed * .7, 0))
    for v in cl.data.vertices:
        t = v.co.z + .5                                # 0 base .. 1 top (the cylinder is centred on its origin)
        a = math.atan2(v.co.y, v.co.x)
        rr = math.hypot(v.co.x, v.co.y)
        top = .22 + .1 * (noise.noise(Vector((math.cos(a) * 1.1, math.sin(a) * 1.1, 0)) - s) * .5 + .5) + .04 * math.cos(a - seed)
        o = outline(a, seed)
        flute = 1 + .035 * noise.noise(Vector((a * 4.5, 0, seed)))          # vertical fluting: varies with angle only
        batter = 1 - .05 * t                                                  # gently sloping cliff face
        notch = 1 - .07 * math.exp(-((t - .1) / .07) ** 2)                  # wave-cut foot
        rad = R * .96 * o * flute * batter * notch * rr
        v.co = Vector((math.cos(a) * rad, math.sin(a) * rad * D, t * top))
    dec = cl.modifiers.new("d", "DECIMATE"); dec.ratio = .5
    bpy.ops.object.modifier_apply(modifier="d")
    assign(cl, "cliff"); shade_auto(cl, 30)
    # Grassy cap draped over the cliff top, rising into a soft hill.
    bpy.ops.mesh.primitive_uv_sphere_add(segments=48, ring_count=20, radius=1)
    cap = bpy.context.active_object
    hill = random.uniform(.09, .13); hx = random.uniform(-.12, .12)

    def top_at(a):
        return .22 + .1 * (noise.noise(Vector((math.cos(a) * 1.1, math.sin(a) * 1.1, 0)) - s) * .5 + .5) + .04 * math.cos(a - seed)

    def cap_z(x, y):
        a = math.atan2(y / D, x)
        dx = (x - hx) / R
        dome = max(0.0, 1 - dx * dx - (y / (R * D)) ** 2)
        return top_at(a) + hill * dome ** .7 + noise.fractal(Vector((x, y, 0)) * 6 + s, .5, 2, 2) * .012

    for v in cap.data.vertices:
        p = v.co.copy(); a = math.atan2(p.y, p.x); rr = math.hypot(p.x, p.y)
        o = outline(a, seed)
        x, y = math.cos(a) * R * o * rr * .97, math.sin(a) * R * o * rr * D * .97
        if p.z >= 0:
            z = cap_z(x, y)
            if rr > .92: z -= (rr - .92) * .25          # a soft grassy brow at the cliff edge
        else:
            # The underside tucks back into the cliff instead of hanging outside it like cloth.
            x *= 1 + p.z * .12; y *= 1 + p.z * .12
            z = top_at(a) - .015 + p.z * .03
        v.co = Vector((x, y, z))
    dec = cap.modifiers.new("d", "DECIMATE"); dec.ratio = .5
    bpy.ops.object.modifier_apply(modifier="d")
    assign(cap, "cap"); shade_auto(cap, 40)
    parts = [cl, cap]
    # Tree clumps in groves, not sprinkled evenly.
    for g in range(random.randint(3, 4)):
        ga = random.uniform(0, math.tau); gr = random.uniform(.1, .6) * R
        for t in range(random.randint(4, 7)):
            a = ga + random.uniform(-.5, .5); r = min(.8 * R, gr + random.uniform(-.08, .08))
            x, y = math.cos(a) * r + random.uniform(-.04, .04), math.sin(a) * r * D
            rad = random.uniform(.035, .06)
            z = cap_z(x, y) + rad * .15                  # sitting on the cap, not floating above it
            bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=1)
            tr = bpy.context.active_object
            ts = Vector((g * 3.1 + t, t * 1.3, g))
            for v in tr.data.vertices:
                p = v.co.copy(); d = 1 + noise.noise(p * 2 + ts) * .25
                v.co = Vector((p.x * rad * d, p.y * rad * d, max(p.z, -.3) * rad * 1.15 * d))
            tr.location = (x, y, z); bpy.ops.object.transform_apply(location=True)
            assign(tr, "trees"); shade_auto(tr, 60)
            parts.append(tr)
    isl = join(parts, "FarIsle")
    export(isl, "far_isle_%d" % k, ["cliff", "cap", "trees"])
