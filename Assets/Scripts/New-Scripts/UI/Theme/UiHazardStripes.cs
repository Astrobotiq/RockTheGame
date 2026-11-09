using UnityEngine;
using UnityEngine.UI;

namespace New_Scripts.UI.Theme
{
    /// <summary>
    /// Bir uGUI <see cref="Graphic"/>'ine (Image, <c>UiPolygon</c> ...) tehlike şeridi verir: sarı zemin, çapraz ink şeritler.
    /// Shader <c>RockTheGame/UI/Hazard</c> (<c>docs/ui-research/mashup-ui.html</c>'deki <c>hazardPath</c>). Şeritler yalnızca
    /// denge <see cref="UiBalance.HazardStripesFrom"/> ve üstündeyken çizilir; altında düz sarı kalır.
    /// Desen elemanın sol-alt köşesinden başlar ve eleman hareket ederse onunla gelir (<see cref="UiConcreteSurface"/> gibi).
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    [DisallowMultipleComponent]
    [AddComponentMenu("RockTheGame/UI/Hazard Stripes")]
    public sealed class UiHazardStripes : MonoBehaviour
    {
        private static readonly int StripesId = Shader.PropertyToID("_Stripes");
        private static readonly int ArtPixelId = Shader.PropertyToID("_ArtPixel");
        private static readonly int RectOriginId = Shader.PropertyToID("_RectOrigin");
        private static readonly int InkId = Shader.PropertyToID("_Ink");
        private static readonly int PeriodId = Shader.PropertyToID("_Period");
        private static readonly int StripeWidthId = Shader.PropertyToID("_StripeWidth");

        [SerializeField] private UiScreen screen = UiScreen.RoomCard;
        [Tooltip("Bir sanat pikselinin canvas birimi (480x270 ızgara 4x = 4).")]
        [SerializeField, Min(0.25f)] private float artPixel = 4f;
        [Tooltip("Şerit periyodu (sanat pikseli).")]
        [SerializeField, Min(2f)] private float period = 16f;
        [Tooltip("Şeritin genişliği (sanat pikseli); periyodun altında olmalı.")]
        [SerializeField, Min(1f)] private float stripeWidth = 8f;

        private Graphic _graphic;
        private Material _instance;

        public UiScreen Screen { get => screen; set { screen = value; Apply(); } }
        public Material Instance => _instance;

        /// <summary>Bu ekranın dengesinde şerit çizilir mi (denge ≥ 0,3).</summary>
        public static bool StripesVisible(float balance) => UiBalance.HasHazardStripes(balance);

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
            if (theme.HazardMaterial == null)
            {
                Debug.LogWarning("[UiHazard] UiTheme'de HazardMaterial atanmamış.", this);
                return;
            }

            if (_instance == null || _instance.shader != theme.HazardMaterial.shader)
            {
                ReleaseInstance();
                _instance = new Material(theme.HazardMaterial) { name = theme.HazardMaterial.name + " (instance)", hideFlags = HideFlags.HideAndDontSave };
            }

            _instance.SetFloat(StripesId, StripesVisible(theme.BalanceFor(screen)) ? 1f : 0f);
            _instance.SetFloat(ArtPixelId, artPixel);
            _instance.SetFloat(PeriodId, period);
            _instance.SetFloat(StripeWidthId, Mathf.Min(stripeWidth, period));
            _instance.SetColor(InkId, theme.Ink);
            UpdateOrigin();

            _graphic.color = theme.Yellow;
            _graphic.material = _instance;
            _graphic.SetMaterialDirty();
        }

        private void LateUpdate()
        {
            // Eleman hareket ederse desen onunla gelsin.
            if (_instance == null || !transform.hasChanged) return;
            transform.hasChanged = false;
            UpdateOrigin();
        }

        /// <summary>Sol-alt köşeyi kök canvas uzayına çevirip shader'a verir (mesh oraya göre gelir). Dönme/ölçek varsayılmaz.</summary>
        private void UpdateOrigin()
        {
            if (_graphic == null) _graphic = GetComponent<Graphic>();
            RectTransform rt = _graphic.rectTransform;
            Rect r = rt.rect;
            Vector3 corner = rt.TransformPoint(new Vector3(r.xMin, r.yMin, 0f));
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
