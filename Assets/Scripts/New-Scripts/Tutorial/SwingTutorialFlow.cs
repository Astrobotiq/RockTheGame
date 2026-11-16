using UnityEngine;

namespace New_Scripts.Tutorial
{
    /// <summary>Swing tutorial'ının adımları (docs/swing-tutorial-plani.md tablosu).</summary>
    public enum SwingTutorialStep
    {
        /// <summary>Hiçbir şey olmuyor; oyuncu havada ve node erişilebilir olunca başlar.</summary>
        Waiting,
        /// <summary>Zaman yavaşlıyor, yükseliş sürüyor. 1. ipucu (kolu çevir) girer.</summary>
        Slowing,
        /// <summary>Tepede bekliyor; oyuncu sağ stick ile node'u seçmeli.</summary>
        Aim,
        /// <summary>Nişan onaylandı; 2. ipucu (ateşle) görünür, oyuncu node'a tutunmalı.</summary>
        Fire,
        /// <summary>Bitti: zaman normale dönüyor; tutorial bu oturumda kapanır.</summary>
        Done
    }

    /// <summary>Akışın her karede aldığı girdiler (sahne bileşeni doldurur).</summary>
    public struct SwingTutorialInput
    {
        /// <summary><c>PlayerController.IsInNormalAir</c>.</summary>
        public bool IsAirborne;
        /// <summary>Sol ya da sağ anchor dolu.</summary>
        public bool HasAnyAnchor;
        /// <summary><c>NodeDetector</c> kurallarıyla (yarıçap, koni, engel) bu node erişilebilir.</summary>
        public bool NodeReachable;
        /// <summary>Öğretilen kolun stick'i bu node'u seçiyor (ölü bölgedeyse false).</summary>
        public bool AimedAtNode;
        /// <summary>Anchor'lardan biri bu node'a oturdu.</summary>
        public bool AnchoredToNode;
        public float VelocityY;
        public float UnscaledDt;
        /// <summary><c>PauseManager.BlocksGameplay</c>.</summary>
        public bool Paused;
    }

    /// <summary>
    /// Swing tutorial'ının saf mantığı: girdilerden adımı ve hedef <c>timeScale</c>'i üretir, Unity'ye dokunmaz.
    /// Tüm sayaçlar <see cref="SwingTutorialInput.UnscaledDt"/> ile ilerler; pause'da hiçbir şey ilerlemez.
    /// Akış oyuncuyu cezalandırmaz: beklenmeyen durumda (iniş, dash, başka node'a tutunma, menzil kaybı)
    /// zamanı 1'e bırakıp Waiting'e döner.
    /// <para>
    /// Kullanım: <see cref="IsControllingTime"/> true iken <see cref="TargetTimeScale"/> yazılır; false'a döndüğü
    /// karede de bir kez yazılır (o anda değer tam 1'dir).
    /// </para>
    /// </summary>
    public sealed class SwingTutorialFlow
    {
        private readonly SwingTutorialSettingsSO _settings;

        private float _rampIn;
        private float _rampOut;
        private float _rampOutFrom = 1f;
        private float _aimHeld;
        private float _aimLost;

        public SwingTutorialFlow(SwingTutorialSettingsSO settings)
        {
            _settings = settings;
        }

        public SwingTutorialStep Step { get; private set; } = SwingTutorialStep.Waiting;

        /// <summary>Bu karede <c>Time.timeScale</c>'e yazılması istenen değer.</summary>
        public float TargetTimeScale { get; private set; } = 1f;

        /// <summary>Akış zamanı sürüyor: yavaşlama, bekleme ya da çıkış rampası.</summary>
        public bool IsControllingTime =>
            Step == SwingTutorialStep.Slowing || Step == SwingTutorialStep.Aim || Step == SwingTutorialStep.Fire
            || (Step == SwingTutorialStep.Done && !RampOutFinished);

        private bool RampOutFinished =>
            _rampOutFrom >= 1f || _rampOut >= _settings.RampOutDuration;

