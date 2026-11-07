"""Round 59: the chunky gardening glove from glove.py, open and palm up, so a clean piece of sea glass can rest in it
when the player holds it up at the sorting trays (review 55: the piece floated alone with the room magnified through
it and read as a pillow or hollow shell; the tray pieces, small and at rest, pass).

The origin is the middle of the palm's upper surface; the palm faces +Z (Unity +Y), the fingers point +Y (Unity +Z)
and curl up a little at the tips, the thumb lies out to +X, and the forearm sleeve runs back and down toward the
camera. Built as in glove.py: overlapping blocking volumes fused into ONE surface with a voxel remesh, smoothed so
the joins flow, coloured by region along the arm (canvas glove, coral knit cuff, blue sleeve), then reduced.
Run: blender -b --factory-startup -P palm_glove.py
"""
import sys, os, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise
from sleeve_shape import shaped_sleeve

reset()
axis_dir = Vector((0, -math.cos(math.radians(16)), -math.sin(math.radians(16))))   # forearm axis, back and down to the camera
wrist = Vector((0, -.078, -.026))
parts = []

# Palm: a broad, soft, flattened mass whose top sits just under the origin, with a shallow cup (heel and finger pads raised).
parts.append(uv_sphere("palm", .05, (0, -.008, -.02), (1.12, 1.08, .42), 28, 16, "glove"))
parts.append(uv_sphere("heel", .03, (.006, -.046, -.016), (1.5, .8, .55), 20, 10, "glove"))            # heel of the hand, raised
parts.append(uv_sphere("pads", .028, (0, .036, -.014), (1.85, .62, .55), 20, 10, "glove"))             # finger pads, raised
parts.append(uv_sphere("thenar", .026, (.034, -.022, -.012), (.9, 1.3, .75), 16, 10, "glove"))         # thumb mound, raised (round 60)
# Round 60 (review 56, all three: "a flat paddle", "a stiff, flat mitten"): a cupped palm, its little-finger edge raised
# like the thumb mound, so the middle sits lower than the rim of the hand.
parts.append(uv_sphere("outer edge", .024, (-.04, -.012, -.011), (.62, 1.65, .7), 16, 10, "glove"))
# Round 67 (review 63: "a stiff, flat mannequin glove", "sausage / tube fingers", "plank fingers"): each finger is built joint by
# joint as a chain of overlapping balls that shrink toward the tip and swell a little at each knuckle, so after fusing it
# tapers, shows its knuckles and bends at three joints. The fingers lie close together and curl up toward the viewer around
# the glass (a relaxed, cupped hand), the little finger curling most; the thumb curls in beside the index finger.
def chain(points, radii, step=.0032):
    out = []
    for (p0, r0), (p1, r1) in zip(zip(points, radii), zip(points[1:], radii[1:])):
        p0, p1 = Vector(p0), Vector(p1); n = max(1, int((p1 - p0).length / step))
        for k in range(n):
            t = k / n
            out.append(uv_sphere("joint", r0 + (r1 - r0) * t, tuple(p0 + (p1 - p0) * t), (1.08, 1, .9), 12, 8, "glove"))
    out.append(uv_sphere("tip", radii[-1], tuple(Vector(points[-1])), (1.08, 1, .9), 12, 8, "glove"))
    return out


for i in range(4):
    x = -.031 + i * .0205
    w = 1.06 if i in (1, 2) else .94                                   # middle fingers a little thicker
    segs = ((.044, .026, .021), (.047, .028, .022), (.045, .027, .021), (.036, .022, .018))[i]
    bends = ((14, 34, 38), (12, 32, 38), (14, 36, 40), (18, 42, 44))[i]   # degrees at the knuckle, middle and end joints
    fan = (-.07, -.02, .02, .08)[i]
    pos = Vector((x, .036, -.014)); ang = math.radians(4); pts = [tuple(pos)]
    for L, bend in zip(segs, bends):
        ang += math.radians(bend)
        d = Vector((fan * math.cos(ang), math.cos(ang), math.sin(ang))).normalized()
        pos = pos + d * L; pts.append(tuple(pos))
    radii = [.0132 * w, .0128 * w, .0117 * w, .0104 * w]
    parts.extend(chain(pts, radii))
# Thumb: from the thumb mound, curling in alongside the index finger.
parts.extend(chain([(.034, -.03, -.013), (.054, -.012, -.006), (.064, .011, .007), (.059, .031, .021), (.049, .043, .031)],
                   [.0158, .0146, .0132, .0118, .0105]))
# Wrist: a short taper from the heel into the cuff.
parts.append(cylinder("wrist", .04, .06, tuple(wrist - axis_dir * .012), rotation=(math.radians(106), 0, 0), vertices=24, material="glove", radius2=.047))

# Round 67: the shared, shaped work-shirt sleeve (sleeve_shape.py).
sl = shaped_sleeve(wrist, axis_dir, L=.8, r0=.05, r1=.082, seed=2)
assign(sl, "glove"); parts.append(sl)
# Knit cuff where glove and sleeve meet.
bpy.ops.mesh.primitive_torus_add(major_radius=.053, minor_radius=.016, major_segments=32, minor_segments=10)
cf = bpy.context.active_object
for v in cf.data.vertices:
    a = math.atan2(v.co.y, v.co.x); v.co.x *= 1.08; v.co.y *= .9; v.co.z += .004 * math.sin(a * 3)
cf.rotation_mode = "QUATERNION"; cf.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(axis_dir)
cf.location = wrist + axis_dir * .014; bpy.ops.object.transform_apply(location=True, rotation=True)   # round 67: nearer the hand, so the cuff shows below the palm
assign(cf, "glove"); parts.append(cf)

# Fuse into one surface.
arm = join(parts, "Palm")
bpy.context.view_layer.objects.active = arm
rm = arm.modifiers.new("fuse", "REMESH"); rm.mode = "VOXEL"; rm.voxel_size = .0026; rm.use_smooth_shade = True
bpy.ops.object.modifier_apply(modifier="fuse")
sm = arm.modifiers.new("soften", "CORRECTIVE_SMOOTH"); sm.factor = .6; sm.iterations = 8; sm.smooth_type = "LENGTH_WEIGHTED"
bpy.ops.object.modifier_apply(modifier="soften")

# Colour regions along the arm.
arm.data.materials.clear()
for n in ("glove", "cuff", "sleeve"): arm.data.materials.append(mat(n))
for p in arm.data.polygons:
    s = (Vector(p.center) - wrist).dot(axis_dir)
    if s < -.004: p.material_index = 0   # round 67: bands moved up with the cuff
    elif s < .034: p.material_index = 1
    else: p.material_index = 2
for p in arm.data.polygons: p.use_smooth = True
edge_band = arm.vertex_groups.new(name="band_edges")
for v in arm.data.vertices:
    s = (Vector(v.co) - wrist).dot(axis_dir)
    if min(abs(s + .004), abs(s - .034)) < .006: edge_band.add([v.index], 1.0, "REPLACE")
dec = arm.modifiers.new("reduce", "DECIMATE"); dec.ratio = .14
dec.vertex_group = "band_edges"; dec.invert_vertex_group = True; dec.vertex_group_factor = 10
bpy.ops.object.modifier_apply(modifier="reduce")
export(arm, "palm_glove", ["glove", "cuff", "sleeve"])
