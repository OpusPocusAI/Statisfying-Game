"""Beach set: faceted rocks, soft dig mounds and small shoreline details.
Run: blender -b --factory-startup -P beach_assets.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise


def normalize_to(obj, half_extent=0.47, base_zero=False):
    """Scale/centre so the object fits a box of +-half_extent (or base at z=0 with height 1)."""
    import numpy as np
    vs = [v.co.copy() for v in obj.data.vertices]
    mn = Vector((min(v.x for v in vs), min(v.y for v in vs), min(v.z for v in vs)))
    mx = Vector((max(v.x for v in vs), max(v.y for v in vs), max(v.z for v in vs)))
    size = mx - mn
    centre = (mx + mn) / 2
    for v in obj.data.vertices:
        p = v.co - centre
        if base_zero:
            v.co = Vector((p.x / size.x * 2.0, p.y / size.y * 2.0, (v.co.z - mn.z) / size.z))
        else:
            v.co = Vector((p.x / size.x * half_extent * 2, p.y / size.y * half_extent * 2, p.z / size.z * half_extent * 2))


# ---------------------------------------------------------------- faceted rocks
# Chunky, flat-shaded rock masses (Lonely Mountains: Downhill style): a displaced sphere,
# decimated into broad planes, flattened underneath.
for k in range(4):
    reset()
    random.seed(100 + k)
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=4, radius=1)
    rock = bpy.context.active_object
    seed = Vector((k * 3.7, k * 1.3, k * 2.1))
    stretch = Vector((random.uniform(1.0, 1.4), random.uniform(0.8, 1.1), random.uniform(0.6, 0.85)))
    for v in rock.data.vertices:
        p = v.co.copy()
        d = noise.fractal(p * 1.3 + seed, 0.6, 2.0, 3) * 0.32 + noise.noise(p * 3.1 + seed) * 0.08
        v.co = p * (1 + d)
        v.co.x *= stretch.x; v.co.y *= stretch.y; v.co.z *= stretch.z
        if v.co.z < -0.35:
            v.co.z = -0.35 + (v.co.z + 0.35) * 0.15   # flat-ish base that sits on the ground
    dec = rock.modifiers.new("dec", "DECIMATE")
    dec.ratio = 0.12
    bpy.context.view_layer.objects.active = rock
    bpy.ops.object.modifier_apply(modifier="dec")
    normalize_to(rock)
    rock.data.materials.append(mat("rock"))
    # Flat shading: every face its own plane.
    for p in rock.data.polygons:
        p.use_smooth = False
    export(rock, "faceted_rock_%d" % k, ["rock"])

# ---------------------------------------------------------------- soft dig mounds
# Base at z=0, height 1, radius ~1 (matches the existing mound transforms).
for k in range(4):
    reset()
    random.seed(200 + k)
    seed = Vector((k * 5.1, k * 2.7, 0))
    ang = random.uniform(0, math.tau)
    dirn = Vector((math.cos(ang), math.sin(ang), 0))
    # Radial mesh: the outer ring lies flat on the ground, so the mound melts into the sand.
    rings, segs = 9, 30
    bm = bmesh.new()
    centre = bm.verts.new((0, 0, 1))
    grid = []
    for ri in range(1, rings + 1):
        row = []
        for si in range(segs):
            a = si / segs * math.tau
            wob = 1 + noise.noise(Vector((math.cos(a) * 1.4, math.sin(a) * 1.4, k * 1.7))) * 0.16
            r = ri / rings * wob
            p = Vector((math.cos(a) * r, math.sin(a) * r, 0))
            t = 1 - ri / rings
            h = t * t * (3 - 2 * t)                                         # soft shoulders, flat rim
            h *= 1 + 0.22 * p.dot(dirn)                                     # lopsided from the wind
            h -= math.exp(-((p - dirn * 0.4).length ** 2) / 0.05) * 0.12 * t  # a gentle scoop mark
            p.z = max(0.0, h)
            row.append(bm.verts.new(p))
        grid.append(row)
    for si in range(segs):
        bm.faces.new((centre, grid[0][si], grid[0][(si + 1) % segs]))
    for ri in range(rings - 1):
        for si in range(segs):
            a, b = grid[ri][si], grid[ri][(si + 1) % segs]
            c, d = grid[ri + 1][(si + 1) % segs], grid[ri + 1][si]
            bm.faces.new((a, d, c, b))
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    me = bpy.data.meshes.new("mound"); bm.to_mesh(me); bm.free()
    m = bpy.data.objects.new("mound", me); bpy.context.collection.objects.link(m)
    m.data.materials.append(mat("sand"))
    for p in m.data.polygons:
        p.use_smooth = True
    normalize_to(m, base_zero=True)
    export(m, "sand_mound_%d" % k, ["sand"])

# ---------------------------------------------------------------- shoreline details (true size, metres)
reset()
random.seed(300)
# Scallop shell: a ribbed fan with a little hinge.
bpy.ops.mesh.primitive_circle_add(vertices=40, radius=0.06, fill_type="TRIFAN")
shell = bpy.context.active_object
for v in shell.data.vertices:
    a = math.atan2(v.co.y, v.co.x)
    r = v.co.length
    if v.co.y < -0.01 and r > 0.01:
        v.co *= 0.55
    rib = abs(math.sin(a * 9)) * 0.004 * (r / 0.06)
    v.co.z = (1 - (r / 0.06) ** 2) * 0.018 + rib
bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=2); bpy.ops.object.mode_set(mode="OBJECT")
sol = shell.modifiers.new("t", "SOLIDIFY"); sol.thickness = 0.003
assign(shell, "shell")
shade_auto(shell, 60)
export(shell, "scallop_shell", ["shell"])

reset()
# Spiral whelk: stacked shrinking rings along a helix.
parts = []
for i in range(9):
    t = i / 8
    r = 0.022 * (1 - t * 0.85)
    ang = t * math.tau * 2.2
    o = uv_sphere("w", r, (math.cos(ang) * r * 0.4, math.sin(ang) * r * 0.4, t * 0.06), (1, 1, 0.8), 16, 8, "shell")
    parts.append(o)
lip = uv_sphere("lip", 0.016, (0.012, 0, 0.004), (1.2, 0.8, 0.6), 16, 8, "shell_inner")
parts.append(lip)
whelk = join(parts, "Whelk")
whelk.rotation_euler = (math.radians(80), 0, 0)
bpy.ops.object.transform_apply(rotation=True)
shade_auto(whelk, 60)
export(whelk, "whelk_shell", ["shell", "shell_inner"])

reset()
# Starfish: five tapered arms.
bpy.ops.mesh.primitive_circle_add(vertices=60, radius=1, fill_type="TRIFAN")
star = bpy.context.active_object
for v in star.data.vertices:
    a = math.atan2(v.co.y, v.co.x)
    r = v.co.length
    arm = 0.45 + 0.55 * (0.5 + 0.5 * math.cos(a * 5)) ** 2.5
    v.co.x = math.cos(a) * r * arm * 0.07
    v.co.y = math.sin(a) * r * arm * 0.07
    v.co.z = (1 - r) * 0.012 * arm + 0.002
bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=1); bpy.ops.object.mode_set(mode="OBJECT")
sol = star.modifiers.new("t", "SOLIDIFY"); sol.thickness = 0.004
assign(star, "starfish")
shade_auto(star, 70)
export(star, "starfish", ["starfish"])

reset()
# Pebble cluster: a few smooth stones of different sizes. Round 34: four more variants in round34.py.
parts = []
for i in range(5):
    r = random.uniform(0.02, 0.05)
    p = uv_sphere("p", r, (random.uniform(-0.08, 0.08), random.uniform(-0.08, 0.08), r * 0.45), (1.3, 1.0, 0.55), 14, 8, "pebble_%d" % (i % 2))
    parts.append(p)
peb = join(parts, "Pebbles")
shade_auto(peb, 80)
export(peb, "pebble_cluster", ["pebble_0", "pebble_1"])

reset()
# Kelp strand: a flat, wavy ribbon lying on the sand.
pts = [(i * 0.06, math.sin(i * 0.9) * 0.05, 0.004) for i in range(9)]
kelp = tube_along("kelp", pts, 0.022, "kelp", resolution=8, bevel_resolution=1)
kelp.scale = (1, 1, 0.12)
bpy.ops.object.transform_apply(scale=True)
shade_auto(kelp, 60)
export(kelp, "kelp_strand", ["kelp"])
