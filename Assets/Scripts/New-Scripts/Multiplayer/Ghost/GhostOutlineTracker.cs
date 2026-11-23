using System.Collections.Generic;
using New_Scripts.Multiplayer.Messages;
using UnityEngine;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>Kontur zamanlama değerleri (saniye). <see cref="GhostOutlineSettingsSO"/>'dan okunur; testler doğrudan kurar.</summary>
    public readonly struct GhostOutlineTiming
    {
        /// <summary>Rakip yüzeye basınca konturun tam görünmesi için gereken süre.</summary>
        public readonly float Enter;
        /// <summary>Rakip yüzeyden ayrılınca kontur bu kadar tam görünür kalır.</summary>
        public readonly float Linger;
        /// <summary>Bekleme bitince solma süresi.</summary>
        public readonly float Fade;
        /// <summary>Öldüren tehlikenin konturunun toplam ömrü.</summary>
        public readonly float HazardLife;
        /// <summary>Tehlike ömrünün son bu kadarı solma.</summary>
        public readonly float HazardFade;

        public GhostOutlineTiming(float enter, float linger, float fade, float hazardLife, float hazardFade)
        {
            Enter = enter;
            Linger = linger;
            Fade = fade;
            HazardLife = hazardLife;
            HazardFade = hazardFade;
        }
    }

    /// <summary>Çizilecek bir kontur: hangi yüzey, gönderenin dünyasındaki pozu ve saydamlığı (0..1).</summary>
    public readonly struct GhostOutlineEntry
    {
        public readonly SurfacePose Pose;
        public readonly float Alpha;

        public GhostOutlineEntry(SurfacePose pose, float alpha)
        {
            Pose = pose;
            Alpha = alpha;
        }
    }

    /// <summary>
    /// Hayalet konturlarının zamanlaması (saf mantık). Rakip bir yüzeyin üstündeyken (taşıyıcı ya da tutunulan node)
    /// o yüzeyin konturu <see cref="GhostOutlineTiming.Enter"/> sürede belirir; rakip ayrılınca son pozunda
    /// <see cref="GhostOutlineTiming.Linger"/> kadar kalır ve <see cref="GhostOutlineTiming.Fade"/> sürede solar. Öldüren
    /// tehlike ölüm anındaki pozunda donuk, <see cref="GhostOutlineTiming.HazardLife"/> ömürlü, son
    /// <see cref="GhostOutlineTiming.HazardFade"/> kadarı solarak görünür. Çizim ve şekil <see cref="GhostOutlineView"/>'da.
    /// </summary>
    public sealed class GhostOutlineTracker
    {
        private const int MaxHazards = 8;

        private sealed class Surface
        {
            public SurfacePose Pose;
            public float Alpha;
            public float SinceLeft;
            public bool Present;
        }

        private sealed class Hazard
        {
            public SurfacePose Pose;
            public float Age;
        }

        private readonly GhostOutlineTiming _timing;
        private readonly Dictionary<uint, Surface> _surfaces = new Dictionary<uint, Surface>();
        private readonly List<Hazard> _hazards = new List<Hazard>();
        private readonly List<uint> _toRemove = new List<uint>();

        public GhostOutlineTracker(GhostOutlineTiming timing) => _timing = timing;

        public int Count => _surfaces.Count + _hazards.Count;

        /// <summary>
        /// Bir kare ilerler. <paramref name="present"/> rakibin şu an üstünde olduğu ya da tutunduğu yüzeyler
        /// (aynı kimlik birden çok kez gelebilir, tek sayılır).
        /// </summary>
        public void Tick(float deltaTime, IReadOnlyList<SurfacePose> present)
        {
            foreach (var surface in _surfaces.Values) surface.Present = false;

            if (present != null)
            {
                foreach (var pose in present)
                {
                    if (!_surfaces.TryGetValue(pose.Id, out var surface))
                    {
                        surface = new Surface();
                        _surfaces[pose.Id] = surface;
                    }
                    surface.Pose = pose;
                    surface.Present = true;
                }
            }

            _toRemove.Clear();
            foreach (var pair in _surfaces)
            {
                var s = pair.Value;
                if (s.Present)
                {
                    s.SinceLeft = 0f;
                    s.Alpha = _timing.Enter <= 0f ? 1f : Mathf.Min(1f, s.Alpha + deltaTime / _timing.Enter);
                    continue;
                }

                s.SinceLeft += deltaTime;
                if (s.SinceLeft <= _timing.Linger) continue;
                s.Alpha -= _timing.Fade <= 0f ? 1f : deltaTime / _timing.Fade;
                if (s.Alpha <= 0f) _toRemove.Add(pair.Key);
            }
            foreach (uint id in _toRemove) _surfaces.Remove(id);

            for (int i = _hazards.Count - 1; i >= 0; i--)
            {
                _hazards[i].Age += deltaTime;
                if (_hazards[i].Age >= _timing.HazardLife) _hazards.RemoveAt(i);
            }
        }

        /// <summary>Rakibi öldüren tehlike: ölüm anındaki pozunda donuk kontur başlatır.</summary>
        public void AddHazard(SurfacePose pose)
        {
            if (_hazards.Count >= MaxHazards) _hazards.RemoveAt(0);
            _hazards.Add(new Hazard { Pose = pose });
        }

        /// <summary>Bütün konturları anında kaldırır (hayalet gizlendi, bağlantı koptu).</summary>
        public void Reset()
        {
            _surfaces.Clear();
            _hazards.Clear();
        }

        /// <summary>Şu an çizilecek konturları listeye ekler (önce liste temizlenir).</summary>
        public void Collect(List<GhostOutlineEntry> into)
        {
            into.Clear();
            foreach (var s in _surfaces.Values)
                if (s.Alpha > 0f) into.Add(new GhostOutlineEntry(s.Pose, Mathf.Clamp01(s.Alpha)));

            foreach (var h in _hazards)
            {
                float fadeStart = _timing.HazardLife - _timing.HazardFade;
                float alpha = h.Age <= fadeStart || _timing.HazardFade <= 0f
                    ? 1f
                    : Mathf.Clamp01((_timing.HazardLife - h.Age) / _timing.HazardFade);
                if (alpha > 0f) into.Add(new GhostOutlineEntry(h.Pose, alpha));
            }
        }
    }

    /// <summary>Kontur kuralları: yerel nesneyle üst üste biniyorsa çizme, piksel → dünya birimi.</summary>
    public static class GhostOutlineMath
    {
        /// <summary>Rakibin dünyasındaki poz yerel nesneyle neredeyse aynıysa (<paramref name="minDistance"/>'dan yakın) kontur gereksiz.</summary>
        public static bool ShouldDraw(Vector2 remotePosition, Vector2 localPosition, float minDistance) =>
            Vector2.Distance(remotePosition, localPosition) >= minDistance;

        /// <summary>Ekranda <paramref name="pixels"/> piksel eden dünya uzunluğu (ortografik kamera, yükseklik piksel).</summary>
        public static float PixelsToWorld(float pixels, float orthographicSize, float screenHeight) =>
            screenHeight <= 0f ? 0f : pixels * 2f * orthographicSize / screenHeight;
    }
}
