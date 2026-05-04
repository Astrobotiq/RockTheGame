using CRTEffect;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// CRT efektinin yoğunluğunu DoTween Sequence kullanarak belirli bekleme süreleriyle 0 ve 1 arasında sürekli (ping-pong) değiştirir.
/// </summary>
internal class CrtPingPongController : MonoBehaviour
{
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float transitionDuration = 1f;
    [SerializeField] private float holdDuration = 0.5f;
    [SerializeField] private Ease transitionEase = Ease.InOutSine;

    private CrtFilterVolume _crtVolume;
    private Sequence _pingPongSequence;

    private void Start()
    {
        if (globalVolume == null) return;

        globalVolume.profile.TryGet(out _crtVolume);

        if (_crtVolume != null)
        {
            _crtVolume.intensity.value = 0f;
            StartPingPongEffect();
        }
    }

    private void StartPingPongEffect()
    {
        _pingPongSequence = DOTween.Sequence();

        _pingPongSequence.Append(DOTween.To(
            () => _crtVolume.intensity.value, 
            x => _crtVolume.intensity.value = x, 
            1f, 
            transitionDuration).SetEase(transitionEase));

        _pingPongSequence.AppendInterval(holdDuration);

        _pingPongSequence.Append(DOTween.To(
            () => _crtVolume.intensity.value, 
            x => _crtVolume.intensity.value = x, 
            0f, 
            transitionDuration).SetEase(transitionEase));

        _pingPongSequence.AppendInterval(holdDuration);

        _pingPongSequence.SetLoops(-1);
    }

    private void OnDestroy()
    {
        if (_pingPongSequence != null && _pingPongSequence.IsActive())
        {
            _pingPongSequence.Kill();
        }
    }
}