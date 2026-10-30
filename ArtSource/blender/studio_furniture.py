"""Studio furniture, storytelling props and first-person tools.
Run: blender -b --factory-startup -P studio_furniture.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *


def rounded_box(name, size, loc, radius, material, segments=3):
    o = cube(name, size, loc, material)
    bevel(o, radius, segments, 50)
    return o


# ---------------------------------------------------------------- workbenches
# Pivot = the old tabletop cube's centre (y=1.0 in Unity); its top surface is at +0.09.
def workbench(length, name):
    reset()
    D = 1.8
    parts = []
    top = rounded_box("top", (length, D, 0.07), (0, 0, 0.055), 0.025, "top", 3)
    parts.append(top)
    # Breadboard ends: a slightly proud strip at each end of the top.
    for sx in (-1, 1):
        parts.append(rounded_box("end", (0.09, D + 0.02, 0.075), (sx * (length / 2 - 0.04), 0, 0.055), 0.02, "top", 2))
    # Apron under the top, inset.
    inset = 0.12
    for sy in (-1, 1):
        parts.append(rounded_box("apron", (length - inset * 2, 0.045, 0.14), (0, sy * (D / 2 - inset), -0.055), 0.01, "frame", 2))
    for sx in (-1, 1):
        parts.append(rounded_box("apron", (0.045, D - inset * 2, 0.14), (sx * (length / 2 - inset), 0, -0.055), 0.01, "frame", 2))
    # Tapered square legs.
    for sx in (-1, 1):
        for sy in (-1, 1):
            x, y = sx * (length / 2 - inset - 0.01), sy * (D / 2 - inset - 0.01)
            bpy.ops.mesh.primitive_cone_add(vertices=4, radius1=0.075, radius2=0.06, depth=0.88, location=(x, y, -0.56), rotation=(0, 0, math.radians(45)))
            leg = bpy.context.active_object; leg.name = "leg"
            bpy.ops.object.transform_apply(rotation=True)
            assign(leg, "frame"); bevel(leg, 0.012, 2, 40); parts.append(leg)
    # Lower shelf of slats on stretchers.
    for sx in (-1, 1):
        parts.append(rounded_box("stretcher", (0.05, D - inset * 2, 0.05), (sx * (length / 2 - inset - 0.01), 0, -0.78), 0.01, "frame", 2))
    slats = int((length - inset * 2) / 0.16)
    for i in range(slats):
        x = -(length / 2 - inset) + 0.1 + i * ((length - inset * 2 - 0.2) / max(1, slats - 1))
        parts.append(rounded_box("slat", (0.11, D - inset * 2 - 0.06, 0.025), (x, 0, -0.745), 0.008, "top", 2))
    # Drawer fronts with brass knobs on the player side (-Y in Blender = -Z in Unity).
    drawers = max(1, int(length // 1.4))
    for i in range(drawers):
        x = -(length - inset * 2) / 2 + (i + 0.5) * (length - inset * 2) / drawers
        parts.append(rounded_box("drawer", ((length - inset * 2) / drawers - 0.08, 0.02, 0.1), (x, -(D / 2 - inset) - 0.03, -0.055), 0.008, "top", 2))
        parts.append(uv_sphere("knob", 0.018, (x, -(D / 2 - inset) - 0.055, -0.055), (1, 0.8, 1), 12, 8, "brass"))
    wb = join(parts, name)
    shade_auto(wb, 40)
    export(wb, name, ["top", "frame", "brass"])


workbench(3.0, "workbench_3m")
workbench(6.0, "workbench_6m")

# ---------------------------------------------------------------- toy beach spade
# Unity local space of "ART • gathering tool": Y up the shaft, blade from ~0.08 to 0.32, grip down to -0.23.
# Blender: Z = Unity Y, -Y = Unity -Z (towards the camera). Blade scoop faces the camera.
reset()
parts = []
bpy.ops.mesh.primitive_grid_add(x_subdivisions=14, y_subdivisions=16, size=1)
blade = bpy.context.active_object; blade.name = "blade"
for v in blade.data.vertices:
    u, w = v.co.x, v.co.y + 0.5             # u -0.5..0.5 across, w 0..1 along
    width = 0.11 * (1 - (max(0, w - 0.62) / 0.38) ** 2.2 * 0.85) * (0.82 + 0.18 * min(1, w * 4))
    x = u * 2 * width
    z = 0.085 + w * 0.24
    y = -((u * 2) ** 2) * 0.03 + 0.03     # scoop: edges curl towards the camera
    v.co = Vector((x, y, z))
sol = blade.modifiers.new("t", "SOLIDIFY"); sol.thickness = 0.007; sol.offset = 0
subsurf(blade, 1)
assign(blade, "blade")
parts.append(blade)
neck = cylinder("neck", 0.03, 0.06, (0, 0.03, 0.08), vertices=20, material="blade", radius2=0.02)
parts.append(neck)
shaft = cylinder("shaft", 0.017, 0.27, (0, 0.03, -0.075), vertices=16, material="wood")
parts.append(shaft)
grip = tube_along("grip", [(-0.06, 0.03, -0.21), (-0.05, 0.03, -0.245), (0, 0.03, -0.255), (0.05, 0.03, -0.245), (0.06, 0.03, -0.21)], 0.016, "blade")
parts.append(grip)
cap = uv_sphere("cap", 0.02, (0, 0.03, -0.215), (1, 1, 0.7), 16, 8, "blade")
parts.append(cap)
spade = join(parts, "ToySpade")
shade_auto(spade, 50)
export(spade, "toy_spade", ["blade", "wood"])

# ---------------------------------------------------------------- round sieve (upgrade)
# Unity local: centred at (0, 0.105, 0) of "Wide sieve attachment", in the XY plane facing the camera.
reset()
parts = []
bpy.ops.mesh.primitive_torus_add(major_radius=0.13, minor_radius=0.014, major_segments=48, minor_segments=10, location=(0, 0, 0.105), rotation=(math.radians(90), 0, 0))
ring = bpy.context.active_object; bpy.ops.object.transform_apply(rotation=True); assign(ring, "frame"); parts.append(ring)
wall = cylinder("wall", 0.128, 0.045, (0, 0.022, 0.105), rotation=(math.radians(90), 0, 0), vertices=48, material="frame")
bm = bmesh.new(); bm.from_mesh(wall.data)
caps = [f for f in bm.faces if abs(f.normal.y) > 0.9]
bmesh.ops.delete(bm, geom=caps, context="FACES"); bm.to_mesh(wall.data); bm.free()
sol = wall.modifiers.new("t", "SOLIDIFY"); sol.thickness = 0.006
parts.append(wall)
for i in range(-5, 6):
    x = i * 0.022
    h = math.sqrt(max(0, 0.125 ** 2 - x ** 2))
    if h < 0.01:
        continue
    parts.append(cylinder("wire", 0.0028, h * 2, (x, 0.035, 0.105), vertices=6, material="wire"))
    parts.append(cylinder("wire", 0.0028, h * 2, (0, 0.035, 0.105 + x), rotation=(0, math.radians(90), 0), vertices=6, material="wire"))
handle = tube_along("handle", [(0.13, 0.02, 0.105), (0.19, 0.02, 0.11), (0.23, 0.02, 0.12)], 0.013, "frame")
parts.append(handle)
sieve = join(parts, "RoundSieve")
shade_auto(sieve, 45)
export(sieve, "round_sieve", ["frame", "wire"])

# ---------------------------------------------------------------- glass jar of sea glass
# Pivot: bottom centre. True size ~0.16 m diameter, 0.24 m tall.
reset()
random.seed(9)
parts = []
bpy.ops.mesh.primitive_cylinder_add(vertices=32, radius=0.08, depth=0.22, location=(0, 0, 0.11))
jar = bpy.context.active_object
bm = bmesh.new(); bm.from_mesh(jar.data)
for v in bm.verts:
    if v.co.z > 0.1:   # neck: narrow the top ring
        v.co.x *= 0.78; v.co.y *= 0.78
top = [f for f in bm.faces if f.normal.z > 0.9]
bmesh.ops.delete(bm, geom=top, context="FACES"); bm.to_mesh(jar.data); bm.free()
md = jar.modifiers.new("round", "BEVEL"); md.width = 0.025; md.segments = 4; md.limit_method = "ANGLE"; md.angle_limit = math.radians(50)
sol = jar.modifiers.new("t", "SOLIDIFY"); sol.thickness = 0.005
assign(jar, "jar_glass"); parts.append(jar)
# Contents: a heap of rounded pebbles filling ~60%.
for i in range(26):
    a = random.uniform(0, math.tau); r = random.uniform(0, 0.055)
    z = 0.025 + (i / 26) * 0.11 + random.uniform(-0.01, 0.01)
    parts.append(uv_sphere("pebble", 0.022, (math.cos(a) * r, math.sin(a) * r, z), (random.uniform(1, 1.4), random.uniform(.8, 1.1), 0.55), 10, 6, "contents"))
lid = cylinder("lid", 0.067, 0.04, (0, 0, 0.235), vertices=28, material="cork")
bevel(lid, 0.008, 2, 40); parts.append(lid)
jarobj = join(parts, "GlassJar")
shade_auto(jarobj, 50)
export(jarobj, "glass_jar", ["jar_glass", "contents", "cork"])

# ---------------------------------------------------------------- slatted crate
reset()
parts = []
W, D, H = 0.62, 0.44, 0.34
for sz in range(3):
    z = 0.06 + sz * 0.11
    for sy in (-1, 1):
        parts.append(rounded_box("slat", (W, 0.02, 0.085), (0, sy * D / 2, z), 0.006, "wood", 2))
    for sx in (-1, 1):
        parts.append(rounded_box("slat", (0.02, D, 0.085), (sx * W / 2, 0, z), 0.006, "wood", 2))
for sx in (-1, 1):
    for sy in (-1, 1):
        parts.append(rounded_box("post", (0.045, 0.045, H), (sx * (W / 2 - 0.02), sy * (D / 2 - 0.02), H / 2), 0.008, "wood_dark", 2))
parts.append(rounded_box("floor", (W, D, 0.02), (0, 0, 0.012), 0.005, "wood_dark", 1))
crate = join(parts, "Crate")
shade_auto(crate, 40)
export(crate, "wooden_crate", ["wood", "wood_dark"])

# ---------------------------------------------------------------- stool
reset()
parts = []
seat = cylinder("seat", 0.19, 0.045, (0, 0, 0.66), vertices=32, material="seat")
bevel(seat, 0.015, 3, 40); parts.append(seat)
for i in range(3):
    a = i * math.tau / 3
    parts.append(tube_along("leg", [(math.cos(a) * 0.12, math.sin(a) * 0.12, 0.64), (math.cos(a) * 0.2, math.sin(a) * 0.2, 0.0)], 0.017, "wood"))
parts.append(tube_along("ring", [(math.cos(a) * 0.165, math.sin(a) * 0.165, 0.24) for a in [i * math.tau / 12 for i in range(13)]], 0.009, "wood"))
stool = join(parts, "Stool")
shade_auto(stool, 45)
export(stool, "stool", ["seat", "wood"])

# ---------------------------------------------------------------- woven rug (striped); superseded by round32.py (thicker, fringed)
reset()
L, Wd = 7.0, 3.2
bpy.ops.mesh.primitive_grid_add(x_subdivisions=28, y_subdivisions=2, size=1)
rug = bpy.context.active_object
rug.scale = (L, Wd, 1); bpy.ops.object.transform_apply(scale=True)
for v in rug.data.vertices:
    v.co.z = 0.006
md = rug.modifiers.new("round", "BEVEL"); md.width = 0.15; md.segments = 3; md.affect = "VERTICES"
bpy.context.view_layer.objects.active = rug; bpy.ops.object.modifier_apply(modifier="round")
sol = rug.modifiers.new("t", "SOLIDIFY"); sol.thickness = 0.012; bpy.ops.object.modifier_apply(modifier="t")
for n in ("rug_base", "rug_stripe", "rug_edge"):
    rug.data.materials.append(mat(n))
for p in rug.data.polygons:
    x = abs(p.center.x)
    band = int((x + 0.0) / 0.25)
    if x > L / 2 - 0.35:
        p.material_index = 2
    elif band % 6 in (0, 1) and x > 0.3:
        p.material_index = 1
    else:
        p.material_index = 0
export(rug, "woven_rug", ["rug_base", "rug_stripe", "rug_edge"])

# ---------------------------------------------------------------- gallery bench
reset()
parts = []
for i in range(4):
    parts.append(rounded_box("slat", (2.2, 0.085, 0.035), (0, -0.15 + i * 0.1, 0.45), 0.012, "wood", 2))
for sx in (-1, 1):
    for sy in (-1, 1):
        parts.append(rounded_box("leg", (0.06, 0.06, 0.43), (sx * 0.95, sy * 0.15, 0.215), 0.012, "frame", 2))
    parts.append(rounded_box("rail", (0.05, 0.36, 0.05), (sx * 0.95, 0, 0.405), 0.01, "frame", 2))
parts.append(rounded_box("stretcher", (1.9, 0.04, 0.04), (0, 0, 0.12), 0.01, "frame", 2))
bench = join(parts, "GalleryBench")
shade_auto(bench, 40)
export(bench, "gallery_bench", ["wood", "frame"])

# ---------------------------------------------------------------- easel with design sketch
reset()
parts = []
for sx in (-1, 1):
    parts.append(tube_along("leg", [(sx * 0.32, 0.08, 0.0), (sx * 0.06, 0.0, 1.75)], 0.018, "wood", bevel_resolution=2))
parts.append(tube_along("back", [(0, 0.55, 0.0), (0, 0.02, 1.6)], 0.016, "wood", bevel_resolution=2))
parts.append(rounded_box("ledge", (0.8, 0.07, 0.03), (0, -0.04, 0.82), 0.008, "wood", 2))
parts.append(rounded_box("board", (0.78, 0.02, 0.62), (0, -0.03, 1.15), 0.006, "paper", 2))
# Loose painted swatches pinned to the sketch, in the triptych colours.
random.seed(4)
for i, m in enumerate(["swatch_a", "swatch_b", "swatch_c", "swatch_d"]):
    bpy.ops.mesh.primitive_circle_add(vertices=14, radius=0.05 + random.uniform(-0.01, 0.015), fill_type="NGON", location=(-0.24 + i * 0.16, -0.042, 1.3 - (i % 2) * 0.16), rotation=(math.radians(90), 0, 0))
    s = bpy.context.active_object; bpy.ops.object.transform_apply(rotation=True)
    for v in s.data.vertices:
        v.co.x += random.uniform(-0.006, 0.006); v.co.z += random.uniform(-0.006, 0.006)
    assign(s, m); parts.append(s)
easel = join(parts, "Easel")
shade_auto(easel, 50)
export(easel, "design_easel", ["wood", "paper", "swatch_a", "swatch_b", "swatch_c", "swatch_d"])
