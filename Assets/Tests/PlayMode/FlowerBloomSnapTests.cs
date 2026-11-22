using System.Collections;
using System.Reflection;
using New_Scripts.Flower;
using New_Scripts.Player;
using New_Scripts.Player.IFramePauseable;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>
    /// Snap darbesi (T6): kapanışta model ezilip geri seker (thud), uyarının son anında çiçek snap'ten önce hafifçe gerilir
    /// (nefes çek), tutulan çiçek kapanırken (bayrak açıksa) hit-stop istenir.
    /// </summary>
    public class FlowerBloomSnapTests
    {
        private const BindingFlags NonPublic = BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly FlowerCycleTimings Timings = new FlowerCycleTimings
        {
            closed = 0.1f, opening = 0.3f, open = 0.8f, warning = 1.0f, snap = 0.6f, hint = 0.05f, grabAt = 0.05f, coyote = 0.05f
        };

        private GameObject _root;
        private System.Action<float> _hitStopSpy;

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.Destroy(_root);
            // Statik olaya yalnız kendi dinleyicimizi ekleyip çıkarırız: atama (= null) sahnedeki HitStopManager'ın aboneliğini silerdi.
            if (_hitStopSpy != null) HitStopEvents.RequestHitStop -= _hitStopSpy;
            _hitStopSpy = null;
        }

        private FlowerCycle Spawn(bool hitStop = false)
        {
            _root = new GameObject("Flower");
            _root.SetActive(false);
            var cycle = _root.AddComponent<FlowerCycle>();
            typeof(FlowerCycle).GetField("timings", NonPublic).SetValue(cycle, Timings);
            typeof(FlowerCycle).GetField("snapHitStop", NonPublic).SetValue(cycle, hitStop);
            _root.SetActive(true);
            return cycle;
        }

        [UnityTest]
        public IEnumerator Closing_ModelSquashesThenSettlesBack()
        {
            var cycle = Spawn();
            var bloom = _root.GetComponent<FlowerBloom>();
            var model = (Transform)typeof(FlowerBloom).GetField("model", NonPublic).GetValue(bloom);

            float minScale = float.MaxValue;
            bool closing = false;
            for (float t = 0f; t < 4f; t += Time.deltaTime)
            {
                yield return null;
                if (cycle.Phase == FlowerPhase.Closing) closing = true;
                if (closing) minScale = Mathf.Min(minScale, model.localScale.x);
            }

            Assert.IsTrue(closing, "Kapanış fazına girilmeli");
            Assert.Less(minScale, 0.95f, $"Snap'te model ezilmeli (thud); en küçük ölçek {minScale:F3}");
        }

        [UnityTest]
        public IEnumerator Warning_FlowerStretchesOpenJustBeforeTheSnap()
        {
            var cycle = Spawn();
            var bloom = _root.GetComponent<FlowerBloom>();

            float minOpen = float.MaxValue, lastOpen = 0f;
            for (float t = 0f; t < 4f; t += Time.deltaTime)
            {
                yield return null;
                if (cycle.Phase != FlowerPhase.Warning) continue;
                minOpen = Mathf.Min(minOpen, bloom.Open);
                lastOpen = bloom.Open;
            }

            Assert.Greater(lastOpen - minOpen, 0.04f,
                $"Uyarının sonunda çiçek snap'ten önce hafifçe açılmalı (nefes çek); en düşük {minOpen:F3}, son {lastOpen:F3}");
        }

        [UnityTest]
        public IEnumerator HeldFlower_RequestsHitStopWhenClosing_OnlyIfFlagIsOn()
        {
            int requests = 0;
            _hitStopSpy = _ => requests++;
            HitStopEvents.RequestHitStop += _hitStopSpy;

            var cycle = Spawn(hitStop: true);
            cycle.OnGrabbed(ActiveArm.Right, Vector2.zero);
            for (float t = 0f; t < 3f && cycle.Phase != FlowerPhase.Closing; t += Time.deltaTime) yield return null;
            yield return null;
            Assert.AreEqual(1, requests, "Tutulan çiçek kapanırken hit-stop istenmeli");
            Object.Destroy(_root);
            yield return null;

            requests = 0;
            cycle = Spawn(hitStop: false);
            cycle.OnGrabbed(ActiveArm.Right, Vector2.zero);
            for (float t = 0f; t < 3f && cycle.Phase != FlowerPhase.Closing; t += Time.deltaTime) yield return null;
            yield return null;
            Assert.AreEqual(0, requests, "Bayrak kapalıyken hit-stop istenmemeli");
        }

        [UnityTest]
        public IEnumerator UnheldFlower_NeverRequestsHitStop()
        {
            int requests = 0;
            _hitStopSpy = _ => requests++;
            HitStopEvents.RequestHitStop += _hitStopSpy;

            var cycle = Spawn(hitStop: true);
            for (float t = 0f; t < 3f && cycle.Phase != FlowerPhase.Closing; t += Time.deltaTime) yield return null;
            yield return null;
            Assert.AreEqual(0, requests, "Tutulmayan çiçek hit-stop istememeli");
        }
    }
}
