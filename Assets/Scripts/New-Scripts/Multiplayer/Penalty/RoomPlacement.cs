using New_Scripts.LevelChange;
using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Multiplayer.Penalty
{
    /// <summary>
    /// Oyuncuyu bir odanın başlangıcına koyar: ışınlama, checkpoint (sonraki ölüm orada), kamera ve oda durumu.
    /// Ceza ve kayıtlı yarışa devam etme ortak kullanır; iris ve duraklatma çağıranın işi.
    /// </summary>
    public static class RoomPlacement
    {
        public static Vector3 PlaceAt(PlayerController player, Room target, RoomManager roomManager)
        {
            Vector3 spawn = target.InitialCheckpoint != null ? target.InitialCheckpoint.transform.position : target.transform.position;
            player.transform.position = new Vector3(spawn.x, spawn.y, player.transform.position.z);
            player.Velocity = Vector2.zero;
            if (target.InitialCheckpoint != null) target.InitialCheckpoint.ActivateCheckpoint();

            if (CameraController.Instance is ICameraTransitionHandler cameraHandler && target.RoomBounds != null)
                cameraHandler.SnapToRoomBounds(target.RoomBounds, spawn, target.OverrideCameraSize, target.OverrideDynamicZoom);
            // SetAsCurrent → Room.OnRoomEntered → yayıncı RoomEntered'ı yollar, RaceSession'ın odası güncellenir.
            if (roomManager != null) roomManager.TransitionToRoom(target);
            return spawn;
        }

        public static Room FindRoom(int roomId)
        {
            foreach (var room in Object.FindObjectsByType<Room>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                if (room.RoomId == roomId) return room;
            return null;
        }
    }
}
