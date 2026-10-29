"""Round 14 models (after the twelfth independent review): four more tumbled sea-glass silhouettes, so the
sorted trays are not rows of identical lentils.
- glass_shape_0: a rounded triangular shard (the classic bottle-shoulder piece)
- glass_shape_1: a long sliver
- glass_shape_2: a thick, chunky nugget
- glass_shape_3: a thin flat oval
Same conventions as glass_pebble_*: about 1.0 across in Blender XZ, thickness along Blender Y (Unity Z),
origin at the centre. Run: blender -b --factory-startup -P round14.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise


def polygon_radius(a, sides, round_k):
    """Radius of a rounded regular polygon (1 = circumscribed) at angle a; round_k 0 = sharp, 1 = circle."""
    if sides == 0:
        return 1.0
    seg = math.tau / sides
    x = (a % seg) - seg / 2
    r = math.cos(seg / 2) / math.cos(x)
    return r * (1 - round_k) + round_k * math.cos(seg / 2) * 1.05


# (sides, roundness, stretch x, stretch z, thickness, flat back)
shapes = [(3, .35, 1.0, .9, .3, .8), (0, 1, 1.0, .38, .26, .85), (5, .55, .82, .72, .4, .9), (0, 1, 1.0, .74, .16, .9)]
for k, (sides, rk, sx, sz, th, back) in enumerate(shapes):
    reset(); random.seed(1400 + k)
    bpy.ops.mesh.primitive_uv_sphere_add(segments=32, ring_count=16, radius=.5)
    o = bpy.context.active_object
    s = Vector((k * 2.3, k * .7, k * 3.1))
    rot = random.uniform(0, math.tau)
    for v in o.data.vertices:
        p = v.co.copy()
        a = math.atan2(p.z, p.x) + rot
        lump = 1 + noise.noise(Vector((math.cos(a) * 1.6, math.sin(a) * 1.6, 0)) + s) * .12
        pr = polygon_radius(a, sides, rk)
        v.co.x = p.x * sx * pr * lump
        v.co.z = p.z * sz * pr * lump
        v.co.y = p.y * th * (back if p.y > 0 else 1.0)
        v.co.y += noise.noise(p * 3.4 + s) * .02 * (1 if p.y < 0 else .6)
    dec = o.modifiers.new("d", "DECIMATE"); dec.ratio = .6; bpy.ops.object.modifier_apply(modifier="d")
    assign(o, "glass")
    shade_auto(o, 35)
    export(o, "glass_shape_%d" % k, ["glass"])
