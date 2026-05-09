using System.Threading;
using Cysharp.Threading.Tasks;
using Farmway.Infrastructure;
using UnityEngine;

namespace Farmway.Gameplay.Player
{
    public interface IPlayerFactory
    {
        UniTask<PlayerView> Spawn(Vector2 spawnPosition, CancellationToken ct);
    }

    public class PlayerFactory : IPlayerFactory
    {
        private readonly IConfigProvider _configProvider;
        private readonly IAssetProvider _assetProvider;

        public PlayerFactory(IConfigProvider configProvider, IAssetProvider assetProvider)
        {
            _configProvider = configProvider;
            _assetProvider = assetProvider;
        }

        public async UniTask<PlayerView> Spawn(Vector2 spawnPosition, CancellationToken ct)
        {
            var config = _configProvider.GetConfig<PlayerConfig>();
            var prefab = await _assetProvider.LoadAssetAsync<PlayerView>(config.Prefab, ct);
            
            var playerView = Object.Instantiate(prefab, spawnPosition, Quaternion.identity);
            return playerView;
        }
    }
}