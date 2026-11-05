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
ISLAND_SCALE = {0: (25.543, 44.399, 36.400), 1: (23.973, 46.330, 48.537), 2: (23.303, 39.806, 54.437)}   # scene scale per island: Blender x, y, z = Unity x, z, y
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
    # Round 50 (review 46, all three: the round 49 fused groves read as "fused knobs" and "lobes pasted on the hills";
    # reviewers want layered canopies with distinct pine silhouettes, as on Tchia's islands). The islands are scaled
    # about 24 x 40 m across and 36-54 m tall in the scene, so last round's crowns were only 2-3 m wide. Each island now
    # carries 5-11 individual umbrella pines built in metres (a leaning tapered trunk, a broad flat-topped crown of
    # 3-4 faceted pads in tiers, like the beach's coastal pines), tallest in the grove centres and shorter toward the
    # edges, then divided by the island's scene scale per axis so they stay round and upright in the scene.
    # Trunks use the "cliff" slot (warm sandstone, which reads as bark at 100-170 m); pads use "trees".
    SX, SY, SZ = ISLAND_SCALE[k]
    placed = []

    def pad(centre, r, hh, seed):
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=1, location=(0, 0, 0))
        o = bpy.context.active_object
        ps = Vector((seed, seed * 1.7, seed * .3))
        for v in o.data.vertices:
            p = v.co.copy(); d = 1 + noise.fractal(p * 1.6 + ps, .6, 2.0, 3) * .26
            v.co = Vector((p.x * r * d, p.y * r * d, p.z * hh * d))
            if v.co.z < -hh * .35: v.co.z = -hh * .35 + (v.co.z + hh * .35) * .3   # flatter underside
        dec = o.modifiers.new("dec", "DECIMATE"); dec.ratio = .5
        bpy.ops.object.modifier_apply(modifier="dec")
        o.location = centre; bpy.ops.object.transform_apply(location=True)
        assign(o, "trees")
        for p in o.data.polygons: p.use_smooth = False
        return o

    def pine(x, y, hm, seed):
        """An umbrella pine hm metres tall at island-unit position (x, y), rooted in the hill."""
        rnd = random.Random(seed)
        lean = Vector((rnd.uniform(-.12, .12), rnd.uniform(-.12, .12), 0)) * hm
        th = hm * .58
        bpy.ops.mesh.primitive_cone_add(vertices=7, radius1=.26 + hm * .012, radius2=.11, depth=th, location=(0, 0, th * .5))
        trunk = bpy.context.active_object
        for v in trunk.data.vertices:
            t = max(0.0, v.co.z / th); v.co = v.co + lean * (t * t)
        assign(trunk, "cliff")
        for p in trunk.data.polygons: p.use_smooth = False
        top = Vector((0, 0, th)) + lean
        pads = [(top + Vector((0, 0, hm * .08)), hm * .50, hm * .17),                       # the broad umbrella
                (top + Vector((rnd.uniform(-.8, .8), rnd.uniform(-.8, .8), hm * .24)), hm * .36, hm * .14),
                (top + Vector((rnd.uniform(-.5, .5), rnd.uniform(-.5, .5), hm * .36)), hm * .22, hm * .10)]
        for i in range(rnd.randint(1, 2)):
            ang = rnd.uniform(0, math.tau); rr = hm * .32
            pads.append((top + Vector((math.cos(ang) * rr, math.sin(ang) * rr, hm * rnd.uniform(.02, .12))), hm * rnd.uniform(.24, .32), hm * .12))
        objs = [trunk] + [pad(c, r, hh, seed * 10 + i) for i, (c, r, hh) in enumerate(pads)]
        tree = join(objs, "Pine")
        bpy.context.view_layer.objects.active = tree; tree.select_set(True)
        bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)   # bake the cone's object offset before rescaling
        zb = surface(x, y) - .6 / SZ                                               # trunk foot sunk 60 cm into the hill
        for v in tree.data.vertices:
            v.co = Vector((x + v.co.x / SX, y + v.co.y / SY, zb + v.co.z / SZ))
        return tree

    for g in range(random.randint(2, 3)):
        ga = random.uniform(0, math.tau); gr = random.uniform(.04, .40) * R
        gx, gy = math.cos(ga) * gr, math.sin(ga) * gr * D
        for t in range(random.randint(2, 4)):
            for attempt in range(12):
                a = random.uniform(0, math.tau); r = random.uniform(0, .13)
                x, y = gx + math.cos(a) * r, gy + math.sin(a) * r * D
                u = math.hypot(x, y / D) / (R * outline(math.atan2(y / D, x), seed))
                if u > .78: continue
                if all(math.hypot((x - px) * SX, (y - py) * SY) > 4.5 for px, py in placed): break
            else:
                continue
            placed.append((x, y))
            centre = 1 - min(1.0, r / .13)                                          # tall in the grove centre, short at its edge
            hm = random.uniform(6.0, 8.0) + centre * random.uniform(2.0, 4.0)
            parts.append(pine(x, y, hm, k * 100 + g * 10 + t))
    for t in range(random.randint(1, 2)):                                           # a lone pine or two out on the ledges
        a = random.uniform(0, math.tau); u = random.uniform(.55, .8)
        o = outline(a, seed); x, y = math.cos(a) * R * o * u, math.sin(a) * R * o * u * D
        if all(math.hypot((x - px) * SX, (y - py) * SY) > 4.5 for px, py in placed):
            placed.append((x, y)); parts.append(pine(x, y, random.uniform(5.0, 7.0), k * 100 + 90 + t))
    isl = join(parts, "FarIsle")
    export(isl, "far_isle_%d" % k, ["cliff", "cap", "trees"])
