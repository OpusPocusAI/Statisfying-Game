"""Truck fusion pass (owner, November 16, from the independent art review: all three reviewers said the truck still reads as
"posts, rails and tubs attached to a body"; posts "plunge into the bonnet with no join", the mirrors "float").

- truck_frame: the four canopy posts and the roof frame as ONE fused piece in the truck's local frame. Each post stands on
  the bodywork on a flared foot (a fillet that spreads into a low pad shaped to the surface under it, found by casting onto
  truck_shell), tapers as it rises, and bends into a rounded roof rail that runs round under the canvas canopy.
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


# The body, to stand the posts on.
data = json.load(open(os.path.join(OUT_DIR, "truck_shell.json")))
P = data["positions"]; verts = [Vector((P[k], P[k + 1], P[k + 2])) for k in range(0, len(P), 3)]
tris = [tuple(s["indices"][k:k + 3]) for s in data["submeshes"] for k in range(0, len(s["indices"]), 3)]
body = BVHTree.FromPolygons(verts, tris)

# ---------------------------------------------------------------- canopy frame
reset()
TOP = 2.47                     # just under the canvas (canopy underside at about 2.46-2.5)
posts = [(sx * .94, y) for sx in (-1, 1) for y in (.67, -.06)]   # back posts on the rear end of the cab rim (behind it the body drops to the cargo deck)
parts = []
for (x, y) in posts:
    hit, n, _, _ = body.ray_cast(Vector((x, y, 3)), Vector((0, 0, -1)))
    foot = hit if hit is not None else Vector((x, y, 1.3))
    n = n if n is not None else Vector((0, 0, 1))
    # tapering post, with a gentle inward lean into the roof rail
    pts = [foot + Vector((0, 0, -.03)), foot + Vector((0, 0, .1)), Vector((x, y, (foot.z + TOP) * .5)), Vector((x * .995, y, TOP - .06)), Vector((x * .99, y, TOP))]
    parts.append(tube(pts, [.075, .052, .046, .043, .045], 14))
    # flared foot: a fillet cone into a low pad lying along the surface
    parts.append(tube([foot + Vector((0, 0, -.02)), foot + Vector((0, 0, .07))], [.1, .055], 16))
    pad = ellipsoid(foot - n * .01, .12, (1, 1, .3))
    pad.rotation_euler = n.to_track_quat("Z", "Y").to_euler(); bpy.context.view_layer.objects.active = pad; bpy.ops.object.transform_apply(rotation=True)
    parts.append(pad)
    # a knuckle where the post turns into the roof rail
    parts.append(ellipsoid(Vector((x * .99, y, TOP)), .062, (1, 1, .9)))
# roof rail: a rounded rectangle through the post tops, a little proud of them front and back
R = .14; x0, x1, y0, y1 = -.93, .93, -.42, 1.45
ring = []
for (cx, cy, a0) in ((x1 - R, y1 - R, 0), (x0 + R, y1 - R, 90), (x0 + R, y0 + R, 180), (x1 - R, y0 + R, 270)):
    for k in range(7):
        a = math.radians(a0 + k * 15)
        ring.append((cx + math.cos(a) * R, cy + math.sin(a) * R, TOP))
ring.append(ring[0])
parts.append(tube(ring, [.034] * len(ring), 12))
# a cross bar between the front posts, and one between the back posts, so the frame reads as one braced cage
for y in (.67, -.06):
    parts.append(tube([(-.93, y, TOP), (0, y, TOP + .015), (.93, y, TOP)], [.03, .03, .03], 12))
o = fuse(parts, .0085, 9000, "frame")
o.data.materials.append(mat("frame"))
export(o, "truck_frame", ["frame"])

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
