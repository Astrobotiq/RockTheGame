using System.Collections.Generic;
using New_Scripts.Player.States;
using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>
    /// FSM durumlarını dinleyerek TrailRenderer ve Ghost (Afterimage) efektlerini yöneten, obje havuzlama (zero-allocation) kullanan görsel sistem.
    /// </summary>
    public class PlayerVFXManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private PlayerController playerContext;
        [SerializeField] private SpriteRenderer playerSpriteRenderer;
        [SerializeField] private TrailRenderer movementTrail;

        [Header("Ghost (Afterimage) Settings")]
        [SerializeField] private GameObject ghostPrefab;
        [SerializeField] private float ghostSpawnRate = 0.05f; 
        [SerializeField] private float ghostDuration = 0.5f; 
        [SerializeField] private Color ghostColor = new Color(0.5f, 0.5f, 1f, 0.5f);

        // Obje Havuzlama listesi.
        private List<GhostAfterimage> ghostPool = new List<GhostAfterimage>();
        private float ghostSpawnTimer;
        
        private Transform staticGhostContainer;

        private void Awake()
        {
            GameObject containerObj = new GameObject("Ghost_Pool_Container");
            staticGhostContainer = containerObj.transform;
        }

        private void Update()
        {
            HandleTrailEffect();
            HandleGhostEffect();
        }

        private void HandleTrailEffect()
        {
            bool shouldTrail = (playerContext.CurrentState is SwingingState || 
                                playerContext.CurrentState is DualSwingingState || 
                                playerContext.CurrentState is DashState);
            
            if (movementTrail.emitting != shouldTrail)
            {
                movementTrail.emitting = shouldTrail;
            }
        }

        private void HandleGhostEffect()
        {
            if (playerContext.CurrentState is DashState)
            {
                ghostSpawnTimer -= Time.deltaTime;
                if (ghostSpawnTimer <= 0f)
                {
                    SpawnGhost();
                    ghostSpawnTimer = ghostSpawnRate;
                }
            }
        }

        private void SpawnGhost()
        {
            GhostAfterimage ghost = null;
            for (int i = 0; i < ghostPool.Count; i++)
            {
                if (!ghostPool[i].gameObject.activeSelf)
                {
                    ghost = ghostPool[i];
                    break;
                }
            }

            if (ghost == null)
            {
                GameObject newGhost = Instantiate(ghostPrefab, staticGhostContainer);
                ghost = newGhost.GetComponent<GhostAfterimage>();
                ghostPool.Add(ghost);
            }

            ghost.Initialize(
                playerSpriteRenderer.sprite, 
                playerSpriteRenderer.transform.position, 
                playerSpriteRenderer.transform.rotation, 
                ghostDuration, 
                ghostColor,
                playerSpriteRenderer.flipX,
                playerSpriteRenderer.flipY
            );
        }
    }
}