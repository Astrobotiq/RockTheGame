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
        [SerializeField] private List<Color> defaultArmColor = new();
        [SerializeField] private List<Color> slingshotExhaustedColor = new(); 
        [SerializeField] private float transitionDuration = 0.15f;

        [Header("Palette Swap (3-Color Crown)")]
        [SerializeField] private Color crownTargetColor1 = new Color(1f, 0.92f, 0.016f); // Default yellow 1
        [SerializeField] private Color crownReplaceColor1 = new Color(0.5f, 0.5f, 0.5f); // Replacement 1
        [SerializeField] private float tolerance1 = 0.05f;

        [SerializeField] private Color crownTargetColor2 = new Color(1f, 0.8f, 0.0f); // Default yellow 2
        [SerializeField] private Color crownReplaceColor2 = new Color(0.4f, 0.4f, 0.4f); // Replacement 2
        [SerializeField] private float tolerance2 = 0.05f;

        [SerializeField] private Color crownTargetColor3 = new Color(0.9f, 0.7f, 0.0f); // Default yellow 3
        [SerializeField] private Color crownReplaceColor3 = new Color(0.3f, 0.3f, 0.3f); // Replacement 3
        [SerializeField] private float tolerance3 = 0.05f;

        private Tween bodyTween;
        private Tween[] armTweens;
        private Material bodyMaterial;

        private void Awake()
        {
            armTweens = new Tween[armRenderers.Length];
            if (bodyRenderer != null)
            {
                bodyMaterial = bodyRenderer.material;
            }
        }

        private void Start()
        {
            if (bodyMaterial != null)
            {
                bodyMaterial.SetColor("_TargetColor1", crownTargetColor1);
                bodyMaterial.SetColor("_ReplaceColor1", crownReplaceColor1);
                bodyMaterial.SetFloat("_Tolerance1", tolerance1);

                bodyMaterial.SetColor("_TargetColor2", crownTargetColor2);
                bodyMaterial.SetColor("_ReplaceColor2", crownReplaceColor2);
                bodyMaterial.SetFloat("_Tolerance2", tolerance2);

                bodyMaterial.SetColor("_TargetColor3", crownTargetColor3);
                bodyMaterial.SetColor("_ReplaceColor3", crownReplaceColor3);
                bodyMaterial.SetFloat("_Tolerance3", tolerance3);

                bodyMaterial.SetFloat("_Blend", 0f);
            }
        }

        public void SetDashExhausted()
        {
            bodyTween?.Kill();
            
            if (bodyMaterial != null)
            {
                bodyTween = bodyMaterial.DOFloat(1f, "_Blend", transitionDuration).SetUpdate(true);
            }
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
            
            if (bodyMaterial != null)
            {
                bodyTween = bodyMaterial.DOFloat(0f, "_Blend", transitionDuration).SetUpdate(true);
            }
        }
    }
}