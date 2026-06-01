using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace New_Scripts.Player.Visual
{
    /// <summary>
    /// Karakterin parca bazli (vucut ve kollar) yetenek kullanimlarina bagli renk gecislerini DOTween ile unscaled time uzerinden yoneten gorsel kontrolcu.
    /// </summary>
    public class PlayerColorController : MonoBehaviour
    {
        [Header("Renderers")]
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private SpriteRenderer[] armRenderers;

        [Header("Colors")]
        [SerializeField] private Color defaultBodyColor = Color.white;
        [SerializeField] private List<Color> defaultArmColor = new();
        [SerializeField] private Color dashExhaustedColor = new Color(1f, 0.4f, 0.4f); 
        [SerializeField] private List<Color> slingshotExhaustedColor = new(); 
        [SerializeField] private float transitionDuration = 0.15f;

        private Tween bodyTween;
        private Tween[] armTweens;

        private void Awake()
        {
            armTweens = new Tween[armRenderers.Length];
        }

        public void SetDashExhausted()
        {
            bodyTween?.Kill();
            bodyTween = bodyRenderer.DOColor(dashExhaustedColor, transitionDuration).SetUpdate(true);
        }

        public void SetSlingshotExhausted()
        {
            for (int i = 0; i < armRenderers.Length; i++)
            {
                armTweens[i]?.Kill();
                armTweens[i] = armRenderers[i].DOColor(slingshotExhaustedColor[i], transitionDuration).SetUpdate(true);
            }
        }

        public void ResetAllColors()
        {
            ResetBodyColor();
            ResetArmColors();
        }
        
        public void ResetArmColors()
        {
            for (int i = 0; i < armRenderers.Length; i++)
            {
                armTweens[i]?.Kill();
                armTweens[i] = armRenderers[i].DOColor(defaultArmColor[i], transitionDuration).SetUpdate(true);
            }
        }
        
        public void ResetBodyColor()
        {
            bodyTween?.Kill();
            bodyTween = bodyRenderer.DOColor(defaultBodyColor, transitionDuration).SetUpdate(true);
        }
    }
}