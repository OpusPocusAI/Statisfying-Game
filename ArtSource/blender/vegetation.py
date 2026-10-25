"""Coastal vegetation: windswept umbrella pines and two-tone dune grass tufts.
Run: blender -b --factory-startup -P vegetation.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise


def canopy_pad(name, centre, radius, height, seed):
    """A flattened, lumpy foliage cloud, faceted like the reference low-poly games."""
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=3, radius=1, location=(0, 0, 0))
    o = bpy.context.active_object; o.name = name
    s = Vector((seed, seed * 1.7, seed * .3))
    for v in o.data.vertices:
        p = v.co.copy()
        d = 1 + noise.fractal(p * 1.6 + s, .6, 2.0, 3) * .28
        v.co = Vector((p.x * radius * d, p.y * radius * d, p.z * height * d))
        if v.co.z < -height * .35:
            v.co.z = -height * .35 + (v.co.z + height * .35) * .3          # flatter underside
    dec = o.modifiers.new("dec", "DECIMATE"); dec.ratio = .45
    bpy.ops.object.modifier_apply(modifier="dec")
    o.location = centre
    bpy.ops.object.transform_apply(location=True)
    assign(o, "foliage")
    for p in o.data.polygons:
        p.use_smooth = False
    return o


for k in range(3):
    reset()
    random.seed(40 + k)
    lean = random.uniform(.35, .7)          # windswept towards +X (inland)
    h = random.uniform(4.6, 5.6)
    pts = [(0, 0, 0), (lean * .2, 0, h * .35), (lean * .65, random.uniform(-.2, .2), h * .7), (lean, 0, h * .92)]
    trunk = tube_along("trunk", pts, .24, "bark", resolution=10, bevel_resolution=3)   # round 5: sturdier (read as acacia when thin)
    for v in trunk.data.vertices:
        t = v.co.z / h
        c = Vector((0, 0, v.co.z))
        # taper towards the top
        # (scale radial offset from the spine approximately by height)
    trunk.data.update()
    parts = [trunk]
    # Two side branches up into the crown.
    for sgn in (-1, 1):
        parts.append(tube_along("branch", [pts[2], (lean * .65 + sgn * 1.1, sgn * .5, h * .9)], .08, "bark", resolution=6, bevel_resolution=2))
    # Crown: a broad, flat umbrella of 4-6 pads.
    top = Vector(pts[3])
    # Round 5: a fuller, two-tier crown (Mediterranean stone pine), not a single flat acacia plate.
    pads = [(top + Vector((0, 0, .45)), 2.2, 1.0), (top + Vector((.2, 0, 1.15)), 1.5, .8)]
    for i in range(random.randint(5, 7)):
        a = i / 4 * math.tau + random.uniform(-.4, .4)
        r = random.uniform(1.1, 1.8)
        pads.append((top + Vector((math.cos(a) * r + .3, math.sin(a) * r, random.uniform(-.45, .5))), random.uniform(1.1, 1.6), random.uniform(.6, .9)))
    for i, (c, r, hh) in enumerate(pads):
        parts.append(canopy_pad("pad", c, r, hh, k * 10 + i))
    tree = join(parts, "CoastalPine")
    export(tree, "coastal_pine_%d" % k, ["foliage", "bark"])

# ---------------------------------------------------------------- dune grass tuft
# Blades split into a dark base (submesh 0) and a pale, sunlit tip (submesh 1).
reset()
random.seed(77)
bm = bmesh.new()
for b in range(26):
    a = random.uniform(0, math.tau); r = random.uniform(0, .12)
    base = Vector((math.cos(a) * r, math.sin(a) * r, 0))
    lean = Vector((math.cos(a), math.sin(a), 0)) * random.uniform(.15, .45) + Vector((.18, 0, 0))   # wind
    height = random.uniform(.45, .85)
    width = random.uniform(.025, .04)
    side = Vector((-math.sin(a + 1.3), math.cos(a + 1.3), 0)) * width
    segs = 5
    prev = None
    for s in range(segs + 1):
        t = s / segs
        p = base + lean * t * t + Vector((0, 0, height * t))
        w = side * (1 - t) ** .8
        l_ = bm.verts.new(p - w); r_ = bm.verts.new(p + w)
        if prev:
            f = bm.faces.new((prev[0], prev[1], r_, l_))
            f.material_index = 0 if t <= .6 else 1
        prev = (l_, r_)
me = bpy.data.meshes.new("tuft"); bm.to_mesh(me); bm.free()
tuft = bpy.data.objects.new("tuft", me); bpy.context.collection.objects.link(tuft)
tuft.data.materials.append(mat("grass_base")); tuft.data.materials.append(mat("grass_tip"))
for p in tuft.data.polygons:
    p.use_smooth = True
bpy.context.view_layer.objects.active = tuft
export(tuft, "dune_grass_tuft", ["grass_base", "grass_tip"])
