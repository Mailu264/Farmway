using System.Collections.Generic;
using Farmway.Gameplay.UI.PopUp;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Farmway.Gameplay.Player
{
    public class InventoryView : PopUpView, IItemSlotsView
    {
        [SerializeField] private InventoryDragView _dragView;
        [SerializeField] private InventorySlotView _slotPrefab;
        [SerializeField] private RectTransform _slotsContainer;

        private readonly List<InventorySlotView> _slots = new();
        private RectTransform _rectTransform;

        public int SlotsCount => _slots.Count;

        public void Initialize(int slotCount)
        {
            _rectTransform = (RectTransform)transform;
            _dragView.Initialize(_rectTransform);
            SpawnSlots(slotCount);
        }

        public InventorySlotView GetSlot(int index) =>
            TryGetSlot(index, out InventorySlotView slot) ? slot : null;

        public void SetSlot(int index, InventorySlotData slotData)
        {
            if (!TryGetSlot(index, out InventorySlotView slot))
                return;

            if (slotData.IsEmpty)
            {
                slot.Clear();
                return;
            }

            slot.SetItem(slotData.Item.Icon, slotData.Count);
        }

        public void ClearSlot(int index)
        {
            if (TryGetSlot(index, out InventorySlotView slot))
                slot.Clear();
        }

        public void SetSlotVisualsVisible(int index, bool isVisible)
        {
            if (TryGetSlot(index, out InventorySlotView slot))
                slot.SetVisualsVisible(isVisible);
        }

        public void ShowDrag(Sprite icon, int count, Vector2 size, PointerEventData eventData)
        {
            _dragView.Show(icon, count, size, eventData);
        }

        public void MoveDrag(PointerEventData eventData) =>
            _dragView.Move(eventData);

        public void HideDrag() =>
            _dragView.Hide();

        public Vector2 GetSlotSize(int index) =>
            TryGetSlot(index, out InventorySlotView slot)
                ? slot.RectTransform.rect.size
                : Vector2.zero;

        private void SpawnSlots(int count)
        {
            foreach (InventorySlotView slot in _slots)
                Destroy(slot.gameObject);

            _slots.Clear();

            for (int i = 0; i < count; i++)
            {
                InventorySlotView slot = Instantiate(_slotPrefab, _slotsContainer);
                slot.Initialize(i);
                _slots.Add(slot);
            }
        }

        private bool TryGetSlot(int index, out InventorySlotView slot)
        {
            if (index >= 0 && index < _slots.Count)
            {
                slot = _slots[index];
                return true;
            }

            Debug.LogError($"Inventory slot view with index {index} not found");
            slot = null;
            return false;
        }
    }
}
