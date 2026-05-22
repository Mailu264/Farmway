using System;
using Farmway.Infrastructure;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public interface IPlayerInventoryService
    {
        bool AddItem(ItemIdEnum itemId, int count);
        bool RemoveItem(ItemIdEnum itemId);
        bool RemoveItem(ItemIdEnum itemId, int count);
    }

    public class PlayerInventoryService : PlayerService, IPlayerInventoryService
    {
        private readonly IInventorySlotsModel _inventorySlotsModel;
        private readonly IHotbarSlotsModel _hotbarSlotsModel;
        private readonly InventoryStorage _inventoryStorage;
        private readonly IConfigProvider _configProvider;

        public PlayerInventoryService(
            IInventorySlotsModel inventorySlotsModel,
            IHotbarSlotsModel hotbarSlotsModel,
            InventoryStorage inventoryStorage,
            IConfigProvider configProvider)
        {
            _inventorySlotsModel = inventorySlotsModel;
            _hotbarSlotsModel = hotbarSlotsModel;
            _inventoryStorage = inventoryStorage;
            _configProvider = configProvider;
        }

        public override void OnInitialize()
        {
            _inventorySlotsModel.Initialize(PlayerConfig.InventorySlotCount);
            _hotbarSlotsModel.Initialize(PlayerConfig.HotbarSlotCount);
        }

        public bool AddItem(ItemIdEnum itemId, int count)
        {
            if (!_inventorySlotsModel.AddItem(itemId, count))
                return false;

            _inventoryStorage.AddItem(itemId, count);
            return true;
        }

        public bool RemoveItem(ItemIdEnum itemId)
        {
            int total = _inventorySlotsModel.GetItemCount(itemId)
                        + _hotbarSlotsModel.GetItemCount(itemId);

            return total > 0 && RemoveItem(itemId, total);
        }

        public bool RemoveItem(ItemIdEnum itemId, int count)
        {
            if (count <= 0)
            {
                Debug.LogError("Count must be positive");
                return false;
            }

            int inInventory = _inventorySlotsModel.GetItemCount(itemId);
            int inHotbar = _hotbarSlotsModel.GetItemCount(itemId);

            if (inInventory + inHotbar < count)
            {
                Debug.LogError($"Not enough {itemId} in inventory");
                return false;
            }

            int remaining = count;

            if (inInventory > 0)
            {
                int fromInventory = Math.Min(inInventory, remaining);
                _inventorySlotsModel.RemoveItem(itemId, fromInventory);
                remaining -= fromInventory;
            }

            if (remaining > 0)
                _hotbarSlotsModel.RemoveItem(itemId, remaining);

            _inventoryStorage.RemoveItem(itemId, count);
            return true;
        }
    }
}
