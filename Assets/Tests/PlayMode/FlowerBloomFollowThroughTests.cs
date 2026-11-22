using System.Collections;
using System.Collections.Generic;
using New_Scripts.Flower;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>
    /// Yaprak ucu follow-through (T4): açılırken yaprak ucu eğriliği (_BaseCurl) yaprak oturduktan sonra da bir süre
    /// daha artar (aşım), sonra son değere yerleşir; kapanışta uç yaprağı geride bırakmaz.
    /// </summary>
    public class FlowerBloomFollowThroughTests
    {
        private static readonly int BaseCurlId = Shader.PropertyToID("_BaseCurl");
        private static readonly int LengthId = Shader.PropertyToID("_Length");
        private static readonly int BaseRadiusId = Shader.PropertyToID("_BaseRadius");

        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.Destroy(_root);
        }

        private static float PetalCurl(GameObject root)
        {
            var block = new MaterialPropertyBlock();
            foreach (var r in root.GetComponentsInChildren<MeshRenderer>())
            {
                r.GetPropertyBlock(block);
                if (block.HasFloat(BaseCurlId) && block.HasFloat(LengthId) && !block.HasFloat(BaseRadiusId))
                    return block.GetFloat(BaseCurlId);   // ilk yaprak yeterli (hepsi aynı yayı paylaşır)
            }
            return float.NaN;
        }

        private GameObject Spawn(FlowerCycleTimings timings)
        {
            _root = new GameObject("Flower");
            _root.SetActive(false);
            var cycle = _root.AddComponent<FlowerCycle>();
            typeof(FlowerCycle).GetField("timings", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(cycle, timings);
            _root.SetActive(true);
            return _root;
        }

        [UnityTest]
        public IEnumerator Opening_TipCurlOvershootsThenSettles()
        {
            Spawn(new FlowerCycleTimings
            {
                closed = 0.2f, opening = 0.3f, open = 10f, warning = 0.3f, snap = 0.1f, hint = 0.05f, grabAt = 0.05f, coyote = 0.05f
            });

            var curls = new List<float>();
            for (float t = 0f; t < 2.5f; t += Time.deltaTime)
            {
                yield return null;
                curls.Add(PetalCurl(_root));
            }

            float final = curls[curls.Count - 1];
            float peak = float.MinValue;
            foreach (float c in curls) peak = Mathf.Max(peak, c);

            Assert.Greater(final, 0.1f, "Açık yaprak eğrilmiş olmalı");
            Assert.Greater(peak, final * 1.1f, $"Uç, yerleşmeden önce son eğriliği aşmalı; tepe {peak:F3}, son {final:F3}");
            Assert.AreEqual(final, curls[curls.Count - 10], final * 0.03f, "Sonunda yerleşmiş olmalı");
        }
    }
}
