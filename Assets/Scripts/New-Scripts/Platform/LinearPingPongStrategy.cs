using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Verilen iki nokta arasında küresel zamana bağlı olarak doğrusal hareket sağlayan strateji.
    /// </summary>
    public class LinearPingPongStrategy : MovementStrategy
    {
        [SerializeField] private Vector2 startPoint;
        [SerializeField] private Vector2 endPoint;
        [SerializeField] private float period;
        [SerializeField] private float phaseOffset;

        public override float Period => period;

        public override Vector2 GetPositionAtTime(float time)
        {
            float evaluatedTime = (time + phaseOffset) % period;
            float t = Mathf.PingPong(evaluatedTime, period / 2f) / (period / 2f);
            return Vector2.Lerp(startPoint, endPoint, t);
        }
    }
}