using Farmway.Gameplay.Services;
using Farmway.Infrastructure;
using VContainer;

namespace Farmway.Gameplay.Player
{
    public abstract class PlayerService : Service
    {
        protected PlayerView PlayerView;
        protected PlayerConfig PlayerConfig;

        [Inject]
        private void Construct(PlayerView playerView, IConfigProvider configProvider)
        {
            PlayerView = playerView;
            PlayerConfig = configProvider.GetConfig<PlayerConfig>();
        }
    }
}
