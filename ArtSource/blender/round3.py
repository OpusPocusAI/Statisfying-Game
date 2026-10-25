"""Round 3 models, from the second side-by-side (ArtReference/comparison.md):
- far_island_0..2: layered cliff bands with a green cap and tree clumps (Tchia, A Short Hike), replacing flat green slabs
- sea_stack_0..2: tall layered sea stacks (Lonely Mountains: Downhill), replacing smooth offshore lumps
- coastal_shrub: a dark, rounded shrub cluster that puts true darks into dune views (Firewatch)
- beach_towel, cool_box, beach_ball, tote_bag: a little picnic group (A Short Hike)
- wicker_basket, potted_plant: studio floor clutter (Unpacking)
Run: blender -b --factory-startup -P round3.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise


def facet(o):
    for p in o.data.polygons:
        p.use_smooth = False


def slab(cx, cy, z, sx, sy, h, seed, material, jitter=.06, cuts=2):
    bpy.ops.mesh.primitive_cube_add(size=1, location=(cx, cy, z + h / 2))
    o = bpy.context.active_object
    o.scale = (sx, sy, h); bpy.ops.object.transform_apply(scale=True)
    bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=max(cuts, 3)); bpy.ops.object.mode_set(mode="OBJECT")
    s = Vector((seed * 1.3, seed * .7, seed * 2.1))
    # Round 5b: fractured, not boxy. Strong low-frequency push on every axis, a chiselled top, and a
    # random tilt, so each slab reads as broken rock (LMD) rather than a bevelled box.
    for v in o.data.vertices:
        q = Vector((v.co.x / max(sx, .3), v.co.y / max(sy, .3), v.co.z / max(h, .1)))
        v.co.x += noise.noise(q * 1.7 + s) * jitter * 1.7 * sx
        v.co.y += noise.noise(q * 1.9 - s) * jitter * 1.7 * sy
        v.co.z += noise.noise(q * 2.3 + s * .5) * h * .28
    dec = o.modifiers.new("d", "DECIMATE"); dec.ratio = .35
    bpy.ops.object.modifier_apply(modifier="d")
    o.rotation_euler = (random.uniform(-.12, .12), random.uniform(-.12, .12), random.uniform(-.5, .5)); bpy.ops.object.transform_apply(rotation=True)
    assign(o, material); facet(o)
    return o


def blob(centre, radius, height, seed, material, sub=2, flat_bottom=True):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=sub, radius=1, location=(0, 0, 0))
    o = bpy.context.active_object
    s = Vector((seed, seed * 1.7, seed * .3))
    for v in o.data.vertices:
        p = v.co.copy()
        d = 1 + noise.fractal(p * 1.5 + s, .6, 2.0, 3) * .3
        v.co = Vector((p.x * radius * d, p.y * radius * d, p.z * height * d))
        if flat_bottom and v.co.z < -height * .3:
            v.co.z = -height * .3
    o.location = centre; bpy.ops.object.transform_apply(location=True)
    assign(o, material); facet(o)
    return o


# ---------------------------------------------------------------- far islands (nominal 1 x 1, ~0.45 high)
for k in range(3):
    reset(); random.seed(300 + k)
    parts = []
    z, w, d, ox = 0.0, 1.0, random.uniform(.75, .9), 0.0
    for i in range(3):                                          # low cliff strata stepping back
        h = random.uniform(.05, .075)
        parts.append(slab(ox, 0, z, w, d, h, k * 10 + i, "cliff", jitter=.05))
        z += h * .95; w *= random.uniform(.74, .82); d *= random.uniform(.76, .84); ox += random.uniform(-.05, .05)
    R, H = w * .62, .2                                          # green hill dome over the strata
    parts.append(blob((ox, 0, z - .02), R, H, k * 7, "cap", sub=3))
    for t in range(random.randint(14, 20)):                     # tree clumps following the dome
        a = random.uniform(0, math.tau); r = random.uniform(.1, .85) * R
        top = z - .02 + H * .7 * math.sqrt(max(0, 1 - (r / R) ** 2))
        parts.append(blob((ox + math.cos(a) * r, math.sin(a) * r * d / w, top), random.uniform(.045, .07), random.uniform(.06, .1), k * 50 + t, "trees", sub=1))
    isl = join(parts, "FarIsland")
    export(isl, "far_island_%d" % k, ["cliff", "cap", "trees"])

# ---------------------------------------------------------------- tall sea stacks (nominal 1 wide, ~1.6 high)
for k in range(3):
    reset(); random.seed(400 + k)
    parts = []
    z, w, d, ox, oy = 0.0, 1.0, random.uniform(.8, .95), 0.0, 0.0
    layers = random.randint(5, 7)
    for i in range(layers):
        h = random.uniform(.13, .38)
        parts.append(slab(ox, oy, z, w, d, h, k * 13 + i, "rock", jitter=.09))
        if i < layers - 1 and random.random() < .6:            # broken side column
            parts.append(slab(ox + w * .42 * random.choice((-1, 1)), oy, z, w * .32, d * .45, h * random.uniform(1.2, 2.2), k * 13 + i + 70, "rock"))
        z += h * .93
        w *= random.uniform(.72, 1.04); d *= random.uniform(.74, 1.02)
        ox += random.uniform(-.17, .17); oy += random.uniform(-.13, .13)
    parts.append(blob((ox, oy, z), w * .42, .08, k * 3, "cap"))  # a tuft of green on top
    st = join(parts, "SeaStack")
    export(st, "sea_stack_%d" % k, ["rock", "cap"])

# ---------------------------------------------------------------- coastal shrub (~1.4 m across, 0.8 high)
reset(); random.seed(500)
parts = [blob((0, 0, .3), .6, .45, 1, "leaf_dark")]
for i in range(5):
    a = i / 5 * math.tau + random.uniform(-.3, .3)
    parts.append(blob((math.cos(a) * .45, math.sin(a) * .45, .22), random.uniform(.3, .42), random.uniform(.25, .35), 10 + i, "leaf_dark" if i % 2 else "leaf_lit"))
parts.append(blob((.1, -.05, .62), .32, .22, 20, "leaf_lit"))
sh = join(parts, "Shrub")
export(sh, "coastal_shrub", ["leaf_dark", "leaf_lit"])

# ---------------------------------------------------------------- beach towel with stripes (1.7 x 0.9), slightly rumpled
reset()
bpy.ops.mesh.primitive_grid_add(x_subdivisions=34, y_subdivisions=18, size=1)
t = bpy.context.active_object
t.scale = (1.7, .9, 1); bpy.ops.object.transform_apply(scale=True)
for v in t.data.vertices:
    v.co.z = .012 + noise.noise(v.co * 2.2) * .018 + (max(0, v.co.x - .7) * .12)   # one end lifted over sand
t.data.materials.append(mat("towel_a")); t.data.materials.append(mat("towel_b"))
for p in t.data.polygons:
    p.material_index = int((p.center.x + .85) / .17) % 2
facet(t)
export(t, "beach_towel", ["towel_a", "towel_b"])

# ---------------------------------------------------------------- cool box (0.55 x 0.38 x 0.38)
reset()
parts = [cube("body", (.55, .38, .3), (0, 0, .15), "box_body"), cube("lid", (.57, .4, .08), (0, 0, .34), "box_lid")]
for o in parts: bevel(o, .02, 2, 40)
parts.append(tube_along("handle", [(-.2, 0, .38), (-.18, 0, .45), (.18, 0, .45), (.2, 0, .38)], .012, "box_lid", resolution=6, bevel_resolution=2))
cb = join(parts, "CoolBox"); shade_auto(cb, 40)
export(cb, "cool_box", ["box_body", "box_lid"])

# ---------------------------------------------------------------- beach ball (0.4 m), six gores
reset()
b = uv_sphere("ball", .2, (0, 0, .2), segments=24, rings=14)
cols = ["ball_white", "ball_red", "ball_white", "ball_yellow", "ball_white", "ball_blue"]
for c in dict.fromkeys(cols): b.data.materials.append(mat(c))
names = [m.name for m in b.data.materials]
for p in b.data.polygons:
    a = (math.atan2(p.center.y, p.center.x) + math.pi) / math.tau
    p.material_index = names.index(cols[int(a * 6) % 6])
shade_auto(b, 60)
export(b, "beach_ball", ["ball_white", "ball_red", "ball_yellow", "ball_blue"])

# ---------------------------------------------------------------- tote bag slumped on the sand
reset()
bag = cube("bag", (.42, .16, .36), (0, 0, .18), "bag")
bpy.context.view_layer.objects.active = bag
bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=3); bpy.ops.object.mode_set(mode="OBJECT")
for v in bag.data.vertices:
    v.co.y *= 1 + (.18 - abs(v.co.z - .18)) * 1.5           # bulge
    v.co.x += noise.noise(v.co * 6) * .015
    v.co.z += -abs(v.co.x) * .1 * (v.co.z / .36)            # slump
parts = [bag]
for sx in (-1, 1):
    parts.append(tube_along("strap", [(-.12, sx * .07, .33), (0, sx * .07, .5), (.12, sx * .07, .33)], .012, "bag_strap", resolution=6, bevel_resolution=2))
parts.append(cube("stripe", (.43, .17, .05), (0, 0, .25), "bag_strap"))
tb = join(parts, "Tote"); shade_auto(tb, 45)
export(tb, "tote_bag", ["bag", "bag_strap"])

# ---------------------------------------------------------------- wicker basket (0.5 across, 0.32 high) with rolled cloths
reset()
parts = [cylinder("basket", .25, .3, (0, 0, .15), vertices=20, material="wicker", radius2=.21)]
bpy.context.view_layer.objects.active = parts[0]
bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=4); bpy.ops.object.mode_set(mode="OBJECT")
for v in parts[0].data.vertices:                              # woven bands
    r = math.hypot(v.co.x, v.co.y)
    if r > .05:
        f = 1 + .03 * math.sin(v.co.z * 70)
        v.co.x *= f; v.co.y *= f
parts.append(tube_along("rim", [(math.cos(a) * .25, math.sin(a) * .25, .3) for a in [i / 16 * math.tau for i in range(16)]], .018, "wicker_dark", resolution=4, bevel_resolution=2, closed=True))
for i, (x, y) in enumerate([(-.08, .02), (.07, -.05), (.04, .09)]):
    parts.append(cylinder("roll", .07, .38, (x, y, .3), rotation=(math.radians(70), 0, i * 1.1), vertices=12, material="cloth"))
wb = join(parts, "Basket"); shade_auto(wb, 40)
export(wb, "wicker_basket", ["wicker", "wicker_dark", "cloth"])

# ---------------------------------------------------------------- potted plant (terracotta pot, 0.9 m)
reset(); random.seed(700)
parts = [cylinder("pot", .2, .34, (0, 0, .17), vertices=18, material="terracotta", radius2=.25),
         cylinder("lip", .27, .06, (0, 0, .35), vertices=18, material="terracotta")]
parts.append(cylinder("soil", .23, .02, (0, 0, .37), vertices=18, material="soil"))
for i in range(11):
    a = i / 11 * math.tau + random.uniform(-.2, .2)
    L = random.uniform(.45, .7)
    tip = (math.cos(a) * L * .7, math.sin(a) * L * .7, .38 + L * .9)
    mid = (math.cos(a) * L * .25, math.sin(a) * L * .25, .38 + L * .75)
    leaf = tube_along("leaf", [(0, 0, .37), mid, tip], .035, "leaf_dark" if i % 3 else "leaf_lit", resolution=6, bevel_resolution=1)
    leaf.scale = (1, 1, 1)
    parts.append(leaf)
pp = join(parts, "Plant"); shade_auto(pp, 40)
export(pp, "potted_plant", ["terracotta", "soil", "leaf_dark", "leaf_lit"])
