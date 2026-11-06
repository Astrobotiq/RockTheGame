using NUnit.Framework;
using New_Scripts.Multiplayer.Dev;
using New_Scripts.Multiplayer.Transport;
using UnityEngine;

namespace RockTheGame.Tests.Multiplayer
{
    /// <summary>
    /// Geliştirici başlatıcısı: kapalıyken hiçbir şey oluşturmaz (tek kişilik oyun aynen), açıkken kökte tek nesne kurar.
    /// docs/superpowers/plans/2026-11-06-mp3-unity-transport.md → Task 2.
    /// </summary>
    public class MultiplayerDevLauncherTests
    {
        private bool _previousEnabled;
        private MultiplayerDevLauncher _spawned;

        [SetUp]
        public void SetUp() => _previousEnabled = MultiplayerDevSettings.Enabled;

        [TearDown]
        public void TearDown()
        {
            if (_spawned != null)
            {
                _spawned.Transport?.Dispose();
                Object.DestroyImmediate(_spawned.gameObject);
            }
            MultiplayerDevSettings.Enabled = _previousEnabled;
        }

        [Test]
        public void SpawnIfEnabled_Disabled_CreatesNothing()
        {
            MultiplayerDevSettings.Enabled = false;

            _spawned = MultiplayerDevLauncher.SpawnIfEnabled();

            Assert.IsNull(_spawned);
            Assert.IsNull(GameObject.Find(MultiplayerDevLauncher.ObjectName));
        }

        [Test]
        public void ResolveRole_Disabled_IsOff()
        {
            MultiplayerDevSettings.Enabled = false;
            Assert.AreEqual(DevRole.Off, MultiplayerDevSettings.ResolveRole());
        }

        [Test]
        public void ResolveRole_EnabledInMainEditor_IsHost()
        {
            MultiplayerDevSettings.Enabled = true;
            Assert.AreEqual(DevRole.Host, MultiplayerDevSettings.ResolveRole());
        }

        [Test]
        public void Spawn_Host_CreatesRootObjectWithSessionAndListens()
        {
            _spawned = MultiplayerDevLauncher.Spawn(DevRole.Host);

            Assert.IsNotNull(_spawned);
            Assert.IsNull(_spawned.transform.parent);
            Assert.AreEqual(MultiplayerDevLauncher.ObjectName, _spawned.gameObject.name);
            Assert.IsNotNull(_spawned.Session);
            Assert.AreEqual(TransportState.Connecting, _spawned.Transport.State);
        }

        [Test]
        public void OnDisable_DisposesTransport()
        {
            // Play Mode'da script yeniden derlenince OnDestroy çağrılmaz ama OnDisable çağrılır; sürücü ve port orada bırakılmalı.
            _spawned = MultiplayerDevLauncher.Spawn(DevRole.Host);
            var onDisable = typeof(MultiplayerDevLauncher).GetMethod("OnDisable",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            Assert.IsNotNull(onDisable, "OnDisable yok");

            onDisable.Invoke(_spawned, null);

            Assert.IsNull(_spawned.Transport);
        }

        [Test]
        public void Spawn_Off_CreatesNothing()
        {
            _spawned = MultiplayerDevLauncher.Spawn(DevRole.Off);
            Assert.IsNull(_spawned);
        }
    }
}
