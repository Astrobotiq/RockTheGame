using System.Collections.Generic;
using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Asitte kalan kafaların ortak havuzu: tüm oyunda en fazla <c>maxHeads</c> kafa, nesneler yeniden kullanılır.
    /// Havuz doluyken yeni ölüm en eski kafayı çözdürür (yanıp sönerek biter), nesnesi sonradan devralınır.
    /// İlk <see cref="Spawn"/> çağrısında sahnede kendiliğinden oluşur; sahneyle birlikte yok olur.
    /// </summary>
    public class AcidHeadPool : MonoBehaviour
    {
        private class Item
        {
            public SpriteRenderer Renderer;
            public AcidHeadHost Host;
            public bool Active;
            public float Born;
            public float DyingAt = -1f;
            public Vector2 Start;
            public Vector2 StartOffset;
            public Vector2 BlobTarget;
            public int Slot;
            public float Fraction;
            public float Phase;
        }

        private static AcidHeadPool _instance;

        private readonly List<Item> _items = new List<Item>();
        private DeathBurstSettingsSO _settings;

        public static void Spawn(DeathBurstSettingsSO settings, AcidHeadHost host, Vector2 startWorld)
        {
            if (_instance == null)
            {
                var go = new GameObject("AcidHeadPool");
                _instance = go.AddComponent<AcidHeadPool>();
            }
            _instance._settings = settings;
            _instance.DoSpawn(host, startWorld);
        }

        /// <summary>Bu asidin kafalarını hemen siler (oda çıkışı, asit devre dışı).</summary>
        public static void Release(AcidHeadHost host)
        {
            if (_instance == null) return;
            foreach (var item in _instance._items)
            {
                if (item.Active && item.Host == host) _instance.Deactivate(item);
            }
        }

        private void OnDestroy()
        {
            if (_instance == this) _instance = null;
        }

        private void DoSpawn(AcidHeadHost host, Vector2 startWorld)
        {
            var s = _settings;

            // Dolunca en eskiyi çözdür
            Item oldest = null;
            int live = 0;
            foreach (var it in _items)
            {
                if (!it.Active || it.DyingAt >= 0f) continue;
                live++;
                if (oldest == null || it.Born < oldest.Born) oldest = it;
            }
            if (live >= s.maxHeads && oldest != null) oldest.DyingAt = Time.time;

            Item item = _items.Find(i => !i.Active);
            if (item == null)
            {
                if (_items.Count < s.maxHeads + 2)
                {
                    item = NewItem();
                }
                else
                {
                    // Hepsi dolu (art arda hızlı ölümler): en eskisini devral
                    foreach (var it in _items)
                        if (item == null || it.Born < item.Born) item = it;
                }
            }

            item.Host = host;
            item.Active = true;
            item.Born = Time.time;
            item.DyingAt = -1f;
            item.Start = startWorld;
            item.StartOffset = startWorld - host.Center;
            item.Fraction = Random.Range(s.headRestFraction.x, s.headRestFraction.y);
            item.Phase = Random.Range(0f, 6.28f);
            item.Slot = 0;

            Bounds b = host.Bounds;
            if (host.Mode == AcidHeadHost.HostMode.Pool)
            {
                int n = SlotCount(host);
                var used = new bool[n];
                foreach (var it in _items)
                    if (it != item && it.Active && it.Host == host && it.Slot >= 0 && it.Slot < n) used[it.Slot] = true;
                int preferred = DeathBurstMath.SlotNear(startWorld.x, b.min.x, HeadSize(), s.headSlotSpacing, n);
                int slot = DeathBurstMath.PickSlotNear(used, preferred);
                item.Slot = slot >= 0 ? slot : preferred;
            }
            else
            {
                float radius = Mathf.Min(b.extents.x, b.extents.y) * s.blobRadiusFactor;
                item.BlobTarget = Random.insideUnitCircle * radius;
            }

            item.Renderer.sprite = s.headSprite;
            item.Renderer.color = Color.white;
            item.Renderer.transform.localScale = Vector3.one;
            item.Renderer.transform.position = DeathBurstMath.Snap(startWorld, s.pixelsPerUnit);
            ApplySorting(item);
            item.Renderer.enabled = true;
        }

        private float HeadSize()
        {
            return _settings.headSprite != null ? _settings.headSprite.bounds.size.x : 0.7f;
        }

        private int SlotCount(AcidHeadHost host)
        {
            return DeathBurstMath.SlotCount(host.Bounds.size.x, HeadSize(), _settings.headSlotSpacing);
        }

        private Item NewItem()
        {
            var go = new GameObject("Head" + _items.Count);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.enabled = false;
            var item = new Item { Renderer = sr };
            _items.Add(item);
            return item;
        }

        private static void ApplySorting(Item item)
        {
            var hostRenderer = item.Host.Renderer;
            if (hostRenderer == null) return;
            item.Renderer.sortingLayerID = hostRenderer.sortingLayerID;
            item.Renderer.sortingOrder = hostRenderer.sortingOrder + 2;
        }

        private void Deactivate(Item item)
        {
            item.Active = false;
            item.Host = null;
            item.Renderer.enabled = false;
        }

        private void Update()
        {
            if (_settings == null) return;
            var s = _settings;
            float ppu = s.pixelsPerUnit;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                if (!item.Active) continue;
                if (item.Host == null) { Deactivate(item); continue; }

                float t = Time.time - item.Born - s.hitstopSeconds;
                Vector2 pos;
                if (item.Host.Mode == AcidHeadHost.HostMode.Pool)
                {
                    int n = SlotCount(item.Host);
                    float slotX = DeathBurstMath.SlotX(Mathf.Min(item.Slot, n - 1), item.Host.Bounds.min.x, HeadSize(), s.headSlotSpacing);
                    float surface = item.Host.SurfaceY();
                    float depth = surface - item.Host.Bounds.min.y;
                    float restY = DeathBurstMath.RestY(surface, depth, item.Fraction);
                    pos = new Vector2(
                        DeathBurstMath.EaseToward(item.Start.x, slotX, s.headSlideRate, t),
                        DeathBurstMath.EaseToward(item.Start.y, restY, s.headSinkRate, t));
                }
                else
                {
                    float a = Mathf.Clamp01(t / Mathf.Max(s.blobEnterSeconds, 1e-3f));
                    float e = 1f - (1f - a) * (1f - a);
                    pos = item.Host.Center + Vector2.Lerp(item.StartOffset, item.BlobTarget, e);
                }

                if (t > 0f)
                {
                    float bob = Mathf.Round(Mathf.Sin(Time.time / Mathf.Max(s.headBobSeconds, 1e-3f) + item.Phase)) / ppu;
                    pos.y += bob * Mathf.Clamp01(t);
                }
                item.Renderer.transform.position = DeathBurstMath.Snap(pos, ppu);

                float tint = DeathBurstMath.HeadTint(item.Fraction, s.headTintShallow, s.headTintDeep, t, s.headTintSettleSeconds);
                item.Renderer.color = Color.Lerp(Color.white, s.headTint, tint);

                float scale = 1f;
                if (item.DyingAt >= 0f)
                {
                    float d = (Time.time - item.DyingAt) / Mathf.Max(s.headDissolveSeconds, 1e-3f);
                    scale = DeathBurstMath.DissolveScale(d);
                    if (d >= 1f) { Deactivate(item); continue; }
                }
                item.Renderer.enabled = scale > 0f;
                item.Renderer.transform.localScale = Vector3.one * scale;
            }
        }
    }
}
