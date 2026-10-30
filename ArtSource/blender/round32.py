"""Round 32 models (after the thirtieth review, three reviewers averaged): the studio floor in front of the mosaic
wall was "large empty planks" in every review, and the rugs "zero-thickness quads".
- trestle_wip: two A-frame trestles under a board, carrying a stained-glass panel in progress (a timber frame,
  dark came lines, a handful of coloured pieces laid in, the rest empty) and a glass cutter. About 1.6 x 0.95 x 0.8.
  Materials: wood, came, glass_a, glass_b, glass_c.
- stepladder: a wooden A-frame stepladder, about 1.9 m, to lean against the wall between two mosaics.
- woven_rug: replaces the flat rug (studio_furniture.py) at the same 7.0 x 3.2 footprint: a thicker woven mat with
  a softly rolled edge and fringe tassels along both short ends (same material slots).
Run: blender -b --factory-startup -P round32.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *


def rounded(name, size, radius, material, segs=3, loc=(0, 0, 0)):
    o = cube(name, size, loc, material)
    bevel(o, radius, segs, 50)
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.modifier_apply(modifier="Bevel")
    return o


def leg(name, a, b, r, material):
    """A square-ish timber from point a to point b."""
    a, b = Vector(a), Vector(b)
    d = b - a
    o = rounded(name, (r, r, d.length), r * .25, material, 2, (0, 0, 0))
    o.rotation_mode = "QUATERNION"; o.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(d.normalized())
    o.location = (a + b) / 2
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.transform_apply(location=True, rotation=True)
    return o


# ---------------------------------------------------------------- trestle table with a panel in progress
reset(); random.seed(3200)
parts = []
L, W, H = 1.6, .95, .74
for y in (-L / 2 + .18, L / 2 - .18):                       # two A-frame trestles
    for sx in (-1, 1):
        parts.append(leg("leg", (sx * .42, y - .08, 0), (sx * .36, y, H - .05), .05, "wood"))
        parts.append(leg("leg", (sx * .42, y + .08, 0), (sx * .36, y, H - .05), .05, "wood"))
    parts.append(rounded("beam", (.9, .08, .07), .015, "wood", 2, (0, y, H - .06)))
    parts.append(rounded("brace", (.84, .04, .04), .01, "wood", 2, (0, y, .28)))
parts.append(rounded("board", (W, L, .04), .012, "wood", 2, (0, 0, H)))
# The panel in progress: a frame, a came grid, some pieces laid in.
pw, pl, top = .78, 1.18, H + .02
parts.append(rounded("frame", (pw, .04, .035), .008, "wood", 2, (0, -pl / 2, top + .018)))
parts.append(rounded("frame", (pw, .04, .035), .008, "wood", 2, (0, pl / 2, top + .018)))
parts.append(rounded("frame", (.04, pl, .035), .008, "wood", 2, (-pw / 2, 0, top + .018)))
parts.append(rounded("frame", (.04, pl, .035), .008, "wood", 2, (pw / 2, 0, top + .018)))
# Came lines: a few straight and slanted leads dividing the panel.
cames = [((-pw / 2, -.25), (pw / 2, -.18)), ((-pw / 2, .12), (pw / 2, .2)), ((-.1, -pl / 2), (.05, pl / 2)), ((.2, -.18), (.28, .2))]
for (x0, y0), (x1, y1) in cames:
    a, b = Vector((x0, y0, top + .012)), Vector((x1, y1, top + .012))
    parts.append(leg("came", a, b, .018, "came"))
# Coloured pieces laid into some cells (low rounded tiles), the rest of the panel still empty board.
cells = [((-.25, -.4), .28, .26, "glass_a"), ((.17, -.42), .2, .24, "glass_b"), ((-.26, -.05), .3, .24, "glass_c"),
         ((.08, -.02), .2, .26, "glass_a"), ((.32, .02), .1, .3, "glass_c")]
for (cx, cy), sx, sy, m in cells:
    t = rounded("piece", (sx - .03, sy - .03, .012), .006, m, 2, (cx, cy, top + .008))
    parts.append(t)
# A few loose pieces and a glass cutter beside the panel.
for i, m in enumerate(("glass_b", "glass_a", "glass_c")):
    t = rounded("loose", (.07, .05, .012), .01, m, 2, (pw / 2 + .06, -.3 + i * .12, top + .006))
    t.rotation_euler = (0, 0, random.uniform(0, 3)); bpy.context.view_layer.objects.active = t; bpy.ops.object.transform_apply(rotation=True)
    parts.append(t)
parts.append(leg("cutter", (-pw / 2 - .02, .35, top + .01), (-pw / 2 - .02, .5, top + .01), .018, "came"))
tw = join(parts, "Trestle"); shade_auto(tw, 40)
export(tw, "trestle_wip", ["wood", "came", "glass_a", "glass_b", "glass_c"])

# ---------------------------------------------------------------- stepladder (leans on a wall: built upright)
reset()
parts = []
Hh, foot = 1.9, .55
for sx in (-1, 1):
    parts.append(leg("rail", (sx * .24, 0, 0), (sx * .18, foot * .55, Hh), .05, "wood"))         # front rails
    parts.append(leg("back", (sx * .22, foot * 1.25, 0), (sx * .18, foot * .55 + .04, Hh - .05), .04, "wood"))
for i in range(5):
    t = (i + 1) / 6
    w = .48 - .12 * t
    parts.append(rounded("step", (w, .1, .03), .008, "wood", 2, (0, foot * .55 * t + .03, Hh * t)))
parts.append(rounded("cap", (.42, .2, .04), .01, "wood", 2, (0, foot * .55 + .03, Hh + .01)))
ld = join(parts, "Ladder"); shade_auto(ld, 40)
export(ld, "stepladder", ["wood"])

# ---------------------------------------------------------------- woven rug, thicker, with fringe
reset()
L, Wd = 7.0, 3.2
# A fine grid along the length (the stripe bands are 0.25 m), given thickness.
bpy.ops.mesh.primitive_grid_add(x_subdivisions=136, y_subdivisions=8, size=1)
base = bpy.context.active_object
base.scale = (L - .2, Wd, 1); bpy.ops.object.transform_apply(scale=True)
for v in base.data.vertices: v.co.z = .03
sol = base.modifiers.new("t", "SOLIDIFY"); sol.thickness = .03; bpy.ops.object.modifier_apply(modifier="t")
base.data.materials.append(mat("rug_base"))
for v in base.data.vertices:                                         # a soft lift towards the rolled edges
    e = max(abs(v.co.y) / (Wd / 2), 0)
    v.co.z += .006 * e * e
base.data.materials.append(mat("rug_stripe")); base.data.materials.append(mat("rug_edge"))
for p in base.data.polygons:
    x = abs(p.center.x)
    band = int(x / .25)
    if x > (L - .2) / 2 - .3: p.material_index = 2
    elif band % 6 in (0, 1) and x > .3: p.material_index = 1
    else: p.material_index = 0
parts = [base]
for sx in (-1, 1):                                                    # fringe tassels on the short ends
    for i in range(30):
        y = -Wd / 2 + .08 + i * (Wd - .16) / 29
        x0 = sx * (L - .2) / 2
        t = leg("tassel", (x0, y, .012), (x0 + sx * random.uniform(.08, .12), y + random.uniform(-.015, .015), .004), .016, "rug_edge")
        parts.append(t)
rug = join(parts, "Rug"); shade_auto(rug, 40)
export(rug, "woven_rug", ["rug_base", "rug_stripe", "rug_edge"])
