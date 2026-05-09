using System.Threading;
using Farmway.Gameplay.Player;
using UnityEngine;
using VContainer.Unity;

namespace Farmway.Gameplay
{
    public class GameplayBoostrapper : IAsyncStartable
    {
        private readonly IPlayerFactory _playerFactory;

        public GameplayBoostrapper(IPlayerFactory playerFactory)
        {
            _playerFactory = playerFactory;
        }

        public async Awaitable StartAsync(CancellationToken cancellation = default)
        {
            await _playerFactory.Spawn(Vector2.zero, cancellation);
        }
    }
}