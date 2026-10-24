using System.Collections.Generic;
using UnityEngine;

namespace SeaGlass
{
    // Small illustrated HUD icons, drawn once at start from signed-distance shapes (no imported art).
    // Style: warm fills with a dark teal outline, matching the cream/teal/coral interface.
    public static class HudIcons
    {
        static readonly Dictionary<string, Texture2D> cache = new();
        static readonly Color Ink = new(.05f, .22f, .23f), Coral = new(.93f, .4f, .25f), Brass = new(.93f, .68f, .3f), Cream = new(.99f, .94f, .82f), Teal = new(.13f, .5f, .5f);
        const int Size = 64;

        public static Texture2D Get(string name)
        {
            if (cache.TryGetValue(name, out var t) && t) return t;
            t = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear, name = "HUD " + name };
            var px = new Color[Size * Size];
            for (int y = 0; y < Size; y++)
                for (int x = 0; x < Size; x++)
                {
                    var p = new Vector2((x + .5f) / Size * 2 - 1, (y + .5f) / Size * 2 - 1);   // -1..1, y up
                    px[y * Size + x] = Shade(name, p);
                }
            t.SetPixels(px); t.Apply(false, true);
            cache[name] = t; return t;
        }

        // Fill colour inside d<0 with an ink outline of width w, antialiased over one pixel.
        static Color Layer(Color under, float d, Color fill, float outline = .09f)
        {
            float aa = 2.2f / Size;
            float inside = Mathf.Clamp01(.5f - d / aa);
            float inFill = Mathf.Clamp01(.5f - (d + outline) / aa);
            var c = Color.Lerp(Ink, fill, inFill); c.a = inside;
            return new Color(Mathf.Lerp(under.r, c.r, c.a), Mathf.Lerp(under.g, c.g, c.a), Mathf.Lerp(under.b, c.b, c.a), Mathf.Max(under.a, c.a));
        }

