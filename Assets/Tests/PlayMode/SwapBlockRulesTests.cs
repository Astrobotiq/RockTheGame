using System.Collections;
using System.Collections.Generic;
using New_Scripts.Death;
using New_Scripts.Platform;
using New_Scripts.Player.States;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.TestTools;

namespace RockTheGame.Tests.PlayMode
{
    /// <summary>
    /// Takas bloğu kuralları SB1–SB5 (docs/teleport-platform-plani.md, TP3). Gerçek oyuncu, gerçek Tilemap ve composite
    /// collider. Çerçeveler testin içinde kodla kurulur (prefab ile aynı bileşenler); varsayılan 3x3 birim, hücre 0.125.
    /// <para>
    /// Süpürülen şerit = gövdenin değdiği satırlar + en çok bir satır pay (<see cref="SwapBlockSweep"/>): tünelin duvarı
    /// gövdeye sıfır boşlukla değerse oyuncu sıkışıyor. Gövde 1x1 olduğu için ızgaraya hizalı yerleşimde 8 satır gövde
    /// + üstte ve altta 1'er pay satırı vardır (satır 7 ve 16); pay satırlarının oyulup oyulmadığına testler bakmaz.
    /// </para>
    /// </summary>
    public class SwapBlockRulesTests : PlayerRuleTestBase
    {
        private const int N = 24;
        private const float Cell = 0.125f;

        private static Tile _solidTile, _pendingTile;

        private sealed class Rig
        {
            public SwapBlockPair Pair;
            public Transform FrameA, FrameB;
            public CompositeCollider2D CompA, CompB;
            public SwapBlockGrid Grid => Pair.Grid;
        }

        private struct DashResult
        {
            public float Distance;
            public int Steps;
            public Vector2 ExitVelocity;
        }

        // ---- Kurulum ----

        private static void EnsureTiles()
        {
            if (_solidTile != null) return;
            var tex = new Texture2D(8, 8, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point };
            var px = new Color[64];
            for (int i = 0; i < px.Length; i++) px[i] = Color.white;
            tex.SetPixels(px);
            tex.Apply();
            var sprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 64f);

            _solidTile = ScriptableObject.CreateInstance<Tile>();
            _solidTile.sprite = sprite;
            _solidTile.colliderType = Tile.ColliderType.Grid;
            _pendingTile = ScriptableObject.CreateInstance<Tile>();
            _pendingTile.sprite = sprite;
            _pendingTile.colliderType = Tile.ColliderType.None;
        }

        private Transform MakeFrame(Transform root, string name, Vector2 origin, out Tilemap tilemap, out CompositeCollider2D composite)
        {
            var go = new GameObject(name) { layer = GroundLayer };
            go.transform.SetParent(root, false);
            go.transform.position = origin;
            tilemap = go.AddComponent<Tilemap>();
            go.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            go.AddComponent<TilemapCollider2D>().compositeOperation = Collider2D.CompositeOperation.Merge;
            composite = go.AddComponent<CompositeCollider2D>();
            composite.geometryType = CompositeCollider2D.GeometryType.Polygons;
            composite.generationType = CompositeCollider2D.GenerationType.Manual;
            return go.transform;
        }

        /// <summary>İki çerçeveyi verilen sol-alt köşelere kurar; pair Awake'te bütün hücreleri A'ya boyar.</summary>
        private Rig SpawnPair(Vector2 originA, Vector2 originB)
        {
            EnsureTiles();
            var root = new GameObject("SwapBlockPair_Test");
            root.SetActive(false);
            Track(root);

            var rig = new Rig();
            rig.Pair = root.AddComponent<SwapBlockPair>();
            rig.FrameA = MakeFrame(root.transform, "FrameA", originA, out Tilemap a, out rig.CompA);
            rig.FrameB = MakeFrame(root.transform, "FrameB", originB, out Tilemap b, out rig.CompB);
            SetField(rig.Pair, "frameA", a);
            SetField(rig.Pair, "frameB", b);
            SetField(rig.Pair, "pinkTile", _solidTile);
            SetField(rig.Pair, "blueTile", _solidTile);
            SetField(rig.Pair, "pendingTile", _pendingTile);

            root.SetActive(true);
            Physics2D.SyncTransforms();
            return rig;
        }

        private Rect PlayerRect()
        {
            Bounds b = Player.PlayerCollider.bounds;
            return new Rect(b.min, b.size);
        }

