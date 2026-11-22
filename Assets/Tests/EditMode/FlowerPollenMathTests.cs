using New_Scripts.Flower;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class FlowerPollenMathTests
    {
        [Test]
        public void Strength_FloorAtLowSpeed_OneAtMaxSpeed()
        {
            Assert.That(FlowerPollenMath.Strength(0f, 4f, 40f, 0.35f), Is.EqualTo(0.35f).Within(1e-6f));
            Assert.That(FlowerPollenMath.Strength(40f, 4f, 40f, 0.35f), Is.EqualTo(1f).Within(1e-6f));
            Assert.That(FlowerPollenMath.Strength(400f, 4f, 40f, 0.35f), Is.EqualTo(1f));
            float mid = FlowerPollenMath.Strength(22f, 4f, 40f, 0.35f);
            Assert.That(mid, Is.InRange(0.35f, 1f));
        }

        [Test]
        public void Count_BaseAtFloor_FullAtMax_MonotonicAndOrdered()
        {
            Assert.AreEqual(10, FlowerPollenMath.Count(0.35f, 0.35f, 10, 18));
            Assert.AreEqual(18, FlowerPollenMath.Count(1f, 0.35f, 10, 18));
            int prev = 0;
            for (int i = 0; i <= 20; i++)
            {
                int c = FlowerPollenMath.Count(0.35f + 0.65f * i / 20f, 0.35f, 10, 18);
                Assert.GreaterOrEqual(c, prev);
                Assert.That(c, Is.InRange(10, 18));
                prev = c;
            }
            Assert.AreEqual(12, FlowerPollenMath.Count(1f, 0.35f, 12, 5), "max < min ise min kalır");
        }

        [Test]
        public void Kind_FullBurstHasRoughlyHalfDotsThirdEllipsesRestPieces()
        {
            int dots = 0, ellipses = 0, pieces = 0;
            for (int i = 0; i < 18; i++)
            {
                switch (FlowerPollenMath.Kind(i, 18, 0.5f, 0.33f))
                {
                    case PollenKind.Dot: dots++; break;
                    case PollenKind.Ellipse: ellipses++; break;
                    default: pieces++; break;
                }
            }
            Assert.That(dots, Is.InRange(7, 11), "nokta ≈ 9");
            Assert.That(ellipses, Is.InRange(4, 8), "elips ≈ 6");
            Assert.That(pieces, Is.InRange(1, 5), "parça ≈ 3");
            Assert.AreEqual(18, dots + ellipses + pieces);
        }

        [Test]
        public void Kind_IsMixedNotSortedAndDeterministic()
        {
            // Parçalar sona yığılmasın: ilk yarıda da son yarıda da en az bir kâğıt parçası olsun.
            int firstHalf = 0, secondHalf = 0;
            for (int i = 0; i < 18; i++)
            {
                Assert.AreEqual(FlowerPollenMath.Kind(i, 18, 0.5f, 0.33f), FlowerPollenMath.Kind(i, 18, 0.5f, 0.33f));
                if (FlowerPollenMath.Kind(i, 18, 0.5f, 0.33f) == PollenKind.Piece) { if (i < 9) firstHalf++; else secondHalf++; }
            }
            Assert.GreaterOrEqual(firstHalf, 1);
            Assert.GreaterOrEqual(secondHalf, 1);
            Assert.AreEqual(PollenKind.Dot, FlowerPollenMath.Kind(0, 0, 0.5f, 0.3f), "sayı 0: bölme yok");
        }

        [Test]
        public void MinDiameter_ScalesWithCamera_AndClampsSizes()
        {
            Assert.That(FlowerPollenMath.MinDiameter(8f), Is.EqualTo(8f / 60f).Within(1e-6f));
            Assert.That(FlowerPollenMath.MinDiameter(22f), Is.EqualTo(22f / 60f).Within(1e-6f));
            Assert.That(FlowerPollenMath.ClampSize(0.14f, 22f), Is.EqualTo(22f / 60f).Within(1e-6f), "geniş kamerada küçük nokta büyür");
            Assert.That(FlowerPollenMath.ClampSize(0.4f, 8f), Is.EqualTo(0.4f), "yeterince büyük boyuta dokunulmaz");
            Assert.That(FlowerPollenMath.MinDiameter(-3f), Is.EqualTo(0f));
        }

        [Test]
        public void OrganPoints_SitOnTheRingAtHalfStepFromTheTop_72DegreesApart()
        {
            Vector2 c = new Vector2(3f, -2f);
            for (int k = 0; k < 5; k++)
            {
                Vector2 p = FlowerPollenMath.OrganPoint(k, 5, c, 0.63f);
                Assert.That((p - c).magnitude, Is.EqualTo(0.63f).Within(1e-5f));
            }
            // Tepede organ yok (yarım adım kayık): hiçbir organ doğrudan yukarıda değil, ikisi tepeye eşit uzaklıkta.
            float best = 360f;
            for (int k = 0; k < 5; k++)
            {
                Vector2 d = FlowerPollenMath.OrganPoint(k, 5, Vector2.zero, 1f);
                best = Mathf.Min(best, Mathf.Abs(Vector2.Angle(Vector2.up, d)));
            }
            Assert.That(best, Is.EqualTo(36f).Within(0.01f), "en yakın organ tepeden 36°");
            Vector2 a = FlowerPollenMath.OrganPoint(0, 5, Vector2.zero, 1f);
            Vector2 b = FlowerPollenMath.OrganPoint(1, 5, Vector2.zero, 1f);
            Assert.That(Vector2.Angle(a, b), Is.EqualTo(72f).Within(0.01f));
            Assert.That(a.x, Is.LessThan(0f), "0. organ tepenin solunda (saat yönünün tersine yarım adım)");
        }

        [Test]
        public void NearestOrgans_FaceThePlayer_ClosestFirst_NoDuplicates()
        {
            var result = new int[5];
            // Oyuncu sağda: sağa en yakın organlar
            int n = FlowerPollenMath.NearestOrgans(Vector2.zero, Vector2.right, 5, 3, result);
            Assert.AreEqual(3, n);
            Assert.That(result[0], Is.Not.EqualTo(result[1]));
            Assert.That(result[1], Is.Not.EqualTo(result[2]));
            Assert.That(result[0], Is.Not.EqualTo(result[2]));
            float prev = -1f;
            for (int i = 0; i < n; i++)
            {
                Vector2 d = FlowerPollenMath.OrganPoint(result[i], 5, Vector2.zero, 1f);
                float angle = Vector2.Angle(Vector2.right, d);
                Assert.That(angle, Is.GreaterThanOrEqualTo(prev - 1e-3f), "en yakın önce");
                prev = angle;
            }
            // Her yönde seçilen en yakın organ gerçekten en yakını mı
            for (float deg = 0f; deg < 360f; deg += 7f)
            {
                Vector2 toward = new Vector2(Mathf.Cos(deg * Mathf.Deg2Rad), Mathf.Sin(deg * Mathf.Deg2Rad));
                FlowerPollenMath.NearestOrgans(Vector2.zero, toward, 5, 1, result);
                float chosen = Vector2.Angle(toward, FlowerPollenMath.OrganPoint(result[0], 5, Vector2.zero, 1f));
                Assert.That(chosen, Is.LessThanOrEqualTo(36.01f), $"{deg}° yönünde en yakın organ en çok 36° uzakta olmalı");
            }
            Assert.AreEqual(0, FlowerPollenMath.NearestOrgans(Vector2.zero, Vector2.up, 5, 0, result));
        }

        [Test]
        public void PickOrgan_RespectsNearShare()
        {
            var near = new[] { 2, 3, 4, 0, 0 };
            // r1 < nearShare: yakın organlardan
            Assert.AreEqual(2, FlowerPollenMath.PickOrgan(0.1f, 0.0f, near, 3, 5, 0.7f));
            Assert.AreEqual(4, FlowerPollenMath.PickOrgan(0.1f, 0.99f, near, 3, 5, 0.7f));
            // r1 >= nearShare: herhangi organ
            Assert.AreEqual(0, FlowerPollenMath.PickOrgan(0.9f, 0.0f, near, 3, 5, 0.7f));
            Assert.AreEqual(4, FlowerPollenMath.PickOrgan(0.9f, 0.99f, near, 3, 5, 0.7f));
            // Yakın organ yoksa her zaman herhangi organ
            Assert.AreEqual(1, FlowerPollenMath.PickOrgan(0.0f, 0.3f, near, 0, 5, 0.7f));

            // Dağılım: 1000 çekimde ~%70+ yakın gruptan
            int fromNear = 0;
            var rng = new System.Random(5);
            for (int i = 0; i < 1000; i++)
            {
                int o = FlowerPollenMath.PickOrgan((float)rng.NextDouble(), (float)rng.NextDouble(), near, 3, 5, 0.7f);
                if (o == 2 || o == 3 || o == 4) fromNear++;
            }
            Assert.That(fromNear / 1000f, Is.InRange(0.7f, 0.92f));
        }

        [Test]
        public void EmitDirection_MixesRadialAndPlayer_WithinCone_AndIsUnit()
        {
            Vector2 radial = Vector2.up, toPlayer = Vector2.right;
            Vector2 noBias = FlowerPollenMath.EmitDirection(radial, toPlayer, 0f, 70f, 0f);
            Assert.That(Vector2.Angle(noBias, radial), Is.LessThan(0.01f));
            Vector2 full = FlowerPollenMath.EmitDirection(radial, toPlayer, 1f, 70f, 0f);
            Assert.That(Vector2.Angle(full, toPlayer), Is.LessThan(0.01f));
            Vector2 half = FlowerPollenMath.EmitDirection(radial, toPlayer, 0.5f, 70f, 0f);
            Assert.That(Vector2.Angle(half, radial), Is.EqualTo(45f).Within(0.01f));
            Assert.That(half.magnitude, Is.EqualTo(1f).Within(1e-5f));

            // Koni: ±coneDeg içinde, rastgele uçlarda tam sınır
            Vector2 edge = FlowerPollenMath.EmitDirection(radial, toPlayer, 0f, 70f, 1f);
            Assert.That(Vector2.Angle(edge, radial), Is.EqualTo(70f).Within(0.01f));
            Vector2 edgeNeg = FlowerPollenMath.EmitDirection(radial, toPlayer, 0f, 70f, -1f);
            Assert.That(Vector2.Angle(edgeNeg, radial), Is.EqualTo(70f).Within(0.01f));
            Assert.That(Vector2.SignedAngle(radial, edge), Is.Not.EqualTo(Vector2.SignedAngle(radial, edgeNeg)).Within(1f));

            // Radyal ile oyuncu tam zıt: NaN yok
            Vector2 opposite = FlowerPollenMath.EmitDirection(Vector2.up, Vector2.down, 0.5f, 70f, 0f);
            Assert.IsFalse(float.IsNaN(opposite.x) || float.IsNaN(opposite.y));
            Assert.That(opposite.magnitude, Is.EqualTo(1f).Within(1e-5f));
            // Sıfır vektörler
            Vector2 zero = FlowerPollenMath.EmitDirection(Vector2.zero, Vector2.zero, 0.5f, 70f, 0f);
            Assert.That(zero.magnitude, Is.EqualTo(1f).Within(1e-5f));
        }

        [Test]
        public void Velocity_AddsCappedInheritedShare()
        {
            Vector2 v = FlowerPollenMath.Velocity(Vector2.right, 4f, new Vector2(0f, 10f), 0.2f, 3.5f);
            Assert.That(v.x, Is.EqualTo(4f).Within(1e-5f));
            Assert.That(v.y, Is.EqualTo(2f).Within(1e-5f), "10 x 0.2");
            Vector2 capped = FlowerPollenMath.Velocity(Vector2.right, 4f, new Vector2(0f, 100f), 0.2f, 3.5f);
            Assert.That(capped.y, Is.EqualTo(3.5f).Within(1e-5f), "pay üst sınırlı");
            Assert.That(FlowerPollenMath.Velocity(Vector2.up, 3f, Vector2.zero, 0.2f, 3.5f), Is.EqualTo(new Vector2(0f, 3f)));
        }

        [Test]
        public void PickWeighted_FollowsWeights_AndZeroWeightsNeverPicked()
        {
            var w = new[] { 0.3f, 0.05f, 0f, 0.4f, 0.25f };
            Assert.AreEqual(0, FlowerPollenMath.PickWeighted(0f, w));
            Assert.AreEqual(4, FlowerPollenMath.PickWeighted(1f, w));
            var counts = new int[5];
            var rng = new System.Random(11);
            for (int i = 0; i < 5000; i++) counts[FlowerPollenMath.PickWeighted((float)rng.NextDouble(), w)]++;
            Assert.AreEqual(0, counts[2], "ağırlığı 0 olan seçilmez");
            Assert.That(counts[3] / 5000f, Is.EqualTo(0.4f).Within(0.04f));
            Assert.That(counts[0] / 5000f, Is.EqualTo(0.3f).Within(0.04f));
            Assert.AreEqual(0, FlowerPollenMath.PickWeighted(0.5f, new[] { 0f, 0f }), "toplam 0: 0");
        }

        [Test]
        public void SettingsDefaults_AreConsistent()
        {
            var so = ScriptableObject.CreateInstance<FlowerPollenSettingsSO>();
            try
            {
                Assert.AreEqual(3, so.size.Length);
                Assert.AreEqual(3, so.speed.Length);
                Assert.That(so.gravity, Is.InRange(0f, 0.5f));
                Assert.That(so.ellipseAspect, Is.InRange(0.3f, 1f));
                Assert.AreEqual(so.palette.Length, so.dotColorWeights.Length);
                Assert.AreEqual(so.palette.Length, so.ellipseColorWeights.Length);
                Assert.AreEqual(so.palette.Length, so.pieceColorWeights.Length);
                foreach (PollenKind k in new[] { PollenKind.Dot, PollenKind.Ellipse, PollenKind.Piece })
                {
                    Assert.That(so.SizeRange(k).y, Is.GreaterThanOrEqualTo(so.SizeRange(k).x));
                    Assert.That(so.SpeedRange(k).y, Is.GreaterThanOrEqualTo(so.SpeedRange(k).x));
                }
                Assert.That(so.SizeRange(PollenKind.Piece).x, Is.GreaterThan(so.SizeRange(PollenKind.Dot).y), "parçalar noktalardan büyük");
                Assert.That(so.dotShare + so.ellipseShare, Is.LessThanOrEqualTo(1f));
                Assert.That(so.count.y, Is.GreaterThanOrEqualTo(so.count.x));
                Assert.That(so.maxParticles, Is.GreaterThanOrEqualTo(so.count.y * 2), "iki patlama + kalıntı sığmalı");
                // Amber (uyarıya ayrılmış) ve pembe yok: palet rengi amber tonuna yakın olmasın
                var amber = new Color(1f, 0.788f, 0.302f);
                foreach (var c in so.palette) Assert.That(Mathf.Abs(c.b - amber.b) + Mathf.Abs(c.g - amber.g), Is.GreaterThan(0.15f));
            }
            finally { Object.DestroyImmediate(so); }
        }
    }
}
