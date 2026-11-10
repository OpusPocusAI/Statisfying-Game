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
shapes = [(4, .4, 1.0, .84, .22, .035, .22), (3, .62, 1.0, .95, .21, .033, .18), (0, 1, 1.0, .9, .23, .05, .42),
          (5, .45, 1.0, .86, .21, .03, .2), (6, .5, .95, .9, .23, .03, .2), (3, .84, 1.0, .9, .21, .04, .26)]   # round 100 (review 96, all three: the palm piece "a plastic guitar pick", "a pointed pick triangle"): piece 5 (glass-03, the verification piece) gets blunt, well-rounded corners and a broader body   # round 61: every piece has one straight broken edge (review 57: "no straight broken edge"). Round 58 (review 54, all three: "thin chips", "a thin curved sheet"): thicker, so the wedge measures about 30% of the width at its thick edge and 11% at its thin edge (5.6)   # round 43: rounder triangle corners (5.6: no pointed tips)   # round 43: the fixed form in ART_DIRECTION 5.6 (about 30% of the width at the thick edge, 12% at the thin edge)
# Round 40 (review 36, all three: "nearly all one almond / lens silhouette"): the pieces lay flat, but most outlines
# tapered to soft points at both ends and the low camera foreshortened them into lenses. Now blunt, broad pieces a
# viewer tells apart at a glance: a squarish chip, a rounded triangle, a D-shaped half-disc, a pentagon chip, a chunky
# hexagonal nugget and a triangle shard, most with one straight broken edge; widths close to lengths.
# Round 37 (review 33, all three: "plump almonds", "gummy sweets", "jelly beans"): flat pieces of bottle wall about a
# quarter as thick as they are wide (they were 45-60%), each outline lopsided, four of six with one worn, straighter
# broken edge (a soft chord in the outline, still rounded in section), and broad shallow dents across both faces so
# the glint breaks into two or three flashes instead of one blob.


BEND = -.06   # round 58: a gentler curl (review 54: the held piece "a bent film"). Round 54: must match FrostedGlass _Bend (set by Round54Pass); negative so the work pieces (rotated 90 about X) lie outer face up


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
        def full_r(a):
            lump = 1 + noise.noise(Vector((math.cos(a) * 1.7, math.sin(a) * 1.7, 0)) + s) * .14
            lop = 1 + .1 * math.cos(a - rot * .7 + 1.1)                      # lopsided: one side fuller than the other
            orr = outline_r(a, sides, rk) * lump * lop
            if cut > 0:
                # A worn broken edge: the outline follows a chord at distance (1 - cut) from the centre, softly blended.
                da = (a - rot * 1.3 + math.pi) % math.tau - math.pi
                if abs(da) < math.pi / 2 - .05:
                    chord = (1 - cut) / math.cos(da)
                    kb = .26   # round 45: rounder corners where the broken edge meets the outline (review 41: "pointed tips")
                    h = max(0.0, min(1.0, .5 + .5 * (chord - orr) / kb))
                    orr = chord * (1 - h) + orr * h - kb * h * (1 - h)
            return orr
        # Round 61 (review 57, all three: "rounded jelly lozenges, no straight broken edge"): about +/-10 degrees (was 24), so
        # only the sharp tips round over and the straight broken edges stay straight.
        # Round 60 (review 56, all three: the held piece "a shield with a pointed tip", against 5.6's "no pointed tips"): the
        # outline radius is averaged over about +/-24 degrees around each angle, so every convex corner rounds over
        # (tumbled glass wears its corners first) while the broad outline, straight broken edge and lopsidedness stay.
        orr = sum(full_r(a + d * .03) * w for d, w in ((-6, .03), (-5, .05), (-4, .07), (-3, .09), (-2, .11), (-1, .12), (0, .13), (1, .12), (2, .11), (3, .09), (4, .07), (5, .05), (6, .03))) / 1.07
        x, z = p.x * sx * orr, p.z * sz * orr
        # Slab profile: flat faces with rounded edges (a superellipse in thickness, not a sphere).
        yn = p.y / .5
        # Round 27 (review 25: "a sand biscuit", "uniform-thickness slab with a hard cut edge"): a lens, not a bent
        # plate. Both faces bulge outwards and the piece thins towards a rolled rim, so the middle is about twice
        # as thick as the edge; no straight broken edge.
        # Round 43 (5.6 fixed form): a blunt, rounded rim with real thickness. Seen edge-on, the old profile (power .7, a
        # strong taper) ran to a knife edge, which is the "almond / lens" every review saw. Power .38 keeps the faces flat
        # and rounds the rim over; a gentle taper leaves the thin side about 12% of the width.
        ys = math.copysign(abs(yn) ** .38, yn) * .5 * th * (1 - .12 * rr * rr)
        # Round 54 (review 50, all three: the held piece is "a pillowy dome", "a creased jelly lump"): both faces bulged
        # outwards, so every piece was a lens or pillow whatever its outline. A shard of bottle wall is a curved plate:
        # the outer face convex, the inner face concave, bent the same way around the bottle's axis (Blender Z). The
        # whole section now follows one cylindrical bend (sagitta about 10% of the width, as for a 3 cm shard of a
        # 7 cm bottle); FrostedGlass measures thickness from the same bent mid-surface (_Bend). Added after the wedge
        # taper below, so the taper does not scale the bend.
        if False and cut > 0:
            d = x * math.cos(rot) + z * math.sin(rot)
            lim = .5 - cut * .5
            if d > lim:
                x -= (d - lim) * math.cos(rot) * .92; z -= (d - lim) * math.sin(rot) * .92
        # Fine tumbled unevenness.
        # Round 41 (review 37, all three: "puffy domes of the same thickness", "blister-pack bubbles"): a wedge. Each piece
        # is thick along one side and tapers to a thin worn edge on the other, as a shard of bottle wall does.
        wd = rot * 1.7 + 0.9
        side = max(-1.0, min(1.0, (x * math.cos(wd) + z * math.sin(wd)) / .45))
        ys *= 1.0 + .45 * side   # round 58: .45 (was .42)
        ys += BEND * (1 - (x / .5) ** 2)   # round 54: the bottle-wall bend, shared by both faces
        ys += noise.noise(Vector((x, z, 0)) * 5 + s) * .008 * (1 - rr ** 4)   # round 38: none at the thin rim, where it zigzagged the silhouette
        # Broad shallow dents (worn facets), strongest on the faces, none at the rim.
        ys += noise.noise(Vector((x, z, 1.7)) * 2.2 + s) * .022 * (1 - rr * rr) * (1 if yn > 0 else .6)
        v.co = Vector((x, ys, z))
    assign(o, "glass")
    shade_auto(o, 50)
    export(o, "glass_slab_%d" % k, ["glass"])
