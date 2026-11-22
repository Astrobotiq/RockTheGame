using System.Collections;
using New_Scripts.Flower;
using New_Scripts.Player;
using New_Scripts.Player.States;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>
    /// Zamanlı çiçek node'u (FlowerCycle): kapalıyken kancayla tutulmaz, açıkken tutulur, kapanırken kanca kopar ve
    /// oyuncu çiçekten dışa itilir. Çiçek NewScene'deki NodeDetector ile gerçek cast edilir.
    /// </summary>
    public class FlowerNodeRulesTests : PlayerRuleTestBase
    {
        private static readonly Vector2 FlowerOffset = new Vector2(4f, 4f);

        private static FlowerCycleTimings Timings(float closed, float open) => new FlowerCycleTimings
        {
            closed = closed,
            opening = 0.2f,
            open = open,
            warning = 0.3f,
            snap = 0.1f,
            hint = 0.1f,
            grabAt = 0.05f,
            coyote = 0.05f
        };

        /// <summary>Çiçeği oyuncunun çapraz üstüne koyar; <paramref name="phaseOffset"/> döngünün neresinden başlayacağını seçer.</summary>
        private GameObject SpawnFlower(FlowerCycleTimings timings, float phaseOffset, float releaseImpulse = 6f)
        {
            var root = new GameObject("Flower");
            root.SetActive(false);
            root.transform.position = Origin + FlowerOffset;

            var cycle = root.AddComponent<FlowerCycle>();   // RequireComponent FlowerBloom'u da ekler
            var node = new GameObject("Node") { layer = GrappleLayer };
            node.transform.SetParent(root.transform, false);
            var collider = node.AddComponent<CircleCollider2D>();
            collider.radius = 0.4f;
            collider.isTrigger = true;

            SetField(cycle, "timings", timings);
            SetField(cycle, "phaseOffset", phaseOffset);
            SetField(cycle, "nodeCollider", collider);
            SetField(cycle, "releaseImpulse", releaseImpulse);

            Track(root);
            root.SetActive(true);
            Physics2D.SyncTransforms();
            return root;
        }

        [UnityTest]
        public IEnumerator ClosedFlower_CannotBeGrabbed()
        {
            SpawnFlower(Timings(closed: 5f, open: 5f), phaseOffset: 0f);

            Input.RightStick = FlowerOffset.normalized;
            Input.IsRightTriggerHeld = true;
            yield return WaitFixedSteps(10);

            Assert.IsNotInstanceOf<SwingingState>(Player.CurrentState, "Kapalı çiçek tutulmamalı");
        }

        [UnityTest]
        public IEnumerator OpenFlower_CanBeGrabbed()
        {
            var t = Timings(closed: 0.3f, open: 5f);
            var flower = SpawnFlower(t, phaseOffset: t.closed + t.opening + 0.1f);
            var nodeObject = flower.transform.Find("Node").gameObject;

            yield return null;
            var cycle = flower.GetComponent<FlowerCycle>();
            Assert.IsTrue(cycle.IsGrabbable, $"Açık fazda tutulabilir olmalı; faz {cycle.Phase}");
            Assert.IsTrue(nodeObject.GetComponent<Collider2D>().enabled, "Tutulabilirken node collider'ı açık olmalı");

            yield return GrabExistingWithRight(nodeObject);

            Assert.IsInstanceOf<SwingingState>(Player.CurrentState);
        }

        [UnityTest]
        public IEnumerator ClosingFlower_ReleasesTheGrapple()
        {
            // Açık fazın başından ~1,3 sn önce kapanır: tutunduktan sonra çiçek kapanana kadar sarkarız.
            var t = Timings(closed: 0.3f, open: 1f);
            var flower = SpawnFlower(t, phaseOffset: t.closed + t.opening + 0.05f);

            yield return GrabExistingWithRight(flower.transform.Find("Node").gameObject);
            yield return WaitWhileState<SwingingState>();

            Assert.IsFalse(Player.RightAnchor.HasValue, "Kapanan çiçekten kanca kopmalı");
            Assert.IsNull(Player.RightAnchorNode);
        }

        [UnityTest]
        public IEnumerator GrabbingFlower_NotifiesGrabbedWithArmAndPoint()
        {
            var t = Timings(closed: 0.3f, open: 5f);
            var flower = SpawnFlower(t, phaseOffset: t.closed + t.opening + 0.1f);
            var cycle = flower.GetComponent<FlowerCycle>();
            int grabbed = 0;
            ActiveArm arm = ActiveArm.None;
            Vector2 point = Vector2.zero;
            cycle.Grabbed += (a, p) => { grabbed++; arm = a; point = p; };

            yield return GrabExistingWithRight(flower.transform.Find("Node").gameObject);

            Assert.AreEqual(1, grabbed, "Tutunma tek kez bildirilmeli");
            Assert.AreEqual(ActiveArm.Right, arm);
            Assert.IsTrue(cycle.IsHeldBy(ActiveArm.Right));
            Assert.IsFalse(cycle.IsHeldBy(ActiveArm.Left));
            Assert.Less(Vector2.Distance(point, flower.transform.position), 0.5f, $"Tutunma noktası çiçekte olmalı: {point}");
        }

        [UnityTest]
        public IEnumerator ClosingFlower_NotifiesReleased()
        {
            var t = Timings(closed: 0.3f, open: 1f);
            var flower = SpawnFlower(t, phaseOffset: t.closed + t.opening + 0.05f);
            var cycle = flower.GetComponent<FlowerCycle>();
            int released = 0;
            cycle.Released += _ => released++;

            yield return GrabExistingWithRight(flower.transform.Find("Node").gameObject);
            Assert.IsTrue(cycle.IsHeld);
            yield return WaitWhileState<SwingingState>();
            yield return null;   // bırakma bildirimi LateUpdate'te yoklanır

            Assert.AreEqual(1, released, "Kapanışta bırakma tek kez bildirilmeli");
            Assert.IsFalse(cycle.IsHeld);
        }

        [UnityTest]
        public IEnumerator ClosingFlower_PushesThePlayerAway()
        {
            var t = Timings(closed: 0.3f, open: 1f);
            var flower = SpawnFlower(t, phaseOffset: t.closed + t.opening + 0.05f, releaseImpulse: 30f);

            yield return GrabExistingWithRight(flower.transform.Find("Node").gameObject);
            yield return WaitWhileState<SwingingState>();

            Vector2 away = (Player.PlayerRigidbody.position - (Vector2)flower.transform.position).normalized;
            Assert.Greater(Vector2.Dot(Player.Velocity, away), 15f,
                $"Çiçekten dışa itilmeli; hız {Player.Velocity}, yön {away}");
        }
    }
}
