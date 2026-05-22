using Farmway.Gameplay.Player;
using Farmway.Gameplay.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Farmway.Gameplay
{
    public class GameplayScope : LifetimeScope
    {
        [SerializeField] private GameplaySceneView _gameplaySceneView;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_gameplaySceneView);
            RegisterSingletonGameServices(builder);
            RegisterGameServices(builder);
            builder.RegisterEntryPoint<GameplayBootstrapper>();
        }

        private static void RegisterSingletonGameServices(IContainerBuilder builder)
        {
            builder.Register<IPlayerFactory, PlayerFactory>(Lifetime.Scoped);
        }

        private static void RegisterGameServices(IContainerBuilder builder)
        {
            builder.Register<GameTimeService>(Lifetime.Scoped)
                .As<IGameTimeService>()
                .As<GameServiceBase>();

            builder.Register<IGameServices, GameServices>(Lifetime.Scoped).AsImplementedInterfaces();
        }
    }
}