        static float Circle(Vector2 p, Vector2 c, float r) => (p - c).magnitude - r;
        static float Box(Vector2 p, Vector2 c, Vector2 half, float round)
        {
            var q = new Vector2(Mathf.Abs(p.x - c.x), Mathf.Abs(p.y - c.y)) - half + Vector2.one * round;
            return new Vector2(Mathf.Max(q.x, 0), Mathf.Max(q.y, 0)).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0) - round;
        }
        static float Polygon(Vector2 p, Vector2[] v)
        {
            float d = Vector2.Dot(p - v[0], p - v[0]); float s = 1;
            for (int i = 0, j = v.Length - 1; i < v.Length; j = i, i++)
            {
                var e = v[j] - v[i]; var w = p - v[i];
                var b = w - e * Mathf.Clamp01(Vector2.Dot(w, e) / Vector2.Dot(e, e));
                d = Mathf.Min(d, Vector2.Dot(b, b));
                bool c1 = p.y >= v[i].y, c2 = p.y < v[j].y, c3 = e.x * w.y > e.y * w.x;
                if ((c1 && c2 && c3) || (!c1 && !c2 && !c3)) s = -s;
            }
            return s * Mathf.Sqrt(d);
        }

        static Color Shade(string name, Vector2 p)
        {
            var c = new Color(0, 0, 0, 0);
            switch (name)
            {
                case "xp":     // five-point star
                {
                    var v = new Vector2[10];
                    for (int i = 0; i < 10; i++) { float a = Mathf.PI / 2 + i * Mathf.PI / 5; float r = i % 2 == 0 ? .9f : .4f; v[i] = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r + new Vector2(0, -.05f); }
                    c = Layer(c, Polygon(p, v) - .04f, Coral);
                    c = Layer(c, Circle(p, new Vector2(-.18f, .2f), .09f), new Color(1, .85f, .7f), 0);
                    break;
                }
                case "caps":   // crimped bottle cap
                {
                    float a = Mathf.Atan2(p.y, p.x);
                    float crimp = p.magnitude - (.82f + .07f * Mathf.Cos(a * 14));
                    c = Layer(c, crimp, Brass);
                    c = Layer(c, Circle(p, Vector2.zero, .5f), new Color(.98f, .8f, .45f), .07f);
                    c = Layer(c, Circle(p, new Vector2(-.15f, .17f), .12f), new Color(1, .95f, .8f), 0);
                    break;
                }
                case "bucket":
                {
                    var body = new[] { new Vector2(-.7f, .35f), new Vector2(.7f, .35f), new Vector2(.5f, -.8f), new Vector2(-.5f, -.8f) };
                    float handle = Mathf.Abs(Circle(p, new Vector2(0, .3f), .62f)) - .07f;
                    if (p.y < .3f) handle = 1;
                    c = Layer(c, handle, Ink, 0);
                    c = Layer(c, Polygon(p, body) - .05f, Cream);
                    c = Layer(c, Box(p, new Vector2(0, .33f), new Vector2(.78f, .1f), .08f), Teal, .07f);
                    break;
                }
                case "truck":
                {
                    c = Layer(c, Box(p, new Vector2(-.22f, .02f), new Vector2(.58f, .36f), .1f), Cream);
                    c = Layer(c, Box(p, new Vector2(.55f, -.08f), new Vector2(.3f, .26f), .12f), Teal);
                    c = Layer(c, Box(p, new Vector2(.6f, .02f), new Vector2(.14f, .1f), .04f), new Color(.75f, .92f, .95f), .05f);
                    c = Layer(c, Circle(p, new Vector2(-.45f, -.45f), .2f), Ink, 0);
                    c = Layer(c, Circle(p, new Vector2(.5f, -.45f), .2f), Ink, 0);
                    c = Layer(c, Circle(p, new Vector2(-.45f, -.45f), .08f), Cream, 0);
                    c = Layer(c, Circle(p, new Vector2(.5f, -.45f), .08f), Cream, 0);
                    break;
                }
                case "gem":    // a tumbled glass pebble; tint with GUI.color
                {
                    var v = new[] { new Vector2(-.55f, .6f), new Vector2(.45f, .72f), new Vector2(.85f, .05f), new Vector2(.4f, -.7f), new Vector2(-.6f, -.62f), new Vector2(-.88f, .02f) };
                    c = Layer(c, Polygon(p, v) - .1f, new Color(.92f, .92f, .92f));
                    c = Layer(c, Circle(p, new Vector2(-.28f, .3f), .16f), Color.white, 0);
                    break;
                }
                case "spade":
                {
                    c = Layer(c, Box(p, new Vector2(0, -.45f), new Vector2(.08f, .45f), .06f), new Color(.75f, .55f, .35f));
                    c = Layer(c, Box(p, new Vector2(0, -.9f), new Vector2(.28f, .08f), .07f), Coral, .06f);
                    var blade = new[] { new Vector2(-.45f, .1f), new Vector2(.45f, .1f), new Vector2(.42f, .6f), new Vector2(0, .92f), new Vector2(-.42f, .6f) };
                    c = Layer(c, Polygon(p, blade) - .06f, Coral);
                    break;
                }
                case "sack":
                {
                    c = Layer(c, Circle(p, new Vector2(0, -.15f), .68f), new Color(.45f, .62f, .5f));
                    c = Layer(c, Box(p, new Vector2(0, .58f), new Vector2(.3f, .14f), .1f), Cream, .07f);
                    break;
                }
                case "sieve":
                {
                    c = Layer(c, Circle(p, new Vector2(-.12f, .05f), .7f), Teal);
                    float grid = Mathf.Min(Mathf.Abs(Mathf.Repeat(p.x * 4, 1) - .5f), Mathf.Abs(Mathf.Repeat(p.y * 4, 1) - .5f));
                    if (Circle(p, new Vector2(-.12f, .05f), .55f) < 0 && grid < .12f) c = Color.Lerp(c, Cream, .8f);
                    c = Layer(c, Box(p, new Vector2(.72f, -.55f), new Vector2(.28f, .09f), .08f), Ink, 0);
                    break;
                }
            }
            return c;
        }
    }
}
