using System;
using Farmway.Infrastructure;
using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public interface IInventorySlotsModel
    {
        IReadOnlyReactiveCollection<InventorySlotData> Slots { get; }

        bool AddItem(ItemIdEnum itemId, int count);
        bool RemoveItem(ItemIdEnum itemId);
        bool RemoveItem(ItemIdEnum itemId, int count);
        bool TryMove(int fromIndex, int toIndex);
    }

    public class InventorySlotsModel : IInventorySlotsModel
    {
        private const int DefaultSlotsCount = 24;

        private readonly ReactiveCollection<InventorySlotData> _slots = new();
        private readonly IInventoryStorage _inventoryStorage;
        private readonly ItemsConfig _itemsConfig;

        public IReadOnlyReactiveCollection<InventorySlotData> Slots => _slots;

        public InventorySlotsModel(IInventoryStorage inventoryStorage, IConfigProvider configProvider)
        {
            _inventoryStorage = inventoryStorage;
            _itemsConfig = configProvider.GetConfig<ItemsConfig>();

            for (int i = 0; i < DefaultSlotsCount; i++)
                _slots.Add(InventorySlotData.Empty);
        }

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
            _inventoryStorage.AddItem(itemId, count);
            return true;
        }

        public bool RemoveItem(ItemIdEnum itemId)
        {
            bool removed = false;

            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].ItemId != itemId)
                    continue;

                _slots[i] = InventorySlotData.Empty;
                removed = true;
            }

            return removed && _inventoryStorage.RemoveItem(itemId);
        }

        public bool RemoveItem(ItemIdEnum itemId, int count)
        {
            if (count <= 0)
            {
                Debug.LogError("Count must be positive");
                return false;
            }

            if (GetItemsCount(itemId) < count)
            {
                Debug.LogError($"Not enough {itemId} in inventory slots");
                return false;
            }

            int remaining = count;

            for (int i = _slots.Count - 1; i >= 0 && remaining > 0; i--)
            {
                InventorySlotData slot = _slots[i];

                if (slot.ItemId != itemId)
                    continue;

                int removedCount = Math.Min(slot.Count, remaining);
                int newCount = slot.Count - removedCount;
                remaining -= removedCount;

                _slots[i] = newCount > 0
                    ? new InventorySlotData(slot.ItemId, newCount)
                    : InventorySlotData.Empty;
            }

            _inventoryStorage.RemoveItem(itemId, count);
            return true;
        }

        public bool TryMove(int fromIndex, int toIndex)
        {
            if (!IsValidIndex(fromIndex) || !IsValidIndex(toIndex))
                return false;

            if (fromIndex == toIndex)
                return false;

            InventorySlotData from = _slots[fromIndex];
            InventorySlotData to = _slots[toIndex];

            if (from.IsEmpty)
                return false;

            if (to.IsEmpty)
            {
                _slots[toIndex] = from;
                _slots[fromIndex] = InventorySlotData.Empty;
                return true;
            }

            if (CanMerge(from, to))
            {
                MergeStacks(fromIndex, toIndex);
                return true;
            }

            _slots[fromIndex] = to;
            _slots[toIndex] = from;
            return true;
        }

        private bool CanPlaceItem(ItemIdEnum itemId, ItemData itemData, int count)
        {
            int remaining = count;
            int maxStackSize = itemData.IsStackable ? Mathf.Max(1, itemData.MaxStackSize) : 1;

            if (itemData.IsStackable)
            {
                for (int i = 0; i < _slots.Count && remaining > 0; i++)
                {
                    InventorySlotData slot = _slots[i];

                    if (slot.ItemId == itemId && slot.Count < maxStackSize)
                        remaining -= maxStackSize - slot.Count;
                }
            }

            for (int i = 0; i < _slots.Count && remaining > 0; i++)
            {
                if (_slots[i].IsEmpty)
                    remaining -= maxStackSize;
            }

            return remaining <= 0;
        }

        private int FillExistingStacks(ItemIdEnum itemId, int maxStackSize, int count)
        {
            int remaining = count;

            for (int i = 0; i < _slots.Count && remaining > 0; i++)
            {
                InventorySlotData slot = _slots[i];

                if (slot.ItemId != itemId || slot.Count >= maxStackSize)
                    continue;

                int freeSpace = maxStackSize - slot.Count;
                int addedCount = Math.Min(freeSpace, remaining);

                _slots[i] = new InventorySlotData(itemId, slot.Count + addedCount);
                remaining -= addedCount;
            }

            return remaining;
        }

        private void FillEmptySlots(ItemIdEnum itemId, ItemData itemData, int count)
        {
            int remaining = count;
            int maxCountInSlot = itemData.IsStackable ? Mathf.Max(1, itemData.MaxStackSize) : 1;

            for (int i = 0; i < _slots.Count && remaining > 0; i++)
            {
                if (!_slots[i].IsEmpty)
                    continue;

                int addedCount = Math.Min(maxCountInSlot, remaining);
                _slots[i] = new InventorySlotData(itemId, addedCount);
                remaining -= addedCount;
            }
        }

        private bool CanMerge(InventorySlotData from, InventorySlotData to)
        {
            if (from.ItemId != to.ItemId)
                return false;

            if (!_itemsConfig.TryGetItem(from.ItemId, out ItemData itemData))
                return false;

            return itemData.IsStackable && to.Count < Mathf.Max(1, itemData.MaxStackSize);
        }

        private void MergeStacks(int fromIndex, int toIndex)
        {
            InventorySlotData from = _slots[fromIndex];
            InventorySlotData to = _slots[toIndex];

            if (!_itemsConfig.TryGetItem(from.ItemId, out ItemData itemData))
                return;

            int freeSpace = Mathf.Max(1, itemData.MaxStackSize) - to.Count;
            int movedCount = Math.Min(freeSpace, from.Count);
            int remainingCount = from.Count - movedCount;

            _slots[toIndex] = new InventorySlotData(to.ItemId, to.Count + movedCount);
            _slots[fromIndex] = remainingCount > 0
                ? new InventorySlotData(from.ItemId, remainingCount)
                : InventorySlotData.Empty;
        }

        private bool IsValidIndex(int index) =>
            index >= 0 && index < _slots.Count;

        private int GetItemsCount(ItemIdEnum itemId)
        {
            int count = 0;

            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].ItemId == itemId)
                    count += _slots[i].Count;
            }

            return count;
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
