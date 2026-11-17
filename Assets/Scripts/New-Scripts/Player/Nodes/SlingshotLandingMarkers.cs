using System.Collections.Generic;
using New_Scripts.Player.States;
using TMPro;
using UnityEngine;

namespace New_Scripts.Player.Nodes
{
    /// <summary>
    /// Test odası aracı: bu objenin altındaki bir node'dan slingshot yapılıp yere inilince gerçek iniş noktasına beyaz
    /// bir direk koyar; yanına da node bırakışta ±<see cref="ghostSpeed"/> b/sn yatay hızla hareket ediyor olsaydı
    /// aynı fırlatışın ineceği yerlere hayalet direkler (+X yeşil, −X turuncu) koyar.
    /// Hayalet hesabı NormalState'in taşınan hızıyla birebir: taşınan X yalnızca eklenir, havada
    /// MovingNodeCarryDecayTime içinde doğrusal söner (çarpan MovingNodeLaunchMultiplier). Yükseklik ve duvar
    /// çarpması hesaba katılmaz; düz zeminde doğrudur. Son <see cref="throwsToKeep"/> atış kalır, eskiler soluk.
    /// </summary>
    [DefaultExecutionOrder(100)] // PlayerController'ın FixedUpdate'inden sonra: o adımda uygulanan hızı sayar.
    public class SlingshotLandingMarkers : MonoBehaviour
    {
        [SerializeField] private float ghostSpeed = 10f;
        [SerializeField] private int throwsToKeep = 3;
        [SerializeField] private float markerHeight = 3f;

        private static readonly Color RealColor = Color.white;
        private static readonly Color PlusColor = new Color(0.3f, 1f, 0.4f);
        private static readonly Color MinusColor = new Color(1f, 0.55f, 0.2f);

        private PlayerController _player;
        private Transform _node;
        private bool _inSlingshot;
        private bool _tracking;
        private float _ghostCarried;
        private float _ghostDecayRate;
        private Material _lineMaterial;
        private readonly Queue<GameObject> _throws = new Queue<GameObject>();

        /// <summary>Son inişte hesaplanan +X hayaletin gerçek inişe göre yatay farkı (−X için eksi işaretlisi).</summary>
        public float LastGhostOffset { get; private set; }

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerController>();
        }

        private void FixedUpdate()
        {
            if (_player == null || _player.IsPaused) return; // hitstop/geçiş: oyuncu o adımda hareket etmedi

            if (_player.CurrentState is SlingshotState)
            {
                Transform node = _player.LeftAnchorNode != null ? _player.LeftAnchorNode : _player.RightAnchorNode;
                _inSlingshot = node != null && node.IsChildOf(transform);
                if (_inSlingshot) _node = node;
                _tracking = false;
                return;
            }

            if (_inSlingshot)
            {
                _inSlingshot = false;
                if (!_player.IsInNormalAir) return;

                // Çıkış adımı: NormalState.EnterState taşınan hızı sönmeden uyguladı.
                PlayerStatsSO stats = _player.Stats;
                _ghostCarried = ghostSpeed * stats.MovingNodeLaunchMultiplier;
                float decayTime = stats.MovingNodeCarryDecayTime;
                _ghostDecayRate = decayTime > 0f ? Mathf.Abs(_ghostCarried) / decayTime : 0f;
                LastGhostOffset = _ghostCarried * Time.fixedDeltaTime;
                _tracking = true;
                return;
            }

            if (!_tracking) return;

            if (_player.IsInNormalAir)
            {
                // NormalState.FixedUpdateAir: önce söndür, sonra uygula.
                _ghostCarried = Mathf.MoveTowards(_ghostCarried, 0f, _ghostDecayRate * Time.fixedDeltaTime);
                LastGhostOffset += _ghostCarried * Time.fixedDeltaTime;
            }
            else if (_player.IsInNormalGround)
            {
                SpawnThrow(_node, _player.PlayerRigidbody.position, LastGhostOffset);
                _tracking = false;
            }
            else
            {
                _tracking = false; // kanca, duvar, dash: iniş sayılmaz
            }
        }

        private void SpawnThrow(Transform node, Vector2 landing, float ghostOffset)
        {
            if (node == null) return;

            foreach (GameObject old in _throws) Fade(old);

            var group = new GameObject($"Throw {node.name}");
            group.transform.SetParent(transform, true);

            float nodeX = node.position.x;
            float y = landing.y;
            Spawn(group.transform, "Landing Real", new Vector2(landing.x, y), $"Normal\ndx {Format(landing.x - nodeX)}", RealColor, 0f);
            Spawn(group.transform, "Landing Ghost +X", new Vector2(landing.x + ghostOffset, y),
                $"+X {ghostSpeed:0}\n{Format(ghostOffset)}", PlusColor, 1.2f);
            Spawn(group.transform, "Landing Ghost -X", new Vector2(landing.x - ghostOffset, y),
                $"-X {ghostSpeed:0}\n{Format(-ghostOffset)}", MinusColor, 1.2f);

            _throws.Enqueue(group);
            while (_throws.Count > Mathf.Max(1, throwsToKeep))
                Destroy(_throws.Dequeue());
        }

        private void Spawn(Transform parent, string name, Vector2 point, string caption, Color color, float labelRaise)
        {
            var marker = new GameObject(name);
            marker.transform.SetParent(parent, true);
            marker.transform.position = point;

            var line = marker.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.SetPosition(0, point + Vector2.down * 0.5f);
            line.SetPosition(1, point + Vector2.up * markerHeight);
            line.startWidth = line.endWidth = 0.15f;
            line.material = LineMaterial();
            line.startColor = line.endColor = color;
            line.sortingOrder = 50;

            var label = new GameObject("Label");
            label.transform.SetParent(marker.transform, false);
            label.transform.localPosition = new Vector3(0f, markerHeight + 1f + labelRaise, 0f);
            var text = label.AddComponent<TextMeshPro>();
            text.text = caption;
            text.fontSize = 4f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;
            text.sortingOrder = 50;
        }

        private static void Fade(GameObject group)
        {
            if (group == null) return;
            foreach (var line in group.GetComponentsInChildren<LineRenderer>())
            {
                line.startColor = line.endColor = WithAlpha(line.startColor, 0.3f);
            }
            foreach (var text in group.GetComponentsInChildren<TextMeshPro>())
            {
                text.text = "";
            }
        }

        private static Color WithAlpha(Color c, float a) => new Color(c.r, c.g, c.b, a);

        private static string Format(float value) => value.ToString("+0.0;-0.0;0.0");

        private Material LineMaterial()
        {
            if (_lineMaterial != null) return _lineMaterial;
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            _lineMaterial = new Material(shader);
            return _lineMaterial;
        }

        private void OnDestroy()
        {
            if (_lineMaterial != null) Destroy(_lineMaterial);
        }
    }
}
