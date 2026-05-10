using Farmway.Gameplay.Player;
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
            builder.Register<IPlayerFactory, PlayerFactory>(Lifetime.Scoped);
            
            builder.RegisterEntryPoint<GameplayBoostrapper>();
        }
    }
}