using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Oyuncu kutusundan, <see cref="SwapBlockGrid.CellsInSweptBox"/> için süpürme girdisi üretir (saf mantık).
    /// Izgara hücre <em>merkezine</em> bakar; fizik ise kutuya değen her hücreye takılır. Bu yüzden kutu yarım hücre
    /// şişirilir (merkez testi "kutuya değiyor" testine eşitlenir) ve hareket yönüne skin kadar uzatılır
    /// (<c>KinematicPhysicsHandler</c> skin kadar ilerisine bakar). Hareketsiz eksene skin eklenmez: oyuncu zeminin
    /// skin kadar üstünde durur, zemin hücresi merkezi şişmiş kutunun dışında kalır ve dash zemini oymaz.
    /// </summary>
    public static class SwapBlockSweep
    {
        /// <summary>
        /// Şişirme yarım hücreden bu kadar eksiktir: yalnızca kutuya <em>değen</em> (sıfır girişim) hücre merkezi
        /// sınıra denk gelip seçilmesin. Izgaraya hizalı kutuda gövdenin bir hücre üstü/altı fazladan oyulurdu.
        /// </summary>
        public const float Tolerance = 0.001f;

        public static void ForPlayer(Rect playerBounds, Vector2 velocity, float fixedDeltaTime, float cellSize,
            float skinWidth, out Rect start, out Vector2 delta)
        {
            float pad = cellSize * 0.5f - Tolerance;
            start = new Rect(playerBounds.xMin - pad, playerBounds.yMin - pad,
                playerBounds.width + 2f * pad, playerBounds.height + 2f * pad);

            delta = velocity * fixedDeltaTime;
            if (velocity.x != 0f) delta.x += Mathf.Sign(velocity.x) * skinWidth;
            if (velocity.y != 0f) delta.y += Mathf.Sign(velocity.y) * skinWidth;
        }
    }
}
