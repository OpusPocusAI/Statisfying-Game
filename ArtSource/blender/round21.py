"""Round 21 models (after the nineteenth review, three reviewers averaged): six tumbled sea-glass pieces that
read as worn glass slabs rather than puffy candy. Every review called the glass "the same inflated lentil".
Real sea glass is a flattish piece of bottle wall: two near-flat faces (one slightly curved, from the
bottle), softly rounded edges, an irregular outline, and often one flatter worn facet or a broken edge.
- glass_slab_0..5. About 1.0 across in Blender XZ, thickness along Blender Y (Unity Z), origin at centre.
Run: blender -b --factory-startup -P round21.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise

# (outline sides 0 = oval, roundness, stretch x, stretch z, thickness, bottle curve, broken-edge cut)
shapes = [(0, 1, 1.0, .72, .2, .06, 0.0), (3, .6, 1.0, .88, .22, .05, .0), (4, .55, 1.0, .7, .18, .08, .32),
          (0, 1, 1.0, .5, .2, .04, .0), (5, .5, .9, .8, .24, .06, .28), (3, .45, 1.0, .82, .19, .07, .0)]


def outline_r(a, sides, rk):
    if sides == 0:
        return 1.0
    seg = math.tau / sides
    x = (a % seg) - seg / 2
    return (math.cos(seg / 2) / math.cos(x)) * (1 - rk) + rk * math.cos(seg / 2) * 1.05


for k, (sides, rk, sx, sz, th, curve, cut) in enumerate(shapes):
    reset(); random.seed(2100 + k)
    bpy.ops.mesh.primitive_uv_sphere_add(segments=36, ring_count=18, radius=.5)
    o = bpy.context.active_object
    s = Vector((k * 1.9, k * .6, k * 2.7))
    rot = random.uniform(0, math.tau)
    for v in o.data.vertices:
        p = v.co.copy()
        a = math.atan2(p.z, p.x) + rot
        rr = math.hypot(p.x, p.z) / .5            # 0 centre .. 1 at the equator
        lump = 1 + noise.noise(Vector((math.cos(a) * 1.7, math.sin(a) * 1.7, 0)) + s) * .1
        orr = outline_r(a, sides, rk) * lump
        x, z = p.x * sx * orr, p.z * sz * orr
        # Slab profile: flat faces with rounded edges (a superellipse in thickness, not a sphere).
        yn = p.y / .5
        ys = math.copysign(abs(yn) ** .62, yn) * .5 * th   # round 23: rounder edges that taper (was a flat-topped slab)
        # The face that was the bottle's outside keeps a gentle curve.
        ys += max(curve, .1) * (1 - (x * x + z * z) / .25) * (1 if yn > 0 else .55)   # round 23: a clearly domed top
        # A broken edge: one straight, slightly bevelled cut across the outline.
        if cut > 0:
            d = x * math.cos(rot) + z * math.sin(rot)
            lim = .5 - cut * .5
            if d > lim:
                x -= (d - lim) * math.cos(rot) * .92; z -= (d - lim) * math.sin(rot) * .92
        # Fine tumbled unevenness.
        ys += noise.noise(Vector((x, z, 0)) * 5 + s) * .012
        v.co = Vector((x, ys, z))
    assign(o, "glass")
    shade_auto(o, 50)
    export(o, "glass_slab_%d" % k, ["glass"])
