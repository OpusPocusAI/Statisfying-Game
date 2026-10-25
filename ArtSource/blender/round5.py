"""Round 5 models, from the independent blind review (ArtReference/comparison.md):
- recycling_press: the recycling station was a plain red cube. A small coral press cabinet with a teal
  press plate, hopper slot, recycling arrows on the lid, a cream name panel (behind the existing
  "FEED THE FUTURE" lettering), a brass crank wheel, feet and rivets.
  True size 1.7 (x) x 0.7 (height) x 1.0 (depth), centred on its origin; the front faces -Y (Unity -Z).
Run: blender -b --factory-startup -P round5.py
"""
import sys, os, math
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *

reset()
W, D, H = 1.7, 1.0, .7
p = []
body = cube("body", (W, D, H - .1), (0, 0, -.05), "press_coral"); bevel(body, .05, 3, 40); p.append(body)
plate = cube("plate", (W + .03, D + .03, .1), (0, 0, H / 2 - .05), "press_teal"); bevel(plate, .03, 2, 40); p.append(plate)
p.append(cube("slot", (1.0, .22, .02), (-.15, .05, H / 2 + .005), "charcoal"))
# Recycling arrows: three bent arrows in a triangle on the lid (cream).
cx, cy, cz, R = .55, .05, H / 2 + .01, .14
for k in range(3):
    a0 = k * 2 * math.pi / 3 + .35; a1 = a0 + 2 * math.pi / 3 - .55
    pts = [(cx + math.cos(a0 + (a1 - a0) * t) * R, cy + math.sin(a0 + (a1 - a0) * t) * R, cz) for t in (0, .5, 1)]
    p.append(tube_along("arrow", pts, .014, "cream", resolution=6, bevel_resolution=1))
    tip = pts[-1]; d = (math.cos(a1 + math.pi / 2), math.sin(a1 + math.pi / 2))
    p.append(cylinder("head", .035, .05, (tip[0] + d[0] * .02, tip[1] + d[1] * .02, cz), rotation=(0, math.radians(90), math.atan2(d[1], d[0])), vertices=3, material="cream", radius2=0))
# Cream name panel on the front, behind the lettering (lettering sits 0.1 above the press centre).
panel = cube("panel", (1.62, .02, .26), (0, -D / 2 - .005, .1), "cream"); bevel(panel, .008, 1, 40); p.append(panel)
# Rivets on the front corners.
for sx in (-1, 1):
    for sz in (-1, 1):
        p.append(uv_sphere("rivet", .018, (sx * (W / 2 - .07), -D / 2 - .01, sz * .2 - .05), (1, .6, 1), 8, 5, "brass"))
# Brass crank wheel on the right side.
bpy.ops.mesh.primitive_torus_add(major_radius=.17, minor_radius=.018, major_segments=24, minor_segments=8, location=(W / 2 + .05, 0, -.02), rotation=(0, math.radians(90), 0))
wheel = bpy.context.active_object; assign(wheel, "brass"); p.append(wheel)
for k in range(4):
    a = k * math.pi / 2
    p.append(cylinder("spoke", .01, .34, (W / 2 + .05, 0, -.02), rotation=(a, 0, 0), vertices=6, material="brass"))
p.append(cylinder("hubc", .035, .06, (W / 2 + .05, 0, -.02), rotation=(0, math.radians(90), 0), vertices=10, material="brass"))
p.append(cylinder("knob", .022, .1, (W / 2 + .1, .15, .09), rotation=(0, math.radians(90), 0), vertices=8, material="charcoal"))
# Feet.
for sx in (-1, 1):
    for sy in (-1, 1):
        p.append(cube("foot", (.14, .14, .05), (sx * (W / 2 - .12), sy * (D / 2 - .12), -H / 2 + .025), "charcoal"))
press = join(p, "Press"); shade_auto(press, 40)
export(press, "recycling_press", ["press_coral", "press_teal", "charcoal", "cream", "brass"])
