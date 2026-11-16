using System;
using New_Scripts.Death;
using New_Scripts.Pause;
using New_Scripts.Player;
using New_Scripts.Player.States;
using UnityEngine;

namespace New_Scripts.Tutorial
{
    /// <summary>Tutorial'ın öğrettiği kol: nişan o kolun stick'iyle, ateş o kolun tetiğiyle.</summary>
    public enum SwingTutorialArm
    {
        Right,
        Left
    }

    /// <summary>
    /// Oda 5'teki ilk node için swing tutorial'ı: oyuncu havada ve node erişilebilirken zamanı yavaşlatır,
    /// tepede bekletir, tutununca normale döndürür. Karar mantığı <see cref="SwingTutorialFlow"/>'da; bu bileşen
    /// oyuncudan girdileri toplar, <c>Time.timeScale</c>/<c>fixedDeltaTime</c>'ı yazar ve geri yükler.
    /// Tasarım: docs/swing-tutorial-plani.md.
    /// </summary>
    public class SwingTutorial : MonoBehaviour
    {
        [SerializeField] private SwingTutorialSettingsSO settings;
        [Tooltip("Boşsa sahnedeki PlayerController aranır.")]
        [SerializeField] private PlayerController player;
        [Tooltip("Öğretilen node (katman Grapple).")]
        [SerializeField] private Transform node;
        [Tooltip("Nişan hangi stick ile alınıyor. Tutunma hangi kolla olursa olsun başarılı sayılır.")]
        [SerializeField] private SwingTutorialArm arm = SwingTutorialArm.Right;

        private SwingTutorialFlow _flow;
        private PlayerHealth _health;
        private float _baseFixedDeltaTime;
        private bool _controllingTime;
        private SwingTutorialStep _lastStep;

        /// <summary>Adım değiştiğinde tetiklenir (ipuçları buna bağlanır).</summary>
        public event Action<SwingTutorialStep> StepChanged;

        public SwingTutorialStep Step => _flow != null ? _flow.Step : SwingTutorialStep.Waiting;

        private void Awake()
        {
            if (settings == null || node == null)
            {
                Debug.LogError("[SwingTutorial] settings ya da node atanmamış, tutorial kapalı.", this);
                enabled = false;
                return;
            }

            if (player == null) player = FindFirstObjectByType<PlayerController>();
            _flow = new SwingTutorialFlow(settings);
            _baseFixedDeltaTime = Time.fixedDeltaTime;
        }

        private void OnEnable()
        {
            if (player == null) return;
            _health = player.GetComponent<PlayerHealth>();
            if (_health != null) _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            if (_health != null) _health.OnDeath -= HandleDeath;
            _health = null;
            // Oda uyutulursa ya da sahne kapanırsa zaman takılı kalmasın.
            RestoreTime();
        }

        private void Update()
        {
            if (_flow == null || player == null) return;

            // Çok oyunculu yarışta timeScale yerel kalır ama rakibin hayaletini de yavaşlatır: tutorial çalışmaz.
            if (PauseManager.IsPauseBlocked)
            {
                RestoreTime();
                return;
            }

            // Ölüyken (oyuncu kapalı) girdi yok; ölüm HandleDeath'te işlendi.
            if (!player.gameObject.activeInHierarchy) return;

            _flow.Tick(BuildInput());
            PublishStep();

            // Pause'da timeScale'e yazılmaz: PauseManager kendi sakladığı değeri devamda geri verir.
            if (PauseManager.BlocksGameplay) return;

            if (_flow.IsControllingTime) ApplyTime(_flow.TargetTimeScale);
            else RestoreTime();
        }

        private SwingTutorialInput BuildInput()
        {
            Vector2 playerPos = player.PlayerRigidbody.position;
            Vector2 nodePos = node.position;
            float threshold = player.Stats.NodeCoincidenceThreshold;

            Vector2 toNode = nodePos - playerPos;
            bool reachable = toNode.sqrMagnitude > 0.0001f
                             && player.TryCastGrapple(toNode.normalized, out Vector2 found)
                             && Vector2.Distance(found, nodePos) < threshold;

            // Detector kendi kurallarıyla (yarıçap, koni, engel) öğretilen kolun stick'inin seçtiği node'u verir.
            Vector2 stick = arm == SwingTutorialArm.Right ? player.Input.RightStick : player.Input.LeftStick;
            bool aimed = player.TryCastGrapple(stick, out Vector2 aimedPoint)
                         && Vector2.Distance(aimedPoint, nodePos) < threshold;

            return new SwingTutorialInput
            {
                IsAirborne = player.IsInNormalAir,
                HasAnyAnchor = player.LeftAnchor.HasValue || player.RightAnchor.HasValue,
                NodeReachable = reachable,
                AimedAtNode = aimed,
                AnchoredToNode = IsAnchoredToNode(player.LeftAnchor, nodePos, threshold)
                                 || IsAnchoredToNode(player.RightAnchor, nodePos, threshold),
                VelocityY = player.Velocity.y,
                UnscaledDt = Time.unscaledDeltaTime,
                Paused = PauseManager.BlocksGameplay
            };
        }

        private static bool IsAnchoredToNode(Vector2? anchor, Vector2 nodePos, float threshold)
        {
            return anchor.HasValue && Vector2.Distance(anchor.Value, nodePos) < threshold;
        }

        private void HandleDeath()
        {
            _flow?.Reset();
            RestoreTime();
            PublishStep();
        }

        private void PublishStep()
        {
            if (_flow.Step == _lastStep) return;
            _lastStep = _flow.Step;
            StepChanged?.Invoke(_lastStep);
        }

        private void ApplyTime(float scale)
        {
            _controllingTime = true;
            Time.timeScale = scale;
            // Yavaşken fizik adımı da küçülsün ki hareket kesik görünmesin. 0'da FixedUpdate zaten çalışmaz.
            if (scale > 0f) Time.fixedDeltaTime = _baseFixedDeltaTime * scale;
        }

        /// <summary>Yalnızca bu bileşen zamanı değiştirdiyse geri yükler; pause'un 0'ını ezmez.</summary>
        private void RestoreTime()
        {
            if (!_controllingTime) return;
            _controllingTime = false;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = _baseFixedDeltaTime;
        }
    }
}