        /// <summary>Ölümde çağrılır: bitmediyse baştan oynar, bittiyse kapalı kalır; zaman hemen 1.</summary>
        public void Reset()
        {
            _rampIn = 0f;
            _aimHeld = 0f;
            _aimLost = 0f;
            TargetTimeScale = 1f;
            if (Step == SwingTutorialStep.Done)
            {
                _rampOutFrom = 1f;
                _rampOut = 0f;
                return;
            }
            Step = SwingTutorialStep.Waiting;
        }

        public void Tick(in SwingTutorialInput input)
        {
            if (input.Paused) return;

            float dt = Mathf.Max(0f, input.UnscaledDt);

            switch (Step)
            {
                case SwingTutorialStep.Waiting:
                    if (input.AnchoredToNode)
                        FinishFlow();
                    else if (input.IsAirborne && !input.HasAnyAnchor && input.NodeReachable)
                        StartSlowing();
                    break;

                case SwingTutorialStep.Slowing:
                    if (input.AnchoredToNode) { FinishFlow(); break; }
                    if (ShouldAbort(input, requireReachable: false)) { Abort(); break; }

                    _rampIn += dt;
                    // Tepe kararı rampa bittikten sonra verilir: yürüyerek düşüşte de yavaşlama görünür.
                    if (input.VelocityY <= 0f && RampInProgress() >= 1f)
                    {
                        if (input.NodeReachable) EnterAim();
                        else Abort();
                    }
                    break;

                case SwingTutorialStep.Aim:
                    if (input.AnchoredToNode) { FinishFlow(); break; }
                    if (ShouldAbort(input, requireReachable: true)) { Abort(); break; }

                    _aimHeld = input.AimedAtNode ? _aimHeld + dt : 0f;
                    if (_aimHeld >= _settings.AimConfirmTime)
                    {
                        Step = SwingTutorialStep.Fire;
                        _aimLost = 0f;
                    }
                    break;

                case SwingTutorialStep.Fire:
                    if (input.AnchoredToNode) { FinishFlow(); break; }
                    if (ShouldAbort(input, requireReachable: true)) { Abort(); break; }

                    _aimLost = input.AimedAtNode ? 0f : _aimLost + dt;
                    if (_aimLost >= _settings.AimLostGrace) EnterAim();
                    break;

                case SwingTutorialStep.Done:
                    if (!RampOutFinished) _rampOut += dt;
                    break;
            }

            TargetTimeScale = ComputeTimeScale();
        }

        private static bool ShouldAbort(in SwingTutorialInput input, bool requireReachable)
        {
            // AnchoredToNode çağıran tarafta önce elenir; buradaki anchor "başka bir anchor"dır.
            return !input.IsAirborne || input.HasAnyAnchor || (requireReachable && !input.NodeReachable);
        }

        private void StartSlowing()
        {
            Step = SwingTutorialStep.Slowing;
            _rampIn = 0f;
        }

        private void EnterAim()
        {
            Step = SwingTutorialStep.Aim;
            _aimHeld = 0f;
            _aimLost = 0f;
        }

        private void Abort()
        {
            Step = SwingTutorialStep.Waiting;
            _rampIn = 0f;
            _aimHeld = 0f;
            _aimLost = 0f;
        }

        private void FinishFlow()
        {
            // Çıkış rampası, o anki ölçekten (en fazla SlowScale'den) 1'e iner.
            _rampOutFrom = Mathf.Max(TargetTimeScale, _settings.SlowScale);
            _rampOut = 0f;
            Step = SwingTutorialStep.Done;
        }

        private float RampInProgress()
        {
            return _settings.RampInDuration <= 0f ? 1f : Mathf.Clamp01(_rampIn / _settings.RampInDuration);
        }

        private float ComputeTimeScale()
        {
            switch (Step)
            {
                case SwingTutorialStep.Slowing:
                    return Mathf.Lerp(1f, _settings.SlowScale, RampInProgress());
                case SwingTutorialStep.Aim:
                case SwingTutorialStep.Fire:
                    return _settings.HoldScale;
                case SwingTutorialStep.Done:
                    if (RampOutFinished) return 1f;
                    float t = Mathf.Clamp01(_rampOut / _settings.RampOutDuration);
                    return Mathf.Lerp(_rampOutFrom, 1f, t);
                default:
                    return 1f;
            }
        }
    }
}
