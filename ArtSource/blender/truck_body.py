"""Truck body with the canopy frame fused in (owner, November 16: close the known gaps; second independent review, all three: the
canopy posts read as "bone-like tubes" pushed into the body, and a "torn flap" sticks out behind each rear wheel).

Built from round77.py's shell (same parts, regions, fuse and normal transfer), with three changes:
- The rear-wheel flap is gone. The arch tubes ran round into the rear bumper's ends, and the wheel-well cut left thin slivers of
  both behind each wheel. The arches now stop short of the bumper (20-160 degrees, was 8-172), and the rear bumper ends at
  x +-0.94 (was 1.02), inside the body side.
- The canopy frame is part of the same surface: four tapered pillars (rounded, wider front to back than across) rise from the
  cab rim out of broad fillets, and bend into a roof rail with cross bars, all fused with the body in one voxel merge, so
  posts grow out of the bodywork instead of standing on it. They take a fifth colour region, "frame" (the chalk paint).
- Round 77's fuse, smoothing and reduce are kept (a finer fuse broke the colour edges into ragged teeth), so the pillars are
  chunky toy sections that survive the smoothing, and the ivory paint at the rear stays on the bumper, not the body corners.
Same name, origin and first four slots as round77.py, so the truck updates in place (TruckFusionPass adds the frame slot).
Run: blender -b --factory-startup -P truck_body.py
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
def arch_tube(pts, radii, sides=16):
    bm = bmesh.new(); rings = []; pts = [Vector(p) for p in pts]
    for i, p in enumerate(pts):
        t = (pts[min(i + 1, len(pts) - 1)] - pts[max(i - 1, 0)]).normalized()
        u = t.cross(Vector((1, 0, 0))).normalized(); v = t.cross(u)
        rings.append([bm.verts.new(p + (u * math.cos(k / sides * math.tau) + v * math.sin(k / sides * math.tau)) * radii[i]) for k in range(sides)])
    for a, b in zip(rings, rings[1:]):
        for k in range(sides): bm.faces.new((a[k], a[(k + 1) % sides], b[(k + 1) % sides], b[k]))
    for ring, flip in ((rings[0], True), (rings[-1], False)):
        c = bm.verts.new(sum((q.co for q in ring), Vector()) / sides)
        for k in range(sides): f = (ring[k], ring[(k + 1) % sides], c); bm.faces.new(f[::-1] if flip else f)
    me = bpy.data.meshes.new("arch"); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new("arch", me); bpy.context.collection.objects.link(o)
    return o


# Arches: the flare is full over the wheel and tapers into the body side at both ends (truck fusion: a full-thickness end
# stood 10 cm proud behind each rear wheel and the wheel-well cut sliced it into a thin "torn flap").
for (wx, wy, wz) in wheels:
    pts, radii = [], []
    for k in range(25):
        t = k / 24; a = math.radians(14 + t * 152)
        swell = math.sin(math.pi * t) ** .6
        side = 1 if wx > 0 else -1
        pts.append((wx * .99 - side * .07 * (1 - swell), wy + math.cos(a) * .55, wz + math.sin(a) * .55))
        radii.append(.03 + .07 * swell)
    parts.append(arch_tube(pts, radii))
# Rounded bumpers front and rear, tucked into the body ends.
parts.append(tube_along("bumperF", [(-1.08, 2.1, .57), (-.6, 2.16, .57), (.6, 2.16, .57), (1.08, 2.1, .57)], .1, None, resolution=16, bevel_resolution=4))
parts.append(tube_along("bumperR", [(-.94, -2.12, .55), (-.6, -2.16, .55), (.6, -2.16, .55), (.94, -2.12, .55)], .085, None, resolution=16, bevel_resolution=4))
# Canopy frame (truck fusion, Nov 16): tapered pillars that grow out of the cab rim on broad fillets, a roof rail and cross bars.
POSTS = [(sx * .94, y) for sx in (-1, 1) for y in (.67, -.06)]
TOP = 2.47


def pillar(x, y):
    bm = bmesh.new(); rings = []
    # (height, half-width across, half-width along): a broad fillet into the rim, then a slim tapered pillar
    prof = [(1.4, .075, .1), (1.47, .066, .09), (1.54, .056, .078), (1.64, .05, .07), (2.1, .046, .064), (2.4, .044, .061), (TOP + .03, .046, .064)]   # chunky toy pillars (thin ones melt in the fuse), a modest fillet
    for (z, hx, hy) in prof:
        ring = []
        for k in range(16):
            a = k / 16 * math.tau; c, s = math.cos(a), math.sin(a)
            # superellipse: rounded, a little squarer than an oval
            px = math.copysign(abs(c) ** .7, c) * hx; py = math.copysign(abs(s) ** .7, s) * hy
            ring.append(bm.verts.new((x * (1 - .006 * (z - 1.4)) + px, y + py, z)))
        rings.append(ring)
    for r0, r1 in zip(rings, rings[1:]):
        for k in range(16): bm.faces.new((r0[k], r0[(k + 1) % 16], r1[(k + 1) % 16], r1[k]))
    bm.faces.new(rings[0][::-1]); bm.faces.new(rings[-1])
    me = bpy.data.meshes.new("pillar"); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new("pillar", me); bpy.context.collection.objects.link(o)
    return o


for (x, y) in POSTS:
    parts.append(pillar(x, y))
R = .16; rx0, rx1, ry0, ry1 = -.93, .93, -.4, 1.45
rail = []
for (cx, cy, a0) in ((rx1 - R, ry1 - R, 0), (rx0 + R, ry1 - R, 90), (rx0 + R, ry0 + R, 180), (rx1 - R, ry0 + R, 270)):
    for k in range(7):
        a = math.radians(a0 + k * 15); rail.append((cx + math.cos(a) * R, cy + math.sin(a) * R, TOP))
rail.append(rail[0])
parts.append(tube_along("rail", rail, .046, None, resolution=16, bevel_resolution=4))
for y in (.67, -.06):
    parts.append(tube_along("crossbar", [(-.93, y, TOP), (0, y, TOP + .02), (.93, y, TOP)], .04, None, resolution=12, bevel_resolution=4))

shell = join(parts, "TruckShell")
bpy.context.view_layer.objects.active = shell; shell.select_set(True)
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)   # regions below test world-frame positions
remesh(shell, .018, (.5, 14))   # round 77's fuse (a finer one broke the colour edges into ragged teeth)
# Wheel wells: cut a cylinder out of the outer body around each wheel so the wheels sit inside the arches.
for (wx, wy, wz) in wheels:
    bpy.ops.mesh.primitive_cylinder_add(vertices=40, radius=.47, depth=.7, location=(wx + (.1 if wx > 0 else -.1), wy, wz), rotation=(0, math.radians(90), 0))
    cut = bpy.context.active_object
    b = shell.modifiers.new("well", "BOOLEAN"); b.operation = "DIFFERENCE"; b.object = cut; b.solver = "EXACT"
    bpy.context.view_layer.objects.active = shell; bpy.ops.object.modifier_apply(modifier="well")
    bpy.data.objects.remove(cut)

shell.data.materials.clear()
for n in ("body", "door", "ivory", "seam", "frame"): shell.data.materials.append(mat(n))


def region(c):
    x, y, z = c.x, c.y, c.z
    ax = abs(x)
    # Frame: everything above the cab rim, and the fillets round the pillar feet.
    if z > 1.49: return 4
    if z > 1.38 and any(math.hypot(x - px, y - py) < .15 for (px, py) in POSTS): return 4   # the pillar feet, so no ragged edge round them
    # Ivory: bonnet, bumpers, the belt line along the cab sides, and the arches around each wheel.
    if y > .8 and z > .84: return 2
    if abs(y) > 2.04 and z < .74 and (y > 0 or ax < .93): return 2   # rear: the bumper only, the body corners stay teal (an ivory corner read as a torn flap)
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
export(shell, "truck_shell", ["body", "door", "ivory", "seam", "frame"])
