using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace New_Scripts.Player.Visual
{
    public enum EyeInsideShape
    {
        Normal,
        Heart,
        Key
    }

    [System.Serializable]
    public class SingleEyeSetup
    {
        [Tooltip("The parent GameObject container of this eye (used for offset flipping if not flipping parent).")]
        public Transform eyeContainer;
        
        [Tooltip("SpriteRenderer for the background shape of this eye.")]
        public SpriteRenderer eyeBgRenderer;
        
        [Tooltip("SpriteRenderer for the inside of this eye (pupil, heart, key).")]
        public SpriteRenderer pupilRenderer;
        
        [Tooltip("SpriteRenderer for the blinking foreground layer of this eye.")]
        public SpriteRenderer blinkRenderer;

        [HideInInspector] public Vector3 defaultContainerLocalPos;
        [HideInInspector] public Vector3 defaultPupilLocalPosition;
    }

    /// <summary>
    /// Karakterin kollarından ve gövdesinden bağımsız çalışan, göz kırpma, yönlü bakış
    /// ve toplanabilir eşyalara göre şekil değiştirme özelliklerini barındıran göz sistemi.
    /// </summary>
    public class PlayerEyesController : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private SpriteRenderer mainSpriteRenderer;

        [Header("Eye Setups")]
        [SerializeField] private List<SingleEyeSetup> eyes = new List<SingleEyeSetup>();

        [Header("Flipping Settings")]
        [Tooltip("Eğer true ise, bu scriptin bağlı olduğu parent Transform (EyesSystem) pozisyon ve ölçek olarak flip edilir. (Önerilen)")]
        [SerializeField] private bool flipParentTransform = true;

        [Header("Blinking Settings")]
        [Tooltip("Gözün kapanması için sırayla oynatılacak 7 frame'li sprite dizisi.")]
        [SerializeField] private Sprite[] blinkFrames;
        [Tooltip("Kareler bittiğinde gözü ters sırada geri açsın mı? (Ping-Pong)")]
        [SerializeField] private bool pingPongBlink = true;
        [SerializeField] private float blinkFrameRate = 0.05f;
        [SerializeField] private float minBlinkDelay = 2.0f;
        [SerializeField] private float maxBlinkDelay = 6.0f;

        [Header("Inside Eye Settings (Look Direction)")]
        [Tooltip("Göz bebeğinin merkezden maksimum kayabileceği mesafe.")]
        [SerializeField] private float maxPupilOffset = 0.1f;
        [Tooltip("Karakterin hızının göz bebeği kaymasına olan etkisi.")]
        [SerializeField] private float velocitySensitivity = 0.01f;
        [Tooltip("Göz bebeğinin kayma yumuşaklığı.")]
        [SerializeField] private float pupilSmoothSpeed = 10f;

        [Header("Eye Inside Sprites (Collectible Shapes)")]
        [SerializeField] private Sprite normalPupilSprite;
        [SerializeField] private Sprite heartSprite;
        [SerializeField] private Sprite keySprite;
        [SerializeField] private float shapeDisplayDuration = 3.0f;

        [Header("Animation Sync")]
        [Tooltip("Animator altındaki, animasyon karelerine göre göz offsetini taşıyan boş GameObject.")]
        [SerializeField] private Transform animatedAnchor;

        private EyeInsideShape currentShape = EyeInsideShape.Normal;
        private float shapeTimer = 0f;
        private Vector2 currentPupilOffset;
        private Vector3 defaultParentLocalPos;
        private Vector3 defaultAnchorLocalPos;
        
        private CancellationTokenSource blinkCts;

        private void Awake()
        {
            if (playerController == null)
            {
                playerController = GetComponentInParent<PlayerController>() ?? FindObjectOfType<PlayerController>();
            }

            if (mainSpriteRenderer == null && playerController != null)
            {
                mainSpriteRenderer = playerController.GetComponentInChildren<SpriteRenderer>();
            }

            // Gözlerin varsayılan pozisyonlarını önbelleğe al
            defaultParentLocalPos = transform.localPosition;

            if (animatedAnchor != null)
            {
                defaultAnchorLocalPos = animatedAnchor.localPosition;
            }

            foreach (var eye in eyes)
            {
                if (eye == null) continue;

                if (eye.eyeContainer != null)
                {
                    eye.defaultContainerLocalPos = eye.eyeContainer.localPosition;
                }
                if (eye.pupilRenderer != null)
                {
                    eye.defaultPupilLocalPosition = eye.pupilRenderer.transform.localPosition;
                }
            }
        }

        private void OnEnable()
        {
            // UniTask göz kırpma döngüsünü başlat
            if (blinkCts == null)
            {
                blinkCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
                StartRandomBlinkAsync(blinkCts.Token).Forget();
            }
        }

        private void OnDisable()
        {
            // Oynayan UniTask'ı durdur ve gözleri açık duruma sıfırla (Karakter öldüğünde kapalı kalmasını önler)
            if (blinkCts != null)
            {
                blinkCts.Cancel();
                blinkCts.Dispose();
                blinkCts = null;
            }

            ResetBlinkState();
        }

        private void Start()
        {
            UpdatePupilSprites();
        }

        private void Update()
        {
            // Özel şekil sayacını güncelle
            if (currentShape != EyeInsideShape.Normal)
            {
                shapeTimer -= Time.deltaTime;
                if (shapeTimer <= 0f)
                {
                    SetEyeInsideShape(EyeInsideShape.Normal);
                }
            }

            UpdatePupilPositions();
        }

        private void LateUpdate()
        {
            HandleFlipping();
        }

        /// <summary>
        /// Gözün içindeki şekli (Normal, Kalp, Anahtar) değiştirir.
        /// </summary>
        public void SetEyeInsideShape(EyeInsideShape shape)
        {
            currentShape = shape;
            UpdatePupilSprites();

            if (shape != EyeInsideShape.Normal)
            {
                shapeTimer = shapeDisplayDuration;
            }
        }

        private void UpdatePupilSprites()
        {
            Sprite spriteToUse = normalPupilSprite;
            switch (currentShape)
            {
                case EyeInsideShape.Normal:
                    spriteToUse = normalPupilSprite;
                    break;
                case EyeInsideShape.Heart:
                    spriteToUse = heartSprite;
                    break;
                case EyeInsideShape.Key:
                    spriteToUse = keySprite;
                    break;
            }

            foreach (var eye in eyes)
            {
                if (eye != null && eye.pupilRenderer != null)
                {
                    eye.pupilRenderer.sprite = spriteToUse;
                }
            }
        }

        private void UpdatePupilPositions()
        {
            if (playerController == null) return;

            // Karakterin hareket hızına göre hedef kaymayı belirle
            Vector2 movementDir = playerController.Velocity;
            Vector2 targetOffset = Vector2.ClampMagnitude(movementDir * velocitySensitivity, maxPupilOffset);

            // Eğer karakter flip edilmişse, eyesContainer scale.x = -1 olacağı için 
            // yerel offset yönünü tersine çevirmeliyiz ki dünya koordinatlarında doğru yöne baksınlar.
            bool isFlipped = mainSpriteRenderer != null && mainSpriteRenderer.flipX;
            if (isFlipped)
            {
                targetOffset.x *= -1f;
            }

            currentPupilOffset = Vector2.Lerp(currentPupilOffset, targetOffset, Time.deltaTime * pupilSmoothSpeed);

            foreach (var eye in eyes)
            {
                if (eye != null && eye.pupilRenderer != null)
                {
                    eye.pupilRenderer.transform.localPosition = eye.defaultPupilLocalPosition + (Vector3)currentPupilOffset;
                }
            }
        }

        private void HandleFlipping()
        {
            if (mainSpriteRenderer == null) return;

            bool isFlipped = mainSpriteRenderer.flipX;

            // Animasyonlu anchor'dan Y offsetini hesapla
            float offsetY = 0f;
            if (animatedAnchor != null)
            {
                offsetY = animatedAnchor.localPosition.y - defaultAnchorLocalPos.y;
            }

            if (flipParentTransform)
            {
                // 1. Parent (bu objeyi) flip et: Varsayılan yerel X pozisyonunu tersine çevir, Y'ye animasyon offsetini ekle
                Vector3 targetPos = defaultParentLocalPos;
                targetPos.y += offsetY;
                if (isFlipped)
                {
                    targetPos.x = -defaultParentLocalPos.x;
                }
                transform.localPosition = targetPos;

                // 2. Ölçeği flip et: x bileşenini yönelime göre aynala
                Vector3 targetScale = Vector3.one;
                if (isFlipped)
                {
                    targetScale.x = -1f;
                }
                transform.localScale = targetScale;
            }
            else
            {
                // Alternatif: Alt gözleri ayrı ayrı flip et
                foreach (var eye in eyes)
                {
                    if (eye == null || eye.eyeContainer == null) continue;

                    Vector3 targetPos = eye.defaultContainerLocalPos;
                    targetPos.y += offsetY;
                    if (isFlipped)
                    {
                        targetPos.x = -eye.defaultContainerLocalPos.x;
                    }
                    eye.eyeContainer.localPosition = targetPos;

                    Vector3 targetScale = Vector3.one;
                    if (isFlipped)
                    {
                        targetScale.x = -1f;
                    }
                    eye.eyeContainer.localScale = targetScale;
                }
            }
        }

        private async UniTaskVoid StartRandomBlinkAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    float delay = Random.Range(minBlinkDelay, maxBlinkDelay);
                    await UniTask.Delay(System.TimeSpan.FromSeconds(delay), cancellationToken: token);
                    await PlayBlinkAnimationAsync(token);
                }
            }
            catch (System.OperationCanceledException)
            {
                // Cancellation is expected on destroy/disable
            }
        }

        private async UniTask PlayBlinkAnimationAsync(CancellationToken token)
        {
            if (blinkFrames == null || blinkFrames.Length == 0) return;

            // Blink renderer'larını aktif et
            foreach (var eye in eyes)
            {
                if (eye != null && eye.blinkRenderer != null)
                {
                    eye.blinkRenderer.enabled = true;
                }
            }

            try
            {
                // Kapanma (İleri sarma)
                for (int i = 0; i < blinkFrames.Length; i++)
                {
                    token.ThrowIfCancellationRequested();
                    SetBlinkSprites(blinkFrames[i]);
                    await UniTask.Delay(System.TimeSpan.FromSeconds(blinkFrameRate), cancellationToken: token);
                }

                // Kısa bir bekleme (tamamen kapalıyken)
                token.ThrowIfCancellationRequested();
                await UniTask.Delay(System.TimeSpan.FromSeconds(blinkFrameRate), cancellationToken: token);

                // Açılma (Ping-Pong ise geriye sarma)
                if (pingPongBlink)
                {
                    for (int i = blinkFrames.Length - 2; i >= 0; i--)
                    {
                        token.ThrowIfCancellationRequested();
                        SetBlinkSprites(blinkFrames[i]);
                        await UniTask.Delay(System.TimeSpan.FromSeconds(blinkFrameRate), cancellationToken: token);
                    }
                }
            }
            catch (System.OperationCanceledException)
            {
                // Will be cleaned up by finally or caller
            }
            finally
            {
                ResetBlinkState();
            }
        }

        private void ResetBlinkState()
        {
            foreach (var eye in eyes)
            {
                if (eye != null && eye.blinkRenderer != null)
                {
                    eye.blinkRenderer.sprite = null;
                    eye.blinkRenderer.enabled = false;
                }
            }
        }

        private void SetBlinkSprites(Sprite sprite)
        {
            foreach (var eye in eyes)
            {
                if (eye != null && eye.blinkRenderer != null)
                {
                    eye.blinkRenderer.sprite = sprite;
                }
            }
        }
    }
}
