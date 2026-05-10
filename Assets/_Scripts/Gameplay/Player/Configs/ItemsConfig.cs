using System;
using System.Collections.Generic;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    [CreateAssetMenu(fileName = "ItemsConfig", menuName = "Configs/Player/ItemsConfig")]
    public class ItemsConfig : ScriptableObject
    {
        [SerializeField] private List<ItemData> _items = new();

        private readonly Dictionary<ItemIdEnum, ItemData> _itemCache = new();

        public IReadOnlyList<ItemData> Items => _items;
        
        public bool TryGetItem(ItemIdEnum item, out ItemData itemData)
        {
            if (_itemCache.TryGetValue(item, out itemData)) 
                return true;
            
            Debug.LogError($"Item {item} not found");
            return false;
        }

        private void OnValidate() => 
            EnsureCache();

        private void EnsureCache()
        {
            if (_itemCache.Count == _items.Count)
                return;

            _itemCache.Clear();

            foreach (var item in _items)
            {
                if (item.ItemId == ItemIdEnum.None)
                    continue;

                if (!_itemCache.TryAdd(item.ItemId, item))
                    Debug.LogError($"Duplicate item id {item.ItemId}", this);
            }
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
