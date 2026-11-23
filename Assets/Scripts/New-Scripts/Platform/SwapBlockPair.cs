using System.Collections.Generic;
using New_Scripts.Audio;
using New_Scripts.Death;
using New_Scripts.Player;
using New_Scripts.Player.States;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace New_Scripts.Platform
{
    /// <summary>
    /// Takas bloğu çifti (docs/teleport-platform-plani.md): eş boyutlu iki çerçeve (A pembe, B mavi). Oyuncu dash
    /// atarken kutusunun süpürdüğü hücreler karşı çerçevede aynı konuma geçer; bloğun tamamı değil, yalnızca dash'in izi.
    /// Hücre durumunun tek kaynağı <see cref="SwapBlockGrid"/>; Tilemap ve collider yalnızca <see cref="ApplyToView"/>
    /// ile ondan beslenen bir görünüm katmanıdır (ileride marching squares ile değiştirilebilsin diye).
    /// </summary>
    // Oyuncunun FixedUpdate'inden önce çalışır: dash malzemeyi fizik çözümünden önce oyar, böylece dash çarpıp durmaz.
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Grid))]
    public class SwapBlockPair : MonoBehaviour, IResettable
    {
        [Header("Çerçeveler (sol-alt köşe = hücre 0,0; ikisi de zemin katmanında)")]
        [SerializeField] private Tilemap frameA;
        [SerializeField] private Tilemap frameB;

        [Header("Izgara")]
        [SerializeField] private int cols = 24;
        [SerializeField] private int rows = 24;
        [Tooltip("Hücre kenarı (birim). Grid cellSize'ı buradan kurulur. 0.125 = 1 birimde 8 hücre.")]
        [SerializeField] private float cellSize = 0.125f;

        [Header("Tile'lar")]
        [SerializeField] private TileBase pinkTile;
        [SerializeField] private TileBase blueTile;
        [Tooltip("Bekleyen hücre: yarı saydam, colliderType None olmalı.")]
        [SerializeField] private TileBase pendingTile;

        [Header("Fizik")]
        [Tooltip("KinematicPhysicsHandler.skinWidth ile aynı olmalı: dash'in önündeki skin kadar mesafe de oyulur.")]
        [SerializeField] private float skinWidth = 0.02f;

        [Header("Görünüm (ilk sürüm, son sanat değil)")]
        [Tooltip("Pembe/mavi tile asset'lerinin rengiyle aynı tutulur: çerçeve çizgisi ve sönen hayalet hücre bunu kullanır.")]
        [SerializeField] private Color pinkColor = new Color(1f, 0.35f, 0.7f);
        [SerializeField] private Color blueColor = new Color(0.3f, 0.55f, 1f);
        [Tooltip("Boş çerçeve de okunsun diye çerçeve çizgisi. Boşsa çizgi yok (testler).")]
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private float outlineWidth = 0.08f;
        [Tooltip("Çizgi, çerçeve renginin siyaha bu oranda çekilmiş hali: açık mavi arka planda mavi çerçeve kaybolmasın.")]
        [Range(0f, 1f)]
        [SerializeField] private float outlineDarken = 0.45f;
        [Tooltip("Takas efekti: kaynakta sönen hayalet hücre, hedefte beliren flaş. Boşsa efekt yok. Dünya uzayında, emisyonu kapalı.")]
        [SerializeField] private ParticleSystem swapFx;
        [Tooltip("Hayalet hücre ve flaşın ömrü (saniye).")]
        [SerializeField] private float swapFxLifetime = 0.25f;
        [SerializeField] private Color arrivalFlashColor = new Color(1f, 1f, 1f, 0.9f);

        [Header("Ses ve titreşim (dash başına bir kez)")]
        [SerializeField] private AudioCuePlayEventChannelSO sfxPlayChannel;
        [Tooltip("Klipsiz cue sessizce atlanır.")]
        [SerializeField] private AudioCueSO swapCue;
        [SerializeField] private PlatformVibrationSettingsSO vibrationSettings;

        private SwapBlockGrid _grid;
        private PlayerController _player;
        private bool _wasDashing;
        private bool _feedbackPlayedThisDash;
        private TilemapCollider2D _colliderA, _colliderB;
        private CompositeCollider2D _compositeA, _compositeB;

        public SwapBlockGrid Grid => _grid;

        private void Awake()
        {
            ApplyGridCellSize();
            _grid = new SwapBlockGrid(cols, rows);
            _colliderA = frameA.GetComponent<TilemapCollider2D>();
            _colliderB = frameB.GetComponent<TilemapCollider2D>();
            _compositeA = frameA.GetComponent<CompositeCollider2D>();
            _compositeB = frameB.GetComponent<CompositeCollider2D>();
            BuildOutline(frameA, pinkColor);
            BuildOutline(frameB, blueColor);
            PaintAll();
        }

        private void OnEnable()
        {
            if (LevelResetManager.Instance != null) LevelResetManager.Instance.Register(this);
        }

        private void OnDisable()
        {
            if (LevelResetManager.Instance != null) LevelResetManager.Instance.Unregister(this);
        }

        private void OnValidate()
        {
            cols = Mathf.Max(1, cols);
            rows = Mathf.Max(1, rows);
            cellSize = Mathf.Max(0.01f, cellSize);
            ApplyGridCellSize();
        }

        private void ApplyGridCellSize()
        {
            var grid = GetComponent<Grid>();
            if (grid != null) grid.cellSize = new Vector3(cellSize, cellSize, 1f);
        }

        /// <summary>Ölünce iki çerçeve başlangıç düzenine döner, bekleyen hücreler iptal edilir.</summary>
        public void ResetToDefault()
        {
            if (_grid == null) return;
            ApplyToView(_grid.Reset());
        }

        private void FixedUpdate()
        {
            if (_player == null) _player = FindFirstObjectByType<PlayerController>();
            if (_player == null || _player.PlayerCollider == null || !_player.PlayerCollider.isActiveAndEnabled) return;

            bool dashing = _player.CurrentState is DashState && !_player.IsPaused;
            if (dashing && !_wasDashing)
            {
                _grid.BeginSweep();
                _feedbackPlayedThisDash = false;
            }
            _wasDashing = dashing;

            Bounds b = _player.PlayerCollider.bounds;
            var playerRect = new Rect(b.min, b.size);
            // Bekleme payı süpürmedeki boşlukla aynı: oyuncuya payın içinde yaklaşan hücre katılaşmaz, yoksa temas
            // halindeki katı hücre oyuncuyu iter ve sıkıştırır (bkz. SwapBlockSweep).
            float margin = skinWidth * SwapBlockSweep.ClearanceFactor;
            var occupiedRect = new Rect(playerRect.xMin - margin, playerRect.yMin - margin,
                playerRect.width + 2f * margin, playerRect.height + 2f * margin);
            var changed = new List<Vector2Int>();

            if (dashing)
            {
                SwapBlockSweep.ForPlayer(playerRect, _player.Velocity, Time.fixedDeltaTime, cellSize, skinWidth,
                    out Rect start, out Vector2 delta);

                var fromA = _grid.SwapCells(
                    SweptCells(start, delta, FrameOrigin(frameA)),
                    SwapBlockSide.A, (c, s) => Occupied(c, s, occupiedRect));
                var fromB = _grid.SwapCells(
                    SweptCells(start, delta, FrameOrigin(frameB)),
                    SwapBlockSide.B, (c, s) => Occupied(c, s, occupiedRect));
                changed.AddRange(fromA);
                changed.AddRange(fromB);
                PlaySwapFeedback(fromA, fromB);
            }

            changed.AddRange(_grid.ResolvePending((c, s) => Occupied(c, s, occupiedRect)));
            ApplyToView(changed);
        }

        /// <summary>Çapraz zarf + oyuncunun fiziğinin gerçekte izlediği X-sonra-Y yolu (bkz. SwapBlockSweep.AxisLegs).</summary>
        private HashSet<Vector2Int> SweptCells(Rect start, Vector2 delta, Vector2 frameOrigin)
        {
            var cells = new HashSet<Vector2Int>(_grid.CellsInSweptBox(start, delta, frameOrigin, cellSize));
            foreach (var (legRect, legDelta) in SwapBlockSweep.AxisLegs(start, delta))
                cells.UnionWith(_grid.CellsInSweptBox(legRect, legDelta, frameOrigin, cellSize));
            return cells;
        }

        private Vector2 FrameOrigin(Tilemap frame) => frame.CellToWorld(Vector3Int.zero);

        private bool Occupied(Vector2Int cell, SwapBlockSide side, Rect playerRect)
        {
            Vector2 min = FrameOrigin(side == SwapBlockSide.A ? frameA : frameB) + (Vector2)cell * cellSize;
            return new Rect(min, new Vector2(cellSize, cellSize)).Overlaps(playerRect);
        }

        /// <summary>Çerçevenin dış hattı: dolu ya da boş, hangi çerçevenin nerede durduğu okunsun.</summary>
        private void BuildOutline(Tilemap frame, Color color)
        {
            if (outlineMaterial == null) return;
            var go = new GameObject("Outline");
            go.transform.SetParent(frame.transform, false);
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.loop = true;
            float w = cols * cellSize, h = rows * cellSize;
            line.positionCount = 4;
            line.SetPositions(new[] { new Vector3(0f, 0f), new Vector3(w, 0f), new Vector3(w, h), new Vector3(0f, h) });
            line.widthMultiplier = outlineWidth;
            line.sharedMaterial = outlineMaterial;
            line.startColor = line.endColor = Color.Lerp(color, Color.black, outlineDarken);
            line.sortingOrder = 1;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
        }

        /// <summary>
        /// Taşınan hücrelerde kaynakta kendi renginde sönen bir hayalet, hedefte beliren bir flaş (ParticleSystem, yeni tween
        /// yok). Ses ve titreşim dash başına bir kez: dash her fizik adımında hücre taşır, hepsi için çalmak yorar.
        /// </summary>
        private void PlaySwapFeedback(List<Vector2Int> fromA, List<Vector2Int> fromB)
        {
            if (fromA.Count + fromB.Count == 0) return;

            EmitSwapFx(fromA, frameA, frameB, pinkColor);
            EmitSwapFx(fromB, frameB, frameA, blueColor);

            if (_feedbackPlayedThisDash) return;
            _feedbackPlayedThisDash = true;

            if (sfxPlayChannel != null && swapCue != null)
                sfxPlayChannel.RaisePlayEvent(swapCue, _player.PlayerCollider.bounds.center);
            if (vibrationSettings != null && HapticManager.Instance != null)
                HapticManager.Instance.Vibrate(vibrationSettings.SwapBlockProfile);
        }

        private void EmitSwapFx(List<Vector2Int> cells, Tilemap source, Tilemap target, Color sourceColor)
        {
            if (swapFx == null || cells.Count == 0) return;

            Vector2 sourceOrigin = FrameOrigin(source), targetOrigin = FrameOrigin(target);
            var ghost = new ParticleSystem.EmitParams { startColor = sourceColor, startSize = cellSize, startLifetime = swapFxLifetime };
            var flash = new ParticleSystem.EmitParams { startColor = arrivalFlashColor, startSize = cellSize, startLifetime = swapFxLifetime };
            foreach (var cell in cells)
            {
                Vector2 offset = (cell + new Vector2(0.5f, 0.5f)) * cellSize;
                ghost.position = sourceOrigin + offset;
                flash.position = targetOrigin + offset;
                swapFx.Emit(ghost, 1);
                swapFx.Emit(flash, 1);
            }
        }

        private void PaintAll()
        {
            var all = new List<Vector2Int>(cols * rows);
            for (int y = 0; y < rows; y++)
            for (int x = 0; x < cols; x++) all.Add(new Vector2Int(x, y));
            ApplyToView(all);
        }

        /// <summary>
        /// Görünüm katmanı: değişen hücreleri Tilemap'lere yazar, collider'ı adımda bir kez yeniler. Dash algılama,
        /// bekleyen hücre ve reset Tilemap'i okumaz, <see cref="SwapBlockGrid"/>'i okur.
        /// </summary>
        private void ApplyToView(IReadOnlyCollection<Vector2Int> changed)
        {
            if (changed.Count == 0) return;

            var positions = new Vector3Int[changed.Count];
            var tilesA = new TileBase[changed.Count];
            var tilesB = new TileBase[changed.Count];
            int i = 0;
            foreach (var cell in changed)
            {
                positions[i] = new Vector3Int(cell.x, cell.y, 0);
                bool inA = _grid.SideOf(cell) == SwapBlockSide.A;
                TileBase tile = _grid.IsPending(cell) ? pendingTile : inA ? pinkTile : blueTile;
                tilesA[i] = inA ? tile : null;
                tilesB[i] = inA ? null : tile;
                i++;
            }

            frameA.SetTiles(positions, tilesA);
            frameB.SetTiles(positions, tilesB);
            RefreshCollider(_colliderA, _compositeA);
            RefreshCollider(_colliderB, _compositeB);
        }

        // Composite elle üretilir: aynı FixedUpdate'te oyuncunun Move'u güncel şekli görsün.
        private static void RefreshCollider(TilemapCollider2D tilemapCollider, CompositeCollider2D composite)
        {
            if (tilemapCollider != null) tilemapCollider.ProcessTilemapChanges();
            if (composite != null) composite.GenerateGeometry();
        }

        private void OnDrawGizmos()
        {
            if (frameA == null || frameB == null) return;
            DrawFrame(frameA, SwapBlockSide.A, pinkColor);
            DrawFrame(frameB, SwapBlockSide.B, blueColor);
        }

        private void DrawFrame(Tilemap frame, SwapBlockSide side, Color color)
        {
            Vector2 origin = frame.CellToWorld(Vector3Int.zero);
            var size = new Vector3(cols * cellSize, rows * cellSize, 0.01f);
            Gizmos.color = color;
            Gizmos.DrawWireCube(origin + (Vector2)size * 0.5f, size);

            // Dolu hücreler satır başına birleşik şeritler olarak: edit modunda her şey A'da.
            var fill = new Color(color.r, color.g, color.b, 0.18f);
            Gizmos.color = fill;
            for (int y = 0; y < rows; y++)
            {
                int runStart = -1;
                for (int x = 0; x <= cols; x++)
                {
                    bool solid = x < cols && (_grid != null ? _grid.IsSolid(new Vector2Int(x, y), side) : side == SwapBlockSide.A);
                    if (solid && runStart < 0) runStart = x;
                    if (solid || runStart < 0) continue;

                    var runSize = new Vector3((x - runStart) * cellSize, cellSize, 0.01f);
                    var center = origin + new Vector2(runStart * cellSize, y * cellSize) + (Vector2)runSize * 0.5f;
                    Gizmos.DrawCube(center, runSize);
                    runStart = -1;
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (frameA == null || frameB == null) return;
            DrawGridLines(frameA, new Color(pinkColor.r, pinkColor.g, pinkColor.b, 0.25f));
            DrawGridLines(frameB, new Color(blueColor.r, blueColor.g, blueColor.b, 0.25f));
        }

        private void DrawGridLines(Tilemap frame, Color color)
        {
            Vector2 origin = frame.CellToWorld(Vector3Int.zero);
            Gizmos.color = color;
            for (int x = 0; x <= cols; x++)
                Gizmos.DrawLine(origin + new Vector2(x * cellSize, 0f), origin + new Vector2(x * cellSize, rows * cellSize));
            for (int y = 0; y <= rows; y++)
                Gizmos.DrawLine(origin + new Vector2(0f, y * cellSize), origin + new Vector2(cols * cellSize, y * cellSize));
        }
    }
}
