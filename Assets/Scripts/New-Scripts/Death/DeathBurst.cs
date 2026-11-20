using System.Collections.Generic;
using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Oyuncu ölünce karakterin görünen karesi (animasyonun o anki sprite'ı) kendi piksellerine dağılır:
    /// kısa beyaz silüet flaşı, sonra sprite'ın dolu pikselleri (cellTexels × cellTexels hücreler, her biri kendi
    /// renginde) saçılır, sönerken yanıp sönüp kaybolur. Dönme ve şekil bozulması yok.
    /// Parçacıklar sprite karesi başına bir kez hesaplanıp saklanır; renderer'lar önceden üretilir ve yeniden kullanılır.
    /// Oyuncu objesinin çocuğu değildir (ölümde devre dışı kalır). Kafayı bu sınıf yönetmez. Asit ölümlerinde kafayı
    /// <see cref="AcidHeadPool"/> bırakır: asit <see cref="Claim"/> çağırır, <c>PlayerHealth.Kill</c>
    /// <see cref="ConsumeClaim"/> ile okur ve asit değilse <see cref="GroundHeadPool"/>'u kullanır.
    /// </summary>
    public class DeathBurst : MonoBehaviour
    {
        private const int MaxParticles = 400;
        private static readonly int FlashAmountId = Shader.PropertyToID("_FlashAmount");
        private static readonly int FlashColorId = Shader.PropertyToID("_FlashColor");

        private static bool _claimed;
        private static SpriteRenderer _behind;

        /// <summary>Asit, ölümü üstlendi: kafayı kendisi bırakır; parçacıklar (varsa) asidin arkasında çizilir.</summary>
        public static void Claim(SpriteRenderer behind)
        {
            _claimed = true;
            _behind = behind;
        }

        /// <summary>Bu ölümde bir asit üstlendi mi? Okununca sıfırlanır.</summary>
        public static bool ConsumeClaim(out SpriteRenderer behind)
        {
            bool claimed = _claimed;
            behind = _behind;
            _claimed = false;
            _behind = null;
            return claimed;
        }

        /// <summary>Bir sprite karesinin parçacık şablonu (kare başına bir kez hesaplanır).</summary>
        private class PixelSet
        {
            public Vector2[] Offsets;   // sprite pivotuna göre, birim cinsinden
            public Color[] Colors;
            public Vector2 Center;      // dolu alanın merkezi (pivota göre, birim)
            public Vector2 HalfSize;    // dolu alanın yarı boyutu (birim)
        }

        private struct Particle
        {
            public SpriteRenderer Renderer;
            public Vector2 Origin;
            public Vector2 Velocity;
            public float VanishAt;
        }

        private DeathBurstSettingsSO _settings;
        private readonly Particle[] _particles = new Particle[MaxParticles];
        private readonly Dictionary<Sprite, PixelSet> _cache = new Dictionary<Sprite, PixelSet>();
        private Sprite _pixel;
        private SpriteRenderer _flash;
        private MaterialPropertyBlock _flashBlock;
        private Vector3 _scale = Vector3.one;
        private float _size;
        private int _count;
        private float _startTime;
        private bool _playing;

        public static DeathBurst Create(DeathBurstSettingsSO settings)
        {
            var go = new GameObject("DeathBurst");
            var burst = go.AddComponent<DeathBurst>();
            burst.Init(settings);
            return burst;
        }

        private void Init(DeathBurstSettingsSO settings)
        {
            _settings = settings;

            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            _pixel = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);

            for (int i = 0; i < _particles.Length; i++) _particles[i].Renderer = NewRenderer("P" + i, _pixel);
            _flash = NewRenderer("Flash", null);
            _flashBlock = new MaterialPropertyBlock();
        }

        private void OnDestroy()
        {
            if (_pixel != null)
            {
                Destroy(_pixel.texture);
                Destroy(_pixel);
            }
        }

        private SpriteRenderer NewRenderer(string objectName, Sprite sprite)
        {
            var go = new GameObject(objectName);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.enabled = false;
            return sr;
        }

        /// <summary>
        /// Sprite karesinin piksellerini okur (okunabilir olması gerekmez: RenderTexture'a çizilir) ve hücrelere böler.
        /// </summary>
        private PixelSet BuildPixels(Sprite src)
        {
            Rect rect = src.rect;
            int w = Mathf.RoundToInt(rect.width), h = Mathf.RoundToInt(rect.height);
            Texture tex = src.texture;

            var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
            var scale = new Vector2(rect.width / tex.width, rect.height / tex.height);
            var offset = new Vector2(rect.x / tex.width, rect.y / tex.height);
            Graphics.Blit(tex, rt, scale, offset);

            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var read = new Texture2D(w, h, TextureFormat.RGBA32, false);
            read.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            read.Apply();
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            Color32[] pixels = read.GetPixels32();
            Destroy(read);

            List<DeathBurstMath.PixelCell> cells = DeathBurstMath.BuildPixelCells(
                pixels, w, h, _settings.cellTexels, (byte)_settings.alphaMin);
            if (cells.Count > MaxParticles) cells.RemoveRange(MaxParticles, cells.Count - MaxParticles);

            float ppu = src.pixelsPerUnit;
            Vector2 pivot = src.pivot;
            var set = new PixelSet
            {
                Offsets = new Vector2[cells.Count],
                Colors = new Color[cells.Count]
            };
            Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
            for (int i = 0; i < cells.Count; i++)
            {
                Vector2 local = (cells[i].Center - pivot) / ppu;
                set.Offsets[i] = local;
                set.Colors[i] = cells[i].Color;
                min = Vector2.Min(min, local);
                max = Vector2.Max(max, local);
            }
            if (cells.Count > 0)
            {
                set.Center = (min + max) * 0.5f;
                set.HalfSize = Vector2.Max((max - min) * 0.5f, new Vector2(0.01f, 0.01f));
            }
            return set;
        }

        /// <summary>Patlamayı başlatır. Kafayı bu sınıf yönetmez: asit <see cref="AcidHeadPool"/>, diğer ölümler <see cref="GroundHeadPool"/>.</summary>
        public void Play(Vector2 playerVelocity, SpriteRenderer body, SpriteRenderer behind)
        {
            if (_settings == null || body == null || body.sprite == null) return;

            var s = _settings;
            if (!_cache.TryGetValue(body.sprite, out PixelSet set))
            {
                set = BuildPixels(body.sprite);
                _cache[body.sprite] = set;
            }

            _count = set.Offsets.Length;
            _size = s.cellTexels / body.sprite.pixelsPerUnit;
            Vector3 lossy = body.transform.lossyScale;
            _scale = new Vector3(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y), 1f);

            int layer = behind != null ? behind.sortingLayerID : body.sortingLayerID;
            int order = behind != null ? behind.sortingOrder - 1 : body.sortingOrder + 1;
            Vector2 inherit = Vector2.ClampMagnitude(playerVelocity * s.velocityInherit, s.maxInheritSpeed);
            Vector2 centerWorld = body.transform.TransformPoint(FlipX(set.Center, body.flipX));

            for (int i = 0; i < _particles.Length; i++)
            {
                var r = _particles[i].Renderer;
                if (i >= _count) { r.enabled = false; continue; }

                Vector2 local = FlipX(set.Offsets[i], body.flipX);
                Vector2 world = body.transform.TransformPoint(local);
                Vector2 rel = world - centerWorld;

                float ang = Mathf.Atan2(rel.y, rel.x) + Random.Range(-0.4f, 0.4f);
                if (rel.sqrMagnitude < 1e-6f) ang = Random.Range(0f, Mathf.PI * 2f);
                float dist = Mathf.Clamp01(new Vector2(rel.x, rel.y / 1.6f).magnitude / 0.6f);
                float speed = DeathBurstMath.ParticleSpeed(s.spread, s.drag, dist, Random.Range(0.75f, 1.15f));

                _particles[i].Origin = world;
                _particles[i].Velocity = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * speed + inherit;
                _particles[i].VanishAt = Random.Range(0.5f, 1f);

                r.color = set.Colors[i];
                r.sortingLayerID = layer;
                r.sortingOrder = order;
                r.transform.localScale = new Vector3(_size, _size, 1f);
                r.transform.position = DeathBurstMath.Snap(world, s.pixelsPerUnit);
                r.enabled = false; // flaş bitene kadar gizli
            }

            // Beyaz silüet: gövde sprite'ının kopyası, aynı materyal, _FlashAmount = 1
            _flash.enabled = false;
            if (s.flashFrames > 0)
            {
                _flash.sprite = body.sprite;
                _flash.flipX = body.flipX;
                _flash.sharedMaterial = body.sharedMaterial;
                _flash.sortingLayerID = body.sortingLayerID;
                _flash.sortingOrder = order + 1;
                _flash.color = body.color;
                _flash.transform.SetPositionAndRotation(body.transform.position, body.transform.rotation);
                _flash.transform.localScale = lossy;
                _flashBlock.Clear();
                _flashBlock.SetFloat(FlashAmountId, 1f);
                // Gövde materyalini stamina uyarısıyla paylaşıyoruz: yanıp sönerken ölünce silüet kırmızı çıkmasın.
                _flashBlock.SetColor(FlashColorId, Color.white);
                _flash.SetPropertyBlock(_flashBlock);
                _flash.enabled = true;
            }

            _startTime = Time.time;
            _playing = true;
        }

        private static Vector2 FlipX(Vector2 v, bool flip)
        {
            return flip ? new Vector2(-v.x, v.y) : v;
        }

        private void Update()
        {
            if (!_playing) return;

            var s = _settings;
            float t = Time.time - _startTime;
            float moving = t - s.hitstopSeconds;
            float progress = DeathBurstMath.Progress(t, s.hitstopSeconds, s.duration);
            bool flash = s.flashFrames > 0 && t < s.flashFrames / 60f;

            if (_flash.enabled && !flash) _flash.enabled = false;

            for (int i = 0; i < _count; i++)
            {
                var p = _particles[i];
                if (flash || !DeathBurstMath.ParticleVisible(progress, p.VanishAt)) { p.Renderer.enabled = false; continue; }

                Vector2 pos = DeathBurstMath.Position(p.Origin, p.Velocity, s.drag, s.gravity, s.gravityDelay, moving);
                p.Renderer.transform.position = DeathBurstMath.Snap(pos, s.pixelsPerUnit);
                p.Renderer.enabled = true;
            }

            if (t >= s.duration + 0.12f) Stop();
        }

        private void Stop()
        {
            _playing = false;
            for (int i = 0; i < _particles.Length; i++) _particles[i].Renderer.enabled = false;
            _flash.enabled = false;
        }
    }
}
