"""Truck fusion pass (owner, November 16, from the independent art review: all three reviewers said the truck still reads as
"posts, rails and tubs attached to a body"; posts "plunge into the bonnet with no join", the mirrors "float").

- (The separate canopy frame first made here was replaced by the frame fused into the body shell, truck_body.py.)
- truck_wheel_well: a dark arch lining for inside each wheel arch, so the coral cargo tub and the body's underside no
  longer show through the arch above the tyre.
Axes (Blender): Unity X = X, Unity Z (forward) = Y, Unity Y (up) = Z; origin at the Sprung body origin.
Run: blender -b --factory-startup -P truck_fusion.py
"""
import sys, os, math, json
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils.bvhtree import BVHTree


def obj_from(bm, name):
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(o)
    return o


def tube(pts, radii, sides=12):
    bm = bmesh.new(); rings = []; pts = [Vector(p) for p in pts]
    for i, p in enumerate(pts):
        t = (pts[min(i + 1, len(pts) - 1)] - pts[max(i - 1, 0)]).normalized()
        a = Vector((0, 0, 1)) if abs(t.z) < .9 else Vector((1, 0, 0))
        u = t.cross(a).normalized(); v = t.cross(u)
        rings.append([bm.verts.new(p + (u * math.cos(k / sides * math.tau) + v * math.sin(k / sides * math.tau)) * radii[i]) for k in range(sides)])
    for a, b in zip(rings, rings[1:]):
        for k in range(sides): bm.faces.new((a[k], a[(k + 1) % sides], b[(k + 1) % sides], b[k]))
    for ring, flip in ((rings[0], True), (rings[-1], False)):
        c = bm.verts.new(sum((v.co for v in ring), Vector()) / sides)
        for k in range(sides): f = (ring[k], ring[(k + 1) % sides], c); bm.faces.new(f[::-1] if flip else f)
    return obj_from(bm, "tube")


def ellipsoid(c, r, s):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=12, radius=r, location=c)
    o = bpy.context.active_object; o.scale = s; bpy.ops.object.transform_apply(location=False, scale=True)
    return o


def fuse(parts, voxel, tris, name, smooth=4):
    o = join(parts, name)
    md = o.modifiers.new("r", "REMESH"); md.mode = "VOXEL"; md.voxel_size = voxel
    bpy.context.view_layer.objects.active = o; bpy.ops.object.modifier_apply(modifier="r")
    sm = o.modifiers.new("s", "CORRECTIVE_SMOOTH"); sm.factor, sm.iterations = .5, smooth
    bpy.ops.object.modifier_apply(modifier="s")
    d = o.modifiers.new("d", "DECIMATE"); d.ratio = min(1.0, tris / max(1, len(o.data.polygons) * 2))
    bpy.ops.object.modifier_apply(modifier="d")
    for p in o.data.polygons: p.use_smooth = True
    return o


# ---------------------------------------------------------------- wheel-well lining (axle along X; inner wall on +X)
# A band over the top of the tyre inside the arch, and a wall on the inner side (toward the truck centre), so nothing of the
# body underside or the cargo tub shows through the arch. Placed at the wheel, mirrored for right-hand wheels.
reset()
bm = bmesh.new(); seg = 20; xo, xi = -.13, .13; r_in, r_out = .442, .5   # inner radius just clear of the tyre (0.43), so nothing shows above it
band = []
for i in range(seg + 1):
    a = math.radians(12 + 156 * i / seg); c, s = math.cos(a), math.sin(a)
    band.append([bm.verts.new((x, c * r, s * r)) for (x, r) in ((xo, r_in), (xi, r_in), (xi, r_out), (xo, r_out))])
for i in range(seg):
    a, b = band[i], band[i + 1]
    for k in range(4): bm.faces.new((a[k], a[(k + 1) % 4], b[(k + 1) % 4], b[k]))
for ring, flip in ((band[0], True), (band[-1], False)): bm.faces.new(ring[::-1] if flip else ring)
# inner wall: a fan from the axle line to the band, on the inner side
hub = bm.verts.new((xi, 0, .02)); rim = [bm.verts.new((xi, math.cos(math.radians(12 + 156 * i / seg)) * r_in, math.sin(math.radians(12 + 156 * i / seg)) * r_in)) for i in range(seg + 1)]
for i in range(seg):
    f = bm.faces.new((hub, rim[i], rim[i + 1]))
o = obj_from(bm, "well")
for p in o.data.polygons: p.use_smooth = False
o.data.materials.append(mat("liner"))
export(o, "truck_wheel_well", ["liner"])
