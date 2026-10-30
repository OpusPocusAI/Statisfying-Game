"""Round 34 models (after the thirty-first review, three reviewers averaged): "the same three-pebble cluster stamped
dozens of times". Four more scatter clusters with the same material slots as pebble_cluster (pebble_0, pebble_1),
so the beach can mix five shapes:
- pebble_cluster_b: one larger flat stone with two small ones tucked against it.
- pebble_cluster_c: a short drift line of tiny gravel, as the tide leaves it.
- pebble_cluster_d: two flat skimming stones, one resting on the other.
- pebble_cluster_e: a single round cobble, half sunk.
Run: blender -b --factory-startup -P round34.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *


def stone(r, at, squash, m, rot=0):
    p = uv_sphere("p", r, at, squash, 14, 8, m)
    p.rotation_euler = (0, 0, rot); bpy.context.view_layer.objects.active = p; bpy.ops.object.transform_apply(rotation=True)
    return p


def finish(parts, name):
    o = join(parts, "Pebbles"); shade_auto(o, 80)
    export(o, name, ["pebble_0", "pebble_1"])


reset(); random.seed(3401)
finish([stone(.07, (0, 0, .025), (1.4, 1.0, .45), "pebble_0", .4),
        stone(.022, (.1, .03, .01), (1.2, 1, .6), "pebble_1"), stone(.018, (.07, -.06, .008), (1.3, 1, .6), "pebble_1")], "pebble_cluster_b")

reset(); random.seed(3402)
parts = []
for i in range(9):
    x = -.18 + i * .045 + random.uniform(-.01, .01); y = math.sin(i * .7) * .02 + random.uniform(-.012, .012)
    r = random.uniform(.009, .018)
    parts.append(stone(r, (x, y, r * .4), (1.3, 1, .6), "pebble_%d" % (i % 2), random.uniform(0, 3)))
finish(parts, "pebble_cluster_c")

reset(); random.seed(3403)
finish([stone(.055, (0, 0, .012), (1.5, 1.0, .25), "pebble_1", .2),
        stone(.04, (.025, .01, .03), (1.5, 1.0, .25), "pebble_0", 1.3)], "pebble_cluster_d")

reset(); random.seed(3404)
finish([stone(.06, (0, 0, .018), (1.1, 1.0, .75), "pebble_0")], "pebble_cluster_e")
