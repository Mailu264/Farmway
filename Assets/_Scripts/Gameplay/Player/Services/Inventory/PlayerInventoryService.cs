using System;
using Farmway.Gameplay.Farm;
using Farmway.Infrastructure;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public interface IPlayerInventoryService
    {
        bool AddItem(ItemDefinition item, int count);
        bool RemoveItem(ItemDefinition item);
        bool RemoveItem(ItemDefinition item, int count);
    }

    public class PlayerInventoryService : PlayerService, IPlayerInventoryService
    {
        private readonly IInventorySlotsModel _inventorySlotsModel;
        private readonly IHotbarSlotsModel _hotbarSlotsModel;
        private readonly IConfigProvider _configProvider;
        private readonly ISaveService _saveService;

        public PlayerInventoryService(
            IInventorySlotsModel inventorySlotsModel,
            IHotbarSlotsModel hotbarSlotsModel,
            IConfigProvider configProvider,
            ISaveService saveService)
        {
            _inventorySlotsModel = inventorySlotsModel;
            _hotbarSlotsModel = hotbarSlotsModel;
            _configProvider = configProvider;
            _saveService = saveService;
        }

        public override void OnInitialize()
        {
            _inventorySlotsModel.Initialize(PlayerConfig.InventorySlotCount);
            _hotbarSlotsModel.Initialize(PlayerConfig.HotbarSlotCount);

            if (!TryRestoreFromSave())
                GiveStartingItems();
        }

        private bool TryRestoreFromSave()
        {
            var save = _saveService.Pending;

            if (save == null)
                return false;

            var lookup = ItemLookup.Build(_configProvider);

            RestoreSlots(save.InventorySlots, _inventorySlotsModel, lookup);
            RestoreSlots(save.HotbarSlots, _hotbarSlotsModel, lookup);
            return true;
        }

        private static void RestoreSlots(
            System.Collections.Generic.List<SlotSave> saved,
            IItemSlotsModel model,
            System.Collections.Generic.Dictionary<string, ItemDefinition> lookup)
        {
            foreach (var slot in saved)
            {
                if (!lookup.TryGetValue(slot.ItemName, out var item))
                {
                    Debug.LogWarning($"[Save] Предмет {slot.ItemName} не найден — пропущен");
                    continue;
                }

                if (model.IsValidIndex(slot.Index))
                    model.SetSlot(slot.Index, new InventorySlotData(item, slot.Count));
            }
        }

        public bool AddItem(ItemDefinition item, int count)
        {
            // Сначала докладываем в существующие стаки хотбара, остальное — в инвентарь
            int remaining = _hotbarSlotsModel.StackExisting(item, count);

            if (remaining <= 0)
                return true;

            remaining = _inventorySlotsModel.StackExisting(item, remaining);

            if (remaining <= 0)
                return true;

            return _inventorySlotsModel.AddItem(item, remaining);
        }

        public bool RemoveItem(ItemDefinition item)
        {
            int total = _inventorySlotsModel.GetItemCount(item)
                        + _hotbarSlotsModel.GetItemCount(item);

            return total > 0 && RemoveItem(item, total);
        }

        public bool RemoveItem(ItemDefinition item, int count)
        {
            if (count <= 0)
            {
                Debug.LogError("Count must be positive");
                return false;
            }

            int inInventory = _inventorySlotsModel.GetItemCount(item);
            int inHotbar = _hotbarSlotsModel.GetItemCount(item);

            if (inInventory + inHotbar < count)
            {
                Debug.LogError($"Not enough {item} in inventory");
                return false;
            }

            int remaining = count;

            if (inInventory > 0)
            {
                int fromInventory = Math.Min(inInventory, remaining);
                _inventorySlotsModel.RemoveItem(item, fromInventory);
                remaining -= fromInventory;
            }

            if (remaining > 0)
                _hotbarSlotsModel.RemoveItem(item, remaining);

            return true;
        }

        private void GiveStartingItems()
        {
            var farmConfig = _configProvider.GetConfig<FarmConfig>();
            int slot = 0;

            foreach (var startingItem in farmConfig.StartingItems)
            {
                if (startingItem.Item == null || startingItem.Count <= 0)
                    continue;

                if (slot < PlayerConfig.HotbarSlotCount)
                    _hotbarSlotsModel.SetSlot(slot++, new InventorySlotData(startingItem.Item, startingItem.Count));
                else
                    _inventorySlotsModel.AddItem(startingItem.Item, startingItem.Count);
            }
        }
    }
}
