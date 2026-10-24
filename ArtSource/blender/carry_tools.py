"""First-person carrying tools: enamel bucket, canvas sack and the finds they hold.
Sizes are nominal; the Unity pass fits each mesh to the bounds of the model it replaces.
Run: blender -b --factory-startup -P carry_tools.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise


def lathe(name, profile, segments, material):
    bm = bmesh.new(); rings = []
    for r, h in profile:
        rings.append([bm.verts.new((r * math.cos(a), r * math.sin(a), h)) for a in [i * math.tau / segments for i in range(segments)]])
    for a, b in zip(rings, rings[1:]):
        for i in range(segments):
            bm.faces.new((a[i], a[(i + 1) % segments], b[(i + 1) % segments], b[i]))
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(o)
    bpy.context.view_layer.objects.active = o; o.select_set(True)
    assign(o, material)
    return o


# ---------------------------------------------------------------- enamel bucket (open top, inside visible)
reset()
H = .26
# Outer wall up to the rim, rolled lip, then down the inside to the floor.
def wall(z):   # outer radius of the tapered wall at height z
    return .105 + (.155 - .105) * (z + H / 2 - .012) / (H - .024)
prof = [(0.0, -H / 2), (.098, -H / 2), (.105, -H / 2 + .012), (wall(-.035), -.035), (wall(.015), .015), (.155, H / 2 - .012), (.162, H / 2), (.158, H / 2 + .008),
        (.148, H / 2), (.142, H / 2 - .02), (.094, -H / 2 + .02), (0.0, -H / 2 + .02)]
body = lathe("bucket", prof, 40, "enamel")
# Painted band around the middle: the faces between two heights on the outside become "band".
body.data.materials.append(mat("band")); body.data.materials.append(mat("rim"))
for p in body.data.polygons:
    r = math.hypot(p.center.x, p.center.y); z = p.center.z
    expected = .105 + (.155 - .105) * (z + H / 2 - .012) / (H - .024)
    outside = r > expected - .004
    if outside and -.035 < z < .015: p.material_index = 1
    elif z > H / 2 - .016 and r > .14: p.material_index = 2
shade_auto(body, 40)
# Handle ears and a bail handle with a wooden grip.
parts = [body]
for s in (-1, 1):
    parts.append(cylinder("ear", .018, .012, (s * .158, 0, H / 2 - .035), rotation=(0, math.radians(90), 0), vertices=14, material="rim"))
parts.append(tube_along("bail", [(-.165, 0, H / 2 - .035), (-.15, 0, H / 2 + .12), (0, 0, H / 2 + .2), (.15, 0, H / 2 + .12), (.165, 0, H / 2 - .035)], .006, "rim", resolution=16, bevel_resolution=2))
parts.append(cylinder("grip", .016, .1, (0, 0, H / 2 + .2), rotation=(0, math.radians(90), 0), vertices=14, material="grip"))
bucket = join(parts, "EnamelBucket")
export(bucket, "enamel_bucket", ["enamel", "band", "rim", "grip"])

# ---------------------------------------------------------------- canvas sack (soft, bulging, open mouth)
reset()
H = .32
prof = []
for i in range(13):
    t = i / 12
    z = -H / 2 + t * H
    r = .12 + .07 * math.sin(math.pi * min(1, t * 1.15)) - .02 * t     # bulging belly, narrower mouth
    if i == 0: prof.append((0.0, z))
    prof.append((r, z))
prof += [(.13, H / 2 + .006), (.118, H / 2 - .015), (.11, -H / 2 + .03), (0.0, -H / 2 + .03)]
sack = lathe("sack", prof, 36, "canvas")
# Soft cloth: wrinkles and sag.
for v in sack.data.vertices:
    a = math.atan2(v.co.y, v.co.x); r = math.hypot(v.co.x, v.co.y)
    if r < 1e-4: continue
    k = 1 + noise.noise(Vector((math.cos(a) * 2.2, math.sin(a) * 2.2, v.co.z * 9))) * .06 + .025 * math.sin(a * 7 + v.co.z * 30)
    v.co.x *= k; v.co.y *= k
sack.data.materials.append(mat("trim")); sack.data.materials.append(mat("patch"))
for p in sack.data.polygons:
    if p.center.z > H / 2 - .03 and math.hypot(p.center.x, p.center.y) > .1: p.material_index = 1      # rolled hem
    elif p.normal.y < -.6 and -.06 < p.center.z < .05 and abs(p.center.x) < .06: p.material_index = 2  # front patch
shade_auto(sack, 60)
parts = [sack]
# Rope drawstring around the mouth and two carry straps.
parts.append(tube_along("rope", [(.135 * math.cos(a), .135 * math.sin(a), H / 2 - .02) for a in [i * math.tau / 16 for i in range(17)]], .008, "trim", resolution=4, bevel_resolution=2))
for s in (-1, 1):
    parts.append(tube_along("strap", [(-.1, s * .07, H / 2 - .02), (-.05, s * .07, H / 2 + .13), (.05, s * .07, H / 2 + .13), (.1, s * .07, H / 2 - .02)], .009, "trim", resolution=10, bevel_resolution=1))
bag = join(parts, "CanvasSack")
export(bag, "canvas_sack", ["canvas", "trim", "patch"])

# ---------------------------------------------------------------- contents: a sandy heap with visible finds
reset()
random.seed(5)
bpy.ops.mesh.primitive_uv_sphere_add(segments=28, ring_count=12, radius=1)
heap = bpy.context.active_object
for v in heap.data.vertices:
    if v.co.z < 0: v.co.z = 0
    else: v.co.z = v.co.z * .55 + noise.noise(v.co * 3) * .05
assign(heap, "sand")
parts = [heap]
glass = ["glass_a", "glass_b", "glass_c", "glass_d"]
for i in range(7):
    a = i / 7 * math.tau + random.uniform(-.3, .3); r = random.uniform(.15, .7)
    z = .55 * math.sqrt(max(0, 1 - r * r)) + .02
    g = uv_sphere("g", .17, (math.cos(a) * r, math.sin(a) * r, z), (1.3, .95, .45), 10, 6, glass[i % 4])
    g.rotation_euler = (random.uniform(-.4, .4), random.uniform(-.4, .4), random.uniform(0, 6))
    parts.append(g)
can = cylinder("can", .14, .42, (-.35, .2, .47), rotation=(math.radians(80), 0, math.radians(30)), vertices=16, material="can")
cap = cylinder("cap", .12, .05, (.4, -.35, .4), rotation=(math.radians(12), 0, 0), vertices=18, material="can")
bottle = cylinder("bottle", .11, .55, (.2, .35, .5), rotation=(math.radians(85), 0, math.radians(-50)), vertices=14, material="bottle")
parts += [can, cap, bottle]
contents = join(parts, "Contents")
shade_auto(contents, 50)
export(contents, "carry_contents", ["sand", "glass_a", "glass_b", "glass_c", "glass_d", "can", "bottle"])
