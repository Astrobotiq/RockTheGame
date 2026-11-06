using New_Scripts.Player;
using NUnit.Framework;
using UnityEngine;

namespace RockTheGame.Tests
{
    /// <summary>
    /// docs/bozulmamali-davranislar.md D3: oyun, respawn'da oldugu gibi dash hakki dolu baslar.
    /// </summary>
    public class PlayerStartStateTests
    {
        private GameObject _go;

        [TearDown]
        public void TearDown()
        {
            if (_go != null) Object.DestroyImmediate(_go);
        }

        [Test]
        public void Player_StartsWithDashCharge()
        {
            _go = new GameObject("PlayerUnderTest");

            var player = _go.AddComponent<PlayerController>();

            Assert.IsTrue(player.HasDashCharge);
        }
    }
}
