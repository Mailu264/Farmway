using System;
using UniRx;

namespace Farmway.Gameplay.Player
{
    public interface IItemSlotsModel
    {
        IReadOnlyReactiveCollection<InventorySlotData> Slots { get; }

        void Initialize(int slotsCount);
        bool IsValidIndex(int index);
        InventorySlotData GetSlot(int index);
        void SetSlot(int index, InventorySlotData slotData);
        int GetItemCount(ItemIdEnum itemId);
        bool RemoveItem(ItemIdEnum itemId, int count);
    }

    public class ItemSlotsCollection
    {
        private readonly ReactiveCollection<InventorySlotData> _slots = new();

        public IReadOnlyReactiveCollection<InventorySlotData> Slots => _slots;

        public void Initialize(int slotsCount)
        {
            _slots.Clear();

            for (int i = 0; i < slotsCount; i++)
                _slots.Add(InventorySlotData.Empty);
        }

        public bool IsValidIndex(int index) =>
            index >= 0 && index < _slots.Count;

        public InventorySlotData GetSlot(int index) =>
            IsValidIndex(index) ? _slots[index] : InventorySlotData.Empty;

        public void SetSlot(int index, InventorySlotData slotData)
        {
            if (!IsValidIndex(index))
                return;

            _slots[index] = slotData;
        }

        public int GetItemCount(ItemIdEnum itemId)
        {
            int count = 0;

            for (int i = 0; i < _slots.Count; i++)
                if (_slots[i].ItemId == itemId)
                    count += _slots[i].Count;

            return count;
        }

        public bool RemoveItem(ItemIdEnum itemId, int count)
        {
            if (count <= 0 || GetItemCount(itemId) < count)
                return false;

            int remaining = count;

            for (int i = _slots.Count - 1; i >= 0 && remaining > 0; i--)
            {
                InventorySlotData slot = _slots[i];

                if (slot.ItemId != itemId)
                    continue;

                int removedCount = Math.Min(slot.Count, remaining);
                remaining -= removedCount;
                int newCount = slot.Count - removedCount;

                _slots[i] = newCount > 0
                    ? new InventorySlotData(slot.ItemId, newCount)
                    : InventorySlotData.Empty;
            }

            return true;
        }
    }
}
