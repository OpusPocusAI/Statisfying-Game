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

    # Round 63: no talus boulders. At 100-170 m the small boulders on the cliff foot read as dark "hexagon patches" (reviews
    # 45-59), and smoothed they read as round bubbles stuck on the cliff face.
    # Round 63 (review 59, all three, and every review since 46: the island trees are "stacks of flat plates", "flat
    # umbrella layers on bare rock", far from Tchia's layered foliage). The umbrella pines were faceted, flat-shaded pads.
    # Now each island carries groves of broadleaf crowns, built in metres: every crown a broad lower tier and a smaller
    # rounded upper tier of overlapping lobes, with low undergrowth between the trunks, and each grove fused into ONE
    # surface (voxel remesh), crinkled into leafy clumps, smoothed and smooth-shaded, so it shows lit tops, shaded
    # undersides and the layered build-up of a canopy rather than separate plates. Crowns sit close enough to overlap
    # into a continuous canopy over the hilltop, with a few lone crowns out on the ledges. Trunks use the "cliff" slot
    # (warm sandstone, which reads as bark at 100-170 m); canopies use "trees".
    SX, SY, SZ = ISLAND_SCALE[k]
    rng = random.Random(9100 + k)

    def surface_m(xm, ym):
        return surface(xm / SX, ym / SY) * SZ

    def u_at(xm, ym):
        x, y = xm / SX, ym / SY
        return math.hypot(x, y / D) / (R * outline(math.atan2(y / D, x), seed))

    def lobe(c, r, sq):
        bpy.ops.mesh.primitive_uv_sphere_add(segments=18, ring_count=10, radius=1, location=(0, 0, 0))
        o = bpy.context.active_object
        for v in o.data.vertices:
            z = v.co.z * sq
            if z < -.4 * sq: z = -.4 * sq + (z + .4 * sq) * .7         # a slightly fuller top than underside, but still round
            v.co = Vector((v.co.x * r, v.co.y * r, z * r))
        o.location = c; bpy.ops.object.transform_apply(location=True)
        return o

    def grove(gx, gy, n, spread, umax, gseed):
        crowns = []
        for t in range(n * 6):
            if len(crowns) >= n: break
            a = rng.uniform(0, math.tau); rr = spread * math.sqrt(rng.random())
            x, y = gx + math.cos(a) * rr, gy + math.sin(a) * rr
            if u_at(x, y) > umax: continue
            edge = rr / spread if spread > 0 else 0
            rc = rng.uniform(2.6, 4.0) * (1.15 - .3 * edge)
            if any(math.hypot(x - cx, y - cy) < (rc + cr) * .55 for cx, cy, cr, _, _ in crowns): continue
            zg = surface_m(x, y)
            crowns.append((x, y, rc, zg, zg + rng.uniform(.6, 1.8) * (1.1 - .35 * edge) + rc * .45))
        if not crowns: return []
        lobes = []
        for (x, y, rc, zg, zc) in crowns:
            for i in range(rng.randint(5, 7)):                                    # the broad lower tier
                a = rng.uniform(0, math.tau); d = rc * rng.uniform(.3, .6)
                lobes.append(lobe(Vector((x + math.cos(a) * d, y + math.sin(a) * d, zc + rng.uniform(-.15, .1) * rc)), rc * rng.uniform(.45, .6), .75))
            for i in range(rng.randint(2, 4)):                                    # the rounded upper tier
                a = rng.uniform(0, math.tau); d = rc * rng.uniform(0, .32)
                lobes.append(lobe(Vector((x + math.cos(a) * d, y + math.sin(a) * d, zc + rc * rng.uniform(.38, .55))), rc * rng.uniform(.38, .5), .85))
            if True:                                                              # undergrowth at the foot, so the canopy meets the hill
                a = rng.uniform(0, math.tau)
                lobes.append(lobe(Vector((x + math.cos(a) * rc * .5, y + math.sin(a) * rc * .5, zg + .2)), rc * rng.uniform(.55, .7), .6))
                lobes.append(lobe(Vector((x, y, zg - .3)), rc * .75, .45))
        g = join(lobes, "Grove")
        bpy.context.view_layer.objects.active = g
        rm = g.modifiers.new("fuse", "REMESH"); rm.mode = "VOXEL"; rm.voxel_size = .38
        bpy.ops.object.modifier_apply(modifier="fuse")
        ps = Vector((gseed * .37, gseed * .71, gseed * .13))
        g.data.update()
        for v in g.data.vertices:                                                 # leafy clumps, not a smooth balloon
            p = v.co.copy()
            v.co = p + v.normal * (noise.noise(p * .55 + ps) * .55 + noise.noise(p * 1.4 + ps * 2) * .2)
        sm = g.modifiers.new("soften", "CORRECTIVE_SMOOTH"); sm.factor = .5; sm.iterations = 5
        bpy.ops.object.modifier_apply(modifier="soften")
        target = 1400 + 650 * len(crowns)
        if len(g.data.polygons) > target:
            dec = g.modifiers.new("reduce", "DECIMATE"); dec.ratio = target / len(g.data.polygons)
            bpy.ops.object.modifier_apply(modifier="reduce")
        assign(g, "trees")
        for p in g.data.polygons: p.use_smooth = True
        out = [g]
        for (x, y, rc, zg, zc) in crowns:                                         # trunks, leaning a little
            lean = Vector((rng.uniform(-.5, .5), rng.uniform(-.5, .5), 0))
            th = zc - zg + .6 - rc * .1
            bpy.ops.mesh.primitive_cone_add(vertices=8, radius1=.42, radius2=.2, depth=th, location=(0, 0, th * .5))
            tr = bpy.context.active_object
            for v in tr.data.vertices:
                t = max(0.0, v.co.z / th); v.co = v.co + lean * (t * t)
            tr.location = (x, y, zg - .6); bpy.ops.object.transform_apply(location=True)
            assign(tr, "cliff"); out.append(tr)
        for o in out:                                                             # metres -> island units
            for v in o.data.vertices:
                v.co = Vector((v.co.x / SX, v.co.y / SY, v.co.z / SZ))
        return out

    for gi in range(rng.randint(2, 3)):                                           # groves over the hilltop
        a = rng.uniform(0, math.tau); r = rng.uniform(0, .3)
        gx, gy = math.cos(a) * r * R * SX, math.sin(a) * r * R * D * SY
        parts += grove(gx, gy, rng.randint(7, 11), rng.uniform(6.0, 8.0), .68, k * 10 + gi)
    for t in range(rng.randint(1, 2)):                                            # small clumps on the upper ledges
        a = rng.uniform(0, math.tau); u = rng.uniform(.6, .68)
        o = outline(a, seed)
        parts += grove(math.cos(a) * R * o * u * SX, math.sin(a) * R * o * u * D * SY, 2, 2.5, .72, k * 10 + 5 + t)
    isl = join(parts, "FarIsle")
    export(isl, "far_isle_%d" % k, ["cliff", "cap", "trees"])
