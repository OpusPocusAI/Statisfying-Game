using UnityEngine;

namespace SeaGlass.Editor
{
    // The authored coastline: where the sand meets the sea (world X) along the beach (world Z).
    // Bays hold the cove landmarks; headlands sit between them. Everything seaward of the
    // invisible boundary at X = -33 is scenery, so playable land is unchanged.
    // Keep in sync with ShoreX() in Assets/Art/TidalWater.shader.
    public static class CoastShape
    {
        public static readonly Vector2[] Points =
        {
            new(-45, -35.0f), new(-20, -37.6f), new(0, -34.4f), new(18, -33.9f), new(33, -33.6f),
            new(48, -38.2f), new(63, -33.8f), new(85, -38.4f), new(105, -33.8f), new(127, -37.4f),
            new(150, -35.0f), new(174, -37.8f), new(205, -35.0f),
        };

        public static float ShoreX(float z)
        {
            int n = Points.Length;
            if (z <= Points[0].x) return Points[0].y;
            if (z >= Points[n - 1].x) return Points[n - 1].y;
            int i = 0;
            while (z > Points[i + 1].x) i++;
            float p0 = Points[Mathf.Max(i - 1, 0)].y, p1 = Points[i].y, p2 = Points[i + 1].y, p3 = Points[Mathf.Min(i + 2, n - 1)].y;
            float t = (z - Points[i].x) / (Points[i + 1].x - Points[i].x);
            // Catmull-Rom.
            return .5f * (2 * p1 + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t * t + (-p0 + 3 * p1 - 3 * p2 + p3) * t * t * t);
        }

        // Height of the sloping wet-sand shoulder (matches ShoreTransitionPolish's profile).
        public static float ShoulderY(float x, float z)
        {
            float shore = ShoreX(z);
            float[] xs = { -30.65f, -31.45f, -32.10f, Mathf.Lerp(-32.10f, shore, .28f), Mathf.Lerp(-32.10f, shore, .65f), shore, shore - 2 };
            float[] ys = { -.088f, -.088f, -.089f, -.23f, -.54f, -.85f, -1.3f };
            if (x >= xs[0]) return -.1f;
            for (int i = 0; i < xs.Length - 1; i++)
                if (x <= xs[i] && x >= xs[i + 1]) return Mathf.Lerp(ys[i], ys[i + 1], (xs[i] - x) / (xs[i] - xs[i + 1]));
            return -1.3f;
        }

        // Z positions of the headlands (most seaward points).
        public static readonly float[] Headlands = { -20, 48, 85, 127, 174 };
    }
}
