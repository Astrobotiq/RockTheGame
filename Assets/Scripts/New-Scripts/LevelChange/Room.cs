using System.Collections.Generic;
using EasyTextEffects.Editor.MyBoxCopy.Attributes;
using New_Scripts.Death;
using UnityEngine;

namespace New_Scripts.LevelChange
{
    public class Room : MonoBehaviour
    {
        [SerializeField] private int roomId;
        [SerializeField] private Collider2D roomBounds;
        [SerializeField] private List<RoomTransitionTrigger> triggers;
        [SerializeField] private Checkpoint initialCheckpoint;
        
        [Header("Optimization Settings")]
        [Tooltip("Odanın içindeki platformlar, düşmanlar ve efektler bu objenin içinde olmalı.")]
        [SerializeField] private GameObject dynamicContent;
        
        [Tooltip("Bu odadan doğrudan geçilebilen komşu odalar.")]
        [SerializeField] private List<Room> neighborRooms;
        
        [Header("Camera Settings")]
        [SerializeField] private bool overrideDynamicZoom = false;

        [ConditionalField(nameof(overrideDynamicZoom))] 
        [SerializeField] private float overrideCameraSize = 8f;

        public int RoomId => roomId;
        public Collider2D RoomBounds => roomBounds;
        
        public Checkpoint InitialCheckpoint => initialCheckpoint;
        
        public List<Room> NeighborRooms => neighborRooms;
        
        public bool OverrideDynamicZoom => overrideDynamicZoom;
        public float OverrideCameraSize => overrideCameraSize;

        // 1. Durum: Oyuncu bu odanın içindeyken (Her şey aktif)
        public void SetAsCurrent()
        {
            if (dynamicContent != null)
                dynamicContent.SetActive(true);

            foreach (var trigger in triggers)
                trigger.Enable();
        }

        // 2. Durum: Oyuncu komşu odadayken (Ön yükleme: İçerik aktif, kapılar kapalı)
        public void SetAsNeighbor()
        {
            if (dynamicContent != null)
                dynamicContent.SetActive(true);

            // Oyuncu henüz bu odada değil, kapı tetikleyicileri yanlışlıkla çalışmasın
            foreach (var trigger in triggers)
                trigger.Disable();
        }

        // 3. Durum: Oyuncu uzaktayken (Her şey kapalı, 0 performans harcaması)
        public void Sleep()
        {
            if (dynamicContent != null)
                dynamicContent.SetActive(false);

            foreach (var trigger in triggers)
                trigger.Disable();
        }
    }
}