using New_Scripts.UI.Theme;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    public class UiMenuComponentsTests
    {
        private GameObject _owner;
        private GameObject _visual;

        [TearDown]
        public void TearDown()
        {
            if (_owner != null) Object.DestroyImmediate(_owner);
            if (_visual != null) Object.DestroyImmediate(_visual);
        }

        [Test]
        public void ExtrusionOffset_GrowsDownAndRightPerStep()
        {
            Assert.AreEqual(Vector2.zero, UiExtrudedText.OffsetFor(0, 4f));
            Vector2 five = UiExtrudedText.OffsetFor(5, 4f);
            Assert.AreEqual(16f, five.x, 1e-4f);
            Assert.AreEqual(-14f, five.y, 1e-4f);
        }

        [Test]
        public void SelectionVisual_FollowsSelectAndDeselect()
        {
            _owner = new GameObject("item");
            _visual = new GameObject("visual");
            var selection = _owner.AddComponent<UiSelectionVisual>();
            selection.Wire(_visual);

            selection.OnSelect(null);
            Assert.IsTrue(_visual.activeSelf, "odakta görsel açık olmalı");

            selection.OnDeselect(null);
            Assert.IsFalse(_visual.activeSelf, "odak gidince görsel kapanmalı");
        }
    }
}
