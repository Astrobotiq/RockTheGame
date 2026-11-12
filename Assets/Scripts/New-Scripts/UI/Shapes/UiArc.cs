using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace New_Scripts.UI.Shapes
{
    /// <summary>
    /// Dinamik daire/halka/yay/dilim için küçük uGUI bileşeni (mesh). Dolgu, başlangıç açısı ve iç yarıçap kodla
    /// değiştirilebilir (sayaç, kırılan stamina halkası, radyal pause dilimi). Statik şekiller için PNG kullan.
    /// Dış yarıçap RectTransform'un kısa kenarının yarısıdır. Renk: <see cref="Graphic.color"/>.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("RockTheGame/UI/Arc")]
    public class UiArc : MaskableGraphic
    {
        [Tooltip("0 = dolu daire/dilim, 1'e yakın = ince halka (dış yarıçapın oranı).")]
        [SerializeField, Range(0f, 0.99f)] private float innerRadiusRatio;
        [Tooltip("0 = hiçbir şey, 1 = tam daire.")]
        [SerializeField, Range(0f, 1f)] private float fillAmount = 1f;
        [Tooltip("Saat gibi: 0 = 12 yönü, pozitif saat yönünde.")]
        [SerializeField] private float startDegrees;
        [SerializeField] private bool counterClockwise;
        [SerializeField, Range(3, 256)] private int segmentsPerCircle = 64;
        [Tooltip("Kenar yumuşatma (ekran pikseli). Overlay canvas MSAA görmez; 0 = sert, tırtıklı kenar.")]
        [SerializeField, Range(0f, 4f)] private float featherPixels = 1f;

        private static readonly List<Vector2> Positions = new List<Vector2>();
        private static readonly List<float> Alphas = new List<float>();
        private static readonly List<int> Triangles = new List<int>();

        public float InnerRadiusRatio { get => innerRadiusRatio; set => Set(ref innerRadiusRatio, Mathf.Clamp(value, 0f, 0.99f)); }
        public float FillAmount { get => fillAmount; set => Set(ref fillAmount, Mathf.Clamp01(value)); }
        public float StartDegrees { get => startDegrees; set => Set(ref startDegrees, value); }
        public bool CounterClockwise { get => counterClockwise; set => Set(ref counterClockwise, value); }
        public float FeatherPixels { get => featherPixels; set => Set(ref featherPixels, Mathf.Max(0f, value)); }

        public UiArc()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void Reset()
        {
            base.Reset();
            raycastTarget = false; // dekor; seçimi Selectable yönetir
        }

        private void Set<T>(ref T field, T value)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect rect = GetPixelAdjustedRect();
            float outer = Mathf.Min(rect.width, rect.height) * 0.5f;
            float scale = canvas != null ? Mathf.Max(0.0001f, canvas.scaleFactor) : 1f;

            ArcMeshBuilder.Build(new ArcMeshBuilder.Settings
            {
                OuterRadius = outer,
                InnerRadius = outer * innerRadiusRatio,
                StartDegrees = startDegrees,
                SweepDegrees = 360f * fillAmount * (counterClockwise ? -1f : 1f),
                SegmentsPerCircle = segmentsPerCircle,
                Feather = featherPixels / scale
            }, Positions, Alphas, Triangles);

            Vector2 center = rect.center;
            Color32 baseColor = color;
            for (int i = 0; i < Positions.Count; i++)
            {
                var c = baseColor;
                c.a = (byte)Mathf.RoundToInt(baseColor.a * Alphas[i]);
                vh.AddVert(Positions[i] + center, c, Vector2.zero);
            }

            for (int i = 0; i < Triangles.Count; i += 3)
                vh.AddTriangle(Triangles[i], Triangles[i + 1], Triangles[i + 2]);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetVerticesDirty();
        }
#endif
    }
}
