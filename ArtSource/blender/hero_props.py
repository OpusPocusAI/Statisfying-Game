"""Hero props and truck pass (owner, November 16: Tchia is the art target; no primitive assemblies).

The truck's body is one fused shell, but its details were still Unity cubes and cylinders stuck on: flat disc wheels with
stacked hub discs, box headlamp bezels, a slatted box grille, box tail lamps and mirrors, stick door handles, box steps,
stick steering spokes, a box skip plate and squashed-sphere cargo packing. Outside, the boardwalk was 20 boxes, its
fence six square posts with square rails, and the cove signs stood on square sticks. Every model here is original and
one fused surface (voxel-merged parts), sized to drop in where the old pieces stood:
- truck_wheel: a chunky balloon tyre with knobbly tread, a deep enamel dish, a domed cap and five bolts (axle +Z here).
- truck_headlamp (bug-eye pod with bezel and domed lens), truck_grille (lipped recess with rounded bars), truck_taillamp
  (rounded housing, coral and amber lenses), truck_marker (pill lens), truck_badge (round medallion), truck_mirror
  (bent arm into a rounded casing with a glass face), truck_handle (grab bar on standoffs), truck_step (ribbed tread on
  brackets), truck_steering (rim, three spokes and hub), truck_skip_plate (rounded plaque with a raised rim).
- cargo_straw and cargo_sack: low lumpy mounds of packing for the two cargo tubs.
- boardwalk_deck (20 bevelled boards with a little irregularity, on two joists), mooring_post (rounded timber with a
  domed cap and a rope lashing), rope_swag (two-strand rope hanging between posts), sign_post (chamfered timber).
Axes (Blender): Unity X = X, Unity Z (forward) = Y, Unity Y (up) = Z. Run: blender -b --factory-startup -P hero_props.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *


def obj_from(bm, name):
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(o)
    return o


def ellipsoid(c, r, s=(1, 1, 1), seg=24):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=seg, ring_count=seg // 2, radius=r, location=c)
    o = bpy.context.active_object; o.scale = s; bpy.ops.object.transform_apply(location=False, scale=True)
    return o


def rbox(size, c=(0, 0, 0), r=.01, rot=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(size=1, location=c, rotation=rot)
    o = bpy.context.active_object; o.scale = size; bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    md = o.modifiers.new("b", "BEVEL"); md.width = r; md.segments = 3
    bpy.context.view_layer.objects.active = o; bpy.ops.object.modifier_apply(modifier="b")
    return o


def cyl(r, depth, c=(0, 0, 0), rot=(0, 0, 0), v=32, r2=None):
    if r2 is None: bpy.ops.mesh.primitive_cylinder_add(vertices=v, radius=r, depth=depth, location=c, rotation=rot)
    else: bpy.ops.mesh.primitive_cone_add(vertices=v, radius1=r, radius2=r2, depth=depth, location=c, rotation=rot)
    o = bpy.context.active_object; bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return o


def torus(R, r, c=(0, 0, 0), rot=(0, 0, 0), scale=(1, 1, 1)):
    bpy.ops.mesh.primitive_torus_add(major_radius=R, minor_radius=r, location=c, rotation=rot, major_segments=48, minor_segments=16)
    o = bpy.context.active_object; o.scale = scale; bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    return o


def tube(pts, radii, sides=10):
    bm = bmesh.new(); rings = []; pts = [Vector(p) for p in pts]
    for i, p in enumerate(pts):
        t = (pts[min(i + 1, len(pts) - 1)] - pts[max(i - 1, 0)]).normalized()
        a = Vector((0, 0, 1)) if abs(t.z) < .9 else Vector((1, 0, 0))
        u = t.cross(a).normalized(); v = t.cross(u)
        rings.append([bm.verts.new(p + (u * math.cos(k / sides * math.tau) + v * math.sin(k / sides * math.tau)) * radii[i]) for k in range(sides)])
    for a, b in zip(rings, rings[1:]):
        for k in range(sides): bm.faces.new((a[k], a[(k + 1) % sides], b[(k + 1) % sides], b[k]))
    for ring, flip in ((rings[0], True), (rings[-1], False)):
        c = bm.verts.new(sum((v.co for v in ring), Vector()) / sides)
        for k in range(sides): f = (ring[k], ring[(k + 1) % sides], c); bm.faces.new(f[::-1] if flip else f)
    return obj_from(bm, "tube")


def fuse(parts, voxel, tris, name, smooth=0):
    o = join(parts, name)
    md = o.modifiers.new("r", "REMESH"); md.mode = "VOXEL"; md.voxel_size = voxel
    bpy.context.view_layer.objects.active = o; bpy.ops.object.modifier_apply(modifier="r")
    if smooth:
        sm = o.modifiers.new("s", "CORRECTIVE_SMOOTH"); sm.factor, sm.iterations = .5, smooth
        bpy.ops.object.modifier_apply(modifier="s")
    d = o.modifiers.new("d", "DECIMATE"); d.ratio = min(1.0, tris / max(1, len(o.data.polygons) * 2))
    bpy.ops.object.modifier_apply(modifier="d")
    for p in o.data.polygons: p.use_smooth = True
    return o


def paint(o, rule, names):
    for n in names: o.data.materials.append(mat(n))
    for p in o.data.polygons: p.material_index = rule(p.center, p.normal)
    return o


def rxy(c): return math.hypot(c.x, c.y)
def rxz(c): return math.hypot(c.x, c.z)


# ---------------------------------------------------------------- wheel (0.86 m across, 0.36 wide; axle along +Z, outer face +Z)
reset()
parts = [torus(.34, .088, scale=(1, 1, 2.05))]
for i in range(30):
    a = i / 30 * math.tau; off = .045 if i % 2 else -.045
    parts.append(rbox((.04, .05, .15), (math.cos(a) * .41, math.sin(a) * .41, off), .016, rot=(0, 0, a)))
parts.append(cyl(.27, .2, (0, 0, -.02)))                                     # rim barrel
parts.append(cyl(.215, .05, (0, 0, .085)))                                   # dish face
parts.append(ellipsoid((0, 0, .115), .088, (1, 1, .62)))                     # domed cap
for i in range(5):
    a = i / 5 * math.tau + .3
    parts.append(ellipsoid((math.cos(a) * .145, math.sin(a) * .145, .112), .02, (1, 1, .7), 12))
o = fuse(parts, .0065, 3600, "wheel", smooth=2)
bolts = [(math.cos(i / 5 * math.tau + .3) * .145, math.sin(i / 5 * math.tau + .3) * .145) for i in range(5)]
paint(o, lambda c, n: 0 if rxy(c) > .262 else (2 if (rxy(c) < .09 and c.z > .1) or any(math.hypot(c.x - bx, c.y - by) < .022 and c.z > .113 for bx, by in bolts) else 1), ["tyre", "hub", "metal"])
export(o, "truck_wheel", ["tyre", "hub", "metal"])

# ---------------------------------------------------------------- headlamp pod (faces +Y; back of the pod at y = 0)
reset()
parts = [cyl(.165, .1, (0, .05, 0), rot=(math.radians(90), 0, 0)), torus(.15, .024, (0, .1, 0), rot=(math.radians(90), 0, 0)),
         ellipsoid((0, .1, 0), .132, (1, .3, 1), 40)]
o = fuse(parts, .0035, 4000, "headlamp", smooth=3)
paint(o, lambda c, n: 2 if c.y > .108 and rxz(c) < .127 else (1 if c.y > .07 and rxz(c) > .12 else 0), ["body", "metal", "lens"])
export(o, "truck_headlamp", ["body", "metal", "lens"])

# ---------------------------------------------------------------- grille (faces +Y; centre at origin)
reset()
frame = rbox((.92, .07, .3), (0, 0, 0), .03)
cut = rbox((.8, .1, .19), (0, .04, 0), .02)
md = frame.modifiers.new("cut", "BOOLEAN"); md.operation = "DIFFERENCE"; md.object = cut; md.solver = "EXACT"
bpy.context.view_layer.objects.active = frame; bpy.ops.object.modifier_apply(modifier="cut")
bpy.data.objects.remove(cut)
parts = [frame] + [tube([(-.4, .005, z), (.4, .005, z)], [.017, .017], 10) for z in (-.066, -.022, .022, .066)]
o = fuse(parts, .004, 2600, "grille", smooth=1)
paint(o, lambda c, n: 0 if abs(c.x) < .395 and abs(c.z) < .092 and c.y < -.002 and n.y > .5 else 1, ["recess", "metal"])
export(o, "truck_grille", ["recess", "metal"])

# ---------------------------------------------------------------- tail lamp (faces -Y; housing centre at origin)
reset()
parts = [rbox((.24, .08, .27), (0, 0, 0), .035), rbox((.18, .04, .11), (0, -.04, .05), .02), rbox((.18, .04, .075), (0, -.04, -.068), .02)]
o = fuse(parts, .0035, 1800, "tail", smooth=1)
paint(o, lambda c, n: (1 if c.z > -.012 else 2) if c.y < -.045 and abs(c.x) < .085 else 0, ["housing", "coral", "amber"])
export(o, "truck_taillamp", ["housing", "coral", "amber"])

# ---------------------------------------------------------------- side marker pill (faces +Y)
reset()
o = fuse([rbox((.13, .05, .07), (0, 0, 0), .024)], .003, 500, "marker", smooth=1)
paint(o, lambda c, n: 0, ["amber"]); export(o, "truck_marker", ["amber"])

# ---------------------------------------------------------------- bonnet badge (faces +Y)
reset()
o = fuse([cyl(.07, .03, (0, 0, 0), rot=(math.radians(90), 0, 0)), torus(.066, .012, (0, .012, 0), rot=(math.radians(90), 0, 0))], .003, 900, "badge", smooth=1)
paint(o, lambda c, n: 1 if rxz(c) > .055 else 0, ["teal", "metal"]); export(o, "truck_badge", ["teal", "metal"])

# ---------------------------------------------------------------- mirror (left side; attaches at the origin, reaches -X)
reset()
parts = [tube([(0, 0, 0), (-.12, 0, .01), (-.22, 0, .04), (-.27, 0, .05)], [.02, .017, .016, .016], 10),
         rbox((.15, .07, .24), (-.31, 0, .05), .04)]
o = fuse(parts, .004, 1800, "mirror", smooth=1)
paint(o, lambda c, n: 2 if c.y < -.03 and abs(c.x + .31) < .055 and abs(c.z - .05) < .095 else (1 if c.x > -.24 else 0), ["casing", "metal", "glass"])
export(o, "truck_mirror", ["casing", "metal", "glass"])

# ---------------------------------------------------------------- door handle (left side; along Y, standing off -X)
reset()
parts = [tube([(-.035, -.09, 0), (-.045, 0, 0), (-.035, .09, 0)], [.016, .017, .016], 10),
         tube([(0, -.085, 0), (-.04, -.088, 0)], [.014, .013], 8), tube([(0, .085, 0), (-.04, .088, 0)], [.014, .013], 8)]
o = fuse(parts, .003, 900, "handle", smooth=1)
paint(o, lambda c, n: 0, ["metal"]); export(o, "truck_handle", ["metal"])

# ---------------------------------------------------------------- cab step (ribbed tread on brackets)
reset()
parts = [rbox((.28, .62, .045), (0, 0, 0), .015)] + [rbox((.03, .56, .02), (x, 0, .03), .009) for x in (-.08, 0, .08)]
parts += [rbox((.04, .05, .14), (.1, y, -.06), .012, rot=(0, math.radians(-20), 0)) for y in (-.22, .22)]
o = fuse(parts, .004, 1600, "step", smooth=1)
paint(o, lambda c, n: 0, ["metal"]); export(o, "truck_step", ["metal"])

# ---------------------------------------------------------------- steering wheel (in the XZ plane, faces -Y toward the driver)
reset()
parts = [torus(.24, .026, rot=(math.radians(90), 0, 0))]
for a in (90, 210, 330):
    r = math.radians(a)
    parts.append(tube([(0, .01, 0), (math.cos(r) * .12, .02, math.sin(r) * .12), (math.cos(r) * .23, .0, math.sin(r) * .23)], [.017, .014, .015], 8))
parts.append(cyl(.055, .06, (0, .02, 0), rot=(math.radians(90), 0, 0)))
parts.append(ellipsoid((0, -.012, 0), .05, (1, .45, 1)))
o = fuse(parts, .0045, 2600, "steering", smooth=1)
paint(o, lambda c, n: 2 if rxz(c) < .062 else (1 if rxz(c) < .21 else 0), ["grip", "metal", "hub"])
export(o, "truck_steering", ["grip", "metal", "hub"])

# ---------------------------------------------------------------- skip plate (rounded plaque with a raised rim on both faces)
reset()
parts = [rbox((.72, .07, .3), (0, 0, 0), .03)]
for y in (-.035, .035):
    parts += [tube([(-.32, y, -.115), (.32, y, -.115)], [.014, .014], 8), tube([(-.32, y, .115), (.32, y, .115)], [.014, .014], 8),
              tube([(-.32, y, -.115), (-.32, y, .115)], [.014, .014], 8), tube([(.32, y, -.115), (.32, y, .115)], [.014, .014], 8)]
o = fuse(parts, .004, 1600, "skip", smooth=1)
paint(o, lambda c, n: 0, ["chalk"]); export(o, "truck_skip_plate", ["chalk"])


# ---------------------------------------------------------------- cargo packing mounds (0.8 x 1.42 m, about 0.1 high)
def mound(name, seed, folds):
    reset(); rnd = random.Random(seed); bm = bmesh.new(); n, m = 26, 44; grid = []
    from mathutils import noise
    for i in range(n + 1):
        row = []
        for j in range(m + 1):
            u, v = i / n * 2 - 1, j / m * 2 - 1
            e = min(1.0, (abs(u) ** 4 + abs(v) ** 4) ** .25)            # squircle: fills the tub's rounded corners
            h = .085 * (1 - e ** 6) - .03
            x, y = u * .4, v * .71
            if folds: h += .022 * math.sin(x * 26 + math.sin(y * 7) * 2) * (1 - e) + .03 * noise.noise(Vector((x * 3, y * 3, seed)))
            else: h += .028 * noise.noise(Vector((x * 9, y * 9, seed))) * (1 - e ** 3) + .012 * noise.noise(Vector((x * 30, y * 30, seed)))
            row.append(bm.verts.new((x, y, h)))
        grid.append(row)
    for i in range(n):
        for j in range(m): bm.faces.new((grid[i][j], grid[i + 1][j], grid[i + 1][j + 1], grid[i][j + 1]))
    o = obj_from(bm, name)
    for p in o.data.polygons: p.use_smooth = True
    paint(o, lambda c, nn: 0, ["packing"]); export(o, name, ["packing"])


mound("cargo_straw", 3, False)
mound("cargo_sack", 7, True)

# ---------------------------------------------------------------- boardwalk deck (5 m wide, 20 boards 0.85 m apart along +Y)
reset(); rnd = random.Random(31); boards = []
for i in range(20):
    b = rbox((5 + rnd.uniform(-.12, .08), .78, .13), (rnd.uniform(-.05, .05), i * .85, .045), .025,
             rot=(math.radians(rnd.uniform(-.7, .7)), math.radians(rnd.uniform(-.5, .5)), math.radians(rnd.uniform(-.6, .6))))
    b.data.materials.append(mat("board" if i % 3 else "board dark")); boards.append(b)
for x in (-1.8, 1.8):
    j = rbox((.14, 16.9, .14), (x, 8.07, -.09), .02); j.data.materials.append(mat("board dark")); boards.append(j)
o = join(boards, "deck")
for p in o.data.polygons: p.use_smooth = False
export(o, "boardwalk_deck", ["board", "board dark"])

# ---------------------------------------------------------------- mooring post (1.3 m, origin on the ground)
reset()
parts = [cyl(.12, 1.18, (0, 0, .59), v=12, r2=.11), ellipsoid((0, 0, 1.2), .125, (1, 1, .55), 16), torus(.115, .016, (0, 0, 1.08))]
for k in range(3): parts.append(torus(.128, .022, (0, 0, .9 + k * .04)))
o = fuse(parts, .008, 2400, "post", smooth=2)
paint(o, lambda c, n: 1 if .86 < c.z < 1.0 and rxy(c) > .118 else 0, ["post", "rope"])
export(o, "mooring_post", ["post", "rope"])

# ---------------------------------------------------------------- rope swag (two twisted strands, 3 m between posts along +Y)
reset(); parts = []
for s in (0, math.pi):
    pts, rad = [], []
    for k in range(61):
        t = k / 60; y = t * 3; sag = -.2 * 4 * t * (1 - t)
        a = t * 3 * 14 + s
        pts.append((math.cos(a) * .012, y, .95 + sag + math.sin(a) * .012)); rad.append(.018)
    parts.append(tube(pts, rad, 8))
o = fuse(parts, .006, 1600, "rope", smooth=1)
paint(o, lambda c, n: 0, ["rope"]); export(o, "rope_swag", ["rope"])

# ---------------------------------------------------------------- sign post (chamfered timber, centre at origin, 1.6 m)
reset()
parts = [rbox((.15, .15, 1.5), (0, 0, -.05), .025), cyl(.106, .12, (0, 0, .76), v=4, r2=.01)]
parts[1].rotation_euler = (0, 0, math.radians(45)); bpy.context.view_layer.objects.active = parts[1]; bpy.ops.object.transform_apply(rotation=True)
o = fuse(parts, .006, 900, "signpost", smooth=1)
for p in o.data.polygons: p.use_smooth = False
paint(o, lambda c, n: 0, ["wood"]); export(o, "sign_post", ["wood"])
