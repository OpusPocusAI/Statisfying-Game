"""Studio restoration props: a flowering sprig for the entry planters and festoon light strings.
Run: blender -b --factory-startup -P restoration.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *

# ---------------------------------------------------------------- flowering sprig (pivot at soil)
reset()
random.seed(3)
parts = []
for s in range(5):
    a = s / 5 * math.tau + random.uniform(-.3, .3)
    tip = (math.cos(a) * random.uniform(.12, .2), math.sin(a) * random.uniform(.12, .2), random.uniform(.32, .5))
    parts.append(tube_along("stem", [(0, 0, 0), (tip[0] * .4, tip[1] * .4, tip[2] * .6), tip], .008, "stem", resolution=6, bevel_resolution=1))
    # Five rounded petals and a centre.
    for p in range(5):
        pa = p / 5 * math.tau
        petal = uv_sphere("petal", .032, (tip[0] + math.cos(pa) * .03, tip[1] + math.sin(pa) * .03, tip[2] + .005), (1.25, .8, .35), 10, 6, "petal")
        petal.rotation_euler = (0, 0, pa); parts.append(petal)
    parts.append(uv_sphere("centre", .016, (tip[0], tip[1], tip[2] + .015), (1, 1, .6), 10, 6, "centre"))
sprig = join(parts, "FlowerSprig")
shade_auto(sprig, 60)
export(sprig, "flower_sprig", ["stem", "petal", "centre"])

# ---------------------------------------------------------------- festoon string (one 6 m span)
# Pivot at the left anchor; the span runs along +X and sags in the middle.
L, sag = 6.0, .45
def y(x): return -sag * (1 - ((x - L / 2) / (L / 2)) ** 2)
reset()
wire = tube_along("wire", [(x, 0, y(x)) for x in [i * L / 12 for i in range(13)]], .006, "wire", resolution=6, bevel_resolution=1)
export(wire, "festoon_wire", ["wire"])
reset()
parts = []
for i in range(1, 12):
    x = i * L / 12
    parts.append(cylinder("socket", .012, .03, (x, 0, y(x) - .025), vertices=10, material="socket"))
    parts.append(uv_sphere("bulb", .028, (x, 0, y(x) - .065), (1, 1, 1.25), 14, 8, "bulb"))
bulbs = join(parts, "Bulbs")
shade_auto(bulbs, 60)
export(bulbs, "festoon_bulbs", ["socket", "bulb"])
