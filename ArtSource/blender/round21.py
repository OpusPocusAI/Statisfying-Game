"""Round 21 models (after the nineteenth review, three reviewers averaged): six tumbled sea-glass pieces that
read as worn glass slabs rather than puffy candy. Every review called the glass "the same inflated lentil".
Real sea glass is a flattish piece of bottle wall: two near-flat faces (one slightly curved, from the
bottle), softly rounded edges, an irregular outline, and often one flatter worn facet or a broken edge.
- glass_slab_0..5. About 1.0 across in Blender XZ, thickness along Blender Y (Unity Z), origin at centre.
Run: blender -b --factory-startup -P round21.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise

# (outline sides 0 = oval, roundness, stretch x, stretch z, thickness, bottle curve, broken-edge cut)
# Round 28 (review 26: "nearly all the same almond lens"): six clearly different pieces: an oval, a rounded
# triangle, a D-shaped chip, a thin sliver, a chunky nugget and a long shard. Lower roundness keeps the corners.
shapes = [(0, 1, 1.0, .74, .17, .06, .30), (3, .55, 1.0, .92, .16, .05, .0), (4, .6, 1.0, .7, .15, .07, .26),
          (0, 1, 1.0, .56, .14, .04, .22), (5, .62, .88, .82, .21, .04, .0), (3, .58, 1.0, .62, .15, .06, .28)]
# Round 37 (review 33, all three: "plump almonds", "gummy sweets", "jelly beans"): flat pieces of bottle wall about a
# quarter as thick as they are wide (they were 45-60%), each outline lopsided, four of six with one worn, straighter
# broken edge (a soft chord in the outline, still rounded in section), and broad shallow dents across both faces so
# the glint breaks into two or three flashes instead of one blob.


def outline_r(a, sides, rk):
    if sides == 0:
        return 1.0
    seg = math.tau / sides
    x = (a % seg) - seg / 2
    return (math.cos(seg / 2) / math.cos(x)) * (1 - rk) + rk * math.cos(seg / 2) * 1.05


for k, (sides, rk, sx, sz, th, curve, cut) in enumerate(shapes):
    reset(); random.seed(2100 + k)
    bpy.ops.mesh.primitive_uv_sphere_add(segments=72, ring_count=28, radius=.5)   # round 31: smoother outline (reviews: "jagged right edge")
    o = bpy.context.active_object
    s = Vector((k * 1.9, k * .6, k * 2.7))
    rot = random.uniform(0, math.tau)
    for v in o.data.vertices:
        p = v.co.copy()
        a = math.atan2(p.z, p.x) + rot
        rr = math.hypot(p.x, p.z) / .5            # 0 centre .. 1 at the equator
        lump = 1 + noise.noise(Vector((math.cos(a) * 1.7, math.sin(a) * 1.7, 0)) + s) * .14
        lop = 1 + .1 * math.cos(a - rot * .7 + 1.1)                      # lopsided: one side fuller than the other
        orr = outline_r(a, sides, rk) * lump * lop
        if cut > 0:
            # A worn broken edge: the outline follows a chord at distance (1 - cut) from the centre, softly blended.
            da = (a - rot * 1.3 + math.pi) % math.tau - math.pi
            if abs(da) < math.pi / 2 - .05:
                chord = (1 - cut) / math.cos(da)
                kb = .12
                h = max(0.0, min(1.0, .5 + .5 * (chord - orr) / kb))
                orr = chord * (1 - h) + orr * h - kb * h * (1 - h)
        x, z = p.x * sx * orr, p.z * sz * orr
        # Slab profile: flat faces with rounded edges (a superellipse in thickness, not a sphere).
        yn = p.y / .5
        # Round 27 (review 25: "a sand biscuit", "uniform-thickness slab with a hard cut edge"): a lens, not a bent
        # plate. Both faces bulge outwards and the piece thins towards a rolled rim, so the middle is about twice
        # as thick as the edge; no straight broken edge.
        ys = math.copysign(abs(yn) ** .7, yn) * .5 * th * (1 - (.2 if th > .33 else .32) * rr * rr)   # the nugget keeps its bulk
        dome = max(curve, .1) * .85 * (1 - rr * rr)
        ys += dome * (1 if yn > 0 else -.6)
        if False and cut > 0:
            d = x * math.cos(rot) + z * math.sin(rot)
            lim = .5 - cut * .5
            if d > lim:
                x -= (d - lim) * math.cos(rot) * .92; z -= (d - lim) * math.sin(rot) * .92
        # Fine tumbled unevenness.
        ys += noise.noise(Vector((x, z, 0)) * 5 + s) * .008 * (1 - rr ** 4)   # round 38: none at the thin rim, where it zigzagged the silhouette
        # Broad shallow dents (worn facets), strongest on the faces, none at the rim.
        ys += noise.noise(Vector((x, z, 1.7)) * 2.2 + s) * .022 * (1 - rr * rr) * (1 if yn > 0 else .6)
        v.co = Vector((x, ys, z))
    assign(o, "glass")
    shade_auto(o, 50)
    export(o, "glass_slab_%d" % k, ["glass"])
