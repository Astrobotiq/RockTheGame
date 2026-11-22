using System.Collections;
using System.Reflection;
using New_Scripts.Flower;
using New_Scripts.UI.Theme;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class FlowerReactMathTests
    {
        private const float Half = 50f * Mathf.Deg2Rad;
        private const float MaxYaw = 12f * Mathf.Deg2Rad;
        private const BindingFlags NonPublic = BindingFlags.NonPublic | BindingFlags.Instance;

        // --- Falloff / açı yardımcıları ---

        [Test]
        public void Falloff_OneAtCenter_ZeroAtRadiusAndBeyond_MonotonicInRange()
        {
            Assert.That(FlowerReactMath.Falloff(0f, 2f), Is.EqualTo(1f).Within(1e-6f));
            Assert.That(FlowerReactMath.Falloff(2f, 2f), Is.EqualTo(0f).Within(1e-6f));
            Assert.That(FlowerReactMath.Falloff(5f, 2f), Is.EqualTo(0f));
            Assert.That(FlowerReactMath.Falloff(1f, 0f), Is.EqualTo(0f), "yarıçap 0 ise etki yok");
            float prev = 1f;
            for (int i = 0; i <= 40; i++)
            {
                float f = FlowerReactMath.Falloff(i * 0.05f, 2f);
                Assert.That(f, Is.InRange(0f, 1f));
                Assert.That(f, Is.LessThanOrEqualTo(prev + 1e-6f));
                prev = f;
            }
        }

        [Test]
        public void WrapPi_StaysInRange_AndKeepsTheAngle()
        {
            for (float a = -20f; a <= 20f; a += 0.37f)
            {
                float w = FlowerReactMath.WrapPi(a);
                Assert.That(w, Is.InRange(-Mathf.PI - 1e-4f, Mathf.PI + 1e-4f));
                Assert.That(Mathf.Cos(w), Is.EqualTo(Mathf.Cos(a)).Within(1e-4f));
                Assert.That(Mathf.Sin(w), Is.EqualTo(Mathf.Sin(a)).Within(1e-4f));
            }
        }

        [Test]
        public void UnwrapAngle_NeverJumpsMoreThanPi_AcrossTheSeam()
        {
            // Yön yayı ±π geçişinde ters yöne tam tur dönmesin.
            float current = 3.0f;
            float target = -3.0f;   // dünyada 0,28 rad ötede (π'yi aşarak)
            float unwrapped = FlowerReactMath.UnwrapAngle(current, target);
            Assert.That(Mathf.Abs(unwrapped - current), Is.LessThan(0.5f));
            Assert.That(Mathf.Cos(unwrapped), Is.EqualTo(Mathf.Cos(target)).Within(1e-4f));

            // Sürekli dönen hedef: açılmış açı sürekli artar, sıçramaz.
            float follow = 0f;
            float last = 0f;
            for (int i = 0; i < 400; i++)
            {
                float targetAngle = FlowerReactMath.WrapPi(i * 0.05f);   // sarılı hedef
                follow = FlowerReactMath.UnwrapAngle(follow, targetAngle);
                Assert.That(follow - last, Is.InRange(-1e-4f, 0.06f), $"adım {i}: sıçrama");
                last = follow;
            }
        }

        // --- Kol süpürmesi ---

        [Test]
        public void PartingYaw_IsOddAroundTheArm()
        {
            float arm = 0.7f;
            for (float d = 0.02f; d < Half; d += 0.02f)
            {
                float plus = FlowerReactMath.PartingYaw(arm + d, arm, Half, MaxYaw);
                float minus = FlowerReactMath.PartingYaw(arm - d, arm, Half, MaxYaw);
                Assert.That(plus, Is.EqualTo(-minus).Within(1e-5f), $"d={d}");
                Assert.That(plus, Is.GreaterThanOrEqualTo(0f), "saat yönünün tersindeki yaprak koldan uzaklaşır (pozitif)");
            }
        }

        [Test]
        public void PartingYaw_ZeroUnderTheArm_ZeroBeyondHalfWidth_BoundedByMax_PeaksAtAThird()
        {
            Assert.That(FlowerReactMath.PartingYaw(1f, 1f, Half, MaxYaw), Is.EqualTo(0f).Within(1e-6f));
            Assert.That(FlowerReactMath.PartingYaw(1f + Half, 1f, Half, MaxYaw), Is.EqualTo(0f).Within(1e-6f));
            Assert.That(FlowerReactMath.PartingYaw(1f + Half + 0.5f, 1f, Half, MaxYaw), Is.EqualTo(0f));
            float peak = 0f;
            float peakAt = 0f;
            for (float d = 0f; d <= Half; d += 0.001f)
            {
                float y = FlowerReactMath.PartingYaw(d, 0f, Half, MaxYaw);
                Assert.That(Mathf.Abs(y), Is.LessThanOrEqualTo(MaxYaw + 1e-5f));
                if (y > peak) { peak = y; peakAt = d; }
            }
            Assert.That(peak, Is.EqualTo(MaxYaw).Within(MaxYaw * 0.01f), "tepe maxYaw");
            Assert.That(peakAt, Is.EqualTo(Half / 3f).Within(0.01f), "tepe yarım genişliğin üçte birinde");
        }

        [Test]
        public void PartingYaw_IsContinuousAcrossThePiSeam()
        {
            // Kol π civarında, yaprak -π tarafında: sarma sıçraması olmamalı.
            float arm = Mathf.PI - 0.1f;
            float prev = float.NaN;
            for (float a = 2.6f; a <= 2.6f + 1.2f; a += 0.005f)
            {
                float y = FlowerReactMath.PartingYaw(FlowerReactMath.WrapPi(a), arm, Half, MaxYaw);
                if (!float.IsNaN(prev)) Assert.That(Mathf.Abs(y - prev), Is.LessThan(0.01f), $"a={a}");
                prev = y;
            }
        }

        [Test]
        public void PartingDip_IsEvenAndNonNegative_AndStrongestUnderTheArm()
        {
            float maxDip = 10f * Mathf.Deg2Rad;
            Assert.That(FlowerReactMath.PartingDip(0.5f, 0.5f, Half, maxDip), Is.EqualTo(maxDip).Within(1e-6f));
            for (float d = 0.01f; d < Half + 0.2f; d += 0.03f)
            {
                float plus = FlowerReactMath.PartingDip(0.5f + d, 0.5f, Half, maxDip);
                float minus = FlowerReactMath.PartingDip(0.5f - d, 0.5f, Half, maxDip);
                Assert.That(plus, Is.EqualTo(minus).Within(1e-5f));
                Assert.That(plus, Is.InRange(0f, maxDip));
            }
            Assert.That(FlowerReactMath.PartingDip(0.5f + Half, 0.5f, Half, maxDip), Is.EqualTo(0f).Within(1e-6f));
        }

        [Test]
        public void FivePetals_FiftyDegreeHalfWidth_CoverEveryArmAngle()
        {
            // 5 yaprak 72° arayla: en uzak komşu 36°, yarım genişlik 50° ile kol her açıda en az bir yaprağı etkiler.
            const int n = 5;
            for (float armDeg = 0f; armDeg < 360f; armDeg += 1f)
            {
                float arm = armDeg * Mathf.Deg2Rad;
                float best = 0f;
                for (int i = 0; i < n; i++)
                {
                    float az = i * 2f * Mathf.PI / n;
                    float influence = Mathf.Max(Mathf.Abs(FlowerReactMath.PartingYaw(az, arm, Half, MaxYaw)),
                                                FlowerReactMath.PartingDip(az, arm, Half, 10f * Mathf.Deg2Rad));
                    best = Mathf.Max(best, influence);
                }
                Assert.That(best, Is.GreaterThan(0.02f), $"kol {armDeg}° civarında hiçbir yaprak etkilenmiyor");
            }
        }

        // --- Gövde yakınlığı ---

        [Test]
        public void BodyYaw_PushesAwayFromTheBody_SignMatchesSide()
        {
            Vector2 tip = new Vector2(3f, 0f);
            Vector2 dir = Vector2.right;   // yaprak +X'e bakıyor, saat yönünün tersi teğet +Y
            // Gövde ucun altında (-Y tarafı): uç +Y'ye itilir, yani saat yönünün tersine (pozitif).
            Assert.That(FlowerReactMath.BodyYaw(tip, tip + Vector2.down * 0.5f, dir, 2f, 0.14f), Is.GreaterThan(0.01f));
            // Gövde ucun üstünde: uç -Y'ye itilir (negatif).
            Assert.That(FlowerReactMath.BodyYaw(tip, tip + Vector2.up * 0.5f, dir, 2f, 0.14f), Is.LessThan(-0.01f));
            // Gövde yaprak ekseninde (uç ile aynı doğrultu): teğete dik, yaw 0.
            Assert.That(FlowerReactMath.BodyYaw(tip, tip + Vector2.right * 0.5f, dir, 2f, 0.14f), Is.EqualTo(0f).Within(1e-6f));
        }

        [Test]
        public void BodyYaw_FadesWithDistance_AndRespectsSpeedScale()
        {
            Vector2 tip = new Vector2(3f, 0f);
            float near = Mathf.Abs(FlowerReactMath.BodyYaw(tip, tip + Vector2.down * 0.5f, Vector2.right, 2f, 0.14f));
            float far = Mathf.Abs(FlowerReactMath.BodyYaw(tip, tip + Vector2.down * 1.8f, Vector2.right, 2f, 0.14f));
            float gone = FlowerReactMath.BodyYaw(tip, tip + Vector2.down * 2.5f, Vector2.right, 2f, 0.14f);
            Assert.That(near, Is.GreaterThan(far));
            Assert.That(gone, Is.EqualTo(0f));
            float slow = Mathf.Abs(FlowerReactMath.BodyYaw(tip, tip + Vector2.down * 0.5f, Vector2.right, 2f, 0.14f, 0.3f));
            Assert.That(slow, Is.EqualTo(near * 0.3f).Within(1e-5f));
            Assert.That(Mathf.Abs(FlowerReactMath.BodyYaw(tip, tip, Vector2.right, 2f, 0.14f)), Is.EqualTo(0f), "üst üste: bölme yok");
        }

        [Test]
        public void SpeedScale_ClampedBetweenMinAndOne()
        {
            Assert.That(FlowerReactMath.SpeedScale(0f, 10f), Is.EqualTo(0.3f));
            Assert.That(FlowerReactMath.SpeedScale(5f, 10f), Is.EqualTo(0.5f).Within(1e-6f));
            Assert.That(FlowerReactMath.SpeedScale(-50f, 10f), Is.EqualTo(1f), "tavan; yön önemsiz");
            Assert.That(FlowerReactMath.SpeedScale(3f, 0f), Is.EqualTo(1f), "referans yoksa tam");
        }

        [Test]
        public void Combine_TakesTheLargerMagnitude_NotTheSum()
        {
            Assert.That(FlowerReactMath.Combine(0.1f, -0.3f), Is.EqualTo(-0.3f));
            Assert.That(FlowerReactMath.Combine(0.3f, -0.1f), Is.EqualTo(0.3f));
            Assert.That(FlowerReactMath.Combine(0f, 0f), Is.EqualTo(0f));
        }

        // --- Kapı ---

        [Test]
        public void Gate_ClosedOrUngrabbableIsSilent_OpensMonotonically_AndWarningShrinksIt()
        {
            Assert.That(FlowerReactMath.Gate(false, 1f, 0f), Is.EqualTo(0f), "tutulamazken sessiz");
            Assert.That(FlowerReactMath.Gate(true, 0f, 0f), Is.EqualTo(0f), "kapalı tomurcuk sessiz");
            Assert.That(FlowerReactMath.Gate(true, 0.5f, 0f), Is.EqualTo(0f).Within(1e-6f));
            Assert.That(FlowerReactMath.Gate(true, 1f, 0f), Is.EqualTo(1f).Within(1e-6f));
            Assert.That(FlowerReactMath.Gate(true, 1.12f, 0f), Is.EqualTo(1f).Within(1e-6f), "overshoot'ta sıçramaz");
            float prev = 0f;
            for (int i = 0; i <= 20; i++)
            {
                float g = FlowerReactMath.Gate(true, i * 0.05f, 0f);
                Assert.That(g, Is.GreaterThanOrEqualTo(prev - 1e-6f));
                prev = g;
            }
            Assert.That(FlowerReactMath.Gate(true, 1f, 1f), Is.EqualTo(0.5f).Within(1e-6f), "uyarıda yarıya iner");
        }

        // --- Yaylar ---

        private static void Overshoot(float k, float c, out float overshoot, out float settleTime, float fps = 240f)
        {
            var s = new UiSpring(k, c) { Value = 0f, Target = 1f };
            float dt = 1f / fps;
            float max = 0f;
            settleTime = 0f;
            for (float t = 0f; t < 3f; t += dt)
            {
                s.Step(dt);
                max = Mathf.Max(max, s.Value);
                if (Mathf.Abs(s.Value - 1f) > 0.02f) settleTime = t + dt;
            }
            overshoot = max - 1f;
        }

        [Test]
        public void PetalSpring_ReadsAsPaperNotRubber()
        {
            Overshoot(FlowerReactMath.PetalStiffness, FlowerReactMath.PetalDamping, out float over, out float settle);
            float zeta = FlowerReactMath.PetalDamping / (2f * Mathf.Sqrt(FlowerReactMath.PetalStiffness));
            Assert.That(over, Is.InRange(0.10f, 0.25f), "aşım bandı");
            Assert.That(settle, Is.LessThan(0.6f), "yerleşme");
            Assert.That(zeta, Is.GreaterThanOrEqualTo(0.45f), "ζ ≥ 0,45: lastik değil");
        }

        [Test]
        public void GripAndDirectionSprings_AreWellDamped()
        {
            Overshoot(FlowerReactMath.GripStiffness, FlowerReactMath.GripDamping, out float gripOver, out float gripSettle);
            Assert.That(gripOver, Is.LessThan(0.08f), "kavrama az aşar");
            Assert.That(gripSettle, Is.LessThan(0.3f), "kavrama hızlı");

            Overshoot(FlowerReactMath.GripStiffness, FlowerReactMath.GripReleaseDamping, out float releaseOver, out _);
            Assert.That(releaseOver, Is.InRange(0.15f, 0.32f), "bırakışta çırpınma");

            Overshoot(FlowerReactMath.DirectionStiffness, FlowerReactMath.DirectionDamping, out float dirOver, out float dirSettle);
            Assert.That(dirOver, Is.LessThan(0.08f), "yön yayı sakin");
            Assert.That(dirSettle, Is.LessThan(0.7f));
        }

        [Test]
        public void PetalSpring_SameTrajectoryAt30_60_144Fps()
        {
            float Value(float fps, float at)
            {
                var s = new UiSpring(FlowerReactMath.PetalStiffness, FlowerReactMath.PetalDamping) { Value = 0f, Target = 1f };
                float dt = 1f / fps;
                for (float t = 0f; t < at - 1e-5f; t += dt) s.Step(dt);
                return s.Value;
            }

            foreach (float at in new[] { 0.1f, 0.2f, 0.3f, 0.5f })
            {
                float reference = Value(240f, at);
                foreach (float fps in new[] { 30f, 60f, 144f })
                    Assert.That(Value(fps, at), Is.EqualTo(reference).Within(0.05f), $"{fps} fps, t={at}");
            }
        }

        // --- Azimut çevirme: işaret tuzağı, gerçek yaprak pivotlarıyla ---

        private static (FlowerBloom bloom, Transform model, Transform[] pivots) BuildFlower(float rootZDeg, float orientationDeg, float open)
        {
            var go = new GameObject("FlowerReactTest");
            go.transform.rotation = Quaternion.Euler(0f, 0f, rootZDeg);
            go.transform.position = new Vector3(2f, -1f, 0f);
            var bloom = go.AddComponent<FlowerBloom>();
            typeof(FlowerBloom).GetField("orientationDeg", NonPublic).SetValue(bloom, orientationDeg);
            bloom.Open = open;
            typeof(FlowerBloom).GetMethod("Apply", NonPublic).Invoke(bloom, null);

            var model = (Transform)typeof(FlowerBloom).GetField("model", NonPublic).GetValue(bloom);
            var list = (IList)typeof(FlowerBloom).GetField("petals", NonPublic).GetValue(bloom);
            var pivots = new Transform[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                var petal = list[i];
                int index = (int)petal.GetType().GetField("Index").GetValue(petal);
                pivots[index] = (Transform)petal.GetType().GetField("Pivot").GetValue(petal);
            }
            return (bloom, model, pivots);
        }

        private static float PetalAzimuth(int index, int count) => index * 2f * Mathf.PI / count;

        private static Vector2 WorldDirOf(Transform model, Transform pivot)
        {
            Vector3 d = pivot.position - model.position;
            return new Vector2(d.x, d.y);
        }

        [Test]
        public void WorldToLocalAzimuth_MatchesTheRealPetalPivots_ForAnyOrientation()
        {
            foreach (float rootZ in new[] { 0f, 37f, -120f, 200f })
            foreach (float orientation in new[] { -90f, 0f, 55f })
            foreach (float open in new[] { 1f, 0.3f })   // açılma dönüşü (twist) de değişsin
            {
                var (bloom, model, pivots) = BuildFlower(rootZ, orientation, open);
                try
                {
                    Assert.AreEqual(5, pivots.Length);
                    for (int i = 0; i < pivots.Length; i++)
                    {
                        Vector2 world = WorldDirOf(model, pivots[i]);
                        Assert.Greater(world.magnitude, 1e-3f, "pivot merkezde olmamalı");
                        float az = FlowerReactMath.WorldToLocalAzimuth(model.rotation, world);
                        float expected = PetalAzimuth(i, pivots.Length);
                        Assert.That(Mathf.Abs(FlowerReactMath.WrapPi(az - expected)), Is.LessThan(1e-3f),
                            $"rootZ={rootZ} orient={orientation} open={open} yaprak {i}: bulundu {az:F3}, beklenen {expected:F3}");
                    }
                }
                finally { Object.DestroyImmediate(bloom.gameObject); }
            }
        }

        [Test]
        public void PivotYawDegrees_ReproducesTheBuiltRotation_AndPositiveYawTurnsTheTipCounterClockwiseInTheWorld()
        {
            foreach (float rootZ in new[] { 0f, 77f, -150f })
            {
                var (bloom, model, pivots) = BuildFlower(rootZ, -90f, 1f);
                try
                {
                    for (int i = 0; i < pivots.Length; i++)
                    {
                        float az = PetalAzimuth(i, pivots.Length);

                        // Rebuild'in kurduğu duruş: yaw 0 ile aynı.
                        Quaternion built = pivots[i].localRotation;
                        Quaternion expected = Quaternion.Euler(0f, FlowerReactMath.PivotYawDegrees(az, 0f), 0f);
                        Assert.That(Quaternion.Angle(built, expected), Is.LessThan(0.01f), $"yaprak {i}: kurulu duruş");

                        // Pozitif yaw: yaprağın dünyadaki yönü saat yönünün tersine döner (açı artar).
                        Vector3 before = pivots[i].TransformDirection(Vector3.right);
                        pivots[i].localRotation = Quaternion.Euler(0f, FlowerReactMath.PivotYawDegrees(az, 0.2f), 0f);
                        Vector3 after = pivots[i].TransformDirection(Vector3.right);
                        float delta = Vector2.SignedAngle(new Vector2(before.x, before.y), new Vector2(after.x, after.y));
                        Assert.That(delta, Is.EqualTo(0.2f * Mathf.Rad2Deg).Within(0.5f),
                            $"rootZ={rootZ} yaprak {i}: +0.2 rad yaw dünyada saat yönünün tersine {0.2f * Mathf.Rad2Deg:F1}° olmalı, ölçülen {delta:F1}°");
                    }
                }
                finally { Object.DestroyImmediate(bloom.gameObject); }
            }
        }

        [Test]
        public void LocalYawSign_AgreesWithBodyAndPartingSigns_InTheWorld()
        {
            // Dünyada: kol yaprağın saat yönündeki yanında (yaprağa göre -teğet tarafı) ise yaprak saat yönünün tersine itilmeli.
            var (bloom, model, pivots) = BuildFlower(63f, -90f, 1f);
            try
            {
                int i = 2;
                Vector2 petalWorld = WorldDirOf(model, pivots[i]).normalized;
                float petalLocalAz = PetalAzimuth(i, pivots.Length);

                // Kolu yaprağın 20° saat yönünde (dünya) konumlandır.
                float armWorldDeg = Mathf.Atan2(petalWorld.y, petalWorld.x) * Mathf.Rad2Deg - 20f;
                Vector2 armDir = new Vector2(Mathf.Cos(armWorldDeg * Mathf.Deg2Rad), Mathf.Sin(armWorldDeg * Mathf.Deg2Rad));
                float armLocalAz = FlowerReactMath.WorldToLocalAzimuth(model.rotation, armDir);

                float yaw = FlowerReactMath.PartingYaw(petalLocalAz, armLocalAz, Half, MaxYaw);
                Assert.That(yaw, Is.GreaterThan(0f), "kol saat yönünde ise yaprak saat yönünün tersine itilir");

                // Aynı yaprağı gövde aynı yandan itiyor: BodyYaw da pozitif (dünya vektörleriyle).
                Vector2 tip = petalWorld * 4f;
                Vector2 body = tip + new Vector2(petalWorld.y, -petalWorld.x) * 0.6f;   // saat yönü tarafı
                Assert.That(FlowerReactMath.BodyYaw(tip, body, petalWorld, 2f, 0.14f), Is.GreaterThan(0f));
            }
            finally { Object.DestroyImmediate(bloom.gameObject); }
        }
    }
}
