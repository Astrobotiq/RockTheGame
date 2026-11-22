using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using New_Scripts.Audio;
using New_Scripts.Flower;
using New_Scripts.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>
    /// Çiçek sesi (T7): uyarıda yükselen tıklar, tutunma ve snap cue'su; klipsiz cue sessiz kalır.
    /// Titreşim (T8) gerçek gamepad ister, burada yalnızca HapticManager yokken hata vermediği doğrulanır.
    /// </summary>
    public class FlowerFeedbackTests
    {
        private const BindingFlags NonPublic = BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly FlowerCycleTimings Timings = new FlowerCycleTimings
        {
            closed = 0.1f, opening = 0.3f, open = 0.6f, warning = 1.0f, snap = 0.5f, hint = 0.05f, grabAt = 0.05f, coyote = 0.05f
        };

        private readonly List<Object> _created = new List<Object>();
        private GameObject _root;

        [TearDown]
        public void TearDown()
        {
            if (_root != null) Object.Destroy(_root);
            foreach (var o in _created) if (o != null) Object.Destroy(o);
            _created.Clear();
        }

        private static void SetField(object target, string name, object value) =>
            target.GetType().GetField(name, NonPublic).SetValue(target, value);

        private AudioCueSO MakeCue(bool withClip)
        {
            var cue = ScriptableObject.CreateInstance<AudioCueSO>();
            _created.Add(cue);
            if (withClip)
            {
                var clip = AudioClip.Create("test", 64, 1, 8000, false);
                _created.Add(clip);
                SetField(cue, "audioClips", new[] { clip });
            }
            return cue;
        }

        private FlowerCycle Spawn(FlowerAudioSettingsSO settings, AudioCuePlayEventChannelSO channel)
        {
            _root = new GameObject("Flower");
            _root.SetActive(false);
            var cycle = _root.AddComponent<FlowerCycle>();
            SetField(cycle, "timings", Timings);
            var feedback = _root.AddComponent<FlowerFeedback>();
            SetField(feedback, "audioSettings", settings);
            SetField(feedback, "sfxPlayChannel", channel);
            _root.SetActive(true);
            return cycle;
        }

        private (FlowerAudioSettingsSO settings, AudioCuePlayEventChannelSO channel, AudioCueSO tick, AudioCueSO grab, AudioCueSO snap) Setup(bool clips)
        {
            var settings = ScriptableObject.CreateInstance<FlowerAudioSettingsSO>();
            var channel = ScriptableObject.CreateInstance<AudioCuePlayEventChannelSO>();
            _created.Add(settings);
            _created.Add(channel);
            settings.warnTickCue = MakeCue(clips);
            settings.grabCue = MakeCue(clips);
            settings.snapCue = MakeCue(clips);
            settings.warnTickCount = 4;
            return (settings, channel, settings.warnTickCue, settings.grabCue, settings.snapCue);
        }

        [UnityTest]
        public IEnumerator Warning_PlaysRisingTicksThenSnap()
        {
            var s = Setup(clips: true);
            var ticks = new List<float>();
            int snaps = 0;
            s.channel.OnPlayRequested += (cue, p) =>
            {
                if (cue == s.tick) ticks.Add(p.PitchMultiplier);
                else if (cue == s.snap) snaps++;
            };

            var cycle = Spawn(s.settings, s.channel);
            for (float t = 0f; t < 2.4f; t += Time.deltaTime) yield return null;

            Assert.AreEqual(4, ticks.Count, "Uyarıda 4 tık");
            for (int i = 1; i < ticks.Count; i++) Assert.Greater(ticks[i], ticks[i - 1], "Tıklar yükselmeli");
            Assert.AreEqual(1, snaps, "Snap cue'su bir kez");
        }

        [UnityTest]
        public IEnumerator Grab_PlaysGrabCueAtTheGripPoint()
        {
            var s = Setup(clips: true);
            var positions = new List<Vector3>();
            s.channel.OnPlayRequested += (cue, p) => { if (cue == s.grab) positions.Add(p.Position); };

            var cycle = Spawn(s.settings, s.channel);
            yield return null;
            cycle.OnGrabbed(ActiveArm.Left, new Vector2(3f, 4f));
            yield return null;

            Assert.AreEqual(1, positions.Count);
            Assert.AreEqual(new Vector3(3f, 4f, 0f), positions[0]);
        }

        [UnityTest]
        public IEnumerator CuesWithoutClips_StaySilent()
        {
            var s = Setup(clips: false);
            int played = 0;
            s.channel.OnPlayRequested += (cue, p) => played++;

            var cycle = Spawn(s.settings, s.channel);
            yield return null;
            cycle.OnGrabbed(ActiveArm.Right, Vector2.zero);
            for (float t = 0f; t < 2.4f; t += Time.deltaTime) yield return null;

            Assert.AreEqual(0, played, "Klipsiz cue sessiz kalmalı");
        }

        [UnityTest]
        public IEnumerator WithoutSettingsOrHaptics_DoesNotThrow()
        {
            var cycle = Spawn(null, null);
            yield return null;
            cycle.OnGrabbed(ActiveArm.Right, Vector2.zero);
            for (float t = 0f; t < 2.4f; t += Time.deltaTime) yield return null;
            Assert.IsNotNull(cycle);
        }
    }
}
