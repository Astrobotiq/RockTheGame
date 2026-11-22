using System;
using New_Scripts.Player;
using New_Scripts.Player.IFramePauseable;
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
    public class FlowerCycle : MonoBehaviour, IGrabbableNode, IGrabReactive
    {
        [SerializeField] private FlowerCycleTimings timings = FlowerCycleTimings.Default;
        [Tooltip("Çiçekler aynı anda açılmasın diye döngüye eklenen kayma (saniye).")]
        [SerializeField] private float phaseOffset;
        [Tooltip("Kanca node'u: Grapple katmanındaki collider. Tutulamazken kapatılır, NodeDetector kapalı çiçeği görmez.")]
        [SerializeField] private Collider2D nodeCollider;
        [Tooltip("Çiçek kapanırken tutuluyorsa oyuncuya çiçekten dışa verilen hız (b/sn). Hissi gamepad ile ayarlanır.")]
        [SerializeField, Min(0f)] private float releaseImpulse = 6f;

        [Header("Snap darbesi (opsiyonel)")]
        [Tooltip("Çiçek tutulurken kapanırsa kısa bir hit-stop (HitStopEvents) fırlatma anını vurgular. Varsayılan kapalı: his gamepadle denenir.")]
        [SerializeField] private bool snapHitStop;
        [Tooltip("Hit-stop süresi (sn). ~3 kare = 0.05. Slingshot'ın 0.2 sn'si çiçek için çok uzun.")]
        [SerializeField, Range(0.02f, 0.1f)] private float snapHitStopSeconds = 0.05f;

        private FlowerBloom _bloom;
        private float _time;
        private FlowerPhase _phase = (FlowerPhase)(-1);
        private bool _grabbable;

        public FlowerPhase Phase => _phase;
        public bool IsGrabbable => _grabbable;
        public float ReleaseImpulse => releaseImpulse;

        /// <summary>Tutulabilirlik değiştiğinde (true = tutulabilir).</summary>
        public event Action<bool> GrabbableChanged;

        // Tutunma durumu (IGrabReactive): çiçek tepkisi (FlowerReactor) ve polen bunu okur. Tutulabilirlik kapısı tüketicidedir.
        private bool _leftHeld;
        private bool _rightHeld;
        private Vector2 _leftGripPoint;
        private Vector2 _rightGripPoint;

        /// <summary>Herhangi bir kol çiçeğe tutunuyor mu.</summary>
        public bool IsHeld => _leftHeld || _rightHeld;
        public bool IsHeldBy(ActiveArm arm) => arm == ActiveArm.Left ? _leftHeld : arm == ActiveArm.Right && _rightHeld;
        /// <summary>Kolun tutunma noktası (dünya); kol tutmuyorsa son nokta.</summary>
        public Vector2 GripPoint(ActiveArm arm) => arm == ActiveArm.Left ? _leftGripPoint : _rightGripPoint;

        /// <summary>Bir kol tutundu (kol, dünya noktası). Aynı karede ikinci kol da tutunabilir.</summary>
        public event Action<ActiveArm, Vector2> Grabbed;
        /// <summary>Bir kol bıraktı (kol). Çiçeğin kapanması, ölüm, el bırakma gibi her neden için gelir.</summary>
        public event Action<ActiveArm> Released;

        public void OnGrabbed(ActiveArm arm, Vector2 worldPoint)
        {
            if (arm == ActiveArm.Left) { _leftHeld = true; _leftGripPoint = worldPoint; }
            else if (arm == ActiveArm.Right) { _rightHeld = true; _rightGripPoint = worldPoint; }
            else return;
            Grabbed?.Invoke(arm, worldPoint);
        }

        public void OnReleased(ActiveArm arm)
        {
            if (!IsHeldBy(arm)) return;
            if (arm == ActiveArm.Left) _leftHeld = false; else _rightHeld = false;
            Released?.Invoke(arm);
        }

        private void OnEnable()
        {
            _leftHeld = _rightHeld = false;   // oda uyandığında eski tutunma kalmasın (olay yok: tüketiciler de sıfırlanır)
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
                    case FlowerPhase.Closing:
                        _bloom.CloseBloom();
                        if (snapHitStop && IsHeld) HitStopEvents.RequestHitStop?.Invoke(snapHitStopSeconds);
                        break;
                    case FlowerPhase.Closed: _bloom.SetWarn(0f); break;
                }
            }

            if (_phase == FlowerPhase.Warning) _bloom.SetWarn(s.Warn01, (1f - s.Warn01) * timings.warning);
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
