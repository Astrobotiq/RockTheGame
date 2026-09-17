using System;
using New_Scripts.Death;
using New_Scripts.Player;
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
        [SerializeField] private GameObject deathParticlePrefab;

        private PlayerController _playerController;

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

            if (deathParticlePrefab != null)
            {
                Instantiate(deathParticlePrefab, transform.position, Quaternion.identity);
            }

            gameObject.SetActive(false);
            OnDeath?.Invoke();
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