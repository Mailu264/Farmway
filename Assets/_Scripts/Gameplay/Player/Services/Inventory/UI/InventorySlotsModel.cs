using System;
using Farmway.Infrastructure;
using UniRx;
using VContainer.Unity;

namespace Farmway.Gameplay.Player.Services.Inventory
{
    public interface IInventorySlotsModel
    {
        IReadOnlyReactiveDictionary<ItemIdEnum, InventorySlotsData> Slots { get; }
    }

    public class InventorySlotsModel : IInventorySlotsModel, IInitializable
    {
        private readonly ReactiveDictionary<ItemIdEnum, InventorySlotsData> _slots = new();

        private readonly IInventoryStorage _inventoryStorage;
        private readonly IConfigProvider _configProvider;

        private ItemsConfig _itemsConfig;

        public IReadOnlyReactiveDictionary<ItemIdEnum, InventorySlotsData> Slots => _slots;

        public InventorySlotsModel(IInventoryStorage inventoryStorage, IConfigProvider configProvider)
        {
            _inventoryStorage = inventoryStorage;
            _configProvider = configProvider;
        }

        public void Initialize()
        {
            _itemsConfig = _configProvider.GetConfig<ItemsConfig>();

            _inventoryStorage.Items.ObserveAdd().Subscribe(pair => OnAddItem(pair.Key, pair.Value));
            _inventoryStorage.Items.ObserveRemove().Subscribe(pair => OnRemoveItem(pair.Key));
            _inventoryStorage.Items.ObserveReplace().Subscribe(pair => OnReplaceItem(pair.Key, pair.NewValue));
        }

        private void OnAddItem(ItemIdEnum itemId, InventoryItemData item)
        {
            if (!_itemsConfig.TryGetItem(itemId, out ItemData itemData))
                return;

            _slots.Add(itemId, CalculateSlots(itemData, item.Count));
        }

        private void OnRemoveItem(ItemIdEnum itemId) =>
            _slots.Remove(itemId);

        private void OnReplaceItem(ItemIdEnum itemId, InventoryItemData newItem)
        {
            if (!_itemsConfig.TryGetItem(itemId, out ItemData itemData))
                return;

            _slots[itemId] = CalculateSlots(itemData, newItem.Count);
        }

        private InventorySlotsData CalculateSlots(ItemData config, int totalCount)
        {
            if (!config.IsStackable)
                return new InventorySlotsData(config, totalCount, 1);

            int fullSlots = totalCount / config.MaxStackSize;
            int remainder = totalCount % config.MaxStackSize;

            int slotsCount = fullSlots + (remainder > 0 ? 1 : 0);
            int countInLastSlot = remainder > 0 ? remainder : config.MaxStackSize;

            return new InventorySlotsData(config, slotsCount, countInLastSlot);
        }
    }

    public class InventorySlotsData
    {
        public ItemData Config { get; private set; }
        public int SlotsCount { get; private set; }
        public int CountInLastSlot { get; private set; }

        public InventorySlotsData(ItemData config, int slotsCount, int countInLastSlot)
        {
            Config = config;
            SlotsCount = slotsCount;
            CountInLastSlot = countInLastSlot;
        }

        public void SetSlotsCount(int slotsCount) =>
            SlotsCount = Math.Max(0, slotsCount);

        public void SetCountInLastSlot(int countInSlot) =>
            CountInLastSlot = Math.Max(0, countInSlot);
    }
}
