using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Farmway.Gameplay.Player
{
    public class InventoryDragView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _count;

        private RectTransform _rectTransform;
        private RectTransform _moveArea;

        public void Initialize(RectTransform moveArea)
        {
            _moveArea = moveArea;
            _rectTransform = (RectTransform)transform;

            _icon.raycastTarget = false;
            _count.raycastTarget = false;

            Hide();
        }

        public void Show(Sprite icon, int count, Vector2 size, PointerEventData eventData)
        {
            _icon.sprite = icon;

            _count.text = count > 1 ? count.ToString() : string.Empty;
            _count.gameObject.SetActive(count > 1);

            _rectTransform.sizeDelta = size;
            gameObject.SetActive(true);
            _rectTransform.SetAsLastSibling();
            Move(eventData);
        }

        public void Move(PointerEventData eventData)
        {
            if (_moveArea == null)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_moveArea, 
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 localPoint))
                _rectTransform.anchoredPosition = localPoint;
        }

        public void Hide() =>
            gameObject.SetActive(false);
    }
}
