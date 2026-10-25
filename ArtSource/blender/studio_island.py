"""Studio interior round 4 (from the side-by-side with Unpacking): furniture that breaks up the open hall,
and dense surface clutter so no surface reads as empty.
- work_island: 2.4 x 1.1 m work table, 0.92 m high, drawers on both long sides, lower shelf with crates.
- open_shelf: 2.0 m long, 0.45 deep, 1.8 tall open shelving (divider; open from both sides).
- shelf_goods: books, jars of sea glass, bottles, pots and baskets that fill one open_shelf.
- table_clutter: sketchbook, books, brush jar, mug, lamp, glass tray, cutting mat for the island top.
- bench_clutter: a small set (books, mug, pencil jar, glass pieces) for a free bench end.
Origins: furniture on the floor at its centre; clutter sets at the surface they stand on.
Run: blender -b --factory-startup -P studio_island.py
"""
import sys, os, math, random
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from sg_export import *


def box(n, size, loc, m, b=.008):
    o = cube(n, size, loc, m)
    if b: bevel(o, b, 1, 40)
    return o


def book(x, y, z, w, d, h, m, lean=0.0, yaw=0.0):
    o = box("book", (w, d, h), (0, 0, h / 2), m, .004)
    o.rotation_euler = (0, lean, yaw); bpy.ops.object.transform_apply(rotation=True)
    o.location = (x, y, z); bpy.ops.object.transform_apply(location=True)
    return o


def jar(x, y, z, r, h, contents):
    parts = [cylinder("jar", r, h, (x, y, z + h / 2), vertices=14, material="jar_glass")]
    parts.append(cylinder("fill", r * .86, h * .62, (x, y, z + h * .33), vertices=12, material=contents))
    parts.append(cylinder("lid", r * .8, h * .1, (x, y, z + h + h * .05), vertices=12, material="cork"))
    return parts


def bottle(x, y, z, r, h, m):
    return [cylinder("b", r, h * .7, (x, y, z + h * .35), vertices=12, material=m),
            cylinder("neck", r * .35, h * .3, (x, y, z + h * .85), vertices=8, material=m)]


def pot(x, y, z, r, h, m):
    return [cylinder("pot", r, h, (x, y, z + h / 2), vertices=14, material=m, radius2=r * .75)]


# ---------------------------------------------------------------- work island
reset()
W, D, H = 2.4, 1.1, .92
p = [box("top", (W, D, .08), (0, 0, H - .04), "wood_top", .015)]
for sx in (-1, 1):
    for sy in (-1, 1):
        p.append(box("leg", (.09, .09, H - .08), (sx * (W / 2 - .08), sy * (D / 2 - .08), (H - .08) / 2), "wood_frame"))
p.append(box("apron", (W - .1, D - .1, .14), (0, 0, H - .15), "wood_frame"))
p.append(box("shelf", (W - .12, D - .12, .04), (0, 0, .22), "wood_top"))
for side in (-1, 1):                                    # three drawers on each long side
    for i in range(3):
        x = (i - 1) * (W - .3) / 3
        p.append(box("drawer", ((W - .4) / 3 - .04, .02, .11), (x, side * (D / 2 - .04), H - .15), "drawer_paint", .006))
        p.append(cylinder("knob", .018, .03, (x, side * (D / 2 - .01), H - .15), rotation=(math.radians(90), 0, 0), vertices=8, material="brass"))
for i, x in enumerate((-.7, .05, .75)):                 # crates on the lower shelf
    c = box("crate", (.55, .42, .3), (x, (-1) ** i * .12, .24 + .15), "crate_wood", .01)
    p.append(c)
    for k in range(2):
        p.append(box("slat", (.56, .43, .025), (x, (-1) ** i * .12, .24 + .08 + k * .14), "wood_frame", 0))
isl = join(p, "Island"); facet_ok = shade_auto(isl, 35)
export(isl, "work_island", ["wood_top", "wood_frame", "drawer_paint", "brass", "crate_wood"])

# ---------------------------------------------------------------- open shelf (2.0 x 0.45 x 1.8)
reset()
L, Dp, Ht = 2.0, .45, 1.8
SH = [.08, .5, .95, 1.4]
p = []
for sx in (-1, 0, 1):
    p.append(box("side", (.04, Dp, Ht), (sx * (L / 2 - .02), 0, Ht / 2), "shelf_paint"))
for z in SH + [Ht - .02]:
    p.append(box("board", (L, Dp, .035), (0, 0, z), "shelf_wood", .005))
p.append(box("rail", (L, .03, .05), (0, Dp / 2 - .015, .2), "shelf_paint", 0))
sh = join(p, "Shelf"); shade_auto(sh, 35)
export(sh, "open_shelf", ["shelf_wood", "shelf_paint"])

