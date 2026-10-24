"""Studio props: lined sorting tray and lead came profile.
Run: blender -b --factory-startup -P studio_props.py
"""
import sys, os, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *

# ---------------------------------------------------------------- sorting tray
# True size 0.76 x 1.20 x 0.12 m, pivot at the box centre (matches the old cube transform).
reset()
W, D, H = 0.76, 1.20, 0.12
wall, floor_t = 0.035, 0.022
outer = cube("outer", (W, D, H), (0, 0, 0))
cavity = cube("cavity", (W - wall * 2, D - wall * 2, H), (0, 0, -H / 2 + floor_t + H / 2))
for o, r in ((outer, 0.045), (cavity, 0.03)):
    md = o.modifiers.new("round", "BEVEL"); md.width = r; md.segments = 5
    md.limit_method = "ANGLE"; md.angle_limit = math.radians(60)
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.modifier_apply(modifier="round")
boolean = outer.modifiers.new("cut", "BOOLEAN"); boolean.operation = "DIFFERENCE"; boolean.object = cavity
bpy.context.view_layer.objects.active = outer
bpy.ops.object.modifier_apply(modifier="cut")
bpy.data.objects.remove(cavity)
# Soften every remaining edge a touch (rim lip, outer base).
md = outer.modifiers.new("soft", "BEVEL"); md.width = 0.006; md.segments = 2
md.limit_method = "ANGLE"; md.angle_limit = math.radians(50)
bpy.ops.object.modifier_apply(modifier="soft")
outer.data.materials.clear()
outer.data.materials.append(mat("timber"))
outer.data.materials.append(mat("lining"))
for p in outer.data.polygons:
    # The inside floor is the felt lining.
    inside = abs(p.center.x) < W / 2 - wall + 0.002 and abs(p.center.y) < D / 2 - wall + 0.002
    p.material_index = 1 if (inside and p.normal.z > 0.9 and p.center.z < -H / 2 + floor_t + 0.004) else 0
# Lining also covers the gently curved floor-to-wall fillet.
for p in outer.data.polygons:
    if abs(p.center.x) < W / 2 - wall + 0.002 and abs(p.center.y) < D / 2 - wall + 0.002 and p.center.z < -H / 2 + floor_t + 0.02 and p.normal.z > 0.3:
        p.material_index = 1
shade_auto(outer, 35)
export(outer, "sorting_tray", ["timber", "lining"])
