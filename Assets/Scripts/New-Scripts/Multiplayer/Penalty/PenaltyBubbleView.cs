using UnityEngine;

namespace New_Scripts.Multiplayer.Penalty
{
    /// <summary>
    /// Ceza balonunun görseli (docs/multiplayer-ceza-balonu-plani.md → BB2, BB4). <c>Sprites/PenaltyBubble</c> materyaliyle
    /// halftone, yanardöner kenarlı, patlarken halka bırakan balon; materyal atanmamışsa (eksik ayar) düz bir daire çizer.
    /// <see cref="PenaltyHandler"/> ve rakibin hayaleti (<c>GhostView</c>) yalnız <see cref="Create"/>, <see cref="Follow"/>
    /// ve <see cref="Pop"/> çağırır.
    /// </summary>
    public sealed class PenaltyBubbleView : MonoBehaviour
    {
        public const string ObjectName = "[MP Penalty Bubble]";
        /// <summary>Shader'daki quad / balon çapı oranı (patlama halkasına yer kalsın); shader'ın <c>_QuadScale</c>'iyle aynı.</summary>
        public const float QuadScale = 1.4f;
        /// <summary>Gidiş yönünde en çok bu kadar uzar, dik yönde yarısı kadar kısalır.</summary>
        private const float PerpendicularShrink = 0.5f;

        private const int TextureSize = 128;
        private const float PopSecondsShader = 0.4f;
        private const float PopSecondsFallback = 0.14f;
        private const float PopGrowthFallback = 1.25f;
        private const int SortingOffset = 5; // oyuncunun önünde

        private static readonly int FlowId = Shader.PropertyToID("_Flow");
        private static readonly int PopId = Shader.PropertyToID("_Pop");
        private static readonly int AlphaId = Shader.PropertyToID("_Alpha");
        private static readonly int RotId = Shader.PropertyToID("_Rot");

        private static Sprite _circleSprite;
        private static Sprite _quadSprite;

        private SpriteRenderer _renderer;
        private MaterialPropertyBlock _block;
        private bool _shaded;
        private float _diameter;
        private float _alpha;
        private float _lastScale = 1f;
        private float _flow;
        private float _rotation;
        private float _popElapsed = -1f;
        private Color _baseColor;

        public bool IsPopping => _popElapsed >= 0f;

        /// <param name="material"><c>Sprites/PenaltyBubble</c> materyali; null ise düz daire.</param>
        /// <param name="alpha">1 = yerel oyuncunun balonu; rakibin hayaletindeki balon daha soluk çizilir.</param>
        public static PenaltyBubbleView Create(float radius, SpriteRenderer sortSource, Material material = null, float alpha = 1f)
        {
            var view = new GameObject(ObjectName).AddComponent<PenaltyBubbleView>();
            view._diameter = radius * 2f;
            view._alpha = alpha;
            view._renderer = view.gameObject.AddComponent<SpriteRenderer>();
            view._shaded = material != null;
            if (view._shaded)
            {
                view._renderer.sharedMaterial = material;
                view._renderer.sprite = GetQuadSprite();
                view._block = new MaterialPropertyBlock();
                view.ApplyBlock(0f);
            }
            else
            {
                view._renderer.sprite = GetCircleSprite();
                view._baseColor = new Color(0.78f, 0.93f, 1f, alpha);
                view._renderer.color = view._baseColor;
            }

            if (sortSource != null)
            {
                view._renderer.sortingLayerID = sortSource.sortingLayerID;
                view._renderer.sortingOrder = sortSource.sortingOrder + SortingOffset;
            }
            view.transform.localScale = Vector3.zero;
            return view;
        }

        /// <summary>Balonu konuma koyar; <paramref name="scale"/> 1 = tam boy (şişerken aşabilir). Esneme yok.</summary>
        public void Follow(Vector3 position, float scale) => Follow(position, scale, Vector2.right, 0f);

