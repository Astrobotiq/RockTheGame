using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using New_Scripts.Death;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Üzerine çıkıldığında titreyip bir süre sonra kırılan/yok olan platform.
    /// Kendi üzerinde Player olup olmadığını kendi kontrol eder.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class BreakablePlatform : MonoBehaviour, IResettable
    {
        [Header("Settings")]
        [Tooltip("Sadece Player'ın bulunduğu Layer'ı seçin.")]
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private float breakDelay = 0.5f;
        [SerializeField] private float respawnTime = 3f;
        [SerializeField] private float shakeIntensity = 0.05f;

        [Header("References")]
        [Tooltip("Titreme efektinin verileceği, SpriteRenderer'ı taşıyan alt obje.")]
        [SerializeField] private Transform visualTransform; 
        [SerializeField] private ParticleSystem breakVFXPrefab;
        [SerializeField] private ParticleSystem reformVFXPrefab;

        private BoxCollider2D _solidCollider;
        private bool _isTriggered;
        private Vector3 _originalVisualPosition;
        private CancellationTokenSource _breakCts;

        private void Awake()
        {
            _solidCollider = GetComponent<BoxCollider2D>();
            
            if (visualTransform != null)
            {
                _originalVisualPosition = visualTransform.localPosition;
            }
        }

        private void OnEnable()
        {
            if (LevelResetManager.Instance != null)
            {
                LevelResetManager.Instance.Register(this);
            }
        }

        private void OnDisable()
        {
            CancelBreakSequence();
            if (LevelResetManager.Instance != null)
            {
                LevelResetManager.Instance.Unregister(this);
            }
        }

        private void FixedUpdate()
        {
            if (_isTriggered) return;

            Vector2 boxCenter = (Vector2)transform.position + _solidCollider.offset + (Vector2.up * 0.05f);
            Vector2 boxSize = _solidCollider.bounds.size;
            boxSize.y += 0.05f; 

            Collider2D hit = Physics2D.OverlapBox(boxCenter, boxSize, 0f, playerLayer);
            
            if (hit != null)
            {
                StartBreakSequence();
            }
        }

        private void StartBreakSequence()
        {
            CancelBreakSequence();
            _breakCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
            BreakSequenceAsync(_breakCts.Token).Forget();
        }

        private void CancelBreakSequence()
        {
            if (_breakCts != null)
            {
                _breakCts.Cancel();
                _breakCts.Dispose();
                _breakCts = null;
            }
        }

        private async UniTaskVoid BreakSequenceAsync(CancellationToken ct)
        {
            _isTriggered = true;

            float timer = 0f;
            while (timer < breakDelay)
            {
                timer += Time.deltaTime;
                
                if (visualTransform != null)
                {
                    Vector2 randomShake = Random.insideUnitCircle * shakeIntensity;
                    visualTransform.localPosition = _originalVisualPosition + (Vector3)randomShake;
                }
                
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            if (visualTransform != null)
            {
                visualTransform.localPosition = _originalVisualPosition;
                visualTransform.gameObject.SetActive(false);
            }
            
            _solidCollider.enabled = false; 
            
            if (breakVFXPrefab != null)
            {
                Vector2 center = (Vector2)transform.position + _solidCollider.offset;
                Instantiate(breakVFXPrefab, center, Quaternion.identity);
            }
            
            float rewindVFXDuration = 0.4f; 
            float initialWait = Mathf.Max(0f, respawnTime - rewindVFXDuration);

            await UniTask.Delay(TimeSpan.FromSeconds(initialWait), cancellationToken: ct);

            if (reformVFXPrefab != null)
            {
                Vector2 center = (Vector2)transform.position + _solidCollider.offset;
                Instantiate(reformVFXPrefab, center, Quaternion.identity);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(rewindVFXDuration), cancellationToken: ct);

            if (visualTransform != null) visualTransform.gameObject.SetActive(true);
            _solidCollider.enabled = true;
            _isTriggered = false;
        }

        /// <summary>
        /// Platformu anında varsayılan durumuna (sağlam ve görünür) geri getirir.
        /// </summary>
        public void ResetToDefault()
        {
            CancelBreakSequence();

            if (visualTransform != null)
            {
                visualTransform.localPosition = _originalVisualPosition;
                visualTransform.gameObject.SetActive(true);
            }

            if (_solidCollider != null)
            {
                _solidCollider.enabled = true;
            }
            _isTriggered = false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_solidCollider == null) _solidCollider = GetComponent<BoxCollider2D>();
            
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Vector2 boxCenter = (Vector2)transform.position + _solidCollider.offset + (Vector2.up * 0.05f);
            Vector2 boxSize = _solidCollider.bounds.size;
            boxSize.y += 0.05f;
            Gizmos.DrawWireCube(boxCenter, boxSize);
        }
#endif
    }
}