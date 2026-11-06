using System.Collections.Generic;
using NUnit.Framework;
using New_Scripts.LevelChange;
using New_Scripts.Multiplayer.Flags;
using New_Scripts.Multiplayer.Messages;
using UnityEditor;
using UnityEngine;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Yarış bayrağı: işaretin yerinde trigger olarak kurulur, ilk dokunuşta bir kez bildirir; alınmış bayrak bildirmez.
    /// Yalnız yarış modunda kurulur. docs/superpowers/plans/2026-11-06-mp5-flags.md → Task 3–4.
    /// </summary>
    public class RaceFlagTests
    {
        private GameObject _room;
        private RaceFlagPoint _point;
        private RaceFlagSettingsSO _settings;

        [SetUp]
        public void SetUp()
        {
            _room = new GameObject("Room-5");
            var room = _room.AddComponent<Room>();
            var so = new SerializedObject(room);
            so.FindProperty("roomId").intValue = 5;
            so.ApplyModifiedPropertiesWithoutUndo();
            var pointGo = new GameObject(RaceFlagPoint.ObjectName);
            pointGo.transform.SetParent(_room.transform, false);
            pointGo.transform.position = new Vector3(3f, 4f, 0f);
            _point = pointGo.AddComponent<RaceFlagPoint>();
            _settings = ScriptableObject.CreateInstance<RaceFlagSettingsSO>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_room);
            Object.DestroyImmediate(_settings);
        }

        [Test]
        public void Reach_FirstTime_NotifiesRoomIdOnce()
        {
            var reached = new List<int>();
            var flag = RaceFlag.Create(_point, _settings, captured: false, reached.Add);

            flag.Reach();
            flag.Reach();

            CollectionAssert.AreEqual(new[] { 5 }, reached);
            Assert.IsTrue(flag.IsCaptured);
        }

        [Test]
        public void Reach_AlreadyCaptured_DoesNotNotify()
        {
            var reached = new List<int>();
            var flag = RaceFlag.Create(_point, _settings, captured: true, reached.Add);

            flag.Reach();

            Assert.IsEmpty(reached);
        }

        [Test]
        public void Create_IsTriggerChildOfPoint_AtPointPosition()
        {
            var flag = RaceFlag.Create(_point, _settings, captured: false, _ => { });

            Assert.AreEqual(_point.transform, flag.transform.parent);
            Assert.AreEqual(_point.transform.position, flag.transform.position);
            var box = flag.GetComponent<BoxCollider2D>();
            Assert.IsNotNull(box);
            Assert.IsTrue(box.isTrigger);
            Assert.IsNotNull(flag.GetComponent<SpriteRenderer>());
        }

        [Test]
        public void ShouldPlaceFlags_RaceOnly()
        {
            Assert.IsTrue(RaceFlag.ShouldPlaceFlags(RaceMode.Race));
            Assert.IsFalse(RaceFlag.ShouldPlaceFlags(RaceMode.Coop));
        }
    }
}
