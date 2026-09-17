using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace New_Scripts.Player
{
    /// <summary>
    /// Gamepad titreşimlerini (rumble/haptics) UniTask kullanarak yöneten Singleton sınıfı.
    /// Tek seferlik (One-Shot) ve sürekli (Continuous) titreşimleri destekler.
    /// </summary>
    public class HapticManager : MonoBehaviour
    {
        public static HapticManager Instance { get; private set; }

        private CancellationTokenSource _vibrationCts;
        private bool _isVibratingContinuous;
        private ContinuousRumbleProfile _continuousProfile;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                ResetHaptics();
                Instance = null;
            }
        }

        private void OnDisable()
        {
            ResetHaptics();
        }

        private void OnApplicationQuit()
        {
            ResetHaptics();
        }

        /// <summary>
        /// Belirli bir süre boyunca gamepad'i titretir.
        /// </summary>
        public void Vibrate(RumbleProfile profile)
        {
            if (profile.duration <= 0f) return;

            // Devam eden tek seferlik titreşimi iptal et
            CancelActiveVibration();

            _vibrationCts = new CancellationTokenSource();
            VibrateAsync(profile, _vibrationCts.Token).Forget();
        }

        private async UniTaskVoid VibrateAsync(RumbleProfile profile, CancellationToken token)
        {
            try
            {
                float elapsed = 0f;
                float duration = profile.duration;

                while (elapsed < duration)
                {
                    float t = elapsed / duration;
                    float leftSpeed = profile.leftMotorCurve != null && profile.leftMotorCurve.length > 0
                        ? profile.leftMotorCurve.Evaluate(t)
                        : 0f;
                    float rightSpeed = profile.rightMotorCurve != null && profile.rightMotorCurve.length > 0
                        ? profile.rightMotorCurve.Evaluate(t)
                        : 0f;

                    SetMotorSpeeds(Mathf.Clamp01(leftSpeed), Mathf.Clamp01(rightSpeed));

                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                    elapsed += Time.deltaTime;
                }
                
                // Tek seferlik bittiğinde aktif bir sürekli titreşim varsa ona geri dön, yoksa sıfırla.
                if (_isVibratingContinuous)
                {
                    SetMotorSpeeds(_continuousProfile.leftMotorSpeed, _continuousProfile.rightMotorSpeed);
                }
                else
                {
                    ResetHaptics();
                }
            }
            catch (OperationCanceledException)
            {
                // Yeni bir titreşim başladığı için iptal edildi. Motorları sıfırlama, yeni görev devralacak.
            }
            finally
            {
                // Eğer bu görev tamamlandıysa ve iptal edilmediyse CTS'i temizle
                if (_vibrationCts != null && _vibrationCts.Token == token)
                {
                    _vibrationCts.Dispose();
                    _vibrationCts = null;
                }
            }
        }

        /// <summary>
        /// Sürekli bir titreşim başlatır (örn: kayma, tırmanma, sallanan zemin).
        /// </summary>
        public void StartContinuousVibration(ContinuousRumbleProfile profile)
        {
            _isVibratingContinuous = true;
            _continuousProfile = profile;

            // Eğer şu an tek seferlik yüksek öncelikli bir titreşim yoksa hemen uygula
            if (_vibrationCts == null)
            {
                SetMotorSpeeds(profile.leftMotorSpeed, profile.rightMotorSpeed);
            }
        }

        /// <summary>
        /// Sürekli titreşimi durdurur.
        /// </summary>
        public void StopContinuousVibration()
        {
            _isVibratingContinuous = false;
            _continuousProfile = default;

            // Eğer tek seferlik titreşim çalışmıyorsa hemen motorları kapat
            if (_vibrationCts == null)
            {
                ResetHaptics();
            }
        }

        /// <summary>
        /// Tüm titreşimleri anında keser ve sıfırlar.
        /// </summary>
        public void ResetHaptics()
        {
            CancelActiveVibration();
            _isVibratingContinuous = false;
            _continuousProfile = default;

            Gamepad.current?.ResetHaptics();
        }

        private void CancelActiveVibration()
        {
            if (_vibrationCts != null)
            {
                _vibrationCts.Cancel();
                _vibrationCts.Dispose();
                _vibrationCts = null;
            }
        }

        private void SetMotorSpeeds(float leftMotorSpeed, float rightMotorSpeed)
        {
            Gamepad.current?.SetMotorSpeeds(leftMotorSpeed, rightMotorSpeed);
        }
    }
}
