using System.Linq;
using UnityEngine;

namespace SeaGlass
{
    // The studio recovers with each completed mosaic panel (ART_DIRECTION.md 5.10):
    // wilted entry planters green up, flowers open panel by panel and festoon lights come on.
    // Presentation only: reads progress, never writes it.
    public class StudioRestoration : MonoBehaviour
    {
        public Renderer[] leaves;
        public Transform[] flowers;          // flowers[i] opens once panel count > (i % 3)
        public Renderer[] bulbs;             // bulbs[i] lights once panel count > (i % 3)
        public float flowerSize = 1.2f;      // authored open size (flowers are saved closed)
        public Color wilted = new(.56f, .46f, .3f), healthy = new(.12f, .36f, .3f), bulbGlow = new(1f, .72f, .38f);
        float shown;
        MaterialPropertyBlock block;

        void Update()
        {
            var s = GameSession.Instance;
            float target = s ? s.progress.completed.Count(c => c) : 0;
            shown = Mathf.MoveTowards(shown, target, Time.deltaTime * .6f);
            Apply(shown);
        }

        public void Apply(float level)
        {
            block ??= new MaterialPropertyBlock();
            var leaf = Color.Lerp(wilted, healthy, Mathf.Clamp01(level));
            foreach (var r in leaves) { if (!r) continue; r.GetPropertyBlock(block); block.SetColor("_BaseColor", leaf); r.SetPropertyBlock(block); }
            for (int i = 0; i < flowers.Length; i++)
            {
                if (!flowers[i]) continue;
                float open = Mathf.SmoothStep(0, 1, Mathf.Clamp01(level - i % 3));
                flowers[i].gameObject.SetActive(open > .01f);
                flowers[i].localScale = Vector3.one * flowerSize * open;
            }
            for (int i = 0; i < bulbs.Length; i++)
            {
                if (!bulbs[i]) continue;
                float on = Mathf.Clamp01(level - i % 3);
                bulbs[i].GetPropertyBlock(block); block.SetColor("_EmissionColor", bulbGlow * on * 2.2f); bulbs[i].SetPropertyBlock(block);
            }
        }
    }
}
