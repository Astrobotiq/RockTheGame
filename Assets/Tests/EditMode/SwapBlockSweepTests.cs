using New_Scripts.Platform;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    /// <summary>Oyuncu kutusundan takas süpürmesi (TP2): hücre merkezi testi, kutuya değen her hücreyi yakalasın.</summary>
    public class SwapBlockSweepTests
    {
        private const float Cell = 0.125f;
        private const float Skin = 0.02f;

        [Test]
        public void Box_IsInflatedByHalfACellPlusClearance_OnEverySide()
        {
            SwapBlockSweep.ForPlayer(new Rect(2f, 3f, 1f, 1.5f), Vector2.zero, 0.02f, Cell, Skin,
                out Rect start, out Vector2 delta);

            float pad = Cell * 0.5f + Skin * SwapBlockSweep.ClearanceFactor;
            Assert.That(start.xMin, Is.EqualTo(2f - pad).Within(1e-5f));
            Assert.That(start.xMax, Is.EqualTo(3f + pad).Within(1e-5f));
            Assert.That(start.yMin, Is.EqualTo(3f - pad).Within(1e-5f));
            Assert.That(start.yMax, Is.EqualTo(4.5f + pad).Within(1e-5f));
        }

        [Test]
        public void GridAlignedBody_GetsOneRowOfClearanceOnEachSide_SoItNeverTouchesTheTunnelWall()
        {
            // Gövde tam ızgara çizgilerine oturuyor (y 1..2 = 8 satır). Duvar gövdeye sıfır boşlukla değerse oyuncu
            // sıkışır (Play Mode'da görüldü): 7. ve 16. satır da oyulur, tünel 10 satır.
            var grid = new SwapBlockGrid(24, 24);
            SwapBlockSweep.ForPlayer(new Rect(-1f, 1f, 1f, 1f), new Vector2(30f, 0f), 0.02f, Cell, Skin,
                out Rect start, out Vector2 delta);

            var rowsHit = RowsOf(grid.CellsInSweptBox(start, delta, Vector2.zero, Cell));

            Assert.That(rowsHit.Count, Is.EqualTo(10));
            Assert.That(rowsHit.Contains(7) && rowsHit.Contains(16), Is.True, "temas eden satırlar oyulmalı");
            Assert.That(rowsHit.Contains(6) || rowsHit.Contains(17), Is.False, "bir satırdan fazla pay yok");
        }

        [Test]
        public void BodyWithRoomToSpare_DoesNotCarveAnExtraRow()
        {
            // Gövde y 1.03..2.03: 7. satırın yüzü (1.0) gövdeden 0.03 > 0.015 uzakta, boşluk zaten var.
            var grid = new SwapBlockGrid(24, 24);
            SwapBlockSweep.ForPlayer(new Rect(-1f, 1.03f, 1f, 1f), new Vector2(30f, 0f), 0.02f, Cell, Skin,
                out Rect start, out Vector2 delta);

            var rowsHit = RowsOf(grid.CellsInSweptBox(start, delta, Vector2.zero, Cell));

            Assert.That(rowsHit.Count, Is.EqualTo(9));
            Assert.That(rowsHit.Contains(7), Is.False);
            Assert.That(rowsHit.Contains(8) && rowsHit.Contains(16), Is.True);
        }

        private static System.Collections.Generic.HashSet<int> RowsOf(System.Collections.Generic.List<Vector2Int> cells)
        {
            var rows = new System.Collections.Generic.HashSet<int>();
            foreach (var c in cells) rows.Add(c.y);
            return rows;
        }

        [Test]
        public void StandingStill_HasNoDelta_NotEvenTheSkin()
        {
            SwapBlockSweep.ForPlayer(new Rect(0f, 0f, 1f, 1f), Vector2.zero, 0.02f, Cell, Skin, out _, out Vector2 delta);
            Assert.That(delta, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void Delta_IsVelocityTimesStep_PlusSkinAlongEachMovingAxis()
        {
            SwapBlockSweep.ForPlayer(new Rect(0f, 0f, 1f, 1f), new Vector2(-30f, 0f), 0.02f, Cell, Skin,
                out _, out Vector2 delta);

            Assert.That(delta.x, Is.EqualTo(-0.6f - Skin).Within(1e-5f), "sola: skin da sola eklenir");
            Assert.That(delta.y, Is.EqualTo(0f), "durağan eksene skin eklenmez (ayak altındaki zemin oyulmaz)");
        }

        [Test]
        public void Diagonal_AddsSkinOnBothAxes()
        {
            SwapBlockSweep.ForPlayer(new Rect(0f, 0f, 1f, 1f), new Vector2(10f, 20f), 0.02f, Cell, Skin,
                out _, out Vector2 delta);

            Assert.That(delta.x, Is.EqualTo(0.2f + Skin).Within(1e-5f));
            Assert.That(delta.y, Is.EqualTo(0.4f + Skin).Within(1e-5f));
        }

        [Test]
        public void FloorUnderRestingFeet_IsNotCarved_ButOverlappingRowIs()
        {
            // Oyuncu zeminin skin kadar üstünde duruyor: zemin hücresi merkezi kutu dışında kalmalı.
            var grid = new SwapBlockGrid(24, 24);
            Rect body = new Rect(1f, 3f + Skin, 1f, 1f); // zemin üstü y=3
            SwapBlockSweep.ForPlayer(body, new Vector2(30f, 0f), 0.02f, Cell, Skin, out Rect start, out Vector2 delta);

            var cells = grid.CellsInSweptBox(start, delta, new Vector2(0f, 0f), Cell);
            // Çerçeve 0..3 yüksekliğinde; gövde 3.02'den başlıyor, çerçevenin üstünde. Hiçbir hücre seçilmemeli.
            Assert.That(cells, Is.Empty);

            // Gövde zemine 0.05 gömülü olsaydı, en üst satır (merkez y=2.9375) kutuya değerdi.
            Rect sunk = new Rect(1f, 2.95f, 1f, 1f);
            SwapBlockSweep.ForPlayer(sunk, new Vector2(30f, 0f), 0.02f, Cell, Skin, out start, out delta);
            cells = grid.CellsInSweptBox(start, delta, Vector2.zero, Cell);
            Assert.That(cells.Count, Is.GreaterThan(0));
            foreach (var c in cells) Assert.That(c.y, Is.EqualTo(23), "yalnızca değen üst satır");
        }
    }
}
