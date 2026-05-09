using Farmway.Infrastructure;
using VContainer;

namespace Farmway.Gameplay.Player
{
    public abstract class PlayerService
    {
        protected PlayerView PlayerView;
        protected PlayerConfig PlayerConfig;

        public bool IsEnabled { get; private set; }
        
        [Inject]
        private void Construct(PlayerView playerView, IConfigProvider configProvider)
        {
            PlayerView = playerView;
            PlayerConfig = configProvider.GetConfig<PlayerConfig>();
        }

        public virtual void OnEnable() => IsEnabled = true;
        public virtual void OnDisable() => IsEnabled = false;
        
        public virtual void OnInitialize(){}
        public virtual void OnStart(){}
        public virtual void OnUpdate(){}
        public virtual void OnLateUpdate(){}
        public virtual void OnFixedUpdate(){}
        public virtual void OnDispose(){}
    }
}