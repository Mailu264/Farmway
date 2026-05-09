using Farmway.Gameplay.Player;
using VContainer;
using VContainer.Unity;

namespace Farmway.Gameplay
{
    public class GameplayScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<IPlayerFactory, PlayerFactory>(Lifetime.Scoped);
            
            builder.RegisterEntryPoint<GameplayBoostrapper>();
        }
    }
}