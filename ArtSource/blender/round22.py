"""Round 22 models (after the twentieth review, three reviewers averaged):
- coastal_shrub_leafy: a layered coastal bush to replace coastal_shrub, which every review read as "faceted
  dark green rocks". Many small leafy clumps in three tiers (a shaded inner mass, a mid ring, sunlit top
  clumps), each clump lumpy with leaf-tip displacement and smooth-shaded so it reads as foliage, plus a few
  splayed leaf blades breaking the silhouette. Same footprint (~1.4 m across, ~0.8 m high) and the same two
  material slots (leaf_dark, leaf_lit), origin at the base centre.
Run: blender -b --factory-startup -P round22.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise

random.seed(2200)
reset()
parts = []


def clump(c, r, h, seed, mat):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=3, radius=1, location=c)
    o = bpy.context.active_object
    s = Vector((seed * 1.3, seed * .7, seed * 2.1))
    for v in o.data.vertices:
        p = v.co.copy()
        d = 1 + noise.noise(p * 2.2 + s) * .22 + max(0, noise.noise(p * 6.5 + s)) * .28   # leafy lumps
        v.co = Vector((p.x * r * d, p.y * r * d, p.z * h * d * (1.0 if p.z > 0 else .55)))
    o.location = c
    assign(o, mat)
    for poly in o.data.polygons:
        poly.use_smooth = True
    parts.append(o)


# Shaded inner mass.
clump((0, 0, .3), .5, .34, 1, "leaf_dark")
# Mid ring of clumps, alternating shade.
for i in range(7):
    a = i / 7 * math.tau + random.uniform(-.25, .25)
    rr = random.uniform(.38, .52)
    clump((math.cos(a) * rr, math.sin(a) * rr, random.uniform(.18, .3)), random.uniform(.22, .3), random.uniform(.18, .26), 10 + i, "leaf_dark" if i % 3 == 0 else "leaf_lit")
# Sunlit top clumps.
for i in range(4):
    a = i / 4 * math.tau + random.uniform(-.4, .4)
    rr = random.uniform(.05, .25)
    clump((math.cos(a) * rr, math.sin(a) * rr, random.uniform(.5, .62)), random.uniform(.2, .27), random.uniform(.16, .2), 30 + i, "leaf_lit")
# A few splayed leaf blades breaking the outline.
for i in range(9):
    a = random.uniform(0, math.tau)
    base = Vector((math.cos(a) * .35, math.sin(a) * .35, random.uniform(.25, .5)))
    tip = base + Vector((math.cos(a) * .3, math.sin(a) * .3, random.uniform(.15, .3)))
    bpy.ops.mesh.primitive_cone_add(vertices=4, radius1=.045, radius2=0, depth=(tip - base).length, location=(base + tip) / 2)
    blade = bpy.context.active_object
    blade.rotation_euler = (tip - base).to_track_quat('Z', 'Y').to_euler()
    blade.scale = (1, .25, 1)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    assign(blade, "leaf_lit")
    parts.append(blade)

sh = join(parts, "LeafyShrub")
export(sh, "coastal_shrub_leafy", ["leaf_dark", "leaf_lit"])
