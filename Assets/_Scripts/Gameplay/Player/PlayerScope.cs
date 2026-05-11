using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Farmway.Gameplay.Player
{
    public class PlayerScope : LifetimeScope
    {
        [SerializeField] private PlayerView _playerView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_playerView);
            
            RegisterPlayerServices(builder);
            
            builder.RegisterEntryPoint<PlayerBootstrapper>();
        }

        private void RegisterPlayerServices(IContainerBuilder builder)
        {
            builder.Register<IPlayerServices, PlayerServices>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<PlayerMovementService>(Lifetime.Scoped).As<PlayerService>();
            builder.Register<PlayerCameraFollowService>(Lifetime.Scoped).As<PlayerService>();
            builder.Register<PlayerInventoryService>(Lifetime.Scoped).AsSelf().As<PlayerService>();
            
            builder.Register<IInventoryStorage, InventoryStorage>(Lifetime.Scoped);
            builder.Register<IInventorySlotsModel, InventorySlotsModel>(Lifetime.Scoped);
            builder.Register<IHotbarSlotsModel, HotbarSlotsModel>(Lifetime.Scoped);
            builder.Register<IItemSlotTransferService, ItemSlotTransferService>(Lifetime.Scoped);
            
            builder.Register<InventoryPresenter>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<HotbarPresenter>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<PlayerInventoryPresenter>(Lifetime.Scoped).AsImplementedInterfaces();
        }
    }
}
