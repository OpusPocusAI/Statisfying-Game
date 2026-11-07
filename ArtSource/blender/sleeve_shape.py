"""Round 67 (review 63, all three: the first-person sleeve is "a long, uniform tapered tube", "a huge featureless cylinder
filling a third of the frame", and it ends in a visible flat cut): one shared, shaped cloth sleeve for both first-person
arms (glove.py and palm_glove.py), so the arm reads as a forearm in a work shirt rather than a primitive.
- A forearm silhouette: an oval section that swells over the upper forearm and widens to the elbow, not a straight cone.
- Uneven compression folds bunched above the cuff, tilted and wandering around the arm (cloth gathered at the wrist).
- Long, shallow drape creases running down the forearm.
- A pushed-up, gathered band two thirds of the way up, as when a sleeve is shoved back to work.
- A soft seam groove along the underside.
- Longer than before, so its end always leaves the frame.
The sleeve is added to the blocking parts and fused with the rest of the arm (voxel remesh) by the caller.
"""
import math
import bpy
from mathutils import Vector, noise


def shaped_sleeve(wrist, axis_dir, L=1.0, r0=.051, r1=.086, seed=0):
    bpy.ops.mesh.primitive_cylinder_add(vertices=48, radius=1, depth=1)
    sl = bpy.context.active_object
    bpy.ops.object.mode_set(mode="EDIT"); bpy.ops.mesh.subdivide(number_cuts=30); bpy.ops.object.mode_set(mode="OBJECT")
    s = Vector((seed * 1.7, seed * .9, 0))
    for v in sl.data.vertices:
        t = v.co.z + .5                                   # 0 at the wrist .. 1 past the elbow
        a = math.atan2(v.co.y, v.co.x); rr = math.hypot(v.co.x, v.co.y)
        r = r0 + (r1 - r0) * t ** .85
        r += .011 * math.exp(-((t - .4) / .17) ** 2)      # the forearm swells below the elbow
        # Compression folds bunched above the cuff: uneven rings that tilt and wander around the arm.
        ring = math.sin(t * 46 + 1.1 * math.sin(a + t * 4) + .6 * noise.noise(Vector((math.cos(a), math.sin(a), t * 3)) + s))
        r += .012 * ring * max(0.0, 1 - t / .36) ** 1.2
        # Long, shallow drape creases down the forearm.
        r += .0065 * math.sin(a * 3 + t * 4.5 + 1.3) * min(1.0, t * 2.5)
        # A pushed-up, gathered band.
        band = math.exp(-((t - .66) / .04) ** 2)
        r += .016 * band * (1 + .25 * math.sin(a * 5 + 2))
        # A soft seam groove along the underside (Blender -Y of the tube before it is turned onto the arm axis).
        da = math.atan2(math.sin(a + math.pi / 2), math.cos(a + math.pi / 2))
        r -= .0025 * math.exp(-(da / .1) ** 2)
        r *= 1 + .025 * noise.noise(Vector((math.cos(a) * 1.6, math.sin(a) * 1.6, t * 5)) + s)
        r *= min(1.0, rr * 1.0001)                        # cap centres stay on the axis
        v.co = Vector((math.cos(a) * r * 1.1, math.sin(a) * r * .88, t * L))
    sl.rotation_mode = "QUATERNION"; sl.rotation_quaternion = Vector((0, 0, 1)).rotation_difference(axis_dir)
    sl.location = wrist + axis_dir * .01
    bpy.ops.object.transform_apply(location=True, rotation=True)
    return sl
