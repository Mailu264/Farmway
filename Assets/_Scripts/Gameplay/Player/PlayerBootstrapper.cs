using VContainer.Unity;

namespace Farmway.Gameplay.Player
{
    public class PlayerBootstrapper : IInitializable
    {
        private readonly IPlayerServices _playerServices;

        public PlayerBootstrapper(IPlayerServices playerServices)
        {
            _playerServices = playerServices;
        }
        
        public void Initialize()
        {
            _playerServices.EnableServices();
        }
    }
}