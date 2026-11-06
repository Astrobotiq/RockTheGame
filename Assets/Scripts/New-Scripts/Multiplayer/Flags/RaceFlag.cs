using System;
using New_Scripts.Multiplayer.Messages;
using New_Scripts.Player;
using UnityEngine;

namespace New_Scripts.Multiplayer.Flags
{
    /// <summary>
    /// Yarış modunda bir RaceFlagPoint'in yerine kurulan bayrak. Yerel oyuncu dokununca bayrak kalkar ve oda kimliği
    /// bir kez bildirilir; bayrağın bir kez sayılması kuralı RaceSession'da, burası yalnız tekrar bildirimi önler.
    /// </summary>
    public sealed class RaceFlag : MonoBehaviour
    {
        public const string ObjectName = "[MP Flag]";
        private static readonly int IsEnteredHash = UnityEngine.Animator.StringToHash("isEntered");

        private Animator _animator;
        private Action<int> _onReached;
        private int _roomId;

        public bool IsCaptured { get; private set; }

        /// <summary>Bayraklar yalnız yarışta var; birlikte modunda ceza da bayrak da yok.</summary>
        public static bool ShouldPlaceFlags(RaceMode mode) => mode == RaceMode.Race;

        public static RaceFlag Create(RaceFlagPoint point, RaceFlagSettingsSO settings, bool captured, Action<int> onReached)
        {
            var go = new GameObject(ObjectName);
            go.transform.SetParent(point.transform, false);
            go.layer = point.gameObject.layer;

            var flag = go.AddComponent<RaceFlag>();
            flag._roomId = point.RoomId;
            flag._onReached = onReached;

            var trigger = go.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            trigger.size = settings.TriggerSize;
            trigger.offset = new Vector2(0f, settings.TriggerSize.y * 0.5f);

            var visual = go.AddComponent<SpriteRenderer>();
            visual.sprite = settings.Sprite;
            visual.sortingLayerName = settings.SortingLayerName;
            visual.sortingOrder = settings.SortingOrder;
            go.transform.localScale = settings.Scale;
            // Görsel ve trigger aynı nesnede: kayma bütün bayrağa uygulanır, trigger tabanı işarette kalsın diye
            // ölçekli yerel uzayda geri alınır.
            go.transform.localPosition = settings.VisualOffset;
            var scale = settings.Scale;
            trigger.offset -= new Vector2(
                scale.x != 0f ? settings.VisualOffset.x / scale.x : 0f,
                scale.y != 0f ? settings.VisualOffset.y / scale.y : 0f);

            if (settings.Animator != null)
            {
                flag._animator = go.AddComponent<Animator>();
                flag._animator.runtimeAnimatorController = settings.Animator;
            }

            if (captured) flag.MarkCaptured();
            return flag;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<PlayerController>() != null) Reach();
        }

        public void Reach()
        {
            if (IsCaptured) return;
            MarkCaptured();
            _onReached?.Invoke(_roomId);
        }

        private void MarkCaptured()
        {
            IsCaptured = true;
            if (_animator != null && _animator.runtimeAnimatorController != null && _animator.isActiveAndEnabled)
                _animator.SetBool(IsEnteredHash, true);
        }

        // Oda uyuyup uyanınca (ya da ilk açılışta) animator sıfırlanır; alınmış bayrak kalkık kalsın.
        private void OnEnable()
        {
            if (IsCaptured) MarkCaptured();
        }
    }
}
