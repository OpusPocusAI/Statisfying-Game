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
# Four fingers, relaxed and curling up from the knuckles (round 60: they lay straight out, a paddle), fanned a little, each
# a slightly different curl, middle two the longest.
for i in range(4):
    x = -.034 + i * .0225
    r = .0118 if i in (1, 2) else .0105
    reach = (.1, .107, .104, .09)[i]
    lift = (.034, .03, .032, .038)[i]
    fan = x * .35
    pts = [(x, .036, -.015), (x + fan * .3, .062, -.008), (x + fan * .7, reach - .014, lift * .55), (x + fan, reach, lift)]
    parts.append(tube_along("finger", pts, r, "glove", resolution=12, bevel_resolution=4))
# Thumb out to the side, its tip lifting.
parts.append(tube_along("thumb", [(.036, -.03, -.016), (.06, -.006, -.008), (.07, .02, .008), (.062, .04, .026)], .0128, "glove", resolution=12, bevel_resolution=4))   # round 60: curling in over the palm
# Wrist: a short taper from the heel into the cuff.
parts.append(cylinder("wrist", .04, .06, tuple(wrist - axis_dir * .012), rotation=(math.radians(106), 0, 0), vertices=24, material="glove", radius2=.047))

# Sleeve: a tapering, gently wrinkled cloth tube from the wrist back past the edge of the frame.
L = .7
bpy.ops.mesh.primitive_cylinder_add(vertices=32, radius=1, depth=1)
sl = bpy.context.active_object
bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=8); bpy.ops.object.mode_set(mode="OBJECT")
for v in sl.data.vertices:
    t = v.co.z + .5
    a = math.atan2(v.co.y, v.co.x); rr = math.hypot(v.co.x, v.co.y)
    r = (.051 + .034 * t ** .8) * min(1.0, rr * 1.0001)
    r *= 1 + .045 * math.sin(a * 2 + t * 9) * (.4 + t)
    r *= 1 + .03 * noise.noise(Vector((math.cos(a) * 2, math.sin(a) * 2, t * 4)))
    v.co = Vector((math.cos(a) * r * 1.08, math.sin(a) * r * .9, t * L))
sl.rotation_mode = "QUATERNION"; sl.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(axis_dir)
sl.location = wrist + axis_dir * .01; bpy.ops.object.transform_apply(location=True, rotation=True)
assign(sl, "glove"); parts.append(sl)
# Knit cuff where glove and sleeve meet.
bpy.ops.mesh.primitive_torus_add(major_radius=.053, minor_radius=.016, major_segments=32, minor_segments=10)
cf = bpy.context.active_object
for v in cf.data.vertices:
    a = math.atan2(v.co.y, v.co.x); v.co.x *= 1.08; v.co.y *= .9; v.co.z += .004 * math.sin(a * 3)
cf.rotation_mode = "QUATERNION"; cf.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(axis_dir)
cf.location = wrist + axis_dir * .03; bpy.ops.object.transform_apply(location=True, rotation=True)
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
    if s < .012: p.material_index = 0
    elif s < .05: p.material_index = 1
    else: p.material_index = 2
for p in arm.data.polygons: p.use_smooth = True
edge_band = arm.vertex_groups.new(name="band_edges")
for v in arm.data.vertices:
    s = (Vector(v.co) - wrist).dot(axis_dir)
    if min(abs(s - .012), abs(s - .05)) < .006: edge_band.add([v.index], 1.0, "REPLACE")
dec = arm.modifiers.new("reduce", "DECIMATE"); dec.ratio = .1
dec.vertex_group = "band_edges"; dec.invert_vertex_group = True; dec.vertex_group_factor = 10
bpy.ops.object.modifier_apply(modifier="reduce")
export(arm, "palm_glove", ["glove", "cuff", "sleeve"])
