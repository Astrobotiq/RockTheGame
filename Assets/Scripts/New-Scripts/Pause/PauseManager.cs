using System;
using System.Collections.Generic;
using DG.Tweening;
using New_Scripts.Player;
using New_Scripts.Player.States;
using UnityEngine;
using UnityEngine.InputSystem;

namespace New_Scripts.Pause
{
    /// <summary>
    /// Oyunu duraklatır: <c>Time.timeScale = 0</c>, <c>AudioListener.pause</c>, titreşim kesilir.
    /// Start tuşu (<c>Pause</c> action'ı) duraklatır / devam ettirir. UI süre dururken de çalışmalı:
    /// pause menüsündeki tween'ler <c>SetUpdate(true)</c>, animator'lar Unscaled Time kullanmalı.
    /// Not: <c>PlayerController.IsPaused</c> hit-stop / respawn içindir, bununla karıştırma.
    /// </summary>
    public class PauseManager : MonoBehaviour
    {
        public static PauseManager Instance { get; private set; }

        /// <summary>Duraklıyken true. Yönetici yoksa false.</summary>
        public static bool IsPaused => Instance != null && Instance._state.IsPaused;

        /// <summary>
        /// Oyun mantığı bu karede işlememeli. Devam ettiği karede de true kalır: devam için basılan tuş
        /// (ör. A) aynı karede oyuncunun zıplama girdisi olarak okunmasın.
        /// </summary>
        public static bool BlocksGameplay => IsPaused || (Instance != null && Time.frameCount <= Instance._resumeFrame);

        /// <summary>Duraklama başladığında (true) ya da bittiğinde (false) tetiklenir.</summary>
        public static event Action<bool> PauseChanged;

        private static readonly HashSet<object> Blockers = new HashSet<object>();

        /// <summary>Sahip kaldırana kadar pause'u kapatır (çok oyunculu yarış gibi). Duraklıysa dokunmaz.</summary>
        public static void Block(object owner) => Blockers.Add(owner);

        public static void Unblock(object owner) => Blockers.Remove(owner);

        [SerializeField] private InputActionReference pauseAction;
        [SerializeField] private PlayerController player;
        [SerializeField] private PauseAudioSettingsSO audioSettings;

        private const string MusicFxTweenId = "PauseMusicFx";

        private readonly PauseState _state = new PauseState();
        private float _timeScaleBeforePause = 1f;
        private int _resumeFrame = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            _state.Changed += OnStateChanged;
        }

        private void OnEnable()
        {
            if (pauseAction != null) pauseAction.action.Enable();
        }

        private void OnDisable()
        {
            if (pauseAction != null) pauseAction.action.Disable();
            // Sahne değişirken ya da obje kapanırken süre durmuş kalmasın.
            if (Instance == this) _state.Resume();
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            _state.Changed -= OnStateChanged;
            Instance = null;
        }

        private void Update()
        {
            if (pauseAction != null && pauseAction.action.WasPressedThisFrame()) Toggle();
        }

        public void Toggle()
        {
            if (_state.IsPaused) _state.Resume();
            else Pause();
        }

        public void Pause()
        {
            bool allowed = CanPause();
            if (!allowed && !_state.IsPaused) Debug.Log($"[Pause] Şu an duraklatılamaz: {WhyBlocked()}");
            _state.TryPause(allowed);
        }

        public void Resume() => _state.Resume();

        private bool CanPause()
        {
            if (player == null) player = FindFirstObjectByType<PlayerController>();
            bool active = player != null && player.gameObject.activeInHierarchy;
            return PauseRules.CanPause(
                active,
                active && player.IsPaused,
                active && player.CurrentState is PlayerTransitionState,
                Blockers.Count > 0);
        }

        private string WhyBlocked()
        {
            if (Blockers.Count > 0) return "çok oyunculu oturum açık (Editor'de Geliştirici başlatıcısı açıksa kapat)";
            if (player == null || !player.gameObject.activeInHierarchy) return "oyuncu yok ya da ölü";
            if (player.IsPaused) return "respawn ya da hit-stop sürüyor";
            return "oda geçişi sürüyor";
        }

        private void OnStateChanged(bool paused)
        {
            if (paused)
            {
                _timeScaleBeforePause = Time.timeScale;
                Time.timeScale = 0f;
                AudioListener.pause = true;
                if (HapticManager.Instance != null) HapticManager.Instance.ResetHaptics();
            }
            else
            {
                Time.timeScale = _timeScaleBeforePause;
                AudioListener.pause = false;
                _resumeFrame = Time.frameCount;
            }

            FadeMusicEffect(paused);
            PauseChanged?.Invoke(paused);
        }

        /// <summary>Müzik pause'ta çalmaya devam eder; "uzaktan geliyor" efekti timeScale'den bağımsız tween'lenir.</summary>
        private void FadeMusicEffect(bool paused)
        {
            if (audioSettings == null || audioSettings.Mixer == null) return;

            var s = audioSettings;
            DOTween.Kill(MusicFxTweenId);
            Fade(s.CutoffParam, paused ? s.PausedCutoffHz : PauseAudioSettingsSO.NormalCutoffHz);
            Fade(s.ReverbRoomParam, paused ? s.PausedReverbRoomMb : PauseAudioSettingsSO.NormalReverbRoomMb);
            Fade(s.DryLevelParam, paused ? s.PausedDryLevelMb : PauseAudioSettingsSO.NormalDryLevelMb);

            void Fade(string param, float target)
            {
                s.Mixer.DOSetFloat(param, target, s.FadeSeconds).SetUpdate(true).SetId(MusicFxTweenId);
            }
        }
    }
}
