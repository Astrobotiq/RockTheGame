using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Player'ın dokunduğu zaman öleceği ve asit shader'ına sahip asit tehlikesi bileşeni.
    /// Obje ölçeklendiğinde (Scale veya Size değiştiğinde) asidin piksel boyutlarının kare ve uniform kalmasını sağlar.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(DeathZone))]
    [ExecuteAlways]
    public class AcidHazard : MonoBehaviour
    {
        [Header("Material Settings")]
        [Tooltip("Asit materyali (Mat_PixelArtAcid).")]
        [SerializeField] private Material acidMaterial;
        
        [Tooltip("Oyunun Pixel Per Unit (PPU) değeri. Piksel boyutlarının tutarlılığı için kullanılır.")]
        [SerializeField] private float referencePPU = 16f;

        [Header("Setup")]
        [Tooltip("Otomatik olarak Collider bileşenini tetikleyici (Trigger) yapar.")]
        [SerializeField] private bool autoConfigureCollider = true;

        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;
        private MaterialPropertyBlock _propBlock;
        private bool _isInitialized;

        private void Awake()
        {
            ConfigureComponents();
            UpdateMaterialScale();
#if !UNITY_EDITOR
            _isInitialized = true;
#endif
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            UpdateMaterialScale();
#else
            // Seviyeler dinamik olarak enable/disable edildiğinde gereksiz hesaplamaları önlemek için
            if (!_isInitialized)
            {
                UpdateMaterialScale();
                _isInitialized = true;
            }
#endif
        }

#if UNITY_EDITOR
        private void Update()
        {
            // Editörde scale değişirse piksel oranını güncelle
            if (transform.hasChanged)
            {
                UpdateMaterialScale();
                transform.hasChanged = false;
            }
        }
#endif

        private void OnValidate()
        {
            // Editörde değerler değiştiğinde tetiklenir
            ConfigureComponents();
            UpdateMaterialScale();
        }

        private void ConfigureComponents()
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_collider == null) _collider = GetComponent<Collider2D>();

            // Asit materyalini otomatik ata
            if (acidMaterial != null && _spriteRenderer.sharedMaterial != acidMaterial)
            {
                _spriteRenderer.sharedMaterial = acidMaterial;
            }

            // Collider ayarlarını tetikleyici yap
            if (autoConfigureCollider && _collider != null)
            {
                _collider.isTrigger = true;
            }
        }

        /// <summary>
        /// Objenin dünya ölçeğine ve sprite ayarlarına göre shader'ın piksel ölçeğini günceller.
        /// Bu sayede pikseller her zaman kare ve uniform kalır.
        /// </summary>
        public void UpdateMaterialScale()
        {
            if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_spriteRenderer == null || _spriteRenderer.sprite == null) return;

            if (_propBlock == null)
            {
                _propBlock = new MaterialPropertyBlock();
            }

            _spriteRenderer.GetPropertyBlock(_propBlock);

            // Sprite'ın temel boyutlarını hesapla (Unit cinsinden)
            float baseWidth = _spriteRenderer.sprite.rect.width / _spriteRenderer.sprite.pixelsPerUnit;
            float baseHeight = _spriteRenderer.sprite.rect.height / _spriteRenderer.sprite.pixelsPerUnit;

            // Sliced veya Tiled modunda ise sprite'ın doğrudan boyutunu (size) kullan
            if (_spriteRenderer.drawMode == SpriteDrawMode.Tiled || _spriteRenderer.drawMode == SpriteDrawMode.Sliced)
            {
                baseWidth = _spriteRenderer.size.x;
                baseHeight = _spriteRenderer.size.y;
            }

            // Dünya ölçeğini hesaba kat
            Vector3 lossyScale = transform.lossyScale;
            float worldWidth = baseWidth * Mathf.Abs(lossyScale.x);
            float worldHeight = baseHeight * Mathf.Abs(lossyScale.y);

            // Toplam piksel sayısını hesapla
            float pixelX = worldWidth * referencePPU;
            float pixelY = worldHeight * referencePPU;

            // Shader'a gönder
            _propBlock.SetVector("_PixelScale", new Vector4(pixelX, pixelY, 0f, 0f));
            _spriteRenderer.SetPropertyBlock(_propBlock);
        }
    }
}
