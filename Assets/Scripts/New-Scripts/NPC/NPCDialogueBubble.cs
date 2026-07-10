using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;

namespace New_Scripts.NPC
{
    public class NPCDialogueBubble : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform bubbleContainer;
        [SerializeField] private TextMeshProUGUI dialogueText;

        [Header("Scale Animation Settings")]
        [SerializeField] private float scaleDuration = 0.3f;
        [SerializeField] private Ease scaleInEase = Ease.OutBack;
        [SerializeField] private Ease scaleOutEase = Ease.InQuad;

        private void Awake()
        {
            if (bubbleContainer == null)
            {
                bubbleContainer = GetComponent<RectTransform>();
            }
            
            // Başlangıçta görünmez yapalım
            bubbleContainer.localScale = Vector3.zero;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Konuşma balonunu büyütür ve metni harf harf yazdırır.
        /// </summary>
        public async UniTask ShowDialogueAsync(string text, float delayBetweenChars, CancellationToken ct)
        {
            // Önceki tweenleri durdur
            bubbleContainer.DOKill();
            
            // Canvas/Arayüzü aktif et
            gameObject.SetActive(true);
            
            // Metni hazırla (MaxVisibleCharacters = 0 yaparak gizliyoruz)
            dialogueText.text = text;
            dialogueText.maxVisibleCharacters = 0;

            // Baloncuğu büyüt
            try
            {
                await PlayScaleTweenAsync(Vector3.one, scaleDuration, scaleInEase, ct);
            }
            catch (OperationCanceledException)
            {
                // İptal durumunda (örn. oyuncu alandan hemen çıktıysa) işlemi durdur
                return;
            }

            // Metni harf harf göster
            try
            {
                int totalChars = text.Length;
                for (int i = 0; i <= totalChars; i++)
                {
                    dialogueText.maxVisibleCharacters = i;
                    
                    // Harfler arası bekleme
                    await UniTask.Delay(TimeSpan.FromSeconds(delayBetweenChars), cancellationToken: ct);
                }
            }
            catch (OperationCanceledException)
            {
                // Yazma animasyonu yarıda kesilirse sessizce durur
            }
        }

        /// <summary>
        /// Konuşma balonunu küçülterek kapatır.
        /// </summary>
        public async UniTask HideDialogueAsync(CancellationToken ct)
        {
            try
            {
                await PlayScaleTweenAsync(Vector3.zero, scaleDuration, scaleOutEase, ct);
                
                // Küçülme tamamlandığında çizim yükünü (Draw Call) önlemek için deaktif et
                gameObject.SetActive(false);
            }
            catch (OperationCanceledException)
            {
                // Kapatma işlemi yarıda kesilirse (örn. oyuncu tekrar yaklaşırsa) deaktif etmeyiz
            }
        }

        private async UniTask PlayScaleTweenAsync(Vector3 targetScale, float duration, Ease ease, CancellationToken ct)
        {
            bubbleContainer.DOKill();
            
            var tcs = new UniTaskCompletionSource<bool>();
            using (ct.Register(() => {
                bubbleContainer.DOKill();
                tcs.TrySetCanceled(ct);
            }))
            {
                bubbleContainer.DOScale(targetScale, duration)
                    .SetEase(ease)
                    .OnComplete(() => tcs.TrySetResult(true));

                await tcs.Task;
            }
        }

        private void OnDestroy()
        {
            if (bubbleContainer != null)
            {
                bubbleContainer.DOKill();
            }
        }
    }
}
