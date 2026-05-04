using System.Collections.Generic;
using UnityEngine;

namespace New_Scripts.LevelChange
{
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private List<Room> rooms;
        [SerializeField] private Room startingRoom;

        private Room activeRoom;

        private void Start()
        {
            foreach (var room in rooms)
                room.Deactivate();

            ActivateRoom(startingRoom);
        }

        public void TransitionToRoom(Room newRoom)
        {
            if (activeRoom != null)
                activeRoom.Deactivate();

            ActivateRoom(newRoom);
        }

        private void ActivateRoom(Room room)
        {
            if (room == null) return;
            activeRoom = room;
            activeRoom.Activate();
        }

        private Room FindRoomById(int id)
        {
            return rooms.Find(r => r.RoomId == id);
        }
    }
}