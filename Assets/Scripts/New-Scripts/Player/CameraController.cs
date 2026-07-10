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
        public static CameraController Instance { get; private set; }

        [Header("References")] [SerializeField]
        private CinemachineCamera virtualCamera;

        [SerializeField] private Rigidbody2D targetRigidbody;
        [SerializeField] private CinemachineConfiner2D confiner;
        [SerializeField] private Transform cameraFollowTarget;

        [Header("Zoom Settings")] [SerializeField]
        private float minOrthoSize = 8f;

        [SerializeField] private float zoomLerpSpeed = 3f;

        private CinemachineImpulseSource impulseSource;
        private float targetOrthoSize;
        private bool isTransitioning;

        private bool isZoomOverridden;
        private float currentOverrideSize;
        private float previousOrthoSize;

        [Header("Swing Focus Settings")]
        [SerializeField, Range(0f, 1f)] private float swingFocusWeight = 0.3f;

        private PlayerController playerController;

        private readonly System.Collections.Generic.List<ICameraOverrideProvider> activeOverrides = 
            new System.Collections.Generic.List<ICameraOverrideProvider>();

#if UNITY_EDITOR
        private Vector2 debugStartPos;
        private Vector2 debugEndPos;
        private Bounds debugNewBounds;
        private float debugTargetSize;
        private bool isDrawingTransitionGizmos;
#endif

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            impulseSource = GetComponent<CinemachineImpulseSource>();
            if (targetRigidbody != null)
            {
                playerController = targetRigidbody.GetComponent<PlayerController>();
            }

            if (virtualCamera != null)
            {
                previousOrthoSize = virtualCamera.Lens.OrthographicSize;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void RegisterOverride(ICameraOverrideProvider provider)
        {
            if (provider != null && !activeOverrides.Contains(provider))
            {
                activeOverrides.Add(provider);
            }
        }

        public void UnregisterOverride(ICameraOverrideProvider provider)
        {
            if (provider != null)
            {
                activeOverrides.Remove(provider);
            }
        }

        private ICameraOverrideProvider GetHighestPriorityOverride()
        {
            ICameraOverrideProvider highest = null;
            for (int i = 0; i < activeOverrides.Count; i++)
            {
                var provider = activeOverrides[i];
                if (provider != null && provider.IsActive)
                {
                    if (highest == null || provider.Priority > highest.Priority)
                    {
                        highest = provider;
                    }
                }
            }
            return highest;
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
            ICameraOverrideProvider activeOverride = GetHighestPriorityOverride();
            CameraOverrideSettings settings = activeOverride?.Settings;

            Vector2 velocity = targetRigidbody.linearVelocity;
            float speed = velocity.magnitude;

            // 1. Look-Ahead Calculation
            bool useLookAhead = settings == null || settings.focusType == CameraFocusType.Player || settings.focusType == CameraFocusType.PlayerAndTransform;
            Vector2 targetLookAhead = Vector2.zero;

            if (useLookAhead)
            {
                float activeLookAheadMultiplier = (settings != null && settings.overrideLookAhead)
                    ? settings.lookAheadMultiplier
                    : lookAheadMultiplier;

                float dynamicMaxDistance = Mathf.Lerp(2f, 12f, speed / 70f);
                targetLookAhead = Vector2.ClampMagnitude(velocity * activeLookAheadMultiplier, dynamicMaxDistance);
            }

            _currentLookAheadOffset = Vector2.Lerp(
                _currentLookAheadOffset,
                targetLookAhead,
                Time.deltaTime * lookAheadLerpSpeed);

            // 2. Focus Center Position Calculation
            Vector2 playerPos = targetRigidbody.position;
            Vector2 focusCenterPos = playerPos;

            if (settings != null && settings.focusType != CameraFocusType.Player)
            {
                if (settings.focusType == CameraFocusType.StaticPosition)
                {
                    focusCenterPos = settings.staticFocusPosition;
                }
                else if (settings.focusType == CameraFocusType.TargetTransform)
                {
                    focusCenterPos = settings.targetFocusTransform != null
                        ? (Vector2)settings.targetFocusTransform.position
                        : playerPos;
                }
                else if (settings.focusType == CameraFocusType.PlayerAndTransform)
                {
                    Vector2 targetPos2 = settings.targetFocusTransform != null
                        ? (Vector2)settings.targetFocusTransform.position
                        : playerPos;
                    focusCenterPos = Vector2.Lerp(playerPos, targetPos2, settings.focusWeight);
                }
            }
            else
            {
                // Default player follow with swing focus helper
                if (playerController != null)
                {
                    bool hasLeft = playerController.LeftAnchor.HasValue;
                    bool hasRight = playerController.RightAnchor.HasValue;

                    if (hasLeft && hasRight)
                    {
                        Vector2 anchorMidpoint = (playerController.LeftAnchor.Value + playerController.RightAnchor.Value) * 0.5f;
                        focusCenterPos = Vector2.Lerp(playerPos, anchorMidpoint, swingFocusWeight);
                    }
                    else if (hasLeft)
                    {
                        focusCenterPos = Vector2.Lerp(playerPos, playerController.LeftAnchor.Value, swingFocusWeight);
                    }
                    else if (hasRight)
                    {
                        focusCenterPos = Vector2.Lerp(playerPos, playerController.RightAnchor.Value, swingFocusWeight);
                    }
                }
            }

            Vector2 targetPos = focusCenterPos + _currentLookAheadOffset;

            // 3. Follow Speed Calculation
            float dynamicFollowSpeed = (settings != null && settings.overrideFollowSpeed)
                ? settings.followLerpSpeed
                : Mathf.Lerp(followLerpSpeed, followLerpSpeed * 4f, speed / 70f);

            cameraFollowTarget.position = Vector3.Lerp(
                cameraFollowTarget.position,
                targetPos,
                Time.deltaTime * dynamicFollowSpeed);

            // 4. Zoom (Orthographic Size) Calculation
            if (virtualCamera != null)
            {
                float normalOrthoSize = (settings != null && settings.overrideZoom)
                    ? settings.cameraSize
                    : (isZoomOverridden ? currentOverrideSize : minOrthoSize);

                float currentSize = virtualCamera.Lens.OrthographicSize;
                float newSize = Mathf.Lerp(currentSize, normalOrthoSize, Time.deltaTime * zoomLerpSpeed);
                virtualCamera.Lens.OrthographicSize = newSize;

                if (confiner != null && Mathf.Abs(newSize - previousOrthoSize) > 0.001f)
                {
                    confiner.InvalidateLensCache();
                }
                previousOrthoSize = newSize;
            }
        }

        private void ClampFollowTargetToBounds()
        {
            if (confiner == null || confiner.BoundingShape2D == null) return;

            Vector2 currentPos = cameraFollowTarget.position;
            Vector2 clamped = confiner.BoundingShape2D.ClosestPoint(currentPos);

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

        private void TriggerShake(Vector3 velocity)
        {
            impulseSource.GenerateImpulseWithVelocity(velocity);
        }

        private Vector2 transitionStartCameraPosition;

        public void PrepareForTransition()
        {
            isTransitioning = true;
            activeOverrides.Clear();
            isZoomOverridden = false;

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