"""Shared helpers for Sea Glass Studio Blender model scripts.

Each model script builds objects in an empty scene and calls export(). The JSON written here is
read by the Unity editor importer (Assets/Scripts/SeaGlass/Editor/ModelLibraryImporter.cs).

Coordinates: Blender is Z-up, right-handed, metres. The importer converts to Unity Y-up
(unity = (x, z, y)) and flips triangle winding. Each Blender material slot becomes one submesh.
"""
import bpy, bmesh, json, math, os, random
from mathutils import Vector, Matrix

OUT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "out")


def reset():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def mat(name):
    m = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    return m


def assign(obj, material_name):
    m = mat(material_name)
    if m.name not in [s.name for s in obj.data.materials]:
        obj.data.materials.append(m)
    idx = [s.name for s in obj.data.materials].index(m.name)
    for p in obj.data.polygons:
        p.material_index = idx
    return obj


def bevel(obj, width, segments=3, angle=35, profile=0.5):
    md = obj.modifiers.new("Bevel", "BEVEL")
    md.width = width
    md.segments = segments
    md.limit_method = "ANGLE"
    md.angle_limit = math.radians(angle)
    md.profile = profile
    md.harden_normals = False
    return md


def subsurf(obj, levels=2):
    md = obj.modifiers.new("Subsurf", "SUBSURF")
    md.levels = levels
    md.render_levels = levels
    return md


def cube(name, size, location=(0, 0, 0), material=None):
    bpy.ops.mesh.primitive_cube_add(size=1, location=location)
    o = bpy.context.active_object
    o.name = name
    o.scale = size
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if material:
        assign(o, material)
    return o


def cylinder(name, radius, depth, location=(0, 0, 0), rotation=(0, 0, 0), vertices=24, material=None, radius2=None):
    if radius2 is None:
        bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=location, rotation=rotation)
    else:
        bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=radius, radius2=radius2, depth=depth, location=location, rotation=rotation)
    o = bpy.context.active_object
    o.name = name
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    if material:
        assign(o, material)
    return o


def uv_sphere(name, radius, location=(0, 0, 0), scale=(1, 1, 1), segments=24, rings=12, material=None):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, radius=radius, location=location)
    o = bpy.context.active_object
    o.name = name
    o.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if material:
        assign(o, material)
    return o


def tube_along(name, points, radius, material=None, resolution=12, bevel_resolution=6, closed=False):
    """A round tube following a poly-bezier through points (list of xyz)."""
    curve = bpy.data.curves.new(name, "CURVE")
    curve.dimensions = "3D"
    curve.bevel_depth = radius
    curve.bevel_resolution = bevel_resolution
    curve.resolution_u = resolution
    curve.use_fill_caps = True
    spline = curve.splines.new("BEZIER")
    spline.bezier_points.add(len(points) - 1)
    for bp, p in zip(spline.bezier_points, points):
        bp.co = Vector(p)
        bp.handle_left_type = bp.handle_right_type = "AUTO"
    spline.use_cyclic_u = closed
    o = bpy.data.objects.new(name, curve)
    bpy.context.collection.objects.link(o)
    bpy.context.view_layer.objects.active = o
    o.select_set(True)
    bpy.ops.object.convert(target="MESH")
    o = bpy.context.active_object
    if material:
        assign(o, material)
    return o


def join(objs, name):
    bpy.ops.object.select_all(action="DESELECT")
    for o in objs:
        o.select_set(True)
        # Apply modifiers so the join keeps them.
        bpy.context.view_layer.objects.active = o
        for md in list(o.modifiers):
            bpy.ops.object.modifier_apply(modifier=md.name)
    bpy.context.view_layer.objects.active = objs[0]
    for o in objs:
        o.select_set(True)
    bpy.ops.object.join()
    j = bpy.context.active_object
    j.name = name
    return j


def shade_auto(obj, angle=40):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    try:
        bpy.ops.object.shade_auto_smooth(angle=math.radians(angle))
    except Exception:
        bpy.ops.object.shade_smooth_by_angle(angle=math.radians(angle))


def export(obj, name, material_order, origin=(0, 0, 0)):
    """Write obj (modifiers applied) as JSON. material_order fixes the submesh order."""
    os.makedirs(OUT_DIR, exist_ok=True)
    deps = bpy.context.evaluated_depsgraph_get()
    ev = obj.evaluated_get(deps)
    mesh = ev.to_mesh()
    mesh.transform(obj.matrix_world)
    mesh.calc_loop_triangles()
    normals = mesh.corner_normals
    names = [m.name if m else "" for m in mesh.materials]
    sub_index = {n: i for i, n in enumerate(material_order)}
    verts, norms = [], []
    subs = [[] for _ in material_order]
    lookup = {}
    ox, oy, oz = origin
    for tri in mesh.loop_triangles:
        mname = names[tri.material_index] if tri.material_index < len(names) else ""
        s = sub_index.get(mname, 0)
        idx = []
        for li in tri.loops:
            v = mesh.vertices[mesh.loops[li].vertex_index].co
            n = normals[li].vector
            key = (round(v.x - ox, 5), round(v.y - oy, 5), round(v.z - oz, 5), round(n.x, 3), round(n.y, 3), round(n.z, 3))
            i = lookup.get(key)
            if i is None:
                i = len(verts)
                lookup[key] = i
                verts.append([key[0], key[1], key[2]])
                norms.append([n.x, n.y, n.z])
            idx.append(i)
        subs[s].extend(idx)
    ev.to_mesh_clear()
    # Flat arrays: Unity's JsonUtility cannot read nested arrays.
    data = {"name": name, "materials": material_order,
            "positions": [c for v in verts for c in v], "normals": [round(c, 4) for n in norms for c in n],
            "submeshes": [{"indices": s} for s in subs]}
    path = os.path.join(OUT_DIR, name + ".json")
    with open(path, "w") as f:
        json.dump(data, f)
    print("exported", path, len(verts), "verts", sum(len(s) for s in subs) // 3, "tris")
