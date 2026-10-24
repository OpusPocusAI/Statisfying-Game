"""Chunky gardening glove gripping a bar that runs along X through the origin (radius ~2 cm).
The palm sits on the player side (-Y in Blender = -Z in Unity); fingers wrap over the top.
Run: blender -b --factory-startup -P glove.py
"""
import sys, os, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *

reset()
parts = []
# Palm: rounded block behind and below the bar.
palm = uv_sphere("palm", .05, (0, -.045, -.02), (1.1, .75, 1.0), 20, 12, "glove")
parts.append(palm)
# Four fingers curling over the bar from behind to the front.
for i in range(4):
    x = -.036 + i * .024
    r = .011 if i in (1, 2) else .0095
    pts = [(x, -.045, .015), (x, -.022, .036), (x, .012, .034), (x, .03, .012), (x, .026, -.008)]
    parts.append(tube_along("finger", pts, r, "glove", resolution=10, bevel_resolution=3))
# Thumb wrapping the other way under the bar.
parts.append(tube_along("thumb", [(-.05, -.06, -.03), (-.055, -.02, -.035), (-.045, .015, -.025)], .012, "glove", resolution=10, bevel_resolution=3))
# Cuff and a bit of forearm sleeve heading back toward the camera.
cuff = cylinder("cuff", .045, .05, (0, -.095, -.045), rotation=(math.radians(82), 0, 0), vertices=20, material="cuff")
bevel(cuff, .008, 2, 40); parts.append(cuff)
# Forearm runs back and up toward the shoulder, long enough to leave the frame.
sleeve = cylinder("sleeve", .05, .62, (0, -.42, 0), rotation=(math.radians(82), 0, 0), vertices=20, material="sleeve", radius2=.068)
parts.append(sleeve)
g = join(parts, "Glove")
shade_auto(g, 50)
export(g, "garden_glove", ["glove", "cuff", "sleeve"])
