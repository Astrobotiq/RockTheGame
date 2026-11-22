using System.Collections;
using System.Collections.Generic;
using New_Scripts.Flower;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>
    /// Yaprakların zaman alanındaki gecikmesi (T3): açılışta yapraklar aynı anda değil art arda açılır (kare başına
    /// yaprakların shader'a giden ilerlemesi birbirinden ayrışır), sonunda hepsi açıktır.
    /// </summary>
    public class FlowerBloomStaggerTests
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
                // Yaprak: _Open ve _Length var, iplik tacı (_BaseRadius) yok.
                if (block.HasFloat(OpenId) && block.HasFloat(LengthId) && !block.HasFloat(BaseRadiusId))
                    list.Add(block.GetFloat(OpenId));
            }
            return list;
        }

        [UnityTest]
        public IEnumerator Opening_PetalsOpenOneAfterAnother()
        {
            _root = new GameObject("Flower");
            _root.SetActive(false);
            var cycle = _root.AddComponent<FlowerCycle>();
            var timings = new FlowerCycleTimings
            {
                closed = 0.2f, opening = 0.3f, open = 5f, warning = 0.3f, snap = 0.1f, hint = 0.05f, grabAt = 0.05f, coyote = 0.05f
            };
            typeof(FlowerCycle).GetField("timings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cycle, timings);
            _root.SetActive(true);

            // Her yaprağın ilerlemesi ilk kez 0,5'i geçtiği an: zaman alanında ne kadar yayılmışlar?
            float[] crossed = { -1f, -1f, -1f, -1f, -1f };
            int petals = 0;
            for (float t = 0f; t < 1.2f; t += Time.deltaTime)
            {
                yield return null;
                var p = PetalProgress(_root);
                petals = p.Count;
                for (int i = 0; i < p.Count && i < crossed.Length; i++)
                    if (crossed[i] < 0f && p[i] >= 0.5f) crossed[i] = Time.time;
            }

            float first = float.MaxValue, last = float.MinValue;
            foreach (float c in crossed)
            {
                Assert.GreaterOrEqual(c, 0f, "Her yaprak açılmalı");
                first = Mathf.Min(first, c);
                last = Mathf.Max(last, c);
            }
            Assert.AreEqual(5, petals, "5 yaprak okunmalı");
            Assert.Greater(last - first, 0.07f, $"Yapraklar %50 ilerlemeyi art arda geçmeli; yayılma {last - first:F3} sn");
            foreach (float v in PetalProgress(_root)) Assert.Greater(v, 0.95f, "Sonunda hepsi açık");
        }
    }
}
