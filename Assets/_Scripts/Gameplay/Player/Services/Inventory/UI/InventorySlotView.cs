using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Farmway.Gameplay.Player
{
    public class InventorySlotView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] private Image _iconPlacement;
        [SerializeField] private TextMeshProUGUI _count;
        [SerializeField] private GameObject _selectedFrame;

        private bool _isEmpty = true;

        public int Index { get; private set; }
        public RectTransform RectTransform => (RectTransform)transform;

        public event Action<int, PointerEventData> BeginDragged;
        public event Action<PointerEventData> Dragged;
        public event Action EndDragged;
        public event Action<int> Dropped;

        public void Initialize(int index)
        {
            Index = index;
            StyleCount();
        }

        // Жирный белый счётчик — читается на любой иконке.
        // Outline через материал не трогаем: на неинициализированном TMP это кидает NRE.
        private void StyleCount()
        {
            if (_count == null)
                return;

            _count.fontSize = 20;
            _count.fontStyle = TMPro.FontStyles.Bold;
            _count.color = Color.white;
            _count.alignment = TMPro.TextAlignmentOptions.BottomRight;
        }

        public void SetItem(Sprite icon, int count)
        {
            _isEmpty = false;
            SetVisualsVisible(true);
            SetIcon(icon, true);
            SetCount(count);
        }

        public void Clear()
        {
            _isEmpty = true;
            SetIcon(null, false);
            SetCount(0);
        }

        public void SetVisualsVisible(bool isVisible)
        {
            _iconPlacement.enabled = isVisible && !_isEmpty;
            _count.gameObject.SetActive(isVisible && !_isEmpty && !string.IsNullOrEmpty(_count.text));
        }

        public void SetSelected(bool isSelected) =>
            _selectedFrame?.SetActive(isSelected);

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isEmpty)
                return;

            BeginDragged?.Invoke(Index, eventData);
        }

        public void OnDrag(PointerEventData eventData) =>
            Dragged?.Invoke(eventData);

        public void OnEndDrag(PointerEventData eventData) =>
            EndDragged?.Invoke();

        public void OnDrop(PointerEventData eventData) =>
            Dropped?.Invoke(Index);

        private void SetIcon(Sprite icon, bool isEnabled)
        {
            _iconPlacement.sprite = icon;
            _iconPlacement.enabled = isEnabled;
        }

        private void SetCount(int count)
        {
            _count.text = count > 1 ? count.ToString() : string.Empty;
            _count.gameObject.SetActive(count > 1);
        }
    }
}