# ---------------------------------------------------------------- shelf goods (fills one open_shelf)
reset(); random.seed(810)
BOOKS = ["book_a", "book_b", "book_c", "book_d"]
GLASS = ["glass_aqua", "glass_amber", "glass_green", "glass_cobalt"]
p = []
tops = [z + .0175 for z in SH]
for level, z in enumerate(tops):
    x = -L / 2 + .07
    while x < L / 2 - .1:
        r = random.random()
        bay_end = 0 if x < 0 else L / 2
        if r < .35:                                     # a run of books
            n = random.randint(4, 9)
            for k in range(n):
                w = random.uniform(.025, .045); h = random.uniform(.2, .32)
                if x + w > L / 2 - .06: break
                lean = random.choice([0, 0, 0, .12]) if k == n - 1 else 0
                p.append(book(x + w / 2, random.uniform(-.04, .04), z, w, random.uniform(.15, .22), h, random.choice(BOOKS), lean))
                x += w + .003
            x += .05
        elif r < .6:                                    # jars of sorted glass
            for k in range(random.randint(1, 3)):
                rr = random.uniform(.045, .07)
                if x + rr * 2 > L / 2 - .06: break
                p += jar(x + rr, random.uniform(-.05, .05), z, rr, random.uniform(.13, .2), random.choice(GLASS)); x += rr * 2 + .02
            x += .04
        elif r < .75:                                   # bottles
            for k in range(random.randint(2, 3)):
                rr = random.uniform(.03, .045)
                if x + rr * 2 > L / 2 - .06: break
                p += bottle(x + rr, random.uniform(-.06, .06), z, rr, random.uniform(.2, .3), random.choice(GLASS)); x += rr * 2 + .015
            x += .04
        elif r < .9:                                    # pottery
            rr = random.uniform(.06, .09)
            if x + rr * 2 < L / 2 - .06:
                p += pot(x + rr, 0, z, rr, random.uniform(.12, .22), random.choice(["pottery_cream", "pottery_teal"])); x += rr * 2 + .05
            else: x += .1
        else:                                           # a lying stack of books
            w = random.uniform(.2, .26)
            if x + w < L / 2 - .06:
                zz = z
                for k in range(random.randint(2, 4)):
                    t = random.uniform(.025, .04)
                    p.append(book(x + w / 2, 0, zz, w, random.uniform(.16, .2), t, random.choice(BOOKS), 0, random.uniform(-.15, .15)))
                    zz += t
                x += w + .05
            else: x += .1
        if level == 0 and random.random() < .5:         # bottom shelf: a basket now and then
            pass
# Two wicker baskets on the bottom shelf.
for bx in (-.6, .45):
    p.append(box("basket", (.38, .32, .22), (bx, 0, tops[0] + .11), "basket", .03))
goods = join(p, "Goods")
for poly in goods.data.polygons: poly.use_smooth = False
export(goods, "shelf_goods", BOOKS + ["jar_glass", "cork"] + GLASS + ["pottery_cream", "pottery_teal", "basket"])

# ---------------------------------------------------------------- table clutter (island top, origin at top surface)
reset(); random.seed(820)
p = []
p.append(box("mat", (.9, .6, .006), (-.55, .05, .003), "cutting_mat", 0))                     # green cutting mat
for i, (x, y) in enumerate([(-.75, -.05), (-.6, .15), (-.45, -.02), (-.35, .18), (-.65, .02)]):  # glass pieces on it
    p.append(uv_sphere("piece", .03, (x, y, .02), (1, .7, .3), 8, 5, GLASS[i % 4]))
p.append(box("sketch_l", (.3, .42, .012), (.15, -.1, .006), "paper", 0))                       # open sketchbook
p.append(box("sketch_r", (.3, .42, .012), (.46, -.1, .006), "paper_sketch", 0))
p.append(box("spine", (.02, .42, .016), (.305, -.1, .008), "book_b", 0))
z = 0
for k, m in enumerate(["book_a", "book_c", "book_d"]):                                          # book stack
    t = .04 + k * .005
    p.append(book(.9, .28, z, .26, .19, t, m, 0, .2 * (k - 1))); z += t
p += jar(.72, .32, 0, .05, .14, "glass_amber")
p.append(cylinder("brushjar", .045, .13, (1.0, -.25, .065), vertices=12, material="pottery_teal"))  # brush jar
for k in range(5):
    a = k / 5 * math.tau
    p.append(cylinder("brush", .007, .26, (1.0 + math.cos(a) * .02, -.25 + math.sin(a) * .02, .19), rotation=(math.cos(a) * .15, math.sin(a) * .15, 0), vertices=6, material="wood_frame"))
p.append(cylinder("mug", .042, .1, (.62, .3, .05), vertices=14, material="pottery_cream"))       # mug
p.append(tube_along("handle", [(.662, .3, .08), (.69, .3, .05), (.662, .3, .02)], .009, "pottery_cream", resolution=4, bevel_resolution=1))
p.append(cylinder("lampbase", .08, .03, (-1.0, .32, .015), vertices=14, material="lamp"))       # desk lamp
p.append(tube_along("arm", [(-1.0, .32, .03), (-1.02, .3, .3), (-.88, .22, .42)], .012, "lamp", resolution=6, bevel_resolution=2))
p.append(cylinder("shade", .09, .12, (-.84, .2, .38), rotation=(0, math.radians(55), math.radians(-30)), vertices=14, material="lamp", radius2=.04))
cl = join(p, "TableClutter")
for poly in cl.data.polygons: poly.use_smooth = False
export(cl, "table_clutter", ["cutting_mat", "paper", "paper_sketch", "book_a", "book_b", "book_c", "book_d", "jar_glass", "cork", "pottery_teal", "pottery_cream", "wood_frame", "lamp"] + GLASS)

# ---------------------------------------------------------------- bench clutter (small, ~0.45 x 0.35)
reset(); random.seed(830)
p = []
z = 0
for k, m in enumerate(["book_d", "book_a"]):
    t = .035
    p.append(book(-.1, .05, z, .22, .16, t, m, 0, .25 * k)); z += t
p.append(cylinder("mug", .04, .095, (.14, .1, .0475), vertices=14, material="pottery_teal"))
p += jar(.12, -.1, 0, .045, .13, "glass_green")
for i in range(4):
    p.append(uv_sphere("piece", .025, (-.12 + i * .06, -.12, .015), (1, .7, .3), 8, 5, GLASS[i]))
bc = join(p, "BenchClutter")
for poly in bc.data.polygons: poly.use_smooth = False
export(bc, "bench_clutter", ["book_a", "book_d", "pottery_teal", "jar_glass", "cork"] + GLASS)