        private static Vector2 CellCenter(Transform frame, int x, int y) =>
            (Vector2)frame.position + new Vector2((x + 0.5f) * Cell, (y + 0.5f) * Cell);

        private static List<Vector2Int> SolidCells(SwapBlockGrid grid, SwapBlockSide side)
        {
            var list = new List<Vector2Int>();
            for (int y = 0; y < grid.Rows; y++)
            for (int x = 0; x < grid.Cols; x++)
                if (grid.IsSolid(new Vector2Int(x, y), side)) list.Add(new Vector2Int(x, y));
            return list;
        }

        private static string Describe(SwapBlockGrid grid, SwapBlockSide side)
        {
            var cells = SolidCells(grid, side);
            if (cells.Count == 0) return "0 hücre";
            int x0 = int.MaxValue, x1 = -1, y0 = int.MaxValue, y1 = -1;
            foreach (var c in cells)
            {
                x0 = Mathf.Min(x0, c.x); x1 = Mathf.Max(x1, c.x);
                y0 = Mathf.Min(y0, c.y); y1 = Mathf.Max(y1, c.y);
            }
            return $"{cells.Count} hücre, sütun {x0}..{x1}, satır {y0}..{y1}";
        }

        private static void AssertNothingMoved(Rig rig, string why)
        {
            Assert.AreEqual(N * N, rig.Grid.CountSolid(SwapBlockSide.A), $"{why}: A'dan hücre çıktı");
            Assert.AreEqual(0, rig.Grid.CountSolid(SwapBlockSide.B), $"{why}: B'ye hücre geçti");
            Assert.AreEqual(0, rig.Grid.CountPending(), $"{why}: bekleyen hücre var");
        }

        // ---- Dash ----

        /// <summary>Dash atar, dash bitene kadar fizik adımı bekler; mesafe, (duraklatılmamış) adım sayısı ve çıkış hızını döner.</summary>
        private IEnumerator Dash(Vector2 direction, System.Action<DashResult> result)
        {
            Input.LeftStick = direction;
            yield return PressDash();
            Assert.IsInstanceOf<DashState>(Player.CurrentState, "dash başlamadı");
            // Başlangıç dash'e girildikten sonra alınır: tuşa basılan karede stick yüzünden atılan yürüme adımı sayılmaz
            // (FixedUpdate her karede Update'ten önce çalışır, bu noktada henüz dash adımı yok).
            Vector2 start = Player.PlayerRigidbody.position;

            int steps = 0;
            float deadline = Time.unscaledTime + Timeout;
            while (Player.CurrentState is DashState && Time.unscaledTime < deadline)
            {
                yield return new WaitForFixedUpdate();
                // Hitstop gerçek zamanlı: duraklatılmış adım sayısı koşudan koşuya ±1 değişir, sayılmaz.
                if (!Player.IsPaused) steps++;
            }

            Input.LeftStick = Vector2.zero;
            result(new DashResult
            {
                Distance = Vector2.Dot(Player.PlayerRigidbody.position - start, direction.normalized),
                Steps = steps,
                ExitVelocity = Player.Velocity,
            });
            yield return WaitForHitStopEnd();
        }

        /// <summary>A'nın sol-alt köşesi: gövdenin alt kenarı çerçevenin 8. satır çizgisine denk gelir.</summary>
        private static Vector2 OriginForBodyRows(Rect body, float leftOfFrameFromBodyLeft) =>
            new Vector2(body.xMin + leftOfFrameFromBodyLeft, body.yMin - 8 * Cell);

