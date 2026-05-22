using New_Scripts.Death;
using UnityEngine;

namespace New_Scripts.LevelChange
{
    /// <summary>
    /// Sadece oyun başladığında seviyenin kurulumunu yapmakla görevlidir (SRP).
    /// Sistemleri birbirine bağlar ve ilk durumu belirler.
    /// </summary>
    public class LevelBootstrapper : MonoBehaviour
    {
        [Header("Starting Parameters")]
        [SerializeField] private Room startingRoom;

        [Header("System References")]
        [SerializeField] private RoomManager roomManager;
        [SerializeField] private RespawnManager respawnManager;
        [SerializeField] private Transform playerTransform;
        
        [Tooltip("CameraController objesini buraya sürükleyin")]
        [SerializeField] private MonoBehaviour cameraHandlerComponent;
        private ICameraTransitionHandler cameraHandler;

        private void Awake()
        {
            if (cameraHandlerComponent != null)
                cameraHandler = cameraHandlerComponent as ICameraTransitionHandler;
        }

        private void Start()
        {
            SetupLevel();
        }

        private void SetupLevel()
        {
            if (startingRoom == null)
            {
                Debug.LogError("Bootstrapper: Starting Room atanmamış!");
                return;
            }

            // 1. Odaları Güvenli Duruma Getir
            roomManager.InitializeRooms();

            // 2. Oyuncu Pozisyonu ve Checkpoint Kurulumu
            Vector2 spawnPos = startingRoom.transform.position;
            if (startingRoom.InitialCheckpoint != null)
            {
                spawnPos = startingRoom.InitialCheckpoint.transform.position;
                respawnManager.OverrideInitialSpawnPoint(startingRoom.InitialCheckpoint.transform);
            }
            playerTransform.position = spawnPos;

            // 3. Kamerayı Kur
            if (cameraHandler != null)
            {
                cameraHandler.SnapToRoomBounds(startingRoom.RoomBounds, spawnPos, startingRoom.OverrideCameraSize, startingRoom.OverrideDynamicZoom);
            }

            // 4. Sistemi Başlat
            roomManager.TransitionToRoom(startingRoom);
            
            Debug.Log($"Seviye {startingRoom.RoomId} referans alınarak başarıyla kuruldu.");
        }
    }
}