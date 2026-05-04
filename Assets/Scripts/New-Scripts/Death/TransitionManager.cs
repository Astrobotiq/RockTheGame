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

        private void Awake()
        {
            _mainCamera = Camera.main;
            _transitionMaterial = _transitionImage.material;
        }

        public async UniTask PlayCloseTransitionAsync(Vector3 worldPosition, CancellationToken token)
        {
            SetCenter(worldPosition);
            await AnimateRadiusAsync(_maxRadius, 0f, token);
        }

        public async UniTask PlayOpenTransitionAsync(Vector3 worldPosition, CancellationToken token)
        {
            SetCenter(worldPosition);
            await AnimateRadiusAsync(0f, _maxRadius, token);
        }

        private void SetCenter(Vector3 worldPosition)
        {
            if (_mainCamera == null) return;
        
            Vector3 viewportPos = _mainCamera.WorldToViewportPoint(worldPosition);
            _transitionMaterial.SetVector(CenterProperty, new Vector2(viewportPos.x, viewportPos.y));
        }

        private async UniTask AnimateRadiusAsync(float startRadius, float endRadius, CancellationToken token)
        {
            float elapsedTime = 0f;
        
            while (elapsedTime < _transitionDuration)
            {
                elapsedTime += Time.deltaTime;
                float currentRadius = Mathf.Lerp(startRadius, endRadius, elapsedTime / _transitionDuration);
                _transitionMaterial.SetFloat(RadiusProperty, currentRadius);
            
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
        
            _transitionMaterial.SetFloat(RadiusProperty, endRadius);
        }
    }
}