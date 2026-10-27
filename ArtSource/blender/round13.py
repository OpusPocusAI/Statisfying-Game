"""Round 13 models (after the tenth independent review):
- scrub_brush_tuft: a wooden scrub brush with a rounded, boat-shaped back and a dense mass of splayed
  bristle tufts (review 10: "a block with peg bristles"). Same footprint and pivot as scrub_brush: the
  pivot is the centre of the bristle-tip plane, long axis +X, so it rests on the glass.
Run: blender -b --factory-startup -P round13.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *

random.seed(1307)
reset()
parts = []

# Boat-shaped back: an ellipse in plan, domed on top, with a bevelled lower edge.
L, W, T = .18, .07, .026
bpy.ops.mesh.primitive_uv_sphere_add(segments=40, ring_count=14, radius=1)
back = bpy.context.active_object
for v in back.data.vertices:
    p = v.co.copy()
    # squarer ellipse in plan (superellipse), flat underside, domed top
    ax, ay = abs(p.x), abs(p.y)
    k = (ax ** 3 + ay ** 3) ** (1 / 3) if (ax + ay) > 0 else 1
    s = (math.hypot(p.x, p.y) / k) if k > 0 else 1
    v.co.x = p.x * s * L / 2
    v.co.y = p.y * s * W / 2
    v.co.z = (max(p.z, -.35) + .35) / 1.35 * T + .024
back.name = "back"
assign(back, "wood")
shade_auto(back, 50)
parts.append(back)

# A darker stock band around the base, where the bristles are set.
band = cylinder("band", 1, .008, (0, 0, .027), vertices=40, material="wood_dark")
band.scale = (L / 2 * 1.01, W / 2 * 1.04, 1)
bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
parts.append(band)

# Hand grip: a smooth arch over the back.
grip = tube_along("grip", [(-.062, 0, .045), (-.045, 0, .082), (0, 0, .094), (.045, 0, .082), (.062, 0, .045)], .0105, "wood_dark")
parts.append(grip)

# Dense bristle tufts: a staggered grid of tufts, each a bundle of slim splayed bristles of slightly
# different lengths, so the underside reads as a full brush, not pegs.
rows = 5
for iy in range(rows):
    y = -W / 2 * .78 + iy * (W * .78 / (rows - 1))
    half = L / 2 * math.sqrt(max(0, 1 - (y / (W / 2 * .9)) ** 2)) * .9
    n = int(half * 2 / .0115) + 1
    for ix in range(n):
        x = -half + ix * (half * 2 / max(1, n - 1)) + (.004 if iy % 2 else 0)
        if abs(x) > half:
            continue
        outer = abs(y) > W * .3 or abs(x) > half - .012
        for b in range(6):
            a = b / 6 * math.tau + random.uniform(0, .6)
            r = .0022 if b else 0
            bx, by = x + math.cos(a) * r, y + math.sin(a) * r
            length = .026 + random.uniform(-.003, .003)
            br = cylinder("bristle", .0016, length, (bx, by, .024 - length / 2), vertices=5, material="bristle", radius2=.0012)
            # splay outward from the tuft centre, more at the brush edge
            sp = (.2 if outer else .1) + random.uniform(-.04, .04)
            br.rotation_euler = (-math.sin(a) * sp + (y / W) * .25, math.cos(a) * sp + (x / L) * .25, 0)
            parts.append(br)

# A packed bristle core under the tufts, so the brush reads as one dense mass with a splayed fringe
# (the tufts alone showed as sparse pins from the player's view).
bpy.ops.mesh.primitive_uv_sphere_add(segments=32, ring_count=10, radius=1)
core = bpy.context.active_object
for v in core.data.vertices:
    p = v.co.copy()
    ax, ay = abs(p.x), abs(p.y)
    k = (ax ** 3 + ay ** 3) ** (1 / 3) if (ax + ay) > 0 else 1
    s = (math.hypot(p.x, p.y) / k) if k > 0 else 1
    v.co.x = p.x * s * L / 2 * .9
    v.co.y = p.y * s * W / 2 * .86
    v.co.z = .024 - (1 - (max(p.z, -.2) + .2) / 1.2) * .021   # flat top at the stock, soft rounded underside
    v.co.z += (math.sin(p.x * 40) * math.sin(p.y * 37)) * .0012   # ragged tips
core.name = "core"
assign(core, "bristle")
parts.append(core)

brush = join(parts, "ScrubBrushTuft")
export(brush, "scrub_brush_tuft", ["wood", "wood_dark", "bristle"])