        /// <summary>Gövdenin değdiği 8 satır (8..15) tamamen B'de, 7 ve 16 pay satırı serbest, geri kalanı A'da.</summary>
        private static void AssertStripMoved(Rig rig, string what)
        {
            for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                var side = rig.Grid.SideOf(new Vector2Int(x, y));
                if (y >= 8 && y <= 15)
                    Assert.AreEqual(SwapBlockSide.B, side, $"{what}: ({x},{y}) gövdenin süpürdüğü şeritte, B'ye geçmeli");
                else if (y <= 6 || y >= 17)
                    Assert.AreEqual(SwapBlockSide.A, side, $"{what}: ({x},{y}) gövdenin değmediği satırda, A'da kalmalı");
            }
        }

        // ---- SB1 ----

        [UnityTest]
        public IEnumerator SB1_HorizontalDashThroughMiddle_MovesOnlySweptStrip_AndDashIsNotStopped()
        {
            yield return LandOnFloor();

            // Malzemesiz dash: dash süresi ve çıkış hızı için ölçü.
            DashResult baseline = default;
            yield return Dash(Vector2.right, r => baseline = r);
            yield return ResetPlayerAt(Origin);
            yield return WaitForGround();

            Rect body = PlayerRect();
            var rig = SpawnPair(OriginForBodyRows(body, 1.5f), new Vector2(body.xMin + 30f, body.yMin - 1f));

            DashResult withBlock = default;
            yield return Dash(Vector2.right, r => withBlock = r);
            yield return WaitFixedSteps(2);

            var inB = SolidCells(rig.Grid, SwapBlockSide.B);
            int movedFromA = N * N - rig.Grid.CountSolid(SwapBlockSide.A);
            Assert.AreEqual(movedFromA, inB.Count, "A'dan çıkan hücre sayısı B'ye eklenene eşit olmalı");
            Assert.That(inB.Count, Is.InRange(N * 8, N * 10),
                $"gövde 8 satır + en çok 2 pay satırı; bulunan: {Describe(rig.Grid, SwapBlockSide.B)}");
            AssertStripMoved(rig, "SB1");

            // Fizik tarafı da güncel: tünel gerçekten açık, şerit B'de gerçekten dolu, geri kalan A'da dolu.
            Assert.IsFalse(rig.CompA.OverlapPoint(CellCenter(rig.FrameA, 12, 12)), "A'da tünel açık olmalı");
            Assert.IsTrue(rig.CompA.OverlapPoint(CellCenter(rig.FrameA, 12, 3)), "A'nın altı yerinde kalmalı");
            Assert.IsTrue(rig.CompA.OverlapPoint(CellCenter(rig.FrameA, 12, 20)), "A'nın üstü yerinde kalmalı");
            Assert.IsTrue(rig.CompB.OverlapPoint(CellCenter(rig.FrameB, 12, 12)), "B'de şerit dolu olmalı");
            Assert.IsFalse(rig.CompB.OverlapPoint(CellCenter(rig.FrameB, 12, 3)), "B'nin şeridin dışı boş olmalı");

            // Dash durmadı: süre, mesafe ve çıkış hızı malzemesiz dash'le aynı.
            Assert.AreEqual(baseline.Steps, withBlock.Steps, "dash süresi (fizik adımı) değişti");
            Assert.AreEqual(baseline.Distance, withBlock.Distance, 0.01f, "dash mesafesi değişti, malzeme dash'i durdurdu");
            Assert.AreEqual(baseline.ExitVelocity.x, withBlock.ExitVelocity.x, 0.05f, "çıkış hızı değişti");
        }

        // ---- SB1b ----

        [UnityTest]
        public IEnumerator SB1b_DashTouchingOnlyAHalfOfTheCorner_MovesOnlyTheTouchedPart()
        {
            yield return LandOnFloor();
            Rect body = PlayerRect();

            // Çerçevenin üst kenarı gövdenin orta yüksekliğinde: gövde yalnızca üst 0.5 birime (4 satır + pay) değiyor.
            // Yatayda dash çerçevenin ortasında bitiyor.
            var origin = new Vector2(body.xMin + 6.5f, body.center.y - 3f);
            var rig = SpawnPair(origin, new Vector2(body.xMin + 40f, body.yMin));

            yield return Dash(Vector2.right, _ => { });
            yield return WaitFixedSteps(2);

            // Dash'in en fazla ulaşabileceği x: gövdenin sağ kenarı + dash mesafesi + son adımın fazlası + pay.
            float reach = body.xMax + Player.Stats.DashDistance + Player.Stats.DashSpeed * Time.fixedDeltaTime + 0.1f;

            var inB = SolidCells(rig.Grid, SwapBlockSide.B);
            Assert.Greater(inB.Count, 0, "değen kısım taşınmalı");
            foreach (var c in inB)
            {
                Assert.That(c.y, Is.InRange(19, 23), $"{c} gövdenin değmediği satırda");
                Assert.LessOrEqual(CellCenter(rig.FrameA, c.x, c.y).x, reach, $"{c} dash'in ulaşmadığı yerde");
            }

            for (int y = 0; y < 19; y++)
            for (int x = 0; x < N; x++)
                Assert.AreEqual(SwapBlockSide.A, rig.Grid.SideOf(new Vector2Int(x, y)), $"({x},{y}) gövdeye değmiyor, yerinde kalmalı");
            for (int y = 19; y < N; y++)
            for (int x = 0; x < N; x++)
                if (CellCenter(rig.FrameA, x, y).x > reach)
                    Assert.AreEqual(SwapBlockSide.A, rig.Grid.SideOf(new Vector2Int(x, y)), $"({x},{y}) dash'in ulaşmadığı kısım, yerinde kalmalı");
            Assert.Less(inB.Count, 5 * N, "üst şeridin tamamı bile taşınmamalı: dash yarıda bitiyor");
        }

        // ---- SB2 ----

        [UnityTest]
        public IEnumerator SB2_DashingThroughTheStripInB_SendsTheSweptPartBackToA()
        {
            yield return LandOnFloor();
            Rect body = PlayerRect();
            var rig = SpawnPair(OriginForBodyRows(body, 1.5f), new Vector2(body.xMin + 9f, body.yMin - 8 * Cell));

            yield return Dash(Vector2.right, _ => { });
            yield return WaitForGround();
            int stripCells = rig.Grid.CountSolid(SwapBlockSide.B);
            Assert.That(stripCells, Is.InRange(N * 8, N * 10), "ilk dash şeridi B'ye taşımalı");

            // Şerit ve oyuncunun gövdesi aynı satırlarda dursun.
            Rect now = PlayerRect();
            rig.FrameB.position = new Vector2(rig.FrameB.position.x, now.yMin - 8 * Cell);
            Physics2D.SyncTransforms();

            yield return Dash(Vector2.right, _ => { });
            yield return WaitFixedSteps(2);

            Assert.AreEqual(0, rig.Grid.CountSolid(SwapBlockSide.B), "şeridin süpürülen kısmı A'ya dönmeli");
            Assert.AreEqual(N * N, rig.Grid.CountSolid(SwapBlockSide.A));
            Assert.IsFalse(rig.CompB.OverlapPoint(CellCenter(rig.FrameB, 12, 12)), "B'de şerit kalmamalı");
            Assert.IsTrue(rig.CompA.OverlapPoint(CellCenter(rig.FrameA, 12, 12)), "A'da tünel kapanmalı");
        }

        // ---- SB3 ----

        [UnityTest]
        public IEnumerator SB3_TargetCellsUnderThePlayer_WaitWithoutCollider_ThenSolidify_PlayerUnharmed()
        {
            Rect body = PlayerRect();
            // Yukarı dash: A oyuncunun başının 0.5 üstünde. B, A'nın tam 1 birim (gövde boyu) altında:
            // dash'in süpürdüğü her hücre B'de oyuncunun gövdesinin içine düşer.
            Vector2 originA = new Vector2(body.center.x - 1.5f, body.yMax + 0.5f);
            var rig = SpawnPair(originA, originA + Vector2.down);

            float startX = Player.PlayerRigidbody.position.x;
            int maxPending = 0;
            int overlapsWithSolid = 0;
            int pendingWithCollider = 0;
            string firstOverlap = "";

            Input.LeftStick = Vector2.up;
            yield return PressDash();
            Assert.IsInstanceOf<DashState>(Player.CurrentState);
            Vector2 start = Player.PlayerRigidbody.position;

            float deadline = Time.unscaledTime + Timeout;
            while (Player.CurrentState is DashState && Time.unscaledTime < deadline)
            {
                yield return new WaitForFixedUpdate();
                maxPending = Mathf.Max(maxPending, rig.Grid.CountPending());

                Bounds b = Player.PlayerCollider.bounds;
                var hit = Physics2D.OverlapBox(b.center, b.size * 0.98f, 0f, 1 << GroundLayer);
                if (hit != null)
                {
                    overlapsWithSolid++;
                    if (firstOverlap == "") firstOverlap = $"{hit.gameObject.name}, oyuncu {b.min}..{b.max}, bekleyen {rig.Grid.CountPending()}";
                }

                for (int y = 0; y < N; y++)
                for (int x = 0; x < N; x++)
                    if (rig.Grid.IsPending(new Vector2Int(x, y)) && rig.CompB.OverlapPoint(CellCenter(rig.FrameB, x, y)))
                        pendingWithCollider++;
            }
            Input.LeftStick = Vector2.zero;
            float travelled = Player.PlayerRigidbody.position.y - start.y;
            yield return WaitForHitStopEnd();
            yield return WaitFixedSteps(5);

            Assert.Greater(maxPending, 0, "hedefteki hücreler beklemeliydi");
            Assert.AreEqual(0, pendingWithCollider, "bekleyen hücrenin collider'ı olmamalı");
            Assert.AreEqual(0, overlapsWithSolid, $"katı hücre oyuncunun içine girdi (ezme/itme): {firstOverlap}");
            Assert.AreEqual(startX, Player.PlayerRigidbody.position.x, 0.01f, "oyuncu yana itildi");
            Assert.AreEqual(Player.Stats.DashDistance, travelled, Player.Stats.DashSpeed * Time.fixedDeltaTime,
                "dash bekleyen hücrelerde takıldı");
            Assert.IsTrue(Player.gameObject.activeSelf, "oyuncu ölmemeli");

            Assert.AreEqual(0, rig.Grid.CountPending(), "oyuncu çıkınca hücreler katılaşmalı");
            Assert.That(rig.Grid.CountSolid(SwapBlockSide.B), Is.InRange(N * 8, N * 10),
                $"taşınan sütunlar B'de katı olmalı; bulunan: {Describe(rig.Grid, SwapBlockSide.B)}");
            Assert.IsTrue(rig.CompB.OverlapPoint(CellCenter(rig.FrameB, 12, 12)), "katılaşan hücrenin collider'ı olmalı");
        }

        // ---- SB4 ----

        [UnityTest]
        public IEnumerator SB4_LandingOnTopAndWalking_MovesNoCells()
        {
            Rect body = PlayerRect();
            var rig = SpawnPair(new Vector2(body.xMin - 1f, body.yMin - 2f - 3f), new Vector2(body.xMin + 40f, body.yMin));

            yield return WaitForGround();
            Assert.Greater(Player.PlayerRigidbody.position.y, rig.FrameA.position.y + 3f, "bloğun üstünde durmalı");

            Input.LeftStick = Vector2.right;
            yield return WaitFixedSteps(8);
            Input.LeftStick = Vector2.zero;

            AssertNothingMoved(rig, "üstüne basmak ve yürümek");
        }

        [UnityTest]
        public IEnumerator SB4_WalkingIntoTheSide_MovesNoCells()
        {
            yield return LandOnFloor();
            Rect body = PlayerRect();
            var rig = SpawnPair(new Vector2(body.xMax + 1f, body.yMin), new Vector2(body.xMin + 40f, body.yMin));

            Input.LeftStick = Vector2.right;
            yield return WaitFixedSteps(40);
            Input.LeftStick = Vector2.zero;

            Assert.Less(Player.PlayerRigidbody.position.x, rig.FrameA.position.x - 0.4f, "blok duvar gibi durdurmalı");
            AssertNothingMoved(rig, "dash'siz çarpmak");
        }

        [UnityTest]
        public IEnumerator SB4_ClimbingTheSide_MovesNoCells()
        {
            Rect body = PlayerRect();
            var rig = SpawnPair(new Vector2(body.xMax + 0.03f, body.center.y - 1.5f), new Vector2(body.xMin + 40f, body.yMin));

            Input.IsRightBumperHeld = true;
            yield return WaitForState<WallClimbingState>();
            yield return WaitFixedSteps(20);

            AssertNothingMoved(rig, "duvar tırmanmak");
        }

        // ---- SB5 ----

        [UnityTest]
        public IEnumerator SB5_DyingAndRespawning_RestoresBothFramesToTheStartLayout()
        {
            yield return LandOnFloor();
            Rect body = PlayerRect();
            var rig = SpawnPair(OriginForBodyRows(body, 1.5f), new Vector2(body.xMin + 9f, body.yMin - 8 * Cell));

            yield return Dash(Vector2.right, _ => { });
            yield return WaitFixedSteps(2);
            Assert.That(rig.Grid.CountSolid(SwapBlockSide.B), Is.InRange(N * 8, N * 10), "ölmeden önce şerit taşınmış olmalı");

            Player.GetComponent<PlayerHealth>().Kill();
            float end = Time.realtimeSinceStartup + 8f;
            while ((!Player.gameObject.activeSelf || Player.IsPaused) && Time.realtimeSinceStartup < end)
                yield return null;
            Assert.IsTrue(Player.gameObject.activeSelf, "oyuncu respawn olmadı");

            AssertNothingMoved(rig, "respawn sonrası");
            Assert.IsTrue(rig.CompA.OverlapPoint(CellCenter(rig.FrameA, 12, 12)), "A'nın tüneli kapanmalı (collider)");
            Assert.IsFalse(rig.CompB.OverlapPoint(CellCenter(rig.FrameB, 12, 12)), "B'deki şerit silinmeli (collider)");
        }
    }
}
