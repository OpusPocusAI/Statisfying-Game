"""Shoreline rubbish: recognisable at 5 m, lightly worn, never gritty.
Each model rests on z=0 as it lies on the sand; the importer pass aligns it to the old reveal height.
Submesh order is always [primary, accent, metal] to match the existing renderer material slots.
Run: blender -b --factory-startup -P rubbish.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise

SLOTS = ["primary", "accent", "metal"]


def lathe(name, profile, segments, material, axis_rot=(0, 0, 0), loc=(0, 0, 0)):
    """profile: list of (radius, height). Builds a closed surface of revolution around Z."""
    bm = bmesh.new()
    rings = []
    for r, h in profile:
        ring = [bm.verts.new((r * math.cos(a), r * math.sin(a), h)) for a in [i * math.tau / segments for i in range(segments)]]
        rings.append(ring)
    for a, b in zip(rings, rings[1:]):
        for i in range(segments):
            bm.faces.new((a[i], a[(i + 1) % segments], b[(i + 1) % segments], b[i]))
    bm.faces.new(list(reversed(rings[0])))
    bm.faces.new(rings[-1])
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(o)
    o.rotation_euler = axis_rot; o.location = loc
    bpy.context.view_layer.objects.active = o; o.select_set(True)
    bpy.ops.object.transform_apply(location=True, rotation=True)
    assign(o, material)
    return o


def rest(obj):
    mn = min(v.co.z for v in obj.data.vertices)
    for v in obj.data.vertices:
        v.co.z -= mn


def dent(obj, centre, radius, depth):
    c = Vector(centre)
    for v in obj.data.vertices:
        d = (v.co - c).length
        if d < radius:
            v.co -= (v.co - Vector((0, 0, v.co.z))).normalized() * depth * (1 - d / radius) ** 2 if (v.co.xy.length > 1e-4) else Vector()


# ---------------------------------------------------------------- 0: drinks can, lying on its side
reset()
parts = []
prof = [(0.0, -0.06), (0.026, -0.061), (0.031, -0.055), (0.033, -0.045), (0.033, 0.045), (0.03, 0.054), (0.025, 0.06), (0.0, 0.061)]
body = lathe("can", prof, 28, "metal")
label = lathe("label", [(0.0334, -0.034), (0.0334, 0.036)], 28, "accent")
stripe = lathe("stripe", [(0.0336, -0.01), (0.0336, 0.006)], 28, "primary")
tab = cube("tab", (0.018, 0.01, 0.002), (0.008, 0, 0.0615), "metal")
can = join([body, label, stripe, tab], "Can")
dent(can, (0.03, 0.0, 0.01), 0.028, 0.007)
can.rotation_euler = (math.radians(90), 0, math.radians(12))
bpy.ops.object.transform_apply(rotation=True)
rest(can)
shade_auto(can, 50)
export(can, "rubbish_can", SLOTS)

# ---------------------------------------------------------------- 1: plastic bottle, lying down
reset()
prof = [(0.0, -0.11), (0.03, -0.112), (0.036, -0.1), (0.036, 0.0), (0.034, 0.02), (0.022, 0.06), (0.013, 0.08), (0.013, 0.095), (0.0, 0.096)]
bottle = lathe("bottle", prof, 24, "primary")
# Ribs: subtle rings on the lower body.
for v in bottle.data.vertices:
    if -0.095 < v.co.z < -0.02 and v.co.xy.length > 0.03:
        k = 1 - 0.05 * (0.5 + 0.5 * math.cos(v.co.z * 260))
        v.co.x *= k; v.co.y *= k
label = lathe("label", [(0.0366, -0.07), (0.0366, -0.025)], 24, "accent")
cap = lathe("cap", [(0.0, 0.094), (0.016, 0.094), (0.016, 0.114), (0.0, 0.115)], 18, "accent")
b = join([bottle, label, cap], "Bottle")
b.rotation_euler = (math.radians(88), 0, math.radians(-20))
bpy.ops.object.transform_apply(rotation=True)
rest(b)
shade_auto(b, 45)
export(b, "rubbish_bottle", SLOTS)

# ---------------------------------------------------------------- 2: crushed can
reset()
prof = [(0.0, -0.06), (0.03, -0.06), (0.033, -0.05), (0.033, 0.05), (0.028, 0.06), (0.0, 0.061)]
c = lathe("crush", prof, 28, "metal")
bpy.context.view_layer.objects.active = c
bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=2); bpy.ops.object.mode_set(mode="OBJECT")
for v in c.data.vertices:
    z = v.co.z
    squash = 0.45 + 0.55 * (abs(z) / 0.06) ** 1.5                 # pinched waist
    fold = 1 + noise.noise(v.co * 60) * 0.25
    v.co.x *= squash * fold
    v.co.y *= 0.55 * fold
    v.co.z *= 0.8
label = lathe("band", [(0.0, 0), (0.0, 0)], 3, "accent")       # placeholder removed below
bpy.data.objects.remove(label)
# Paint the middle band as the accent colour.
c.data.materials.append(mat("accent"))
for p in c.data.polygons:
    if abs(p.center.z) < 0.025:
        p.material_index = 1
c.rotation_euler = (math.radians(80), math.radians(15), 0)
bpy.ops.object.transform_apply(rotation=True)
rest(c)
shade_auto(c, 30)
export(c, "rubbish_crushed", SLOTS)

# ---------------------------------------------------------------- 3: bottle cap
reset()
cap = lathe("cap", [(0.0, 0.0), (0.0145, 0.0), (0.0145, 0.006), (0.0128, 0.0068), (0.0, 0.0068)], 42, "metal")
for v in cap.data.vertices:
    if v.co.xy.length > 0.0135:
        a = math.atan2(v.co.y, v.co.x)
        k = 1 + 0.08 * math.cos(a * 21)
        v.co.x *= k; v.co.y *= k
top = cylinder("top", 0.0118, 0.0006, (0, 0, 0.0071), vertices=32, material="accent")
cap = join([cap, top], "Cap")
cap.scale = (2.2, 2.2, 2.2)                                       # game scale: readable from standing height
bpy.ops.object.transform_apply(scale=True)
cap.rotation_euler = (math.radians(8), 0, 0)
bpy.ops.object.transform_apply(rotation=True)
rest(cap)
shade_auto(cap, 40)
export(cap, "rubbish_cap", SLOTS)

# ---------------------------------------------------------------- 4: driftwood branch
reset()
random.seed(12)
main = tube_along("trunk", [(-0.36, 0, 0.04), (-0.15, 0.02, 0.05), (0.08, -0.02, 0.045), (0.34, 0.03, 0.03)], 0.042, "primary", resolution=10, bevel_resolution=3)
for v in main.data.vertices:
    t = (v.co.x + 0.36) / 0.7
    k = 1.15 - 0.55 * t + noise.noise(v.co * 18) * 0.12
    c0 = Vector((v.co.x, 0, 0.045))
    off = v.co - c0; off.x = 0
    v.co = c0 + off * k
branch = tube_along("branch", [(-0.05, 0.0, 0.05), (0.05, 0.1, 0.06), (0.12, 0.17, 0.04)], 0.018, "primary", resolution=8, bevel_resolution=2)
stub = tube_along("stub", [(0.15, 0.0, 0.05), (0.19, -0.07, 0.07)], 0.014, "primary", resolution=6, bevel_resolution=2)
wood = join([main, branch, stub], "Driftwood")
rest(wood)
shade_auto(wood, 35)
export(wood, "rubbish_driftwood", SLOTS)
