using System.Collections;
using System.Collections.Generic;
using New_Scripts.Flower;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>
    /// Uyarı sayacı (T5): uyarıda yapraklar sırayla katlanır; uyarı öncesi açık fazda yapraklar birbirine eşittir.
    /// </summary>
    public class FlowerBloomWarnTests
    {
        private static readonly int OpenId = Shader.PropertyToID("_Open");
        private static readonly int LengthId = Shader.PropertyToID("_Length");
        private static readonly int BaseRadiusId = Shader.PropertyToID("_BaseRadius");

        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.Destroy(_root);
        }

        private static List<float> PetalProgress(GameObject root)
        {
            var list = new List<float>();
            var block = new MaterialPropertyBlock();
            foreach (var r in root.GetComponentsInChildren<MeshRenderer>())
            {
                r.GetPropertyBlock(block);
                if (block.HasFloat(OpenId) && block.HasFloat(LengthId) && !block.HasFloat(BaseRadiusId))
                    list.Add(block.GetFloat(OpenId));
            }
            return list;
        }

        private static float Spread(List<float> v)
        {
            float lo = float.MaxValue, hi = float.MinValue;
            foreach (float x in v) { lo = Mathf.Min(lo, x); hi = Mathf.Max(hi, x); }
            return hi - lo;
        }

        [UnityTest]
        public IEnumerator Warning_FoldsPetalsOneByOne()
        {
            _root = new GameObject("Flower");
            _root.SetActive(false);
            var cycle = _root.AddComponent<FlowerCycle>();
            typeof(FlowerCycle).GetField("timings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cycle, new FlowerCycleTimings
                {
                    closed = 0.1f, opening = 0.3f, open = 1.2f, warning = 1.2f, snap = 0.3f, hint = 0.05f, grabAt = 0.05f, coyote = 0.05f
                });
            _root.SetActive(true);

            float openSpread = float.MaxValue, warnSpread = 0f;
            float openPhaseEnd = 0f;
            for (float t = 0f; t < 2.6f; t += Time.deltaTime)
            {
                yield return null;
                var p = PetalProgress(_root);
                if (p.Count != 5) continue;
                if (cycle.Phase == FlowerPhase.Open) openPhaseEnd = t;
                // Açık fazın son 0,25 sn'si: yapraklar yerleşmiş, uyarı yok.
                if (cycle.Phase == FlowerPhase.Open && t > 1.3f) openSpread = Mathf.Min(openSpread, Spread(p));
                if (cycle.Phase == FlowerPhase.Warning) warnSpread = Mathf.Max(warnSpread, Spread(p));
            }

            Assert.Less(openSpread, 0.05f, $"Uyarı öncesi yapraklar eşit olmalı (yayılma {openSpread:F3}, açık faz sonu {openPhaseEnd:F2} sn)");
            Assert.Greater(warnSpread, 0.12f, $"Uyarıda yapraklar farklı katlanmalı (yayılma {warnSpread:F3})");
        }
    }
}
