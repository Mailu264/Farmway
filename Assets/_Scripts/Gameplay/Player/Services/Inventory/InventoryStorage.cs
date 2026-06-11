using System;
using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public interface IInventoryStorage
    {
        IReadOnlyReactiveDictionary<ItemDefinition, InventoryItemData> Items { get; }
    }

    public class InventoryStorage : IInventoryStorage
    {
        private readonly ReactiveDictionary<ItemDefinition, InventoryItemData> _items = new();

        public IReadOnlyReactiveDictionary<ItemDefinition, InventoryItemData> Items => _items;

        public bool AddItem(ItemDefinition item, int count)
        {
            if (count <= 0)
            {
                Debug.LogError("Count must be positive");
                return false;
            }

            if (_items.TryGetValue(item, out InventoryItemData itemData))
            {
                _items[item] = new InventoryItemData(item, itemData.Count + count);
                return true;
            }

            _items.Add(item, new InventoryItemData(item, count));
            return true;
        }

        public bool RemoveItem(ItemDefinition item) =>
            RemoveItem(item, GetItemsCount(item));

        public bool RemoveItem(ItemDefinition item, int count)
        {
            if (count <= 0)
            {
                Debug.LogError("Count must be positive");
                return false;
            }

            if (!_items.TryGetValue(item, out InventoryItemData itemData))
            {
                Debug.LogError($"Item {item} not found");
                return false;
            }

            if (itemData.Count < count)
            {
                Debug.LogError($"Not enough {item} in inventory");
                return false;
            }

            int newCount = itemData.Count - count;

            if (newCount > 0)
                _items[item] = new InventoryItemData(item, newCount);
            else
                _items.Remove(item);

            return true;
        }

        private int GetItemsCount(ItemDefinition item) =>
            _items.TryGetValue(item, out InventoryItemData itemData) ? itemData.Count : 0;
    }

    public readonly struct InventoryItemData
    {
        public ItemDefinition Item { get; }
        public int Count { get; }

        public InventoryItemData(ItemDefinition item, int count)
        {
            Item = item;
            Count = Math.Max(0, count);
        }
    }
}
