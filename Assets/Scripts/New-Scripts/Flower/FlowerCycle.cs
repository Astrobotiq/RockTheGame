using System;
using New_Scripts.Player.Nodes;
using UnityEngine;

namespace New_Scripts.Flower
{
    /// <summary>
    /// Zamanlı node döngüsü: kapalı -> açılış -> açık -> uyarı -> snap. Zamanı <see cref="FlowerCycleMath"/> hesaplar;
    /// bu bileşen faz değişimlerinde <see cref="FlowerBloom"/>'u sürer ve kancaya <see cref="IsGrabbable"/> ile tutulabilirliği bildirir.
    ///
    /// Kanca bağlantısı: node collider'ı yalnızca tutulabilirken açıktır (<c>NodeDetector</c> kapalı çiçeği görmez, hedef işareti de çıkmaz).
    /// Çiçek kapanırken tutuluyorsa <see cref="IGrabbableNode"/> üzerinden <c>PlayerController</c> kolu bırakır ve oyuncuyu
    /// <see cref="ReleaseImpulse"/> kadar çiçekten iter.
    /// </summary>
    [RequireComponent(typeof(FlowerBloom))]
    public class FlowerCycle : MonoBehaviour, IGrabbableNode
    {
        [SerializeField] private FlowerCycleTimings timings = FlowerCycleTimings.Default;
        [Tooltip("Çiçekler aynı anda açılmasın diye döngüye eklenen kayma (saniye).")]
        [SerializeField] private float phaseOffset;
        [Tooltip("Kanca node'u: Grapple katmanındaki collider. Tutulamazken kapatılır, NodeDetector kapalı çiçeği görmez.")]
        [SerializeField] private Collider2D nodeCollider;
        [Tooltip("Çiçek kapanırken tutuluyorsa oyuncuya çiçekten dışa verilen hız (b/sn). Hissi gamepad ile ayarlanır.")]
        [SerializeField, Min(0f)] private float releaseImpulse = 6f;

        private FlowerBloom _bloom;
        private float _time;
        private FlowerPhase _phase = (FlowerPhase)(-1);
        private bool _grabbable;

        public FlowerPhase Phase => _phase;
        public bool IsGrabbable => _grabbable;
        public float ReleaseImpulse => releaseImpulse;

        /// <summary>Tutulabilirlik değiştiğinde (true = tutulabilir).</summary>
        public event Action<bool> GrabbableChanged;

        private void OnEnable()
        {
            _bloom = GetComponent<FlowerBloom>();
            _time = phaseOffset;
            _phase = (FlowerPhase)(-1);
            var s = FlowerCycleMath.Evaluate(_time, timings);
            _bloom.Snap(s.Phase == FlowerPhase.Closed || s.Phase == FlowerPhase.Closing ? 0f : 1f);
            SetGrabbable(s.Grabbable, notify: false);
        }

        private void Update()
        {
            if (!Application.isPlaying) return;

            _time += Time.deltaTime;
            var s = FlowerCycleMath.Evaluate(_time, timings);

            if (s.Phase != _phase)
            {
                _phase = s.Phase;
                switch (_phase)
                {
                    case FlowerPhase.Opening: _bloom.OpenBloom(); break;
                    case FlowerPhase.Closing: _bloom.CloseBloom(); break;
                    case FlowerPhase.Closed: _bloom.SetWarn(0f); break;
                }
            }

            if (_phase == FlowerPhase.Warning) _bloom.SetWarn(s.Warn01);
            if (_phase == FlowerPhase.Closed) _bloom.SetHint(s.Hint01);

            if (s.Grabbable != _grabbable) SetGrabbable(s.Grabbable, notify: true);
        }

        private void SetGrabbable(bool grabbable, bool notify)
        {
            _grabbable = grabbable;
            if (nodeCollider != null) nodeCollider.enabled = grabbable;
            if (notify) GrabbableChanged?.Invoke(grabbable);
        }
    }
}
