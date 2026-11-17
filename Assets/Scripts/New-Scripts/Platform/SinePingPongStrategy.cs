using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// İki nokta arasında uçlarda yavaşlayıp ortada hızlanan (sinüs) gidip gelme. İvme süreklidir: ani dönüş yok,
    /// hareketli node'da salınıma geçen atalet her an hissedilir ve dönüş önceden okunur (docs/hareketli-node-tasarimi.md).
    /// Noktalar dünya koordinatıdır.
    /// </summary>
    public class SinePingPongStrategy : MovementStrategy
    {
        [SerializeField] private Vector2 startPoint;
        [SerializeField] private Vector2 endPoint;
        [SerializeField] private float period = 4f;
        [SerializeField] private float phaseOffset;

        public override float Period => period;

        public override bool TryGetPathBounds(out Rect worldBounds)
        {
            worldBounds = RectFromPoints(startPoint, endPoint);
            return true;
        }

        public override Vector2 GetPositionAtTime(float time)
        {
            if (period <= 0f) return startPoint;

            float t = 0.5f - 0.5f * Mathf.Cos(2f * Mathf.PI * (time + phaseOffset) / period);
            return Vector2.Lerp(startPoint, endPoint, t);
        }
    }
}
