using System.Collections.Generic;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Platform;
using UnityEngine;

namespace New_Scripts.Multiplayer.Ghost
{
    /// <summary>
    /// Sahnedeki hareketli nesnelerin kimlik tablosu: platformlar, <c>Death</c> tehlikeleri, hareketli node'lar ve hareketli
    /// launch pad'ler hep <see cref="PlatformController"/> ya da <see cref="TriggeredPlatformController"/> ile sürülüyor,
    /// ikisi de kendi transform'unu hareket ettiriyor; bu yüzden kayıt o transform'lar. Kimlik <see cref="SceneObjectId"/>.
    /// İki nesne aynı kimliği alırsa (aynı yol özeti) uyarı loglanır ve o kimlik kullanılmaz: yanlış nesneye bağlanmaktansa
    /// düz dünya konumuna düşmek daha güvenli. Sahne yüklenince bir kez kurulur; çalışırken yaratılanlar kaydedilmez.
    /// </summary>
    public sealed class MovingSurfaceRegistry
    {
        private readonly Dictionary<uint, Transform> _byId = new Dictionary<uint, Transform>();
        private readonly Dictionary<Transform, uint> _byTransform = new Dictionary<Transform, uint>();
        private readonly HashSet<uint> _ambiguous = new HashSet<uint>();

        /// <summary>Kullanılabilir (çakışmasız) kayıt sayısı.</summary>
        public int Count => _byId.Count;

        /// <summary>Çakışma yüzünden kullanılmayan kimlik sayısı.</summary>
        public int AmbiguousCount => _ambiguous.Count;

        /// <summary>
        /// Sahnedeki bütün hareketli sürücüleri (pasif odalardakiler dahil) kimlikleriyle kaydeder.
        /// </summary>
        public static MovingSurfaceRegistry BuildFromScene()
        {
            var registry = new MovingSurfaceRegistry();
            foreach (var c in Object.FindObjectsByType<PlatformController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                registry.Register(c.transform);
            foreach (var c in Object.FindObjectsByType<TriggeredPlatformController>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                registry.Register(c.transform);
            return registry;
        }

        /// <summary>
        /// Transform'u kendi kimliğiyle kaydeder. Başka bir nesneyle çakışırsa ikisi de düşer ve uyarı loglanır; false döner.
        /// Aynı transform ikinci kez kaydedilirse sessizce yok sayılır.
        /// </summary>
        public bool Register(Transform transform)
        {
            if (transform == null || _byTransform.ContainsKey(transform)) return true;
            return Register(transform, SceneObjectId.For(transform));
        }

        /// <summary>Kimliği dışarıdan veren kayıt: aynı yol bir sahnede iki nesneye düşmez, çakışma yalnızca özet çakışmasıyla olur; testler bu kapıdan dener.</summary>
        public bool Register(Transform transform, uint id)
        {
            if (transform == null || _byTransform.ContainsKey(transform)) return true;

            if (_ambiguous.Contains(id))
            {
                Debug.LogWarning($"[MP] Hareketli nesne kimliği çakışıyor, kullanılmayacak: {SceneObjectId.BuildPath(transform)}");
                return false;
            }

            if (_byId.TryGetValue(id, out var other))
            {
                Debug.LogWarning($"[MP] Hareketli nesne kimliği çakışıyor, ikisi de kullanılmayacak: {SceneObjectId.BuildPath(other)} ve {SceneObjectId.BuildPath(transform)}");
                _byId.Remove(id);
                _byTransform.Remove(other);
                _ambiguous.Add(id);
                return false;
            }

            _byId[id] = transform;
            _byTransform[transform] = id;
            return true;
        }

        /// <summary>Kayıtlı (çakışmasız) bütün yüzeyler.</summary>
        public IEnumerable<Transform> Surfaces => _byId.Values;

        /// <summary>Kimlikten transform; kayıtsız, çakışmalı ya da yok edilmişse false.</summary>
        public bool TryGet(uint id, out Transform transform)
        {
            if (_byId.TryGetValue(id, out transform) && transform != null) return true;
            transform = null;
            return false;
        }

        /// <summary>
        /// Transform'un kendisi ya da en yakın kayıtlı atası için kimlik ve o sahibin şu anki dünya pozu. Hareketli bir node
        /// çoğu zaman sürücüyü taşıyan nesnenin çocuğu olduğundan ata aranır; hiçbiri kayıtlı değilse (sabit nesne) false.
        /// </summary>
        public bool TryGetPose(Transform transform, out SurfacePose pose)
        {
            for (var t = transform; t != null; t = t.parent)
            {
                if (!_byTransform.TryGetValue(t, out uint id)) continue;
                pose = new SurfacePose(id, t.position, t.eulerAngles.z);
                return true;
            }
            pose = default;
            return false;
        }

        /// <summary>Transform'dan kimlik; kayıtsız ya da çakışmalıysa false.</summary>
        public bool TryGetId(Transform transform, out uint id)
        {
            id = 0;
            return transform != null && _byTransform.TryGetValue(transform, out id);
        }
    }
}
