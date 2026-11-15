using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace New_Scripts.Death
{
    /// <summary>
    /// Ekran geçiş efektlerini UI Material üzerinden UniTask ile asenkron ve performanslı olarak yönetir.
    /// </summary>
    public class TransitionManager : MonoBehaviour
    {
        [SerializeField] private Image _transitionImage;
        [SerializeField] private float _transitionDuration = 0.5f;
        [SerializeField] private float _maxRadius = 1.5f;

        private Material _transitionMaterial;
        private Camera _mainCamera;
    
        private static readonly int CenterProperty = Shader.PropertyToID("_Center");
        private static readonly int RadiusProperty = Shader.PropertyToID("_Radius");
        private static readonly int EdgeSmoothingProperty = Shader.PropertyToID("_EdgeSmoothing");

        private void Awake()
        {
            _mainCamera = Camera.main;
            _transitionMaterial = _transitionImage.material;
        }

        public async UniTask PlayCloseTransitionAsync(Vector3 worldPosition, CancellationToken token)
        {
            await AnimateRadiusAsync(worldPosition, true, token);
        }

        public async UniTask PlayOpenTransitionAsync(Vector3 worldPosition, CancellationToken token)
        {
            await AnimateRadiusAsync(worldPosition, false, token);
        }

        /// <summary>Merkezi viewport'a çevirip materyale yazar; tam açık yarıçapı (en uzak köşe) döndürür.</summary>
        private float SetCenter(Vector3 worldPosition)
        {
            if (_mainCamera == null) return _maxRadius;

            Vector3 viewportPos = _mainCamera.WorldToViewportPoint(worldPosition);
            _transitionMaterial.SetVector(CenterProperty, new Vector2(viewportPos.x, viewportPos.y));

            // Merkez ekranın kenarına/dışına düşerse sabit _maxRadius karşı köşeyi açamaz (ekranın bir kısmı siyah kalır).
            float aspect = Screen.height > 0 ? (float)Screen.width / Screen.height : 16f / 9f;
            float edge = _transitionMaterial.HasProperty(EdgeSmoothingProperty) ? _transitionMaterial.GetFloat(EdgeSmoothingProperty) : 0.02f;
            return Mathf.Max(_maxRadius, IrisMath.RequiredRadius(new Vector2(viewportPos.x, viewportPos.y), aspect, edge));
        }

        /// <summary>
        /// Merkez her karede dünya konumunu izler: açılırken kamera hâlâ hareket ediyor olabilir (çerçeve/override
        /// kamerasından oyuncu kamerasına blend), merkezi bir kez hesaplamak iris'i yanlış yerden açıyordu.
        /// </summary>
        private async UniTask AnimateRadiusAsync(Vector3 worldPosition, bool closing, CancellationToken token)
        {
            float elapsedTime = 0f;
            float maxRadius = SetCenter(worldPosition);

            while (elapsedTime < _transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                maxRadius = SetCenter(worldPosition);
                float t = elapsedTime / _transitionDuration;
                float currentRadius = closing ? Mathf.Lerp(maxRadius, -1f, t) : Mathf.Lerp(-1f, maxRadius, t);
                _transitionMaterial.SetFloat(RadiusProperty, currentRadius);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            maxRadius = SetCenter(worldPosition);
            _transitionMaterial.SetFloat(RadiusProperty, closing ? -1f : maxRadius);
        }
    }
}