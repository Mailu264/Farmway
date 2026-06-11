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
        int GetItemCount(ItemDefinition item);
        bool RemoveItem(ItemDefinition item, int count);
        int StackExisting(ItemDefinition item, int count);
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

        public int GetItemCount(ItemDefinition item)
        {
            int count = 0;

            for (int i = 0; i < _slots.Count; i++)
                if (_slots[i].Item == item)
                    count += _slots[i].Count;

            return count;
        }

        // Докладывает в уже существующие стаки, пустые слоты не занимает. Возвращает остаток.
        public int StackExisting(ItemDefinition item, int count)
        {
            if (item == null || !item.IsStackable || count <= 0)
                return count;

            int maxStack = Math.Max(1, item.MaxStackSize);
            int remaining = count;

            for (int i = 0; i < _slots.Count && remaining > 0; i++)
            {
                InventorySlotData slot = _slots[i];

                if (slot.Item != item || slot.Count >= maxStack)
                    continue;

                int added = Math.Min(maxStack - slot.Count, remaining);
                _slots[i] = new InventorySlotData(item, slot.Count + added);
                remaining -= added;
            }

            return remaining;
        }

        public bool RemoveItem(ItemDefinition item, int count)
        {
            if (count <= 0 || GetItemCount(item) < count)
                return false;

            int remaining = count;

            for (int i = _slots.Count - 1; i >= 0 && remaining > 0; i--)
            {
                InventorySlotData slot = _slots[i];

                if (slot.Item != item)
                    continue;

                int removedCount = Math.Min(slot.Count, remaining);
                remaining -= removedCount;
                int newCount = slot.Count - removedCount;

                _slots[i] = newCount > 0
                    ? new InventorySlotData(slot.Item, newCount)
                    : InventorySlotData.Empty;
            }

            return true;
        }
    }
}
