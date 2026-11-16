"""Ambient life pass (owner, November 16: Tchia is the art target).

A tropical coast is alive: seabirds wheeling over the water, crabs scuttling on the wet sand, butterflies over the flowers
and little fish darting in the clear shallows. All original designs, each one fused surface (voxel-merged parts, never
shapes stuck together), sized to animate in Unity by bending vertices (see Assets/Scripts/SeaGlass/AmbientCreature.cs):
- seabird: a slender tern-like bird, 1 m span, wings with a soft gull bend, pointed swept tips, forked tail.
- beach_crab: a rounded coral crab, 0.3 m across the legs, one big claw, eye stalks, four walking legs a side.
- butterfly: 9 cm span, fore and hind wings with a darker rim (thin, two-sided wings joined at the body).
- reef_fish: a 22 cm deep-bodied fish with a forked tail and a dorsal fin.
Axes (Blender): forward +Y, span +X, up +Z. Run: blender -b --factory-startup -P ambient_life.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *


def smoothstep(a, b, x):
    t = max(0.0, min(1.0, (x - a) / (b - a)))
    return t * t * (3 - 2 * t)


def obj_from(bm, name):
    me = bpy.data.meshes.new(name); bm.to_mesh(me); bm.free()
    o = bpy.data.objects.new(name, me); bpy.context.collection.objects.link(o)
    return o


def ellipsoid(c, r, s):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=14, radius=r, location=c)
    o = bpy.context.active_object; o.scale = s; bpy.ops.object.transform_apply(location=False, scale=True)
    return o


def tube(pts, radii, sides=8):
    """A closed tube through pts with per-point radii (one solid for the voxel merge)."""
    bm = bmesh.new(); rings = []
    pts = [Vector(p) for p in pts]
    for i, p in enumerate(pts):
        t = (pts[min(i + 1, len(pts) - 1)] - pts[max(i - 1, 0)]).normalized()
        a = Vector((0, 0, 1)) if abs(t.z) < .9 else Vector((1, 0, 0))
        u = t.cross(a).normalized(); v = t.cross(u)
        rings.append([bm.verts.new(p + (u * math.cos(k / sides * math.tau) + v * math.sin(k / sides * math.tau)) * radii[i]) for k in range(sides)])
    for a, b in zip(rings, rings[1:]):
        for k in range(sides):
            bm.faces.new((a[k], a[(k + 1) % sides], b[(k + 1) % sides], b[k]))
    for ring, flip in ((rings[0], True), (rings[-1], False)):
        c = bm.verts.new(sum((v.co for v in ring), Vector()) / sides)
        for k in range(sides):
            f = (ring[k], ring[(k + 1) % sides], c)
            bm.faces.new(f[::-1] if flip else f)
    return obj_from(bm, "tube")


def loft(sections):
    """Closed loft through cross-sections (each a list of Vectors, same count), capped at both ends."""
    bm = bmesh.new(); rings = [[bm.verts.new(p) for p in s] for s in sections]; n = len(sections[0])
    for a, b in zip(rings, rings[1:]):
        for k in range(n):
            bm.faces.new((a[k], a[(k + 1) % n], b[(k + 1) % n], b[k]))
    bm.faces.new(rings[0][::-1]); bm.faces.new(rings[-1])
    return obj_from(bm, "loft")


def fuse(parts, voxel, tris, name):
    """Voxel-merge the parts into one smooth surface, then thin it to about `tris` triangles."""
    o = join(parts, name)
    md = o.modifiers.new("r", "REMESH"); md.mode = "VOXEL"; md.voxel_size = voxel
    bpy.context.view_layer.objects.active = o; bpy.ops.object.modifier_apply(modifier="r")
    d = o.modifiers.new("d", "DECIMATE"); d.ratio = min(1.0, tris / max(1, len(o.data.polygons) * 2))
    bpy.ops.object.modifier_apply(modifier="d")
    for p in o.data.polygons: p.use_smooth = True
    return o


def paint(o, rule, names):
    """Material per face from its centre (rule returns an index into names)."""
    for n in names:
        o.data.materials.append(mat(n))
    for p in o.data.polygons:
        p.material_index = rule(p.center, p.normal)


def wing(side, stations):
    """One bird wing: an aerofoil lofted through (x, leading-edge y, chord, thickness, z) stations."""
    secs = []
    for (x, le, ch, th, z) in stations:
        sec = []
        for k in range(8):
            a = k / 8 * math.tau
            u = .5 - .5 * math.cos(a)                     # 0 at the leading edge, 1 at the trailing edge
            camber = .25 * th * math.sin(math.pi * u)
            sec.append(Vector((side * x, le - u * ch, z + camber + .5 * th * math.sin(a) * (1 - .6 * u))))
        secs.append(sec)
    return loft(secs)


# ---------------------------------------------------------------- seabird (1 m span)
reset()
parts = [ellipsoid((0, 0, 0), .06, (.85, 2.9, .9)),                       # body, 35 cm
         ellipsoid((0, .165, .028), .04, (.95, 1.15, .95)),              # head
         tube([(0, .19, .03), (0, .235, .022), (0, .275, .012)], [.013, .008, .002], 8)]   # beak
for side in (-1, 1):
    parts.append(wing(side, [(.0, .07, .16, .03, .015), (.1, .085, .16, .026, .05), (.2, .09, .145, .02, .075),
                             (.3, .065, .12, .015, .068), (.4, .025, .085, .011, .045), (.5, -.04, .03, .008, .025)]))
    # forked tail streamer
    parts.append(tube([(side * .012, -.14, .0), (side * .045, -.24, -.004), (side * .07, -.33, -.006)], [.018, .01, .003], 6))
o = fuse(parts, .0055, 2600, "seabird")
paint(o, lambda c, n: 2 if c.y > .235 else (1 if abs(c.x) > .38 or (abs(c.x) > .02 and abs(c.x) < .045 and abs(c.y - .18) < .012 and c.z > .03)
                                              else 0), ["bird white", "bird tip", "bird beak"])
export(o, "seabird", ["bird white", "bird tip", "bird beak"])


# ---------------------------------------------------------------- beach crab (0.3 m across the legs)
reset()
parts = [ellipsoid((0, 0, .05), .078, (1, .74, .42))]                    # carapace
for side in (-1, 1):
    parts.append(tube([(side * .022, .04, .06), (side * .026, .05, .085), (side * .028, .052, .1)], [.0065, .005, .004], 6))   # eye stalk
    parts.append(ellipsoid((side * .028, .053, .104), .009, (1, 1, 1)))
    k = 1.45 if side > 0 else 1.0                                       # one big claw
    # Arm: out of the front corner, bending forward to the wrist.
    parts.append(tube([(side * .045, .038, .046), (side * .072, .062, .056), (side * .08, .078 + .004 * k, .052)], [.011, .009 * k, .008 * k], 7))
    # Claw: a tapered palm with a fixed upper finger and a lower finger, a gap between (a pincer, not a ball).
    wy = .078 + .004 * k
    parts.append(tube([(side * .08, wy - .004, .052), (side * .081, wy + .016 * k, .054), (side * .078, wy + .03 * k, .056)], [.012 * k, .015 * k, .011 * k], 8))
    parts.append(tube([(side * .077, wy + .029 * k, .06), (side * .071, wy + .046 * k, .059), (side * .062, wy + .057 * k, .053)], [.0068 * k, .005 * k, .0028], 6))
    parts.append(tube([(side * .079, wy + .028 * k, .046), (side * .074, wy + .042 * k, .041), (side * .066, wy + .051 * k, .044)], [.0055 * k, .004 * k, .0026], 6))
    # Four walking legs a side, fanned (front ones reach forward, back ones back), each with a high knee.
    for i, y in enumerate((.024, .006, -.012, -.03)):
        fan = (i - 1.5) * .046
        parts.append(tube([(side * .06, y, .046), (side * .096, y + fan * .5, .074), (side * .124, y + fan, .055), (side * .14, y + fan * 1.35, .004)],
                          [.0098, .0078, .0062, .0038], 7))
o = fuse(parts, .0024, 3400, "crab")
paint(o, lambda c, n: 2 if c.z > .093 and abs(c.x) < .04 else (1 if abs(c.x) > .09 and c.y < .07 and c.z < .08 else 0), ["crab shell", "crab leg", "crab eye"])
export(o, "beach_crab", ["crab shell", "crab leg", "crab eye"])


# ---------------------------------------------------------------- butterfly (9 cm span)
reset()
bm = bmesh.new()
# Body: a slim tapering tube with a round head (built in the same mesh as the wings, which grow out of it).
pts = [Vector((0, y, 0)) for y in (-.024, -.012, 0, .01, .016, .021)]
radii = [.0015, .0034, .0038, .0034, .0036, .0025]
rings = []
for p, r in zip(pts, radii):
    rings.append([bm.verts.new(p + Vector((math.cos(a) * r, 0, math.sin(a) * r))) for a in [k / 6 * math.tau for k in range(6)]])
for a, b in zip(rings, rings[1:]):
    for k in range(6): bm.faces.new((a[k], a[(k + 1) % 6], b[(k + 1) % 6], b[k]))
for ring, fl in ((rings[0], True), (rings[-1], False)):
    c = bm.verts.new(sum((v.co for v in ring), Vector()) / 6)
    for k in range(6): f = (ring[k], ring[(k + 1) % 6], c); bm.faces.new(f[::-1] if fl else f)
body_faces = set(bm.faces)
edge_faces = set()


def wing_fan(side, root, outline):
    """A flat fan wing from its root on the body: an inner coloured panel and a darker rim (two rings)."""
    c = bm.verts.new(root); inner, outer = [], []
    for (ang, rad) in outline:
        d = Vector((side * math.cos(math.radians(ang)), math.sin(math.radians(ang)), 0))
        lift = lambda r: .12 * r * r / .05
        inner.append(bm.verts.new(root + d * rad * .78 + Vector((0, 0, lift(rad * .78)))))
        outer.append(bm.verts.new(root + d * rad + Vector((0, 0, lift(rad)))))
    for k in range(len(outline) - 1):
        f = (c, inner[k], inner[k + 1]); bm.faces.new(f if side > 0 else f[::-1])
        f = (inner[k], outer[k], outer[k + 1], inner[k + 1]); edge_faces.add(bm.faces.new(f if side > 0 else f[::-1]))


for side in (-1, 1):
    fore = [(a, r) for a, r in ((62, .012), (48, .036), (36, .046), (24, .05), (12, .047), (0, .04), (-12, .028))]
    hind = [(a, r) for a, r in ((-6, .02), (-20, .032), (-34, .036), (-50, .033), (-66, .027), (-82, .018), (-96, .01))]
    wing_fan(side, Vector((side * .003, .006, 0)), fore)
    wing_fan(side, Vector((side * .003, -.002, 0)), hind)
names = ["wing", "wing rim", "butterfly body"]
bm.faces.index_update()
rim_idx = {f.index for f in edge_faces}; body_idx = {f.index for f in body_faces}
o = obj_from(bm, "butterfly")
for n in names: o.data.materials.append(mat(n))
for p in o.data.polygons:
    p.use_smooth = p.index in body_idx
    p.material_index = 2 if p.index in body_idx else (1 if p.index in rim_idx else 0)
export(o, "butterfly", names)


# ---------------------------------------------------------------- reef fish (22 cm)
reset()
parts = [ellipsoid((0, .01, 0), .03, (.55, 3.0, 1.15))]
# Tail: a thin forked fan in the vertical plane, on a narrow wrist.
parts.append(tube([(0, -.07, 0), (0, -.1, 0)], [.012, .006], 6))
bm = bmesh.new()
for sgn in (1, -1):
    pts = [Vector((0, -.095, 0)), Vector((0, -.135, sgn * .022)), Vector((0, -.15, sgn * .038)), Vector((0, -.128, sgn * .008))]
    sec = [[p + Vector((x, 0, 0)) for p in pts] for x in (-.0036, .0036)]
    vs = [[bm.verts.new(p) for p in s] for s in sec]
    n = len(pts)
    for k in range(n):
        bm.faces.new((vs[0][k], vs[0][(k + 1) % n], vs[1][(k + 1) % n], vs[1][k]))
    bm.faces.new(vs[0][::-1]); bm.faces.new(vs[1])
parts.append(obj_from(bm, "tail"))
# Dorsal fin: a low swept ridge.
parts.append(loft([[Vector((x, y, z)) for (x, z) in ((-.003, .02), (.003, .02), (.001, .045 - abs(y) * .2), (-.001, .045 - abs(y) * .2))] for y in (.03, -.01, -.05)]))
o = fuse(parts, .0024, 2000, "fish")
paint(o, lambda c, n: 1 if c.y < -.085 or c.z > .034 else 0, ["fish body", "fish fin"])   # back-to-belly shading comes from the top tint
export(o, "reef_fish", ["fish body", "fish fin"])
