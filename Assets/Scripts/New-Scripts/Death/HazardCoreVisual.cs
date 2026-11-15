using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Asit kabuğu (HazardAcid shader'lı SpriteRenderer) ile ortadaki çekirdek makineyi eşler.
    /// Çekirdek bu objenin child'ı; tehlike ne kadar gerilirse gerilsin çekirdek kare kalır ve
    /// boyutu kabuğun kısa kenarının _CoreFraction katıdır. Değer materyalden okunur, tek kaynak orası.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(SpriteRenderer))]
    public class HazardCoreVisual : MonoBehaviour
    {
        private static readonly int CoreFractionId = Shader.PropertyToID("_CoreFraction");

        [SerializeField] private Transform core;

        private SpriteRenderer _acid;

        private void OnEnable()
        {
            _acid = GetComponent<SpriteRenderer>();
            Apply();
        }

        private void LateUpdate()
        {
            if (!Application.isPlaying || transform.hasChanged) Apply();
        }

        private void Apply()
        {
            if (core == null || _acid == null) return;
            var mat = _acid.sharedMaterial;
            float fraction = mat != null && mat.HasProperty(CoreFractionId) ? mat.GetFloat(CoreFractionId) : 0.55f;
            Vector3 s = transform.lossyScale;
            Vector2 local = HazardCoreMath.CoreLocalScale(new Vector2(s.x, s.y), fraction);
            core.localScale = new Vector3(local.x, local.y, 1f);
            core.localPosition = Vector3.zero;
            transform.hasChanged = false;
        }
    }

    /// <summary>HazardCoreVisual'ın saf hesabı (EditMode'da test edilir).</summary>
    public static class HazardCoreMath
    {
        /// <summary>
        /// Ebeveynin dünya ölçeği verildiğinde, 1 birimlik çekirdek sprite'ının dünyada
        /// fraction * kısa kenar boyunda ve kare görünmesi için gereken yerel ölçek.
        /// </summary>
        public static Vector2 CoreLocalScale(Vector2 parentLossyScale, float fraction)
        {
            float sx = Mathf.Abs(parentLossyScale.x), sy = Mathf.Abs(parentLossyScale.y);
            if (sx < 1e-5f || sy < 1e-5f) return Vector2.one;
            float world = Mathf.Clamp01(fraction) * Mathf.Min(sx, sy);
            return new Vector2(world / sx, world / sy);
        }
    }
}
