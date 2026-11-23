using System.Collections.Generic;
using New_Scripts.Death;
using New_Scripts.Multiplayer.Messages;
using UnityEngine;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>
    /// Rakibin üstünde durduğu hareketli yüzeyin ve onu öldüren hareketli tehlikenin kâğıt beyazı kesik konturu. Kontur,
    /// yerel dünyadaki aynı nesnenin collider şeklini rakibin dünyasındaki pozda (<see cref="SurfacePose"/>) çizer; böylece
    /// platform fazı oyuncular arasında ayrışsa da hayalet kendi platformunun üstünde durur. Zamanlama
    /// <see cref="GhostOutlineTracker"/>, şekil <see cref="GhostOutlineShape"/>. Hiçbir zaman yerel platform ve
    /// tehlikelerin önüne çizilmez (sıralama hepsinin arkasında). Dünyaya hiçbir şey yazmaz.
    /// Sahne kökünde durur, hayalet görünürken çizer.
    /// </summary>
    public sealed class GhostOutlineView : MonoBehaviour
    {
        public const string ObjectName = "[MP Ghost Outline]";

        private readonly List<SurfacePose> _present = new List<SurfacePose>(3);
        private readonly List<GhostOutlineEntry> _entries = new List<GhostOutlineEntry>();
        private readonly Dictionary<uint, List<OutlinePath>> _shapes = new Dictionary<uint, List<OutlinePath>>();
        private readonly List<LineRenderer> _lines = new List<LineRenderer>();

        private MovingSurfaceRegistry _registry;
        private GhostOutlineSettingsSO _settings;
        private GhostOutlineTracker _tracker;
        private Material _material;
        private bool _visible;
        private int _sortingLayerId;
        private int _sortingOrder;

        private static readonly int PeriodId = Shader.PropertyToID("_Period");
        private static readonly int DutyId = Shader.PropertyToID("_Duty");

        public int ActiveLineCount { get; private set; }

        public static GhostOutlineView Create(MovingSurfaceRegistry registry, GhostOutlineSettingsSO settings)
        {
            var view = new GameObject(ObjectName).AddComponent<GhostOutlineView>();
            view._registry = registry;
            view._settings = settings;
            view._tracker = new GhostOutlineTracker(settings.Timing);
            view._material = settings.dashMaterial;
            if (view._material == null)
                Debug.LogWarning("[MP] GhostOutlineSettings'te dashMaterial yok; hayalet konturu çizilmeyecek.");
            view.ComputeBackSorting();
            return view;
        }

        /// <summary>Hayalet görünürken kontur çizilir; gizlenince bütün konturlar anında kalkar.</summary>
        public void SetVisible(bool visible)
        {
            if (_visible == visible) return;
            _visible = visible;
            if (!visible)
            {
                _tracker.Reset();
                HideAllLines();
            }
        }

        /// <summary>Rakip hareketli bir tehlikeye çarpıp öldü: tehlike ölüm anındaki pozunda kısa süre görünür.</summary>
        public void ShowHazard(SurfacePose killer)
        {
            if (_visible) _tracker.AddHazard(killer);
        }

        /// <summary>Gelen hayalet pozundaki taşıyıcı ve tutunulan node'lar; rakip ölüyken (gizli) hiçbiri sayılmaz.</summary>
        public void SetPresent(GhostPose pose)
        {
            _present.Clear();
            if (pose.IsHidden) return;
            if (pose.Carrier.HasValue) _present.Add(pose.Carrier.Value);
            if (pose.LeftNode.HasValue) _present.Add(pose.LeftNode.Value);
            if (pose.RightNode.HasValue) _present.Add(pose.RightNode.Value);
        }

        private void LateUpdate() => Tick(Time.deltaTime);

        /// <summary>Zamanlamayı ilerletir ve çizer (LateUpdate'te; Edit Mode çekimleri ve testler doğrudan çağırır).</summary>
        public void Tick(float deltaTime)
        {
            if (!_visible || _material == null) return;

            _tracker.Tick(deltaTime, _present);
            _tracker.Collect(_entries);
            Draw();
        }

        private void Draw()
        {
            var camera = Camera.main;
            if (camera == null || !camera.orthographic)
            {
                HideAllLines();
                return;
            }

            float width = GhostOutlineMath.PixelsToWorld(_settings.widthPixels, camera.orthographicSize, Screen.height);
            float dash = GhostOutlineMath.PixelsToWorld(_settings.dashPixels, camera.orthographicSize, Screen.height);
            float gap = GhostOutlineMath.PixelsToWorld(_settings.gapPixels, camera.orthographicSize, Screen.height);
            float period = Mathf.Max(dash + gap, 1e-3f);
            _material.SetFloat(PeriodId, period);
            _material.SetFloat(DutyId, Mathf.Clamp(dash / period, 0.05f, 0.95f));

            int used = 0;
            foreach (var entry in _entries)
            {
                if (!_registry.TryGet(entry.Pose.Id, out Transform surface)) continue;
                if (!GhostOutlineMath.ShouldDraw(entry.Pose.Position, surface.position, _settings.minLocalDistance)) continue;

                var matrix = Matrix4x4.TRS(new Vector3(entry.Pose.Position.x, entry.Pose.Position.y, surface.position.z),
                    Quaternion.Euler(0f, 0f, entry.Pose.Rotation), surface.lossyScale);
                Color color = _settings.color;
                color.a *= _settings.maxAlpha * entry.Alpha;

                foreach (var path in ShapesFor(entry.Pose.Id, surface))
                {
                    var line = Acquire(used++);
                    for (int i = 0; i < path.Points.Length; i++)
                        path.Scratch[i] = matrix.MultiplyPoint3x4(path.Points[i]);
                    line.loop = path.Loop;
                    line.positionCount = path.Scratch.Length;
                    line.SetPositions(path.Scratch);
                    line.widthMultiplier = width;
                    line.startColor = color;
                    line.endColor = color;
                    line.enabled = true;
                }
            }

            for (int i = used; i < _lines.Count; i++) _lines[i].enabled = false;
            ActiveLineCount = used;
        }

        private List<OutlinePath> ShapesFor(uint id, Transform surface)
        {
            if (!_shapes.TryGetValue(id, out var paths))
            {
                paths = GhostOutlineShape.Extract(surface, _registry);
                _shapes[id] = paths;
            }
            return paths;
        }

        private LineRenderer Acquire(int index)
        {
            while (_lines.Count <= index)
            {
                var go = new GameObject("Outline " + _lines.Count);
                go.transform.SetParent(transform, false);
                var line = go.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.textureMode = LineTextureMode.Tile;
                line.alignment = LineAlignment.View;
                line.numCornerVertices = 0;
                line.numCapVertices = 0;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
                line.sharedMaterial = _material;
                line.sortingLayerID = _sortingLayerId;
                line.sortingOrder = _sortingOrder;
                line.enabled = false;
                _lines.Add(line);
            }
            return _lines[index];
        }

        private void HideAllLines()
        {
            foreach (var line in _lines) line.enabled = false;
            ActiveLineCount = 0;
        }

        /// <summary>
        /// Hayalet konturu bütün yerel platform ve tehlikelerin arkasında kalsın: kayıtlı yüzeylerin ve ölüm bölgelerinin
        /// renderer'ları arasında en arkadaki sıralama katmanı/sırası bulunur, kontur bir sıra daha gerisine konur.
        /// </summary>
        private void ComputeBackSorting()
        {
            int bestLayerValue = int.MaxValue;
            _sortingLayerId = 0;
            _sortingOrder = 0;
            bool found = false;

            void Consider(Renderer renderer)
            {
                int layerValue = SortingLayer.GetLayerValueFromID(renderer.sortingLayerID);
                if (found && (layerValue > bestLayerValue || (layerValue == bestLayerValue && renderer.sortingOrder >= _sortingOrder + 1))) return;
                found = true;
                bestLayerValue = layerValue;
                _sortingLayerId = renderer.sortingLayerID;
                _sortingOrder = renderer.sortingOrder - 1;
            }

            foreach (var surface in _registry.Surfaces)
                foreach (var renderer in surface.GetComponentsInChildren<Renderer>(true)) Consider(renderer);
            foreach (var zone in FindObjectsByType<DeathZone>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                foreach (var renderer in zone.GetComponentsInChildren<Renderer>(true)) Consider(renderer);
        }

        private void OnDestroy()
        {
            // Materyal ortak asset: yok edilmez.
            _shapes.Clear();
        }
    }
}
