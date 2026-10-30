"""Round 29 models (after the twenty-seventh review, three reviewers averaged): the truck, beach props and signs
that every review still called "boxes".
- truck_bonnet_r: a crowned bonnet that rolls down into the grille, instead of a bevelled slab.
- truck_body_r: the body with rolled, pontoon-like sills and a swage line, instead of a flat chassis slab.
- truck_post_r: a tapered canopy post with a flared foot and a cap, instead of a plain cylinder.
- cool_box_r: a cooler with a rounded body, a lid that overhangs with a lip, hinges, a recessed handle and a
  two-tone split (materials "body" and "lid").
- sign_board_r: a routed wooden sign: a bevelled board with a raised frame (materials "board" and "frame").
All are built near unit size; Round29Pass fits each one to the bounds of the part it replaces.
Run: blender -b --factory-startup -P round29.py
"""
import sys, os, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
import bmesh


def rounded(name, size, radius, material, segs=4, loc=(0, 0, 0)):
    o = cube(name, size, loc, material)
    bevel(o, radius, segs, 50)
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.modifier_apply(modifier="Bevel")
    return o


def deform(o, fn):
    for v in o.data.vertices:
        v.co = fn(v.co.copy())


# ---------------------------------------------------------------- bonnet (Unity 2.0 x 0.52 x 1.16 -> Blender x, y=depth, z=height)
reset()
b = rounded("bonnet", (2.0, 1.16, .52), .16, "paint", 6)
bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=4); bpy.ops.object.mode_set(mode="OBJECT")


def bonnet_shape(p):
    # Crown across the width and a roll-down towards the front (Blender +y = Unity +z = forward).
    w = p.x / 1.0
    crown = .07 * (1 - w * w)
    front = max(0.0, (p.y - .1) / .48)
    if p.z > 0:
        p.z += crown - .16 * front * front
        p.x *= 1 - .06 * front                      # narrows slightly to the nose
    return p


deform(b, bonnet_shape)
shade_auto(b, 50)
export(b, "truck_bonnet_r", ["paint"])

# ---------------------------------------------------------------- body (Unity 2.0 x 0.4 x 4.2)
reset()
body = rounded("body", (1.84, 4.2, .34), .1, "paint", 4)
parts = [body]
for sx in (-1, 1):                                     # rolled sills along both sides
    bpy.ops.mesh.primitive_cylinder_add(vertices=20, radius=.13, depth=4.0, location=(sx * .91, 0, -.03), rotation=(math.pi / 2, 0, 0))
    s = bpy.context.active_object
    s.scale = (1, 1.0, 1); bpy.ops.object.transform_apply(scale=True)
    bevel(s, .05, 3, 40); bpy.context.view_layer.objects.active = s; bpy.ops.object.modifier_apply(modifier="Bevel")
    assign(s, "paint"); parts.append(s)
    # A thin swage line just above the sill.
    bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=.018, depth=3.7, location=(sx * .93, 0, .1), rotation=(math.pi / 2, 0, 0))
    l = bpy.context.active_object; assign(l, "trim"); parts.append(l)
body = join(parts, "Body")
shade_auto(body, 45)
export(body, "truck_body_r", ["paint", "trim"])

# ---------------------------------------------------------------- canopy post (Unity 0.18 x 1.3)
reset()
bpy.ops.mesh.primitive_cylinder_add(vertices=14, radius=.5, depth=1.0)
p = bpy.context.active_object
bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=8); bpy.ops.object.mode_set(mode="OBJECT")


def post(v):
    t = v.z + .5
    r = math.hypot(v.x, v.y)
    if r < 1e-4:
        return v
    k = .62 - .14 * t                                   # tapers upwards
    k += .38 * math.exp(-(t / .07) ** 2)                # flared foot
    k += .22 * math.exp(-((t - 1) / .05) ** 2)          # cap
    v.x *= k / .5 * .5; v.y *= k / .5 * .5
    return v


deform(p, post)
assign(p, "post"); shade_auto(p, 50)
export(p, "truck_post_r", ["post"])

# ---------------------------------------------------------------- cooler (nominal 1 x 0.62 x 0.6)
reset()
W, D, H = 1.0, .62, .6
lower = rounded("lower", (W, D, H * .72), .09, "body", 5, (0, 0, H * .36))
lid = rounded("lid", (W + .04, D + .04, H * .26), .08, "lid", 5, (0, 0, H * .72 + H * .13))
parts = [lower, lid]
for sx in (-.28, .28):                                  # hinges at the back
    hg = rounded("hinge", (.12, .05, .06), .015, "lid", 2, (sx, -D / 2 - .03, H * .73)); parts.append(hg)
# Recessed carry handle on the lid, and a latch tab at the front.
bpy.ops.mesh.primitive_torus_add(major_radius=.17, minor_radius=.025, location=(0, 0, H + .02), rotation=(math.pi / 2, 0, 0))
hd = bpy.context.active_object; hd.scale = (1, 1, .6); bpy.ops.object.transform_apply(scale=True)
assign(hd, "lid"); parts.append(hd)
tab = rounded("latch", (.16, .04, .1), .015, "lid", 2, (0, D / 2 + .02, H * .7)); parts.append(tab)
cooler = join(parts, "Cooler")
shade_auto(cooler, 40)
export(cooler, "cool_box_r", ["body", "lid"])

# ---------------------------------------------------------------- routed sign board (nominal 5 x 1.1 x 0.14)
reset()
board = rounded("board", (5.0, .12, 1.1), .05, "board", 3)
frame_parts = [board]
for (w, h, x, z) in ((5.0, .09, 0, .505), (5.0, .09, 0, -.505), (.09, 1.1, 2.455, 0), (.09, 1.1, -2.455, 0)):
    f = rounded("frame", (w, .16, h), .03, "frame", 2, (x, 0, z)); frame_parts.append(f)
sign = join(frame_parts, "Sign")
shade_auto(sign, 40)
export(sign, "sign_board_r", ["board", "frame"])
