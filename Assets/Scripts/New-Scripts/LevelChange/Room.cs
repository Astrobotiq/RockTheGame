using System.Collections.Generic;
using EasyTextEffects.Editor.MyBoxCopy.Attributes;
using New_Scripts.Death;
using New_Scripts.Player;
using UnityEngine;
using UnityEngine.Events;

namespace New_Scripts.LevelChange
{
    public class Room : MonoBehaviour, ICameraOverrideProvider
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
        
        [Header("Camera Override Settings")]
        [SerializeField] private bool useRoomCameraOverride = false;
        [SerializeField] private CameraOverrideSettings cameraOverrideSettings;
        [SerializeField] private int cameraOverridePriority = 0;

        public int Priority => cameraOverridePriority;
        public bool IsActive => useRoomCameraOverride;
        public CameraOverrideSettings Settings => cameraOverrideSettings;
        
        [Header("Room Events")]
        [Tooltip("Oyuncu bu odaya tam olarak girdiğinde tetiklenir.")]
        public UnityEvent OnRoomEntered;
        
        [Tooltip("Oyuncu bu odadan çıktığında tetiklenir.")]
        public UnityEvent OnRoomExited;

        public int RoomId => roomId;
        public Collider2D RoomBounds => roomBounds;
        
        public Checkpoint InitialCheckpoint => initialCheckpoint;
        
        public List<Room> NeighborRooms => neighborRooms;
        
        public bool OverrideDynamicZoom => useRoomCameraOverride && cameraOverrideSettings != null && cameraOverrideSettings.overrideZoom;
        public float OverrideCameraSize => (useRoomCameraOverride && cameraOverrideSettings != null) ? cameraOverrideSettings.cameraSize : 8f;

        // 1. Durum: Oyuncu bu odanın içindeyken (Her şey aktif)
        public void SetAsCurrent()
        {
            if (dynamicContent != null)
                dynamicContent.SetActive(true);

            foreach (var trigger in triggers)
                trigger.Enable();
            
            if (useRoomCameraOverride && New_Scripts.Player.CameraController.Instance != null)
            {
                New_Scripts.Player.CameraController.Instance.RegisterOverride(this);
            }

            OnRoomEntered?.Invoke();
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
            
            if (useRoomCameraOverride && New_Scripts.Player.CameraController.Instance != null)
            {
                New_Scripts.Player.CameraController.Instance.UnregisterOverride(this);
            }

            OnRoomExited?.Invoke();
        }
    }
}