        /// <summary>
        /// Esneyerek: <paramref name="direction"/> gidiş yönü, <paramref name="stretch"/> 0–1'in altında küçük bir oran
        /// (<see cref="PenaltyBubbleMath.StretchAmount"/>); yönde uzar, dik yönde kısalır.
        /// </summary>
        public void Follow(Vector3 position, float scale, Vector2 direction, float stretch)
        {
            if (IsPopping) return;
            _lastScale = scale;
            transform.position = position;

            _rotation = direction.sqrMagnitude > 1e-6f ? Mathf.Atan2(direction.y, direction.x) : 0f;
            transform.rotation = Quaternion.Euler(0f, 0f, _rotation * Mathf.Rad2Deg);

            float size = _diameter * Mathf.Max(0f, scale) * (_shaded ? QuadScale : 1f);
            transform.localScale = new Vector3(size * (1f + stretch), size * (1f - stretch * PerpendicularShrink), 1f);
            if (_shaded) ApplyBlock(0f);
        }

        /// <summary>Patlar: kenar halkası dışa yayılıp solar.</summary>
        public void Pop() => _popElapsed = 0f;

        private void Update()
        {
            _flow += Time.deltaTime;
            if (!IsPopping)
            {
                if (_shaded) ApplyBlock(0f);
                return;
            }

            _popElapsed += Time.deltaTime;
            float seconds = _shaded ? PopSecondsShader : PopSecondsFallback;
            float u = Mathf.Clamp01(_popElapsed / seconds);

            if (_shaded)
            {
                ApplyBlock(u);
            }
            else
            {
                float size = _diameter * Mathf.Max(0f, _lastScale) * Mathf.Lerp(1f, PopGrowthFallback, u);
                transform.localScale = new Vector3(size, size, 1f);
                _renderer.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, _baseColor.a * (1f - u));
            }

            if (u >= 1f) Destroy(gameObject);
        }

        private void ApplyBlock(float pop)
        {
            _renderer.GetPropertyBlock(_block);
            _block.SetFloat(FlowId, _flow);
            _block.SetFloat(PopId, pop);
            _block.SetFloat(AlphaId, _alpha);
            _block.SetFloat(RotId, _rotation);
            _renderer.SetPropertyBlock(_block);
        }

        // Shader'lı balon: sprite dokusu kullanılmaz, yalnız tam dikdörtgen mesh (±0.5) gerekir.
        private static Sprite GetQuadSprite()
        {
            if (_quadSprite != null) return _quadSprite;
            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            var pixels = new Color32[16];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            _quadSprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f, 0, SpriteMeshType.FullRect);
            _quadSprite.hideFlags = HideFlags.HideAndDontSave;
            return _quadSprite;
        }

        // Yedek: dolgu hafif saydam, kenar parlak; 1 birim çaplı (ppu = doku boyu).
        private static Sprite GetCircleSprite()
        {
            if (_circleSprite != null) return _circleSprite;

            var texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
                hideFlags = HideFlags.HideAndDontSave,
            };
            float half = TextureSize * 0.5f;
            var pixels = new Color32[TextureSize * TextureSize];
            for (int y = 0; y < TextureSize; y++)
            for (int x = 0; x < TextureSize; x++)
            {
                float r = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(half, half)) / half;
                float edge = Mathf.Clamp01((1f - r) * half);           // dış kenar yumuşaklığı (≈1 piksel)
                float k = Mathf.Clamp01(Mathf.InverseLerp(0.86f, 0.95f, r));
                float rim = k * k * (3f - 2f * k);                      // parlak halka
                float alpha = edge * Mathf.Lerp(0.16f, 0.9f, rim);
                pixels[y * TextureSize + x] = new Color(1f, 1f, 1f, alpha);
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);

            _circleSprite = Sprite.Create(texture, new Rect(0, 0, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), TextureSize);
            _circleSprite.hideFlags = HideFlags.HideAndDontSave;
            return _circleSprite;
        }
    }
}
