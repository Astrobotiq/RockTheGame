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
    [RequireComponent(typeof(Collider2D))]
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

        [Tooltip("Atanırsa algılama (kutu yerine) bu collider'ın şekliyle yapılır: üçgen/daire pad için pedin biraz büyütülmüş tetik collider'ı. " +
                 "isTrigger olmalı ve oyuncu/zemin/kanca katmanlarının dışında bir katmanda durmalı. Boşsa yukarıdaki kutu kullanılır.")]
        [SerializeField] private Collider2D detectionCollider;

        [Header("Launch Direction")]
        [Tooltip("TransformUp: pad'in yukarı yönü. OutwardFromCenter: merkezden oyuncunun temas ettiği noktaya, dışa (daire).")]
        [SerializeField] private LaunchDirectionMode directionMode = LaunchDirectionMode.TransformUp;

        [Tooltip("OutwardFromCenter için merkez (yerel ofset).")]
        [SerializeField] private Vector2 outwardCenterOffset = Vector2.zero;

        [Tooltip("TransformUp modunda yön, pad'in yukarı yönünden bu kadar derece döndürülür (saat yönünün tersi pozitif). " +
                 "Dik üçgen köşe pedi için -45: hipotenüsün dışa bakan normali.")]
        [SerializeField] private float launchAngleOffset = 0f;

#if UNITY_EDITOR // sadece editor gizmo kullaniyor; player derlemesinde CS0414 uretiyordu
        [Header("Editor Trajectory Gizmo")]
        [Tooltip("Karakterin fizik ayarlarını barındıran ScriptableObject (Yörünge çizgisi çizimi için gereklidir).")]
        [SerializeField] private PlayerStatsSO playerStats;
        
        [Tooltip("Yörünge çizgisinin uzunluğu (adım sayısı).")]
        [SerializeField] private int trajectorySteps = 60;
        
        [Tooltip("Simülasyondaki her adımın zaman aralığı (saniye).")]
        [SerializeField] private float stepDeltaTime = 0.02f;
