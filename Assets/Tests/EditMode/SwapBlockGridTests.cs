using System.Collections.Generic;
using New_Scripts.Platform;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    /// <summary>
    /// Takas bloğu ızgarası (docs/teleport-platform-plani.md, TP1). Varsayılan çerçeve: 3x3 birim, hücre 0.125
    /// (24x24 hücre), sol-alt köşe dünya (0,0). Test geometrisi hücre çizgilerine oturur, böylece hücre merkezi
    /// hiçbir zaman süpürme sınırına denk gelmez.
    /// </summary>
    public class SwapBlockGridTests
    {
        private const int N = 24;
        private const float Cell = 0.125f;
        private static readonly Vector2 Origin = Vector2.zero;

        private static bool Free(Vector2Int c, SwapBlockSide s) => false;
        private static bool Busy(Vector2Int c, SwapBlockSide s) => true;

        private static List<Vector2Int> Sweep(SwapBlockGrid g, Rect start, Vector2 delta) =>
            g.CellsInSweptBox(start, delta, Origin, Cell);

        // --- Başlangıç düzeni ---

        [Test]
        public void NewGrid_DefaultsEverythingToSideA()
        {
            var g = new SwapBlockGrid(N, N);
            Assert.That(g.CountSolid(SwapBlockSide.A), Is.EqualTo(N * N));
            Assert.That(g.CountSolid(SwapBlockSide.B), Is.EqualTo(0));
            Assert.That(g.CountPending(), Is.EqualTo(0));
        }

        [Test]
        public void NewGrid_HonoursStartsInALayout()
        {
            var layout = new bool[N * N];
            layout[0] = true; // yalnızca (0,0) A'da, kalan her hücre B'de
            var g = new SwapBlockGrid(N, N, layout);

            Assert.That(g.SideOf(new Vector2Int(0, 0)), Is.EqualTo(SwapBlockSide.A));
            Assert.That(g.SideOf(new Vector2Int(1, 0)), Is.EqualTo(SwapBlockSide.B));
            Assert.That(g.SideOf(new Vector2Int(0, 1)), Is.EqualTo(SwapBlockSide.B));
            Assert.That(g.CountSolid(SwapBlockSide.A), Is.EqualTo(1));
            Assert.That(g.CountSolid(SwapBlockSide.B), Is.EqualTo(N * N - 1));
        }

        // --- Süpürülen alan ---

        [Test]
        public void HorizontalSweep_ThroughMiddle_SelectsOnlyTheMiddleStrip()
        {
            var g = new SwapBlockGrid(N, N);
            // Gövde 1 birim yüksekliğinde (y 1..2), blok ortasından geçiyor ve iki yandan taşıyor.
            var cells = Sweep(g, new Rect(-1f, 1f, 1f, 1f), new Vector2(4f, 0f));

            Assert.That(cells.Count, Is.EqualTo(N * 8), "1 birim = 8 satır, tüm sütunlar");
            foreach (var c in cells)
                Assert.That(c.y, Is.InRange(8, 15), $"{c} şeridin dışında");
        }

        [Test]
        public void SweepHalfTouchingCorner_SelectsOnlyTheTouchedCells()
        {
            var g = new SwapBlockGrid(N, N);
            // Kutu (-0.5..0.5)^2: yalnızca çerçevenin sol-alt köşesindeki 0.5x0.5'lik kısım malzemeye değiyor.
            var cells = Sweep(g, new Rect(-0.5f, -0.5f, 1f, 1f), Vector2.zero);

            Assert.That(cells.Count, Is.EqualTo(4 * 4));
            foreach (var c in cells)
            {
                Assert.That(c.x, Is.InRange(0, 3));
                Assert.That(c.y, Is.InRange(0, 3));
            }
        }

        [Test]
        public void DiagonalSweep_SelectsABandNotTheBoundingRectangle()
        {
            var g = new SwapBlockGrid(N, N);
            // 1x1 kutu (2,2) ötelemesiyle çapraz gidiyor. Süpürülen alan = 1 + 2 + 2 = 5 birim² = 320 hücre.
            // 45° kenar ızgara köşelerinden geçiyor, hücre merkezleri kenara tam denk gelirdi (eşitlik sayıyı şişirir).
            // Kutu yalnızca y'de yarım hücre (0.0625) kaydırıldı: merkezler kenarların tam ortasına düşer. Üst ucu 3'te kırpılır (~4 hücre).
            var cells = Sweep(g, new Rect(0f, 0.0625f, 1f, 1f), new Vector2(2f, 2f));
            var set = new HashSet<Vector2Int>(cells);

            Assert.That(cells.Count, Is.InRange(305, 330), $"bant ~316 hücre; sınır kutusu 576 olurdu (bulunan {cells.Count})");
            Assert.That(set.Contains(new Vector2Int(12, 12)), Is.True, "köşegen üstü seçilir");
            Assert.That(set.Contains(new Vector2Int(0, 23)), Is.False, "sol-üst köşe bantta değil");
            Assert.That(set.Contains(new Vector2Int(23, 0)), Is.False, "sağ-alt köşe bantta değil");
        }

        [Test]
        public void ZeroDeltaSweep_SelectsJustTheBoxCells()
        {
            var g = new SwapBlockGrid(N, N);
            var cells = Sweep(g, new Rect(1f, 1f, 0.5f, 0.5f), Vector2.zero);
            Assert.That(cells.Count, Is.EqualTo(4 * 4));
        }

        [Test]
        public void Sweep_FullyOutsideTheGrid_SelectsNothing()
        {
            var g = new SwapBlockGrid(N, N);
            Assert.That(Sweep(g, new Rect(10f, 10f, 1f, 1f), new Vector2(1f, 0f)), Is.Empty);
        }

        // --- Takas ---

        [Test]
        public void Swap_FromA_MovesSelectedCellsToB_AtTheSameLocalIndex()
        {
            var g = new SwapBlockGrid(N, N);
            var cells = new List<Vector2Int> { new Vector2Int(3, 4), new Vector2Int(5, 6) };

            var changed = g.SwapCells(cells, SwapBlockSide.A, Free);

            Assert.That(changed, Is.EquivalentTo(cells));
            foreach (var c in cells)
            {
                Assert.That(g.SideOf(c), Is.EqualTo(SwapBlockSide.B));
                Assert.That(g.IsSolid(c, SwapBlockSide.B), Is.True);
                Assert.That(g.IsSolid(c, SwapBlockSide.A), Is.False);
            }
            Assert.That(g.CountSolid(SwapBlockSide.A), Is.EqualTo(N * N - 2));
            Assert.That(g.CountSolid(SwapBlockSide.B), Is.EqualTo(2));
        }

        [Test]
        public void Swap_FromB_MovesCellsBackToA()
        {
            var g = new SwapBlockGrid(N, N);
            var cell = new Vector2Int(7, 7);
            g.SwapCells(new[] { cell }, SwapBlockSide.A, Free);
            g.BeginSweep(); // yeni dash

            var changed = g.SwapCells(new[] { cell }, SwapBlockSide.B, Free);

            Assert.That(changed, Is.EquivalentTo(new[] { cell }));
            Assert.That(g.SideOf(cell), Is.EqualTo(SwapBlockSide.A));
            Assert.That(g.CountSolid(SwapBlockSide.A), Is.EqualTo(N * N));
        }

        [Test]
        public void Swap_IgnoresCellsThatAreNotOnTheRequestedSide()
        {
            var g = new SwapBlockGrid(N, N); // hepsi A'da
            var changed = g.SwapCells(new[] { new Vector2Int(2, 2) }, SwapBlockSide.B, Free);

            Assert.That(changed, Is.Empty);
            Assert.That(g.SideOf(new Vector2Int(2, 2)), Is.EqualTo(SwapBlockSide.A));
        }

        [Test]
        public void Swap_OutOfRangeCells_AreIgnored()
        {
            var g = new SwapBlockGrid(N, N);
            var changed = g.SwapCells(new[] { new Vector2Int(-1, 0), new Vector2Int(N, 0), new Vector2Int(0, N) },
                SwapBlockSide.A, Free);
            Assert.That(changed, Is.Empty);
        }

        [Test]
        public void SameSweep_NeverMovesACellTwice_NewSweepAllowsIt()
        {
            var g = new SwapBlockGrid(N, N);
            var cell = new Vector2Int(4, 4);

            g.SwapCells(new[] { cell }, SwapBlockSide.A, Free);
            var again = g.SwapCells(new[] { cell }, SwapBlockSide.B, Free); // aynı dash'te geri dönmemeli

            Assert.That(again, Is.Empty);
            Assert.That(g.SideOf(cell), Is.EqualTo(SwapBlockSide.B));

            g.BeginSweep();
            var next = g.SwapCells(new[] { cell }, SwapBlockSide.B, Free);

            Assert.That(next, Is.EquivalentTo(new[] { cell }));
            Assert.That(g.SideOf(cell), Is.EqualTo(SwapBlockSide.A));
        }

        // --- Bekleyen hücre ---

        [Test]
        public void TargetOccupied_CellBecomesPending_NotSolidOnEitherSide()
        {
            var g = new SwapBlockGrid(N, N);
            var cell = new Vector2Int(9, 9);

            var changed = g.SwapCells(new[] { cell }, SwapBlockSide.A, Busy);

            Assert.That(changed, Is.EquivalentTo(new[] { cell }));
            Assert.That(g.SideOf(cell), Is.EqualTo(SwapBlockSide.B));
            Assert.That(g.IsPending(cell), Is.True);
            Assert.That(g.IsSolid(cell, SwapBlockSide.A), Is.False);
            Assert.That(g.IsSolid(cell, SwapBlockSide.B), Is.False);
            Assert.That(g.CountPending(), Is.EqualTo(1));
        }

        [Test]
        public void ResolvePending_StaysPendingWhileOccupied_ThenTurnsSolidWhenFree()
        {
            var g = new SwapBlockGrid(N, N);
            var cell = new Vector2Int(9, 9);
            g.SwapCells(new[] { cell }, SwapBlockSide.A, Busy);

            Assert.That(g.ResolvePending(Busy), Is.Empty);
            Assert.That(g.IsPending(cell), Is.True);

            var solidified = g.ResolvePending(Free);

            Assert.That(solidified, Is.EquivalentTo(new[] { cell }));
            Assert.That(g.IsPending(cell), Is.False);
            Assert.That(g.IsSolid(cell, SwapBlockSide.B), Is.True);
        }

        [Test]
        public void ResolvePending_ChecksOccupancyOnTheCellsOwnSide()
        {
            var g = new SwapBlockGrid(N, N);
            var cell = new Vector2Int(1, 1);
            g.SwapCells(new[] { cell }, SwapBlockSide.A, Busy); // hedef B

            SwapBlockSide asked = SwapBlockSide.A;
            g.ResolvePending((c, s) => { asked = s; return true; });

            Assert.That(asked, Is.EqualTo(SwapBlockSide.B));
        }

        [Test]
        public void PendingCell_IsNotSwappedBySweeps()
        {
            var g = new SwapBlockGrid(N, N);
            var cell = new Vector2Int(2, 3);
            g.SwapCells(new[] { cell }, SwapBlockSide.A, Busy);
            g.BeginSweep();

            var changed = g.SwapCells(new[] { cell }, SwapBlockSide.B, Free);

            Assert.That(changed, Is.Empty, "katı olmayan hücre taşınmaz");
            Assert.That(g.IsPending(cell), Is.True);
            Assert.That(g.SideOf(cell), Is.EqualTo(SwapBlockSide.B));
        }

        // --- Reset ve değişmez kural ---

        [Test]
        public void Reset_RestoresStartLayout_AndCancelsPending()
        {
            var layout = new bool[N * N];
            for (int i = 0; i < layout.Length; i++) layout[i] = i % 2 == 0;
            var g = new SwapBlockGrid(N, N, layout);

            g.SwapCells(new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) }, SwapBlockSide.A, Free);
            g.SwapCells(new[] { new Vector2Int(1, 0) }, SwapBlockSide.B, Busy); // (1,0) B'deydi: A'ya geçer, bekler
            g.SwapCells(new[] { new Vector2Int(3, 3) }, SwapBlockSide.B, Busy);

            var changed = g.Reset();

            Assert.That(g.CountPending(), Is.EqualTo(0));
            for (int i = 0; i < layout.Length; i++)
            {
                var cell = new Vector2Int(i % N, i / N);
                Assert.That(g.SideOf(cell), Is.EqualTo(layout[i] ? SwapBlockSide.A : SwapBlockSide.B), $"{cell}");
                Assert.That(g.IsSolid(cell, g.SideOf(cell)), Is.True);
            }
            Assert.That(changed.Count, Is.GreaterThan(0));

            // Reset sweep işaretlerini de temizler: ilk hücre yeniden taşınabilir.
            Assert.That(g.SwapCells(new[] { new Vector2Int(0, 0) }, SwapBlockSide.A, Free), Is.Not.Empty);
        }

        [Test]
        public void Invariant_EveryCellIsSolidOnExactlyOneSideOrPending()
        {
            var g = new SwapBlockGrid(N, N);
            var rng = new System.Random(1234);
            for (int step = 0; step < 200; step++)
            {
                if (step % 5 == 0) g.BeginSweep();
                var picks = new List<Vector2Int>();
                for (int i = 0; i < 40; i++) picks.Add(new Vector2Int(rng.Next(N), rng.Next(N)));
                var from = rng.Next(2) == 0 ? SwapBlockSide.A : SwapBlockSide.B;
                bool busy = rng.Next(3) == 0;
                g.SwapCells(picks, from, busy ? Busy : Free);
                if (step % 7 == 0) g.ResolvePending(Free);

                int solidA = g.CountSolid(SwapBlockSide.A);
                int solidB = g.CountSolid(SwapBlockSide.B);
                Assert.That(solidA + solidB + g.CountPending(), Is.EqualTo(N * N), $"adım {step}");
                for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                {
                    var c = new Vector2Int(x, y);
                    Assert.That(g.IsSolid(c, SwapBlockSide.A) && g.IsSolid(c, SwapBlockSide.B), Is.False, $"{c} iki tarafta katı");
                }
            }
        }
    }
}
