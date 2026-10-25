"""Round 12 models (after the eighth independent review):
- glass_pebble_0..4: one family of tumbled sea-glass pebbles shared by every colour (held, tray, beach and
  truck displays; mosaic pieces keep their outline-fitting meshes). About 1.0 x 0.75 across in Blender XZ,
  30-40% as thick along Blender Y, so in Unity they lie in the XY plane with thickness along Z like the
  authored pieces. Rounded on all sides, slightly flatter on the back, with gentle tumbled facets.
- enamel_bowl: an oval enamel wash bowl with a rolled rim and a soft inner floor. True size 1.9 (x) x
  1.25 (along Blender y = Unity z) x 0.22 high; origin at the base centre.
- bowl_water: oval disc for the water surface, unit size (-0.5..0.5), lying flat.
Run: blender -b --factory-startup -P round12.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *
from mathutils import noise

shapes = [(1.0, .78, .36), (1.0, .64, .32), (.92, .82, .4), (1.0, .7, .3), (.86, .76, .38)]
for k, (sx, sz, th) in enumerate(shapes):
    reset(); random.seed(1200 + k)
    bpy.ops.mesh.primitive_uv_sphere_add(segments=28, ring_count=16, radius=.5)
    o = bpy.context.active_object
    s = Vector((k * 1.7, k * .3, k * 2.9))
    for v in o.data.vertices:
        p = v.co.copy()
        # irregular outline: lumpy in plan
        a = math.atan2(p.z, p.x)
        lump = 1 + noise.noise(Vector((math.cos(a) * 1.4, math.sin(a) * 1.4, 0)) + s) * .16
        v.co.x = p.x * sx * lump
        v.co.z = p.z * sz * lump
        # thickness along Y, a little flatter on the back (+Y = away from the player)
        v.co.y = p.y * th * (0.82 if p.y > 0 else 1.0)
        # tumbled, softly faceted surface
        v.co.y += noise.noise(p * 3.2 + s) * .025 * (1 if p.y < 0 else .6)
    dec = o.modifiers.new("d", "DECIMATE"); dec.ratio = .55; bpy.ops.object.modifier_apply(modifier="d")
    assign(o, "glass")
    shade_auto(o, 35)
    export(o, "glass_pebble_%d" % k, ["glass"])

# ---------------------------------------------------------------- enamel bowl
reset()
L, Wd, H = 1.9, 1.25, .22
def oval(r_x, r_y, z, n=48):
    return [(math.cos(i / n * math.tau) * r_x, math.sin(i / n * math.tau) * r_y, z) for i in range(n)]
bm = bmesh.new()
rings = [oval(L / 2 * .78, Wd / 2 * .74, .02), oval(L / 2 * .86, Wd / 2 * .84, .05), oval(L / 2 * .93, Wd / 2 * .92, .13),
         oval(L / 2 * .97, Wd / 2 * .965, H - .02), oval(L / 2, Wd / 2, H)]
vr = [[bm.verts.new(p) for p in ring] for ring in rings]
centre = bm.verts.new((0, 0, .02))
n = len(rings[0])
for i in range(n):
    bm.faces.new((centre, vr[0][(i + 1) % n], vr[0][i]))
for r in range(len(vr) - 1):
    for i in range(n):
        bm.faces.new((vr[r][i], vr[r][(i + 1) % n], vr[r + 1][(i + 1) % n], vr[r + 1][i]))
me = bpy.data.meshes.new("bowl"); bm.to_mesh(me); bm.free()
bowl = bpy.data.objects.new("Bowl", me); bpy.context.collection.objects.link(bowl)
bpy.context.view_layer.objects.active = bowl; bowl.select_set(True)
sol = bowl.modifiers.new("s", "SOLIDIFY"); sol.thickness = .025; sol.offset = 1; bpy.ops.object.modifier_apply(modifier="s")
assign(bowl, "enamel")
parts = [bowl]
# rolled rim (dark blue enamel edge, like a classic enamel bowl)
rim = tube_along("rim", [p for p in oval(L / 2 + .005, Wd / 2 + .005, H)], .022, "rim", resolution=4, bevel_resolution=3, closed=True)
parts.append(rim)
b = join(parts, "EnamelBowl"); shade_auto(b, 50)
export(b, "enamel_bowl", ["enamel", "rim"])

# ---------------------------------------------------------------- oval water disc (unit)
reset()
bpy.ops.mesh.primitive_circle_add(vertices=48, radius=.5, fill_type="TRIFAN")
w = bpy.context.active_object
assign(w, "water")
export(w, "bowl_water", ["water"])
