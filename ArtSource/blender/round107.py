"""Round 107 model (after review 103, all three: the spade "a flat red paddle on a stick, with no curvature or toy-like bevel",
"a thick, faceted paddle"; round 95 below did not carry at first-person scale). Same name, frame and slots as round 95, with:
- a much deeper dish (the sides curl well forward, so the scoop shades from lit edges into a hollow middle),
- a moulded rib down the middle of the inside of the scoop and two shorter side ribs, as a moulded plastic toy spade has,
  which catch the light along the dish,
- a fatter rolled rim, and lighter decimation so the dish stays smooth.
Run: blender -b --factory-startup -P round107.py

Round 95 model (after the ninety-first review, all three: the first-person spade is "a flat red pentagon plate on a
stick", "a flat faceted pentagon on a stick", "a flat shield on a stick"). Replaces toy_spade (round36.py) under the
same name, local frame and material slots, still ONE fused surface:
- The blade is a rounded scoop, not a shield: square shoulders running into a full round tip (the old width curve
  came to a point, and the decimation cut it into a pentagon).
- The dish is deeper and the blade is canted back from the shaft, so from the player's view the inside of the scoop
  turns away from the light at its sides and its middle shades into a hollow, instead of one evenly lit flat face.
- A fatter rolled rim gives the outline a thick, moulded-plastic edge.
- Lighter decimation keeps the round tip round.
Run: blender -b --factory-startup -P round95.py
"""
import sys, os, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *


def fuse(objs, name, voxel):
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


# Unity local space of "ART • gathering tool": Y up the shaft, blade from ~0.08 to 0.32, grip down to -0.25.
# Blender: Z = Unity Y, -Y = Unity -Z (towards the camera). The scoop faces the camera.
reset()
parts = []
bpy.ops.mesh.primitive_grid_add(x_subdivisions=28, y_subdivisions=30, size=1)
blade = bpy.context.active_object

W, TIP = 0.112, 0.58   # half-width at the shoulders; where the round tip begins (fraction of the blade length)


def half_width(w):
    # Narrower at the socket, full width by a quarter of the way up, then a full round (elliptical) tip.
    base = W * (0.8 + 0.2 * min(1, w * 4))
    if w <= TIP: return base
    t = (w - TIP) / (1 - TIP)
    return base * math.sqrt(max(0.0, 1 - t * t))


def blade_point(u, w):
    hw = half_width(w)
    x = u * 2 * hw
    z = 0.085 + w * 0.24
    # A deep dish across the blade (the sides curl towards the camera), deepest in the middle of its length, and the
    # whole blade canted back from the shaft so the scoop opens towards the player.
    across = (u * 2) ** 2
    y = -across * 0.105 * (0.55 + 0.45 * math.sin(min(1, w * 1.15) * math.pi * .5)) + 0.04 + w * 0.05 - 0.016 * math.sin(w * math.pi)
    # Turned about the shaft (x = 0, y = 0.03), so the player sees the scoop at an angle and its curled side in depth, not a
    # flat face square to the camera. The shaft and grip stay put, so the glove still holds it.
    a = math.radians(TWIST); dx, dy = x, y - 0.03
    return Vector((dx * math.cos(a) - dy * math.sin(a), 0.03 + dx * math.sin(a) + dy * math.cos(a), z))


TWIST = 24


for v in blade.data.vertices:
    v.co = blade_point(v.co.x, v.co.y + 0.5)
sol = blade.modifiers.new("t", "SOLIDIFY"); sol.thickness = 0.012; sol.offset = 0
bpy.context.view_layer.objects.active = blade; bpy.ops.object.modifier_apply(modifier="t")
assign(blade, "blade"); parts.append(blade)
# Rolled rim around both long edges and the round tip, following the outline closely.
N = 40
rim = [blade_point(-.5, k / N) for k in range(0, N + 1)]
rim += [blade_point(-.5 + k / 12, 1.0) for k in range(1, 12)]
rim += [blade_point(.5, 1 - k / N) for k in range(0, N + 1)]
parts.append(tube_along("rim", [tuple(p) for p in rim], 0.013, "blade", resolution=4, bevel_resolution=4))
# Moulded ribs on the inside of the scoop (the side facing the player), standing a little proud of the dish.
def inner(u, w, lift):
    p = blade_point(u, w); q = blade_point(u, w + .01); s = blade_point(u + .02, w)
    nrm = (q - p).cross(s - p).normalized()
    if nrm.y > 0: nrm = -nrm   # towards the camera (-Y), the inside of the scoop
    return tuple(p + nrm * lift)
parts.append(tube_along("rib", [inner(0, k / 20, .006) for k in range(1, 15)], 0.0065, "blade", resolution=4, bevel_resolution=4))
for u in (-.24, .24): parts.append(tube_along("rib_side", [inner(u * (0.6 + 0.4 * k / 9), k / 18, .005) for k in range(1, 9)], 0.005, "blade", resolution=4, bevel_resolution=4))
# Foot step across the top of the blade.
parts.append(tube_along("step", [tuple(blade_point(-.5, 0) + Vector((0, 0.004, 0.0))), tuple(blade_point(.5, 0) + Vector((0, 0.004, 0.0)))], 0.013, "blade", resolution=4, bevel_resolution=4))
# Socket collar flowing from the blade into the shaft.
parts.append(cylinder("collar", 0.022, 0.07, (0, 0.03, 0.06), vertices=24, material="blade", radius2=0.034))
# Wooden shaft.
parts.append(cylinder("shaft", 0.0165, 0.25, (0, 0.03, -0.09), vertices=20, material="wood"))
# D-grip: two arms out from the shaft end, joined by a fat crossbar.
parts.append(tube_along("grip", [(0, 0.03, -0.2), (-0.045, 0.03, -0.235), (-0.052, 0.03, -0.27), (0, 0.03, -0.278), (0.052, 0.03, -0.27), (0.045, 0.03, -0.235), (0, 0.03, -0.2)], 0.012, "blade", resolution=24, bevel_resolution=4))
parts.append(cylinder("crossbar", 0.016, 0.1, (0, 0.03, -0.272), rotation=(0, math.radians(90), 0), vertices=20, material="blade"))
spade = fuse(parts, "ToySpade", 0.0022)
spade.data.materials.clear()
for n in ("blade", "wood"): spade.data.materials.append(mat(n))
for p in spade.data.polygons:
    z = p.center.z
    p.material_index = 1 if (-0.205 < z < 0.03 and math.hypot(p.center.x, p.center.y - 0.03) < 0.025) else 0
    p.use_smooth = True
reduce(spade, 0.45, lambda c: abs(c.z + 0.205) < .006 or abs(c.z - 0.03) < .006)
export(spade, "toy_spade", ["blade", "wood"])
