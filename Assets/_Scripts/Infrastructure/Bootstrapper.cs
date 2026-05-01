using System.Threading;
using UnityEngine;
using VContainer.Unity;

namespace Farmway.Infrastructure
{
    public class Bootstrapper : IAsyncStartable
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly IStaticDataProvider _staticDataProvider;

        public Bootstrapper(ISceneLoader sceneLoader, IStaticDataProvider staticDataProvider)
        {
            _sceneLoader = sceneLoader;
            _staticDataProvider = staticDataProvider;
        }

        public async Awaitable StartAsync(CancellationToken cancellation = default)
        {
            await _staticDataProvider.Initialize(cancellation);

            var scenesConfig = _staticDataProvider.GetConfig<ScenesConfig>();
            await _sceneLoader.Load(scenesConfig.GameScene, cancellation);
        }
    }
}
