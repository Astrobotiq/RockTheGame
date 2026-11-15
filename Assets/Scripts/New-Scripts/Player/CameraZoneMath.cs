using UnityEngine;

namespace New_Scripts.Player
{
    /// <summary>Kamera bölgesi giriş/çıkış kararı (histerezis). Saf hesap.</summary>
    public static class CameraZoneMath
    {
        /// <summary>
        /// İçerideyse aktif. Dışarıdayken yalnızca zaten aktifse ve bölgeden uzaklık <paramref name="exitMargin"/>'i
        /// aşmadıysa aktif kalır; sınırda zıplayan oyuncu kamerayı ileri geri çekmez.
        /// </summary>
        public static bool IsActive(bool wasActive, bool inside, float distanceOutside, float exitMargin)
        {
            if (inside) return true;
            return wasActive && distanceOutside <= Mathf.Max(0f, exitMargin);
        }
    }
}
