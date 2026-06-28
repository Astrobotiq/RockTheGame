using System.Collections.Generic;
using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Seviyedeki aktif IResettable bileşenlerini takip eden ve oyuncu öldüğünde
    /// hepsinin sıfırlanmasını koordine eden yönetici.
    /// </summary>
    public class LevelResetManager : MonoBehaviour
    {
        public static LevelResetManager Instance { get; private set; }

        private readonly List<IResettable> _resettables = new List<IResettable>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Bir sıfırlanabilir bileşeni sisteme kaydeder.
        /// </summary>
        public void Register(IResettable resettable)
        {
            if (!_resettables.Contains(resettable))
            {
                _resettables.Add(resettable);
            }
        }

        /// <summary>
        /// Bir sıfırlanabilir bileşeni sistemden çıkarır.
        /// </summary>
        public void Unregister(IResettable resettable)
        {
            _resettables.Remove(resettable);
        }

        /// <summary>
        /// Kayıtlı tüm bileşenleri varsayılan durumlarına sıfırlar.
        /// </summary>
        public void ResetAll()
        {
            // Olası koleksiyon değişimlerini önlemek için geçici bir liste kopyası oluşturuyoruz
            var copy = new List<IResettable>(_resettables);

            foreach (var resettable in copy)
            {
                // Unity nesnesinin yok edilip edilmediğini kontrol eder
                if (resettable is Object unityObject && unityObject == null)
                {
                    continue;
                }

                if (resettable != null)
                {
                    resettable.ResetToDefault();
                }
            }

            Debug.Log($"LevelResetManager: {copy.Count} adet bileşen sıfırlandı.");
        }
    }
}
