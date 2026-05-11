using System;
using Farmway.Infrastructure;
using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public interface IInventorySlotsModel : IItemSlotsModel
    {
        bool AddItem(ItemIdEnum itemId, int count);
        bool RemoveItem(ItemIdEnum itemId);
    }

    public class InventorySlotsModel : IInventorySlotsModel
    {
        private readonly ItemSlotsCollection _slots = new();
        private readonly ItemsConfig _itemsConfig;

        public IReadOnlyReactiveCollection<InventorySlotData> Slots => _slots.Slots;

        public InventorySlotsModel(IConfigProvider configProvider)
        {
            _itemsConfig = configProvider.GetConfig<ItemsConfig>();
        }

        public void Initialize(int slotsCount) =>
            _slots.Initialize(slotsCount);

        public bool IsValidIndex(int index) =>
            _slots.IsValidIndex(index);

        public InventorySlotData GetSlot(int index) =>
            _slots.GetSlot(index);

        public void SetSlot(int index, InventorySlotData slotData) =>
            _slots.SetSlot(index, slotData);

        public bool AddItem(ItemIdEnum itemId, int count)
        {
            if (count <= 0)
            {
                Debug.LogError("Count must be positive");
                return false;
            }

            if (!_itemsConfig.TryGetItem(itemId, out ItemData itemData))
                return false;

            if (!CanPlaceItem(itemId, itemData, count))
            {
                Debug.LogError($"Not enough inventory slots for {itemId}");
                return false;
            }

            int remaining = count;

            if (itemData.IsStackable)
                remaining = FillExistingStacks(itemId, Mathf.Max(1, itemData.MaxStackSize), remaining);

            FillEmptySlots(itemId, itemData, remaining);
            return true;
        }

        public bool RemoveItem(ItemIdEnum itemId) =>
            _slots.RemoveItem(itemId, _slots.GetItemCount(itemId));

        public bool RemoveItem(ItemIdEnum itemId, int count) =>
            _slots.RemoveItem(itemId, count);

        public int GetItemCount(ItemIdEnum itemId) =>
            _slots.GetItemCount(itemId);

        private bool CanPlaceItem(ItemIdEnum itemId, ItemData itemData, int count)
        {
            int remaining = count;
            int maxStackSize = itemData.IsStackable ? Mathf.Max(1, itemData.MaxStackSize) : 1;

            if (itemData.IsStackable)
            {
                for (int i = 0; i < Slots.Count && remaining > 0; i++)
                {
                    InventorySlotData slot = Slots[i];

                    if (slot.ItemId == itemId && slot.Count < maxStackSize)
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

        private int FillExistingStacks(ItemIdEnum itemId, int maxStackSize, int count)
        {
            int remaining = count;

            for (int i = 0; i < Slots.Count && remaining > 0; i++)
            {
                InventorySlotData slot = Slots[i];

                if (slot.ItemId != itemId || slot.Count >= maxStackSize)
                    continue;

                int freeSpace = maxStackSize - slot.Count;
                int addedCount = Math.Min(freeSpace, remaining);

                SetSlot(i, new InventorySlotData(itemId, slot.Count + addedCount));
                remaining -= addedCount;
            }

            return remaining;
        }

        private void FillEmptySlots(ItemIdEnum itemId, ItemData itemData, int count)
        {
            int remaining = count;
            int maxCountInSlot = itemData.IsStackable ? Mathf.Max(1, itemData.MaxStackSize) : 1;

            for (int i = 0; i < Slots.Count && remaining > 0; i++)
            {
                if (!Slots[i].IsEmpty)
                    continue;

                int addedCount = Math.Min(maxCountInSlot, remaining);
                SetSlot(i, new InventorySlotData(itemId, addedCount));
                remaining -= addedCount;
            }
        }

    }

    public readonly struct InventorySlotData
    {
        public static InventorySlotData Empty => new(ItemIdEnum.None, 0);

        public ItemIdEnum ItemId { get; }
        public int Count { get; }

        public bool IsEmpty => ItemId == ItemIdEnum.None || Count <= 0;

        public InventorySlotData(ItemIdEnum itemId, int count)
        {
            ItemId = itemId;
            Count = Math.Max(0, count);
        }
    }
}
