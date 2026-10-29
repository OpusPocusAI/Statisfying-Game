"""Round 13 models (after the tenth and eleventh independent reviews):
- scrub_brush_tuft: a classic wooden scrubbing brush. A long, slim, flat-backed block with chamfered edges,
  a tall block of dense, slightly splayed bristles (about as deep as the wood, so the bristles are half the
  silhouette) and an arched grip on top for the gloved hand. Review 11 read the earlier rounded,
  boat-shaped back as "a clog or slipper". Pivot: centre of the bristle-tip plane, long axis +X, like
  scrub_brush, so it rests on the glass.
Run: blender -b --factory-startup -P round13.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *

random.seed(1307)
reset()
parts = []

L, W = .2, .062          # block length and width
BR = .034                # bristle length (the tip plane is z = 0)
T = .022                 # wood thickness
z0 = BR                  # underside of the wood

block = cube("block", (L, W, T), (0, 0, z0 + T / 2), "wood")
bevel(block, .007, 3, 30)
parts.append(block)

# A thin darker stock line where the bristles are set, so the wood and the bristles separate clearly.
stock = cube("stock", (L * .97, W * .94, .004), (0, 0, z0 + .001), "wood_dark")
bevel(stock, .0015, 2, 30)
parts.append(stock)

# Arched grip over the block.
grip = tube_along("grip", [(-.064, 0, z0 + T - .002), (-.046, 0, z0 + T + .032), (0, 0, z0 + T + .043),
                           (.046, 0, z0 + T + .032), (.064, 0, z0 + T - .002)], .0105, "wood_dark")
parts.append(grip)

# A packed bristle block, slightly inset and with ragged tips, so the underside reads as one dense mass...
core = cube("core", (L * .9, W * .82, BR * .92), (0, 0, z0 - BR * .46), "bristle")
bevel(core, .004, 2, 30)
for v in core.data.vertices:
    if v.co.z < z0 - BR * .5:
        v.co.z += (math.sin(v.co.x * 260) * math.sin(v.co.y * 210)) * .0015
parts.append(core)

# ...with a fringe of splayed tufts around the edge, so it never reads as a solid pad.
def tuft(x, y, out_x, out_y):
    for b in range(5):
        a = b / 5 * math.tau + random.uniform(0, .7)
        r = .0018 if b else 0
        length = BR + random.uniform(-.003, .002)
        br = cylinder("bristle", .0015, length, (x + math.cos(a) * r, y + math.sin(a) * r, z0 - length / 2), vertices=5, material="bristle", radius2=.0011)
        sp = .16 + random.uniform(-.04, .05)
        br.rotation_euler = (-out_y * sp, out_x * sp, 0)
        parts.append(br)

hx, hy = L * .46, W * .43
n = 14
for i in range(n + 1):
    x = -hx + i * (2 * hx / n)
    tuft(x, hy, 0, 1); tuft(x, -hy, 0, -1)
for j in range(1, 4):
    y = -hy + j * (2 * hy / 4)
    tuft(hx, y, 1, 0); tuft(-hx, y, -1, 0)

brush = join(parts, "ScrubBrushTuft")
shade_auto(brush, 40)
export(brush, "scrub_brush_tuft", ["wood", "wood_dark", "bristle"])
