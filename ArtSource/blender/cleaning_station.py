"""Cleaning station hero props: scrub brush, hand sprayer, gooseneck tap and enamel basin.
Run: blender -b -P cleaning_station.py   (writes out/*.json)
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *

random.seed(7)

# ---------------------------------------------------------------- scrub brush
# Pivot: centre of the bristle tips' plane, so it sits on the glass. Long axis = +X.
reset()
parts = []
block = cube("block", (0.17, 0.066, 0.03), (0, 0, 0.037), "wood")
bevel(block, 0.011, 4, 30)
parts.append(block)
# Hand grip: a smooth arch over the block.
grip = tube_along("grip", [(-0.058, 0, 0.05), (-0.04, 0, 0.083), (0, 0, 0.094), (0.04, 0, 0.083), (0.058, 0, 0.05)], 0.0105, "wood_dark")
parts.append(grip)
# Bristle tufts in a staggered grid, slightly splayed.
for ix in range(8):
    for iy in range(3):
        x = -0.07 + ix * 0.02 + (0.01 if iy == 1 else 0)
        if x > 0.075:
            continue
        y = -0.02 + iy * 0.02
        t = cylinder("tuft", 0.0062, 0.026, (x, y, 0.011), vertices=8, material="bristle", radius2=0.0078)
        t.rotation_euler = (random.uniform(-.12, .12), random.uniform(-.12, .12), 0)
        parts.append(t)
brush = join(parts, "ScrubBrush")
shade_auto(brush, 50)
export(brush, "scrub_brush", ["wood", "wood_dark", "bristle"])

# ---------------------------------------------------------------- hand sprayer (washer upgrade)
# Pivot: nozzle tip at origin is awkward to hold; use body centre. Nozzle points +X.
reset()
parts = []
body = cylinder("body", 0.036, 0.13, (0, 0, 0.065), vertices=32, material="body")
bevel(body, 0.01, 3, 40)
parts.append(body)
shoulder = cylinder("shoulder", 0.036, 0.03, (0, 0, 0.145), vertices=32, material="body", radius2=0.018)
parts.append(shoulder)
label = cylinder("label", 0.0372, 0.06, (0, 0, 0.065), vertices=32, material="label")
parts.append(label)
collar = cylinder("collar", 0.02, 0.022, (0, 0, 0.17), vertices=24, material="head")
bevel(collar, 0.003, 2)
parts.append(collar)
head = cube("head", (0.085, 0.034, 0.04), (0.018, 0, 0.198), "head")
bevel(head, 0.012, 4, 30)
parts.append(head)
nozzle = cylinder("nozzle", 0.0075, 0.03, (0.072, 0, 0.205), rotation=(0, math.radians(90), 0), vertices=16, material="nozzle")
parts.append(nozzle)
trigger = tube_along("trigger", [(0.045, 0, 0.18), (0.052, 0, 0.155), (0.044, 0, 0.13)], 0.0075, "head")
parts.append(trigger)
sprayer = join(parts, "HandSprayer")
shade_auto(sprayer, 45)
export(sprayer, "hand_sprayer", ["body", "label", "head", "nozzle"])

# ---------------------------------------------------------------- gooseneck tap (superseded by round36.py: tall riser, fused)
# Pivot: base centre on the tabletop. Spout reaches towards -Y (towards the player).
reset()
parts = []
flange = cylinder("flange", 0.05, 0.014, (0, 0, 0.007), vertices=32, material="brass")
bevel(flange, 0.005, 3, 30)
parts.append(flange)
riser = cylinder("riser", 0.019, 0.2, (0, 0, 0.11), vertices=24, material="brass")
parts.append(riser)
neck = tube_along("neck", [(0, 0, 0.2), (0, 0, 0.3), (0, -0.07, 0.37), (0, -0.17, 0.35), (0, -0.21, 0.29)], 0.0155, "brass", resolution=16)
parts.append(neck)
mouth = cylinder("mouth", 0.019, 0.025, (0, -0.212, 0.278), vertices=24, material="brass_dark")
parts.append(mouth)
hub = cylinder("hub", 0.026, 0.03, (0, 0, 0.17), vertices=24, material="brass")
parts.append(hub)
for a in (0, 90, 180, 270):
    r = math.radians(a)
    spoke = tube_along("spoke", [(0, 0, 0.17), (0.045 * math.cos(r), 0.045 * math.sin(r), 0.175)], 0.0065, "brass")
    knob = uv_sphere("knob", 0.011, (0.05 * math.cos(r), 0.05 * math.sin(r), 0.176), material="porcelain")
    parts += [spoke, knob]
tap = join(parts, "GooseneckTap")
shade_auto(tap, 50)
export(tap, "gooseneck_tap", ["brass", "brass_dark", "porcelain"])

# ---------------------------------------------------------------- enamel basin
# Pivot: bottom centre. Rounded tub, tapered walls, teal rolled rim.
reset()
W, D, H = 1.9, 1.25, 0.19
bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, H / 2))
tub = bpy.context.active_object
tub.scale = (W, D, H)
bpy.ops.object.transform_apply(scale=True)
bm = bmesh.new()
bm.from_mesh(tub.data)
# Taper: shrink the bottom loop.
for v in bm.verts:
    if v.co.z < 0.01:
        v.co.x *= 0.9
        v.co.y *= 0.88
# Remove the top face so it's open.
top = [f for f in bm.faces if f.normal.z > 0.9]
bmesh.ops.delete(bm, geom=top, context="FACES")
bm.to_mesh(tub.data)
bm.free()
# Round the vertical corners strongly, then give the walls thickness and a rolled lip.
md = tub.modifiers.new("corners", "BEVEL")
md.width = 0.2
md.segments = 8
md.limit_method = "ANGLE"
md.angle_limit = math.radians(60)
md.affect = "EDGES"
sol = tub.modifiers.new("wall", "SOLIDIFY")
sol.thickness = 0.03
sol.offset = -1
lip = tub.modifiers.new("lip", "BEVEL")
lip.width = 0.012
lip.segments = 3
lip.limit_method = "ANGLE"
lip.angle_limit = math.radians(50)
bpy.context.view_layer.objects.active = tub
for m in list(tub.modifiers):
    bpy.ops.object.modifier_apply(modifier=m.name)
tub.data.materials.append(mat("enamel"))
tub.data.materials.append(mat("rim"))
for p in tub.data.polygons:
    c = p.center
    p.material_index = 1 if c.z > H - 0.022 else 0
shade_auto(tub, 40)
export(tub, "enamel_basin", ["enamel", "rim"])
