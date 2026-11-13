"""Round 125 (review 121, all three: the suds "popcorn clusters of white spheres", "clustered white spheres", "blobby sphere clumps";
most reviews since 112): foam_clump was a dome ringed by distinct metaballs, which at the basin read as balls of popcorn. Soap suds
are a soft mass of very many small bubbles: a low mound with an irregular, lobed outline, a gently rolling top and a flat foot where
it sits in the water. The many small bubbles are drawn on it as a fine cell texture (FoamClump.shader), not as separate balls.
- foam_mound: about 1.0 across and 0.24 high (lower than a meringue).
Run: blender -b --factory-startup -P round125.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise, Vector

reset(); random.seed(1250)
bpy.ops.mesh.primitive_uv_sphere_add(segments=64, ring_count=32, radius=.5)
o = bpy.context.active_object
s = Vector((3.1, 1.7, 5.3))
for v in o.data.vertices:
    p = v.co.copy()
    a = math.atan2(p.y, p.x)
    # An irregular, lobed outline (three or four soft lobes and a little wobble), never a circle.
    lobe = 1 + .14 * math.sin(a * 3 + 1.1) + .07 * math.sin(a * 5 + 2.3) + noise.noise(Vector((math.cos(a), math.sin(a), 0)) * 1.6 + s) * .12
    r = math.hypot(p.x, p.y) / .5
    h = p.z / .5   # -1 .. 1
    x, y = p.x * lobe, p.y * lobe
    if h <= 0:
        # The foot: pressed flat a little under the waterline.
        z = -.03
        x *= 1.04; y *= 1.04
    else:
        # A low mound, flatter than a dome (soft shoulders), with a gently rolling top of broad bumps.
        z = .24 * (h ** .8)
        z += noise.noise(Vector((x, y, 0)) * 3.2 + s) * .045 * h
        z += noise.noise(Vector((x, y, 0)) * 7.0 + s * 2) * .015 * h
    v.co = Vector((x, y, z))
assign(o, "foam")
shade_auto(o, 70)
export(o, "foam_mound", ["foam"])