#endif

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

        [Tooltip("Her fırlatmada ayrıca bir nota çalar (zincir, ses sınırı ortak). Boşsa nota çalmaz.")]
        [SerializeField] private BounceNotePlayerSO noteSystem;

        [Header("Vibration")]
        [SerializeField] private PlatformVibrationSettingsSO vibrationSettings;

        [Header("Juicy Scale Properties")]
        [SerializeField] private float squashDuration = 0.05f;
        [SerializeField] private float squashAmountScaleY = 0.6f;
        [SerializeField] private float stretchAmountScaleX = 1.3f;
        [SerializeField] private float stretchDuration = 0.08f;
        [SerializeField] private float launchStretchAmountScaleY = 1.4f;
        [SerializeField] private float launchSquashAmountScaleX = 0.7f;
        [SerializeField] private float returnDuration = 0.15f;

        /// <summary>Bu pad'in son çaldığı nota (görsel/shader buna bağlanır). Hiç çalmadıysa default.</summary>
        public NoteInfo LastNote { get; private set; }

        /// <summary>Fırlatmada bir nota çalındığında tetiklenir (renk, enstrüman, frekans bilgisiyle).</summary>
        public event Action<NoteInfo> NotePlayed;

        /// <summary>Her fırlatmada tetiklenir (nota çalmasa bile); varsa <see cref="NotePlayed"/> bundan hemen önce gelir.</summary>
        public event Action Launched;

        private Collider2D _collider;

        /// <summary>Pad'in yukarı yönü, <c>launchAngleOffset</c> kadar döndürülmüş hali (TransformUp modunun yönü).</summary>
        private Vector2 LaunchUp => Quaternion.Euler(0f, 0f, launchAngleOffset) * transform.up;
        private ContactFilter2D _detectionFilter;
        private readonly Collider2D[] _detectionBuffer = new Collider2D[8];
        private float _cooldownTimer;
        private Vector3 _originalVisualScale;
        private CancellationTokenSource _cts;

        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _detectionFilter = new ContactFilter2D { useLayerMask = true, layerMask = playerLayer, useTriggers = true };
            
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

            Collider2D hit = DetectPlayerCollider();

            if (hit != null && hit.TryGetComponent(out PlayerController player))
            {
                LaunchPlayer(player, hit);
            }
        }

        /// <summary>
        /// Oyuncunun collider'ını bulur: <c>detectionCollider</c> atanmışsa onun şekliyle (üçgen/daire),
        /// değilse yerel kutuyla (rotasyon ve pozisyon hesaba katılarak).
        /// </summary>
        private Collider2D DetectPlayerCollider()
        {
            if (detectionCollider != null)
            {
                int count = detectionCollider.Overlap(_detectionFilter, _detectionBuffer);
                for (int i = 0; i < count; i++)
                {
                    if (_detectionBuffer[i] != null && _detectionBuffer[i].TryGetComponent(out PlayerController _))
                        return _detectionBuffer[i];
                }
                return null;
            }

            Vector2 boxCenter = transform.TransformPoint(detectionOffset);
            return Physics2D.OverlapBox(boxCenter, detectionSize, transform.eulerAngles.z, playerLayer);
        }

        private void LaunchPlayer(PlayerController player, Collider2D hit)
        {
            _cooldownTimer = triggerCooldown;

            if (vibrationSettings != null && HapticManager.Instance != null)
            {
                HapticManager.Instance.Vibrate(vibrationSettings.LaunchPadProfile);
            }

            // Fırlatma vektörünü hesapla: transform.up ya da (daire) merkezden temas noktasına, dışa
            Vector2 launchDirection = LaunchDirectionMath.Resolve(directionMode,
                transform.TransformPoint(outwardCenterOffset), hit.bounds.center, LaunchUp);
            Vector2 launchVelocity = launchDirection * launchSpeed;

            // 1. Oyuncunun hareket yeteneklerini yenile
            player.ResetDash();
            player.ResetSlingshot();
            player.RefillWallStamina();
            player.ResetWallSlideTime();
            if (player.ColorController != null)
            {
                player.ColorController.ResetAllColors();
            }

            // 2. Oyuncu hızını ata ve havaya geçir (NormalState, launch pad profili)
            player.Velocity = launchVelocity;
            player.TransitionToState(new NormalState(player, launchVelocity, AirEntry.LaunchPad()));

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

            // Nota: fiziksel 'thump' yukarıda anında çaldı, nota onun üstüne biner. Seçilen nota görsele de gider.
            if (noteSystem != null && noteSystem.TryPlay(out NoteInfo note))
            {
                LastNote = note;
                NotePlayed?.Invoke(note);
            }
            Launched?.Invoke();

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
            Vector2 boxCenter = transform.TransformPoint(detectionOffset);

            if (detectionCollider == null)
            {
                Gizmos.color = new Color(0f, 1f, 0f, 0.4f);

                // Gizmos çizerken rotasyonu uygulamak için matrisi ayarlayalım
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
                DrawTrajectoryGizmo(boxCenter, LaunchUp);
                return;
            }

            // Şekilli algılama (üçgen/daire): algılama collider'ının hattı
            Gizmos.color = new Color(0f, 1f, 0f, 0.6f);
            DrawColliderOutline(detectionCollider);

            Vector2 center = directionMode == LaunchDirectionMode.OutwardFromCenter
                ? (Vector2)transform.TransformPoint(outwardCenterOffset)
                : (Vector2)detectionCollider.bounds.center;

            if (directionMode == LaunchDirectionMode.OutwardFromCenter)
            {
                // Yön temas noktasına bağlı: yukarıdan ±60° arası beş olası yönün yörüngesi
                float radius = detectionCollider.bounds.extents.x;
                foreach (Vector2 dir in LaunchDirectionMath.FanDirections(LaunchUp, 60f, 5))
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawLine(center, center + dir * (radius + 0.4f));
                    DrawTrajectoryGizmo(center + dir * radius, dir);
                }
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(center, center + LaunchUp * 0.6f);
                DrawTrajectoryGizmo(center, LaunchUp);
            }
        }

        private static void DrawColliderOutline(Collider2D c)
        {
            switch (c)
            {
                case CircleCollider2D circle:
                {
                    Vector3 s = circle.transform.lossyScale;
                    Gizmos.DrawWireSphere(circle.bounds.center, circle.radius * Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y)));
                    break;
                }
                case PolygonCollider2D polygon:
                    for (int path = 0; path < polygon.pathCount; path++)
                    {
                        Vector2[] pts = polygon.GetPath(path);
                        for (int i = 0; i < pts.Length; i++)
                        {
                            Vector3 a = polygon.transform.TransformPoint(pts[i] + polygon.offset);
                            Vector3 b = polygon.transform.TransformPoint(pts[(i + 1) % pts.Length] + polygon.offset);
                            Gizmos.DrawLine(a, b);
                        }
                    }
                    break;
                default:
                    Gizmos.DrawWireCube(c.bounds.center, c.bounds.size);
                    break;
            }
        }

        private void DrawTrajectoryGizmo(Vector2 startPos, Vector2 direction)
        {
            if (playerStats == null) return;

            Gizmos.color = new Color(0f, 0.8f, 1f, 0.8f);
            Vector2 currentPos = startPos;
            Vector2 currentVel = direction.normalized * launchSpeed;

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
