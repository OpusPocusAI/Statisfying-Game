"""Round 11 models (after the seventh independent review):
- driftwood_branch: a bleached, tapering, bent branch with a fork and a broken stub (replaces a hexagonal
  prism log). About 2.5 m long, lying on z=0.
- truck_fender: a rounded fender shell that arches over a wheel, with a flared lip.
  True size 0.32 (x, across) x 0.62 (height) x 1.25 (along y); origin at the wheel centre height.
Run: blender -b --factory-startup -P round11.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise

# ---------------------------------------------------------------- driftwood branch
reset(); random.seed(1101)
main = [(-1.25, 0, .14), (-.6, .08, .17), (0, -.05, .16), (.6, .1, .13), (1.2, .02, .09)]
parts = [tube_along("main", main, .13, "wood", resolution=10, bevel_resolution=3)]
# taper along X and add knotty, wind-worn noise
o = parts[0]
for v in o.data.vertices:
    t = (v.co.x + 1.25) / 2.45
    c = Vector((v.co.x, 0, .14))
    v.co.y *= 1 - .45 * t; v.co.z = .14 + (v.co.z - .14) * (1 - .45 * t)
    v.co += Vector((0, noise.noise(v.co * 5) * .02, noise.noise(v.co * 6 + Vector((3, 0, 0))) * .015))
    v.co.z = max(v.co.z, .01)
parts.append(tube_along("fork", [(.1, -.02, .15), (.55, -.35, .14), (.95, -.55, .1)], .06, "wood", resolution=8, bevel_resolution=2))
parts.append(tube_along("stub", [(-.55, .07, .2), (-.5, .2, .34)], .05, "wood", resolution=6, bevel_resolution=2))
parts.append(cylinder("root", .2, .12, (-1.3, 0, .16), rotation=(0, math.radians(90), 0), vertices=9, material="wood_end"))
br = join(parts, "Driftwood")
for p in br.data.polygons: p.use_smooth = False
export(br, "driftwood_branch", ["wood", "wood_end"])

# ---------------------------------------------------------------- truck fender
reset()
R_in, R_out, W, L = .44, .5, .32, 1.25
bm = bmesh.new()
segs = 18
ring_in, ring_out = [], []
for s in range(segs + 1):
    a = math.pi * s / segs                     # 0..pi over the top of the wheel
    y = -math.cos(a) * L / 2
    z = math.sin(a) * (R_out + .02)
    for side in (-1, 1):
        pass
    ring_out.append((y, z))
verts = {}
for s, (y, z) in enumerate(ring_out):
    for k, x in enumerate((-W / 2, -W / 2 + .03, W / 2 - .03, W / 2)):
        # shell: outer surface plus a slight inward lip at both sides
        zz = z - (.03 if k in (0, 3) else 0)
        verts[(s, k)] = bm.verts.new((x, y, zz))
        verts[(s, k, "in")] = bm.verts.new((x, y * .97, zz - .05))
for s in range(segs):
    for k in range(3):
        a, b = verts[(s, k)], verts[(s, k + 1)]; c, d = verts[(s + 1, k + 1)], verts[(s + 1, k)]
        bm.faces.new((a, b, c, d))
        ai, bi = verts[(s, k, "in")], verts[(s, k + 1, "in")]; ci, di = verts[(s + 1, k + 1, "in")], verts[(s + 1, k, "in")]
        bm.faces.new((di, ci, bi, ai))
    for k in (0, 3):                           # side edges
        a, b = verts[(s, k)], verts[(s + 1, k)]; c, d = verts[(s + 1, k, "in")], verts[(s, k, "in")]
        bm.faces.new((a, b, c, d) if k == 0 else (d, c, b, a))
me = bpy.data.meshes.new("fender"); bm.to_mesh(me); bm.free()
fd = bpy.data.objects.new("Fender", me); bpy.context.collection.objects.link(fd)
bpy.context.view_layer.objects.active = fd; fd.select_set(True)
sub = fd.modifiers.new("s", "SUBSURF"); sub.levels = 1; bpy.ops.object.modifier_apply(modifier="s")
assign(fd, "paint")
shade_auto(fd, 50)
export(fd, "truck_fender", ["paint"])
