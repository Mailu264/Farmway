using Farmway.Gameplay.Farm;
using Farmway.Infrastructure;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public class ShopService : PlayerService
    {
        private readonly MoneyModel _moneyModel;
        private readonly IPlayerInventoryService _inventoryService;
        private readonly IInventorySlotsModel _inventorySlotsModel;
        private readonly IHotbarSlotsModel _hotbarSlotsModel;
        private readonly FarmConfig _farmConfig;

        private readonly ISaveService _saveService;

        public ShopService(
            MoneyModel moneyModel,
            IPlayerInventoryService inventoryService,
            IInventorySlotsModel inventorySlotsModel,
            IHotbarSlotsModel hotbarSlotsModel,
            IConfigProvider configProvider,
            ISaveService saveService)
        {
            _moneyModel = moneyModel;
            _inventoryService = inventoryService;
            _inventorySlotsModel = inventorySlotsModel;
            _hotbarSlotsModel = hotbarSlotsModel;
            _farmConfig = configProvider.GetConfig<FarmConfig>();
            _saveService = saveService;
        }

        public override void OnInitialize() =>
            _moneyModel.SetAmount(_saveService.Pending?.Money ?? _farmConfig.StartMoney);

        public int GetItemCount(ItemDefinition item) =>
            _inventorySlotsModel.GetItemCount(item) + _hotbarSlotsModel.GetItemCount(item);

        public bool Buy(ItemDefinition item)
        {
            if (item == null || item.BuyPrice <= 0)
                return false;

            if (!_moneyModel.TrySpend(item.BuyPrice))
            {
                Debug.Log($"[Shop] Not enough money for {item.Name}");
                return false;
            }

            if (_inventoryService.AddItem(item, 1))
                return true;

            _moneyModel.Add(item.BuyPrice); // инвентарь полон — возврат денег
            return false;
        }

        public bool Sell(ItemDefinition item)
        {
            if (item == null || item.SellPrice <= 0 || GetItemCount(item) <= 0)
                return false;

            if (!_inventoryService.RemoveItem(item, 1))
                return false;

            _moneyModel.Add(item.SellPrice);
            return true;
        }
    }
}
