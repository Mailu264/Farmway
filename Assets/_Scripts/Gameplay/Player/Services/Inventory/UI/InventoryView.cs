using System.Collections.Generic;
using Farmway.Gameplay.UI.PopUp;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Farmway.Gameplay.Player
{
    public class InventoryView : PopUpView
    {
        [SerializeField] private InventoryDragView _dragView;
        [SerializeField] private List<InventorySlotView> _inventorySlots = new();

        private RectTransform _rectTransform;

        public int SlotsCount => _inventorySlots.Count;

        public void Initialize()
        {
            _rectTransform = (RectTransform)transform;
            _dragView?.Initialize(_rectTransform);

            for (int i = 0; i < _inventorySlots.Count; i++)
                _inventorySlots[i].Initialize(i);
        }

        public InventorySlotView GetSlot(int index) =>
            TryGetSlot(index, out InventorySlotView slot) ? slot : null;

        public void SetSlot(int index, InventorySlotData slotData, ItemData itemData)
        {
            if (!TryGetSlot(index, out InventorySlotView slot))
                return;

            if (slotData.IsEmpty)
            {
                slot.Clear();
                return;
            }

            slot.SetItem(itemData.Icon, slotData.Count);
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

        public void ShowDrag(Sprite icon, int count, int sourceSlotIndex, PointerEventData eventData)
        {
            if (icon == null || !TryGetSlot(sourceSlotIndex, out InventorySlotView sourceSlot))
                return;

            _dragView.Show(icon, count, sourceSlot.RectTransform.rect.size, eventData);
            sourceSlot.SetVisualsVisible(false);
        }

        public void MoveDrag(PointerEventData eventData) =>
            _dragView.Move(eventData);

        public void HideDrag() =>
            _dragView.Hide();

        private bool TryGetSlot(int index, out InventorySlotView slot)
        {
            if (index >= 0 && index < _inventorySlots.Count)
            {
                slot = _inventorySlots[index];
                return true;
            }

            Debug.LogError($"Inventory slot view with index {index} not found");
            slot = null;
            return false;
        }
    }
}
