using System.Collections.Generic;
using UnityEngine;

namespace New_Scripts.LevelChange
{
    public class RoomManager : MonoBehaviour
    {
        [SerializeField] private List<Room> rooms;

        public void InitializeRooms()
        {
            foreach (var room in rooms)
                room.Sleep();
        }

        public void TransitionToRoom(Room newRoom)
        {
            if (newRoom == null) return;

            foreach (var room in rooms)
            {
                if (room == newRoom)
                    room.SetAsCurrent();
                else if (newRoom.NeighborRooms != null && newRoom.NeighborRooms.Contains(room))
                    room.SetAsNeighbor();
                else
                    room.Sleep();
            }
        }

        private Room FindRoomById(int id)
        {
            return rooms.Find(r => r.RoomId == id);
        }
    }
}