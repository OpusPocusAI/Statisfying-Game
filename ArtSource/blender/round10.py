"""Round 10 models (bigger modelling work after the sixth independent review):
- truck_canopy: crowned canvas canopy with a scalloped, striped valance (replaces the flat roof slab).
  True size 2.15 (x) x 1.9 (depth) x ~0.3 (height); origin at its centre.
- cargo_rail_tube: rounded tube side rail (replaces the square cream frames). True size 0.05 x 1.6 x 0.55.
- palm_0..2: coastal palm, a second tree species: curved ringed trunk, drooping two-sided fronds, coconuts.
  About 5-6 m tall, base at the origin.
Run: blender -b --factory-startup -P round10.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise


# ---------------------------------------------------------------- truck canopy
reset()
W, D = 2.15, 1.9
bpy.ops.mesh.primitive_grid_add(x_subdivisions=16, y_subdivisions=14, size=1)
top = bpy.context.active_object
top.scale = (W, D, 1); bpy.ops.object.transform_apply(scale=True)
for v in top.data.vertices:                       # gentle crown
    u, w = v.co.x / (W / 2), v.co.y / (D / 2)
    v.co.z = .1 * (1 - u * u) * (1 - .4 * w * w) + .05
sol = top.modifiers.new("s", "SOLIDIFY"); sol.thickness = .04; bpy.ops.object.modifier_apply(modifier="s")
assign(top, "canvas")
for p in top.data.polygons: p.use_smooth = True
parts = [top]
# Scalloped valance around the edge: alternating coral / cream tabs.
mat("canvas"); mat("stripe")
n_x, n_y = 9, 8
def tab(cx, cy, along_x, k):
    w = (W if along_x else D) / (n_x if along_x else n_y)
    bpy.ops.mesh.primitive_cylinder_add(vertices=12, radius=w / 2, depth=.012, location=(cx, cy, .0),
                                        rotation=(math.radians(90), 0, 0) if along_x else (math.radians(90), 0, math.radians(90)))
    o = bpy.context.active_object
    for v in o.data.vertices:                     # keep the lower half: a rounded scallop hanging down
        if v.co.z > 0: v.co.z = 0
    o.scale = (1, 1, 1.3); bpy.ops.object.transform_apply(scale=True)
    assign(o, "stripe" if k % 2 else "canvas")
    return o
for i in range(n_x):
    x = -W / 2 + (i + .5) * W / n_x
    parts.append(tab(x, -D / 2, True, i)); parts.append(tab(x, D / 2, True, i + 1))
for j in range(n_y):
    y = -D / 2 + (j + .5) * D / n_y
    parts.append(tab(-W / 2, y, False, j)); parts.append(tab(W / 2, y, False, j + 1))
can = join(parts, "Canopy"); shade_auto(can, 50)
export(can, "truck_canopy", ["canvas", "stripe"])

# ---------------------------------------------------------------- rounded tube side rail
reset()
L, Hh = 1.6, .55
parts = [tube_along("top", [(0, -L / 2, Hh / 2 - .03), (0, 0, Hh / 2 - .02), (0, L / 2, Hh / 2 - .03)], .025, "rail", resolution=8, bevel_resolution=3)]
for y in (-L / 2 + .04, -L / 6, L / 6, L / 2 - .04):
    parts.append(cylinder("post", .02, Hh - .05, (0, y, -.02), vertices=10, material="rail"))
rail = join(parts, "Rail"); shade_auto(rail, 50)
export(rail, "cargo_rail_tube", ["rail"])

# ---------------------------------------------------------------- palms
def frond(base, yaw, droop, length, seed):
    """A drooping frond: a curved midrib with a band of leaflets, two-sided."""
    bm = bmesh.new()
    segs = 10
    rnd = random.Random(seed)
    prev = None
    for s in range(segs + 1):
        t = s / segs
        # midrib: up and out, then drooping down
        r = length * t
        z = base.z + length * (.35 * t - droop * t * t)
        x = base.x + math.cos(yaw) * r
        y = base.y + math.sin(yaw) * r
        width = .38 * math.sin(math.pi * min(1, t * 1.1)) * (1 - .3 * t) + .02
        side = Vector((-math.sin(yaw), math.cos(yaw), 0))
        # leaflets fold down along the midrib (V-shaped section)
        l = bm.verts.new(Vector((x, y, z)) - side * width + Vector((0, 0, -.08 * width / .38)))
        m = bm.verts.new(Vector((x, y, z + .02)))
        rr = bm.verts.new(Vector((x, y, z)) + side * width + Vector((0, 0, -.08 * width / .38)))
        if prev:
            bm.faces.new((prev[0], prev[1], m, l)); bm.faces.new((prev[1], prev[2], rr, m))
        prev = (l, m, rr)
    me = bpy.data.meshes.new("frond"); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new("frond", me); bpy.context.collection.objects.link(o)
    # serrate the leaflet edge a little
    for v in o.data.vertices:
        v.co.z += noise.noise(v.co * 7 + Vector((seed, 0, 0))) * .03
    assign(o, "frond")
    return o

for k in range(3):
    reset(); random.seed(900 + k)
    H = random.uniform(4.6, 5.8)
    lean = random.uniform(.6, 1.3)
    pts = [(0, 0, 0), (lean * .15, 0, H * .35), (lean * .55, 0, H * .7), (lean, 0, H)]
    parts = [tube_along("trunk", pts, .16, "bark", resolution=12, bevel_resolution=3)]
    # ring bands up the trunk
    for i in range(1, 12):
        t = i / 12
        # point on the quadratic-ish curve (approximate with the control polyline)
        seg = min(2, int(t * 3)); u = t * 3 - seg
        a, b = Vector(pts[seg]), Vector(pts[seg + 1]); p = a.lerp(b, u)
        bpy.ops.mesh.primitive_torus_add(major_radius=.165 - .03 * t, minor_radius=.018, major_segments=14, minor_segments=5, location=tuple(p))
        ring = bpy.context.active_object; assign(ring, "bark_ring"); parts.append(ring)
    top = Vector(pts[-1])
    parts.append(uv_sphere("crown", .22, tuple(top + Vector((0, 0, .05))), (1, 1, .8), 10, 6, "bark_ring"))
    for c in range(3):                              # coconuts
        a = c * 2.1 + .4
        parts.append(uv_sphere("nut", .1, tuple(top + Vector((math.cos(a) * .17, math.sin(a) * .17, -.12))), (1, 1, 1.1), 10, 6, "nut"))
    nf = random.randint(9, 11)
    for f in range(nf):
        yaw = f / nf * math.tau + random.uniform(-.2, .2)
        parts.append(frond(top + Vector((0, 0, .1)), yaw, random.uniform(.55, .85), random.uniform(1.9, 2.5), k * 30 + f))
    palm = join(parts, "Palm")
    for p in palm.data.polygons: p.use_smooth = False
    export(palm, "palm_%d" % k, ["bark", "bark_ring", "frond", "nut"])
