using Farmway.Gameplay.Player.Services.ItemsService;
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
            builder.Register<PlayerAnimationService>(Lifetime.Scoped).As<PlayerService>();
            builder.Register<PlayerCameraFollowService>(Lifetime.Scoped).As<PlayerService>();
            builder.Register<PlayerInventoryService>(Lifetime.Scoped).As<IPlayerInventoryService>().As<PlayerService>();
            builder.Register<PlayerFarmService>(Lifetime.Scoped).As<PlayerService>();
            builder.Register<PlayerSleepService>(Lifetime.Scoped).AsSelf().As<PlayerService>();
            builder.Register<PlayerShopService>(Lifetime.Scoped).AsSelf().As<PlayerService>();
            builder.Register<ShopService>(Lifetime.Scoped).AsSelf().As<PlayerService>();
            builder.Register<PlayerItemsService>(Lifetime.Scoped).AsSelf().As<PlayerService>();
            builder.Register<PlayerFatigueService>(Lifetime.Scoped).As<PlayerService>();

            builder.Register<MoneyModel>(Lifetime.Scoped);
            builder.Register<FatigueModel>(Lifetime.Scoped);
            builder.Register<GameSaver>(Lifetime.Scoped);
            builder.Register<IInventorySlotsModel, InventorySlotsModel>(Lifetime.Scoped);
            builder.Register<IHotbarSlotsModel, HotbarSlotsModel>(Lifetime.Scoped);
            builder.Register<IItemSlotTransferService, ItemSlotTransferService>(Lifetime.Scoped);

            builder.Register<InventoryPresenter>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<HotbarPresenter>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<PlayerInventoryPresenter>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<HudPresenter>(Lifetime.Scoped).AsImplementedInterfaces();
            builder.Register<ShopPresenter>(Lifetime.Scoped).AsImplementedInterfaces();
        }
    }
}
