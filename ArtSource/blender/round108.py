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
Round 108: rebuilt as thin plates (see PLATE below).
Run: blender -b --factory-startup -P round108.py
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
# Round 102 (review 98, all three: the basin hero "a thick green jelly slab", "a thick jelly or soap bar"): piece 5's outline
# reaches only about 0.55 of the nominal radius (its rounded triangle and two broken edges), so at the shared thickness it stood
# about 30% of its own width at the thick edge, half again the 19% the other hero pieces have. It is now scaled to match.
HERO_TH_K = {5: .4}
# Round 108 (review 104, all three: the held piece "pillowy green jelly", "a thick gummy jelly", "thick gummy candy"; owner: "Thin hero
# glass plate"): a plate of bottle wall about 10% of its width thick at the thick edge and 4% at the thin one (was about 19% / 7%,
# with face bumps up to half the thickness that domed it), flat faces and a narrow rounded rim.
PLATE = .035
PLATE_K = {5: .55}
SIDE_N = {5: .25}   # piece 5 is only about half as wide, so the wedge is measured across its own width
U0 = .8
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
        # Round 108: a plate, not a pillow. The sphere's latitude is remapped so most of it is one flat face at a constant
        # half-thickness (the wedge and the bottle-wall bend give the only curvature), turning over a narrow rounded rim whose
        # width is about the local half-thickness, instead of a dome that rolls over a broad bevel.
        polar = math.acos(max(-1.0, min(1.0, abs(p.y) / .5)))
        u = polar / (math.pi / 2)
        dx, dz = p.x, p.z
        dl = math.hypot(dx, dz)
        if dl < 1e-9:
            dx, dz, dl = 1.0, 0.0, 1.0
        dx, dz = dx / dl, dz / dl
        ro = .5 * orr
        wd = rot * 1.7 + 0.9
        side = max(-1.0, min(1.0, (dx * sx * ro * math.cos(wd) + dz * sz * ro * math.sin(wd)) / SIDE_N.get(k, .45)))
        hl = PLATE * PLATE_K.get(k, 1.0) * (1.0 + .45 * side)
        e = hl * 1.15
        if u < U0:
            r, yh = (ro - e) * (u / U0), hl
        else:
            ph = (u - U0) / (1 - U0) * math.pi / 2
            r, yh = ro - e + e * math.sin(ph), hl * math.cos(ph) ** .85
        rr = r / max(ro, 1e-6)
        x, z = dx * r * sx, dz * r * sz
        yn = 1.0 if p.y >= 0 else -1.0
        ys = yn * yh
        face = 1.0 if u < U0 else 0.0
        # Flake scars: each chip leaves a shallow dished scar on the face beside it.
        ys -= yn * .35 * hl * chip * smooth(.72, .95, rr) * face
        ys += BEND * (1 - (x / .5) ** 2)
        ys += noise.noise(Vector((x, z, 1.7)) * 2.2 + s) * .004 * (1 - rr * rr)
        ys += noise.noise(Vector((x, z, 0)) * 6 + s) * .0015 * (1 - rr ** 4)
        v.co = Vector((x, ys, z))
    assign(o, "glass")
    shade_auto(o, 40)
    export(o, "glass_basin_%d" % k, ["glass"])
