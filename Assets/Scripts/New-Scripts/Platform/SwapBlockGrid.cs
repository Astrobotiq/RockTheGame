using System;
using System.Collections.Generic;
using UnityEngine;

namespace New_Scripts.Platform
{
    /// <summary>Takas bloğu çerçevesi: A (pembe) ya da B (mavi).</summary>
    public enum SwapBlockSide { A, B }

    /// <summary>
    /// Takas bloğu hücre durumu ve kuralları (saf mantık, EditMode testlenebilir; docs/teleport-platform-plani.md).
    /// Her hücre her an yalnızca bir çerçevede durur (değişmez kural): A'daki (x,y) hücresi ile B'deki (x,y) hücresi eştir.
    /// Hedefte oyuncu varken taşınan hücre <em>bekler</em>: hedef çerçevededir ama katı değildir.
    /// Tilemap/collider yalnızca bu sınıftan beslenen bir görünüm katmanıdır (<c>SwapBlockPair.ApplyToView</c>).
    /// </summary>
    public sealed class SwapBlockGrid
    {
        private readonly bool[] _startsInA;
        private readonly SwapBlockSide[] _side;
        private readonly bool[] _pending;
        private readonly bool[] _movedThisSweep;

        public int Cols { get; }
        public int Rows { get; }

        /// <param name="startsInA">Hücre başına başlangıç düzeni, indeks <c>y * cols + x</c>. <c>null</c> = hepsi A'da.</param>
        public SwapBlockGrid(int cols, int rows, bool[] startsInA = null)
        {
            if (cols <= 0 || rows <= 0) throw new ArgumentOutOfRangeException(nameof(cols), "Izgara boş olamaz.");
            int count = cols * rows;
            if (startsInA != null && startsInA.Length != count)
                throw new ArgumentException("startsInA uzunluğu cols * rows olmalı.", nameof(startsInA));

            Cols = cols;
            Rows = rows;
            _startsInA = new bool[count];
            for (int i = 0; i < count; i++) _startsInA[i] = startsInA == null || startsInA[i];
            _side = new SwapBlockSide[count];
            _pending = new bool[count];
            _movedThisSweep = new bool[count];
            ApplyStartLayout();
        }

        public bool InRange(Vector2Int cell) => cell.x >= 0 && cell.x < Cols && cell.y >= 0 && cell.y < Rows;

        private int Index(Vector2Int cell) => cell.y * Cols + cell.x;
        private Vector2Int CellOf(int index) => new Vector2Int(index % Cols, index / Cols);

        /// <summary>Hücrenin bulunduğu (bekliyorsa: bulunacağı) çerçeve.</summary>
        public SwapBlockSide SideOf(Vector2Int cell) => _side[Index(cell)];

        public bool IsPending(Vector2Int cell) => _pending[Index(cell)];

        /// <summary>Hücre bu çerçevede dolu ve katı mı (bekleyen hücre hiçbir çerçevede katı değildir).</summary>
        public bool IsSolid(Vector2Int cell, SwapBlockSide side)
        {
            int i = Index(cell);
            return !_pending[i] && _side[i] == side;
        }

        public int CountSolid(SwapBlockSide side)
        {
            int n = 0;
            for (int i = 0; i < _side.Length; i++)
                if (!_pending[i] && _side[i] == side) n++;
            return n;
        }

        public int CountPending()
        {
            int n = 0;
            for (int i = 0; i < _pending.Length; i++)
                if (_pending[i]) n++;
            return n;
        }

        /// <summary>
        /// Kutunun <paramref name="delta"/> boyunca süpürdüğü alana merkezi düşen hücreler. Süpürülen alan kutu ile
        /// kutu+delta'nın dışbükey zarfıdır (yatay/dikey: dikdörtgen, çapraz: eğik bant). Yalnızca geometri: hücrenin
        /// hangi çerçevede olduğuna bakmaz, çerçeve başına çağrılır (<paramref name="frameOrigin"/> o çerçevenin sol-alt köşesi).
        /// </summary>
        public List<Vector2Int> CellsInSweptBox(Rect start, Vector2 delta, Vector2 frameOrigin, float cellSize)
        {
            var result = new List<Vector2Int>();
            if (cellSize <= 0f) return result;

            float minX = Mathf.Min(start.xMin, start.xMin + delta.x);
            float maxX = Mathf.Max(start.xMax, start.xMax + delta.x);
            float minY = Mathf.Min(start.yMin, start.yMin + delta.y);
            float maxY = Mathf.Max(start.yMax, start.yMax + delta.y);

            int x0 = Mathf.Max(0, Mathf.FloorToInt((minX - frameOrigin.x) / cellSize));
            int x1 = Mathf.Min(Cols - 1, Mathf.FloorToInt((maxX - frameOrigin.x) / cellSize));
            int y0 = Mathf.Max(0, Mathf.FloorToInt((minY - frameOrigin.y) / cellSize));
            int y1 = Mathf.Min(Rows - 1, Mathf.FloorToInt((maxY - frameOrigin.y) / cellSize));

            for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                var center = new Vector2(
                    frameOrigin.x + (x + 0.5f) * cellSize,
                    frameOrigin.y + (y + 0.5f) * cellSize);
                if (SweptBoxContains(start, delta, center)) result.Add(new Vector2Int(x, y));
            }
            return result;
        }

