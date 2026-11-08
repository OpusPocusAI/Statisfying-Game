"""Round 77 (review 73, all three, and every review since 66: the truck bonnet and fenders "lumpy", "crumpled", "mottled",
"melted rather than softly bevelled"; two: "possibly bad normals"). truck_shell (round46.py) was voxel-fused and then
decimated to 17% of its triangles, which left long irregular triangles over the big curved bonnet and arches; their
averaged normals wobbled, so the smooth ivory enamel shaded in blotches and creases. Rebuilt here with:
- a finer voxel fuse and twice the smoothing passes, so the bonnet crown, arches and bumpers are clean curves;
- a lighter decimation (30%, was 17%);
- normals transferred from the smooth, full-resolution surface onto the decimated mesh (custom split normals), so the
  shading follows the true curve, not the triangles.
Same name, origin, slots and regions as round46.py, so it updates the truck in place.
Run: blender -b --factory-startup -P round77.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise


def remesh(o, voxel, smooth=(.5, 6)):
    bpy.context.view_layer.objects.active = o
    rm = o.modifiers.new("fuse", "REMESH"); rm.mode = "VOXEL"; rm.voxel_size = voxel
    bpy.ops.object.modifier_apply(modifier="fuse")
    if smooth:
        sm = o.modifiers.new("soften", "CORRECTIVE_SMOOTH"); sm.factor, sm.iterations = smooth
        bpy.ops.object.modifier_apply(modifier="soften")


def reduce(o, ratio, protect):
    g = o.vertex_groups.new(name="keep")
    for v in o.data.vertices:
        if protect(v.co): g.add([v.index], 1.0, "REPLACE")
    d = o.modifiers.new("reduce", "DECIMATE"); d.ratio = ratio
    d.vertex_group = "keep"; d.invert_vertex_group = True; d.vertex_group_factor = 10
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.modifier_apply(modifier="reduce")


def rounded_box(name, x0, x1, y0, y1, z0, z1, r):
    o = cube(name, (x1 - x0, y1 - y0, z1 - z0), ((x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2))
    bevel(o, r, segments=4, angle=80)
    bpy.context.view_layer.objects.active = o
    for m in list(o.modifiers): bpy.ops.object.modifier_apply(modifier=m.name)
    return o


# ================================================================ truck shell
reset()
parts = []
# Lower body (the chassis), cab sides with the doors, and the bonnet, overlapping so the voxel merge fuses them.
parts.append(rounded_box("lower", -1.0, 1.0, -2.1, 2.05, .45, .88, .1))
for (xa, xb) in ((.79, 1.0), (-1.0, -.79)):
    parts.append(rounded_box("cabside", xa, xb, -.2, 1.2, .78, 1.46, .05))
# Bonnet: a rounded mass with a crowned top.
bon = rounded_box("bonnet", -1.0, 1.0, .78, 1.98, .78, 1.38, .16)
for v in bon.data.vertices:
    if v.co.z > 1.2: v.co.z += .05 * (1 - (v.co.x / 1.0) ** 2)
parts.append(bon)
# Wheel arches flaring out of the body over each wheel.
wheels = [(sx * 1.04, wy, .45) for sx in (-1, 1) for wy in (-1.4, 1.4)]
for (wx, wy, wz) in wheels:
    pts = []
    for k in range(13):
        a = math.radians(8 + k * (164 / 12))
        pts.append((wx * .99, wy + math.cos(a) * .55, wz + math.sin(a) * .55))
    parts.append(tube_along("arch", pts, .1, None, resolution=16, bevel_resolution=4))
# Rounded bumpers front and rear, tucked into the body ends.
parts.append(tube_along("bumperF", [(-1.08, 2.1, .57), (-.6, 2.16, .57), (.6, 2.16, .57), (1.08, 2.1, .57)], .1, None, resolution=16, bevel_resolution=4))
parts.append(tube_along("bumperR", [(-1.02, -2.12, .55), (-.6, -2.16, .55), (.6, -2.16, .55), (1.02, -2.12, .55)], .085, None, resolution=16, bevel_resolution=4))
shell = join(parts, "TruckShell")
bpy.context.view_layer.objects.active = shell; shell.select_set(True)
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)   # regions below test world-frame positions
remesh(shell, .018, (.5, 14))   # round 77: finer and smoother (was .022, 6 passes)
# Wheel wells: cut a cylinder out of the outer body around each wheel so the wheels sit inside the arches.
for (wx, wy, wz) in wheels:
    bpy.ops.mesh.primitive_cylinder_add(vertices=40, radius=.47, depth=.7, location=(wx + (.1 if wx > 0 else -.1), wy, wz), rotation=(0, math.radians(90), 0))
    cut = bpy.context.active_object
    b = shell.modifiers.new("well", "BOOLEAN"); b.operation = "DIFFERENCE"; b.object = cut; b.solver = "EXACT"
    bpy.context.view_layer.objects.active = shell; bpy.ops.object.modifier_apply(modifier="well")
    bpy.data.objects.remove(cut)

shell.data.materials.clear()
for n in ("body", "door", "ivory", "seam"): shell.data.materials.append(mat(n))


def region(c):
    x, y, z = c.x, c.y, c.z
    ax = abs(x)
    # Ivory: bonnet, bumpers, the belt line along the cab sides, and the arches around each wheel.
    if y > .8 and z > .84: return 2
    if abs(y) > 2.04 and z < .74: return 2
    if ax > .9 and -.2 < y < 1.2 and 1.36 < z < 1.44: return 2
    for (wx, wy, wz) in wheels:
        d = math.hypot(y - wy, z - wz)
        if .44 < d < .68 and z > wz - .05 and ax > 1.005: return 2   # only the flare standing proud of the body side
    # Door seams: a thin dark outline on each cab side.
    if ax > .9 and .8 < z < 1.36 and (abs(y + .14) < .018 or abs(y - 1.14) < .018 or abs(z - .84) < .014): return 3
    # Doors: the cab sides.
    if ax > .9 and -.14 < y < 1.14 and .84 < z < 1.36: return 1
    return 0


for p in shell.data.polygons:
    p.material_index = region(Vector(p.center))
    p.use_smooth = True


def near_region_edge(c):
    ax = abs(c.x)
    if ax > .85 and (abs(c.y + .14) < .05 or abs(c.y - 1.14) < .05 or abs(c.z - .84) < .05 or abs(c.z - 1.4) < .06): return True
    if c.y > .74 and c.z > .8 and abs(c.y - .8) < .05: return True
    for (wx, wy, wz) in wheels:
        d = math.hypot(c.y - wy, c.z - wz)
        if abs(d - .44) < .05 or abs(d - .68) < .05: return True
    return False


# Keep the smooth full-resolution surface, decimate, then carry its normals over.
hi = shell.copy(); hi.data = shell.data.copy(); bpy.context.collection.objects.link(hi)
reduce(shell, .3, near_region_edge)
dt = shell.modifiers.new("smooth normals", "DATA_TRANSFER"); dt.object = hi
dt.use_loop_data = True; dt.data_types_loops = {"CUSTOM_NORMAL"}; dt.loop_mapping = "POLYINTERP_NEAREST"
bpy.context.view_layer.objects.active = shell; bpy.ops.object.modifier_apply(modifier=dt.name)
bpy.data.objects.remove(hi)
export(shell, "truck_shell", ["body", "door", "ivory", "seam"])
