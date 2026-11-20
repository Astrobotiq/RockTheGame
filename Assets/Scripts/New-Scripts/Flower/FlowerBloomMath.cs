using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Çiçek açılışı için saf hesap. Genel açılma değeri (0 tomurcuk, 1 tam açık) her yaprağa katman ve sıra
    /// gecikmesiyle dağıtılır: dış katman önce, iç katman sonra açılır (ortası en son görünür); kapanırken ters
    /// sırayla (iç önce) kapanır. Aynı katmandaki yapraklar altın oran adımıyla dağılır, komşular birlikte hareket etmez.
    /// </summary>
    public static class FlowerBloomMath
    {
        private const float GoldenRatioConjugate = 0.6180339887f;

        /// <summary>Katman içi gecikme sırası (0..1): altın oran adımı, komşu yapraklar ardışık değil dağınık sırada açılır.</summary>
        public static float PetalRank(int index) => Mathf.Repeat(index * GoldenRatioConjugate, 1f);

        /// <summary>
        /// Bir yaprağın kendi açılma ilerlemesi (0..1, yumuşatılmış). open 0 iken hepsi 0, open 1 iken hepsi 1.
        /// layerDelay: bir iç katmanın başlama gecikmesi, indexStagger: katman içi en geç yaprağın gecikmesi
        /// (ikisi de toplam açılma değerinin oranı; toplamları 0,9'u aşarsa pencere çok daralmasın diye sınırlanır).
        /// </summary>
        public static float PetalProgress(float open, int layer, int layerCount, int index, float layerDelay, float indexStagger)
        {
            layerCount = Mathf.Max(layerCount, 1);

            float maxDelay = Mathf.Min(0.9f, (layerCount - 1) * layerDelay + indexStagger);
            float delay = Mathf.Min(maxDelay, layer * layerDelay + indexStagger * PetalRank(index));
            float window = 1f - maxDelay;

            float t = Mathf.Clamp01((open - delay) / window);
            return t * t * (3f - 2f * t);
        }
    }
}
