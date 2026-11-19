using System;
using New_Scripts.Death;
using New_Scripts.Player;
using New_Scripts.Player.IFramePauseable;
using UnityEngine;

namespace New_Scripts.Death
{
    /// <summary>
    /// Karakterin yaşam döngüsünü yönetir, arayüz sözleşmesini yerine getirir ve ölüm anında dış sistemleri bilgilendirir.
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IKillable
    {
        public event Action OnDeath;

        [Header("VFX Settings")]
        [Tooltip("Eski ölüm partikülü. DeathBurst ayarı atanmışsa kullanılmaz.")]
        [SerializeField] private GameObject deathParticlePrefab;
        [Tooltip("Blok Çözülme ölüm efekti (ayar, hitstop, sarsıntı). Boşsa eski partikül çalışır.")]
        [SerializeField] private DeathBurstSettingsSO burstSettings;

        private PlayerController _playerController;
        private DeathBurst _burst;

        private void Awake()
        {
            _playerController = GetComponent<PlayerController>();
        }

        public void Kill()
        {
            if (_playerController != null && _playerController.VibrationSettings != null && HapticManager.Instance != null)
            {
                HapticManager.Instance.Vibrate(_playerController.VibrationSettings.Death);
            }

            if (burstSettings != null)
            {
                PlayBurst();
            }
            else if (deathParticlePrefab != null)
            {
                Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
            }

            gameObject.SetActive(false);
            OnDeath?.Invoke();
        }

        /// <summary>
        /// Blok Çözülme: gövde blokları saçılır, kısa hitstop ve kamera sarsıntısı. Oyuncu devre dışı kalmadan önce
        /// çağrılır (konum, hız ve gövde sprite'ı geçerli). Asit ölümü üstlendiyse kafayı asit bırakır.
        /// </summary>
        private void PlayBurst()
        {
            bool claimed = DeathBurst.ConsumeClaim(out SpriteRenderer behind);
            SpriteRenderer body = _playerController != null ? _playerController.SpriteRenderer : null;
            if (body == null) return;

            Vector2 velocity = _playerController.Velocity;
            if (_burst == null) _burst = DeathBurst.Create(burstSettings);
            _burst.Play(velocity, body, behind);

            // Asit dışı ölümde kafa fiziksel bir nesne olarak kalır (asitte AcidHeadHost bırakır).
            if (!claimed)
            {
                Vector2 inherit = Vector2.ClampMagnitude(velocity * burstSettings.velocityInherit, burstSettings.maxInheritSpeed);
                Vector2 launch = DeathBurstMath.HeadLaunch(burstSettings.spread, burstSettings.drag,
                    burstSettings.headThrowFactor, UnityEngine.Random.Range(0.9f, 2.2f), inherit);
                GroundHeadPool.Spawn(burstSettings, (Vector2)transform.position + Vector2.up * burstSettings.headStartOffsetY,
                    launch, _playerController, body.sortingLayerID, body.sortingOrder);
            }

            if (burstSettings.hitstopSeconds > 0f)
                HitStopEvents.RequestHitStop?.Invoke(burstSettings.hitstopSeconds);

            if (burstSettings.shakeForce > 0f && CameraController.Instance != null)
            {
                Vector2 dir = velocity.sqrMagnitude > 1e-4f ? velocity.normalized : Vector2.right;
                CameraController.Instance.ShakeImpulse(dir * burstSettings.shakeForce);
            }
        }
    }
}

#if UNITY_EDITOR
/// <summary>
/// EntityHealth sınıfı için Inspector arayüzünü özelleştirir ve test butonları sağlar.
/// </summary>
[UnityEditor.CustomEditor(typeof(PlayerHealth))]
public class EntityHealthEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PlayerHealth health = (PlayerHealth)target;

        if (GUILayout.Button("Test: Kill Entity"))
        {
            health.Kill();
        }
    }
}
#endif