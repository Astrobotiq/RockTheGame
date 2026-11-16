using System.Threading;
using Cysharp.Threading.Tasks;
using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Animation
{
    /// <summary>
    /// Oyuncu tetikleyici alana girince bir Animator state'ini baştan oynatır, bitince ilk poza döner.
    /// Genel amaçlı: tabela, çalı, zincir gibi "dokununca sallanan" dekorlara kod yazmadan eklenir.
    /// Animator controller'ında değişiklik gerekmez; state'in kendiliğinden başlamasını da bu bileşen engeller.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class TouchAnimationTrigger : MonoBehaviour
    {
        [Header("Hedef")]
        [Tooltip("Oynatılacak Animator. Boşsa bu objede, sonra çocuklarında aranır.")]
        [SerializeField] private Animator animator;

        [Tooltip("Oynatılacak state'in adı. Boşsa controller'daki varsayılan state oynar.")]
        [SerializeField] private string stateName = "";

        [Header("Oynatma")]
        [Tooltip("Klip kaç tur oynasın. Klip döngülüyse 1 = bir sallanma.")]
        [Min(1)] [SerializeField] private int cycles = 1;

        [Tooltip("Oynarken yeniden dokunulursa animasyon baştan mı başlasın? Kapalıysa bitene kadar yok sayılır.")]
        [SerializeField] private bool restartOnRetouch = false;

        private int stateHash;
        private CancellationTokenSource cts;
        private bool isPlaying;

        public bool IsPlaying => isPlaying;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogWarning($"[{name}] TouchAnimationTrigger: Animator bulunamadı.", this);
                enabled = false;
                return;
            }

            // Varsayılan state'in hash'i ancak animator bir kez değerlendirildikten sonra okunabilir.
            animator.Update(0f);
            stateHash = string.IsNullOrEmpty(stateName)
                ? animator.GetCurrentAnimatorStateInfo(0).shortNameHash
                : Animator.StringToHash(stateName);
        }

        private void OnEnable()
        {
            // Oda uyuyup uyanınca animator state'i sıfırlanır; hız 1'de kalmış olabilir, o yüzden her seferinde dinlenme pozuna al.
            ResetToRest();
        }

        private void OnDisable()
        {
            CancelPlayTask();
            isPlaying = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out PlayerController _))
            {
                Play();
            }
        }

        /// <summary>Animasyonu baştan oynatır. UnityEvent veya başka koddan da çağrılabilir.</summary>
        public void Play()
        {
            if (animator == null || !isActiveAndEnabled) return;
            if (isPlaying && !restartOnRetouch) return;

            CancelPlayTask();
            cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            PlayAsync(cts.Token).Forget();
        }

        private async UniTaskVoid PlayAsync(CancellationToken token)
        {
            isPlaying = true;
            animator.speed = 1f;
            animator.Play(stateHash, 0, 0f);
            // Play bir sonraki kareye kadar state bilgisini güncellemez; sıfırdan başladığını hemen yansıt.
            animator.Update(0f);

            bool cancelled = await UniTask
                .WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= cycles, cancellationToken: token)
                .SuppressCancellationThrow();

            if (cancelled) return;

            isPlaying = false;
            ResetToRest();
        }

        private void ResetToRest()
        {
            if (animator == null) return;

            animator.Play(stateHash, 0, 0f);
            animator.Update(0f);
            animator.speed = 0f;
        }

        private void CancelPlayTask()
        {
            if (cts == null) return;
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }
}
