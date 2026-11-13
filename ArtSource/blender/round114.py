"""Round 114 (review 110, all three: the basin suds "flat white cut-out shapes with ring bubbles", "a flat white decal splat, not
PowerWash's volumetric spray", "flat white sticker shapes"): every foam sprite, however it was shaded, lay flat against the eye or the
water. Suds stand up off the water as heaps of packed bubbles, so they are now real geometry:
- foam_clump: a heap of soap foam about 1.0 across and 0.45 high: one large dome of bubbles with smaller bubbles packed around and
  on top of it, all merged into a single smooth surface (metaballs, so it reads as one heap, never as balls stuck together), with
  a flat underside where it sits in the water. Drawn as lit mesh particles at the basin (Feedback.cs, FoamClump.shader).
Run: blender -b --factory-startup -P round114.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *

reset(); random.seed(1140)
mb = bpy.data.metaballs.new("foam")
mb.resolution = .03
mb.threshold = .6
ob = bpy.data.objects.new("foam_clump", mb)
bpy.context.collection.objects.link(ob)

def ball(x, y, z, r):
    e = mb.elements.new()
    e.co = (x, y, z)
    e.radius = r

# The main dome and a ring of bubbles around it, larger toward the middle, a few riding on top.
ball(0, 0, .02, .34)
for i in range(9):
    a = i / 9 * math.tau + random.uniform(-.25, .25)
    d = random.uniform(.26, .4)
    ball(math.cos(a) * d, math.sin(a) * d * .9, random.uniform(-.02, .06), random.uniform(.13, .21))
for i in range(6):
    a = random.uniform(0, math.tau)
    d = random.uniform(.05, .22)
    ball(math.cos(a) * d, math.sin(a) * d, random.uniform(.17, .27), random.uniform(.1, .16))
for i in range(7):
    a = random.uniform(0, math.tau)
    d = random.uniform(.38, .5)
    ball(math.cos(a) * d, math.sin(a) * d, random.uniform(-.04, .02), random.uniform(.07, .11))

bpy.context.view_layer.objects.active = ob
ob.select_set(True)
bpy.ops.object.convert(target='MESH')
o = bpy.context.active_object
# Sit it in the water: everything below the waterline is pressed flat, a little under the surface.
for v in o.data.vertices:
    if v.co.z < -.05:
        v.co.z = -.05
dec = o.modifiers.new("d", 'DECIMATE')
dec.ratio = .35
assign(o, "foam")
shade_auto(o, 60)
export(o, "foam_clump", ["foam"])
