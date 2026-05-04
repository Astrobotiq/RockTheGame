using System.Collections.Generic;
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

        public int RoomId => roomId;
        public Collider2D RoomBounds => roomBounds;
        
        public Checkpoint InitialCheckpoint => initialCheckpoint;

        public void Activate()
        {
            foreach (var trigger in triggers)
                trigger.Enable();
        }

        public void Deactivate()
        {
            foreach (var trigger in triggers)
                trigger.Disable();
        }
    }
}