        /// <summary>Nokta, kutunun [0,1] aralığında delta kadar kaydırılmış hallerinden birinin içinde mi.</summary>
        private static bool SweptBoxContains(Rect start, Vector2 delta, Vector2 p)
        {
            float lo = 0f, hi = 1f;
            return NarrowT(p.x, start.xMin, start.xMax, delta.x, ref lo, ref hi)
                && NarrowT(p.y, start.yMin, start.yMax, delta.y, ref lo, ref hi);
        }

        // min <= p - t*d <= max olan t aralığını [lo,hi] ile kesiştirir; boşsa false.
        private static bool NarrowT(float p, float min, float max, float d, ref float lo, ref float hi)
        {
            if (Mathf.Approximately(d, 0f)) return p >= min && p <= max;
            float t0 = (p - max) / d;
            float t1 = (p - min) / d;
            if (t0 > t1) { float s = t0; t0 = t1; t1 = s; }
            lo = Mathf.Max(lo, t0);
            hi = Mathf.Min(hi, t1);
            return lo <= hi;
        }

        /// <summary>Yeni bir dash başlar: "bir dash bir hücreyi bir kez taşır" işaretlerini temizler.</summary>
        public void BeginSweep() => Array.Clear(_movedThisSweep, 0, _movedThisSweep.Length);

        /// <summary>
        /// <paramref name="cells"/> içinden <paramref name="from"/> çerçevesinde katı olan hücreleri karşıya geçirir ve
        /// değişenleri döner. Bu dash'te zaten taşınmış, bekleyen ya da aralık dışı hücreler atlanır.
        /// <paramref name="isOccupied"/> (hücre, hedef çerçeve) true dönerse hücre bekler.
        /// </summary>
        public List<Vector2Int> SwapCells(IEnumerable<Vector2Int> cells, SwapBlockSide from,
            Func<Vector2Int, SwapBlockSide, bool> isOccupied)
        {
            var changed = new List<Vector2Int>();
            var to = from == SwapBlockSide.A ? SwapBlockSide.B : SwapBlockSide.A;
            foreach (var cell in cells)
            {
                if (!InRange(cell)) continue;
                int i = Index(cell);
                if (_pending[i] || _movedThisSweep[i] || _side[i] != from) continue;

                _side[i] = to;
                _movedThisSweep[i] = true;
                _pending[i] = isOccupied(cell, to);
                changed.Add(cell);
            }
            return changed;
        }

        /// <summary>Bekleyen hücrelerden artık hedefte kimse olmayanlar katılaşır; katılaşanlar döner.</summary>
        public List<Vector2Int> ResolvePending(Func<Vector2Int, SwapBlockSide, bool> isOccupied)
        {
            var solidified = new List<Vector2Int>();
            for (int i = 0; i < _pending.Length; i++)
            {
                if (!_pending[i]) continue;
                var cell = CellOf(i);
                if (isOccupied(cell, _side[i])) continue;
                _pending[i] = false;
                solidified.Add(cell);
            }
            return solidified;
        }

        /// <summary>Başlangıç düzenine döner, bekleyenleri iptal eder; görünümü değişen hücreleri döner.</summary>
        public List<Vector2Int> Reset()
        {
            var changed = new List<Vector2Int>();
            for (int i = 0; i < _side.Length; i++)
            {
                var startSide = _startsInA[i] ? SwapBlockSide.A : SwapBlockSide.B;
                if (_side[i] != startSide || _pending[i]) changed.Add(CellOf(i));
            }
            ApplyStartLayout();
            return changed;
        }

        private void ApplyStartLayout()
        {
            for (int i = 0; i < _side.Length; i++)
                _side[i] = _startsInA[i] ? SwapBlockSide.A : SwapBlockSide.B;
            Array.Clear(_pending, 0, _pending.Length);
            Array.Clear(_movedThisSweep, 0, _movedThisSweep.Length);
        }
    }
}
