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
from sleeve_shape import shaped_sleeve

reset()
axis_dir = Vector((0, -math.cos(math.radians(-10)), math.sin(math.radians(-10))))   # forearm axis, pointing back to the camera; round 69: -10 in the mesh, so with the brush tipped 18 degrees to show its bristles (EquipmentVisuals.BristleTilt) the forearm rises about 8 degrees to the lower right; round 67 had 13, round 35 8
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

# Round 67: the shared, shaped work-shirt sleeve (sleeve_shape.py) instead of a plain tapered tube.
sl = shaped_sleeve(wrist, axis_dir, L=1.0, r0=.042, r1=.064, seed=1)   # round 69 (review 65: "a huge sleeve tube"): about a quarter slimmer
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
for n in ("glove", "cuff", "sleeve", "coat"): arm.data.materials.append(mat(n))
for p in arm.data.polygons:
    s = (Vector(p.center) - wrist).dot(axis_dir)     # < 0 on the hand, > 0 along the forearm
    wob = 0.0   # round 35: straight bands; a wobble zigzagged across the reduced triangles
    if s < .012 + wob: p.material_index = 0
    elif s < .05 + wob: p.material_index = 1
    else: p.material_index = 2
    # Round 68 (review 64: "a flat, single-tone glove"): a rubber-dipped work glove. The palm side and the fingers from the
    # knuckles on are coated in a contrasting colour; the back of the hand stays canvas, so the hand reads in two tones.
    c = Vector(p.center)
    if p.material_index == 0 and (p.normal.y < -.35 or (c.y > -.03 and c.z > -.002)): p.material_index = 3
for p in arm.data.polygons: p.use_smooth = True
# Reduce only after colouring the dense surface, so the band edges stay clean.
edge_band = arm.vertex_groups.new(name="band_edges")
for v in arm.data.vertices:
    s = (Vector(v.co) - wrist).dot(axis_dir)
    if min(abs(s - .012), abs(s - .05)) < .006: edge_band.add([v.index], 1.0, "REPLACE")
dec = arm.modifiers.new("reduce", "DECIMATE"); dec.ratio = .14   # round 67: a little more, so the sleeve folds survive
dec.vertex_group = "band_edges"; dec.invert_vertex_group = True; dec.vertex_group_factor = 10   # keep detail along the colour edges
bpy.ops.object.modifier_apply(modifier="reduce")
export(arm, "garden_glove", ["glove", "cuff", "sleeve", "coat"])
