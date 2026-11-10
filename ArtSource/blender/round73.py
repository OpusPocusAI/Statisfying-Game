"""Round 73 models (after review 69, all three: the revealed basin piece "a thick jelly or gummy shield", "a mint jelly or
soap bar", "pillowy and evenly thick, like a gummy sweet"). Two shader rounds (71-72) did not move that reading. The same
glass_slab meshes pass in the trays and the palm at a few centimetres across; at the basin the piece fills a fifth of the
frame, and the soft, fully rounded rim and smooth faces read as a moulded sweet at that size.
- glass_basin_0..5: hero versions of glass_slab_0..5 (round21.py), same outlines, wedge, bend and scale (about 1.0 across,
  thickness along Blender Y, so FrostedGlass's _Bend and thickness thresholds still hold), for the basin only:
  - a crisper edge: flatter faces that turn over a narrow worn bevel, not a rolled, pillowy rim;
  - small conchoidal chips along the rim: shallow scalloped bites out of the outline, each with a dished flake scar on
    the face beside it, as on real worn bottle glass;
  - fine pitting over both faces (the frosted "C-marks" of tumbled glass), so the glint breaks up and the surface reads
    as worn glass rather than a moulded sweet.
Run: blender -b --factory-startup -P round73.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise

shapes = [(4, .4, 1.0, .84, .22, .035, .22), (3, .62, 1.0, .95, .21, .033, .18), (0, 1, 1.0, .9, .23, .05, .42),
          (5, .45, 1.0, .86, .21, .03, .2), (6, .5, .95, .9, .23, .03, .2), (3, .84, 1.0, .9, .21, .04, .26)]   # round 100 (review 96, all three: the palm piece "a plastic guitar pick", "a pointed pick triangle"): piece 5 (glass-03, the verification piece) gets blunt, well-rounded corners and a broader body   # as round21.py
# Round 101 (review 97, all three: the palm piece "a rounded, opaque-looking aqua lozenge, more jelly sweet than shard", the 05b
# edge "jelly or gummy"; A: "one or two straight broken edges"): piece 5's single broken edge never bit, because its chord
# (0.74) lay outside the rounded outline (about 0.55). It now has two broken edges that do bite, straight and meeting at one
# blunt corner, with a crisp blend and a calmer outline wobble, while the far side stays rounded and worn.
CUTS = {5: [(0, .6), (1.35, .57)]}
CUT_KB = {5: .03}
LUMP = {5: .07}

BEND = -.06   # must match round21.py and FrostedGlass _Bend
# Round 74 (review 70, all three: the hero piece "a thick gummy block", "thick vertical sides"; owner's decision, November 8):
# the basin hero shard is thinner than 5.6's 30%/12% wedge, about 19% of the width at its thick edge and 7% at its thin
# edge, and its edge wall slopes over a broader bevel instead of standing straight up.
HERO_TH = .6
# Round 75 (review 71, all three: "blobby highlights", "a soft rim"): the face pits caught the key light as soft rings, so they
# are off (the sharp flashes come from FrostedGlass at the basin), and the bevel is tighter again (power .22, was .3).


def outline_r(a, sides, rk):
    if sides == 0:
        return 1.0
    seg = math.tau / sides
    x = (a % seg) - seg / 2
    return (math.cos(seg / 2) / math.cos(x)) * (1 - rk) + rk * math.cos(seg / 2) * 1.05


def smooth(e0, e1, x):
    t = max(0.0, min(1.0, (x - e0) / (e1 - e0)))
    return t * t * (3 - 2 * t)


for k, (sides, rk, sx, sz, th, curve, cut) in enumerate(shapes):
    reset(); random.seed(2100 + k)
    bpy.ops.mesh.primitive_uv_sphere_add(segments=160, ring_count=72, radius=.5)
    o = bpy.context.active_object
    s = Vector((k * 1.9, k * .6, k * 2.7))
    rot = random.uniform(0, math.tau)
    for v in o.data.vertices:
        p = v.co.copy()
        a = math.atan2(p.z, p.x) + rot
        rr = math.hypot(p.x, p.z) / .5

        def full_r(a):
            lump = 1 + noise.noise(Vector((math.cos(a) * 1.7, math.sin(a) * 1.7, 0)) + s) * LUMP.get(k, .14)
            lop = 1 + .1 * math.cos(a - rot * .7 + 1.1)
            orr = outline_r(a, sides, rk) * lump * lop
            for off, depth in CUTS.get(k, [(0, cut)] if cut > 0 else []):
                # A worn broken edge: the outline follows a chord at distance (1 - depth) from the centre, softly blended.
                da = (a - rot * 1.3 + off + math.pi) % math.tau - math.pi
                if abs(da) < math.pi / 2 - .05:
                    chord = (1 - depth) / math.cos(da)
                    kb = CUT_KB.get(k, .26)   # round 45: rounder corners where the broken edge meets the outline (review 41: "pointed tips")
                    h = max(0.0, min(1.0, .5 + .5 * (chord - orr) / kb))
                    orr = chord * (1 - h) + orr * h - kb * h * (1 - h)
            return orr
        orr = sum(full_r(a + d * .03) * w for d, w in ((-6, .03), (-5, .05), (-4, .07), (-3, .09), (-2, .11), (-1, .12), (0, .13), (1, .12), (2, .11), (3, .09), (4, .07), (5, .05), (6, .03))) / 1.07
        # Conchoidal chips: shallow scalloped bites out of the outline, uneven in size and spacing.
        ca = a * 7 + noise.noise(Vector((math.cos(a) * 2.3, math.sin(a) * 2.3, 4.0)) + s) * 2.4
        chip = max(0.0, math.sin(ca)) ** 3 * smooth(.1, .5, noise.noise(Vector((math.cos(a) * 3.1, math.sin(a) * 3.1, 9.0)) + s) + .25)
        orr *= 1 - .03 * chip
        x, z = p.x * sx * orr, p.z * sz * orr
        yn = p.y / .5
        # A crisper edge: flat faces turning over a narrow worn bevel (round21.py's power .38 rolled the whole rim over).
        ys = math.copysign(abs(yn) ** .22, yn) * .5 * th * HERO_TH * (1 - .1 * rr * rr)
        wd = rot * 1.7 + 0.9
        side = max(-1.0, min(1.0, (x * math.cos(wd) + z * math.sin(wd)) / .45))
        ys *= 1.0 + .45 * side
        # Flake scars: each chip leaves a shallow dished scar on the face beside it.
        face = abs(yn) ** 2
        ys -= math.copysign(1, yn) * .028 * chip * smooth(.68, .98, rr) * face * (1 + .45 * side)
        ys += BEND * (1 - (x / .5) ** 2)
        ys += noise.noise(Vector((x, z, 0)) * 5 + s) * .008 * (1 - rr ** 4)
        ys += noise.noise(Vector((x, z, 1.7)) * 2.2 + s) * .022 * (1 - rr * rr) * (1 if yn > 0 else .6)
        # Fine pitting on both faces: small round dents, none on the edge wall.
        pit = smooth(.42, .62, noise.noise(Vector((x, z, 3.3)) * 26 + s))
        ys -= math.copysign(1, yn) * 0 * pit * face * (1 - smooth(.85, 1.0, rr))
        v.co = Vector((x, ys, z))
    assign(o, "glass")
    shade_auto(o, 40)
    export(o, "glass_basin_%d" % k, ["glass"])
