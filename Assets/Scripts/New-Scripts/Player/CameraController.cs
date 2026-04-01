using Unity.Cinemachine;
using UnityEngine;

namespace New_Scripts.Player
{
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class CameraController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CinemachineCamera virtualCamera;
        [SerializeField] private Rigidbody2D targetRigidbody;

        [Header("Zoom Settings")]
        [SerializeField] private float minOrthoSize = 8f;
        [SerializeField] private float maxOrthoSize = 16f;
        [SerializeField] private float minVelocity = 5f;
        [SerializeField] private float maxVelocity = 40f;
        [SerializeField] private float zoomLerpSpeed = 3f;

        private CinemachineImpulseSource impulseSource;
        private float targetOrthoSize;

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
    }
}


namespace New_Scripts.Visuals
{
}