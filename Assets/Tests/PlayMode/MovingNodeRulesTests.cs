using System.Collections;
using New_Scripts.Player;
using New_Scripts.Player.States;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>docs/bozulmamali-davranislar.md "Hareketli node" (M1-M5). Tasarim: docs/hareketli-node-tasarimi.md.</summary>
    public class MovingNodeRulesTests : PlayerRuleTestBase
    {
        private static readonly Vector2 NodeStart = Origin + new Vector2(0f, 3f);

        /// <summary>Oyuncu durgun havada, node tam ustunde; sag kolla tutunur.</summary>
        private IEnumerator GrabNodeAbove(Vector2 nodePosition, System.Action<GameObject> onSpawned)
        {
            EnterAir(Vector2.zero, AirEntry.Fall());
            yield return GrabWithRight(nodePosition);
            onSpawned(FindSpawnedNode(nodePosition));
        }

        private static GameObject FindSpawnedNode(Vector2 position)
        {
            foreach (var col in Physics2D.OverlapPointAll(position, 1 << GrappleLayer))
                return col.gameObject;
            Assert.Fail("Node bulunamadi");
            return null;
        }

        [UnityTest]
        public IEnumerator M1_AnchorFollowsMovingNode()
        {
            GameObject node = null;
            yield return GrabNodeAbove(NodeStart, n => node = n);

            yield return MoveEachFixedStep(node, new Vector2(0.2f, 0f), 10);
            yield return WaitFixedSteps(1); // anchor node'un son kaymasını bir sonraki adımda alır

            Assert.IsNotNull(Player.RightAnchor);
            Assert.Less(Vector2.Distance(Player.RightAnchor.Value, node.transform.position), 0.01f);
            Assert.AreEqual(node.transform, Player.RightAnchorNode);
        }

        [UnityTest]
        public IEnumerator M1b_DeactivatedNode_KeepsLastAnchor()
        {
            GameObject node = null;
            yield return GrabNodeAbove(NodeStart, n => node = n);

            yield return MoveEachFixedStep(node, new Vector2(0.2f, 0f), 5);
            yield return WaitFixedSteps(1);
            Vector2 lastActive = node.transform.position;
            node.SetActive(false);
            yield return WaitFixedSteps(5);

            Assert.IsInstanceOf<SwingingState>(Player.CurrentState);
            Assert.IsNotNull(Player.RightAnchor);
            Assert.Less(Vector2.Distance(Player.RightAnchor.Value, lastActive), 0.01f);
        }

        [UnityTest]
        public IEnumerator M1c_Respawn_ClearsAnchorNode()
        {
            GameObject node = null;
            yield return GrabNodeAbove(NodeStart, n => node = n);

            Player.OnStartRespawn();

            Assert.IsNull(Player.RightAnchor);
            Assert.IsNull(Player.RightAnchorNode);
        }

        [UnityTest]
        public IEnumerator M2_SwingingPlayerIsCarried()
        {
            GameObject node = null;
            yield return GrabNodeAbove(NodeStart, n => node = n);

            float ropeLength = Vector2.Distance(Player.PlayerRigidbody.position, node.transform.position);
            float startX = Player.PlayerRigidbody.position.x;

            // 10 birim/sn, 25 adim = 5 birim. Atalet oyuncuyu sallar (M6), ama ip boyu korunur ve oyuncu node'la ilerler.
            yield return MoveEachFixedStep(node, new Vector2(0.2f, 0f), 25);
            yield return WaitFixedSteps(1);

            Assert.IsInstanceOf<SwingingState>(Player.CurrentState);
            Assert.AreEqual(ropeLength, Vector2.Distance(Player.PlayerRigidbody.position, node.transform.position), 0.15f,
                "Ip boyu korunmali");
            Assert.Greater(Player.PlayerRigidbody.position.x - startX, 2.5f, "Oyuncu node'la birlikte ilerlemeli");
        }

        [UnityTest]
        public IEnumerator M6_NodeStartingToMove_PlayerLagsBehind()
        {
            GameObject node = null;
            yield return GrabNodeAbove(NodeStart, n => node = n);

            // Duran node aniden 10 b/sn ile kalkar: atalet oyuncuyu geride birakir, ip salinim baslatir
            yield return MoveEachFixedStep(node, new Vector2(0.2f, 0f), 12);
            yield return WaitFixedSteps(1);

            Assert.IsInstanceOf<SwingingState>(Player.CurrentState);
            Assert.Less(Player.PlayerRigidbody.position.x - node.transform.position.x, -0.3f,
                "Node kalkinca oyuncu geride kalmali (node'un ivmesi salinima gecer)");
        }

        [UnityTest]
        public IEnumerator M2b_CarriedIntoWall_BreaksTheHook()
        {
            GameObject node = null;
            yield return GrabNodeAbove(NodeStart, n => node = n);
            SpawnWall(Origin.x + 2f);
            Player.UseDash(); // yere inmek hakki yenilerdi: harcanmis kalirsa karakter inmemis demektir

            yield return MoveEachFixedStep(node, new Vector2(0.2f, 0f), 20);
            yield return WaitWhileState<SwingingState>();

            AssertInAir();
            Assert.IsNull(Player.RightAnchor);
        }

        [UnityTest]
        public IEnumerator M3_ReleaseFromMovingNode_InheritsNodeVelocity()
        {
            GameObject node = null;
            yield return GrabNodeAbove(NodeStart, n => node = n);

            yield return MoveEachFixedStep(node, new Vector2(0.2f, 0f), 10);
            // Birakis: node hala hareket ederken. Sallanirken Velocity node'a gore hizdir; birakinca node hizi ustune eklenir.
            float swingVelocityX = Player.Velocity.x;
            Input.IsRightTriggerHeld = false;
            yield return MoveEachFixedStep(node, new Vector2(0.2f, 0f), 1);
            yield return WaitForAir();
            yield return WaitFixedSteps(1);

            Assert.Greater(Player.Velocity.x - swingVelocityX, 8f);
        }

        /// <summary>Sol kolla sol node'a, sonra sag kolla sag node'a tutunur.</summary>
        private IEnumerator GrabTwoNodes(Vector2 leftPos, Vector2 rightPos)
        {
            EnterAir(Vector2.zero, AirEntry.Fall());
            yield return GrabWithLeft(leftPos);
            SpawnNode(rightPos);
            Input.RightStick = (rightPos - Player.PlayerRigidbody.position).normalized;
            Input.IsRightTriggerHeld = true;
        }

        [UnityTest]
        public IEnumerator M4_DualSwing_CarriedAndLaunched()
        {
            // Simetrik ve durgun baslar: node'a gore yatay hiz 0 kalir, olcum salinim fazina bagli olmaz.
            Vector2 leftPos = Origin + new Vector2(-1.5f, 3f), rightPos = Origin + new Vector2(1.5f, 3f);
            GameObject left = SpawnNode(leftPos), right = SpawnNode(rightPos);
            EnterAir(Vector2.zero, AirEntry.Fall());
            Player.Velocity = Vector2.zero;
            Player.AttachAnchor(ActiveArm.Left, leftPos, left.transform);
            Player.AttachAnchor(ActiveArm.Right, rightPos, right.transform);
            Input.IsLeftTriggerHeld = Input.IsRightTriggerHeld = true;
            Player.TransitionToState(new DualSwingingState(Player));

            Vector2 step = new Vector2(0.2f, 0f);
            for (int i = 0; i < 25; i++)
            {
                yield return new WaitForFixedUpdate();
                left.transform.position += (Vector3)step;
                right.transform.position += (Vector3)step;
                Physics2D.SyncTransforms();
            }
            yield return WaitFixedSteps(1);

            Assert.IsInstanceOf<DualSwingingState>(Player.CurrentState);
            float meanX = (left.transform.position.x + right.transform.position.x) * 0.5f;
            Assert.Less(Mathf.Abs(Player.PlayerRigidbody.position.x - meanX), 0.5f);

            Input.IsLeftTriggerHeld = Input.IsRightTriggerHeld = false;
            yield return new WaitForFixedUpdate();
            left.transform.position += (Vector3)step;
            right.transform.position += (Vector3)step;
            yield return WaitForAir();
            yield return WaitFixedSteps(1);

            Assert.Greater(Player.Velocity.x, 8f);
        }

        [UnityTest]
        public IEnumerator M4b_DualSwing_OneMovingOneStatic_StaysFinite()
        {
            Vector2 leftPos = Origin + new Vector2(-1.5f, 3f), rightPos = Origin + new Vector2(1.5f, 3f);
            Player.UseSlingshot();
            yield return GrabTwoNodes(leftPos, rightPos);
            yield return WaitForState<DualSwingingState>();

            yield return MoveEachFixedStep(FindSpawnedNode(rightPos), new Vector2(0.2f, 0f), 25);

            Vector2 v = Player.Velocity, p = Player.PlayerRigidbody.position;
            Assert.IsFalse(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(p.x) || float.IsNaN(p.y));
            Assert.Less(v.magnitude, 100f);
            Assert.IsTrue(Player.CurrentState is DualSwingingState || Player.IsInNormalGround,
                $"durum {Player.CurrentState?.GetType().Name}");
        }

        [UnityTest]
        public IEnumerator M5_SlingshotFromMovingNode_InheritsNodeVelocity()
        {
            yield return GrabTwoNodes(NodeStart, NodeStart);
            yield return WaitForState<SlingshotState>();
            GameObject node = FindSpawnedNode(NodeStart);

            float deadline = Time.unscaledTime + Timeout;
            while (Player.CurrentState is SlingshotState && Time.unscaledTime < deadline)
            {
                yield return new WaitForFixedUpdate();
                node.transform.position += new Vector3(0.2f, 0f, 0f);
                Physics2D.SyncTransforms();
            }
            yield return WaitForHitStopEnd();
            yield return WaitFixedSteps(1);

            AssertInAir();
            Assert.Greater(Player.Velocity.x, 8f);
        }

        [UnityTest]
        public IEnumerator M1e_RopeReachesMovingNode()
        {
            var visuals = Object.FindFirstObjectByType<New_Scripts.Player.GrappleVisualManager>();
            Assert.IsNotNull(visuals, "GrappleVisualManager bulunamadi");
            var rope = (LineRenderer)typeof(New_Scripts.Player.GrappleVisualManager)
                .GetField("rightRopeRenderer", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .GetValue(visuals);

            GameObject node = null;
            yield return GrabNodeAbove(NodeStart, n => node = n);
            // Firlatma animasyonu (0.15 sn) bittikten sonra da node hareket ediyor
            yield return MoveEachFixedStep(node, new Vector2(0.2f, 0f), 20);
            yield return WaitFixedSteps(1);
            yield return new WaitForEndOfFrame();

            Vector2 ropeEnd = rope.GetPosition(rope.positionCount - 1);
            Assert.Less(Vector2.Distance(ropeEnd, Player.RightAnchor.Value), 0.05f,
                "Ip hareketli node'a ulasmali; anchor her adim degisince firlatma animasyonu bastan basliyordu");
        }

        [UnityTest]
        public IEnumerator M1d_RawAnchorAssignment_IsStatic()
        {
            GameObject node = SpawnNode(NodeStart);
            StartSwingFrom(NodeStart);

            Assert.IsNull(Player.RightAnchorNode);
            yield return MoveEachFixedStep(node, new Vector2(0.2f, 0f), 5);

            Assert.IsNotNull(Player.RightAnchor);
            Assert.Less(Vector2.Distance(Player.RightAnchor.Value, NodeStart), 0.01f);
        }
    }
}
