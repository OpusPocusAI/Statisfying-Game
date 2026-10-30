"""Chunky gardening glove gripping a bar that runs along X through the origin (radius ~2 cm), with the forearm
sleeve running back toward the camera. The palm sits on the player side (-Y in Blender = -Z in Unity); fingers wrap
over the top.

Round 35 (owner: "the arm and hand look terrible ... made out of different shapes put together"): the hand, wrist,
cuff and sleeve are no longer separate primitives with visible joins. They are blocked in as overlapping volumes,
fused into ONE surface with a voxel remesh, smoothed so the joins flow (knuckles into palm, palm into wrist, wrist
into cuff, cuff into sleeve), reduced, and then coloured by region along the arm: canvas glove, a coral knit cuff,
the blue sleeve.
Run: blender -b --factory-startup -P glove.py
"""
import sys, os, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise

reset()
axis_dir = Vector((0, -math.cos(math.radians(8)), math.sin(math.radians(8))))   # forearm axis, pointing back to the camera
wrist = Vector((0, -.095, -.045))                                                  # where the glove meets the cuff
parts = []

# ---------------------------------------------------------------- blocking volumes (all one material for now)
# Palm and back of the hand: a soft, slightly flattened mass behind and below the bar.
parts.append(uv_sphere("palm", .052, (0, -.048, -.018), (1.12, .8, 1.0), 24, 14, "glove"))
# Knuckle ridge, so the fingers grow out of the hand instead of being stuck on.
parts.append(uv_sphere("knuckles", .03, (.0, -.036, .012), (1.75, .7, .7), 20, 10, "glove"))
# Four fingers curling over the bar, thick enough to stay distinct after fusing.
for i in range(4):
    x = -.036 + i * .024
    r = .0125 if i in (1, 2) else .011
    pts = [(x, -.046, .012), (x, -.024, .036), (x, .01, .036), (x, .03, .014), (x, .027, -.008)]
    parts.append(tube_along("finger", pts, r, "glove", resolution=12, bevel_resolution=4))
# Thumb wrapping under the bar, rooted in the palm.
parts.append(tube_along("thumb", [(-.035, -.07, -.03), (-.056, -.05, -.036), (-.056, -.018, -.036), (-.046, .012, -.026)], .0135, "glove", resolution=12, bevel_resolution=4))
# Wrist: a short taper from the heel of the hand into the cuff.
parts.append(cylinder("wrist", .043, .07, tuple(wrist - axis_dir * .015), rotation=(math.radians(82), 0, 0), vertices=24, material="glove", radius2=.05))

# Sleeve: a tapering, gently wrinkled cloth tube from the wrist back past the edge of the frame.
L = .72
bpy.ops.mesh.primitive_cylinder_add(vertices=32, radius=1, depth=1)
sl = bpy.context.active_object
bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=8); bpy.ops.object.mode_set(mode="OBJECT")
for v in sl.data.vertices:
    t = v.co.z + .5                                  # 0 at the wrist .. 1 at the elbow
    a = math.atan2(v.co.y, v.co.x); rr = math.hypot(v.co.x, v.co.y)
    r = (.053 + .034 * t ** .8) * min(1.0, rr * 1.0001)
    r *= 1 + .045 * math.sin(a * 2 + t * 9) * (.4 + t)
    r *= 1 + .03 * noise.noise(Vector((math.cos(a) * 2, math.sin(a) * 2, t * 4)))
    v.co = Vector((math.cos(a) * r * 1.08, math.sin(a) * r * .9, t * L))
sl.rotation_mode = "QUATERNION"; sl.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(axis_dir)
sl.location = wrist + axis_dir * .01; bpy.ops.object.transform_apply(location=True, rotation=True)
assign(sl, "glove"); parts.append(sl)
# Knit cuff: a soft band where glove and sleeve meet, slightly proud of both.
bpy.ops.mesh.primitive_torus_add(major_radius=.055, minor_radius=.016, major_segments=32, minor_segments=10)
cf = bpy.context.active_object
for v in cf.data.vertices:
    a = math.atan2(v.co.y, v.co.x); v.co.x *= 1.08; v.co.y *= .9; v.co.z += .004 * math.sin(a * 3)
cf.rotation_mode = "QUATERNION"; cf.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(axis_dir)
cf.location = wrist + axis_dir * .03; bpy.ops.object.transform_apply(location=True, rotation=True)
assign(cf, "glove"); parts.append(cf)

# ---------------------------------------------------------------- fuse into one surface
arm = join(parts, "Arm")
bpy.context.view_layer.objects.active = arm
rm = arm.modifiers.new("fuse", "REMESH"); rm.mode = "VOXEL"; rm.voxel_size = .0028; rm.use_smooth_shade = True
bpy.ops.object.modifier_apply(modifier="fuse")
sm = arm.modifiers.new("soften", "CORRECTIVE_SMOOTH"); sm.factor = .6; sm.iterations = 8; sm.smooth_type = "LENGTH_WEIGHTED"
bpy.ops.object.modifier_apply(modifier="soften")

# ---------------------------------------------------------------- colour regions along the arm
arm.data.materials.clear()
for n in ("glove", "cuff", "sleeve"): arm.data.materials.append(mat(n))
for p in arm.data.polygons:
    s = (Vector(p.center) - wrist).dot(axis_dir)     # < 0 on the hand, > 0 along the forearm
    wob = 0.0   # round 35: straight bands; a wobble zigzagged across the reduced triangles
    if s < .012 + wob: p.material_index = 0
    elif s < .05 + wob: p.material_index = 1
    else: p.material_index = 2
for p in arm.data.polygons: p.use_smooth = True
# Reduce only after colouring the dense surface, so the band edges stay clean.
edge_band = arm.vertex_groups.new(name="band_edges")
for v in arm.data.vertices:
    s = (Vector(v.co) - wrist).dot(axis_dir)
    if min(abs(s - .012), abs(s - .05)) < .006: edge_band.add([v.index], 1.0, "REPLACE")
dec = arm.modifiers.new("reduce", "DECIMATE"); dec.ratio = .1
dec.vertex_group = "band_edges"; dec.invert_vertex_group = True; dec.vertex_group_factor = 10   # keep detail along the colour edges
bpy.ops.object.modifier_apply(modifier="reduce")
export(arm, "garden_glove", ["glove", "cuff", "sleeve"])
