using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace New_Scripts.UI.Theme
{
    /// <summary>
    /// Yazıya mockup'taki gibi ekstrüzyon verir: ana yazının arkasına, ofset kaydırılmış ink renkli kopyalar dizilir
    /// (<c>ext()</c>, docs/ui-research/mashup-ui.html). Derinlik <see cref="UiThemeSO"/> dengesinden gelir; denge 0'da kopya yok.
    /// Kopyalar Play'de oluşur ve kaydedilmez. Metin ya da font koddan değişirse <see cref="Refresh"/> çağır.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    [DisallowMultipleComponent]
    [AddComponentMenu("RockTheGame/UI/Extruded Text")]
    public sealed class UiExtrudedText : MonoBehaviour
    {
        [SerializeField] private UiScreen screen = UiScreen.MainMenu;
        [Tooltip("Bir sanat pikselinin canvas birimi (480x270 ızgara 4x = 4).")]
        [SerializeField, Min(0.25f)] private float artPixel = 4f;

        private readonly List<GameObject> _copies = new List<GameObject>();

        public UiScreen Screen { get => screen; set { screen = value; Refresh(); } }

        /// <summary>Ekstrüzyonun <paramref name="step"/>. katmanının ana yazıya göre kayması (canvas birimi; aşağı-sağ).</summary>
        public static Vector2 OffsetFor(int step, float artPixel) => new Vector2(step * 0.8f, -step * 0.7f) * artPixel;

        private void OnEnable() => Refresh();

        private void OnDisable() => Clear();

        public void Refresh()
        {
            Clear();
            if (!Application.isPlaying || !isActiveAndEnabled || transform.parent == null) return;

            var source = GetComponent<TMP_Text>();
            var theme = UiThemeSO.Load();
            int depth = theme.ExtrusionDepthFor(screen);
            if (depth <= 0) return;

            var rect = (RectTransform)transform;
            int index = transform.GetSiblingIndex();
            // En uzak katman en arkada olsun: önce çizilir.
            for (int step = depth; step >= 1; step--)
            {
                var go = new GameObject($"{name} (extrude {step})", typeof(RectTransform)) { layer = gameObject.layer, hideFlags = HideFlags.DontSave };
                var rt = (RectTransform)go.transform;
                rt.SetParent(transform.parent, false);
                rt.anchorMin = rect.anchorMin;
                rt.anchorMax = rect.anchorMax;
                rt.pivot = rect.pivot;
                rt.sizeDelta = rect.sizeDelta;
                rt.localRotation = rect.localRotation;
                rt.localScale = rect.localScale;
                rt.anchoredPosition = rect.anchoredPosition + OffsetFor(step, artPixel);

                var text = go.AddComponent<TextMeshProUGUI>();
                text.font = source.font;
                text.fontSharedMaterial = source.fontSharedMaterial;
                text.fontSize = source.fontSize;
                text.fontStyle = source.fontStyle;
                text.alignment = source.alignment;
                text.characterSpacing = source.characterSpacing;
                text.text = source.text;
                text.color = theme.Ink;
                text.raycastTarget = false;
                rt.SetSiblingIndex(index++);
                _copies.Add(go);
            }
        }

        private void Clear()
        {
            for (int i = 0; i < _copies.Count; i++)
            {
                if (_copies[i] == null) continue;
                if (Application.isPlaying) Destroy(_copies[i]);
                else DestroyImmediate(_copies[i]);
            }

            _copies.Clear();
        }
    }
}
