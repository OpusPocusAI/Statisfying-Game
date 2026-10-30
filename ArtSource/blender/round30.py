"""Round 30 models (after the twenty-eighth review, three reviewers averaged):
- truck_door_r: a door that bulges outwards with rolled edges, instead of a flat panel (Unity 0.14 x 0.58 x 1.35).
- truck_tub_r: a cargo tub with flared, slightly bellied sides and a rolled rim (Unity 0.94 x 0.5 x 1.6).
- sand_drift_0..2: low lobed tongues of blown sand that spill over the road edge (about 2.4 x 0.9 m, a few cm high).
- potted_plant_b/c/d: a drooping fern in a wide bowl, a round leafy bush in a tall pot, an upright snake plant in a
  square planter (same material slots as potted_plant), so the studio stops repeating one plant.
- wicker_basket_b: a low, wide basket with folded linen instead of the rolled cloth (same slots as wicker_basket).
Run: blender -b --factory-startup -P round30.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise


def rounded(name, size, radius, material, segs=4, loc=(0, 0, 0)):
    o = cube(name, size, loc, material)
    bevel(o, radius, segs, 50)
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.modifier_apply(modifier="Bevel")
    return o


def subdiv(o, cuts):
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=cuts); bpy.ops.object.mode_set(mode="OBJECT")


# ---------------------------------------------------------------- door (Blender x = Unity x, y = Unity z, z = Unity y)
reset()
d = rounded("door", (.1, 1.35, .58), .045, "paint", 4)
subdiv(d, 6)
for v in d.data.vertices:
    u = v.co.y / .675; w = v.co.z / .29
    bulge = .03 * max(0.0, 1 - u * u) * max(0.0, 1 - w * w)
    v.co.x += math.copysign(bulge, v.co.x)
shade_auto(d, 45)
export(d, "truck_door_r", ["paint"])

# ---------------------------------------------------------------- cargo tub
reset()
W, D, H, t = .94, 1.6, .5, .045
outer = rounded("outer", (W - .06, D - .04, H), .07, "paint", 4, (0, 0, 0))
subdiv(outer, 3)
for v in outer.data.vertices:
    k = (v.co.z / H + .5)                                    # 0 bottom .. 1 top
    v.co.x *= .94 + .06 * k                                  # flares towards the rim
    belly = .012 * math.sin(math.pi * k)
    v.co.x += math.copysign(belly, v.co.x); v.co.y += math.copysign(belly * .6, v.co.y)
inner = cube("inner", (W - .06 - 2 * t, D - .04 - 2 * t, H), (0, 0, t + .001), None)
bevel(inner, .05, 3, 50); bpy.context.view_layer.objects.active = inner; bpy.ops.object.modifier_apply(modifier="Bevel")
boolean = outer.modifiers.new("cut", "BOOLEAN"); boolean.operation = "DIFFERENCE"; boolean.object = inner
bpy.context.view_layer.objects.active = outer; bpy.ops.object.modifier_apply(modifier="cut")
bpy.data.objects.remove(inner)
# Rolled rim: a rounded-rectangle tube around the top edge.
rw, rd, rr, n = W / 2 - .01, D / 2 - .01, .07, 6
pts = []
for cx, cy, a0 in ((rw - rr, rd - rr, 0), (-(rw - rr), rd - rr, 90), (-(rw - rr), -(rd - rr), 180), (rw - rr, -(rd - rr), 270)):
    for i in range(n + 1):
        a = math.radians(a0 + 90 * i / n)
        pts.append((cx + math.cos(a) * rr, cy + math.sin(a) * rr, H / 2))
rim = tube_along("rim", pts, .028, "rim", resolution=4, bevel_resolution=3, closed=True)
tub = join([outer, rim], "Tub"); shade_auto(tub, 45)
export(tub, "truck_tub_r", ["paint", "rim"])

# ---------------------------------------------------------------- sand drifts
for k in range(3):
    reset(); random.seed(3000 + k)
    bpy.ops.mesh.primitive_circle_add(vertices=40, radius=1, fill_type="TRIFAN")
    o = bpy.context.active_object
    subdiv(o, 3)
    s = Vector((k * 2.1, k * .7, 0))
    for v in o.data.vertices:
        r = math.hypot(v.co.x, v.co.y); a = math.atan2(v.co.y, v.co.x)
        lobes = 1 + .28 * noise.noise(Vector((math.cos(a) * 1.6, math.sin(a) * 1.6, 0)) + s) + .12 * math.sin(a * 3 + k)
        v.co.x *= 1.2 * lobes; v.co.y *= .45 * lobes
        v.co.z = .035 * max(0.0, 1 - r) ** .7 * (1 + .3 * noise.noise(v.co * 3 + s))
    assign(o, "sand"); shade_auto(o, 60)
    export(o, "sand_drift_%d" % k, ["sand"])

# ---------------------------------------------------------------- plants
def pot(kind):
    if kind == "bowl":
        return [cylinder("pot", .2, .2, (0, 0, .1), vertices=20, material="terracotta", radius2=.32),
                cylinder("lip", .34, .05, (0, 0, .21), vertices=20, material="terracotta"),
                cylinder("soil", .3, .02, (0, 0, .22), vertices=20, material="soil")], .23
    if kind == "tall":
        return [cylinder("pot", .17, .5, (0, 0, .25), vertices=18, material="terracotta", radius2=.2),
                cylinder("lip", .22, .05, (0, 0, .51), vertices=18, material="terracotta"),
                cylinder("soil", .19, .02, (0, 0, .53), vertices=18, material="soil")], .54
    p = rounded("pot", (.4, .4, .36), .04, "terracotta", 3, (0, 0, .18))
    return [p, cube("soil", (.34, .34, .02), (0, 0, .36), "soil")], .37


# Fern: many thin drooping fronds.
reset(); random.seed(810)
parts, top = pot("bowl")
for i in range(16):
    a = i / 16 * math.tau + random.uniform(-.15, .15); L = random.uniform(.45, .65)
    pts = [(0, 0, top), (math.cos(a) * L * .35, math.sin(a) * L * .35, top + L * .45), (math.cos(a) * L * .75, math.sin(a) * L * .75, top + L * .35), (math.cos(a) * L, math.sin(a) * L, top + L * .05)]
    f = tube_along("frond", pts, .028, "leaf_lit" if i % 4 == 0 else "leaf_dark", resolution=6, bevel_resolution=1)
    f.scale = (1, 1, .45); bpy.context.view_layer.objects.active = f; bpy.ops.object.transform_apply(scale=True)
    parts.append(f)
pl = join(parts, "Fern"); shade_auto(pl, 40)
export(pl, "potted_plant_b", ["terracotta", "soil", "leaf_dark", "leaf_lit"])

# Round leafy bush in a tall pot.
reset(); random.seed(820)
parts, top = pot("tall")
for i in range(14):
    a = random.uniform(0, math.tau); r = random.uniform(0, .18); h = random.uniform(.1, .42)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=2, radius=1, location=(math.cos(a) * r, math.sin(a) * r, top + .12 + h))
    b = bpy.context.active_object; s = random.uniform(.09, .14); b.scale = (s, s, s * .8)
    bpy.ops.object.transform_apply(scale=True)
    assign(b, "leaf_lit" if h > .3 else "leaf_dark"); parts.append(b)
pl = join(parts, "Bush"); shade_auto(pl, 45)
export(pl, "potted_plant_c", ["terracotta", "soil", "leaf_dark", "leaf_lit"])

# Snake plant: upright flat blades.
reset(); random.seed(830)
parts, top = pot("square")
for i in range(9):
    a = random.uniform(0, math.tau); r = random.uniform(.02, .12); L = random.uniform(.5, .85)
    x, y = math.cos(a) * r, math.sin(a) * r
    pts = [(x, y, top), (x * 1.3, y * 1.3, top + L * .5), (x * 1.6, y * 1.6, top + L)]
    bl = tube_along("blade", pts, .04, "leaf_dark" if i % 3 else "leaf_lit", resolution=4, bevel_resolution=1)
    bl.scale = (1, .3, 1); bpy.context.view_layer.objects.active = bl; bpy.ops.object.transform_apply(scale=True)
    bl.rotation_euler = (0, 0, random.uniform(0, math.pi)); bpy.ops.object.transform_apply(rotation=True)
    parts.append(bl)
pl = join(parts, "Snake"); shade_auto(pl, 40)
export(pl, "potted_plant_d", ["terracotta", "soil", "leaf_dark", "leaf_lit"])

# ---------------------------------------------------------------- low basket with folded linen
reset()
parts = [cylinder("basket", .3, .18, (0, 0, .09), vertices=22, material="wicker", radius2=.27)]
subdiv(parts[0], 3)
for v in parts[0].data.vertices:
    r = math.hypot(v.co.x, v.co.y)
    if r > .05:
        f = 1 + .025 * math.sin(v.co.z * 80); v.co.x *= f; v.co.y *= f
parts.append(tube_along("rim", [(math.cos(a) * .3, math.sin(a) * .3, .18) for a in [i / 18 * math.tau for i in range(18)]], .02, "wicker_dark", resolution=4, bevel_resolution=2, closed=True))
for i in range(3):                                           # folded linen squares, stacked and slightly turned
    c = rounded("fold", (.36, .3, .04), .015, "cloth", 2, (0, 0, .2 + i * .042))
    c.rotation_euler = (0, 0, i * .25 - .2); bpy.context.view_layer.objects.active = c; bpy.ops.object.transform_apply(rotation=True)
    parts.append(c)
bk = join(parts, "Basket"); shade_auto(bk, 40)
export(bk, "wicker_basket_b", ["wicker", "wicker_dark", "cloth"])
