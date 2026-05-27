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
        [Header("References")] [SerializeField]
        private CinemachineCamera virtualCamera;

        [SerializeField] private Rigidbody2D targetRigidbody;
        [SerializeField] private CinemachineConfiner2D confiner;
        [SerializeField] private Transform cameraFollowTarget;

        [Header("Zoom Settings")] [SerializeField]
        private float minOrthoSize = 8f;

        [SerializeField] private float maxOrthoSize = 16f;
        [SerializeField] private float minVelocity = 5f;
        [SerializeField] private float maxVelocity = 40f;
        [SerializeField] private float zoomLerpSpeed = 3f;

        private CinemachineImpulseSource impulseSource;
        private float targetOrthoSize;
        private bool isTransitioning;

        private bool isZoomOverridden;
        private float currentOverrideSize;

#if UNITY_EDITOR
        private Vector2 debugStartPos;
        private Vector2 debugEndPos;
        private Bounds debugNewBounds;
        private float debugTargetSize;
        private bool isDrawingTransitionGizmos;
#endif

        private void Awake()
        {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        private void Start()
        {
            // targetOrthoSize = minOrthoSize;
            // virtualCamera.Lens.OrthographicSize = targetOrthoSize;
            //
            // if (targetRigidbody != null)
            // {
            //     cameraFollowTarget.position = targetRigidbody.position;
            //
            //     if (virtualCamera != null)
            //     {
            //         virtualCamera.PreviousStateIsValid = false;
            //         virtualCamera.transform.position = new Vector3(targetRigidbody.position.x,
            //             targetRigidbody.position.y, virtualCamera.transform.position.z);
            //         confiner.InvalidateBoundingShapeCache();
            //     }
            // }
        }

        private void OnEnable()
        {
            Player.PlayerController.OnHighImpact += TriggerShake;
            PlayerController.OnSlingshotLaunch += SnapToTarget;
        }

        private void OnDisable()
        {
            Player.PlayerController.OnHighImpact -= TriggerShake;
            PlayerController.OnSlingshotLaunch -= SnapToTarget;
        }

        private void LateUpdate()
        {
            if (isTransitioning) return;

            UpdateFollowTarget();
            ClampFollowTargetToBounds();
        }


        [Header("Follow Settings")]
        [SerializeField] private float followLerpSpeed = 8f;        // Normal takip hızı
        [SerializeField] private float lookAheadMultiplier = 0.15f; // 65 * 0.15 = ~9.75 birim öne bakış
        [SerializeField] private float lookAheadLerpSpeed = 10f;    // Offset'in tepki hızı

        private Vector2 _currentLookAheadOffset;

        private void UpdateFollowTarget()
        {
            Vector2 velocity = targetRigidbody.linearVelocity;
            float speed = velocity.magnitude;

            float dynamicMaxDistance = Mathf.Lerp(2f, 12f, speed / 70f);
            Vector2 rawLookAhead = Vector2.ClampMagnitude(velocity * lookAheadMultiplier, dynamicMaxDistance);

            _currentLookAheadOffset = Vector2.Lerp(
                _currentLookAheadOffset,
                rawLookAhead,
                Time.deltaTime * lookAheadLerpSpeed);

            Vector2 targetPos = (Vector2)targetRigidbody.position + _currentLookAheadOffset;

            float dynamicFollowSpeed = Mathf.Lerp(followLerpSpeed, followLerpSpeed * 4f, speed / 70f);

            cameraFollowTarget.position = Vector3.Lerp(
                cameraFollowTarget.position,
                targetPos,
                Time.deltaTime * dynamicFollowSpeed);
        }

        private void ClampFollowTargetToBounds()
        {
            if (confiner == null || confiner.BoundingShape2D == null) return;

            float currentSize = virtualCamera.Lens.OrthographicSize;
            Vector2 clamped = CalculateConfinedPosition(
                cameraFollowTarget.position,
                confiner.BoundingShape2D,
                currentSize);

            cameraFollowTarget.position = new Vector3(
                clamped.x, clamped.y,
                cameraFollowTarget.position.z);
        }

        public void SnapToTarget()
        {
            if (isTransitioning) return;
            cameraFollowTarget.position = targetRigidbody.position;
        }

        private void HandleStaticZoom()
        {
            virtualCamera.Lens.OrthographicSize = Mathf.Lerp(
                virtualCamera.Lens.OrthographicSize,
                currentOverrideSize,
                Time.deltaTime * zoomLerpSpeed);
        }

        private void HandleDynamicZoom()
        {
            float currentSpeed = targetRigidbody.linearVelocity.magnitude;
            float speedPercentage = Mathf.InverseLerp(minVelocity, maxVelocity, currentSpeed);

            targetOrthoSize = Mathf.Lerp(minOrthoSize, maxOrthoSize, speedPercentage);
            virtualCamera.Lens.OrthographicSize = Mathf.Lerp(virtualCamera.Lens.OrthographicSize, targetOrthoSize,
                Time.deltaTime * zoomLerpSpeed);
        }

        private void TriggerShake(Vector3 velocity)
        {
            impulseSource.GenerateImpulseWithVelocity(velocity);
        }

        private Vector2 transitionStartCameraPosition;

        public void PrepareForTransition()
        {
            isTransitioning = true;

            if (Camera.main != null)
            {
                transitionStartCameraPosition = (Vector2)Camera.main.transform.position;
            }
            else
            {
                transitionStartCameraPosition = (Vector2)virtualCamera.transform.position;
            }

            if (confiner != null) confiner.enabled = false;
        }

        [Header("Transition Settings")] [SerializeField]
        private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        public async UniTask PanAndZoomCameraAsync(Vector2 targetPosition, float targetSize, bool isOverridden,
            Collider2D newBounds, float duration, CancellationToken token)
        {
            Vector2 startPosition = transitionStartCameraPosition;
            cameraFollowTarget.position = startPosition;
            virtualCamera.PreviousStateIsValid = false;
            float startSize = virtualCamera.Lens.OrthographicSize;

            float actualTargetSize = isOverridden ? targetSize : minOrthoSize;
            Vector2 clampedTargetPosition = CalculateConfinedPosition(targetPosition, newBounds, actualTargetSize);

#if UNITY_EDITOR
            // Gizmo verilerini kaydet
            debugStartPos = startPosition;
            debugEndPos = clampedTargetPosition;
            debugNewBounds = newBounds.bounds;
            debugTargetSize = actualTargetSize;
            isDrawingTransitionGizmos = true;
#endif

            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float linearT = elapsedTime / duration;
                float t = transitionCurve.Evaluate(linearT);

                cameraFollowTarget.position = Vector2.Lerp(startPosition, clampedTargetPosition, t);
                virtualCamera.Lens.OrthographicSize = Mathf.Lerp(startSize, actualTargetSize, t);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            cameraFollowTarget.position = clampedTargetPosition;
            virtualCamera.Lens.OrthographicSize = actualTargetSize;

            await UniTask.WaitForEndOfFrame(this, token);
        }

        private Vector2 CalculateConfinedPosition(Vector2 rawPosition, Collider2D bounds, float targetSize)
        {
            Bounds b = bounds.bounds;
            float aspect = Camera.main != null ? Camera.main.aspect : 16f / 9f;

            float halfHeight = targetSize;
            float halfWidth = targetSize * aspect;

            float minX = b.min.x + halfWidth;
            float maxX = b.max.x - halfWidth;
            float minY = b.min.y + halfHeight;
            float maxY = b.max.y - halfHeight;

            if (maxX < minX) minX = maxX = b.center.x;
            if (maxY < minY) minY = maxY = b.center.y;

            return new Vector2(Mathf.Clamp(rawPosition.x, minX, maxX), Mathf.Clamp(rawPosition.y, minY, maxY));
        }

        public void FinalizeTransition(Collider2D newBounds, float targetSize, bool isOverridden)
        {
            confiner.BoundingShape2D = newBounds;
            confiner.InvalidateBoundingShapeCache();

            if (confiner != null)
                confiner.enabled = true;

            isZoomOverridden = isOverridden;
            currentOverrideSize = targetSize;

            cameraFollowTarget.position = targetRigidbody.position;
            virtualCamera.PreviousStateIsValid = false;

            isTransitioning = false;

#if UNITY_EDITOR
            isDrawingTransitionGizmos = false;
#endif
        }

        public void SnapToRoomBounds(Collider2D newBounds, Vector2 targetPosition, float targetSize, bool isOverridden)
        {
            confiner.BoundingShape2D = newBounds;
            confiner.InvalidateBoundingShapeCache();
            confiner.enabled = true;

            isZoomOverridden = isOverridden;
            currentOverrideSize = isOverridden ? targetSize : minOrthoSize;
            virtualCamera.Lens.OrthographicSize = currentOverrideSize;

            Vector2 confinedPos = CalculateConfinedPosition(targetPosition, newBounds, currentOverrideSize);

            cameraFollowTarget.position = confinedPos;
            virtualCamera.transform.position =
                new Vector3(confinedPos.x, confinedPos.y, virtualCamera.transform.position.z);

            virtualCamera.PreviousStateIsValid = false;

            if (virtualCamera != null)
            {
                virtualCamera.PreviousStateIsValid = false;
                virtualCamera.transform.position = new Vector3(targetRigidbody.position.x, targetRigidbody.position.y,
                    virtualCamera.transform.position.z);
                confiner.InvalidateBoundingShapeCache();
            }

            isTransitioning = false;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!isDrawingTransitionGizmos) return;

            // 1. Yeni odanın sınırlarını Sarı çiz
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(debugNewBounds.center, debugNewBounds.size);

            // 2. Başlangıç (Yeşil) ve Bitiş (Kırmızı) noktalarını çiz ve aralarını birleştir
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(debugStartPos, 0.5f);

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(debugEndPos, 0.5f);

            Gizmos.color = Color.white;
            Gizmos.DrawLine(debugStartPos, debugEndPos);

            // 3. Kameranın varacağı noktadaki (ClampedTargetPosition) tahmini kamera görüş alanını (Cyan) çiz
            float aspect = Camera.main != null ? Camera.main.aspect : 16f / 9f;
            Vector2 cameraViewSize = new Vector2(debugTargetSize * aspect * 2, debugTargetSize * 2);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(debugEndPos, cameraViewSize);
        }
#endif
    }
}