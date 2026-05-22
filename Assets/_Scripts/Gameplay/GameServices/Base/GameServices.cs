using System.Collections.Generic;

namespace Farmway.Gameplay.Services
{
    public interface IGameServices
    {
        bool IsEnabled { get; }
        void EnableServices();
        void DisableServices();
        void EnableService<T>() where T : GameServiceBase;
        void DisableService<T>() where T : GameServiceBase;
    }

    public class GameServices : ServiceOrchestrator<GameServiceBase>, IGameServices
    {
        public GameServices(IEnumerable<GameServiceBase> services) : base(services) { }

        public void EnableService<T>() where T : GameServiceBase => EnableService(typeof(T));
        public void DisableService<T>() where T : GameServiceBase => DisableService(typeof(T));
    }
}
