using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Oyuncu kutusundan, <see cref="SwapBlockGrid.CellsInSweptBox"/> için süpürme girdisi üretir (saf mantık).
    /// Izgara hücre <em>merkezine</em> bakar; fizik ise kutuya değen her hücreye takılır. Bu yüzden kutu yarım hücre
    /// şişirilir (merkez testi "kutuya değiyor" testine eşitlenir) ve hareket yönüne skin kadar uzatılır
    /// (<c>KinematicPhysicsHandler</c> skin kadar ilerisine bakar).
    /// <para>
    /// Tünelin duvarı gövdeye <em>sıfır boşlukla</em> değerse oyuncu geçemez: <c>KinematicPhysicsHandler</c> temas
    /// halindeki hücreleri çakışma sayıp oyuncuyu skin kadar iter, oyuncu tünelin altına gömülüp sıkışır (Play Mode'da
    /// görüldü: 3 birimlik blokta dash 0.85'te durdu). Bu yüzden gövdeye <see cref="ClearanceFactor"/> x skin kadar
    /// yaklaşan hücreler de oyulur. Hareketsiz eksende bu pay skin'den küçüktür: oyuncu zeminin tam skin kadar üstünde
    /// durur, zemin hücresi payın dışında kalır ve dash zemini oymaz.
    /// </para>
    /// </summary>
    public static class SwapBlockSweep
    {
        /// <summary>Gövdeye bu oranda skin'den yakın hücreler oyulur. 1'in altında olmalı (zemini korur).</summary>
        public const float ClearanceFactor = 0.75f;

        public static void ForPlayer(Rect playerBounds, Vector2 velocity, float fixedDeltaTime, float cellSize,
            float skinWidth, out Rect start, out Vector2 delta)
        {
            float pad = cellSize * 0.5f + skinWidth * ClearanceFactor;
            start = new Rect(playerBounds.xMin - pad, playerBounds.yMin - pad,
                playerBounds.width + 2f * pad, playerBounds.height + 2f * pad);

            delta = velocity * fixedDeltaTime;
            if (velocity.x != 0f) delta.x += Mathf.Sign(velocity.x) * skinWidth;
            if (velocity.y != 0f) delta.y += Mathf.Sign(velocity.y) * skinWidth;
        }
    }
}
