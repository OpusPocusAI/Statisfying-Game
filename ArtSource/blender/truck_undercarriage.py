"""Truck undercarriage (owner playtest, November 16: "Wheel is floating, nothing attached to it"). Hiding the old painted
chassis removed the rear-wheel flap but also everything that visibly held the wheels: under the body you looked straight
through to the road. This is one fused, dark running gear under the shell floor (which sits at axle height, 0.45):
two chassis rails along the length, a beam axle front and rear running into each wheel hub, a rounded differential on
each axle, leaf springs riding on the axles up into the rails, and a gearbox lump in the middle. Voxel-fused so it reads
as one cast piece, painted the arch-liner charcoal so it stays in the shade under the truck.
Axes (Blender): Unity X = X, Unity Z (forward) = Y, Unity Y (up) = Z; origin at the Sprung body origin.
Run: blender -b --factory-startup -P truck_undercarriage.py
"""
import sys, os, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *

AXLE_Z, AXLES, HUB_X = .45, (-1.40, 1.40), .93


def obj_from(bm, name):
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(o)
    return o


def tube(pts, radii, sides=14):
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


def blob(c, r, s):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=12, radius=r, location=c)
    o = bpy.context.active_object; o.scale = s; bpy.ops.object.transform_apply(location=False, scale=True)
    return o


def slab(c, size):
    bpy.ops.mesh.primitive_cube_add(size=1, location=c)
    o = bpy.context.active_object; o.scale = size; bpy.ops.object.transform_apply(location=False, scale=True)
    return o


reset()
parts = []
# chassis rails: a deep C-section reads as one rounded beam, kinked up over each axle like a real ladder frame
for side in (-1, 1):
    x = side * .5
    pts, rad = [], []
    for y in [-2.05, -1.8, -1.62, -1.40, -1.18, -.9, -.4, 0, .4, .9, 1.18, 1.40, 1.62, 1.8, 2.0]:
        over = max(0, 1 - abs(abs(y) - 1.40) / .3)                 # rises a little over the axle
        pts.append((x, y, .38 + .05 * over)); rad.append(.07)
    parts.append(tube(pts, rad))
    # leaf springs: a shallow arc under each rail, hung from the rail ends and clamped on the axle
    for ay in AXLES:
        sp = [(x, ay + d, AXLE_Z - .02 + .1 * (d / .5) ** 2) for d in [-.5, -.3, -.12, 0, .12, .3, .5]]
        parts.append(tube(sp, [.035, .04, .045, .05, .045, .04, .035], 10))
# beam axles, thickening into each hub so the wheel is visibly carried
for ay in AXLES:
    xs = [-HUB_X, -.8, -.5, -.2, 0, .2, .5, .8, HUB_X]
    parts.append(tube([(x, ay, AXLE_Z) for x in xs], [.1, .065, .055, .06, .07, .06, .055, .065, .1]))
    parts.append(blob((.08, ay, AXLE_Z - .03), .17, (1, .9, 1)))          # differential, a little off centre
# rear: a cross member at the tail ties the rails; front: a tie rod ahead of the axle
parts.append(tube([(-.5, -1.95, .4), (0, -1.95, .41), (.5, -1.95, .4)], [.06, .06, .06]))
parts.append(tube([(-.85, 1.55, AXLE_Z - .02), (0, 1.58, AXLE_Z - .02), (.85, 1.55, AXLE_Z - .02)], [.03, .03, .03], 10))
# gearbox and transfer lump between the rails, under the cab floor
parts.append(blob((0, .55, .36), .2, (1.15, 2.2, .8)))
# propeller shafts to each axle
parts.append(tube([(0, .1, .36), (.04, -1.0, .42), (.08, -1.25, AXLE_Z)], [.04, .04, .045], 10))
parts.append(tube([(0, 1.0, .36), (.04, 1.25, AXLE_Z)], [.04, .045], 10))

o = join(parts, "undercarriage")
md = o.modifiers.new("r", "REMESH"); md.mode = "VOXEL"; md.voxel_size = .016
bpy.context.view_layer.objects.active = o; bpy.ops.object.modifier_apply(modifier="r")
sm = o.modifiers.new("s", "CORRECTIVE_SMOOTH"); sm.factor, sm.iterations = .5, 6
bpy.ops.object.modifier_apply(modifier="s")
d = o.modifiers.new("d", "DECIMATE"); d.ratio = min(1.0, 9000 / max(1, len(o.data.polygons)))
bpy.ops.object.modifier_apply(modifier="d")
for p in o.data.polygons: p.use_smooth = True
o.data.materials.append(mat("chassis"))
export(o, "truck_undercarriage", ["chassis"])
