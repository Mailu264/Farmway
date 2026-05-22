using System.Threading;
using Farmway.Gameplay.Services;
using Farmway.Gameplay.Player;
using UnityEngine;
using VContainer.Unity;

namespace Farmway.Gameplay
{
    public class GameplayBootstrapper : IAsyncStartable
    {
        private readonly IPlayerFactory _playerFactory;
        private readonly IGameServices _gameServices;

        public GameplayBootstrapper(IPlayerFactory playerFactory, IGameServices gameServices)
        {
            _playerFactory = playerFactory;
            _gameServices = gameServices;
        }

        public async Awaitable StartAsync(CancellationToken cancellation = default)
        {
            await _playerFactory.Spawn(Vector2.zero, cancellation);
            _gameServices.EnableServices();
        }
    }
}