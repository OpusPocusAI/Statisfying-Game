"""Round 36 models (after the thirty-second review and the owner's note that the hand looked like "different
shapes put together"); both replace earlier models under the same names, built as ONE fused surface each:
- toy_spade (was studio_furniture.py): a chunky toy beach spade: a dished blade with a rolled rim and a foot step,
  a socket collar, a wooden shaft and a D-grip, voxel-fused and coloured by region (plastic / wood). Reviews: "the
  shovel blade is a flat paddle", "a polygon on a stick".
- gooseneck_tap (was cleaning_station.py): the riser is twice as tall, so the base flange and the cross handle
  stand above the basin rim instead of hiding behind it (reviews: "the tap is a bare bent pipe"); fused, with the
  porcelain knobs and the darker mouth as regions.
Same local frames and material slots as the models they replace.
Run: blender -b --factory-startup -P round36.py
"""
import sys, os, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *


def fuse(objs, name, voxel, ratio, keep=None):
    o = join(objs, name)
    bpy.context.view_layer.objects.active = o
    rm = o.modifiers.new("fuse", "REMESH"); rm.mode = "VOXEL"; rm.voxel_size = voxel
    bpy.ops.object.modifier_apply(modifier="fuse")
    sm = o.modifiers.new("soften", "CORRECTIVE_SMOOTH"); sm.factor = .5; sm.iterations = 5
    bpy.ops.object.modifier_apply(modifier="soften")
    return o


def reduce(o, ratio, edge_test):
    g = o.vertex_groups.new(name="edges")
    for v in o.data.vertices:
        if edge_test(v.co): g.add([v.index], 1.0, "REPLACE")
    d = o.modifiers.new("reduce", "DECIMATE"); d.ratio = ratio
    d.vertex_group = "edges"; d.invert_vertex_group = True; d.vertex_group_factor = 10
    bpy.ops.object.modifier_apply(modifier="reduce")


# ---------------------------------------------------------------- toy spade
# Unity local space of "ART • gathering tool": Y up the shaft, blade from ~0.08 to 0.32, grip down to -0.25.
# Blender: Z = Unity Y, -Y = Unity -Z (towards the camera). The scoop faces the camera.
reset()
parts = []
bpy.ops.mesh.primitive_grid_add(x_subdivisions=20, y_subdivisions=22, size=1)
blade = bpy.context.active_object


def blade_point(u, w):
    width = 0.115 * (1 - (max(0, w - 0.6) / 0.4) ** 2.0 * 0.8) * (0.84 + 0.16 * min(1, w * 4))
    x = u * 2 * width
    z = 0.085 + w * 0.24
    y = -((u * 2) ** 2) * 0.045 + 0.04 - 0.012 * math.sin(w * math.pi)   # a real dish, deepest mid-blade
    return Vector((x, y, z))


for v in blade.data.vertices:
    v.co = blade_point(v.co.x, v.co.y + 0.5)
