"""Coastal workshop wall clutter: life ring, net with floats, coiled rope, picture frame, chalkboard.
Run: blender -b --factory-startup -P wall_clutter.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *

# Life ring (pivot centre, ring in the XZ plane facing -Y = Unity -Z). 0.7 m across.
reset()
bpy.ops.mesh.primitive_torus_add(major_radius=.3, minor_radius=.07, major_segments=40, minor_segments=14, rotation=(math.radians(90), 0, 0))
ring = bpy.context.active_object; bpy.ops.object.transform_apply(rotation=True)
ring.data.materials.append(mat("ring_white")); ring.data.materials.append(mat("ring_red"))
for p in ring.data.polygons:
    a = math.atan2(p.center.z, p.center.x)
    p.material_index = 1 if int((a + math.pi) / (math.pi / 2)) % 2 == 0 else 0
parts = [ring]
for k in range(4):
    a = k * math.pi / 2 + math.pi / 4
    parts.append(tube_along("rope", [(math.cos(a) * .2, -.075, math.sin(a) * .2), (math.cos(a) * .3, -.09, math.sin(a) * .3), (math.cos(a) * .4, -.075, math.sin(a) * .4)], .012, "rope", resolution=6, bevel_resolution=1))
lr = join(parts, "LifeRing"); shade_auto(lr, 50)
export(lr, "life_ring", ["ring_white", "ring_red", "rope"])

# Fishing net draped along a wall with cork and glass floats. Spans X 0..4 m, hangs down to -0.9 m.
reset()
random.seed(2)
parts = []
L = 4.0
def sag(x): return -.25 * (1 - ((x - L / 2) / (L / 2)) ** 2)
for j in range(5):                       # horizontal strands, sagging more lower down
    y0 = -j * .18
    pts = [(x, 0, y0 + sag(x) * (1 + j * .5)) for x in [i * L / 16 for i in range(17)]]
    parts.append(tube_along("h", pts, .006, "net", resolution=4, bevel_resolution=1))
for i in range(17):                      # vertical strands
    x = i * L / 16
    pts = [(x, 0, -j * .18 + sag(x) * (1 + j * .5)) for j in range(5)]
    parts.append(tube_along("v", pts, .006, "net", resolution=4, bevel_resolution=1))
for i in range(7):                       # floats along the top strand
    x = .3 + i * (L - .6) / 6
    if i % 2 == 0:
        parts.append(uv_sphere("glassfloat", .11, (x, -.05, sag(x) - .02), (1, 1, 1), 16, 10, "float_glass"))
    else:
        parts.append(cylinder("cork", .07, .14, (x, -.04, sag(x) - .02), rotation=(0, math.radians(90), 0), vertices=12, material="cork"))
net = join(parts, "Net"); shade_auto(net, 60)
export(net, "fishing_net", ["net", "float_glass", "cork"])

# Coiled rope on the floor (pivot base centre), ~0.6 m across.
reset()
pts = []
for i in range(120):
    t = i / 119; a = t * math.tau * 4.2
    r = .1 + .2 * t
    pts.append((math.cos(a) * r, math.sin(a) * r, .03 + (.035 if t > .55 else 0) * math.sin(t * 9) ** 2))
coil = tube_along("coil", pts, .028, "rope", resolution=4, bevel_resolution=2); shade_auto(coil, 50)
export(coil, "coiled_rope", ["rope"])

# Picture frame (pivot centre, picture faces -Y). 1 x 0.75 m; picture is a separate submesh with UVs.
reset()
W, H, T, B = 1.0, .75, .04, .07
frame = []
for (cx, cz, sx, sz) in [(0, H / 2 - B / 2, W, B), (0, -H / 2 + B / 2, W, B), (W / 2 - B / 2, 0, B, H), (-W / 2 + B / 2, 0, B, H)]:
    o = cube("f", (sx, T, sz), (cx, 0, cz), "frame"); bevel(o, .012, 2, 40); frame.append(o)
bpy.ops.mesh.primitive_plane_add(size=1, location=(0, -.005, 0), rotation=(math.radians(90), 0, 0))
pic = bpy.context.active_object; pic.scale = (W - 2 * B + .01, H - 2 * B + .01, 1); bpy.ops.object.transform_apply(rotation=True, scale=True)
assign(pic, "picture")
frame.append(pic)
fr = join(frame, "Frame")
export(fr, "picture_frame", ["frame", "picture"])

# Chalkboard on a little easel-less wall mount: 1.1 x 0.8 m.
reset()
W, H = 1.1, .8
parts = []
for (cx, cz, sx, sz) in [(0, H / 2 - .04, W, .08), (0, -H / 2 + .04, W, .08), (W / 2 - .04, 0, .08, H), (-W / 2 + .04, 0, .08, H)]:
    o = cube("f", (sx, .05, sz), (cx, 0, cz), "frame"); bevel(o, .015, 2, 40); parts.append(o)
bpy.ops.mesh.primitive_plane_add(size=1, location=(0, -.01, 0), rotation=(math.radians(90), 0, 0))
b = bpy.context.active_object; b.scale = (W - .15, H - .15, 1); bpy.ops.object.transform_apply(rotation=True, scale=True)
assign(b, "picture"); parts.append(b)
parts.append(cube("ledge", (W * .9, .07, .03), (0, -.04, -H / 2 - .01), "frame"))
cb = join(parts, "Chalkboard")
export(cb, "chalkboard", ["frame", "picture"])
