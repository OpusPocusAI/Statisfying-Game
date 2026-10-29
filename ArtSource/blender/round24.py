"""Round 24 models (after the twenty-second review, three reviewers averaged):
- shore_rock_0..3: weathered boulder groups replacing rock_stack_* ("stacked boxes", "box-like shore rocks").
  Each is one big rounded boulder chiselled by a few large planar cuts (Lonely Mountains: Downhill), with two
  or three smaller boulders leaning against it. Nominal ~1 x 1 footprint, ~1 high, base on z=0, like rock_stack.
- (moved to round25.py) far_isle_0..2: islands as one cliff mass with vertical fluting and a wave-cut foot, under a grassy cap that
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



# The far islands (far_isle_*) moved to round25.py.
