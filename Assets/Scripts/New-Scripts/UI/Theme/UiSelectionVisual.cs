using UnityEngine;
using UnityEngine.EventSystems;

namespace New_Scripts.UI.Theme
{
    /// <summary>
    /// Seçilebilir bir öğe (Button) odak alınca bağlı görseli (sarı levha + halka imleci) açar, bırakınca kapatır.
    /// Gamepad ile gezilen menüler için; fare üstüne gelmeyi dinlemez (oyun yalnızca gamepad).
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("RockTheGame/UI/Selection Visual")]
    public sealed class UiSelectionVisual : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private GameObject visual;

        public void Wire(GameObject selectedVisual) => visual = selectedVisual;

        private void OnEnable()
        {
            var system = EventSystem.current;
            Show(system != null && system.currentSelectedGameObject == gameObject);
        }

        private void OnDisable() => Show(false);

        public void OnSelect(BaseEventData eventData) => Show(true);

        public void OnDeselect(BaseEventData eventData) => Show(false);

        private void Show(bool on)
        {
            if (visual != null) visual.SetActive(on);
        }
    }
}
