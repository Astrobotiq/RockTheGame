using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Baslangic ve bitis Transform nesnelerinin pozisyonlarini baslangicta onbellege alarak, iki nokta arasinda zamana bagli dogrusal hareket saglayan strateji.
    /// </summary>
    public class TransformPingPongStrategy : MovementStrategy
    {
        [SerializeField] private Transform startTransform;
        [SerializeField] private Transform endTransform;
        [SerializeField] private float period = 1f;
        [SerializeField] private float phaseOffset;

        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private bool _isInitialized;

        private void Awake()
        {
            if (startTransform != null && endTransform != null)
            {
                _startPosition = startTransform.position;
                _endPosition = endTransform.position;
                _isInitialized = true;
            }
            else
            {
                Debug.LogError("TransformPingPongStrategy: Baslangic veya bitis Transform referanslari atanmadi!", this);
            }
        }

        public override Vector2 GetPositionAtTime(float time)
        {
            if (!_isInitialized)
                return Vector2.zero;

            float evaluatedTime = (time + phaseOffset) % period;
            float t = Mathf.PingPong(evaluatedTime, period / 2f) / (period / 2f);

            return Vector2.Lerp(_startPosition, _endPosition, t);
        }
    }
}