"""Layered rock formations (Lonely Mountains: Downhill / A Short Hike style): several blocky slabs
stacked into ledges, each one faceted, so tops catch the sun and sides fall into cool shadow.
Nominal size ~1 x 1 x 1 (base on z=0); the Unity pass scales them.
Run: blender -b --factory-startup -P rock_stacks.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise


def slab(cx, cy, z, sx, sy, h, seed):
    bpy.ops.mesh.primitive_cube_add(size=1, location=(cx, cy, z + h / 2))
    o = bpy.context.active_object
    o.scale = (sx, sy, h); bpy.ops.object.transform_apply(scale=True)
    bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=2); bpy.ops.object.mode_set(mode="OBJECT")
    s = Vector((seed * 1.3, seed * .7, seed * 2.1))
    for v in o.data.vertices:
        n = noise.noise(v.co * 3.1 + s)
        v.co.x += n * .06; v.co.y += noise.noise(v.co * 2.7 - s) * .06
        if v.co.z > z + h * .9: v.co.z += noise.noise(v.co * 4 + s) * .04          # uneven top
    md = o.modifiers.new("b", "BEVEL"); md.width = .035; md.segments = 1; md.limit_method = "ANGLE"; md.angle_limit = math.radians(40)
    bpy.ops.object.modifier_apply(modifier="b")
    dec = o.modifiers.new("d", "DECIMATE"); dec.ratio = .5
    bpy.ops.object.modifier_apply(modifier="d")
    o.rotation_euler = (random.uniform(-.06, .06), random.uniform(-.06, .06), random.uniform(-.5, .5))
    bpy.ops.object.transform_apply(rotation=True)
    assign(o, "rock")
    for p in o.data.polygons: p.use_smooth = False
    return o


for k in range(4):
    reset()
    random.seed(60 + k)
    parts = []
    z = 0.0
    w, d = random.uniform(.9, 1.1), random.uniform(.7, .95)
    ox = oy = 0.0
    layers = random.randint(3, 5)
    for i in range(layers):
        h = random.uniform(.16, .3) * (1.2 if i == 0 else 1)
        # each layer steps back and shifts, forming ledges
        parts.append(slab(ox, oy, z, w, d, h, k * 10 + i))
        if random.random() < .5 and i > 0:     # a loose side block on some ledges
            parts.append(slab(ox + w * .45, oy + random.uniform(-.2, .2), z, w * .35, d * .4, h * .9, k * 10 + i + 50))
        z += h * .92
        w *= random.uniform(.72, .95); d *= random.uniform(.75, .97)
        ox += random.uniform(-.22, .22); oy += random.uniform(-.18, .18)
    rock = join(parts, "RockStack")
    for p in rock.data.polygons: p.use_smooth = False
    export(rock, "rock_stack_%d" % k, ["rock"])
