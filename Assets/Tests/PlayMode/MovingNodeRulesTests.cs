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

        /// <summary>Gercek hareketli node: PlatformController + LinearPingPongStrategy, child node. Spawn aninda baslangic noktasinda.</summary>
        private GameObject SpawnPlatformNode(Vector2 start, Vector2 end, float period)
        {
            var root = new GameObject("MovingNodeRoot");
            root.SetActive(false);
            root.transform.position = start;
            var body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.interpolation = RigidbodyInterpolation2D.Interpolate; // Node-Moving prefab'indaki gibi
            var strategy = root.AddComponent<New_Scripts.Platform.LinearPingPongStrategy>();
            SetField(strategy, "startPoint", start);
            SetField(strategy, "endPoint", end);
            SetField(strategy, "period", period);
            SetField(strategy, "phaseOffset", -Time.time);
            var controller = root.AddComponent<New_Scripts.Platform.PlatformController>();
            SetField(controller, "movementStrategy", strategy);
            GameObject node = SpawnNode(start);
            node.transform.SetParent(root.transform, true);
            root.SetActive(true);
            Track(root);
            return node;
        }

        private static void SetField(object target, string name, object value)
        {
            var f = target.GetType().GetField(name, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.IsNotNull(f, name);
            f.SetValue(target, value);
        }

        [UnityTest]
        public IEnumerator M7_GrabbingMovingNode_KeepsWorldVelocity()
        {
            // Node 10 b/sn ile saga gidiyor; oyuncu havada durgun. Tutununca oyuncu node'un hizini aninda almamali,
            // ip onu cekmeli (yakalama aninda dunya hizi korunur).
            EnterAir(Vector2.zero, AirEntry.Fall());
            GameObject node = SpawnPlatformNode(NodeStart + new Vector2(-1f, 0f), NodeStart + new Vector2(39f, 0f), 8f);
            yield return WaitFixedSteps(3);

            Input.RightStick = ((Vector2)node.transform.position - Player.PlayerRigidbody.position).normalized;
            Input.IsRightTriggerHeld = true;
            yield return WaitForState<SwingingState>();
            float grabX = Player.PlayerRigidbody.position.x;
            yield return WaitFixedSteps(3);

            Assert.Less(Player.PlayerRigidbody.position.x - grabX, 0.3f,
                "Hareketli node'u yakalayan oyuncu node'un hizini aninda almamali");
        }

        [UnityTest]
        public IEnumerator M8_PlatformNode_AnchorDeltaIsSteady()
        {
            // Sabit 10 b/sn giden gercek node (interpolation acik): anchor her adim ayni kaymayi gormeli.
            // Gurultulu kayma ataletle buyur ve salinimda titreme olarak gorunur.
            EnterAir(Vector2.zero, AirEntry.Fall());
            GameObject node = SpawnPlatformNode(NodeStart + new Vector2(-1f, 0f), NodeStart + new Vector2(39f, 0f), 8f);
            yield return WaitFixedSteps(3);
            Input.RightStick = ((Vector2)node.transform.position - Player.PlayerRigidbody.position).normalized;
            Input.IsRightTriggerHeld = true;
            yield return WaitForState<SwingingState>();
            yield return WaitFixedSteps(2);

            float expected = 10f * Time.fixedDeltaTime, min = float.MaxValue, max = float.MinValue;
            for (int i = 0; i < 20; i++)
            {
                yield return new WaitForFixedUpdate();
                float dx = Player.AnchorDelta(New_Scripts.Player.ActiveArm.Right).x;
                min = Mathf.Min(min, dx);
                max = Mathf.Max(max, dx);
            }

            Assert.AreEqual(expected, min, expected * 0.1f, $"en kucuk adim kaymasi {min}, en buyuk {max}");
            Assert.AreEqual(expected, max, expected * 0.1f, $"en kucuk adim kaymasi {min}, en buyuk {max}");
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

        [UnityTest]
        public IEnumerator M9_CarriedNodeVelocity_FadesOverDecayTime()
        {
            GameObject node = null;
            yield return GrabNodeAbove(NodeStart, n => node = n);
            yield return MoveEachFixedStep(node, new Vector2(0.2f, 0f), 10);

            float swingVelocityX = Player.Velocity.x;
            Input.IsRightTriggerHeld = false;
            yield return MoveEachFixedStep(node, new Vector2(0.2f, 0f), 1);
            yield return WaitForAir();
            yield return WaitFixedSteps(1);
            Assert.Greater(Player.Velocity.x - swingVelocityX, 6f, "Birakis aninda node hizi tasinmali");

            // Sonme suresi + pay: stick bos, kendi hiz hava surtunmesiyle zaten soner; node'dan gelen de sonmeli.
            float decay = Player.Stats.MovingNodeCarryDecayTime;
            yield return WaitFixedSteps(Mathf.CeilToInt((decay + 0.15f) / Time.fixedDeltaTime));

            AssertInAir();
            Assert.Less(Mathf.Abs(Player.Velocity.x), 2f, $"Tasinan yatay hiz {decay} sn icinde sonmeli");
        }

        [UnityTest]
        public IEnumerator M10_SimulatedNodeVelocity_OnlyAffectsRelease()
        {
            // Yerinde duran node, bırakışta +X'te 10 b/sn hareket ediyormus gibi davranir (test/tasarim araci).
            GameObject node = SpawnNode(NodeStart);
            var simulated = node.AddComponent<New_Scripts.Player.Nodes.SimulatedNodeVelocity>();
            SetField(simulated, "velocity", new Vector2(10f, 0f));

            EnterAir(Vector2.zero, AirEntry.Fall());
            Vector2 aim = (NodeStart - Player.PlayerRigidbody.position).normalized;
            Input.LeftStick = aim;
            Input.IsLeftTriggerHeld = true;
            yield return WaitForState<SwingingState>();
            Input.RightStick = aim;
            Input.IsRightTriggerHeld = true;
            yield return WaitForState<SlingshotState>();
            yield return WaitWhileState<SlingshotState>();
            float exitX = Player.PlayerRigidbody.position.x;
            yield return WaitForHitStopEnd();
            yield return WaitFixedSteps(1);

            Assert.AreEqual(NodeStart, (Vector2)node.transform.position, "Node yerinde durmali");
            Assert.Less(Mathf.Abs(exitX - Origin.x), 0.5f, "Slingshot sirasinda oyuncu tasinmamali");
            AssertInAir();
            Assert.Greater(Player.Velocity.x, 8f, "Birakista sahte node hizi aktarilmali");
        }

        /// <summary>Isaretci odasinda NodeStart'taki node'dan slingshot yapip yere iner; inis x'ini dondurur.</summary>
        private IEnumerator SlingshotAndLand(System.Action<float> landedX)
        {
            EnterAir(Vector2.zero, AirEntry.Fall());
            Vector2 aim = (NodeStart - Player.PlayerRigidbody.position).normalized;
            Input.LeftStick = Input.RightStick = aim;
            Input.IsLeftTriggerHeld = true;
            yield return WaitForState<SwingingState>();
            Input.IsRightTriggerHeld = true;
            yield return WaitForState<SlingshotState>();
            yield return WaitWhileState<SlingshotState>();
            Input.Clear();
            yield return WaitForGround();
            landedX(Player.PlayerRigidbody.position.x);
            yield return new WaitForFixedUpdate();
            yield return null;
        }

        private New_Scripts.Player.Nodes.SlingshotLandingMarkers SpawnMarkerRoom(out GameObject node)
        {
            var room = new GameObject("MarkerRoom");
            Track(room);
            var markers = room.AddComponent<New_Scripts.Player.Nodes.SlingshotLandingMarkers>();
            node = SpawnNode(NodeStart);
            node.transform.SetParent(room.transform, true);
            SpawnFloor(Origin.y - 0.5f);
            return markers;
        }

        [UnityTest]
        public IEnumerator T1_LandingMarker_DropsRealAndGhostLandings()
        {
            var markers = SpawnMarkerRoom(out _);
            yield return SlingshotAndLand(_ => { });

            var group = markers.transform.Find("Throw Node");
            Assert.IsNotNull(group, "Atis isaretcisi olusmali");
            foreach (string name in new[] { "Landing Real", "Landing Ghost +X", "Landing Ghost -X" })
                Assert.IsNotNull(group.Find(name)?.GetComponent<LineRenderer>(), $"{name} diregi olmali");
            Assert.Greater(markers.LastGhostOffset, 1f, "Hayalet inis gercek inisten ayrismali");
        }

        [UnityTest]
        public IEnumerator T2_GhostLanding_MatchesSimulatedNodeLanding()
        {
            // Normal node'dan atis: +X hayaletin yeri tahmin edilir.
            var markers = SpawnMarkerRoom(out GameObject node);
            float normalX = 0f;
            yield return SlingshotAndLand(x => normalX = x);
            float predictedX = normalX + markers.LastGhostOffset;

            // Ayni node bu kez birakista +10 hareket ediyormus gibi: gercek inis tahminle ortusmeli.
            yield return WaitForHitStopEnd();
            yield return ResetPlayerAt(Origin);
            yield return WaitFixedSteps(25);
            var simulated = node.AddComponent<New_Scripts.Player.Nodes.SimulatedNodeVelocity>();
            SetField(simulated, "velocity", new Vector2(10f, 0f));
            float simulatedX = 0f;
            yield return SlingshotAndLand(x => simulatedX = x);

            TestContext.WriteLine($"T2 normal {normalX - Origin.x:0.000}, tahmin +X {predictedX - Origin.x:0.000}, gercek +X {simulatedX - Origin.x:0.000}");
            Assert.AreEqual(predictedX, simulatedX, 0.25f,
                $"Hayalet +X inisi gercek +X inisiyle ortusmeli (normal {normalX - Origin.x:0.00}, ofset {markers.LastGhostOffset:0.00})");
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
            GameObject left = SpawnNode(Origin + new Vector2(-1.5f, 3f)), right = SpawnNode(Origin + new Vector2(1.5f, 3f));
            StartDualSwing(left, right);
            float startX = Player.PlayerRigidbody.position.x;

            Vector2 step = new Vector2(0.2f, 0f);
            for (int i = 0; i < 25; i++)
            {
                yield return new WaitForFixedUpdate();
                left.transform.position += (Vector3)step;
                right.transform.position += (Vector3)step;
                Physics2D.SyncTransforms();
            }
            yield return WaitFixedSteps(1);

            // Atalet oyuncuyu sallar (M6b), ama oyuncu node'larla birlikte ilerler.
            Assert.IsInstanceOf<DualSwingingState>(Player.CurrentState);
            Assert.Greater(Player.PlayerRigidbody.position.x - startX, 2.5f, "Oyuncu node'larla birlikte ilerlemeli");

            // Cift kancada Velocity dunyaya gore (yay hizi + node tasimasi): birakinca korunmali, node hizi kaybolmamali.
            float worldVelocityX = Player.Velocity.x;
            Input.IsLeftTriggerHeld = Input.IsRightTriggerHeld = false;
            yield return new WaitForFixedUpdate();
            left.transform.position += (Vector3)step;
            right.transform.position += (Vector3)step;
            yield return WaitForAir();
            yield return WaitFixedSteps(1);

            Assert.AreEqual(worldVelocityX, Player.Velocity.x, 2f, "Birakista dunya hizi korunmali (node hizi aktarilir)");
        }

        /// <summary>Iki node'a simetrik ve durgun cift kanca: node'a gore yatay hiz 0 baslar.</summary>
        private void StartDualSwing(GameObject left, GameObject right)
        {
            EnterAir(Vector2.zero, AirEntry.Fall());
            Player.Velocity = Vector2.zero;
            Player.AttachAnchor(ActiveArm.Left, left.transform.position, left.transform);
            Player.AttachAnchor(ActiveArm.Right, right.transform.position, right.transform);
            Input.IsLeftTriggerHeld = Input.IsRightTriggerHeld = true;
            Player.TransitionToState(new DualSwingingState(Player));
        }

        [UnityTest]
        public IEnumerator M6b_DualSwing_NodesStartingToMove_PlayerLagsBehind()
        {
            GameObject left = SpawnNode(Origin + new Vector2(-1.5f, 3f)), right = SpawnNode(Origin + new Vector2(1.5f, 3f));
            StartDualSwing(left, right);

            Vector2 step = new Vector2(0.2f, 0f);
            for (int i = 0; i < 12; i++)
            {
                yield return new WaitForFixedUpdate();
                left.transform.position += (Vector3)step;
                right.transform.position += (Vector3)step;
                Physics2D.SyncTransforms();
            }
            yield return WaitFixedSteps(1);

            float meanX = (left.transform.position.x + right.transform.position.x) * 0.5f;
            Assert.Less(Player.PlayerRigidbody.position.x - meanX, -0.3f,
                "Node'lar kalkinca oyuncu geride kalmali (node'larin ivmesi yaylara gecer)");
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
