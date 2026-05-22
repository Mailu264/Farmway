using System.Collections.Generic;
using Farmway.Gameplay.Services;

namespace Farmway.Gameplay.Player
{
    public interface IPlayerServices
    {
        bool IsEnabled { get; }
        void EnableServices();
        void DisableServices();
        void EnableService<T>() where T : PlayerService;
        void DisableService<T>() where T : PlayerService;
    }

    public class PlayerServices : ServiceOrchestrator<PlayerService>, IPlayerServices
    {
        public PlayerServices(IEnumerable<PlayerService> services) : base(services) { }

        public void EnableService<T>() where T : PlayerService => EnableService(typeof(T));
        public void DisableService<T>() where T : PlayerService => DisableService(typeof(T));
    }
}
