using New_Scripts.UI.Theme;
using NUnit.Framework;

namespace RockTheGame.Tests
{
    public class UiSpringTests
    {
        [Test]
        public void Kick_OvershootsThenSettlesAtZero()
        {
            var spring = new UiSpring();
            spring.Kick(10f);
            float max = 0f, min = 0f;
            for (int i = 0; i < 60; i++) { spring.Step(1f / 60f); max = System.Math.Max(max, spring.Value); min = System.Math.Min(min, spring.Value); }
            Assert.Greater(max, 0.1f, "ittiği yöne gitmeli");
            Assert.Less(min, -0.01f, "sönümlü ama geri aşmalı");
            for (int i = 0; i < 240; i++) spring.Step(1f / 60f);
            Assert.IsTrue(spring.IsRest, "birkaç saniyede durmalı");
        }

        [Test]
        public void Kicks_Stack()
        {
            var spring = new UiSpring();
            spring.Kick(3f);
            spring.Kick(-3f);
            spring.Step(1f / 60f);
            Assert.AreEqual(0f, spring.Value, 1e-5f);
        }

        [Test]
        public void Target_SettlesExactlyOnTheTarget_AfterOvershoot()
        {
            var spring = new UiSpring { Target = 5f };
            float max = 0f;
            for (int i = 0; i < 60; i++) { spring.Step(1f / 60f); max = System.Math.Max(max, spring.Value); }
            Assert.Greater(max, 5f, "hedefi biraz aşmalı (canlılık)");
            for (int i = 0; i < 300; i++) spring.Step(1f / 60f);
            Assert.AreEqual(5f, spring.Value, 1e-3f, "her seferinde aynı pozda durmalı");
        }
    }
}
