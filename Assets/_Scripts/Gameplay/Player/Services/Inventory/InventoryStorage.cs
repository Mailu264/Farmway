using System;
using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public interface IInventoryStorage
    {
        IReadOnlyReactiveDictionary<ItemIdEnum, InventoryItemData> Items { get; }

        bool AddItem(ItemIdEnum itemId, int count);
        bool RemoveItem(ItemIdEnum itemId);
        bool RemoveItem(ItemIdEnum itemId, int count);
    }

    public class InventoryStorage : IInventoryStorage
    {
        private readonly ReactiveDictionary<ItemIdEnum, InventoryItemData> _items = new();

        public IReadOnlyReactiveDictionary<ItemIdEnum, InventoryItemData> Items => _items;

        public bool AddItem(ItemIdEnum itemId, int count)
        {
            if (count <= 0)
            {
                Debug.LogError("Count must be positive");
                return false;
            }

            if (_items.TryGetValue(itemId, out InventoryItemData itemData))
            {
                _items[itemId] = new InventoryItemData(itemId, itemData.Count + count);
                return true;
            }

            _items.Add(itemId, new InventoryItemData(itemId, count));
            return true;
        }

        public bool RemoveItem(ItemIdEnum itemId) =>
            RemoveItem(itemId, GetItemsCount(itemId));

        public bool RemoveItem(ItemIdEnum itemId, int count)
        {
            if (count <= 0)
            {
                Debug.LogError("Count must be positive");
                return false;
            }

            if (!_items.TryGetValue(itemId, out InventoryItemData itemData))
            {
                Debug.LogError($"Item {itemId} not found");
                return false;
            }

            if (itemData.Count < count)
            {
                Debug.LogError($"Not enough {itemId} in inventory");
                return false;
            }

            int newCount = itemData.Count - count;

            if (newCount > 0)
                _items[itemId] = new InventoryItemData(itemId, newCount);
            else
                _items.Remove(itemId);

            return true;
        }

        private int GetItemsCount(ItemIdEnum itemId) =>
            _items.TryGetValue(itemId, out InventoryItemData itemData) ? itemData.Count : 0;
    }

    public readonly struct InventoryItemData
    {
        public ItemIdEnum ItemId { get; }
        public int Count { get; }

        public InventoryItemData(ItemIdEnum itemId, int count)
        {
            ItemId = itemId;
            Count = Math.Max(0, count);
        }
    }
}
