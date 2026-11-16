using New_Scripts.UI;
using New_Scripts.UI.Theme;
using UnityEngine;

namespace New_Scripts.Tutorial
{
    /// <summary>
    /// Swing tutorial'ının iki ipucunu (1: kolu çevir + RS, 2: ateşle + RT) <see cref="SwingTutorial.StepChanged"/>'e
    /// göre gösterir/gizler. Giriş ve çıkış <see cref="UiSpring"/> ölçeğiyle, unscaled zamanla (tutorial zamanı
    /// yavaşlatırken/durdururken de çalışmalı). Başta gizli; görünürken düz durur (nefes yok).
    /// <para>
    /// Bir ipucu, grubun yalnızca ikon (<c>Animation</c>) ve yazı (<c>Tutorial-Text</c>) parçalarıdır; grubun kökü
    /// ve içindeki "SWING" yazısı (<c>TextEffect</c>) hep açık kalır, tutorial'dan bağımsız sallanır.
    /// </para>
    /// </summary>
    public class SwingTutorialHints : MonoBehaviour
    {
        [SerializeField] private SwingTutorial tutorial;
        [Tooltip("Move Right Arm + RS animasyonu: ikon ve yazı nesneleri.")]
        [SerializeField] private Transform[] armHint;
        [Tooltip("Right Grapple + RT animasyonu: ikon ve yazı nesneleri.")]
        [SerializeField] private Transform[] fireHint;

        [Header("Hareket (başlangıç önerisi, gamepad ile ayarlanır)")]
        [SerializeField] private float stiffness = 340f;
        [SerializeField] private float damping = 22f;

        private sealed class Hint
        {
            public Transform[] Parts;
            public Vector3[] BaseScales;
            public UiSpring Spring;
        }

        private Hint _arm;
        private Hint _fire;

        /// <summary>Adıma göre hangi ipucu görünür (saf tablo, EditMode testli).</summary>
        public static void VisibleHints(SwingTutorialStep step, out bool arm, out bool fire)
        {
            arm = step == SwingTutorialStep.Slowing || step == SwingTutorialStep.Aim;
            fire = step == SwingTutorialStep.Fire;
        }

        private void Awake()
        {
            _arm = Create(armHint);
            _fire = Create(fireHint);
        }

        private Hint Create(Transform[] parts)
        {
            if (parts == null || parts.Length == 0) return null;
            var hint = new Hint
            {
                Parts = parts,
                BaseScales = new Vector3[parts.Length],
                Spring = new UiSpring(stiffness, damping)
            };
            for (int i = 0; i < parts.Length; i++) hint.BaseScales[i] = parts[i].localScale;
            Apply(hint);
            return hint;
        }

        private void OnEnable()
        {
            if (tutorial == null) return;
            tutorial.StepChanged += OnStepChanged;
            // Oda uyandığında mevcut adıma anında oturur.
            Retarget(tutorial.Step, snap: true);
        }

        private void OnDisable()
        {
            if (tutorial != null) tutorial.StepChanged -= OnStepChanged;
        }

        private void OnStepChanged(SwingTutorialStep step) => Retarget(step, snap: false);

        private void Retarget(SwingTutorialStep step, bool snap)
        {
            VisibleHints(step, out bool arm, out bool fire);
            SetTarget(_arm, arm, snap);
            SetTarget(_fire, fire, snap);
        }

        private void SetTarget(Hint hint, bool visible, bool snap)
        {
            if (hint == null) return;
            hint.Spring.Target = visible ? 1f : 0f;
            if (snap)
            {
                hint.Spring.Value = hint.Spring.Target;
                hint.Spring.Velocity = 0f;
                Apply(hint);
            }
        }

        private void Update()
        {
            Step(_arm);
            Step(_fire);
        }

        private void Step(Hint hint)
        {
            if (hint == null) return;
            hint.Spring.Step(Time.unscaledDeltaTime);
            Apply(hint);
        }

        private static void Apply(Hint hint)
        {
            float s = Mathf.Max(0f, hint.Spring.Value);
            bool visible = hint.Spring.Target > 0f || s > 0.001f;

            for (int i = 0; i < hint.Parts.Length; i++)
            {
                var part = hint.Parts[i];
                if (part == null) continue;

                if (part.gameObject.activeSelf != visible)
                {
                    part.gameObject.SetActive(visible);
                    if (visible) RestartAnimator(part);
                }

                if (visible) part.localScale = hint.BaseScales[i] * s;
            }
        }

        /// <summary>
        /// Kapanıp açılan ikonun Animator'ı parametrelerini sıfırlar; <see cref="UIAnimatorController"/> hangi
        /// animasyonun oynayacağını yalnızca <c>OnEnable</c>'da yazdığı için kökü kapatıp açarak yeniden yazdırır.
        /// </summary>
        private static void RestartAnimator(Transform part)
        {
            var controller = part.GetComponentInParent<UIAnimatorController>();
            if (controller == null) return;
            controller.enabled = false;
            controller.enabled = true;
        }
    }
}
