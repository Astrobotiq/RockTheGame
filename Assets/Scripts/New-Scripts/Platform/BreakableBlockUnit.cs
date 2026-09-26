using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Parçalanabilir platformun her bir tekil blok birimini (kutusunu) temsil eden sınıf.
    /// İçten dışa doğru sprite animasyonu ile yok olmasını ve collider kapatma işlemini yönetir.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class BreakableBlockUnit : MonoBehaviour
    {
        [Header("Animation Settings")]
        [Tooltip("İçten dışa doğru parçalanma karelerini sırasıyla ekleyin.")]
        [SerializeField] private Sprite[] breakFrames;
        [Tooltip("Animasyonun saniyedeki kare hızı (FPS).")]
        [SerializeField] private float frameRate = 16f;

        [Header("References")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private BoxCollider2D blockCollider;

        private Sprite _defaultSprite;
        private bool _isBroken;

        public bool IsBroken => _isBroken;
        public BoxCollider2D BlockCollider => blockCollider;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (blockCollider == null) blockCollider = GetComponent<BoxCollider2D>();

            if (spriteRenderer != null)
            {
                _defaultSprite = spriteRenderer.sprite;
            }
        }

        /// <summary>
        /// Bloğun içten dışa parçalanma animasyonunu oynatır ve tamamlandığında katı özelliğini devre dışı bırakır.
        /// </summary>
        public async UniTask BreakBlockAsync(CancellationToken ct)
        {
            if (_isBroken) return;
            _isBroken = true;

            if (breakFrames != null && breakFrames.Length > 0)
            {
                float delayPerFrame = 1f / Mathf.Max(1f, frameRate);

                for (int i = 0; i < breakFrames.Length; i++)
                {
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.sprite = breakFrames[i];
                    }

                    await UniTask.Delay(TimeSpan.FromSeconds(delayPerFrame), cancellationToken: ct);
                }
            }

            // Animasyon tamamlandıktan sonra bloğun fiziksel temasını ve görselini kapat
            if (blockCollider != null)
            {
                blockCollider.enabled = false;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
        }

        /// <summary>
        /// Bloğu varsayılan sağlam durumuna sıfırlar.
        /// </summary>
        public void ResetBlock()
        {
            _isBroken = false;

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = (breakFrames != null && breakFrames.Length > 0) ? breakFrames[0] : _defaultSprite;
                spriteRenderer.enabled = true;
            }

            if (blockCollider != null)
            {
                blockCollider.enabled = true;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (blockCollider == null) blockCollider = GetComponent<BoxCollider2D>();
        }
#endif
    }
}
