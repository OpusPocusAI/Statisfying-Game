"""Round 25 models (after the twenty-third review, three reviewers averaged):
- far_isle_0..2: islands sculpted as one heightfield instead of a fluted cylinder under a grassy lid (reviews:
  "layer cakes", "the cap overhangs like a lid", "vertical extruded walls"). From the water up:
    a slumped rock apron and talus boulders at the waterline,
    a tapering cliff broken by two or three irregular ledges,
    a soft grassy hill on top.
  Material follows slope: flat ledges and the hill are grass ("cap"), steep faces are rock ("cliff"), so the
  grass steps down the cliff instead of sitting on it. Nominal 1 x ~0.85 footprint, ~0.45 high, base on z=0
  (the scene sinks the islands so the waterline sits at about z = 0.14).
Run: blender -b --factory-startup -P round25.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise
import bmesh


def smooth(e0, e1, x):
    t = max(0.0, min(1.0, (x - e0) / (e1 - e0)))
    return t * t * (3 - 2 * t)


def outline(a, seed):
    """Island plan radius: lumpy, with a bay."""
    s = Vector((seed * .9, seed * 1.4, 0))
    r = 1 + noise.noise(Vector((math.cos(a) * 1.4, math.sin(a) * 1.4, 0)) + s) * .2
    r -= .14 * math.exp(-((math.atan2(math.sin(a - seed), math.cos(a - seed))) / .35) ** 2)
    return r


RINGS, SEGS, UMAX = 46, 120, 1.2
for k in range(3):
    reset(); random.seed(2500 + k)
    seed = 2450 + k                                   # same outlines as round 24, so placement still fits
    R, D = .5, random.uniform(.78, .9)
    s = Vector((seed * .3, seed * .7, 0))
    hill = random.uniform(.08, .12)
    steps = random.choice([2, 3, 3])
    gullies = [random.uniform(0, math.tau) for _ in range(random.randint(4, 6))]

    def ang_noise(a, off, f=1.1):
        return noise.noise(Vector((math.cos(a) * f, math.sin(a) * f, off)) + s)

    def height(u, a):
        top = .34 + .07 * ang_noise(a, 0) + .03 * math.cos(a - seed)
        foot = .16 + .02 * ang_noise(a, 5, 2.3)      # top of the rock apron, just above the waterline
        plateau = .58 + .06 * ang_noise(a, 9)
        if u <= plateau:
            z = top + hill * (1 - (u / plateau) ** 2) ** .8
        elif u <= 1:
            x = (1 - u) / (1 - plateau)               # 0 at the cliff foot .. 1 at the plateau edge
            # Round 26 (review 24: "evenly spaced green stripes on every tier"): uneven ledge spacing that
            # changes around the island, so tiers bunch up, spread out and fade into a sheer face.
            xw = x ** (.6 + .8 * (ang_noise(a, 7, 1.3) * .5 + .5))
            ph = .5 + .45 * ang_noise(a, 3, 1.7)      # the ledges wander up and down around the island
            st = xw * steps + ph
            fl = math.floor(st); fr = st - fl
            y = (fl + smooth(.05, .42, fr) - ph) / steps   # steep risers, narrow sloping ledges
            sheer = smooth(.15, .45, ang_noise(a, 11, 1.9) * .5 + .5)   # some stretches are one sheer face
            y = y * (1 - sheer) + x * sheer
            y = max(0.0, min(1.0, y * .85 + x * .15))      # an overall batter, so the cliff tapers
            # Vertical gullies cut down through the ledges.
            g = max(math.exp(-((math.atan2(math.sin(a - ga), math.cos(a - ga))) / .045) ** 2) for ga in gullies)
            y *= 1 - .4 * g * (1 - x * .5)
            z = foot + (top - foot) * y
        else:
            x = (u - 1) / (UMAX - 1)                  # slumped apron down under the water
            z = foot * (1 - x) ** 1.4
        # Fine rocky unevenness, stronger on the cliff band.
        rough = .004 if u <= plateau else .012
        z += noise.fractal(Vector((math.cos(a) * u * 4, math.sin(a) * u * 4, 1)) + s, .5, 2, 3) * rough
        return z

    bm = bmesh.new()
    centre = bm.verts.new((0, 0, height(0, 0)))
    rings = []
    for i in range(1, RINGS + 1):
        u = (i / RINGS) ** .8 * UMAX                  # denser rings towards the cliff
        ring = []
        for j in range(SEGS):
            a = j / SEGS * math.tau
            o = outline(a, seed)
            rr = R * o * u * (1 + noise.noise(Vector((math.cos(a) * 3, math.sin(a) * 3, u * 6)) + s) * .025)
            ring.append(bm.verts.new((math.cos(a) * rr, math.sin(a) * rr * D, height(u, a))))
        rings.append(ring)
    for j in range(SEGS):
        bm.faces.new((centre, rings[0][j], rings[0][(j + 1) % SEGS]))
    for i in range(RINGS - 1):
        for j in range(SEGS):
            j2 = (j + 1) % SEGS
            bm.faces.new((rings[i][j], rings[i + 1][j], rings[i + 1][j2], rings[i][j2]))
    me = bpy.data.meshes.new("FarIsle")
    bm.to_mesh(me); bm.free()
    isl = bpy.data.objects.new("FarIsle", me); bpy.context.collection.objects.link(isl)
    bpy.context.view_layer.objects.active = isl; isl.select_set(True)
    # Materials by slope: grass where the ground is flat enough to hold it.
    isl.data.materials.append(mat("cliff")); isl.data.materials.append(mat("cap")); isl.data.materials.append(mat("trees"))
    me.update()
    for p in me.polygons:
        c = p.center; a = math.atan2(c.y / D, c.x); u = math.hypot(c.x, c.y / D) / (R * outline(a, seed))
        top_area = u < .64
        # Round 26: grass on the hilltop always, but only on some ledges, in patches (not a stripe per tier).
        patch = noise.noise(Vector((c.x * 3.5, c.y * 3.5, c.z * 2)) + s) > -.3
        p.material_index = 0   # round 27: one ground material; the Painterly shader paints grass on upward faces per pixel (_TopTint)
    shade_auto(isl, 34)
    parts = [isl]

    def surface(x, y):
        a = math.atan2(y / D, x); u = math.hypot(x, y / D) / (R * outline(a, seed))
        return height(u, a)

    # Talus boulders at the waterline.
    for b in range(random.randint(7, 11)):
        a = random.uniform(0, math.tau); u = random.uniform(1.0, 1.08)
        o = outline(a, seed); x, y = math.cos(a) * R * o * u, math.sin(a) * R * o * u * D
        sz = random.uniform(.018, .035)
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=1)
        bo = bpy.context.active_object
        for v in bo.data.vertices:
            p = v.co.copy(); d = 1 + noise.noise(p * 1.7 + Vector((b, k, 3))) * .3
            v.co = Vector((p.x * sz * 1.3 * d, p.y * sz * d, p.z * sz * .8 * d))
        bo.location = (x, y, max(surface(x, y), .12)); bpy.ops.object.transform_apply(location=True)
        assign(bo, "cliff"); shade_auto(bo, 30)
        parts.append(bo)
    # Tree groves on the hill and a few on the ledges.
    for g in range(random.randint(3, 4)):
        ga = random.uniform(0, math.tau); gr = random.uniform(.05, .42) * R
        for t in range(random.randint(4, 7)):
            a = ga + random.uniform(-.5, .5); r = gr + random.uniform(-.06, .06)
            x, y = math.cos(a) * r + random.uniform(-.03, .03), math.sin(a) * r * D
            rad = random.uniform(.035, .06)
            bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=1)
            tr = bpy.context.active_object
            ts = Vector((g * 3.1 + t, t * 1.3, g))
            for v in tr.data.vertices:
                p = v.co.copy(); d = 1 + noise.noise(p * 2 + ts) * .25
                v.co = Vector((p.x * rad * d, p.y * rad * d, max(p.z, -.3) * rad * 1.15 * d))
            tr.location = (x, y, surface(x, y) + rad * .15); bpy.ops.object.transform_apply(location=True)
            assign(tr, "trees"); shade_auto(tr, 60)
            parts.append(tr)
    isl = join(parts, "FarIsle")
    export(isl, "far_isle_%d" % k, ["cliff", "cap", "trees"])
