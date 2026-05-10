using Farmway.Infrastructure;

namespace Farmway.Gameplay.Player.Services.Inventory
{
    public class PlayerInventoryService : PlayerService
    {
        private readonly IConfigProvider _configProvider;
        private readonly IInventoryStorage _inventoryStorage;

        private ItemsConfig _itemsConfig;

        public PlayerInventoryService(IConfigProvider configProvider, IInventoryStorage inventoryStorage)
        {
            _configProvider = configProvider;
            _inventoryStorage = inventoryStorage;
        }

        public override void OnInitialize()
        {
            _itemsConfig = _configProvider.GetConfig<ItemsConfig>();
        }

        public bool AddItem(ItemIdEnum itemId, int count) => 
            _inventoryStorage.AddItem(itemId, count);
        
        public bool RemoveItem(ItemIdEnum itemId) =>
            _inventoryStorage.RemoveItem(itemId);
    }
}