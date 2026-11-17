using System.Collections;
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