sol = blade.modifiers.new("t", "SOLIDIFY"); sol.thickness = 0.011; sol.offset = 0
bpy.context.view_layer.objects.active = blade; bpy.ops.object.modifier_apply(modifier="t")
assign(blade, "blade"); parts.append(blade)
# Rolled rim around both long edges and the rounded tip.
rim = [blade_point(-.5, w / 20) for w in range(0, 21)] + [blade_point(u / 10 - .5, 1.0) for u in range(1, 10)] + [blade_point(.5, 1 - w / 20) for w in range(0, 21)]
parts.append(tube_along("rim", [tuple(p) for p in rim], 0.0085, "blade", resolution=4, bevel_resolution=4))
# Foot step across the top of the blade.
parts.append(tube_along("step", [tuple(blade_point(-.5, 0) + Vector((0, 0.004, 0.0))), tuple(blade_point(.5, 0) + Vector((0, 0.004, 0.0)))], 0.012, "blade", resolution=4, bevel_resolution=4))
# Socket collar flowing from the blade into the shaft.
parts.append(cylinder("collar", 0.022, 0.07, (0, 0.03, 0.06), vertices=24, material="blade", radius2=0.034))
# Wooden shaft.
parts.append(cylinder("shaft", 0.0165, 0.25, (0, 0.03, -0.09), vertices=20, material="wood"))
# D-grip: two arms out from the shaft end, joined by a fat crossbar.
parts.append(tube_along("grip", [(0, 0.03, -0.2), (-0.045, 0.03, -0.235), (-0.052, 0.03, -0.27), (0, 0.03, -0.278), (0.052, 0.03, -0.27), (0.045, 0.03, -0.235), (0, 0.03, -0.2)], 0.012, "blade", resolution=24, bevel_resolution=4))
parts.append(cylinder("crossbar", 0.016, 0.1, (0, 0.03, -0.272), rotation=(0, math.radians(90), 0), vertices=20, material="blade"))
spade = fuse(parts, "ToySpade", 0.0022, 0.3)
spade.data.materials.clear()
for n in ("blade", "wood"): spade.data.materials.append(mat(n))
for p in spade.data.polygons:
    z = p.center.z
    p.material_index = 1 if (-0.205 < z < 0.03 and math.hypot(p.center.x, p.center.y - 0.03) < 0.025) else 0
    p.use_smooth = True
reduce(spade, 0.12, lambda c: abs(c.z + 0.205) < .006 or abs(c.z - 0.03) < .006)
export(spade, "toy_spade", ["blade", "wood"])

# ---------------------------------------------------------------- gooseneck tap with a tall riser
# Pivot: base centre on the tabletop. Spout reaches towards -Y (towards the player).
reset()
parts = []
H = 0.42                                                   # riser height (was 0.2)
flange = cylinder("flange", 0.055, 0.02, (0, 0, 0.01), vertices=32, material="brass"); parts.append(flange)
parts.append(cylinder("riser", 0.02, H, (0, 0, H / 2), vertices=24, material="brass", radius2=0.018))
parts.append(tube_along("neck", [(0, 0, H - .01), (0, 0, H + .1), (0, -0.07, H + .17), (0, -0.17, H + .15), (0, -0.21, H + .09)], 0.0165, "brass", resolution=16, bevel_resolution=4))
mouth_at = Vector((0, -0.212, H + 0.075))
parts.append(cylinder("mouth", 0.021, 0.03, tuple(mouth_at), vertices=24, material="brass"))
hub_z = H - 0.08
parts.append(cylinder("hub", 0.028, 0.034, (0, 0, hub_z), vertices=24, material="brass"))
knobs = []
for a in (0, 90, 180, 270):
    r = math.radians(a)
    parts.append(tube_along("spoke", [(0, 0, hub_z), (0.05 * math.cos(r), 0.05 * math.sin(r), hub_z + .006)], 0.007, "brass", resolution=4, bevel_resolution=3))
    k = Vector((0.056 * math.cos(r), 0.056 * math.sin(r), hub_z + .007))
    parts.append(uv_sphere("knob", 0.013, tuple(k), material="brass")); knobs.append(k)
tap = fuse(parts, "GooseneckTap", 0.002, 0.3)
tap.data.materials.clear()
for n in ("brass", "brass_dark", "porcelain"): tap.data.materials.append(mat(n))
for p in tap.data.polygons:
    c = Vector(p.center)
    if any((c - k).length < 0.016 for k in knobs): p.material_index = 2
    elif (c - mouth_at).length < 0.024 and c.z < mouth_at.z + .004: p.material_index = 1
    else: p.material_index = 0
    p.use_smooth = True
reduce(tap, 0.1, lambda c: any(abs((c - k).length - 0.016) < .004 for k in knobs))
export(tap, "gooseneck_tap", ["brass", "brass_dark", "porcelain"])
