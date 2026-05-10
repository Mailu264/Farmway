using UniRx;
using UnityEngine;

namespace Farmway.Gameplay.Player.Services.Inventory
{
    public interface IInventoryStorage
    {
        IReadOnlyReactiveDictionary<ItemIdEnum, InventoryItemData> Items { get; }
        
        bool AddItem(ItemIdEnum itemId, int count);
        bool RemoveItem(ItemIdEnum itemId);
    }

    public class InventoryStorage : IInventoryStorage
    {
        private readonly ReactiveDictionary<ItemIdEnum, InventoryItemData> _items = new();
        
        public IReadOnlyReactiveDictionary<ItemIdEnum, InventoryItemData> Items => _items;
        
        public bool AddItem(ItemIdEnum itemId, int count)
        {
            if (count <= 0)
            {
                Debug.LogError($"Count must be positive");
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

        public bool RemoveItem(ItemIdEnum itemId)
        {
            if (_items.Remove(itemId))
                return true;
            
            Debug.LogError($"Item {itemId} not found");
            return false;
        }
    }

    public class InventoryItemData
    {
        public ItemIdEnum ItemId { get; private set; }
        public int Count { get; private set; }

        public InventoryItemData(ItemIdEnum itemId, int count)
        {
            ItemId = itemId;
            Count = count;
        }
    }
}
