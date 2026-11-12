using UnityEngine;
using UnityEngine.UI;

namespace New_Scripts.UI.Theme
{
    public enum ConcreteTone
    {
        Wall,
        Slab,
        Mid
    }

    /// <summary>
    /// Bir uGUI <see cref="Graphic"/>'ine (Image, <c>UiArc</c> ...) kodla üretilen beton yüzeyi verir.
    /// Dengeyi, tonu ve materyali <see cref="UiThemeSO"/>'dan alır; shader <c>RockTheGame/UI/Concrete</c>.
    /// Beton: zemin rengi denge ile kâğıttan betona geçer. Boya (<see cref="paint"/>): renge dokunmaz, yalnızca benek ekler.
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    [DisallowMultipleComponent]
    [AddComponentMenu("RockTheGame/UI/Concrete Surface")]
    public sealed class UiConcreteSurface : MonoBehaviour
    {
        private static readonly int BalanceId = Shader.PropertyToID("_Balance");
        private static readonly int ConcreteId = Shader.PropertyToID("_Concrete");
        private static readonly int ArtPixelId = Shader.PropertyToID("_ArtPixel");
        private static readonly int RectOriginId = Shader.PropertyToID("_RectOrigin");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");

        [SerializeField] private UiScreen screen = UiScreen.MainMenu;
        [SerializeField] private ConcreteTone tone = ConcreteTone.Wall;
        [Tooltip("Boya: rengi Graphic.color belirler, yüzey yalnızca benek alır (kalıp çizgisi ve delik yok).")]
        [SerializeField] private bool paint;
        [Tooltip("Açıksa ekranın dengesi yerine aşağıdaki değer kullanılır (tek yüzeyi denemek için).")]
        [SerializeField] private bool useBalanceOverride;
        [SerializeField, Range(0f, 1f)] private float balanceOverride = 0.55f;
        [Tooltip("Bir sanat pikselinin canvas birimi (480x270 ızgara 4x = 4).")]
        [SerializeField, Min(0.25f)] private float artPixel = 4f;
        [SerializeField] private float seed;

        private Graphic _graphic;
        private Material _instance;

        public UiScreen Screen { get => screen; set { screen = value; Apply(); } }
        public Material Instance => _instance;

        /// <summary>
        /// Sahne kurarken (Editor, Play dışı) ayarlar; materyal örneği oluşturmaz, çünkü kayıtlı sahneye girmemeli.
        /// Yüzey Play'de <c>OnEnable</c>'da uygulanır.
        /// </summary>
        public void Configure(UiScreen forScreen, ConcreteTone forTone, bool asPaint)
        {
            screen = forScreen;
            tone = forTone;
            paint = asPaint;
        }

        private void OnEnable() => Apply();

        private void OnDestroy() => ReleaseInstance();

        private void OnRectTransformDimensionsChange()
        {
            if (_instance != null) UpdateOrigin();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (isActiveAndEnabled) Apply();
        }
#endif

        /// <summary>Tema değişince ya da ayarlar kodla değişince çağır.</summary>
        public void Apply()
        {
            if (_graphic == null) _graphic = GetComponent<Graphic>();
            var theme = UiThemeSO.Load();
            if (theme.ConcreteMaterial == null)
            {
                Debug.LogWarning("[UiConcrete] UiTheme'de ConcreteMaterial atanmamış.", this);
                return;
            }

            if (_instance == null || _instance.shader != theme.ConcreteMaterial.shader)
            {
                ReleaseInstance();
                _instance = new Material(theme.ConcreteMaterial) { name = theme.ConcreteMaterial.name + " (instance)", hideFlags = HideFlags.HideAndDontSave };
            }

            float balance = BalanceNow(theme);
            _instance.SetFloat(BalanceId, balance);
            _instance.SetFloat(ConcreteId, paint ? 0f : 1f);
            _instance.SetFloat(ArtPixelId, artPixel);
            _instance.SetFloat(SeedId, seed);
            UpdateOrigin();

            if (!paint) _graphic.color = SurfaceColor(theme, tone, balance);
            _graphic.material = _instance;
            _graphic.SetMaterialDirty();
        }

        private float BalanceNow(UiThemeSO theme) =>
            useBalanceOverride ? UiBalance.Clamp(balanceOverride) : theme.BalanceFor(screen);

        /// <summary>Beton zemin rengi: denge düşükken kâğıt, yükseldikçe seçilen beton tonu.</summary>
        public static Color SurfaceColor(UiThemeSO theme, ConcreteTone tone, float balance)
        {
            Color target = tone == ConcreteTone.Slab ? theme.ConcreteSlab : tone == ConcreteTone.Mid ? theme.ConcreteMid : theme.ConcreteWall;
            return Color.Lerp(theme.Paper, target, UiBalance.ConcreteBlend(balance));
        }

        private void LateUpdate()
        {
            // Eleman hareket ederse desen onunla gelsin (kayma/animasyon).
            if (_instance == null || !transform.hasChanged) return;
            transform.hasChanged = false;
            UpdateOrigin();
        }

        /// <summary>
        /// uGUI mesh'i shader'a kök canvas uzayında verir (yığınlama sırasında dönüştürülür), yerel uzayda değil;
        /// bu yüzden sol-üst köşe kök canvas uzayına çevrilir. Dönme/ölçek varsayılmaz (eksene hizalı).
        /// </summary>
        private void UpdateOrigin()
        {
            if (_graphic == null) _graphic = GetComponent<Graphic>();
            RectTransform rt = _graphic.rectTransform;
            Rect r = rt.rect;
            Vector3 corner = rt.TransformPoint(new Vector3(r.xMin, r.yMax, 0f));
            Canvas canvas = _graphic.canvas;
            if (canvas != null) corner = canvas.rootCanvas.transform.InverseTransformPoint(corner);
            _instance.SetVector(RectOriginId, new Vector4(corner.x, corner.y, 0f, 0f));
        }

        private void ReleaseInstance()
        {
            if (_instance == null) return;
            if (Application.isPlaying) Destroy(_instance);
            else DestroyImmediate(_instance);
            _instance = null;
        }
    }
}
