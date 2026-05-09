using System.Threading;
using UnityEngine;
using VContainer.Unity;

namespace Farmway.Infrastructure
{
    public class Bootstrapper : IAsyncStartable
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly IAssetProvider _assetProvider;
        private readonly IConfigProvider _configProvider;

        public Bootstrapper(ISceneLoader sceneLoader, IAssetProvider assetProvider, IConfigProvider configProvider)
        {
            _sceneLoader = sceneLoader;
            _assetProvider = assetProvider;
            _configProvider = configProvider;
        }

        public async Awaitable StartAsync(CancellationToken cancellation = default)
        {
            await _configProvider.Initialize(cancellation);
            await _assetProvider.WarmupAsync(cancellation);
            
            var scenesConfig = _configProvider.GetConfig<ScenesConfig>();
            await _sceneLoader.Load(scenesConfig.GameScene, cancellation);
        }
    }
}
