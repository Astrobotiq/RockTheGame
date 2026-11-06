using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using New_Scripts.Platform;
using New_Scripts.Player;
using New_Scripts.Player.IFramePauseable;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>
    /// docs/bozulmamali-davranislar.md kurallarini gercek oyuncuyla sinar: NewScene yuklenir, oyuncu
    /// odalardan uzak bir alana tasinir, girdi FakeInput ile verilir ve geometri testin kendisi kurar.
    /// </summary>
    public abstract class PlayerRuleTestBase
    {
        protected const int GroundLayer = 6;
        protected const int GrappleLayer = 7;
        protected const float Timeout = 3f;

        /// <summary>Oyuncu kutusu 1x1; sensorler 0.04 birim uzagi gorur.</summary>
        protected const float WallTouchOffset = 0.53f;

        protected static readonly Vector2 Origin = new Vector2(5000f, 5000f);

        private readonly List<GameObject> _spawned = new List<GameObject>();

        protected PlayerController Player { get; private set; }
        protected FakeInput Input { get; private set; }

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            if (Object.FindFirstObjectByType<PlayerController>() == null)
            {
                SceneManager.LoadScene("NewScene");
                yield return null;
                yield return new WaitForSeconds(0.5f);
            }

            Player = Object.FindFirstObjectByType<PlayerController>();
            Assert.IsNotNull(Player, "NewScene icinde PlayerController bulunamadi");

            Input = new FakeInput();
            var inputField = typeof(PlayerController).GetField("<Input>k__BackingField",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(inputField, "PlayerController.Input alani bulunamadi");
            inputField.SetValue(Player, Input);

            yield return WaitForHitStopEnd();
            yield return ResetPlayerAt(Origin);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            Input.Clear();
            foreach (var go in _spawned)
                if (go != null) Object.Destroy(go);
            _spawned.Clear();

            yield return WaitForHitStopEnd();
            yield return ResetPlayerAt(Origin);
        }

        // ---- Oyuncu ----

        protected IEnumerator ResetPlayerAt(Vector2 position)
        {
            Player.OnStartRespawn();
            Teleport(position);
            Player.OnEndRespawn();
            Player.ResetWallSlideTime();
            yield return null;
        }

        protected void Teleport(Vector2 position)
        {
            Player.PlayerRigidbody.position = position;
            Player.transform.position = position;
            Physics2D.SyncTransforms();
        }

        protected IEnumerator WaitForHitStopEnd()
        {
            var manager = Object.FindFirstObjectByType<HitStopManager>();
            if (manager == null) yield break;

            var field = typeof(HitStopManager).GetField("_isHitStopActive",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(field, "HitStopManager._isHitStopActive alani bulunamadi");

            float deadline = Time.unscaledTime + Timeout;
            while ((bool)field.GetValue(manager) && Time.unscaledTime < deadline)
                yield return null;
        }

        protected IEnumerator WaitForState<T>() where T : IPlayerState
        {
            float deadline = Time.unscaledTime + Timeout;
            while (!(Player.CurrentState is T) && Time.unscaledTime < deadline)
                yield return null;

            Assert.IsInstanceOf<T>(Player.CurrentState, "Beklenen duruma gecilmedi");
        }

        protected IEnumerator WaitWhileState<T>() where T : IPlayerState
        {
            float deadline = Time.unscaledTime + Timeout;
            while (Player.CurrentState is T && Time.unscaledTime < deadline)
                yield return null;

            Assert.IsNotInstanceOf<T>(Player.CurrentState,
                $"Durumdan cikilmadi; konum farki {Player.PlayerRigidbody.position - Origin}, hiz {Player.Velocity}");
        }

        protected IEnumerator WaitFixedSteps(int count)
        {
            for (int i = 0; i < count; i++)
                yield return new WaitForFixedUpdate();
            yield return null;
        }

        protected IEnumerator PressDash()
        {
            Input.IsDashPressed = true;
            yield return null;
            Input.IsDashPressed = false;
        }

        protected IEnumerator PressJump()
        {
            Input.IsJumpPressed = true;
            yield return null;
            Input.IsJumpPressed = false;
        }

        // ---- Durum kurulumu ----

        protected IEnumerator LandOnFloor()
        {
            SpawnFloor(Player.PlayerRigidbody.position.y - 1f);
            yield return WaitForState<New_Scripts.Player.States.GroundedState>();
        }

        protected IEnumerator GrabWithRight(Vector2 nodePosition)
        {
            SpawnNode(nodePosition);
            Input.RightStick = (nodePosition - Player.PlayerRigidbody.position).normalized;
            Input.IsRightTriggerHeld = true;
            yield return WaitForState<New_Scripts.Player.States.SwingingState>();
        }

        protected IEnumerator GrabWithLeft(Vector2 nodePosition)
        {
            SpawnNode(nodePosition);
            Input.LeftStick = (nodePosition - Player.PlayerRigidbody.position).normalized;
            Input.IsLeftTriggerHeld = true;
            yield return WaitForState<New_Scripts.Player.States.SwingingState>();
        }

        /// <summary>Oyuncu tepki vermeden ipe tutunmus olarak baslar: oyuncu hemen durumuna gecer.</summary>
        protected void StartSwingFrom(Vector2 anchor)
        {
            Player.RightAnchor = anchor;
            Input.IsRightTriggerHeld = true;
            Player.TransitionToState(new New_Scripts.Player.States.SwingingState(Player, ActiveArm.Right));
        }

        protected IEnumerator ClimbRightWall()
        {
            SpawnWall(Player.PlayerRigidbody.position.x + WallTouchOffset);
            Input.IsRightBumperHeld = true;
            yield return WaitForState<New_Scripts.Player.States.WallClimbingState>();
        }

        protected IEnumerator SlideOnRightWall()
        {
            SpawnWall(Player.PlayerRigidbody.position.x + WallTouchOffset);
            Input.LeftStick = Vector2.right;
            yield return WaitForState<New_Scripts.Player.States.WallSlidingState>();
        }

        protected IEnumerator TouchLaunchPad()
        {
            LaunchPad source = null;
            foreach (var pad in Object.FindObjectsByType<LaunchPad>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                source = pad;
                break;
            }
            Assert.IsNotNull(source, "NewScene icinde LaunchPad bulunamadi");

            var clone = Object.Instantiate(source, Player.PlayerRigidbody.position, Quaternion.identity);
            _spawned.Add(clone.gameObject);
            clone.gameObject.SetActive(true);

            var offset = typeof(LaunchPad).GetField("detectionOffset", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(offset, "LaunchPad.detectionOffset alani bulunamadi");
            Teleport(clone.transform.TransformPoint((Vector2)offset.GetValue(clone)));

            yield return WaitFixedSteps(3);
        }

        // ---- Geometri ----

        /// <summary>Ust yuzeyi verilen yukseklikte olan genis zemin.</summary>
        protected GameObject SpawnFloor(float topY)
        {
            return SpawnBox("Floor", new Vector2(Player.PlayerRigidbody.position.x, topY - 0.5f), new Vector2(60f, 1f), 0f);
        }

        /// <summary>Sol yuzeyi verilen x'te olan dik duvar.</summary>
        protected GameObject SpawnWall(float leftX, float topY = float.NaN)
        {
            float y = Player.PlayerRigidbody.position.y;
            float top = float.IsNaN(topY) ? y + 30f : topY;
            float height = top - (y - 30f);
            return SpawnBox("Wall", new Vector2(leftX + 1f, top - height / 2f), new Vector2(2f, height), 0f);
        }

        protected GameObject SpawnBox(string name, Vector2 center, Vector2 size, float rotationZ)
        {
            var go = new GameObject(name) { layer = GroundLayer };
            go.transform.SetPositionAndRotation(center, Quaternion.Euler(0f, 0f, rotationZ));
            go.AddComponent<BoxCollider2D>().size = size;
            _spawned.Add(go);
            Physics2D.SyncTransforms();
            return go;
        }

        protected GameObject SpawnNode(Vector2 position)
        {
            var go = new GameObject("Node") { layer = GrappleLayer };
            go.transform.position = position;
            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.3f;
            collider.isTrigger = true;
            _spawned.Add(go);
            Physics2D.SyncTransforms();
            return go;
        }

        protected sealed class FakeInput : IInputReader
        {
            public Vector2 LeftStick { get; set; }
            public Vector2 RightStick { get; set; }
            public bool IsLeftTriggerHeld { get; set; }
            public bool IsRightTriggerHeld { get; set; }
            public bool IsJumpPressed { get; set; }
            public bool IsJumpHeld { get; set; }
            public bool IsDashPressed { get; set; }
            public bool IsLeftBumperHeld { get; set; }
            public bool IsRightBumperHeld { get; set; }

            public void Clear()
            {
                LeftStick = RightStick = Vector2.zero;
                IsLeftTriggerHeld = IsRightTriggerHeld = false;
                IsJumpPressed = IsJumpHeld = IsDashPressed = false;
                IsLeftBumperHeld = IsRightBumperHeld = false;
            }
        }
    }
}
