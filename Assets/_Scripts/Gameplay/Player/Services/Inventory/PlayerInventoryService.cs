namespace Farmway.Gameplay.Player
{
    public class PlayerInventoryService : PlayerService
    {
        private readonly IInventorySlotsModel _inventorySlotsModel;

        public PlayerInventoryService(IInventorySlotsModel inventorySlotsModel)
        {
            _inventorySlotsModel = inventorySlotsModel;
        }

        public override void OnInitialize()
        {
            AddItem(ItemIdEnum.Item1, 10);
            AddItem(ItemIdEnum.Item1, 7);
            AddItem(ItemIdEnum.Item1, 4);
        }

        public bool AddItem(ItemIdEnum itemId, int count) => 
            _inventorySlotsModel.AddItem(itemId, count);
        
        public bool RemoveItem(ItemIdEnum itemId) =>
            _inventorySlotsModel.RemoveItem(itemId);

        public bool RemoveItem(ItemIdEnum itemId, int count) =>
            _inventorySlotsModel.RemoveItem(itemId, count);
    }
}
