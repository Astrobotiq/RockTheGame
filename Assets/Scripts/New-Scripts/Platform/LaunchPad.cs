using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using New_Scripts.Audio;
using New_Scripts.Player;
using New_Scripts.Player.States;
using UnityEngine;
using UnityEngine.Events;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Üzerine basıldığında oyuncuyu fırlatan fiziksel zemin/platform.
    /// Fırlatma yönünü objenin kendi 'transform.up' yönünden alır.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class LaunchPad : MonoBehaviour
    {
        [Header("Settings")]
        [Tooltip("Oyuncu katmanını seçin.")]
        [SerializeField] private LayerMask playerLayer;
        
        [Tooltip("Oyuncunun fırlatılma hızı.")]
        [SerializeField] private float launchSpeed = 15f;
        
        [Tooltip("Fırlatma tetiklenmesi sonrası tekrar tetiklenebilmesi için gereken süre.")]
        [SerializeField] private float triggerCooldown = 0.2f;

        [Header("Detection Settings")]
        [Tooltip("Algılama alanının yerel merkez ofseti.")]
        [SerializeField] private Vector2 detectionOffset = new Vector2(0f, 0.5f);
        
        [Tooltip("Algılama alanının yerel genişlik ve yüksekliği.")]
        [SerializeField] private Vector2 detectionSize = new Vector2(1.8f, 0.2f);

        [Header("Editor Trajectory Gizmo")]
        [Tooltip("Karakterin fizik ayarlarını barındıran ScriptableObject (Yörünge çizgisi çizimi için gereklidir).")]
        [SerializeField] private PlayerStatsSO playerStats;
        
        [Tooltip("Yörünge çizgisinin uzunluğu (adım sayısı).")]
        [SerializeField] private int trajectorySteps = 60;
        
        [Tooltip("Simülasyondaki her adımın zaman aralığı (saniye).")]
        [SerializeField] private float stepDeltaTime = 0.02f;

        [Header("Juicy Feedback")]
        [Tooltip("Ezilme/bükülme animasyonu uygulanacak görsel alt obje.")]
        [SerializeField] private Transform visualTransform;
        
        [Tooltip("Fırlatma anında doğacak parçacık efekti prefabı.")]
        [SerializeField] private ParticleSystem launchVFXPrefab;
        
        [Tooltip("Fırlatma tetiklendiğinde çalışacak olaylar (örn. ses efekti çalmak için).")]
        [SerializeField] private UnityEvent onLaunch;

        [Tooltip("Fırlatma animasyonu tamamlanıp orijinal boyuta dönüldüğünde çalışacak olaylar.")]
        [SerializeField] private UnityEvent onLaunchComplete;

        [Header("Audio")]
        [Tooltip("Ses oynatma olay kanalı.")]
        [SerializeField] private AudioCuePlayEventChannelSO sfxPlayChannel;

        [Tooltip("Fırlatma anında oynatılacak ses efekti.")]
        [SerializeField] private AudioCueSO launchSoundCue;

        [Header("Juicy Scale Properties")]
        [SerializeField] private float squashDuration = 0.05f;
        [SerializeField] private float squashAmountScaleY = 0.6f;
        [SerializeField] private float stretchAmountScaleX = 1.3f;
        [SerializeField] private float stretchDuration = 0.08f;
        [SerializeField] private float launchStretchAmountScaleY = 1.4f;
        [SerializeField] private float launchSquashAmountScaleX = 0.7f;
        [SerializeField] private float returnDuration = 0.15f;

        private BoxCollider2D _collider;
        private float _cooldownTimer;
        private Vector3 _originalVisualScale;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            
            if (visualTransform != null)
            {
                _originalVisualScale = visualTransform.localScale;
            }
        }

        private void OnDestroy()
        {
            CancelScaleTween();
        }

        private void OnDisable()
        {
            CancelScaleTween();
        }

        private void Update()
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }
        }

        private void FixedUpdate()
        {
            if (_cooldownTimer > 0f) return;

            // Yerel ofseti dünya koordinatlarına çeviriyoruz (rotasyon ve pozisyonu hesaba katarak)
            Vector2 boxCenter = transform.TransformPoint(detectionOffset);
            Vector2 overlapSize = detectionSize;
            float angle = transform.eulerAngles.z;

            Collider2D hit = Physics2D.OverlapBox(boxCenter, overlapSize, angle, playerLayer);

            if (hit != null && hit.TryGetComponent(out PlayerController player))
            {
                LaunchPlayer(player);
            }
        }

        private void LaunchPlayer(PlayerController player)
        {
            _cooldownTimer = triggerCooldown;

            // Fırlatma vektörünü hesapla (yerel transform.up yönü)
            Vector2 launchVelocity = (Vector2)transform.up * launchSpeed;

            // 1. Oyuncunun hareket yeteneklerini yenile
            player.ResetDash();
            player.ResetSlingshot();
            player.RefillWallStamina();
            player.ResetWallSlideTime();
            if (player.ColorController != null)
            {
                player.ColorController.ResetAllColors();
            }

            // 2. Oyuncu hızını ata ve AirborneState'e geçir
            player.Velocity = launchVelocity;
            player.TransitionToState(new AirborneState(
                player,
                launchVelocity,
                isJumping: true,
                bypassJumpGravity: true
            ));

            // 3. Efektleri tetikle
            if (launchVFXPrefab != null)
            {
                Instantiate(launchVFXPrefab, transform.position, transform.rotation);
            }

            // Ses çalma olayını tetikle
            if (sfxPlayChannel != null && launchSoundCue != null)
            {
                sfxPlayChannel.RaisePlayEvent(launchSoundCue, transform.position);
            }

            onLaunch?.Invoke();

            if (visualTransform != null)
            {
                CancelScaleTween();
                _cts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
                PlayScaleTweenAsync(_cts.Token).Forget();
            }
            else
            {
                onLaunchComplete?.Invoke();
            }
        }

        private async UniTaskVoid PlayScaleTweenAsync(CancellationToken ct)
        {
            if (visualTransform == null) return;

            Vector3 originalScale = _originalVisualScale;

            // 1. Ezilme Aşaması (Squash)
            float elapsed = 0f;
            while (elapsed < squashDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / squashDuration;
                float scaleY = Mathf.Lerp(1f, squashAmountScaleY, t);
                float scaleX = Mathf.Lerp(1f, stretchAmountScaleX, t);
                visualTransform.localScale = new Vector3(originalScale.x * scaleX, originalScale.y * scaleY, originalScale.z);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            // 2. Fırlama Esneme Aşaması (Launch Stretch)
            elapsed = 0f;
            while (elapsed < stretchDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / stretchDuration;
                float scaleY = Mathf.Lerp(squashAmountScaleY, launchStretchAmountScaleY, t);
                float scaleX = Mathf.Lerp(stretchAmountScaleX, launchSquashAmountScaleX, t);
                visualTransform.localScale = new Vector3(originalScale.x * scaleX, originalScale.y * scaleY, originalScale.z);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            // 3. Orijinal Boyuta Dönüş Aşaması (Return)
            elapsed = 0f;
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / returnDuration;
                float scaleY = Mathf.Lerp(launchStretchAmountScaleY, 1f, t);
                float scaleX = Mathf.Lerp(launchSquashAmountScaleX, 1f, t);
                visualTransform.localScale = new Vector3(originalScale.x * scaleX, originalScale.y * scaleY, originalScale.z);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            visualTransform.localScale = originalScale;
            onLaunchComplete?.Invoke();
        }

        private void CancelScaleTween()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
            if (visualTransform != null)
            {
                visualTransform.localScale = _originalVisualScale;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
            
            // Gizmos çizerken rotasyonu uygulamak için matrisi ayarlayalım
            Vector2 boxCenter = transform.TransformPoint(detectionOffset);
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
            Gizmos.matrix = rotationMatrix;
            
            // Merkez artık lokal sıfırda, çünkü matrisimizi boxCenter'a öteledik.
            // Sadece lokal döndürülmüş küpü çiziyoruz.
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(detectionSize.x, detectionSize.y, 0.1f));
            
            // Yönü gösteren bir ok çizelim
            Gizmos.color = Color.green;
            Gizmos.DrawLine(Vector3.zero, Vector3.up * 0.5f);
            Gizmos.DrawLine(Vector3.up * 0.5f, Vector3.up * 0.35f + Vector3.left * 0.1f);
            Gizmos.DrawLine(Vector3.up * 0.5f, Vector3.up * 0.35f + Vector3.right * 0.1f);

            // Matrisi sıfırlayarak dünya koordinatlarında çizim yapalım (yerçekimi aşağı doğru kalsın)
            Gizmos.matrix = Matrix4x4.identity;
            DrawTrajectoryGizmo(boxCenter);
        }

        private void DrawTrajectoryGizmo(Vector2 startPos)
        {
            if (playerStats == null) return;

            Gizmos.color = new Color(0f, 0.8f, 1f, 0.8f);
            Vector2 currentPos = startPos;
            Vector2 currentVel = (Vector2)transform.up * launchSpeed;

            float gravity = playerStats.Gravity;
            float fallGravityMult = playerStats.FallGravityMultiplier;
            float jumpEarlyGravityMult = playerStats.JumpEndEarlyGravityMultiplier;
            float terminalVel = playerStats.TerminalVelocity;
            float airDrag = playerStats.AirDrag;

            Vector2 lastPos = currentPos;

            for (int i = 0; i < trajectorySteps; i++)
            {
                // Dikey hıza göre yerçekimi çarpanını belirle
                float gravityMultiplier = 1f;
                if (currentVel.y < 0f)
                {
                    gravityMultiplier = fallGravityMult;
                }
                else if (currentVel.y > 0f)
                {
                    gravityMultiplier = jumpEarlyGravityMult;
                }

                // Yerçekimini uygula
                float gravityStep = gravity * gravityMultiplier * stepDeltaTime;
                currentVel.y += gravityStep;
                currentVel.y = Mathf.Max(currentVel.y, terminalVel);

                // Sürtünmeyi uygula (tuş girdisi olmadan)
                currentVel.x = Mathf.MoveTowards(currentVel.x, 0f, airDrag * stepDeltaTime);

                // Pozisyon güncelle
                currentPos += currentVel * stepDeltaTime;

                // Çizgi
                Gizmos.DrawLine(lastPos, currentPos);

                // Belirli adımlarda nokta koy
                if (i % 4 == 0)
                {
                    Gizmos.DrawSphere(currentPos, 0.05f);
                }

                lastPos = currentPos;
            }
        }
#endif
    }
}
