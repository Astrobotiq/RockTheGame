using New_Scripts.Player.Nodes.Rotation;
using UnityEngine;

namespace New_Scripts.Player.Nodes.Spiral
{
    public enum SpinDirection
    {
        Clockwise,
        CounterClockwise
    }

    /// <summary>
    /// Tek kancayla tutunulan oyuncuyu kendi yönünde döndüren node (docs/spiral-node-tasarimi.md).
    /// Motoru SwingingState çalıştırır; bu bileşen yönü, hedef hızı ve ters dönen spiralin durumunu taşır.
    /// reverseEveryDegrees > 0 ise ters dönen spiraldir: tutunurken o kadar dönünce yön değiştirir.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class SpiralNode : MonoBehaviour
    {
        [Tooltip("Başlangıç dönüş yönü. Ters dönen spiral bırakılınca bu yöne döner.")]
        [SerializeField] private SpinDirection direction = SpinDirection.CounterClockwise;

        [Tooltip("Oyuncuyu döndürdüğü teğet hız (birim/sn).")]
        [SerializeField] private float targetSpeed = 18f;

        [Tooltip("0 = normal spiral (hiç yön değiştirmez). > 0 = ters dönen spiral: tutunurken bu kadar derece dönünce yön değiştirir.")]
        [SerializeField] private float reverseEveryDegrees = 0f;

        [Tooltip("Kimse tutunmuyorken görselin dönüş hızını hesaplamak için varsayılan ip boyu.")]
        [SerializeField] private float idleReferenceRadius = 3f;

        private bool _flipped;
        private SpiralReverseCounter _reverseCounter;

        public int SpinSign => (direction == SpinDirection.CounterClockwise ? 1 : -1) * (_flipped ? -1 : 1);
        public float TargetSpeed => targetSpeed;
        public float ReverseEveryDegrees => reverseEveryDegrees;
        public bool IsReversing => reverseEveryDegrees > 0f;
        public bool IsConnected { get; private set; }

        /// <summary>Bağlı oyuncunun açısal hızı (derece/sn, işaretli); görsel için.</summary>
        public float ReportedAngularSpeed { get; private set; }

        protected PlayerController ConnectedPlayer { get; private set; }

        /// <summary>Ters dönen spiral yön değiştirdiğinde (ses, titreşim, görsel).</summary>
        public event System.Action Reversed;

        /// <summary>Ters dönen spiralde bir sonraki çevrilmeye ilerleme (0..1); normal spiralde 0.</summary>
        public float ReverseProgress => _reverseCounter != null ? _reverseCounter.Progress : 0f;

        public void Connect(PlayerController player)
        {
            ConnectedPlayer = player;
            IsConnected = true;
            _reverseCounter = IsReversing ? new SpiralReverseCounter(reverseEveryDegrees) : null;
        }

        /// <summary>
        /// Motorun çalıştığı her adımda çağrılır. Ters dönen spiralde spin yönünde reverseEveryDegrees birikince
        /// yön değişir ve Reversed tetiklenir; sayaç sıfırlanıp tekrar saymaya başlar.
        /// </summary>
        public void TickReverse(Vector2 playerPos, Vector2 anchorPos)
        {
            if (!IsConnected || _reverseCounter == null) return;
            if (!_reverseCounter.Tick(playerPos, anchorPos, SpinSign)) return;

            Flip();
            Reversed?.Invoke();
        }

        /// <param name="stillHeld">Kol node'da kaldı mı (örn. çift kancaya geçiş). Görsel için.</param>
        public void Disconnect(bool stillHeld)
        {
            IsConnected = false;
            _flipped = false;
            _reverseCounter = null;
            ReportedAngularSpeed = 0f;
        }

        public void Flip()
        {
            _flipped = !_flipped;
        }

        public void ReportAngularSpeed(float degreesPerSecond)
        {
            ReportedAngularSpeed = degreesPerSecond;
        }

        private void OnValidate()
        {
            if (GetComponentInParent<FullRotationNode>() != null || GetComponentInChildren<FullRotationNode>() != null)
                Debug.LogWarning($"{name}: SpiralNode ve FullRotationNode aynı node'da kullanılamaz; spiral varken dönüş sayacı çalışmaz.", this);
        }

        private void OnDrawGizmos()
        {
            Vector3 center = transform.position;
            int sign = SpinSign;
            Gizmos.color = IsReversing ? new Color(1f, 0.6f, 0.1f) : new Color(0.2f, 0.9f, 1f);

            // Yön oku: R=1 çember yayı + ok ucu
            const int segments = 24;
            Vector3 previous = center + Vector3.right;
            for (int i = 1; i <= segments; i++)
            {
                float a = sign * (i / (float)segments) * Mathf.PI * 1.5f;
                Vector3 p = center + new Vector3(Mathf.Cos(a), Mathf.Sin(a));
                Gizmos.DrawLine(previous, p);
                previous = p;
            }
            float end = sign * Mathf.PI * 1.5f;
            Vector3 tangent = new Vector3(-Mathf.Sin(end), Mathf.Cos(end)) * sign;
            Vector3 radial = new Vector3(Mathf.Cos(end), Mathf.Sin(end));
            Gizmos.DrawLine(previous, previous - tangent * 0.3f + radial * 0.15f);
            Gizmos.DrawLine(previous, previous - tangent * 0.3f - radial * 0.15f);

#if UNITY_EDITOR
            if (targetSpeed <= 0f) return;
            string label = "";
            foreach (float r in new[] { 2f, 5f, 10f })
                label += $"R{r}: {2f * Mathf.PI * r / targetSpeed:0.0} sn\n";
            UnityEditor.Handles.Label(center + new Vector3(1.2f, 1.2f), label.TrimEnd());
#endif
        }
    }
}
