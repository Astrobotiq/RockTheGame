using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace New_Scripts.UI.Shapes
{
    /// <summary>
    /// Dışbükey dolu çokgen (üçgen, mercek) için küçük uGUI bileşeni. Köşeler RectTransform'un pivotuna göre yerel koordinattır;
    /// Rect boyutu çizimi etkilemez. Overlay canvas MSAA görmez: kenarda alfası 0'a inen ince "tüy" şeridi eklenir (<see cref="UiArc"/> gibi).
    /// Geometri <see cref="PolygonMath"/>'te. Animasyon için mesh değil transform'u oynat.
    /// </summary>
    [RequireComponent(typeof(CanvasRenderer))]
    [AddComponentMenu("RockTheGame/UI/Polygon")]
    public class UiPolygon : MaskableGraphic
    {
        [SerializeField] private Vector2[] points = new Vector2[0];
        [Tooltip("Kenar yumuşatma (ekran pikseli); 0 = sert kenar.")]
        [SerializeField, Range(0f, 4f)] private float featherPixels = 1f;

        public float FeatherPixels { get => featherPixels; set { featherPixels = Mathf.Max(0f, value); SetVerticesDirty(); } }

        public UiPolygon()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void Reset()
        {
            base.Reset();
            raycastTarget = false;
        }

        public void SetPoints(IList<Vector2> value)
        {
            points = new Vector2[value.Count];
            for (int i = 0; i < points.Length; i++) points[i] = value[i];
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            int n = points != null ? points.Length : 0;
            if (n < 3) return;

            Color32 solid = color;
            Color32 clear = solid;
            clear.a = 0;

            vh.AddVert(PolygonMath.Centroid(points), solid, Vector2.zero);
            for (int i = 0; i < n; i++) vh.AddVert(points[i], solid, Vector2.zero);
            for (int i = 0; i < n; i++) vh.AddTriangle(0, 1 + i, 1 + (i + 1) % n);

            float scale = canvas != null ? Mathf.Max(0.0001f, canvas.scaleFactor) : 1f;
            if (featherPixels <= 0f) return;

            Vector2[] outer = PolygonMath.OffsetConvex(points, featherPixels / scale);
            for (int i = 0; i < n; i++) vh.AddVert(outer[i], clear, Vector2.zero);
            for (int i = 0; i < n; i++)
            {
                int a = 1 + i, b = 1 + (i + 1) % n, c = 1 + n + i, d = 1 + n + (i + 1) % n;
                vh.AddTriangle(a, c, d);
                vh.AddTriangle(a, d, b);
            }
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
