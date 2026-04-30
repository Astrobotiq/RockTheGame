using System.Threading;
using Cysharp.Threading.Tasks;
using New_Scripts.LevelChange;
using Unity.Cinemachine;
using UnityEngine;

namespace New_Scripts.Player
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CameraController : MonoBehaviour, ICameraTransitionHandler
    {
        [Header("References")]
        [SerializeField] private CinemachineCamera virtualCamera;
        [SerializeField] private Rigidbody2D targetRigidbody;
        [SerializeField] private CinemachineConfiner2D confiner;
        [SerializeField] private Transform cameraFollowTarget;

        [Header("Zoom Settings")]
        [SerializeField] private float minOrthoSize = 8f;
        [SerializeField] private float maxOrthoSize = 16f;
        [SerializeField] private float minVelocity = 5f;
        [SerializeField] private float maxVelocity = 40f;
        [SerializeField] private float zoomLerpSpeed = 3f;

        private CinemachineImpulseSource impulseSource;
        private float targetOrthoSize;
        private bool isTransitioning;

        private void Awake()
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        private void Start()
        {
            targetOrthoSize = minOrthoSize;
            virtualCamera.Lens.OrthographicSize = targetOrthoSize;
        }

        private void OnEnable()
        {
            Player.PlayerController.OnHighImpact += TriggerShake;
        }

        private void OnDisable()
        {
            Player.PlayerController.OnHighImpact -= TriggerShake;
        }

        private void LateUpdate()
        {
            if (isTransitioning) return;

            cameraFollowTarget.position = targetRigidbody.position;
            HandleDynamicZoom();
        }

        private void HandleDynamicZoom()
        {
            float currentSpeed = targetRigidbody.linearVelocity.magnitude;
            float speedPercentage = Mathf.InverseLerp(minVelocity, maxVelocity, currentSpeed);
        
            targetOrthoSize = Mathf.Lerp(minOrthoSize, maxOrthoSize, speedPercentage);
            virtualCamera.Lens.OrthographicSize = Mathf.Lerp(virtualCamera.Lens.OrthographicSize, targetOrthoSize, Time.deltaTime * zoomLerpSpeed);
        }

        private void TriggerShake(Vector3 velocity)
        {
            impulseSource.GenerateImpulseWithVelocity(velocity);
        }
        
        public void PrepareForTransition()
        {
            isTransitioning = true;
        }
        
        private Vector2 CalculateConfinedPosition(Vector2 rawPosition, Collider2D bounds)
        {
            Bounds b = bounds.bounds;
            float orthoSize = virtualCamera.Lens.OrthographicSize;
            float aspect = Camera.main != null ? Camera.main.aspect : 16f / 9f;
        
            float halfHeight = orthoSize;
            float halfWidth = orthoSize * aspect;

            float minX = b.min.x + halfWidth;
            float maxX = b.max.x - halfWidth;
            float minY = b.min.y + halfHeight;
            float maxY = b.max.y - halfHeight;

            if (maxX < minX) minX = maxX = b.center.x;
            if (maxY < minY) minY = maxY = b.center.y;

            float clampedX = Mathf.Clamp(rawPosition.x, minX, maxX);
            float clampedY = Mathf.Clamp(rawPosition.y, minY, maxY);

            return new Vector2(clampedX, clampedY);
        }

        public async UniTask PanCameraToAsync(Vector2 targetPosition, Collider2D newBounds, float duration, CancellationToken token)
        {
            Vector2 startPosition = cameraFollowTarget.position;
            Vector2 clampedTargetPosition = CalculateConfinedPosition(targetPosition, newBounds);
        
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
                cameraFollowTarget.position = Vector2.Lerp(startPosition, clampedTargetPosition, t);
            
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            cameraFollowTarget.position = clampedTargetPosition;
        
            await UniTask.WaitForEndOfFrame(this, token);
        }

        public void FinalizeTransition(Collider2D newBounds)
        {
            confiner.BoundingShape2D = newBounds;
            confiner.InvalidateBoundingShapeCache();
            isTransitioning = false;
        }
    }
}