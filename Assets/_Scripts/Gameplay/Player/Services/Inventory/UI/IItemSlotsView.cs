using UnityEngine;
using UnityEngine.EventSystems;

namespace Farmway.Gameplay.Player
{
    public interface IItemSlotsView
    {
        int SlotsCount { get; }

        void Initialize(int slotCount);
        InventorySlotView GetSlot(int index);
        void SetSlot(int index, InventorySlotData slotData, ItemData itemData);
        void ClearSlot(int index);
        void SetSlotVisualsVisible(int index, bool isVisible);
        Vector2 GetSlotSize(int index);
        void ShowDrag(Sprite icon, int count, Vector2 size, PointerEventData eventData);
        void MoveDrag(PointerEventData eventData);
        void HideDrag();
    }
}
