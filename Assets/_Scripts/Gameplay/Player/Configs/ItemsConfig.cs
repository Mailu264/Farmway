using System;
using System.Collections.Generic;
using Farmway.Gameplay.Player.Services.Inventory;
using UnityEngine;

namespace Farmway.Infrastructure
{
    [CreateAssetMenu(fileName = "InventoryConfig", menuName = "Configs/Player/InventoryConfig")]
    public class ItemsConfig : ScriptableObject
    {
        [SerializeField] private List<ItemData> _items = new();

        private readonly Dictionary<ItemIdEnum, ItemData> _itemCache = new();

        public IReadOnlyList<ItemData> Items => _items;
        
        private void Awake()
        {
            foreach (var item in _items)
                _itemCache.Add(item.ItemId, item);
        }

        public bool TryGetItem(ItemIdEnum item, out ItemData itemData)
        {
            if (_itemCache.TryGetValue(item, out itemData)) 
                return true;
            
            Debug.LogError($"Item {item} not found");
            return false;
        }
    }
    
    [Serializable]
    public struct ItemData
    {
        public ItemIdEnum ItemId;
        public ItemTypesEnum ItemType;
        public Sprite Icon;
        public string Name;
        public int MaxStackSize;
        public bool IsStackable;
    }
}