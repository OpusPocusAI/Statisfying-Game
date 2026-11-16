"""Soft dig mounds (owner, November 16; second independent review, all three: the dig piles read as "flat biscuits", "cookie
discs with blobs stuck on"). sand_mound_0..3 are rebuilt under the same names, bounds and pivot (x, y in -1..1, z 0..1, origin
at the centre of the base), so every "Soft sand mound" (dig piles over buried finds and the beach's scattered mounds) updates
in place. Each is a heap of sand that feathers into the beach: a rounded crown, slopes that ease out to nothing at an irregular
outline (no rim), a slight spill to one side and a little scoop texture on top, and a lip tucked a touch below the sand so the
edge never shows.
Run: blender -b --factory-startup -P soft_mounds.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise


def mound(k):
    reset(); rnd = random.Random(5100 + k)
    seed = Vector((k * 3.7, k * 1.3, 0))
    spill = rnd.uniform(0, math.tau); spill_amt = rnd.uniform(.12, .22)
    rings, segs = 16, 40
    bm = bmesh.new(); grid = []
    top = bm.verts.new((0, 0, 1.0))
    for i in range(1, rings + 1):
        t = i / rings; row = []
        for j in range(segs):
            a = j / segs * math.tau
            # irregular outline, pushed out a little toward the spill side
            edge = .86 + .1 * noise.noise(Vector((math.cos(a) * 1.3, math.sin(a) * 1.3, 0)) + seed) + spill_amt * max(0, math.cos(a - spill)) * .6
            edge = min(edge, .99)
            r = t * edge
            # a soft heap: rounded crown, slopes that ease out to zero (no rim)
            h = (math.cos(min(1, t) * math.pi * .5) ** 2.2)
            h *= 1 + .12 * noise.noise(Vector((math.cos(a) * r * 3, math.sin(a) * r * 3, 2)) + seed)      # lumpy, scooped
            h += .08 * math.exp(-((t - .55) ** 2) / .03) * max(0, math.cos(a - spill))              # spill shoulder
            if i == rings: h = -.03                                                                  # lip tucks just under the sand (deeper drew a crease)
            row.append(bm.verts.new((math.cos(a) * r, math.sin(a) * r, max(-.03, min(1, h)))))
        grid.append(row)
    for j in range(segs):
        bm.faces.new((top, grid[0][j], grid[0][(j + 1) % segs]))
    for i in range(rings - 1):
        for j in range(segs):
            a, b = grid[i][j], grid[i][(j + 1) % segs]; c, d = grid[i + 1][(j + 1) % segs], grid[i + 1][j]
            bm.faces.new((a, d, c, b))
    me = bpy.data.meshes.new("mound"); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new("mound", me); bpy.context.collection.objects.link(o)
    # keep the original bounds convention exactly (z 0..1 for the heap; the tucked lip dips just below 0)
    for p in o.data.polygons: p.use_smooth = True
    bpy.context.view_layer.objects.active = o
    o.data.materials.append(mat("sand"))
    # faces must point up: flip if the first face points down
    if o.data.polygons[0].normal.z < 0:
        bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.select_all(action="SELECT"); bpy.ops.mesh.flip_normals(); bpy.ops.object.mode_set(mode="OBJECT")
    export(o, "sand_mound_%d" % k, ["sand"])


for k in range(4):
    mound(k)
