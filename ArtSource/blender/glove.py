"""Chunky gardening glove gripping a bar that runs along X through the origin (radius ~2 cm).
The palm sits on the player side (-Y in Blender = -Z in Unity); fingers wrap over the top.
Run: blender -b --factory-startup -P glove.py
"""
import sys, os, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *

reset()
parts = []
# Palm: rounded block behind and below the bar.
palm = uv_sphere("palm", .05, (0, -.045, -.02), (1.1, .75, 1.0), 20, 12, "glove")
parts.append(palm)
# Four fingers curling over the bar from behind to the front.
for i in range(4):
    x = -.036 + i * .024
    r = .011 if i in (1, 2) else .0095
    pts = [(x, -.045, .015), (x, -.022, .036), (x, .012, .034), (x, .03, .012), (x, .026, -.008)]
    parts.append(tube_along("finger", pts, r, "glove", resolution=10, bevel_resolution=3))
# Thumb wrapping the other way under the bar.
parts.append(tube_along("thumb", [(-.05, -.06, -.03), (-.055, -.02, -.035), (-.045, .015, -.025)], .012, "glove", resolution=10, bevel_resolution=3))
# Cuff and a bit of forearm sleeve heading back toward the camera.
cuff = cylinder("cuff", .045, .05, (0, -.095, -.045), rotation=(math.radians(82), 0, 0), vertices=20, material="cuff")
bevel(cuff, .008, 2, 40); parts.append(cuff)
# Round 34 (reviews: "a long segmented blue pipe filling a quarter of the frame"): one sculpted cloth sleeve,
# shorter, oval in section, swelling towards the elbow, with soft diagonal wrinkles and a loose, uneven turned-up
# cuff, instead of a straight tube with ring folds.
from mathutils import noise
L = .72   # long enough that the open end leaves the frame
axis_dir = Vector((0, -math.cos(math.radians(8)), math.sin(math.radians(8))))   # sleeve axis (82 deg about X from +Z)
start = Vector((0, -.095, -.045)) - axis_dir * .02                              # at the glove cuff
bpy.ops.mesh.primitive_cylinder_add(vertices=28, radius=1, depth=1, end_fill_type="NOTHING")   # open: the ends are hidden in the cuff and off-screen
sl = bpy.context.active_object
bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=10); bpy.ops.object.mode_set(mode="OBJECT")
for v in sl.data.vertices:
    t = v.co.z + .5                                    # 0 at the wrist .. 1 at the elbow
    a = math.atan2(v.co.y, v.co.x); rr = math.hypot(v.co.x, v.co.y)
    if rr < 1e-4: continue
    r = .05 + .036 * t ** .8                           # tapers to the wrist
    r *= 1 + .05 * math.sin(a * 2 + t * 9) * (.4 + t)  # soft diagonal wrinkles
    r *= 1 + .035 * noise.noise(Vector((math.cos(a) * 2, math.sin(a) * 2, t * 4)))
    v.co = Vector((math.cos(a) * r * 1.1, math.sin(a) * r * .88, t * L))

# Orient along the sleeve axis, from the cuff back towards the elbow.
sl.rotation_mode = "QUATERNION"; sl.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(axis_dir)   # axis_dir points back toward the camera
sl.location = start; bpy.ops.object.transform_apply(location=True, rotation=True)
assign(sl, "sleeve"); parts.append(sl)
# A loose turned-up cuff: an uneven, slightly tilted roll just behind the glove.
bpy.ops.mesh.primitive_torus_add(major_radius=.056, minor_radius=.013, major_segments=24, minor_segments=8)
roll = bpy.context.active_object
for v in roll.data.vertices:
    a = math.atan2(v.co.y, v.co.x)
    v.co.x *= 1.1; v.co.y *= .9; v.co.z += .006 * math.sin(a * 3)
roll.rotation_mode = "QUATERNION"; roll.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(axis_dir)
roll.location = start + axis_dir * .03; bpy.ops.object.transform_apply(location=True, rotation=True)
assign(roll, "cuff"); parts.append(roll)
g = join(parts, "Glove")
shade_auto(g, 50)
export(g, "garden_glove", ["glove", "cuff", "sleeve"])
