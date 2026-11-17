using System.Collections.Generic;
using New_Scripts.Player.States;
using TMPro;
using UnityEngine;

namespace New_Scripts.Player.Nodes
{
    /// <summary>
    /// Test odası aracı: bu objenin altındaki bir node'dan slingshot yapılıp yere inilince iniş noktasına renkli bir
    /// direk ve "node adı / dx" etiketi bırakır (dx = iniş x − node x). Renk node'un <see cref="SimulatedNodeVelocity"/>
    /// yönünden: +X yeşil, −X turuncu, yoksa beyaz. Her node için son <see cref="markersPerNode"/> iniş kalır.
    /// </summary>
    public class SlingshotLandingMarkers : MonoBehaviour
    {
        [SerializeField] private int markersPerNode = 4;
        [SerializeField] private float markerHeight = 3f;

        private PlayerController _player;
        private Transform _node;
        private bool _tracking;
        private Material _lineMaterial;
        private readonly Dictionary<Transform, Queue<GameObject>> _markers = new Dictionary<Transform, Queue<GameObject>>();

        private void Start()
        {
            _player = FindFirstObjectByType<PlayerController>();
        }

        private void Update()
        {
            if (_player == null) return;

            if (_player.CurrentState is SlingshotState)
            {
                Transform node = _player.LeftAnchorNode != null ? _player.LeftAnchorNode : _player.RightAnchorNode;
                if (node != null && node.IsChildOf(transform))
                {
                    _node = node;
                    _tracking = true;
                }
                return;
            }

            if (!_tracking) return;

            if (_player.IsInNormalGround)
            {
                Spawn(_node, _player.PlayerRigidbody.position);
                _tracking = false;
            }
            else if (!_player.IsInNormalAir)
            {
                _tracking = false; // kanca, duvar, dash: iniş sayılmaz
            }
        }

        private void Spawn(Transform node, Vector2 landing)
        {
            if (node == null) return;

            Color color = ColorFor(node);
            float dx = landing.x - node.position.x;

            var marker = new GameObject($"Landing {node.name}");
            marker.transform.SetParent(transform, true);
            marker.transform.position = landing;

            var line = marker.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 2;
            line.SetPosition(0, landing + Vector2.down * 0.5f);
            line.SetPosition(1, landing + Vector2.up * markerHeight);
            line.startWidth = line.endWidth = 0.15f;
            line.material = LineMaterial();
            line.startColor = line.endColor = color;
            line.sortingOrder = 50;

            var label = new GameObject("Label");
            label.transform.SetParent(marker.transform, false);
            label.transform.localPosition = new Vector3(0f, markerHeight + 1f, 0f);
            var text = label.AddComponent<TextMeshPro>();
            text.text = $"{node.name}\ndx {dx:+0.0;-0.0;0.0}";
            text.fontSize = 4f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = color;
            text.sortingOrder = 50;

            if (!_markers.TryGetValue(node, out var queue))
            {
                queue = new Queue<GameObject>();
                _markers[node] = queue;
            }
            queue.Enqueue(marker);
            while (queue.Count > Mathf.Max(1, markersPerNode))
                Destroy(queue.Dequeue());
        }

        private static Color ColorFor(Transform node)
        {
            var simulated = node.GetComponentInParent<SimulatedNodeVelocity>();
            if (simulated == null || Mathf.Approximately(simulated.Velocity.x, 0f)) return Color.white;
            return simulated.Velocity.x > 0f ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.55f, 0.2f);
        }

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
