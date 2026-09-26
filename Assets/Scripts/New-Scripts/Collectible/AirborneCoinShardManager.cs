using System.Collections.Generic;
using DG.Tweening;
using New_Scripts.Death;
using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Collectible
{
    /// <summary>
    /// Oyuncunun yerle temas etmeden (grounded olmadan) topladığı parça grubunu yönetir.
    /// Oyuncu yere basarsa parçalar kavisli bir şekilde (Bezier) eski yerlerine döner.
    /// Hepsi toplandığında ise oyuncunun üzerinde birleşip hedef altını aktif hale getirir.
    /// </summary>
    public class AirborneCoinShardManager : MonoBehaviour
    {
        [Header("Puzzle Settings")]
        [Tooltip("Sahnede toplanması gereken parça listesi.")]
        [SerializeField] private List<Collectible> shards = new();

        [Tooltip("Tüm parçalar toplandığında aktif olacak hedef altın.")]
        [SerializeField] private Collectible targetCoin;

        [Header("Follow Settings")]
        [Tooltip("Oyuncunun etrafında dönerken kullanılacak yarıçap.")]
        [SerializeField] private float orbitRadius = 1.2f;

        [Tooltip("Dönme hızı.")]
        [SerializeField] private float orbitSpeed = 4.0f;

        [Tooltip("Parçaların oyuncunun yörünge pozisyonuna süzülme yumuşaklığı.")]
        [SerializeField] private float followLerpSpeed = 8.0f;

        [Header("Flyback & Merge Settings")]
        [Tooltip("Yere basıldığında parçaların geri uçuş süresi.")]
        [SerializeField] private float flybackDuration = 0.6f;

        [Tooltip("Geri uçuş kavis yüksekliği.")]
        [SerializeField] private float arcHeight = 1.5f;

        [Tooltip("Uçuş kavisindeki rastgele sapma miktarı.")]
        [SerializeField] private float arcDeviation = 0.4f;

        [Tooltip("Birleşme (merge) animasyonu süresi.")]
        [SerializeField] private float mergeDuration = 0.3f;

        [Header("Merge Flying Settings")]
        [Tooltip("Bütün parçalar toplandıktan sonra hedef coine uçuş süresi.")]
        [SerializeField] private float mergeFlyDuration = 0.8f;

        [Tooltip("Hedef coine uçuş kavis yüksekliği.")]
        [SerializeField] private float mergeArcHeight = 2.0f;

        [Tooltip("Hedef coine uçuş kavisindeki rastgele sapma miktarı.")]
        [SerializeField] private float mergeArcDeviation = 0.5f;

        [Tooltip("Hedef coine uçuşun hız/kolaylaştırma eğrisi (ease curve).")]
        [SerializeField] private AnimationCurve mergeEaseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        private PlayerController player;
        private PlayerHealth playerHealth;
        private Dictionary<Collectible, Vector3> originalPositions = new();
        private List<Collectible> followingShards = new();
        private bool isAllCollected = false;

        private void OnEnable()
        {
            foreach (var shard in shards)
            {
                if (shard != null)
                {
                    shard.OnCollected += HandleShardCollected;
                }
            }
        }

        private void OnDisable()
        {
            foreach (var shard in shards)
            {
                if (shard != null)
                {
                    shard.OnCollected -= HandleShardCollected;
                }
            }
        }

        private void Start()
        {
            // Eğer hedef altın zaten toplanmış ve kaydedilmişse bulmacayı deaktif et
            if (targetCoin != null && (targetCoin.IsCollected || targetCoin.IsCommitted))
            {
                gameObject.SetActive(false);
                return;
            }

            // Orijinal konumları kaydet ve görsel gizleme ayarlarını yap
            foreach (var shard in shards)
            {
                if (shard != null)
                {
                    originalPositions[shard] = shard.transform.position;
                    shard.HideVisualOnCollect = false; // Görselin anında kaybolmasını engelle
                }
            }

            if (targetCoin != null)
            {
                targetCoin.gameObject.SetActive(false);
            }

            // Oyuncu referanslarını bul
            player = FindObjectOfType<PlayerController>();
            playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnDeath += HandlePlayerDeath;
            }
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
            {
                playerHealth.OnDeath -= HandlePlayerDeath;
            }
        }

        private void Update()
        {
            if (isAllCollected || followingShards.Count == 0) return;

            // Oyuncu referansı kaybolduysa tekrar bulmayı dene
            if (player == null)
            {
                player = FindObjectOfType<PlayerController>();
                if (player == null) return;
            }

            // Eğer tüm parçalar toplandıysa
            if (followingShards.Count >= shards.Count)
            {
                // Yere bastığında birleşme tetiklenir
                if (player.IsGrounded)
                {
                    isAllCollected = true;
                    TriggerMergeAndUnlock();
                    return;
                }
            }
            else
            {
                // Tüm parçalar toplanmadıysa ve oyuncu yere bastıysa (grounded) toplanan parçaları geri gönder
                if (player.IsGrounded)
                {
                    TriggerFlyback();
                    return;
                }
            }

            // Parçaların oyuncunun etrafında yörüngede (orbit) dönmesini sağla
            OrbitFollowingShards();
        }

        private void HandleShardCollected(Collectible shard)
        {
            if (isAllCollected) return;

            if (shard != null && !followingShards.Contains(shard))
            {
                followingShards.Add(shard);

                // Eğer tüm parçalar toplandıysa bulmaca tamamlandı
                if (followingShards.Count >= shards.Count)
                {
                    if (player == null)
                    {
                        player = FindObjectOfType<PlayerController>();
                    }
                    // Oyuncu zaten yere basıyorsa hemen birleşmeyi başlat
                    if (player != null && player.IsGrounded)
                    {
                        isAllCollected = true;
                        TriggerMergeAndUnlock();
                    }
                }
            }
        }

        private void OrbitFollowingShards()
        {
            int count = followingShards.Count;
            if (count == 0 || player == null) return;

            for (int i = 0; i < count; i++)
            {
                Collectible shard = followingShards[i];
                if (shard == null) continue;

                // Eşit açılarda yörünge pozisyonunu hesapla
                float angle = Time.time * orbitSpeed + (i * (360f / count) * Mathf.Deg2Rad);
                Vector3 orbitOffset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * orbitRadius;
                Vector3 targetPos = player.transform.position + orbitOffset;

                // Yumuşakça yörüngeye takip ettir
                shard.transform.position = Vector3.Lerp(shard.transform.position, targetPos, Time.deltaTime * followLerpSpeed);
            }
        }

        private void TriggerFlyback()
        {
            // Liste üzerinde işlem yaparken listenin değişmesini engellemek için kopyala
            var shardsToReset = new List<Collectible>(followingShards);
            followingShards.Clear();

            foreach (var shard in shardsToReset)
            {
                if (shard == null) continue;

                shard.transform.DOKill(); // Olası diğer tween'leri durdur

                Vector3 p0 = shard.transform.position;
                Vector3 p2 = originalPositions.ContainsKey(shard) ? originalPositions[shard] : shard.transform.position;
                Vector3 mid = (p0 + p2) * 0.5f;

                // Bezier eğrisi için kavis yönü belirle (yukarı doğru + rastgele sapma)
                Vector3 perpendicular = Vector3.Cross(p2 - p0, Vector3.forward).normalized;
                Vector3 p1 = mid + (Vector3.up * arcHeight) + (perpendicular * Random.Range(-arcDeviation, arcDeviation));

                float t = 0f;
                DOTween.To(() => t, x =>
                {
                    t = x;
                    Vector3 m1 = Vector3.Lerp(p0, p1, t);
                    Vector3 m2 = Vector3.Lerp(p1, p2, t);
                    shard.transform.position = Vector3.Lerp(m1, m2, t);
                }, 1f, flybackDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    shard.ForceReset();
                });
            }
        }

        private void TriggerMergeAndUnlock()
        {
            var shardsToMerge = new List<Collectible>(followingShards);
            followingShards.Clear();

            Vector3 targetMergePos = targetCoin != null ? targetCoin.transform.position : transform.position;

            foreach (var shard in shardsToMerge)
            {
                if (shard == null) continue;

                shard.transform.DOKill();

                Vector3 p0 = shard.transform.position;
                Vector3 p2 = targetMergePos;
                Vector3 mid = (p0 + p2) * 0.5f;

                // Bezier eğrisi için kavis yönü belirle (yukarı doğru + rastgele sapma)
                Vector3 perpendicular = Vector3.Cross(p2 - p0, Vector3.forward).normalized;
                Vector3 p1 = mid + (Vector3.up * mergeArcHeight) + (perpendicular * Random.Range(-mergeArcDeviation, mergeArcDeviation));

                float t = 0f;
                DOTween.To(() => t, x =>
                {
                    t = x;
                    Vector3 m1 = Vector3.Lerp(p0, p1, t);
                    Vector3 m2 = Vector3.Lerp(p1, p2, t);
                    shard.transform.position = Vector3.Lerp(m1, m2, t);
                }, 1f, mergeFlyDuration)
                .SetEase(mergeEaseCurve)
                .OnComplete(() =>
                {
                    shard.gameObject.SetActive(false);
                });
            }

            // Birleşme animasyonundan sonra hedef altını aktif et
            DOVirtual.DelayedCall(mergeFlyDuration, () =>
            {
                if (targetCoin != null)
                {
                    targetCoin.gameObject.SetActive(true);
                }
            });
        }

        private void HandlePlayerDeath()
        {
            // Eğer bulmaca tamamlanmadıysa veya tamamlanıp henüz kaydedilmediyse sıfırla
            if (targetCoin == null || !targetCoin.IsCommitted)
            {
                isAllCollected = false;
                followingShards.Clear();

                foreach (var shard in shards)
                {
                    if (shard != null)
                    {
                        shard.transform.DOKill();
                        shard.gameObject.SetActive(true);
                        shard.ForceReset();
                        if (originalPositions.ContainsKey(shard))
                        {
                            shard.transform.position = originalPositions[shard];
                        }
                    }
                }

                if (targetCoin != null)
                {
                    targetCoin.gameObject.SetActive(false);
                }
            }
        }
    }
}
