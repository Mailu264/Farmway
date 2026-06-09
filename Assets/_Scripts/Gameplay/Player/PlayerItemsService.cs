using UniRx;

namespace Farmway.Gameplay.Player
{
    public class PlayerItemsService : PlayerService
    {
        private readonly PlayerView _playerView;
        private readonly IHotbarSlotsModel _hotbarSlotsModel;
        private readonly ItemsConfig _itemsConfig;

        private readonly CompositeDisposable _disposables = new();

        public PlayerItemsService(
            PlayerView playerView,
            IHotbarSlotsModel hotbarSlotsModel,
            ItemsConfig itemsConfig)
        {
            _playerView = playerView;
            _hotbarSlotsModel = hotbarSlotsModel;
            _itemsConfig = itemsConfig;
        }

        public override void OnInitialize()
        {
            _hotbarSlotsModel.CurrentItem
                .Subscribe(OnItemChanged)
                .AddTo(_disposables);
        }

        private void OnItemChanged(InventorySlotData slot)
        {
            if (slot.IsEmpty)
            {
                _playerView.ItemView.SetItem(null);
                return;
            }

            if (!_itemsConfig.TryGetItem(slot.ItemId, out var itemData))
            {
                _playerView.ItemView.SetItem(null);
                return;
            }

            _playerView.ItemView.SetItem(itemData.InHandSprite);
        }

        public override void OnDispose()
        {
            _disposables.Dispose();
        }
    }
}