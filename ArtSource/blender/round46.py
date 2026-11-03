"""Round 46 models (after the forty-second review; every reviewer, every review: the truck "reads as boxes stuck
together, with a canopy fringe of discs", and the bushes are "piles of balls"; the owner's rule is no primitive
assemblies). All three are rebuilt as ONE fused surface each, coloured by region:
- truck_shell: the truck's bodywork in one piece: lower body, cab sides with the doors, a crowned bonnet, wheel arches
  that flare out of the body, and rounded front and rear bumpers, with the wheel wells cut out. Colour regions: teal
  body, teal-green doors, ivory bonnet, arches, bumpers and belt line, and dark door seams. In the truck's local frame
  ("Sprung body": origin at the truck centre on the axle line, 0.3 m above the ground); Blender X = Unity x, Blender
  Y = Unity z (forward), Blender Z = Unity y.
- truck_canopy: the crowned canvas roof with ONE continuous scalloped valance (it was a row of separate discs),
  striped coral / cream by scallop. Same size and origin as before.
- coastal_shrub_leafy: one fused, leafy canopy (overlapping clumps voxel-merged, crinkled, smoothed) with leaves along
  the outline, lit on top and shaded underneath. Same footprint, slots and origin as before.
Run: blender -b --factory-startup -P round46.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise


def remesh(o, voxel, smooth=(.5, 6)):
    bpy.context.view_layer.objects.active = o
    rm = o.modifiers.new("fuse", "REMESH"); rm.mode = "VOXEL"; rm.voxel_size = voxel
    bpy.ops.object.modifier_apply(modifier="fuse")
    if smooth:
        sm = o.modifiers.new("soften", "CORRECTIVE_SMOOTH"); sm.factor, sm.iterations = smooth
        bpy.ops.object.modifier_apply(modifier="soften")


def reduce(o, ratio, protect):
    g = o.vertex_groups.new(name="keep")
    for v in o.data.vertices:
        if protect(v.co): g.add([v.index], 1.0, "REPLACE")
    d = o.modifiers.new("reduce", "DECIMATE"); d.ratio = ratio
    d.vertex_group = "keep"; d.invert_vertex_group = True; d.vertex_group_factor = 10
    bpy.context.view_layer.objects.active = o
    bpy.ops.object.modifier_apply(modifier="reduce")


def rounded_box(name, x0, x1, y0, y1, z0, z1, r):
    o = cube(name, (x1 - x0, y1 - y0, z1 - z0), ((x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2))
    bevel(o, r, segments=4, angle=80)
    bpy.context.view_layer.objects.active = o
    for m in list(o.modifiers): bpy.ops.object.modifier_apply(modifier=m.name)
    return o


# ================================================================ truck shell
reset()
parts = []
# Lower body (the chassis), cab sides with the doors, and the bonnet, overlapping so the voxel merge fuses them.
parts.append(rounded_box("lower", -1.0, 1.0, -2.1, 2.05, .45, .88, .1))
for (xa, xb) in ((.79, 1.0), (-1.0, -.79)):
    parts.append(rounded_box("cabside", xa, xb, -.2, 1.2, .78, 1.46, .05))
# Bonnet: a rounded mass with a crowned top.
bon = rounded_box("bonnet", -1.0, 1.0, .78, 1.98, .78, 1.38, .16)
for v in bon.data.vertices:
    if v.co.z > 1.2: v.co.z += .05 * (1 - (v.co.x / 1.0) ** 2)
parts.append(bon)
# Wheel arches flaring out of the body over each wheel.
wheels = [(sx * 1.04, wy, .45) for sx in (-1, 1) for wy in (-1.4, 1.4)]
for (wx, wy, wz) in wheels:
    pts = []
    for k in range(13):
        a = math.radians(8 + k * (164 / 12))
        pts.append((wx * .99, wy + math.cos(a) * .55, wz + math.sin(a) * .55))
    parts.append(tube_along("arch", pts, .1, None, resolution=16, bevel_resolution=4))
# Rounded bumpers front and rear, tucked into the body ends.
parts.append(tube_along("bumperF", [(-1.08, 2.1, .57), (-.6, 2.16, .57), (.6, 2.16, .57), (1.08, 2.1, .57)], .1, None, resolution=16, bevel_resolution=4))
parts.append(tube_along("bumperR", [(-1.02, -2.12, .55), (-.6, -2.16, .55), (.6, -2.16, .55), (1.02, -2.12, .55)], .085, None, resolution=16, bevel_resolution=4))
shell = join(parts, "TruckShell")
bpy.context.view_layer.objects.active = shell; shell.select_set(True)
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)   # regions below test world-frame positions
remesh(shell, .022, (.45, 6))
# Wheel wells: cut a cylinder out of the outer body around each wheel so the wheels sit inside the arches.
for (wx, wy, wz) in wheels:
    bpy.ops.mesh.primitive_cylinder_add(vertices=40, radius=.47, depth=.7, location=(wx + (.1 if wx > 0 else -.1), wy, wz), rotation=(0, math.radians(90), 0))
    cut = bpy.context.active_object
    b = shell.modifiers.new("well", "BOOLEAN"); b.operation = "DIFFERENCE"; b.object = cut; b.solver = "EXACT"
    bpy.context.view_layer.objects.active = shell; bpy.ops.object.modifier_apply(modifier="well")
    bpy.data.objects.remove(cut)

shell.data.materials.clear()
for n in ("body", "door", "ivory", "seam"): shell.data.materials.append(mat(n))


def region(c):
    x, y, z = c.x, c.y, c.z
    ax = abs(x)
    # Ivory: bonnet, bumpers, the belt line along the cab sides, and the arches around each wheel.
    if y > .8 and z > .84: return 2
    if abs(y) > 2.04 and z < .74: return 2
    if ax > .9 and -.2 < y < 1.2 and 1.36 < z < 1.44: return 2
    for (wx, wy, wz) in wheels:
        d = math.hypot(y - wy, z - wz)
        if .44 < d < .68 and z > wz - .05 and ax > 1.005: return 2   # only the flare standing proud of the body side
    # Door seams: a thin dark outline on each cab side.
    if ax > .9 and .8 < z < 1.36 and (abs(y + .14) < .018 or abs(y - 1.14) < .018 or abs(z - .84) < .014): return 3
    # Doors: the cab sides.
    if ax > .9 and -.14 < y < 1.14 and .84 < z < 1.36: return 1
    return 0


for p in shell.data.polygons:
    p.material_index = region(Vector(p.center))
    p.use_smooth = True


def near_region_edge(c):
    ax = abs(c.x)
    if ax > .85 and (abs(c.y + .14) < .05 or abs(c.y - 1.14) < .05 or abs(c.z - .84) < .05 or abs(c.z - 1.4) < .06): return True
    if c.y > .74 and c.z > .8 and abs(c.y - .8) < .05: return True
    for (wx, wy, wz) in wheels:
        d = math.hypot(c.y - wy, c.z - wz)
        if abs(d - .44) < .05 or abs(d - .68) < .05: return True
    return False


reduce(shell, .17, near_region_edge)   # about 20k tris
export(shell, "truck_shell", ["body", "door", "ivory", "seam"])

# ================================================================ truck canopy with one continuous valance
reset()
W, D = 2.15, 1.9
bpy.ops.mesh.primitive_grid_add(x_subdivisions=16, y_subdivisions=14, size=1)
top = bpy.context.active_object
top.scale = (W, D, 1); bpy.ops.object.transform_apply(scale=True)
for v in top.data.vertices:
    u, w = v.co.x / (W / 2), v.co.y / (D / 2)
    v.co.z = .1 * (1 - u * u) * (1 - .4 * w * w) + .05
sol = top.modifiers.new("s", "SOLIDIFY"); sol.thickness = .04
bpy.context.view_layer.objects.active = top; bpy.ops.object.modifier_apply(modifier="s")
assign(top, "canvas")
for p in top.data.polygons: p.use_smooth = True
mat("canvas"); mat("stripe")
# Perimeter path (counter-clockwise), with a scallop lobe every ~0.24 m along it.
perim = [(-W / 2, -D / 2), (W / 2, -D / 2), (W / 2, D / 2), (-W / 2, D / 2)]
lengths = [W, D, W, D]
total = sum(lengths)
lobes = 34
lobe_len = total / lobes
steps = lobes * 10
verts, faces, lobe_of = [], [], []
for i in range(steps + 1):
    s = i / steps * total
    acc = 0
    for k in range(4):
        if s <= acc + lengths[k] + 1e-6 or k == 3:
            t = (s - acc) / lengths[k]
            a, b = perim[k], perim[(k + 1) % 4]
            x, y = a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t
            break
        acc += lengths[k]
    f = (s / lobe_len) % 1.0
    drop = .07 + .085 * (1 - (2 * f - 1) ** 2) ** .5     # a rounded lobe hanging from the roof edge
    flare = 1.02 + .012 * (1 - (2 * f - 1) ** 2)
    verts.append((x * flare, y * flare, .05))
    verts.append((x * (flare + .01), y * (flare + .01), .05 - drop))
for i in range(steps):
    faces.append((2 * i, 2 * i + 2, 2 * i + 3, 2 * i + 1))
    lobe_of.append(int(((i + .5) / steps * total) / lobe_len))
me = bpy.data.meshes.new("valance"); me.from_pydata(verts, [], faces); me.update()
val = bpy.data.objects.new("valance", me); bpy.context.collection.objects.link(val)
val.data.materials.append(mat("canvas")); val.data.materials.append(mat("stripe"))
for p, lb in zip(val.data.polygons, lobe_of):
    p.material_index = lb % 2
    p.use_smooth = True
sol = val.modifiers.new("s", "SOLIDIFY"); sol.thickness = .012
bpy.context.view_layer.objects.active = val; bpy.ops.object.modifier_apply(modifier="s")
can = join([top, val], "Canopy"); shade_auto(can, 50)
export(can, "truck_canopy", ["canvas", "stripe"])

# ================================================================ fused leafy shrub
random.seed(4600)
reset()
parts = []


def blob(c, r, h, mat_name="leaf_dark"):
    bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=3, radius=1, location=c)
    o = bpy.context.active_object
    o.scale = (r, r, h); bpy.ops.object.transform_apply(scale=True)
    assign(o, mat_name); parts.append(o)


blob((0, 0, .3), .52, .34)
for i in range(7):
    a = i / 7 * math.tau + random.uniform(-.25, .25); rr = random.uniform(.36, .5)
    blob((math.cos(a) * rr, math.sin(a) * rr, random.uniform(.2, .3)), random.uniform(.24, .3), random.uniform(.2, .26))
for i in range(4):
    a = i / 4 * math.tau + random.uniform(-.4, .4); rr = random.uniform(.05, .22)
    blob((math.cos(a) * rr, math.sin(a) * rr, random.uniform(.48, .58)), random.uniform(.22, .28), random.uniform(.17, .21))
sh = join(parts, "Shrub")
bpy.context.view_layer.objects.active = sh; sh.select_set(True)
bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)
remesh(sh, .04, (.85, 24))   # round 46b: one smooth mass, the lumps melted together (they still read as separate balls)
# Crinkle the fused canopy into leafy rosettes: bumps along the normal, stronger on top, none at the ground.
sh.data.update()
for v in sh.data.vertices:
    p = v.co
    k = noise.noise(p * 9.0 + Vector((3, 1, 7)))
    k2 = noise.noise(p * 19.0 + Vector((5, 9, 2)))
    amt = (.03 * k + .02 * k2) * min(1.0, max(0.0, (p.z - .04) / .2))
    v.co = p + v.normal * amt
sm = sh.modifiers.new("soften", "CORRECTIVE_SMOOTH"); sm.factor, sm.iterations = .35, 3
bpy.context.view_layer.objects.active = sh; bpy.ops.object.modifier_apply(modifier="soften")
reduce(sh, .12, lambda c: False)
# Leaves along the outline, rooted in the canopy, so the silhouette breaks into leaf tips instead of round lumps.
leaves = []
for i in range(72):
    a = random.uniform(0, math.tau); h = random.uniform(.18, .62)
    rad = .62 * math.sqrt(max(0.05, 1 - ((h - .3) / .42) ** 2))
    base = Vector((math.cos(a) * rad * .82, math.sin(a) * rad * .82, h))
    out = Vector((math.cos(a), math.sin(a), random.uniform(.5, 1.2))).normalized()   # angled up, like leaves, not thorns
    L = random.uniform(.15, .24); Wd = L * .62
    side = out.cross(Vector((0, 0, 1))).normalized()
    if side.length < .1: side = Vector((1, 0, 0))
    tip = base + out * L
    mid = base + out * (L * .45)
    q1 = base + out * (L * .25); q2 = base + out * (L * .7)
    vs = [tuple(base), tuple(q1 + side * Wd * .42), tuple(q2 + side * Wd * .38), tuple(tip), tuple(q2 - side * Wd * .38), tuple(q1 - side * Wd * .42)]   # a rounded, ovate leaf
    me = bpy.data.meshes.new("leaf"); me.from_pydata(vs, [], [(0, 1, 2, 3, 4, 5)]); me.update()
    lf = bpy.data.objects.new("leaf", me); bpy.context.collection.objects.link(lf)
    s2 = lf.modifiers.new("s", "SOLIDIFY"); s2.thickness = .006
    bpy.context.view_layer.objects.active = lf; bpy.ops.object.modifier_apply(modifier="s")
    assign(lf, "leaf_lit")
    leaves.append(lf)
sh.data.materials.clear(); sh.data.materials.append(mat("leaf_dark")); sh.data.materials.append(mat("leaf_lit"))
for p in sh.data.polygons:
    c = Vector(p.center)
    lit = c.z + noise.noise(c * 4.0 + Vector((2, 2, 2))) * .12 > .2 or p.normal.z > .05
    p.material_index = 1 if lit else 0
    p.use_smooth = True
full = join([sh] + leaves, "LeafyShrub")
export(full, "coastal_shrub_leafy", ["leaf_dark", "leaf_lit"])
