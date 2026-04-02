using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// Karakterin o anki görüntüsünü kopyalayan, zamanla solan ve obje havuzuna geri dönen hayalet efekti bileşeni.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class GhostAfterimage : MonoBehaviour
    {
        private SpriteRenderer ghostRenderer;
        private Color initialColor;
        
        private float activeTimer;
        private float activeDuration;

        private void Awake()
        {
            ghostRenderer = GetComponent<SpriteRenderer>();
            initialColor = ghostRenderer.color;
        }

        public void Initialize(Sprite playerSprite, Vector3 position, Quaternion rotation, float duration, Color color)
        {
            transform.position = position;
            transform.rotation = rotation;
            
            ghostRenderer.sprite = playerSprite;
            activeDuration = duration;
            activeTimer = 0f;
            
            ghostRenderer.color = color;
            gameObject.SetActive(true);
        }

        private void Update()
        {
            activeTimer += Time.deltaTime;
            float percent = activeTimer / activeDuration;
            
            Color currentColor = ghostRenderer.color;
            currentColor.a = Mathf.Lerp(initialColor.a, 0f, percent);
            ghostRenderer.color = currentColor;

            if (activeTimer >= activeDuration)
            {
                gameObject.SetActive(false);
            }
        }
    }
}