using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace New_Scripts.Animation
{
    /// <summary>
    /// İki cüce (gnome) arasında sıra sıra kusma animasyonunu yöneten basit bir zamanlayıcı kontrolcüsü.
    /// UniTask tabanlı çalışır. Bir cücenin 'isPuking' değerini true yaparken diğerini false yapar.
    /// </summary>
    public class GnomePukeAnimator : MonoBehaviour
    {
        [Header("Gnome Animators")]
        [Tooltip("Birinci cücenin Animator bileşeni.")]
        [SerializeField] private Animator gnomeAnimator1;
        
        [Tooltip("İkinci cücenin Animator bileşeni.")]
        [SerializeField] private Animator gnomeAnimator2;

        [Header("Timing Settings")]
        [Tooltip("Kusma süresinin minimum saniye cinsinden değeri.")]
        [SerializeField] private float minDuration = 3f;
        
        [Tooltip("Kusma süresinin maksimum saniye cinsinden değeri.")]
        [SerializeField] private float maxDuration = 5f;

        [Header("Animator Parameter")]
        [Tooltip("Animator içindeki bool parametresinin adı.")]
        [SerializeField] private string isPukingParameterName = "isPuking";

        private int isPukingHash;
        private CancellationTokenSource cts;

        private void Start()
        {
            isPukingHash = Animator.StringToHash(isPukingParameterName);

            if (gnomeAnimator1 == null || gnomeAnimator2 == null)
            {
                Debug.LogWarning($"[{name}] GnomePukeAnimator: Her iki cüce Animator bileşeninin de atanması gerekiyor!");
            }
        }

        private void OnEnable()
        {
            if (gnomeAnimator1 == null || gnomeAnimator2 == null) return;

            // Önceki task'i güvenli şekilde iptal et ve yeni token oluştur
            CancelPukeTask();
            cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            
            PukeLoopAsync(cts.Token).Forget();
        }

        private void OnDisable()
        {
            CancelPukeTask();
        }

        private void CancelPukeTask()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
                cts = null;
            }
        }

        private async UniTaskVoid PukeLoopAsync(CancellationToken token)
        {
            // Başlangıç durumu: Birinci cüce kusuyor, ikinci cüce kusmuyor.
            bool isFirstGnomePuking = true;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    // Parametreleri güncelle
                    gnomeAnimator1.SetBool(isPukingHash, isFirstGnomePuking);
                    gnomeAnimator2.SetBool(isPukingHash, !isFirstGnomePuking);

                    // 3 ile 5 saniye arasında rastgele bir süre bekle
                    float randomDelay = Random.Range(minDuration, maxDuration);
                    await UniTask.Delay(TimeSpan.FromSeconds(randomDelay), cancellationToken: token);

                    // Sırayı değiştir
                    isFirstGnomePuking = !isFirstGnomePuking;
                }
            }
            catch (OperationCanceledException)
            {
                // Task iptal edildiğinde hata fırlatmak yerine temiz bir şekilde çık
            }
        }
    }
}
