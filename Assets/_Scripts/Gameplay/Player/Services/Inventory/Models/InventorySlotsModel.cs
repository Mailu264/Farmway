using System;
using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public interface IInventorySlotsModel : IItemSlotsModel
    {
        bool AddItem(ItemDefinition item, int count);
        bool RemoveItem(ItemDefinition item);
    }

    public class InventorySlotsModel : IInventorySlotsModel
    {
        private readonly ItemSlotsCollection _slots = new();

        public IReadOnlyReactiveCollection<InventorySlotData> Slots => _slots.Slots;

        public void Initialize(int slotsCount) =>
            _slots.Initialize(slotsCount);

        public bool IsValidIndex(int index) =>
            _slots.IsValidIndex(index);

        public InventorySlotData GetSlot(int index) =>
            _slots.GetSlot(index);

        public void SetSlot(int index, InventorySlotData slotData) =>
            _slots.SetSlot(index, slotData);

        public bool AddItem(ItemDefinition item, int count)
        {
            if (item == null)
            {
                Debug.LogError("Item is null");
                return false;
            }

            if (count <= 0)
            {
                Debug.LogError("Count must be positive");
                return false;
            }

            if (!CanPlaceItem(item, count))
            {
                Debug.LogError($"Not enough inventory slots for {item.Name}");
                return false;
            }

            int remaining = count;

            if (item.IsStackable)
                remaining = FillExistingStacks(item, Mathf.Max(1, item.MaxStackSize), remaining);

            FillEmptySlots(item, remaining);
            return true;
        }

        public bool RemoveItem(ItemDefinition item) =>
            _slots.RemoveItem(item, _slots.GetItemCount(item));

        public bool RemoveItem(ItemDefinition item, int count) =>
            _slots.RemoveItem(item, count);

        public int StackExisting(ItemDefinition item, int count) =>
            _slots.StackExisting(item, count);

        public int GetItemCount(ItemDefinition item) =>
            _slots.GetItemCount(item);

        private bool CanPlaceItem(ItemDefinition item, int count)
        {
            int remaining = count;
            int maxStackSize = item.IsStackable ? Mathf.Max(1, item.MaxStackSize) : 1;

            if (item.IsStackable)
            {
                for (int i = 0; i < Slots.Count && remaining > 0; i++)
                {
                    InventorySlotData slot = Slots[i];

                    if (slot.Item == item && slot.Count < maxStackSize)
                        remaining -= maxStackSize - slot.Count;
                }
            }

            for (int i = 0; i < Slots.Count && remaining > 0; i++)
            {
                if (Slots[i].IsEmpty)
                    remaining -= maxStackSize;
            }

            return remaining <= 0;
        }

        private int FillExistingStacks(ItemDefinition item, int maxStackSize, int count)
        {
            int remaining = count;

            for (int i = 0; i < Slots.Count && remaining > 0; i++)
            {
                InventorySlotData slot = Slots[i];

                if (slot.Item != item || slot.Count >= maxStackSize)
                    continue;

                int freeSpace = maxStackSize - slot.Count;
                int addedCount = Math.Min(freeSpace, remaining);

                SetSlot(i, new InventorySlotData(item, slot.Count + addedCount));
                remaining -= addedCount;
            }

            return remaining;
        }

        private void FillEmptySlots(ItemDefinition item, int count)
        {
            int remaining = count;
            int maxCountInSlot = item.IsStackable ? Mathf.Max(1, item.MaxStackSize) : 1;

            for (int i = 0; i < Slots.Count && remaining > 0; i++)
            {
                if (!Slots[i].IsEmpty)
                    continue;

                int addedCount = Math.Min(maxCountInSlot, remaining);
                SetSlot(i, new InventorySlotData(item, addedCount));
                remaining -= addedCount;
            }
        }
    }

    public readonly struct InventorySlotData
    {
        public static InventorySlotData Empty => new(null, 0);

        public ItemDefinition Item { get; }
        public int Count { get; }

        public bool IsEmpty => Item == null || Count <= 0;

        public InventorySlotData(ItemDefinition item, int count)
        {
            Item = item;
            Count = Math.Max(0, count);
        }
    }
}
