using System.Collections.Generic;
using New_Scripts.Death;
using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Manages a set of platforms, moving them from a starting boundary transform (leftBound) 
    /// to an ending boundary transform (rightBound). When a platform crosses the end bound,
    /// it wraps back to the start bound.
    /// </summary>
    public class WrappingPlatformManager : MonoBehaviour, IResettable
    {
        [Header("References")]
        [Tooltip("The platforms to manage and wrap. Make sure they have PlatformController components with no MovementStrategy assigned.")]
        [SerializeField] private List<PlatformController> platforms = new List<PlatformController>();

        [Tooltip("The starting boundary transform (X and Y coordinate).")]
        [SerializeField] private Transform leftBound;

        [Tooltip("The ending boundary transform (X and Y coordinate).")]
        [SerializeField] private Transform rightBound;

        [Header("Movement Settings")]
        [Tooltip("Minimum movement speed.")]
        [SerializeField] private float minSpeed = 3f;

        [Tooltip("Maximum movement speed.")]
        [SerializeField] private float maxSpeed = 3f;

        private float[] _speeds;
        private float[] _initialSpeeds;
        private Vector2[] _initialPositions;

        private void OnEnable()
        {
            if (LevelResetManager.Instance != null)
            {
                LevelResetManager.Instance.Register(this);
            }
        }

        private void OnDisable()
        {
            if (LevelResetManager.Instance != null)
            {
                LevelResetManager.Instance.Unregister(this);
            }
        }

        private void Start()
        {
            if (leftBound == null || rightBound == null)
            {
                Debug.LogError("leftBound and rightBound must be assigned on WrappingPlatformManager!", this);
                enabled = false;
                return;
            }

            _speeds = new float[platforms.Count];
            _initialSpeeds = new float[platforms.Count];
            _initialPositions = new Vector2[platforms.Count];

            for (int i = 0; i < platforms.Count; i++)
            {
                if (platforms[i] != null)
                {
                    _initialPositions[i] = platforms[i].transform.position;
                }
                _speeds[i] = Random.Range(minSpeed, maxSpeed);
                _initialSpeeds[i] = _speeds[i];
            }
        }

        private void FixedUpdate()
        {
            if (leftBound == null || rightBound == null) return;

            Vector2 leftPos = leftBound.position;
            Vector2 rightPos = rightBound.position;
            Vector2 pathVector = rightPos - leftPos;
            float pathLength = pathVector.magnitude;

            if (pathLength < 0.001f) return;

            Vector2 direction = pathVector / pathLength;

            for (int i = 0; i < platforms.Count; i++)
            {
                PlatformController platform = platforms[i];
                if (platform == null) continue;

                float speed = _speeds[i];
                Vector2 newPos = platform.Position + direction * (speed * Time.fixedDeltaTime);

                Vector2 offsetFromLeft = newPos - leftPos;
                float projectionDistance = Vector2.Dot(offsetFromLeft, direction);

                if (projectionDistance >= pathLength)
                {
                    // Calculate overflow so the movement is continuous and smooth
                    float overflow = projectionDistance - pathLength;
                    Vector2 wrappedPos = leftPos + direction * overflow;

                    // Assign a new speed for variety when wrapping
                    _speeds[i] = Random.Range(minSpeed, maxSpeed);

                    platform.TeleportTo(wrappedPos);
                }
                else
                {
                    platform.MoveTo(newPos);
                }
            }
        }

        /// <summary>
        /// Tüm platformları başlangıç pozisyonlarına döndürür ve hızlarını sıfırlar.
        /// </summary>
        public void ResetToDefault()
        {
            if (_initialPositions == null) return;

            for (int i = 0; i < platforms.Count; i++)
            {
                if (platforms[i] != null && i < _initialPositions.Length)
                {
                    platforms[i].TeleportTo(_initialPositions[i]);
                }
                if (_speeds != null && _initialSpeeds != null && i < _speeds.Length)
                {
                    _speeds[i] = _initialSpeeds[i];
                }
            }
        }
    }
}
