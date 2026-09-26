using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace New_Scripts.Audio
{
    /// <summary>
    /// Sahne geçişlerinde yok olmayan (persistent), ses havuzlaması (pooling),
    /// müzik/ortam çapraz geçişi (crossfading) ve ses ayarı kalıcılığını yöneten ana ses yöneticisi.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Event Channels - SFX")]
        [SerializeField] private AudioCuePlayEventChannelSO sfxPlayChannel;
        [SerializeField] private AudioCueStopEventChannelSO sfxStopChannel;

        [Header("Event Channels - Music")]
        [SerializeField] private AudioCuePlayEventChannelSO musicPlayChannel;
        [SerializeField] private AudioCueStopEventChannelSO musicStopChannel;

        [Header("Event Channels - Ambient")]
        [SerializeField] private AudioCuePlayEventChannelSO ambientPlayChannel;
        [SerializeField] private AudioCueStopEventChannelSO ambientStopChannel;

        [Header("Audio Mixer Settings")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string masterVolumeParam = "MasterVolume";
        [SerializeField] private string sfxVolumeParam = "SFXVolume";
        [SerializeField] private string musicVolumeParam = "MusicVolume";
        [SerializeField] private string ambientVolumeParam = "AmbientVolume";

        [Header("Pooling Settings")]
        [SerializeField] private int initialPoolSize = 10;
        [SerializeField] private int maxPoolSize = 30;

        // Dedicated sources for Music & Ambient crossfading
        private AudioSource _musicSourceA;
        private AudioSource _musicSourceB;
        private bool _isMusicSourceAActive = true;
        private CancellationTokenSource _musicFadeCts;

        private AudioSource _ambientSourceA;
        private AudioSource _ambientSourceB;
        private bool _isAmbientSourceAActive = true;
        private CancellationTokenSource _ambientFadeCts;

        // Pool collections
        private List<AudioSource> _sfxPool = new List<AudioSource>();
        private Dictionary<AudioCueSO, List<AudioSource>> _activeLoopingSFX = new Dictionary<AudioCueSO, List<AudioSource>>();

        // Volume Values
        public float MasterVolume { get; private set; } = 1.0f;
        public float SFXVolume { get; private set; } = 1.0f;
        public float MusicVolume { get; private set; } = 1.0f;
        public float AmbientVolume { get; private set; } = 1.0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializePool();
            InitializeDedicatedSources();
            LoadVolumeSettings();
        }

        private void Start()
        {
            ApplyAllVolumes();
        }

        private void OnEnable()
        {
            // SFX
            if (sfxPlayChannel != null) sfxPlayChannel.OnPlayRequested += HandlePlaySFX;
            if (sfxStopChannel != null) sfxStopChannel.OnStopRequested += HandleStopSFX;

            // Music
            if (musicPlayChannel != null) musicPlayChannel.OnPlayRequested += HandlePlayMusic;
            if (musicStopChannel != null) musicStopChannel.OnStopRequested += HandleStopMusic;

            // Ambient
            if (ambientPlayChannel != null) ambientPlayChannel.OnPlayRequested += HandlePlayAmbient;
            if (ambientStopChannel != null) ambientStopChannel.OnStopRequested += HandleStopAmbient;
        }

        private void OnDisable()
        {
            // SFX
            if (sfxPlayChannel != null) sfxPlayChannel.OnPlayRequested -= HandlePlaySFX;
            if (sfxStopChannel != null) sfxStopChannel.OnStopRequested -= HandleStopSFX;

            // Music
            if (musicPlayChannel != null) musicPlayChannel.OnPlayRequested -= HandlePlayMusic;
            if (musicStopChannel != null) musicStopChannel.OnStopRequested -= HandleStopMusic;

            // Ambient
            if (ambientPlayChannel != null) ambientPlayChannel.OnPlayRequested -= HandlePlayAmbient;
            if (ambientStopChannel != null) ambientStopChannel.OnStopRequested -= HandleStopAmbient;

            CancelFades();
        }

        private void OnDestroy()
        {
            CancelFades();
        }

        #region Pool Initialization

        private void InitializePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewPoolSource();
            }
        }

        private AudioSource CreateNewPoolSource()
        {
            GameObject obj = new GameObject($"SFX_Source_{_sfxPool.Count}");
            obj.transform.SetParent(transform);
            
            AudioSource source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            _sfxPool.Add(source);
            
            return source;
        }

        private void InitializeDedicatedSources()
        {
            // Music Sources
            GameObject musicObjA = new GameObject("Music_Source_A");
            musicObjA.transform.SetParent(transform);
            _musicSourceA = musicObjA.AddComponent<AudioSource>();
            _musicSourceA.playOnAwake = false;

            GameObject musicObjB = new GameObject("Music_Source_B");
            musicObjB.transform.SetParent(transform);
            _musicSourceB = musicObjB.AddComponent<AudioSource>();
            _musicSourceB.playOnAwake = false;

            // Ambient Sources
            GameObject ambientObjA = new GameObject("Ambient_Source_A");
            ambientObjA.transform.SetParent(transform);
            _ambientSourceA = ambientObjA.AddComponent<AudioSource>();
            _ambientSourceA.playOnAwake = false;

            GameObject ambientObjB = new GameObject("Ambient_Source_B");
            ambientObjB.transform.SetParent(transform);
            _ambientSourceB = ambientObjB.AddComponent<AudioSource>();
            _ambientSourceB.playOnAwake = false;
        }

        #endregion

        #region SFX Playback

        private void HandlePlaySFX(AudioCueSO cue, AudioCuePlayParams playParams)
        {
            AudioClip clip = cue.GetRandomClip();
            if (clip == null)
            {
                Debug.LogWarning($"[{name}] AudioCue '{cue.name}' içinde oynatılacak ses klibi bulunamadı!");
                return;
            }

            AudioSource source = GetAvailableSFXSource();
            if (source == null)
            {
                Debug.LogWarning($"[{name}] SFX çalınamadı. Kullanılabilir AudioSource yok ve maksimum havuz sınırına ({maxPoolSize}) ulaşıldı.");
                return;
            }

            // Configure Spatial Settings
            bool is3D = cue.Is3D || playParams.Is3D;
            ConfigureSourceSpatialSettings(source, cue, is3D, playParams);

            // Configure General Settings
            source.clip = clip;
            source.outputAudioMixerGroup = cue.MixerGroup;
            source.loop = cue.Loop;
            
            // Apply volume & pitch within limits, scaled by request parameters
            source.volume = Random.Range(cue.VolumeMin, cue.VolumeMax) * playParams.VolumeMultiplier;
            source.pitch = Random.Range(cue.PitchMin, cue.PitchMax) * playParams.PitchMultiplier;

            source.Play();

            if (cue.Loop)
            {
                if (!_activeLoopingSFX.TryGetValue(cue, out var sourcesList))
                {
                    sourcesList = new List<AudioSource>();
                    _activeLoopingSFX[cue] = sourcesList;
                }
                sourcesList.Add(source);
            }
            else
            {
                // One-shot monitors itself to recycle after playing
                MonitorOneShotPlayback(source, this.GetCancellationTokenOnDestroy()).Forget();
            }
        }

        private void HandleStopSFX(AudioCueSO cue)
        {
            if (_activeLoopingSFX.TryGetValue(cue, out var sourcesList))
            {
                foreach (var source in sourcesList)
                {
                    if (source != null)
                    {
                        source.Stop();
                        RecycleSource(source);
                    }
                }
                _activeLoopingSFX.Remove(cue);
            }
        }

        private AudioSource GetAvailableSFXSource()
        {
            // Find a source that isn't currently playing
            for (int i = 0; i < _sfxPool.Count; i++)
            {
                if (!_sfxPool[i].isPlaying && !IsLoopingSourceTracked(_sfxPool[i]))
                {
                    return _sfxPool[i];
                }
            }

            // If none are free, expand the pool if within limits
            if (_sfxPool.Count < maxPoolSize)
            {
                return CreateNewPoolSource();
            }

            // If we reached max limit, steal the oldest non-looping source
            for (int i = 0; i < _sfxPool.Count; i++)
            {
                if (!IsLoopingSourceTracked(_sfxPool[i]))
                {
                    _sfxPool[i].Stop();
                    RecycleSource(_sfxPool[i]);
                    return _sfxPool[i];
                }
            }

            return null;
        }

        private bool IsLoopingSourceTracked(AudioSource source)
        {
            foreach (var kvp in _activeLoopingSFX)
            {
                if (kvp.Value.Contains(source))
                {
                    return true;
                }
            }
            return false;
        }

        private void ConfigureSourceSpatialSettings(AudioSource source, AudioCueSO cue, bool is3D, AudioCuePlayParams playParams)
        {
            if (is3D)
            {
                source.spatialBlend = cue.SpatialBlend;
                source.minDistance = cue.MinDistance;
                source.maxDistance = cue.MaxDistance;
                source.rolloffMode = AudioRolloffMode.Logarithmic;

                source.transform.position = playParams.Position;
                if (playParams.Parent != null)
                {
                    source.transform.SetParent(playParams.Parent);
                }
                else
                {
                    source.transform.SetParent(null);
                }
            }
            else
            {
                source.spatialBlend = 0f;
                source.transform.SetParent(transform);
                source.transform.localPosition = Vector3.zero;
            }
        }

        private void RecycleSource(AudioSource source)
        {
            source.transform.SetParent(transform);
            source.transform.localPosition = Vector3.zero;
            source.clip = null;
        }

        private async UniTaskVoid MonitorOneShotPlayback(AudioSource source, CancellationToken token)
        {
            try
            {
                // Wait for the source to start playing (or short buffer frame)
                await UniTask.Yield(PlayerLoopTiming.Update, token);

                while (source != null && source.isPlaying)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, token);
                }
            }
            catch (OperationCanceledException)
            {
                // AudioManager or Game Object was destroyed
            }
            finally
            {
                if (source != null && !IsLoopingSourceTracked(source))
                {
                    RecycleSource(source);
                }
            }
        }

        #endregion

        #region Music Playback & Crossfading

        private void HandlePlayMusic(AudioCueSO cue, AudioCuePlayParams playParams)
        {
            _musicFadeCts?.Cancel();
            _musicFadeCts?.Dispose();
            _musicFadeCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            AudioSource activeSource = _isMusicSourceAActive ? _musicSourceA : _musicSourceB;
            AudioSource inactiveSource = _isMusicSourceAActive ? _musicSourceB : _musicSourceA;
            _isMusicSourceAActive = !_isMusicSourceAActive;

            AudioClip clip = cue.GetRandomClip();
            float targetVol = Random.Range(cue.VolumeMin, cue.VolumeMax) * playParams.VolumeMultiplier;
            float pitch = Random.Range(cue.PitchMin, cue.PitchMax) * playParams.PitchMultiplier;

            inactiveSource.outputAudioMixerGroup = cue.MixerGroup;
            inactiveSource.spatialBlend = 0f; // Music is typically 2D

            CrossfadeSourcesAsync(activeSource, inactiveSource, clip, targetVol, pitch, 1.5f, cue.Loop, _musicFadeCts.Token).Forget();
        }

        private void HandleStopMusic(AudioCueSO cue)
        {
            _musicFadeCts?.Cancel();
            _musicFadeCts?.Dispose();
            _musicFadeCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            AudioSource activeSource = _isMusicSourceAActive ? _musicSourceA : _musicSourceB;
            FadeOutSourceAsync(activeSource, 1.5f, _musicFadeCts.Token).Forget();
        }

        #endregion

        #region Ambient Playback & Crossfading

        private void HandlePlayAmbient(AudioCueSO cue, AudioCuePlayParams playParams)
        {
            _ambientFadeCts?.Cancel();
            _ambientFadeCts?.Dispose();
            _ambientFadeCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            AudioSource activeSource = _isAmbientSourceAActive ? _ambientSourceA : _ambientSourceB;
            AudioSource inactiveSource = _isAmbientSourceAActive ? _ambientSourceB : _ambientSourceA;
            _isAmbientSourceAActive = !_isAmbientSourceAActive;

            AudioClip clip = cue.GetRandomClip();
            float targetVol = Random.Range(cue.VolumeMin, cue.VolumeMax) * playParams.VolumeMultiplier;
            float pitch = Random.Range(cue.PitchMin, cue.PitchMax) * playParams.PitchMultiplier;

            inactiveSource.outputAudioMixerGroup = cue.MixerGroup;
            
            bool is3D = cue.Is3D || playParams.Is3D;
            ConfigureSourceSpatialSettings(inactiveSource, cue, is3D, playParams);

            CrossfadeSourcesAsync(activeSource, inactiveSource, clip, targetVol, pitch, 1.5f, cue.Loop, _ambientFadeCts.Token).Forget();
        }

        private void HandleStopAmbient(AudioCueSO cue)
        {
            _ambientFadeCts?.Cancel();
            _ambientFadeCts?.Dispose();
            _ambientFadeCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

            AudioSource activeSource = _isAmbientSourceAActive ? _ambientSourceA : _ambientSourceB;
            FadeOutSourceAsync(activeSource, 1.5f, _ambientFadeCts.Token).Forget();
        }

        #endregion

        #region Transition Helpers (UniTask)

        private async UniTask CrossfadeSourcesAsync(
            AudioSource activeSource,
            AudioSource inactiveSource,
            AudioClip newClip,
            float targetVolume,
            float targetPitch,
            float duration,
            bool loop,
            CancellationToken token)
        {
            inactiveSource.clip = newClip;
            inactiveSource.volume = 0f;
            inactiveSource.pitch = targetPitch;
            inactiveSource.loop = loop;
            inactiveSource.Play();

            float elapsed = 0f;
            float initialActiveVolume = activeSource.isPlaying ? activeSource.volume : 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                if (activeSource.isPlaying)
                {
                    activeSource.volume = Mathf.Lerp(initialActiveVolume, 0f, t);
                }
                inactiveSource.volume = Mathf.Lerp(0f, targetVolume, t);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            if (activeSource.isPlaying)
            {
                activeSource.Stop();
                activeSource.clip = null;
            }
            
            inactiveSource.volume = targetVolume;
        }

        private async UniTask FadeOutSourceAsync(AudioSource activeSource, float duration, CancellationToken token)
        {
            if (!activeSource.isPlaying) return;

            float elapsed = 0f;
            float initialVolume = activeSource.volume;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                activeSource.volume = Mathf.Lerp(initialVolume, 0f, t);
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }

            activeSource.Stop();
            activeSource.clip = null;
            activeSource.volume = 0f;
        }

        private void CancelFades()
        {
            _musicFadeCts?.Cancel();
            _musicFadeCts?.Dispose();
            _musicFadeCts = null;

            _ambientFadeCts?.Cancel();
            _ambientFadeCts?.Dispose();
            _ambientFadeCts = null;
        }

        #endregion

        #region Volume Control and Settings Persistence

        public void SetMasterVolume(float volume)
        {
            MasterVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("Audio_MasterVolume", MasterVolume);
            ApplyVolume(masterVolumeParam, MasterVolume);
        }

        public void SetSFXVolume(float volume)
        {
            SFXVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("Audio_SFXVolume", SFXVolume);
            ApplyVolume(sfxVolumeParam, SFXVolume);
        }

        public void SetMusicVolume(float volume)
        {
            MusicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("Audio_MusicVolume", MusicVolume);
            ApplyVolume(musicVolumeParam, MusicVolume);
        }

        public void SetAmbientVolume(float volume)
        {
            AmbientVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat("Audio_AmbientVolume", AmbientVolume);
            ApplyVolume(ambientVolumeParam, AmbientVolume);
        }

        private void ApplyVolume(string paramName, float volume)
        {
            if (audioMixer == null) return;
            
            // Standard logarithmic conversion for audio mixer parameters (0-1 linear mapped to decibels)
            float dB = volume > 0.0001f ? Mathf.Log10(volume) * 20f : -80f;
            
            bool success = audioMixer.SetFloat(paramName, dB);
            if (!success)
            {
                Debug.LogWarning($"[{name}] '{paramName}' paramatresi AudioMixer içinde bulunamadı ya da exposed edilmedi.");
            }
        }

        private void ApplyAllVolumes()
        {
            ApplyVolume(masterVolumeParam, MasterVolume);
            ApplyVolume(sfxVolumeParam, SFXVolume);
            ApplyVolume(musicVolumeParam, MusicVolume);
            ApplyVolume(ambientVolumeParam, AmbientVolume);
        }

        private void LoadVolumeSettings()
        {
            MasterVolume = PlayerPrefs.GetFloat("Audio_MasterVolume", 1.0f);
            SFXVolume = PlayerPrefs.GetFloat("Audio_SFXVolume", 1.0f);
            MusicVolume = PlayerPrefs.GetFloat("Audio_MusicVolume", 1.0f);
            AmbientVolume = PlayerPrefs.GetFloat("Audio_AmbientVolume", 1.0f);
        }

        #endregion
    }
}
