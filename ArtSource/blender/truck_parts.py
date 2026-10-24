"""Truck refinements at true size (pivot = centre, matching the cubes they replace).
Run: blender -b --factory-startup -P truck_parts.py
"""
import sys, os, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *


def rounded(name, size, radius, material, segs=4):
    o = cube(name, size, (0, 0, 0), material)
    bevel(o, radius, segs, 50)
    return o


# Chassis: Blender (x, y, z) = Unity (x, z, y). Unity size (2.0, 0.4, 4.2).
reset()
c = rounded("chassis", (2.0, 4.2, .4), .12, "paint", 5)
shade_auto(c, 40)
export(c, "truck_chassis", ["paint"])

# Open cargo tub: Unity size (0.94, 0.5, 1.6), with a thick rolled rim.
reset()
W, D, H, t = .94, 1.6, .5, .045
outer = rounded("outer", (W, D, H), .07, "paint", 4)
inner = cube("inner", (W - 2 * t, D - 2 * t, H), (0, 0, t + .001), None)
bevel(inner, .05, 3, 50)
bpy.context.view_layer.objects.active = inner
bpy.ops.object.modifier_apply(modifier="Bevel")
boolean = outer.modifiers.new("cut", "BOOLEAN"); boolean.operation = "DIFFERENCE"; boolean.object = inner
bpy.context.view_layer.objects.active = outer
for md in list(outer.modifiers):
    bpy.ops.object.modifier_apply(modifier=md.name)
bpy.data.objects.remove(inner)
md = outer.modifiers.new("soft", "BEVEL"); md.width = .012; md.segments = 2; md.limit_method = "ANGLE"; md.angle_limit = math.radians(50)
bpy.ops.object.modifier_apply(modifier="soft")
outer.data.materials.clear(); outer.data.materials.append(mat("paint"))
shade_auto(outer, 40)
export(outer, "truck_cargo_tub", ["paint"])

# Bench seat cushion (0.7, 0.2, 0.7) and backrest (0.7, 0.7, 0.18): soft rounded cushions.
reset()
s = rounded("seat", (.7, .7, .2), .07, "cushion", 5)
shade_auto(s, 50)
export(s, "truck_seat", ["cushion"])
reset()
b = rounded("back", (.7, .18, .7), .07, "cushion", 5)
shade_auto(b, 50)
export(b, "truck_seat_back", ["cushion"])
