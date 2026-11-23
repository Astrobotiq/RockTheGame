using New_Scripts.Collectible;
using New_Scripts.Door;
using New_Scripts.LevelChange;

namespace New_Scripts.Multiplayer.Penalty
{
    /// <summary>
    /// Ceza, oyuncuyu bir odaya "ilk kez giriyormuş" gibi koyar: odadaki anahtar–kapı durumu ilk hâline döner.
    /// Tek kişilik kod odaya her zaman ilk kez girildiğini varsayıyordu; ceza bu varsayımı bozan tek yol.
    /// İki tür kapı sıfırlanır: anahtarla yönetilenler (<see cref="RoomKeyManager"/>) ve açık başlayıp odaya girince arkandan
    /// kapananlar (oda 9, 10, 21). İkincisi unutulursa 9 → 8 cezasında oda 9'un kapalı kapısı, oda 8'in kapısıyla aynı geçitte
    /// durup yolu tıkıyordu. Kapalı başlayıp anahtarsız açılan kapılar (oda 17) ve toplanabilirler (ananas) kapsam dışı.
    /// </summary>
    public static class RoomStateReset
    {
        /// <summary>Odanın altındaki bütün anahtar yöneticilerini sıfırlar (uyuyan oda kapalı olabilir, o yüzden pasifler dahil).</summary>
        public static void ResetRoom(Room room)
        {
            if (room == null) return;
            foreach (var manager in room.GetComponentsInChildren<RoomKeyManager>(true))
                manager.ResetForRoomReentry();

            foreach (var door in room.GetComponentsInChildren<RoomDoor>(true))
                if (door.StartsOpen) door.ResetToInitial();
        }
    }
}
