using System;
using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Multiplayer.Flags
{
    /// <summary>
    /// Multiplayer'da RaceFinishPoint'e kurulan bitiş çizgisi (iki modda da). Yerel oyuncu dokununca bir kez bildirir;
    /// bitişin bir kez sayılması RaceSession'da, burası tekrar bildirimi önler.
    /// </summary>
    public sealed class RaceFinishLine : MonoBehaviour
    {
        public const string ObjectName = "[MP Finish]";

        private Action _onReached;
        public bool IsReached { get; private set; }

        public static RaceFinishLine Create(RaceFinishPoint point, RaceFlagSettingsSO settings, Action onReached)
        {
            var go = new GameObject(ObjectName);
            go.transform.SetParent(point.transform, false);
            go.layer = point.gameObject.layer;
            go.transform.localScale = settings.FinishScale;
            go.transform.localPosition = settings.FinishVisualOffset;

            var line = go.AddComponent<RaceFinishLine>();
            line._onReached = onReached;

            var visual = go.AddComponent<SpriteRenderer>();
            visual.sprite = settings.FinishSprite;
            visual.sortingLayerName = settings.SortingLayerName;
            visual.sortingOrder = settings.SortingOrder;

            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            var scale = settings.FinishScale;
            trigger.size = new Vector2(
                scale.x != 0f ? settings.FinishTriggerSize.x / scale.x : settings.FinishTriggerSize.x,
                scale.y != 0f ? settings.FinishTriggerSize.y / scale.y : settings.FinishTriggerSize.y);
            return line;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() != null) Reach();
        }

        public void Reach()
        {
            if (IsReached) return;
            IsReached = true;
            _onReached?.Invoke();
        }
    }
}
