using System.Collections.Generic;
using UnityEngine;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>Hayaletin dash/iz efektinin kaynağı: PlayerVFXManager'ın afterimage ayarları ve hareket izi.</summary>
    public sealed class GhostDashSource
    {
        public float SpawnInterval = 0.05f;
        public float Duration = 0.5f;
        public Color Color = new Color(0.5f, 0.5f, 1f, 0.5f);
        public TrailRenderer Trail;
    }

    /// <summary>
    /// Oyuncudaki PlayerVFXManager'ın hayalet karşılığı: dash sırasında solan kopyalar (afterimage), salınırken ve dash'te
    /// iz (TrailRenderer). Kopyalar hareket etmeyen ayrı bir kökte durur; hayalet yok edilince o da yok edilir.
    /// </summary>
    internal sealed class GhostDashFx
    {
        public const string ContainerName = "[MP Ghost Afterimages]";

        private sealed class Afterimage
        {
            public SpriteRenderer Renderer;
            public float Age;
            public bool Active;
        }

        private readonly List<Afterimage> _pool = new List<Afterimage>();
        private GameObject _container;
        private TrailRenderer _trail;
        private float _interval;
        private float _duration;
        private Color _color;
        private float _spawnTimer;

        public int ActiveCount
        {
            get
            {
                int n = 0;
                foreach (var a in _pool) if (a.Active) n++;
                return n;
            }
        }

        public static GhostDashFx Create(GhostDashSource source, Transform ghostRoot, SpriteRenderer body, Color tint, int sortingOrderOffset)
        {
            var fx = new GhostDashFx
            {
                _interval = Mathf.Max(0.01f, source.SpawnInterval),
                _duration = Mathf.Max(0.01f, source.Duration),
                _color = new Color(source.Color.r * tint.r, source.Color.g * tint.g, source.Color.b * tint.b, source.Color.a * tint.a)
            };
            fx._container = new GameObject(ContainerName);
            if (Application.isPlaying) Object.DontDestroyOnLoad(fx._container);

            if (source.Trail != null) fx._trail = CopyTrail(source.Trail, ghostRoot, tint, sortingOrderOffset);
            return fx;
        }

        private static TrailRenderer CopyTrail(TrailRenderer source, Transform ghostRoot, Color tint, int offset)
        {
            var trail = new GameObject("Trail").AddComponent<TrailRenderer>();
            trail.transform.SetParent(ghostRoot, false);
            trail.transform.localPosition = source.transform.localPosition;
            trail.sharedMaterial = source.sharedMaterial;
            trail.time = source.time;
            trail.widthMultiplier = source.widthMultiplier;
            trail.widthCurve = source.widthCurve;
            trail.minVertexDistance = source.minVertexDistance;
            trail.numCapVertices = source.numCapVertices;
            var gradient = source.colorGradient;
            var alphas = gradient.alphaKeys;
            for (int i = 0; i < alphas.Length; i++) alphas[i].alpha *= tint.a;
            var colors = gradient.colorKeys;
            for (int i = 0; i < colors.Length; i++) colors[i].color *= new Color(tint.r, tint.g, tint.b, 1f);
            gradient.SetKeys(colors, alphas);
            trail.colorGradient = gradient;
            trail.sortingLayerID = source.sortingLayerID;
            trail.sortingOrder = source.sortingOrder - offset;
            trail.emitting = false;
            return trail;
        }

        public void Tick(GhostPose pose, bool shown, SpriteRenderer body, float deltaTime)
        {
            if (_trail != null)
            {
                bool emit = shown && (pose.IsDashing || pose.IsSwinging);
                if (_trail.emitting != emit) _trail.emitting = emit;
            }

            if (shown && pose.IsDashing)
            {
                _spawnTimer -= deltaTime;
                if (_spawnTimer <= 0f)
                {
                    Spawn(body);
                    _spawnTimer = _interval;
                }
            }
            else
            {
                _spawnTimer = 0f; // sonraki dash'in ilk karesinde hemen bir kopya
            }

            foreach (var a in _pool)
            {
                if (!a.Active) continue;
                a.Age += deltaTime;
                float t = a.Age / _duration;
                if (t >= 1f)
                {
                    a.Active = false;
                    a.Renderer.enabled = false;
                    continue;
                }
                var c = _color;
                c.a = Mathf.Lerp(_color.a, 0f, t);
                a.Renderer.color = c;
            }
        }

        private void Spawn(SpriteRenderer body)
        {
            Afterimage image = null;
            foreach (var a in _pool)
                if (!a.Active) { image = a; break; }

            if (image == null)
            {
                var renderer = new GameObject("Afterimage").AddComponent<SpriteRenderer>();
                renderer.transform.SetParent(_container.transform, false);
                image = new Afterimage { Renderer = renderer };
                _pool.Add(image);
            }

            var r = image.Renderer;
            r.sprite = body.sprite;
            r.sharedMaterial = body.sharedMaterial;
            r.sortingLayerID = body.sortingLayerID;
            r.sortingOrder = body.sortingOrder - 1;
            r.flipX = body.flipX;
            r.flipY = body.flipY;
            r.transform.SetPositionAndRotation(body.transform.position, body.transform.rotation);
            r.transform.localScale = body.transform.lossyScale;
            r.color = _color;
            r.enabled = true;
            image.Age = 0f;
            image.Active = true;
        }

        /// <summary>Işınlanma ya da yeniden belirmede iz haritanın öbür ucuna çizgi çekmesin.</summary>
        public void ClearTrail()
        {
            if (_trail != null) _trail.Clear();
        }

        public void Dispose()
        {
            if (_container == null) return;
            if (Application.isPlaying) Object.Destroy(_container);
            else Object.DestroyImmediate(_container);
            _container = null;
        }
    }
}
