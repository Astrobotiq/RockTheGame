using New_Scripts.Tutorial;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    /// <summary>
    /// SwingTutorialFlow'un adım tablosu (docs/swing-tutorial-plani.md). Ayarlar SO varsayılanlarıdır:
    /// SlowScale 0.1, HoldScale 0, RampIn 0.15, RampOut 0.3, AimConfirm 0.2, AimLostGrace 0.3.
    /// </summary>
    public class SwingTutorialFlowTests
    {
        private SwingTutorialSettingsSO _settings;
        private SwingTutorialFlow _flow;

        [SetUp]
        public void SetUp()
        {
            _settings = ScriptableObject.CreateInstance<SwingTutorialSettingsSO>();
            _flow = new SwingTutorialFlow(_settings);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_settings);
        }

        // Havada, node erişilebilir, yükseliyor.
        private static SwingTutorialInput Rising(float dt = 0.02f) => new SwingTutorialInput
        {
            IsAirborne = true, NodeReachable = true, VelocityY = 5f, UnscaledDt = dt
        };

        private static SwingTutorialInput AtApex(bool aimed, float dt) => new SwingTutorialInput
        {
            IsAirborne = true, NodeReachable = true, VelocityY = 0f, AimedAtNode = aimed, UnscaledDt = dt
        };

        private void Tick(SwingTutorialInput input) => _flow.Tick(input);

        // Waiting → Slowing → Aim (rampa bitmiş, tepede).
        private void GoToAim()
        {
            Tick(Rising());
            Tick(Rising(0.2f));
            Tick(AtApex(false, 0.02f));
            Assert.AreEqual(SwingTutorialStep.Aim, _flow.Step, "yardımcı: Aim'e varılamadı");
        }

        private void GoToFire()
        {
            GoToAim();
            Tick(AtApex(true, 0.25f));
            Assert.AreEqual(SwingTutorialStep.Fire, _flow.Step, "yardımcı: Fire'a varılamadı");
        }

        private static SwingTutorialInput Anchored() => new SwingTutorialInput
        {
            AnchoredToNode = true, HasAnyAnchor = true, UnscaledDt = 0.02f
        };

        [Test]
        public void Waiting_OnGround_StaysWaitingAtNormalSpeed()
        {
            Tick(new SwingTutorialInput { IsAirborne = false, NodeReachable = true, UnscaledDt = 0.02f });

            Assert.AreEqual(SwingTutorialStep.Waiting, _flow.Step);
            Assert.AreEqual(1f, _flow.TargetTimeScale);
            Assert.IsFalse(_flow.IsControllingTime);
        }

        [Test]
        public void Waiting_AirborneButNodeUnreachable_StaysWaiting()
        {
            Tick(new SwingTutorialInput { IsAirborne = true, NodeReachable = false, VelocityY = 5f, UnscaledDt = 0.02f });

            Assert.AreEqual(SwingTutorialStep.Waiting, _flow.Step);
        }

        [Test]
        public void Waiting_AirborneWithAnchor_StaysWaiting()
        {
            Tick(new SwingTutorialInput
            {
                IsAirborne = true, NodeReachable = true, HasAnyAnchor = true, VelocityY = 5f, UnscaledDt = 0.02f
            });

            Assert.AreEqual(SwingTutorialStep.Waiting, _flow.Step);
        }

        [Test]
        public void Waiting_AirborneAndNodeReachable_StartsSlowing()
        {
            Tick(Rising());

            Assert.AreEqual(SwingTutorialStep.Slowing, _flow.Step);
            Assert.IsTrue(_flow.IsControllingTime);
            Assert.AreEqual(1f, _flow.TargetTimeScale, 1e-5f, "rampa şimdi başladı");
        }

        [Test]
        public void Slowing_RampsToSlowScaleOverRampInDuration()
        {
            Tick(Rising());
            Tick(Rising(0.075f)); // rampanın yarısı
            Assert.AreEqual(Mathf.Lerp(1f, 0.1f, 0.5f), _flow.TargetTimeScale, 1e-4f);

            Tick(Rising(0.5f)); // rampa bitti, hâlâ yükseliyor
            Assert.AreEqual(0.1f, _flow.TargetTimeScale, 1e-5f);
            Assert.AreEqual(SwingTutorialStep.Slowing, _flow.Step, "yükselirken Aim'e geçilmez");
        }

        [Test]
        public void Slowing_AtApexAfterRamp_GoesToAimAndHoldsTime()
        {
            Tick(Rising());
            Tick(Rising(0.2f));
            Tick(AtApex(false, 0.02f));

            Assert.AreEqual(SwingTutorialStep.Aim, _flow.Step);
            Assert.AreEqual(0f, _flow.TargetTimeScale, "HoldScale 0");
        }

        [Test]
        public void Slowing_FallingBeforeRampEnds_WaitsForRampBeforeAim()
        {
            // Kenardan yürüyerek düşüş: ilk karede zaten vy <= 0.
            Tick(AtApex(false, 0.02f));
            Assert.AreEqual(SwingTutorialStep.Slowing, _flow.Step);

            Tick(AtApex(false, 0.05f));
            Assert.AreEqual(SwingTutorialStep.Slowing, _flow.Step, "rampa bitmedi");

            Tick(AtApex(false, 0.2f));
            Assert.AreEqual(SwingTutorialStep.Aim, _flow.Step);
        }

        [Test]
        public void Aim_StickHeldForConfirmTime_GoesToFire()
        {
            GoToAim();

            Tick(AtApex(true, 0.15f));
            Assert.AreEqual(SwingTutorialStep.Aim, _flow.Step, "0.2 sn dolmadı");

            Tick(AtApex(true, 0.1f));
            Assert.AreEqual(SwingTutorialStep.Fire, _flow.Step);
            Assert.AreEqual(0f, _flow.TargetTimeScale, "Fire'da zaman hâlâ bekliyor");
        }

        [Test]
        public void Aim_StickReleasedBeforeConfirm_RestartsConfirmTimer()
        {
            GoToAim();

            Tick(AtApex(true, 0.15f));
            Tick(AtApex(false, 0.02f)); // kesinti sayacı sıfırlar
            Tick(AtApex(true, 0.15f));

            Assert.AreEqual(SwingTutorialStep.Aim, _flow.Step, "toplam 0.3 sn ama kesintili");
        }

        [Test]
        public void Fire_AnchoredToNode_FinishesAndRampsOut()
        {
            GoToFire();

            Tick(Anchored());

            Assert.AreEqual(SwingTutorialStep.Done, _flow.Step);
            Assert.IsTrue(_flow.IsControllingTime, "çıkış rampası sürüyor");
            Assert.AreEqual(0.1f, _flow.TargetTimeScale, 1e-5f, "rampa SlowScale'den başlar");

            Tick(new SwingTutorialInput { UnscaledDt = 0.15f });
            Assert.AreEqual(Mathf.Lerp(0.1f, 1f, 0.5f), _flow.TargetTimeScale, 1e-4f);

            Tick(new SwingTutorialInput { UnscaledDt = 0.5f });
            Assert.AreEqual(1f, _flow.TargetTimeScale, 1e-5f);
            Assert.IsFalse(_flow.IsControllingTime);
        }

        [Test]
        public void Slowing_AnchoredWhileRising_GoesStraightToDone()
        {
            Tick(Rising());
            Tick(Rising(0.2f));

            Tick(Anchored());

            Assert.AreEqual(SwingTutorialStep.Done, _flow.Step);
            Assert.AreEqual(0.1f, _flow.TargetTimeScale, 1e-5f);
        }

        [Test]
        public void Slowing_AnchoredMidRamp_RampsOutFromCurrentScale()
        {
            Tick(Rising());
            Tick(Rising(0.075f)); // scale ~0.55

            Tick(Anchored());

            Assert.AreEqual(Mathf.Lerp(1f, 0.1f, 0.5f), _flow.TargetTimeScale, 1e-4f, "SlowScale'e inmeden çıkış");
        }

        [Test]
        public void Aim_AnchoredBeforeFire_GoesStraightToDone()
        {
            GoToAim();

            Tick(Anchored());

            Assert.AreEqual(SwingTutorialStep.Done, _flow.Step);
        }

        [Test]
        public void Waiting_AnchoredToNode_FinishesWithoutSlowing()
        {
            Tick(Anchored());

            Assert.AreEqual(SwingTutorialStep.Done, _flow.Step);
            Assert.AreEqual(1f, _flow.TargetTimeScale);
            Assert.IsFalse(_flow.IsControllingTime);
        }

        [Test]
        public void Fire_AimLostWithinGrace_StaysInFire()
        {
            GoToFire();

            Tick(AtApex(false, 0.2f));
            Assert.AreEqual(SwingTutorialStep.Fire, _flow.Step, "0.3 sn tolerans dolmadı");

            Tick(AtApex(false, 0.05f));
            Assert.AreEqual(SwingTutorialStep.Fire, _flow.Step, "toplam 0.25 sn");
        }

        [Test]
        public void Fire_AimLostBeyondGrace_ReturnsToAim()
        {
            GoToFire();

            Tick(AtApex(false, 0.2f));
            Tick(AtApex(false, 0.15f));

            Assert.AreEqual(SwingTutorialStep.Aim, _flow.Step);
            Assert.AreEqual(0f, _flow.TargetTimeScale);
        }

        [Test]
        public void Fire_AimRegainedInsideGrace_ResetsGraceTimer()
        {
            GoToFire();

            Tick(AtApex(false, 0.25f));
            Tick(AtApex(true, 0.02f));   // nişan geri geldi
            Tick(AtApex(false, 0.25f));  // yeni 0.25 sn, sayaç sıfırdan

            Assert.AreEqual(SwingTutorialStep.Fire, _flow.Step);
        }

        [Test]
        public void Fire_ReturnToAim_RequiresConfirmTimeAgain()
        {
            GoToFire();
            Tick(AtApex(false, 0.35f)); // Aim'e döndü

            Tick(AtApex(true, 0.1f));

            Assert.AreEqual(SwingTutorialStep.Aim, _flow.Step, "onay süresi sıfırdan sayılır");
        }

        [Test]
        public void Paused_FreezesCountersAndTransitions()
        {
            GoToAim();
            Tick(AtApex(true, 0.15f)); // onay sayacı 0.15

            // Pause'da ne kadar unscaled zaman akarsa aksın sayaç ilerlemez.
            Tick(new SwingTutorialInput
            {
                IsAirborne = true, NodeReachable = true, AimedAtNode = true, UnscaledDt = 5f, Paused = true
            });
            Assert.AreEqual(SwingTutorialStep.Aim, _flow.Step);

            Tick(AtApex(true, 0.03f));
            Assert.AreEqual(SwingTutorialStep.Aim, _flow.Step, "0.15 + 0.03, pause süresi sayılmadı");

            Tick(AtApex(true, 0.03f));
            Assert.AreEqual(SwingTutorialStep.Fire, _flow.Step);
        }

        [Test]
        public void Paused_FreezesRampIn()
        {
            Tick(Rising());
            Tick(Rising(0.075f));
            float before = _flow.TargetTimeScale;

            Tick(new SwingTutorialInput
            {
                IsAirborne = true, NodeReachable = true, VelocityY = 5f, UnscaledDt = 10f, Paused = true
            });

            Assert.AreEqual(before, _flow.TargetTimeScale, 1e-6f);
            Assert.AreEqual(SwingTutorialStep.Slowing, _flow.Step);
        }

        [Test]
        public void Paused_FreezesRampOut()
        {
            GoToFire();
            Tick(Anchored());
            Tick(new SwingTutorialInput { UnscaledDt = 0.15f });
            float before = _flow.TargetTimeScale;

            Tick(new SwingTutorialInput { UnscaledDt = 10f, Paused = true });

            Assert.AreEqual(before, _flow.TargetTimeScale, 1e-6f);
            Assert.IsTrue(_flow.IsControllingTime);
        }

        [Test]
        public void Reset_MidFlow_ReturnsToWaitingAtNormalSpeed_AndReplays()
        {
            GoToFire();

            _flow.Reset();

            Assert.AreEqual(SwingTutorialStep.Waiting, _flow.Step);
            Assert.AreEqual(1f, _flow.TargetTimeScale);
            Assert.IsFalse(_flow.IsControllingTime);

            Tick(Rising());
            Assert.AreEqual(SwingTutorialStep.Slowing, _flow.Step, "bitmediyse tekrar oynar");
        }

        [Test]
        public void Reset_AfterDone_StaysDoneAndTimeNormal()
        {
            GoToFire();
            Tick(Anchored());

            _flow.Reset();

            Assert.AreEqual(SwingTutorialStep.Done, _flow.Step, "tutorial oturumda bir kez");
            Assert.AreEqual(1f, _flow.TargetTimeScale);
            Assert.IsFalse(_flow.IsControllingTime);

            Tick(Rising());
            Assert.AreEqual(SwingTutorialStep.Done, _flow.Step);
            Assert.AreEqual(1f, _flow.TargetTimeScale);
        }

        [Test]
        public void Done_NeverRestarts()
        {
            GoToFire();
            Tick(Anchored());
            Tick(new SwingTutorialInput { UnscaledDt = 1f });

            Tick(Rising());
            Tick(Rising(0.5f));

            Assert.AreEqual(SwingTutorialStep.Done, _flow.Step);
            Assert.AreEqual(1f, _flow.TargetTimeScale);
        }

        [Test]
        public void Slowing_PlayerLands_AbortsToWaitingAtNormalSpeed()
        {
            Tick(Rising());
            Tick(Rising(0.2f));

            Tick(new SwingTutorialInput { IsAirborne = false, NodeReachable = true, UnscaledDt = 0.02f });

            Assert.AreEqual(SwingTutorialStep.Waiting, _flow.Step);
            Assert.AreEqual(1f, _flow.TargetTimeScale);
            Assert.IsFalse(_flow.IsControllingTime);
        }

        [Test]
        public void Aim_PlayerLeavesNormalAir_Aborts_NoTimeLock()
        {
            // timeScale 0 iken oyuncu dash'e girerse FixedUpdate hiç çalışmaz: zaman bırakılmalı.
            GoToAim();

            Tick(new SwingTutorialInput { IsAirborne = false, NodeReachable = true, UnscaledDt = 0.02f });

            Assert.AreEqual(SwingTutorialStep.Waiting, _flow.Step);
            Assert.AreEqual(1f, _flow.TargetTimeScale);
        }

        [Test]
        public void Fire_AnchoredToOtherNode_Aborts()
        {
            GoToFire();

            Tick(new SwingTutorialInput { IsAirborne = true, HasAnyAnchor = true, NodeReachable = true, UnscaledDt = 0.02f });

            Assert.AreEqual(SwingTutorialStep.Waiting, _flow.Step);
            Assert.AreEqual(1f, _flow.TargetTimeScale);
        }

        [Test]
        public void Aim_NodeNoLongerReachable_Aborts_NoTimeLock()
        {
            GoToAim();

            Tick(new SwingTutorialInput { IsAirborne = true, NodeReachable = false, UnscaledDt = 0.02f });

            Assert.AreEqual(SwingTutorialStep.Waiting, _flow.Step);
            Assert.AreEqual(1f, _flow.TargetTimeScale);
        }

        [Test]
        public void Slowing_ApexReachedButNodeUnreachable_AbortsInsteadOfHolding()
        {
            Tick(Rising());
            Tick(Rising(0.2f));

            Tick(new SwingTutorialInput { IsAirborne = true, NodeReachable = false, VelocityY = 0f, UnscaledDt = 0.02f });

            Assert.AreEqual(SwingTutorialStep.Waiting, _flow.Step);
            Assert.AreEqual(1f, _flow.TargetTimeScale);
        }

        [Test]
        public void NegativeDt_IsTreatedAsZero()
        {
            Tick(Rising());
            float before = _flow.TargetTimeScale;

            Tick(Rising(-1f));

            Assert.AreEqual(before, _flow.TargetTimeScale, 1e-6f);
        }
    }
}
